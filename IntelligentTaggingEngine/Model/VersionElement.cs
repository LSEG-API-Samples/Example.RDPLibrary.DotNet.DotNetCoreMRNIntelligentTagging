using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IntelligentTagging.Model
{
    public class VersionElement : IMetaData
    {
        public string TypeGroup { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string ForEndUserDisplay { get; set; }
        IDictionary<string, JToken> IMetaData._Attribute { get; set; }
        [JsonProperty("version")]
        public IList<string> Version { get; set; }
        public override string ToString()
        {
            var strBuilder = new StringBuilder();
            strBuilder.Append($"TypeGroup:{TypeGroup}\n");
            strBuilder.Append($"Type:{Type}\n");
            strBuilder.Append($"Name:{Name}\n");
            strBuilder.Append($"ForEndUserDisplay:{ForEndUserDisplay}\n");
            strBuilder.Append(string.Join("\n",Version));

            return strBuilder.ToString();
        }

    }
}