using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandTerminal;
using DV.Logic.Job;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace FoxyTools
{
    static class StationInfoDump
    {
        [FTCommand(Help = "Dump info about station configs")]
        public static void GetStationInfo(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            var stations = GameObject.FindObjectsOfType<StationController>();
            var stationList = new JArray();

            foreach (var station in stations)
            {
                var stationJson = new JObject()
                {
                    { "name", station.stationInfo.Name },
                    { "id", station.stationInfo.YardID }
                };

                // job rules
                var ruleSet = station.proceduralJobsRuleset;
                var inputGroups = new JArray(ruleSet.inputCargoGroups.Select(CargoGroupJson));
                var outputGroups = new JArray(ruleSet.outputCargoGroups.Select(CargoGroupJson));

                stationJson.Add("inputs", inputGroups);
                stationJson.Add("outputs", outputGroups);

                var spawners = station.GetComponentsInChildren<StationLocoSpawner>();
                stationJson.Add("spawners", new JArray(spawners.Select(LocoSpawnerJson)));

                stationList.Add(stationJson);
            }

            GameObjectDumper.SendJsonToFile("Resources", "stations", stationList);
        }

        private static JObject CargoGroupJson(CargoGroup group)
        {
            return new JObject()
            {
                { "cargoTypes", new JArray(group.cargoTypes.Select(CargoTypes.GetCargoName)) },
                { "stations", new JArray(group.stations.Select(s => s.stationInfo.YardID)) }
            };
        }

        private static JObject LocoSpawnerJson(StationLocoSpawner spawner)
        {
            return new JObject()
            {
                { "locoSpawnTrackName", spawner.locoSpawnTrackName },
                { "spawnRotationFlipped", spawner.spawnRotationFlipped },
                { "locoTypeGroupsToSpawn", new JArray(spawner.locoTypeGroupsToSpawn.Select(ListTrainCarTypeWrapperJson)) }
            };
        }

        private static JArray ListTrainCarTypeWrapperJson(ListTrainCarTypeWrapper list)
        {
            var names = list.trainCarTypes.Select(t => t.DisplayName());
            return new JArray(names);
        }
    }
}
