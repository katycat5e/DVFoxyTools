using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandTerminal;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using DV.CabControls.Spec;

namespace FoxyTools
{
    public static class CarPrefabInfo
    {
        private static bool TryGetSelectedCar(CommandArg[] args, out TrainCarType carType, out string name)
        {
            if (args.Length == 0)
            {
                var car = PlayerManager.Car;
                if (car)
                {
                    carType = car.carType;
                    name = carType.DisplayName();
                    return true;
                }
            }
            else
            {
                name = args[0].String;

                if (Enum.TryParse(name, out carType))
                {
                    return true;
                }

                Debug.LogWarning($"Couldn't find car type \"{name}\"");
            }

            carType = TrainCarType.NotSet;
            name = null;
            return false;
        }

        [FTCommand(0, 1, "Print the structure of the traincar prefab with given name")]
        public static void DumpCarPrefab( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;

            if (!TryGetSelectedCar(args, out TrainCarType carType, out string name)) return;

            GameObject prefab = CarTypes.GetCarPrefab(carType);
            if( !prefab )
            {
                Debug.LogError($"CarType {name} has missing prefab");
                return;
            }

            JToken contents = GameObjectDumper.DumpObject(prefab);
            GameObjectDumper.SendJsonToFile(name, "prefab", contents);
        }

        [FTCommand(0, 1, "Print the interior structure of the traincar prefab with given name")]
        public static void DumpCarInterior( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;

            if (!TryGetSelectedCar(args, out TrainCarType carType, out string name)) return;

            GameObject prefab = CarTypes.GetCarPrefab(carType);
            if( !prefab )
            {
                Debug.LogError($"CarType {name} has missing prefab");
                return;
            }

            TrainCar car = prefab.GetComponent<TrainCar>();
            if( !car )
            {
                Debug.LogError($"Couldn't find TrainCar on carType {name}");
                return;
            }

            if( !car.interiorPrefab )
            {
                Debug.LogWarning($"TrainCar on carType {name} doesn't have an interiorPrefab assigned");
                return;
            }

            JToken contents = GameObjectDumper.DumpObject(car.interiorPrefab);
            GameObjectDumper.SendJsonToFile(name, "interior", contents);
        }

        [FTCommand(0, 1, "Print the colliders of the traincar prefab with given name")]
        public static void ExportCarColliders( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;

            if (!TryGetSelectedCar(args, out TrainCarType carType, out string name)) return;

            GameObject prefab = CarTypes.GetCarPrefab(carType);
            if( !prefab )
            {
                Debug.LogError($"CarType {name} has missing prefab");
                return;
            }

            var colliderJson = GetCollidersRecursive(prefab.transform);
            GameObjectDumper.SendJsonToFile(name, "colliders", colliderJson);
        }

        [FTCommand(0, 1, "Print the interior colliders of the traincar prefab with given name")]
        public static void ExportInteriorColliders( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;

            if (!TryGetSelectedCar(args, out TrainCarType carType, out string name)) return;

            GameObject prefab = CarTypes.GetCarPrefab(carType);
            if( !prefab )
            {
                Debug.LogError($"CarType {name} has missing prefab");
                return;
            }

            TrainCar car = prefab.GetComponent<TrainCar>();
            if( !car )
            {
                Debug.LogError($"Couldn't find TrainCar on carType {name}");
                return;
            }

            if( !car.interiorPrefab )
            {
                Debug.LogWarning($"TrainCar on carType {name} doesn't have an interiorPrefab assigned");
                return;
            }

            var colliderJson = GetCollidersRecursive(car.interiorPrefab.transform);
            GameObjectDumper.SendJsonToFile(name, "interior_colliders", colliderJson);
        }

        private static JObject GetCollidersRecursive( Transform currentLevel )
        {
            var collidersOnThis = currentLevel.gameObject.GetComponents<Collider>();

            JArray colliderList = null;
            if( collidersOnThis.Length > 0 )
            {
                 colliderList = ComponentsToJson.Colliders(collidersOnThis);
            }

            JArray children = null;
            if( currentLevel.childCount > 0 )
            {
                children = new JArray();

                foreach( Transform child in currentLevel )
                {
                    JObject childJson = GetCollidersRecursive(child);
                    if( childJson != null )
                    {
                        children.Add(childJson);
                    }
                }
            }

            if( (colliderList != null) || ((children != null) && (children.Count > 0)) )
            {
                JObject result = new JObject()
                {
                    { "name", currentLevel.name },
                    { "layer", LayerMask.LayerToName(currentLevel.gameObject.layer) }
                };

                if( colliderList != null )
                {
                    result.Add(new JProperty("colliders", colliderList));
                }

                if( (children != null) && (children.Count > 0) )
                {
                    result.Add(new JProperty("children", children));
                }

                return result;
            }
            else return null;
        }

        [FTCommand("ExportLocoCurves", 0, 1, "Print the physics curves of the loco with given name")]
        public static void ExportLocoControllerCurves(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            if (!TryGetSelectedCar(args, out TrainCarType carType, out string name)) return;

            GameObject prefab = CarTypes.GetCarPrefab(carType);
            if (!prefab)
            {
                Debug.LogError($"CarType {name} has missing prefab");
                return;
            }

            LocoControllerBase locoController = prefab.GetComponent<LocoControllerBase>();
            if (!locoController)
            {
                Debug.LogWarning($"CarType {name} prefab does not have a loco controller");
                return;
            }

            var props = new JObject();

            // brake & traction
            JObject brakeCurve = ComponentsToJson.AnimationCurve(locoController.brakePowerCurve);
            props.Add("brakePowerCurve", brakeCurve);

            if (locoController is LocoControllerDiesel lcd)
            {
                var tractionCurve = ComponentsToJson.AnimationCurve(lcd.tractionTorqueCurve);
                props.Add("tractionTorqueCurve", tractionCurve);
            }
            else if (locoController is LocoControllerSteam lcs)
            {
                var tractionCurve = ComponentsToJson.AnimationCurve(lcs.tractionTorqueCurve);
                props.Add("tractionTorqueCurve", tractionCurve);
            }
            else if (locoController is LocoControllerShunter lcShunt)
            {
                var tractionCurve = ComponentsToJson.AnimationCurve(lcShunt.tractionTorqueCurve);
                props.Add("tractionTorqueCurve", tractionCurve);
            }

            // driving force
            props.Add("drivingForce", ComponentsToJson.DrivingForce(locoController.drivingForce));

            GameObjectDumper.SendJsonToFile(name, "loco_curves", props);
        }

        [FTCommand(0, 1, "Print the top level script properties of the car with given name")]
        public static void ExportTrainProperties(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            if (!TryGetSelectedCar(args, out TrainCarType carType, out string name)) return;

            GameObject prefab = CarTypes.GetCarPrefab(carType);
            if (!prefab)
            {
                Debug.LogError($"CarType {name} has missing prefab");
                return;
            }

            var json = ComponentsToJson.GenericObject(prefab.GetComponents<MonoBehaviour>(), 10);
            GameObjectDumper.SendJsonToFile(name, "car_scripts", json);
        }

        [FTCommand(0, 1, "Print the damage controller properties of the loco with given name")]
        public static void ExportDamageProperties( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;

            if (!TryGetSelectedCar(args, out TrainCarType carType, out string name)) return;

            GameObject prefab = CarTypes.GetCarPrefab(carType);
            if( !prefab )
            {
                Debug.LogError($"CarType {name} has missing prefab");
                return;
            }

            var damage = prefab.GetComponent<DamageController>();
            if( !damage )
            {
                Debug.LogWarning($"CarType {name} prefab does not have a damage controller");
                return;
            }

            var ctrlProps = new JObject
            {
                { "wheelsHP", damage.wheels.fullHitPoints },
                { "speedToBrakeDamageCurve", ComponentsToJson.AnimationCurve(damage.speedToBrakeDamageCurve) },
            };

            if( damage is DamageControllerDiesel dcd )
            {
                ctrlProps.Add("engineHP", dcd.engine.fullHitPoints);
            }
            else if( damage is DamageControllerShunter dcs )
            {
                ctrlProps.Add("engineHP", dcs.engine.fullHitPoints);
            }

            if( TrainCarAndCargoDamageProperties.carDamageProperties.TryGetValue(carType, out CarDamageProperties dmgProps) )
            {
                ctrlProps.Add("bodyDamage", ComponentsToJson.CarDamageProperties(dmgProps));
            }

            GameObjectDumper.SendJsonToFile(name, "damage", ctrlProps);
        }

        [FTCommand(0, 1, "Print the cab control specs of the loco with given name")]
        public static void ExportCabControls( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;

            if (!TryGetSelectedCar(args, out TrainCarType carType, out string name)) return;

            GameObject prefab = CarTypes.GetCarPrefab(carType);
            if( !prefab )
            {
                Debug.LogError($"CarType {name} has missing prefab");
                return;
            }

            TrainCar car = prefab.GetComponent<TrainCar>();
            if( !car )
            {
                Debug.LogError($"Couldn't find TrainCar on carType {name}");
                return;
            }

            if( !car.interiorPrefab )
            {
                Debug.LogWarning($"TrainCar on carType {name} doesn't have an interiorPrefab assigned");
                return;
            }

            var specList = new JArray();
            var controlSpecs = car.interiorPrefab.GetComponentsInChildren<ControlSpec>();
            foreach( ControlSpec spec in controlSpecs )
            {
                specList.Add(ComponentsToJson.GenericObject(spec));
            }

            var indicators = car.interiorPrefab.GetComponentsInChildren<Indicator>();
            foreach( Indicator ind in indicators )
            {
                specList.Add(ComponentsToJson.GenericObject(ind));
            }

            GameObjectDumper.SendJsonToFile(name, "control_spec", specList);
        }

        [FTCommand(0, 1, "Print the particle systems of the selected car")]
        public static void ExportCarParticles(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            if (!TryGetSelectedCar(args, out TrainCarType carType, out string name)) return;

            GameObject prefab = CarTypes.GetCarPrefab(carType);
            if (!prefab)
            {
                Debug.LogError($"CarType {name} has missing prefab");
                return;
            }

            var systems = prefab.GetComponentsInChildren<ParticleSystem>();
            var json = ComponentsToJson.GenericObject(systems);

            GameObjectDumper.SendJsonToFile(name, "particles", json);
        }
    }
}
