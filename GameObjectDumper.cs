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
        //public static string DumpObject( GameObject obj )
        //{
        //    if( obj == null ) return "";

        //    var sb = new StringBuilder();
        //    DumpObjRecursive(obj, sb, "");
        //    return sb.ToString();
        //}

        //private static void DumpObjRecursive( GameObject obj, StringBuilder sb, string indent )
        //{
        //    sb.AppendFormat("\n{0}[Object: {1}]", indent, obj.name);

        //    string subIndent = indent + '\t';
        //    foreach( Transform t in obj.transform )
        //    {
        //        DumpObjRecursive(t.gameObject, sb, subIndent);
        //    }

        //    foreach( Component c in obj.GetComponents<Component>() )
        //    {
        //        if( c ) sb.AppendFormat("\n{0}[Component: {1}]", subIndent, c.GetType().Name);
        //    }
        //}

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
                    componentList.Add(c.GetType().Name);
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
                    new JProperty("name", obj.name)
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

        public static void SendJsonToFile( string objName, string objType, JToken json )
        {
            string outPath = Path.Combine(FoxyToolsMain.ModEntry.Path, $"{objName}_{objType}.json");

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
