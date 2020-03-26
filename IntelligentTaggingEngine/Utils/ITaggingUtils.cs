using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace IntelligentTagging.Utils
{
    
    public class ITaggingUtils
    {
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
        public static string UriLastPart(string uri)
        {
            return uri.Split('/').Last();
        }
    }
}
