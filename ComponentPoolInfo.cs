using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandTerminal;
using Newtonsoft.Json.Linq;

namespace FoxyTools
{
    public class ComponentPoolInfo
    {
        [FTCommand(Help = "Print the contents of the TrainComponentPool")]
        public static void DumpComponentPools( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;

            JToken audioPool = ComponentsToJson.AudioPoolReferences(TrainComponentPool.Instance.audioPoolReferences);
            //JToken cargoPool = ComponentsToJson.GenericObject(TrainComponentPool.Instance.cargoPoolReferences.poolData);

            var output = new JObject()
            {
                new JProperty("audioPool", audioPool),
                //new JProperty("cargoPool", cargoPool)
            };

            GameObjectDumper.SendJsonToFile("TrainComponentPool", "members", output);
        }
    }
}
