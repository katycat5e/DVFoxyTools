using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandTerminal;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Audio;

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
            var json = ComponentsToJson.GenericObject(foundObjs);
            GameObjectDumper.SendJsonToFile("Resources", desiredType.Name, json);
        }

        [FTCommand(0, 0, "List all unity LayerMask layers")]
        public static void ListLayers( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;

            string[] layerNames = Enumerable.Range(0, 32).Select(n => $"{n} - {LayerMask.LayerToName(n)}").ToArray();
            var json = ComponentsToJson.GenericObject(layerNames);
            GameObjectDumper.SendJsonToFile("Resources", "layers", json);
        }

        [FTCommand(0, 0, "Export audio properties")]
        public static void ExportAudioProps(CommandArg[] args)
        {
            var json = new JObject();

            ComponentsToJson.IgnoreCurves = true;

            var layered = Resources.FindObjectsOfTypeAll<LayeredAudio>();//.Where(la => !la.name.Contains("(Clone)"));
            var processedNames = new HashSet<string>();

            var layerJson = new JArray();
            foreach (var component in layered)
            {
                if (!processedNames.Contains(component.name))
                {
                    layerJson.Add(ComponentsToJson.GenericObject(component, 5));
                    processedNames.Add(component.name);
                }
            }
            json.Add("layered", layerJson);

            processedNames.Clear();

            var clips = Resources.FindObjectsOfTypeAll<AudioClip>();
            var clipJson = new JArray();
            foreach (var component in clips)
            {
                if (!processedNames.Contains(component.name))
                {
                    layerJson.Add(ComponentsToJson.GenericObject(component, 5));
                    processedNames.Add(component.name);
                }
            }
            json.Add("clips", clipJson);

            ComponentsToJson.Reset();

            GameObjectDumper.SendJsonToFile("Resources", "audio", json);
        }

        [FTCommand(0, 0, "Export audio manager")]
        public static void ExportAudioManager(CommandArg[] args)
        {
            ComponentsToJson.IgnoreCurves = true;
            GameObjectDumper.SendJsonToFile("Resources", "audioManager", ComponentsToJson.GenericObject(AudioManager.e, 5));
            ComponentsToJson.Reset();
        }

        [FTCommand(0, 0, "Export audio mixers")]
        public static void ExportAudioMixers(CommandArg[] args)
        {
            ComponentsToJson.IgnoreCurves = true;

            var mixers = Resources.FindObjectsOfTypeAll<AudioMixerGroup>();
            var mixJson = new JArray();
            foreach (var mixer in mixers)
            {
                mixJson.Add(ComponentsToJson.GenericObject(mixer, 5));
            }

            ComponentsToJson.Reset();
            GameObjectDumper.SendJsonToFile("Resources", "audioMixers", mixJson);
        }
    }
}
