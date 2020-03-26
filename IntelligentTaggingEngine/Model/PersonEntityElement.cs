using System.Collections.Generic;
using System.Text;
using IntelligentTagging.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IntelligentTagging.Model
{
    public class PersonEntityElement : IMetaData
    {
        public string TypeGroup { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string ForEndUserDisplay { get; set; }
        IDictionary<string, JToken> IMetaData._Attribute { get; set; }
        [JsonProperty("confidencelevel")]
        public string ConfidentceLevel { get; set; }
        [JsonProperty("persontype")]
        public string PersonType { get; set; }
        [JsonProperty("firstname")]
        public string FirstName { get; set; }
        [JsonProperty("lastname")]
        public string LastName { get; set; }
        [JsonProperty("nationality")]
        public string Nationality { get; set; }
        [JsonProperty("commonname")]
        public string CommonName { get; set; }

        [JsonProperty("_typeReference")] 
        private string _typeReference;

        public string TypeReference
        {
            get => ITaggingUtils.UriLastPart(_typeReference);
            set => _typeReference = value;
        }
   
        [JsonProperty("permid")]
        private string _permid;

        public string PermId
        {
            get => ITaggingUtils.UriLastPart(_permid);
            set => _permid = value;
        }

        [JsonProperty("relevance")]
        public string Relevance { get; set; }
        [JsonProperty("confidence")]
        public Confidence ConfidenceObject { get; set; }
        public override string ToString()
        {
            var strBuilder = new StringBuilder();
            strBuilder.Append($"TypeGroup:{TypeGroup}\n");
            strBuilder.Append($"Type:{Type}\n");
            strBuilder.Append($"Name:{Name}\n");
            strBuilder.Append($"ForEndUserDisplay:{ForEndUserDisplay}\n");
            strBuilder.Append($"confidencelevel:{ConfidentceLevel}\n");
            strBuilder.Append($"persontype:{PersonType}\n");
            strBuilder.Append($"FirstName:{FirstName}\n");
            strBuilder.Append($"LastName:{LastName}\n");
            strBuilder.Append($"nationality:{Nationality}\n");
            strBuilder.Append($"_typeReference:{TypeReference}\n");
            strBuilder.Append($"permid:{PermId}\n");
            strBuilder.Append($"commonname:{CommonName}\n");
            strBuilder.Append($"relevance:{Relevance}\n");
            strBuilder.Append($"confidence:\n");
            strBuilder.Append(ConfidenceObject);


            return strBuilder.ToString();
        }
    }
}