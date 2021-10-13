using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandTerminal;
using UnityEngine;

namespace FoxyTools
{
    static class ResourceUtil
    {
        [FTCommand(1, 1, "Instantiate the resource with given name")]
        public static void SpawnPrefab( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;

            string name = args[0].String;

            var prefab = Resources.Load(name);
            if( prefab == null )
            {
                Debug.LogWarning("Couldn't find resource named " + name);
                return;
            }

            var transform = PlayerManager.PlayerTransform;
            Vector3 location = transform.position;
            Quaternion rotation = transform.rotation;

            var obj = UnityEngine.Object.Instantiate(prefab, location, rotation);

            if( obj != null )
            {
                Debug.Log($"Created object from prefab {name}");
            }
            else
            {
                Debug.LogWarning($"Failed to spawn object from prefab {name}");
            }
        }

        [FTCommand(1, 1, "Copy the game object with the given name")]
        public static void CopyObject( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;

            string name = args[0].String;

            var gameObj = GameObject.Find(name);
            if( gameObj == null )
            {
                Debug.LogWarning("Couldn't find object " + name);
                return;
            }

            var transform = PlayerManager.PlayerTransform;
            Vector3 location = transform.position;
            Quaternion rotation = transform.rotation;

            var obj = UnityEngine.Object.Instantiate(gameObj, location, rotation);

            if( obj != null )
            {
                Debug.Log($"Copied object \"{name}\"");
            }
            else
            {
                Debug.LogWarning($"Failed to copy object \"{name}\"");
            }
        }

        [FTCommand("DumpObject", 1, 1, "Print the structure of the object with given name")]
        public static void DumpObjStructure( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;

            string name = args[0].String;

            var gameObj = GameObject.Find(name);
            if( gameObj == null )
            {
                Debug.LogWarning("Couldn't find object " + name);
                return;
            }

            var contents = GameObjectDumper.DumpObject(gameObj);
            GameObjectDumper.SendJsonToFile(name, "object", contents);
        }

        [FTCommand("FindResources", 1, 1, "Find all resources of the given type")]
        public static void FindResourcesOfType( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;

            string typeName = args[0].String;
            Type desiredType = null;
            foreach( var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                desiredType = assembly.GetType(typeName, false, true);
                if( desiredType != null ) break;
            }

            if( desiredType == null )
            {
                Debug.LogWarning($"Specified type \"{typeName}\" not found");
                return;
            }

            var foundObjs = Resources.FindObjectsOfTypeAll(desiredType);
            string outString = string.Join("\n", foundObjs.Select(o => o.name));
            Debug.Log(outString);
        }

        [FTCommand(0, 0, "List all unity LayerMask layers")]
        public static void ListLayers( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;

            for( int i = 0; i < 32; i++ )
            {
                string layerName = LayerMask.LayerToName(i);
                Debug.Log($"Layer {i}: {layerName}");
            }
        }
    }
}
