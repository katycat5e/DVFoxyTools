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

namespace FoxyTools
{
    class CarPrefabInfo
    {
        public static void RegisterCommands()
        {
            Terminal.Shell.AddCommand("FT.DumpCarPrefab", DumpCarPrefab, 1, 1, "Print the structure of the traincar prefab with given name");
            Terminal.Autocomplete.Register("FT.DumpCarPrefab");
            
            Terminal.Shell.AddCommand("FT.DumpCarInterior", DumpCarInterior, 1, 1, "Print the interior structure of the traincar prefab with given name");
            Terminal.Autocomplete.Register("FT.DumpCarInterior");

            Terminal.Shell.AddCommand("FT.ExportColliders", ExportCarColliders, 1, 1, "Print the colliders of the traincar prefab with given name");
            Terminal.Autocomplete.Register("FT.ExportColliders");

            Terminal.Shell.AddCommand("FT.ExportInteriorColliders", ExportInteriorColliders, 1, 1, "Print the interior colliders of the traincar prefab with given name");
            Terminal.Autocomplete.Register("FT.ExportInteriorColliders");

            Terminal.Shell.AddCommand("FT.ExportLocoCurves", ExportLocoControllerCurves, 1, 1, "Print the physics curves of the loco with given name");
            Terminal.Autocomplete.Register("FT.ExportLocoCurves");
        }

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

                JObject brakeCurve = ComponentsToJson.AnimationCurve(locoController.brakePowerCurve);
                brakeCurve.Add("name", "brakePowerCurve");

                JArray curves = new JArray() { brakeCurve };

                if( locoController is LocoControllerDiesel lcd )
                {
                    var tractionCurve = ComponentsToJson.AnimationCurve(lcd.tractionTorqueCurve);
                    tractionCurve.Add("name", "tractionTorqueCurve");
                    curves.Add(tractionCurve);
                }
                else if( locoController is LocoControllerSteam lcs )
                {
                    var tractionCurve = ComponentsToJson.AnimationCurve(lcs.tractionTorqueCurve);
                    tractionCurve.Add("name", "tractionTorqueCurve");
                    curves.Add(tractionCurve);
                }
                else if( locoController is LocoControllerShunter lcShunt )
                {
                    var tractionCurve = ComponentsToJson.AnimationCurve(lcShunt.tractionTorqueCurve);
                    tractionCurve.Add("name", "tractionTorqueCurve");
                    curves.Add(tractionCurve);
                }

                GameObjectDumper.SendJsonToFile(name, "loco_curves", curves);
            }
        }
    }
}
