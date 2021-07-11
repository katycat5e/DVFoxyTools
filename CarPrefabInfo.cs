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

                        var colliderCategory = ConvertCollidersToJson(colliders);
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
                 colliderList = ConvertCollidersToJson(collidersOnThis);
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

        private static JArray ConvertCollidersToJson( IEnumerable<Collider> colliders )
        {
            var colliderList = new JArray();

            foreach( Collider collider in colliders )
            {
                var props = new JObject();

                if( collider is BoxCollider bc )
                {
                    props.Add(new JProperty("type", "box"));
                    props.Add(new JProperty("center", $"{bc.center.x},{bc.center.y},{bc.center.z}"));
                    props.Add(new JProperty("size", $"{bc.size.x},{bc.size.y},{bc.size.z}"));
                }
                else if( collider is CapsuleCollider cc )
                {
                    props.Add(new JProperty("type", "capsule"));
                    props.Add(new JProperty("center", $"{cc.center.x},{cc.center.y},{cc.center.z}"));
                    props.Add(new JProperty("direction", $"{cc.direction}"));
                    props.Add(new JProperty("height", $"{cc.height}"));
                    props.Add(new JProperty("radius", $"{cc.radius}"));
                }
                else if( collider is SphereCollider sc )
                {
                    props.Add(new JProperty("type", "sphere"));
                    props.Add(new JProperty("center", $"{sc.center.x},{sc.center.y},{sc.center.z}"));
                    props.Add(new JProperty("radius", $"{sc.radius}"));
                }
                else if( collider is MeshCollider mc )
                {
                    props.Add(new JProperty("type", "mesh"));
                    props.Add(new JProperty("mesh", mc.sharedMesh.name));
                }

                colliderList.Add(props);
            }

            return colliderList;
        }
    }
}
