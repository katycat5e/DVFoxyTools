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

            Terminal.Shell.AddCommand("FT.ExportColliders", ExportCarColliders, 1, 1, "Print the colliders of the traincar prefab with given name");
            Terminal.Autocomplete.Register("FT.ExportColliders");
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

                string contents = GameObjectDumper.DumpObject(prefab);
                Debug.Log(contents);
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

                        var colliderCategory = new JArray();

                        if( colliders.Length > 0 )
                        {
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

                                colliderCategory.Add(props);
                            }
                        }

                        colliderDict.Add(new JProperty(categoryName, colliderCategory));
                    }
                }

                string outPath = Path.Combine(FoxyToolsMain.ModEntry.Path, $"colliders_{name}.json");

                try
                {
                    using( var sw = File.CreateText(outPath) )
                    {
                        using( var jtw = new JsonTextWriter(sw) )
                        {
                            jtw.Formatting = Formatting.Indented;
                            colliderDict.WriteTo(jtw);
                        }
                    }
                }
                catch( Exception ex )
                {
                    Debug.LogError("Couldn't open output file:\n" + ex.Message);
                }
            }
            else
            {
                Debug.LogWarning("Invalid car type " + name);
            }
        }
    }
}
