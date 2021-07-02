using System;
using System.Collections.Generic;
using System.Linq;
using CommandTerminal;
using UnityEngine;

namespace FoxyTools
{
    static class ResourceUtil
    {
        public static void RegisterCommands()
        {
            Terminal.Shell.AddCommand("FT.SpawnPrefab", SpawnPrefab, 1, 1, "Instantiate the resource with given name");
            Terminal.Autocomplete.Register("FT.SpawnPrefab");

            Terminal.Shell.AddCommand("FT.CopyObject", CopyObject, 1, 1, "Copy the game object with the given name");
            Terminal.Autocomplete.Register("FT.CopyObject");

            Terminal.Shell.AddCommand("FT.DumpObject", DumpObjStructure, 1, 1, "Print the structure of the object with given name");
            Terminal.Autocomplete.Register("FT.DumpObject");

            Terminal.Shell.AddCommand("FT.DumpCarPrefab", DumpCarPrefab, 1, 1, "Print the structure of the traincar prefab with given name");
            Terminal.Autocomplete.Register("FT.DumpCarPrefab");

            Terminal.Shell.AddCommand("FT.FindResources", FindResourcesOfType, 1, 1, "Find all resources of the given type");
            Terminal.Autocomplete.Register("FT.FindResources");
        }

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

            string contents = GameObjectDumper.DumpObject(gameObj);
            Debug.Log(contents);
        }

        public static void DumpCarPrefab( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;

            string name = args[0].String;

            if( Enum.TryParse(name, out TrainCarType carType) )
            {
                GameObject prefab = CarTypes.GetCarPrefab(carType);
                string contents = GameObjectDumper.DumpObject(prefab);
                Debug.Log(contents);
            }
            else
            {
                Debug.LogWarning("Invalid car type " + name);
            }
        }

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
    }
}
