using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IntelligentTagging.Model
{
    public interface IMetaData
    {
        [JsonProperty("_typeGroup")]
        string TypeGroup { get; set; }

        [JsonProperty("_type")]
        string Type { get; set; }


        [JsonProperty("name")]
        string Name { get; set; }

        [JsonProperty("forenduserdisplay")]
        string ForEndUserDisplay { get; set; }


        [Newtonsoft.Json.JsonExtensionData]
        IDictionary<string, JToken> _Attribute { get; set; }
    }
}