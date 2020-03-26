using System;
using System.Text;
using Newtonsoft.Json;

namespace IntelligentTagging.Model
{
    public class Confidence
    {
        [JsonProperty("statisticalfeature")]
        public string StatisticalFeature { get; set; }
        [JsonProperty("dblookup")]
        public string DbLookup { get; set; }
        [JsonProperty("resolution")]
        public string Resolution { get; set; }
        [JsonProperty("aggregate")]
        public string Aggregate { get; set; }
        public override string ToString()
        {
            var strBuilder = new StringBuilder();
            strBuilder.Append($"\tstatisticalfeature:{StatisticalFeature}\n");
            strBuilder.Append($"\tdblooku:{DbLookup}\n");
            strBuilder.Append($"\tresolutione:{Resolution}\n");
            strBuilder.Append($"\taggregate:{Aggregate}\n");

            return strBuilder.ToString();
        }
    }
}