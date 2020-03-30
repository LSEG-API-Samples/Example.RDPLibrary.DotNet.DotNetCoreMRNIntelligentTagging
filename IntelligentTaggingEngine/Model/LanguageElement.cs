using System.Collections.Generic;
using System.Text;
using IntelligentTagging.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IntelligentTagging.Model
{
    /// <summary>
    /// LanguageElement is a class to holds language and permid of the output from Intelligent Tagging.
    /// </summary>
    public class LanguageElement : IMetaData
    {
        public string TypeGroup { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string ForEndUserDisplay { get; set; }
        IDictionary<string, JToken> IMetaData._Attribute { get; set; }

        [JsonProperty("language")]
        private string _language;
        public string Language
        {
            get => ITaggingUtils.UriLastPart(_language);
            set => _language = value;
        }

        [JsonProperty("permid")]
        public string PermId { get; set; }

        public override string ToString()
        {
            var strBuilder = new StringBuilder();
            strBuilder.Append($"TypeGroup:{TypeGroup}\n");
            strBuilder.Append($"Type:{Type}\n");
            strBuilder.Append($"Name:{Name}\n");
            strBuilder.Append($"ForEndUserDisplay:{ForEndUserDisplay}\n");
            strBuilder.Append($"language:{Language}\n");
            strBuilder.Append($"permid:{PermId}\n");

            return strBuilder.ToString();
        }

    }
}