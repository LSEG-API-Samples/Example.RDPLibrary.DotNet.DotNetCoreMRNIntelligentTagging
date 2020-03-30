using System.Collections.Generic;
using System.Text;
using IntelligentTagging.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IntelligentTagging.Model
{
    /// <summary>
    /// SocialTagElement is a specific class represent a fields from Intelligent Tagging JSON message which contains metadata for social tag.
    /// </summary>
    public class SocialTagElement : IMetaData
    {
        public string TypeGroup { get; set; }
        public string Type { get; set; }

        [JsonProperty("id")] 
        private string _id;
        public string ID
        {
            get => ITaggingUtils.UriLastPart(_id);
            set => _id = value;
        }
        public string Name { get; set; }
        public string ForEndUserDisplay { get; set; }
        IDictionary<string, JToken> IMetaData._Attribute { get; set; }
        [JsonProperty("importance")]
        public string Importance { get; set; }
        [JsonProperty("originalValue")]
        public string OriginalValue { get; set; }
        public override string ToString()
        {
            var strBuilder = new StringBuilder();
            strBuilder.Append($"TypeGroup:{TypeGroup}\n");
            strBuilder.Append($"Type:{Type}\n");
            strBuilder.Append($"ID:{ID}\n");
            strBuilder.Append($"Name:[{Name}]\n");
            strBuilder.Append($"ForEndUserDisplay:{ForEndUserDisplay}\n");
            strBuilder.Append($"Importance:{Importance}\n");
            strBuilder.Append($"OriginalValue:{OriginalValue}\n");
        

            return strBuilder.ToString();
        }
    }
}