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
    class CarPrefabInfo
    {
        [FTCommand(1, 1, "Print the structure of the traincar prefab with given name")]
        public static void DumpCarPrefab( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;

            string name = args[0].String;

            if( Enum.TryParse(name, out TrainCarType carType) )
            {
                GameObject prefab = CarTypes.GetCarPrefab(carType);
                if( !prefab )
                {
                    Debug.LogError($"CarType {name} has missing prefab");
                    return;
                }

                JToken contents = GameObjectDumper.DumpObject(prefab);
                GameObjectDumper.SendJsonToFile(name, "prefab", contents);
            }
            else
            {
                Debug.LogWarning("Invalid car type " + name);
            }
        }

        [FTCommand(1, 1, "Print the interior structure of the traincar prefab with given name")]
        public static void DumpCarInterior( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;

            string name = args[0].String;

            if( Enum.TryParse(name, out TrainCarType carType) )
            {
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
            else
            {
                Debug.LogWarning("Invalid car type " + name);
            }
        }

        private static readonly string[] colliderCategories =
        {
            "[collision]", "[walkable]", "[items]", "[camera dampening]", "[bogies]"
        };

        [FTCommand(1, 1, "Print the colliders of the traincar prefab with given name")]
        public static void ExportCarColliders( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;

            string name = args[0].String;

            if( Enum.TryParse(name, out TrainCarType carType) )
            {
                GameObject prefab = CarTypes.GetCarPrefab(carType);
                if( !prefab )
                {
                    Debug.LogError($"CarType {name} has missing prefab");
                    return;
                }

                Transform colliderRoot = prefab.transform.Find("[colliders]");
                if( !colliderRoot )
                {
                    Debug.LogWarning($"CarType {name} does not have a colliders root transform");
                    return;
                }

                var colliderDict = new JObject();
                
                foreach( string categoryName in colliderCategories )
                {
                    Transform subTransform = colliderRoot.Find(categoryName);
                    if( subTransform )
                    {
                        Collider[] colliders = subTransform.gameObject.GetComponentsInChildren<Collider>();

                        var colliderCategory = ComponentsToJson.Colliders(colliders);
                        colliderDict.Add(new JProperty(categoryName, colliderCategory));
                    }
                }

                GameObjectDumper.SendJsonToFile(name, "colliders", colliderDict);
            }
            else
            {
                Debug.LogWarning("Invalid car type " + name);
            }
        }

        [FTCommand(1, 1, "Print the interior colliders of the traincar prefab with given name")]
        public static void ExportInteriorColliders( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;

            string name = args[0].String;

            if( Enum.TryParse(name, out TrainCarType carType) )
            {
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
                    new JProperty("name", currentLevel.name)
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

        [FTCommand("ExportLocoCurves", 1, 1, "Print the physics curves of the loco with given name")]
        public static void ExportLocoControllerCurves( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;

            string name = args[0].String;

            if( Enum.TryParse(name, out TrainCarType carType) )
            {
                GameObject prefab = CarTypes.GetCarPrefab(carType);
                if( !prefab )
                {
                    Debug.LogError($"CarType {name} has missing prefab");
                    return;
                }

                LocoControllerBase locoController = prefab.GetComponent<LocoControllerBase>();
                if( !locoController )
                {
                    Debug.LogWarning($"CarType {name} prefab does not have a loco controller");
                    return;
                }

                var props = new JObject();

                // brake & traction
                JObject brakeCurve = ComponentsToJson.AnimationCurve(locoController.brakePowerCurve);
                props.Add("brakePowerCurve", brakeCurve);

                if( locoController is LocoControllerDiesel lcd )
                {
                    var tractionCurve = ComponentsToJson.AnimationCurve(lcd.tractionTorqueCurve);
                    props.Add("tractionTorqueCurve", tractionCurve);
                }
                else if( locoController is LocoControllerSteam lcs )
                {
                    var tractionCurve = ComponentsToJson.AnimationCurve(lcs.tractionTorqueCurve);
                    props.Add("tractionTorqueCurve", tractionCurve);
                }
                else if( locoController is LocoControllerShunter lcShunt )
                {
                    var tractionCurve = ComponentsToJson.AnimationCurve(lcShunt.tractionTorqueCurve);
                    props.Add("tractionTorqueCurve", tractionCurve);
                }

                // driving force
                props.Add("drivingForce", ComponentsToJson.DrivingForce(locoController.drivingForce));

                GameObjectDumper.SendJsonToFile(name, "loco_curves", props);
            }
        }

        [FTCommand(1, 1, "Print the damage controller properties of the loco with given name")]
        public static void ExportDamageProperties( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;

            string name = args[0].String;

            if( Enum.TryParse(name, out TrainCarType carType) )
            {
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
        }

        [FTCommand(1, 1, "Print the cab control specs of the loco with given name")]
        public static void ExportCabControls( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;

            string name = args[0].String;

            if( Enum.TryParse(name, out TrainCarType carType) )
            {
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
        }
    }
}
