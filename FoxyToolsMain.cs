using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommandTerminal;
using UnityModManagerNet;
using HarmonyLib;

namespace FoxyTools
{
    public static class FoxyToolsMain
    {
        public static UnityModManager.ModEntry ModEntry;

        public static bool Load( UnityModManager.ModEntry modEntry )
        {
            ModEntry = modEntry;

            FTCommandAttribute.RegisterAll();

            var harmony = new Harmony("cc.foxden.foxytools");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
        }
    }
}
