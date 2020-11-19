using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace FoxyTools
{
    public static class FoxyToolsMain
    {
        public static UnityModManager.ModEntry ModEntry;
        //internal static FTModSettings Settings;

        public static bool Load( UnityModManager.ModEntry modEntry )
        {
            ModEntry = modEntry;

            //// Initialize settings
            //Settings = UnityModManager.ModSettings.Load<FTModSettings>(ModEntry);
            //ModEntry.OnGUI = Settings.Draw;
            //ModEntry.OnSaveGUI = Settings.Save;

            ResourceUtil.RegisterCommands();
            PlayerInfo.RegisterCommands();
            PositionTester.RegisterCommands();

            return true;
        }
    }
}
