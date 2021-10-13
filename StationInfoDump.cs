using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandTerminal;
using DV.Logic.Job;
using UnityEngine;

namespace FoxyTools
{
    static class StationInfoDump
    {
        [FTCommand(Help = "Dump info about station configs")]
        public static void GetStationInfo( CommandArg[] args )
        {
            string outPath = Path.Combine(FoxyToolsMain.ModEntry.Path, "station.txt");

            try
            {
                using( var fs = new FileStream(outPath, FileMode.Create) )
                {
                    using( var sw = new StreamWriter(fs) )
                    {
                        var stations = GameObject.FindObjectsOfType<StationController>();
                        foreach( StationController station in stations )
                        {
                            StationInfo info = station.stationInfo;
                            sw.WriteLine($"[{info.Name} ({info.YardID})]");

                            // job rules
                            var ruleSet = station.proceduralJobsRuleset;
                            sw.WriteLine("\tInputs:");
                            foreach( CargoType input in ruleSet.inputCargoGroups.SelectMany(group => group.cargoTypes) )
                            {
                                sw.WriteLine($"\t\t{input.GetCargoName()}");
                            }

                            sw.WriteLine("\n\tOutputs:");
                            foreach( CargoType output in ruleSet.outputCargoGroups.SelectMany(group => group.cargoTypes) )
                            {
                                sw.WriteLine($"\t\t{output.GetCargoName()}");
                            }

                            sw.WriteLine();
                        }
                    }
                }
            }
            catch( Exception ex )
            {
                Debug.LogError("Couldn't open output file:\n" + ex.Message);
            }
        }
    }
}
