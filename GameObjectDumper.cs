using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;

namespace FoxyTools
{
    static class GameObjectDumper
    {
        public const string EXPORT_DIR = "FT_Export";

        public static JToken DumpObject( GameObject obj )
        {
            if( obj == null ) return "";

            DumpObjRecursive(obj, out JToken json);
            return json;
        }

        private static void DumpObjRecursive( GameObject obj, out JToken root )
        {
            var nonTransformComps = obj.GetComponents<Component>().Where(comp => !(comp is Transform));
            var componentList = new JArray();
            foreach( Component c in nonTransformComps )
            {
                if( c )
                {
                    componentList.Add(ComponentsToJson.GenericObject(c));
                }
            }

            var children = new JArray();
            if( obj.transform.childCount > 0 )
            {
                foreach( Transform t in obj.transform )
                {
                    DumpObjRecursive(t.gameObject, out JToken childProps);
                    children.Add(childProps);
                }
            }

            if( (componentList.Count > 0) || (children.Count > 0) )
            {
                var rootObj = new JObject()
                {
                    { "name", obj.name },
                    { "tag", obj.tag },
                    { "layer", obj.layer }
                };

                if( componentList.Count > 0 )
                {
                    rootObj.Add(new JProperty("components", componentList));
                }

                if( children.Count > 0 )
                {
                    rootObj.Add(new JProperty("children", children));
                }

                root = rootObj;
            }
            else
            {
                root = new JValue(obj.name);
            }
        }

        public static void SendJsonToFile(string objName, string subType, JToken json)
        {
            string outDir = Path.Combine(FoxyToolsMain.Instance.Path, EXPORT_DIR, objName);

            try
            {
                Directory.CreateDirectory(outDir);
                string outPath = Path.Combine(outDir, $"{objName}_{subType}.json");
                ExportJson(outPath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError("Couldn't open output file:\n" + ex.Message);
                return;
            }
        }

        private static void ExportJson(string outPath, JToken json)
        {
            try
            {
                using( var outFile = File.CreateText(outPath) )
                {
                    using( var jtw = new JsonTextWriter(outFile) )
                    {
                        jtw.Formatting = Formatting.Indented;
                        json.WriteTo(jtw);
                    }
                }
            }
            catch( Exception ex )
            {
                Debug.LogError("Couldn't open output file:\n" + ex.Message);
                return;
            }

            Debug.Log($"Wrote object structure to {outPath}");
        }
    }
}
