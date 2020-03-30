using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IntelligentTagging.Model
{
    /// <summary>
    /// IMetaData interface
    /// Contains a mains property required by tag and element from the Intelligent Tagging Output
    /// </summary>
    public interface IMetaData
    {
        /// <summary>
        /// TypeGroup property holds the _typeGroup returned by IntelligentTagging API.
        /// Application need to check the value of _typeGroup in order to deserialize the JSON object to specific class.
        /// Some JSON object such as doc object may return empty string.
        /// </summary>
        [JsonProperty("_typeGroup")]
        string TypeGroup { get; set; }

        /// <summary>
        ///  Type of entity object. It may returns empty string for a Tag
        /// </summary>
        [JsonProperty("_type")]
        string Type { get; set; }

        /// <summary>
        /// Tag or entity name
        /// </summary>
        [JsonProperty("name")]
        string Name { get; set; }

        /// <summary>
        /// ForeEndUserDisplay is a flag represents value of forenduserdisplay from Intelligent Tagging JSON message.
        /// </summary>
        [JsonProperty("forenduserdisplay")]
        string ForEndUserDisplay { get; set; }
        /// <summary>
        /// _Attribute used to holds unhandled JSON data from the Intelligent Tagging output
        /// </summary>

        [Newtonsoft.Json.JsonExtensionData]
        IDictionary<string, JToken> _Attribute { get; set; }
    }
}