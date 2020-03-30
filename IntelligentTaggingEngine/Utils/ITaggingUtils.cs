using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace IntelligentTagging.Utils
{
    
    /// <summary>
    /// ITaggingUtils is a utility class contains a function to retrieve Intelligent Tagging data.
    /// Utility function for parsing the output will be added to this class.
    /// </summary>
    public class ITaggingUtils
    {
        /// <summary>
        /// GetIntelligentTagging is a function to retrieve a tag and entity metadata from Calais endpoint.
        /// </summary>
        /// <param name="client"> is a HTTPClient</param>
        /// <param name="appKey"> is Intelligent Tagging application key</param>
        /// <param name="textMsg">is a text a string client want to to get the tag metadata from Intelligent Tagging service</param>
        /// <returns> Returns JObject represent the JSON output from Intelligent Tagging service.</returns>
        public static JObject GetIntelligentTagging(HttpClient client, string appKey, string textMsg)
        {
            var postUri = new Uri("https://api-eit.refinitiv.com/permid/calais");
            var request = new HttpRequestMessage(HttpMethod.Post, postUri) { Content = new StringContent(textMsg) };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/raw");
            request.Content.Headers.Add("X-AG-Access-Token", appKey);
            request.Content.Headers.Add("x-calais-selectiveTags", "company,person,industry,socialtags,topic,country");
            request.Content.Headers.Add("omitOutputtingOriginalText", "false");
            request.Content.Headers.Add("outputformat", "application/json");


            var response = client.SendAsync(request).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            var jsonData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();



            return JObject.Parse(jsonData);
        }
        /// <summary>
        /// Used to get the last part of Uri represents by the RDF document.
        /// </summary>
        /// <param name="uri">Uri string</param>
        /// <returns>Return last part of the Uri</returns>
        public static string UriLastPart(string uri)
        {
            return uri.Split('/').Last();
        }
    }
}
