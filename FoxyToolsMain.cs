using System;
using System.Collections.Generic;
using System.Linq;
using CommandTerminal;
using UnityModManagerNet;

namespace FoxyTools
{
    public static class FoxyToolsMain
    {
        public static UnityModManager.ModEntry ModEntry;

        public static bool Load( UnityModManager.ModEntry modEntry )
        {
            ModEntry = modEntry;

            ResourceUtil.RegisterCommands();
            PlayerInfo.RegisterCommands();
            PositionTester.RegisterCommands();
            StationInfoDump.RegisterCommands();

            return true;
        }
    }
}
