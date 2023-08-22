using System.Linq;
using CommandTerminal;
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

            var stations = Object.FindObjectsOfType<StationController>();
            var stationList = new JArray();

            foreach (var station in stations)
            {
                var builder = new JObjectBuilder<StationController>(station)
                    .WithDataClass(s => s.stationInfo)
                    .With(s => s.proceduralJobsRuleset, RuleSetJson)
                    .WithEach(s => s.storageRailtracksGONames)
                    .WithEach(s => s.transferInRailtracksGONames)
                    .WithEach(s => s.transferOutRailtracksGONames)
                    .WithEach(s => s.warehouseMachineControllers, w => w.warehouseTrackName);

                var stationJson = builder.Result;

                var spawners = station.GetComponentsInChildren<StationLocoSpawner>();
                stationJson.Add("spawners", new JArray(spawners.Select(LocoSpawnerJson)));

                stationList.Add(stationJson);
            }

            GameObjectDumper.SendJsonToFile("Resources", "stations", stationList);
        }

        private static JObject RuleSetJson(StationProceduralJobsRuleset ruleset)
        {
            return new JObjectBuilder<StationProceduralJobsRuleset>(ruleset)
                .WithEach(r => r.inputCargoGroups, CargoGroupJson)
                .WithEach(r => r.outputCargoGroups, CargoGroupJson)
                .Result;
        }

        private static JObject CargoGroupJson(CargoGroup group)
        {
            return new JObject()
            {
                { "cargoTypes", new JArray(group.cargoTypes.Select(c => c.ToString())) },
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
            var names = list.liveries.Select(t => t.localizationKey.Local());
            return new JArray(names);
        }
    }
}
