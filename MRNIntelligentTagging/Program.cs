using System;
using System.Threading;
using System.IO;
using System.Linq;
using System.Net.Http;
using Refinitiv.DataPlatform;
using Refinitiv.DataPlatform.Core;
using Refinitiv.DataPlatform.Content;
using Refinitiv.DataPlatform.Content.News;
using Newtonsoft.Json.Linq;
using IntelligentTagging;
using MRNIntelligentTagging.Model;

namespace MRNIntelligentTagging
{
    class Program
    {
        /// <summary>
        /// TREPCredential is a region to set account for use the sample app with local deployed TREP server.
        /// You need to specify DACS user and set WebSocket Server on ADS.
        /// </summary>
        #region TREPCredential
            private const string TREPUser = "<DACS User>";
            private const string appID = "256";
            private const string position = "<Your IP>/net";
            private const string WebSocketHost = "<Websocket Server IP>:<Websocket Port> eg. 192.168.27.46:15000";
        #endregion

        /// <summary>
        /// RDPUserCredntial is a region to set Username and Password with the Application key in case that you want to use the sample app with ERT in Cloud.
        /// </summary>
        #region RDPUserCredential
            private const string RDPUser = "<RDP User/Email>";
            private const string RDPPassword = "<RDP Password>";
            private const string RDPAppKey = "<App Key>";

        #endregion

        // Set useRDP to true to connecting to ERT in cloud and use login from RDPUserCredential region.
        private static readonly bool useRDP = true;
        
        // Set redirectOutputToFile to redirect output to Output.txt under running directory
        private static readonly bool redirectOutputToFile = false;

        private static Session.State _sessionState = Session.State.Closed;
        private static readonly int runtime=60000000;
        static void Main()
        {
            Console.WriteLine("Start retrieving MRN Story data. Press Ctrl+C to exit");
            // Set RDP.NET Logger level to Trace
            Log.Level = NLog.LogLevel.Trace;
            FileStream fileStream=null;
            StreamWriter streamWriter=null;
            var @out = Console.Out;
            if (redirectOutputToFile)
            {
                Console.WriteLine("Redirect Output to file Output.txt");
                try
                {
                    fileStream = new FileStream("./Output.txt", FileMode.OpenOrCreate, FileAccess.Write);
                    streamWriter = new StreamWriter(fileStream);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Cannot open Output.txt for writing");
                    Console.WriteLine(e.Message);
                    return;
                }
                Console.SetOut(streamWriter);
                @out = Console.Out;
            }

            ISession session;
            if (!useRDP)
            {
                System.Console.WriteLine("Start Deployed PlatformSession");
                session = CoreFactory.CreateSession(new DeployedPlatformSession.Params()
                    .Host(WebSocketHost)
                    .WithDacsUserName(TREPUser)
                    .WithDacsApplicationID(appID)
                    .WithDacsPosition(position)
                    .OnState((s, state, msg) =>
                    {
                        Console.WriteLine($"{DateTime.Now}:  {msg}. (State: {state})");
                        _sessionState=state;
                    })
                    .OnEvent((s, eventCode, msg) => Console.WriteLine($"{DateTime.Now}: {msg}. (Event: {eventCode})")));
            }else
            {
                System.Console.WriteLine("Start RDP PlatformSession");
                session = CoreFactory.CreateSession(new PlatformSession.Params()
                    .WithOAuthGrantType(new GrantPassword().UserName(RDPUser)
                        .Password(RDPPassword))
                    .AppKey(RDPAppKey)
                    .WithTakeSignonControl(true)
                    .OnState((s, state, msg) =>
                    {
                        Console.WriteLine($"{DateTime.Now}:  {msg}. (State: {state})");
                        _sessionState = state;
                    })
                    .OnEvent((s, eventCode, msg) => Console.WriteLine($"{DateTime.Now}: {msg}. (Event: {eventCode})")));
            }
            session.Open();
            if(_sessionState==Session.State.Opened)
            {
                System.Console.WriteLine("Session is now Opened");
                // Validate the selection

                System.Console.WriteLine("Sending MRN_STORY request");
                using var mrn = MachineReadableNews.Definition().OnError((stream, err) => Console.WriteLine($"{DateTime.Now}:{err}"))
                        .OnStatus((stream, status) => Console.WriteLine(status))
                        .NewsDatafeed(MachineReadableNews.Datafeed.MRN_STORY)
                        .OnNewsStory((stream, newsItem) => ProcessNewsContent(newsItem.Raw));
                mrn.Open();
                Thread.Sleep(runtime);
            }

            if (redirectOutputToFile)
            {
                streamWriter?.Close();
                if (fileStream != null) fileStream.Close();
            }

            Console.WriteLine("Stop and Quit the applicaiton");
        }
        /// <summary>
        /// Callback function to process MRN Story update. It returns JSON message in JObject class.
        /// </summary>
        /// <param name="msg"></param>
        private static void ProcessNewsContent(JObject msg)
        {
            if (msg == null) throw new ArgumentNullException(nameof(msg));
            // Print JSON plain text
            //System.Console.WriteLine("***************** RAW JSON Data *******************");
            //System.Console.WriteLine(msg);
            //System.Console.WriteLine("***************************************************");
            var mrnobj = msg.ToObject<Model.StoryData>();

            Console.WriteLine($"========================= Story update=======================");
            Console.WriteLine($"GUID:{mrnobj.Id}");
            Console.WriteLine($"AltId:{mrnobj.AltId}");
            Console.WriteLine($"Headline:{mrnobj.Headline}\n");
            Console.WriteLine($"Body:{mrnobj.Body}");
            Console.WriteLine("==============================================================");
            if(mrnobj.Language.ToLower() == "en")
                 GetMetaData(mrnobj, "<API KEY>");
        }

        private static void GetMetaData(StoryData storyData,string appKey)
        {
            if (storyData == null) throw new ArgumentNullException(nameof(storyData));
            if (appKey == null) throw new ArgumentNullException(nameof(appKey));
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var it_content= IntelligentTagging.Utils.ITaggingUtils.GetIntelligentTagging(client, appKey, storyData.Body);
                    //Print JSON plain text
                    //Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(it_content, Newtonsoft.Json.Formatting.Indented));
                    DumpTagging(it_content);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
        }
        /// <summary>
        /// DumpTagging is a function to print a known tag and entity such as Person and Company to console output.
        /// </summary>
        /// <param name="tagObj"> is Intelligent Tagging output in JObject class</param>
        private static void DumpTagging(JObject tagObj)
        {
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>Intelligent Tagging Output <<<<<<<<<<<<<<<<<<<<<<");
            var it = new IntelligentTaggingParser(tagObj);
            Console.WriteLine("=====Language======");
            Console.WriteLine(it.LanguageTag);
            Console.WriteLine("===================\n");

            if (it.SocialTags.Any())
            {
                Console.WriteLine("\n===========Social Tags List==========");
                foreach(var item in it.SocialTags)
                    Console.WriteLine(item);
                Console.WriteLine("\n=====================================");
            }
            else
            {
                Console.WriteLine("No SocialTags");
            }

            if (it.PersonElements.Any())
            {
                Console.WriteLine("\n===========Person Entity List==========");
                foreach(var person in it.PersonElements)
                    Console.WriteLine(person);
                Console.WriteLine("\n=======================================");
            }
            else
            {
                Console.WriteLine("No Person Entity");
            }

            if (it.CompanyElements.Any())
            {
                Console.WriteLine("\n===============Company Entity===========");
                foreach(var company in it.CompanyElements)
                    Console.WriteLine(company);
                Console.WriteLine("\n========================================");
            }
            else
            {
                Console.WriteLine("No Company Entity");
            }

            Console.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
        }
    }
}
