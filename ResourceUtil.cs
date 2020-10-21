using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        }

        public static void SpawnPrefab( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;

            string name = args[0].String;

            var prefab = Resources.Load(name);
            if( prefab == null ) return;

            var transform = PlayerManager.PlayerTransform;
            Vector3 location = transform.position;
            Quaternion rotation = transform.rotation;

            var obj = UnityEngine.Object.Instantiate(prefab, location, rotation);

            if( obj != null )
            {
                FoxyToolsMain.ModEntry.Logger.Log($"Created object from prefab {name}");
            }
            else
            {
                FoxyToolsMain.ModEntry.Logger.Warning($"Failed to spawn object from prefab {name}");
            }
        }
    }
}
