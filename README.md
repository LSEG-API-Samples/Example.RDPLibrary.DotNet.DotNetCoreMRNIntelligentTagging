# Using Intelligent Tagging REST API with RDP.NET to generates tag metadata from MRN Real-Time News Story

## Introduction

[Intelligent Tagging REST API](https://developers.refinitiv.com/open-permid/intelligent-tagging-restful-api) can use to automatically analyzes your input text. And then identify and tag mentions (text strings) of things like companies, people, deals, geographical locations, industries, physical assets, organizations, products, events, etc. based on a list of predefined metadata types. Moreover, it can assigns tags such as topic tags, social tags, industry tags, and slugline tags that describe what the input document is about as a whole. 

There are some use cases that users want to extract the tags(e.g., social tags) and metadata like the importance and confidence score from news content that came with the [MRN Story Real-Time](https://developers.refinitiv.com/sites/default/files/ThomsonReutersMRNElektronDataModelsv210_2.pdf) data and MRN achieved(custom solution provided by Elektron). Then it can feed the output from Intelligent Tagging API to their own sentiment analysis system. This article will describe steps to use the REST API to analyze the input text from the MRN news story retrieved from deployed TREP server or [Elektron Real-Time(ERT)](https://developers.refinitiv.com/elektron/websocket-api/learning?content=63493&type=learning_material_item) in Cloud. We will create a .NET Core sample application that is using RDP.NET API to communicating and retrieving MRN News Story from the server. And then, extract the news body from the MRN JSON message and pass it to the Intelligent Tagging API to identify tags and relevant metadata. 

## Prerequisites

* Please read [Intelligent Tagging quick start guide](https://developers.refinitiv.com/open-permid/intelligent-tagging-restful-api/quick-start) before reading this article. You need to access the link from the quick start guide to register for a free account and create your API key. It required the key for API call.
* Please read [this article](https://developers.refinitiv.com/article/create-mrn-real-time-news-story-consumer-app-using-net-core-and-rdpnet-library), you need to understand how to use RDP.NET library to retrieve MRN Story data.
* You need ERT in cloud account or deployed TREP server(version 3.2 or higher) with a DACS User with permission to request MRN STORY data.
* You need to install .NET Core SDK. Highly recommended version 3.0 or higher versions.
* [Visual Studio Code](https://code.visualstudio.com/)/Text editor or Visual Studio 2017/2019 to open solution/project and modify codes.

## Creating .NET Core application 

There are three main steps in our sample application. 

1) Requesting MRN Story data from ERT in cloud or deployed TREP server via the WebSocket connection. These steps will use RDP.NET SDK, and it provides a callback method that returns the MRN JSON data. 

2) Using Intelligent Tagging REST API with the MRN news content.

3) Parsing a SocialTag and metadata from the response message provided by the REST API and print the output to console. 

### Step 1 Requesting MRN Story data

To make it short, we will modify the sample application provided in the article [Create MRN Real-Time News Story Consumer app using .NET Core and RDP.NET Library](https://developers.refinitiv.com/article/create-mrn-real-time-news-story-consumer-app-using-net-core-and-rdpnet-library) to retrieve the MRN Story data. Then we can extract the news body from the JSON message and pass it to the Intelligent Tagging REST API. Thus we will not describe the background of MRN and how to use RDP.NET in this article. 

Below is a sample code to use __MachineReadableNews__ for retrieving and handling MRN Story update.

```C#

      using (MachineReadableNews mrnNews =ContentFactory.CreateMachineReadableNews(new MachineReadableNews.Params()
              .Session(session)
              .WithNewsDatafeed("MRN_STORY")
              .OnError((e,msg)=>Console.WriteLine(msg))
              .OnStatus((e, msg) => Console.WriteLine(msg))
              .OnNews((e, msg) => ProcessNewsContent(msg))))
      {
          mrnNews.Open();
          Thread.Sleep(runtime);
      }
```
In the sample app, we wish to extract the news content which contains the English language code. Therefore, we need to filter the message by checking the __language__ code in the JSON message. 

Below is a sample of JSON message for the MRN Story, which contains English language code "en."

```JSON
{
    "altId": "nBSE4njFBT",
    "audiences": [ "NP:BSE" ],
    "body": "Firstsource Solutions Ltd has informed BSE that the members of the Company have passed the resolution by way of Postal Ballot, under Clause 35A.\n\n \n\nhttp://pdf.reuters.com/pdfnews/pdfnews.asp?i=43059c3bf0e37541&u=urn:newsml:reuters.com:20150924:nBS E6yFYfg\n\n \n\n \n\nDouble click on the URL above to view the article.Please note that internet access is required. If you experience problem accessing the internet, please consult your network administrator or technical support\n\nLatest version of Adobe Acrobat reader is recommended to view PDF files. The latest version of the reader can be obtained from http://www.adobe.com/products/acrobat/readstep2.html\n\n",
    "firstCreated": "2015-09-24T15:41:50.000Z",
    "headline": "FIRSTSOURCE SOLUTIONS LTD. - Results of Postal Ballot (Clause 35A) <FISO.NS>",
    "id": "BSE4njFBT_1509242kv2m5neJzQ52U7adOPFd2fc4P6PMZ/X8yPsDxw",
    "instancesOf": [],
    "language": "en",
    "messageType": 2,
    "mimeType": "text/plain",
    "provider": "NS:BSE",
    "pubStatus": "stat:usable",
    "subjects": [ "R:FISO.NS", "P:4295873587", "B:195", "B:34", "B:43", "B:49", "BL:52", "G:1", "G:5B", "G:K", "M:Z", "N2:BSUP", "N2:INDS", "N2:ISER", "N2:CMSS", "N2:BUS", "N2:EMRG", "N2:IN", "N2:ASIA", "N2:CMPNY" ],
    "takeSequence": 1,
    "urgency": 3,
    "versionCreated": "2015-09-24T15:41:50.000Z"
}
``` 

### Step 2 Using Intelligent Tagging REST API with the news content

The RDP.NET library returns the MRN Story data as the [JObject](https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_Linq_JObject.htm) in the callback function. Therefore, we need to create a StoryData class to hold the value of the MRN Story update. Then the application will call method JObject.ToObject<Model.StoryData>() to deserialize the JSON data to StoryData object. Below is the StoryData class, which created based on the structure of the MRN Story JSON message.

```c#
 public class StoryData
    {
        [Newtonsoft.Json.JsonProperty("altId", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string AltId { get; set; }
        ...
        public string Id { get; set; }
        [Newtonsoft.Json.JsonProperty("instanceOf", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public IList<string> InstanceOf { get; set; }
        [Newtonsoft.Json.JsonProperty("language", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Language { get; set; }
        [Newtonsoft.Json.JsonProperty("provider", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Provider { get; set; }
        [Newtonsoft.Json.JsonProperty("pubStatus", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string PubStatus { get; set; }
        [Newtonsoft.Json.JsonProperty("subjects", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        ...
        public DateTime VersionCreated { get; set; }
        [Newtonsoft.Json.JsonProperty("body", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Body { get; set; }
        [Newtonsoft.Json.JsonProperty("mineType", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        ...

    }
```
Application uses the following codes to deserialize the JSON data from the instance of JObject to StoryData.

```c#
 var mrnobj = msg.ToObject<Model.StoryData>();
```
Then the application can get the news content from mrnObj.Body and pass it to HTTP request content of the REST API. 

#### Calling Intelligent Tagging API

The API call to tag content is made via a simple HTTP REST interface. Below are the endpoint and HTTP Request message and required parameters we need to set in the request.

* Endpoint: https://api-eit.refinitiv.com/permid/calais

* HTTP Method: __POST__

* HTTP Request Headers:

        Content-Type : text/raw 
        X-AG-Access-Token : <Your App Key>
        x-calais-selectiveTags : <a custom set of metadata tag types to be included in the output likes company,person,industry,socialtags,topic>
        outputformat : application/json
        omitOutputtingOriginalText: <true/false> Optional and highly recommended for large input files.

* HTTP Request Content: Text from a news content

We will use the above headers in our sample app. You can find additional headers with the details about the value you can set from [Input Headers document](https://developers.refinitiv.com/open-permid/intelligent-tagging-restful-api/docs?content=76840&type=documentation_item). Please refer to the [online document](https://developers.refinitiv.com/open-permid/intelligent-tagging-restful-api/quick-start) in case that there might be the change of endpoint for your package type.

Below is a sample of C# codes to call the REST API tag content from the MRN Story body. We set the output format to "__application/json__" because we need the output in JSON format. Then we can use the JSON.NET library to parse specific metadata such as social tag, company, person and its scores from the JSON output.

```C#
            using (HttpClient client = new HttpClient())
            {
                var postUri = new Uri("https://api-eit.refinitiv.com/permid/calais");
                var request = new HttpRequestMessage(HttpMethod.Post, postUri);
                request.Content = new StringContent(mrnobj.Body);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/raw");
                request.Content.Headers.Add("X-AG-Access-Token", appKey);
                request.Content.Headers.Add("x-calais-selectiveTags", "company,person,industry,socialtags,topic");
                request.Content.Headers.Add("omitOutputtingOriginalText", "false");
                request.Content.Headers.Add("outputformat", "application/json");

                var response = client.SendAsync(request).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    var jsonData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Console.WriteLine(JToken.Parse(jsonData).ToString(Formatting.Indented));
                }
                else
                {
                    // Error in Exception 
                    Console.WriteLine(response);
                }
              
            }
```

The following JSON message is sample output from Intelligent Tagging API call.

```JSON
{
  "doc": {
    "info": {
      "calaisRequestID": "fe2f8c19-8db0-8a01-70e7-fca923784d2e",
      "id": "http://id.opencalais.com/PXlKTZ0ztAnbLi8MZvcBxw",
      "ontology": "http://mdaas-virtual-onecalais.int.thomsonreuters.com/owlschema/13.0.rc2/onecalais.owl.allmetadata.xml",
      "docId": "http://d.opencalais.com/dochash-1/c1dbb7d5-9ea9-364e-9cae-e9be01b46df5",
      "document": "...",
      "docTitle": "",
      "docDate": "2020-03-17 10:14:21.089"
    },
    "meta": {
      "contentType": "text/raw",
      "processingVer": "AllMetadata",
      "selectiveTags": "company,person,industry,socialtags,topic",
      "serverVersion": "13.0.310:310",
      "stagsVer": "defaultVersion",
      "submissionDate": "2020-03-17 10:14:20.195",
      "submitterCode": "0ca6a864-5659-789d-5f32-f365f695e757",
      "signature": "digestalg-1|3EWT4Ml/WuJe0YGFqKS/E+Hjtv4=|HWjZFEceMj7QbJvOPI/IXl4MdeslhblvZnnVXE50rn6M+8iSTHGc+g==",
      "language": "English"
    }
  },
  "http://d.opencalais.com/dochash-1/c1dbb7d5-9ea9-364e-9cae-e9be01b46df5/lid/DefaultLangId": {
    "_typeGroup": "language",
    "language": "http://d.opencalais.com/lid/DefaultLangId/English",
    "forenduserdisplay": "false",
    "permid": "505062"
  },
  "http://d.opencalais.com/dochash-1/c1dbb7d5-9ea9-364e-9cae-e9be01b46df5/SocialTag/1": {
    "_typeGroup": "socialTag",
    "id": "http://d.opencalais.com/dochash-1/c1dbb7d5-9ea9-364e-9cae-e9be01b46df5/SocialTag/1",
    "socialTag": "http://d.opencalais.com/genericHasher-1/f1af9688-5498-363e-9052-be898dce559c",
    "forenduserdisplay": "true",
    "name": "Money",
    "importance": "1",
    "originalValue": "Money"
  },
  "http://d.opencalais.com/dochash-1/c1dbb7d5-9ea9-364e-9cae-e9be01b46df5/SocialTag/2": {
    "_typeGroup": "socialTag",
    "id": "http://d.opencalais.com/dochash-1/c1dbb7d5-9ea9-364e-9cae-e9be01b46df5/SocialTag/2",
    "socialTag": "http://d.opencalais.com/genericHasher-1/acd44d54-d8f3-3c36-a78c-2da2af5a2ae8",
    "forenduserdisplay": "true",
    "name": "Finance",
    "importance": "1",
    "originalValue": "Finance"
  },
  "http://d.opencalais.com/dochash-1/c1dbb7d5-9ea9-364e-9cae-e9be01b46df5/SocialTag/3": {
    "_typeGroup": "socialTag",
    "id": "http://d.opencalais.com/dochash-1/c1dbb7d5-9ea9-364e-9cae-e9be01b46df5/SocialTag/3",
    "socialTag": "http://d.opencalais.com/genericHasher-1/412d2c87-2ead-391a-ae44-6add965d8ada",
    "forenduserdisplay": "true",
    "name": "Economy",
    "importance": "1",
    "originalValue": "Economy"
  },
  ...
  "http://d.opencalais.com/dochash-1/c1dbb7d5-9ea9-364e-9cae-e9be01b46df5/ComponentVersions": {
    "_typeGroup": "versions",
    "version": [
     ...<List of Versions>
    ]
  },
  "http://d.opencalais.com/comphash-1/0103595a-e6ab-3eda-923f-cd23fc7d7fee": {
    "_typeGroup": "entities",
    "_type": "Company",
    "forenduserdisplay": "true",
    "name": "U.S. Companies [TOP/EQU",
    "confidencelevel": "1.0",
    "recognizedas": "name",
    "_typeReference": "http://s.opencalais.com/1/type/em/e/Company",
    "instances": [
      {
        "detection": "[ IFR Markets              [TOP/NOW2]   ]U.S. Companies                 [TOP/EQU[]\n  European Companies        [TOP/EQE]   Asian]",
        "prefix": " IFR Markets              [TOP/NOW2]   ",
        "exact": "U.S. Companies                 [TOP/EQU",
        "suffix": "]\n  European Companies        [TOP/EQE]   Asian",
        "offset": 1913,
        "length": 39
      }
    ],
    "relevance": 0.2,
    "confidence": {
      "statisticalfeature": "0.768",
      "dblookup": "0.0",
      "resolution": "0.0",
      "aggregate": "1.0"
    }
  },
  "http://d.opencalais.com/comphash-1/ddbb8a21-4aae-36f9-b7e6-77e5b142f973": {
    "_typeGroup": "entities",
    "_type": "Company",
    "forenduserdisplay": "false",
    "name": "BMAT",
    "score": 2,
    "confidencelevel": "0.641",
    "__label": "Processing section",
    "__value": "BMAT",
    "recognizedas": "name",
    "_typeReference": "http://s.opencalais.com/1/type/em/e/Company",
    "instances": [
      {
        "detection": "[     [TOP/BASIC] \n  Basic Mterials           [TOP/]BMAT[]   Healthcare                  [TOP/HEALTH]\n ]",
        "prefix": "     [TOP/BASIC] \n  Basic Mterials           [TOP/",
        "exact": "BMAT",
        "suffix": "]   Healthcare                  [TOP/HEALTH]\n ",
        "offset": 2313,
        "length": 4
      }
    ],
    "relevance": 0.2,
    "confidence": {
      "statisticalfeature": "0.876",
      "dblookup": "0.0",
      "resolution": "0.0046225167",
      "aggregate": "0.641"
    },
    "resolutions": [
      {
        "name": "BMAT INC(BWAY CORP)",
        "permid": "4296687281",
        "ispublic": "false",
        "commonname": "BMAT INC(BWAY",
        "score": 0.0046225167,
        "id": "https://permid.org/1-4296687281"
      }
    ]
  },
  "http://d.opencalais.com/comphash-1/8b73a23b-07f3-373f-9422-44dbd6eb149b": {
    "_typeGroup": "entities",
    "_type": "Company",
    "forenduserdisplay": "true",
    "name": "MCDONALD 'S",
    "confidencelevel": "1.0",
    "recognizedas": "name",
    "_typeReference": "http://s.opencalais.com/1/type/em/e/Company",
    "instances": [
      {
        "detection": "[on phone lines and Lego      [nL8N2B97DV]\n> ]McDonald's[, Starbucks limit dine-in service in U.S. to slow]",
        "prefix": "on phone lines and Lego      [nL8N2B97DV]\n> ",
        "exact": "McDonald's",
        "suffix": ", Starbucks limit dine-in service in U.S. to slow",
        "offset": 681,
        "length": 10
      }
    ],
    "relevance": 0.2,
    "resolutions": [
      {
        "name": "MCDONALD'S CORPORATION",
        "permid": "4295904499",
        "primaryric": "MCD.N",
        "ispublic": "true",
        "commonname": "Mcdonald's Corp",
        "score": 0.28596395,
        "id": "https://permid.org/1-4295904499",
        "ticker": "MCD"
      }
    ],
    "confidence": {
      "statisticalfeature": "0.668",
      "dblookup": "0.0",
      "resolution": "0.28596395",
      "aggregate": "1.0"
    }
  }
}
```
The whole JSON message consists of a list of JSON objects for tag and entity like socialTag, Person and Company entity. Please note that result in the output depends on the input text and type of account you are using. 
 
The application can use JSON.NET to parse the object and check key "_typeGroup"  to identify the type of the object. Then the application can get specific property such as the confidence score, importance level from the key in JSON object. You can find available properties for each type of tag metadata from [Intelligent Tagging Document](https://developers.refinitiv.com/open-permid/calais-tagging-restful-api/docs?content=76844&type=documentation_item)

### Step 3 Parsing a SocialTag and metadata from the response message

The application can get tag metadata from the Intelligent Tagging JSON output, by using JSON.NET to convert the JSON plain text to JObject by calling __JObject.Parse(jsonoutput)__ method. And then you can access the value of social tag and entities using the following snippet of codes.

```c#
var it_content = JObject.Parse(jsonoutput);
// iterate through each object
foreach (var obj in it_content)
{
    if (obj.Value["_typeGroup"] != null)
    {
        switch (((string) obj.Value["_typeGroup"]).ToLower())
        {
            case @"socialtag":
            {
                Console.WriteLine($"Social Tag Name:{obj.Value["name"]}");
                break;
            }
            case @"entities":
            {
                Console.WriteLine($"Entity Type:{obj.Value["_type"]} Name:{obj.Value["name"]}");
                break;
            }
            //... Unhandled type
         }
    }
}
``` 

It will show the output like the following sample.

```
Social Tag Name:Financial software
Social Tag Name:Thomson Reuters
Social Tag Name:Companies
Social Tag Name:Eikon
Social Tag Name:Business
Social Tag Name:Economy
Social Tag Name:CNBC
Social Tag Name:Reuters
Social Tag Name:Westlaw
Social Tag Name:Isan
Entity Type:Company Name:Thomson Reuters
```

Anyway, our sample will create an interface named IMetaData, which is a based interface to create a more specific class for particular tag and entity. Then application just needs to check the value of the property named "_typeGroup" to identify what kind of class needs to use for deserializing from the JSON object.

Below is an implementation of the IMetaData interface.

```c#
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
```
And the following class is the SocialTagElement class and BaseElement class which sharing a generic property with the other class. SocialTagElement class was created for holding SocialTag data. We will add a more specific property, such as an importance score and original value to the class.

```c#
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
            ...
        }
    }
 public class BaseElement : IMetaData
    {
        public string TypeGroup { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string ForEndUserDisplay { get; set; }
        public IDictionary<string, JToken> _Attribute { get; set; }
        public override string ToString()
        {
            ...
        }
    }
```
The sample app also provides IntelligentTaggingParser class for parsing the JSON object. It will keep the tag metadata object in the C# Dictionary class where the Dictionary's key is a hash key defined by Intelligent Tagging API.

```c#
private readonly IDictionary<string,IMetaData> _elementList=new Dictionary<string, IMetaData>();
//...

private void Parse(JObject taggingObject)
{
    foreach (var element in taggingObject )
    {
        if (element.Key != "doc")
        {
          var baseElement = element.Value.ToObject<BaseElement>() ?? throw new ArgumentNullException("element.Value.ToObject<BaseElement>()");
          switch (baseElement.TypeGroup.Trim())
          {
              case @"language":
              {
                  var newObj = element.Value.ToObject<LanguageElement>();
                  _elementList.Add(element.Key,newObj);
                  break;
              }
              case @"socialTag":
              {
                  var newObj = element.Value.ToObject<SocialTagElement>();
                  _elementList.Add(element.Key, newObj);
                  break;
              }
              case @"versions":
              {
                  var newObj = element.Value.ToObject<VersionElement>();
                  _elementList.Add(element.Key, newObj);
                  break;
              }
              case @"entities":
              {
                  switch (baseElement.Type)
                  {
                      case @"Person":
                      {
                          var newObj = element.Value.ToObject<PersonEntityElement>();
                          _elementList.Add(element.Key, newObj);
                          break;
                      }
                      case @"Company":
                      {
                          var newObj = element.Value.ToObject<CompanyEntityElement>();
                          _elementList.Add(element.Key, newObj);
                          break;
                      }
                  }
                    break;
               }
               default:
               {
                   // Add unhandled tag or entity to the list.
                   _elementList.Add(element.Key, baseElement);
                  break;
                }
          }
        }

    }
}
```
The class also provide property to get a list of tag and entities. The below snippet of codes uses Linq to query the list of the social tag from Dictionary object.
```c#
        public IList<SocialTagElement> SocialTags => _elementList.Where(y => y.Value.TypeGroup == "socialTag").Select(x => (SocialTagElement)x.Value).ToList<SocialTagElement>();
```

Please note that the sample app currently supports only some of a known tag or entity such as Language, Version, SocialTag, Person, and Company entity. Anyway, you can modify the app to add more entity classes to get a more specific object from the JSON message.

#### Use IntelligentTaggingParser in the sample application.

The application will create a HttpClient object and pass it to GetIntelligentTagging, which is a utility function. It will return JObject, which you can pass it to IntelligentTaggingParser class.

Sample codes to get JObject from Intelligent Tagging REST API

```c#
  private static void GetMetaData(StoryData storyData,string appKey)
        {
            if (storyData == null) throw new ArgumentNullException(nameof(storyData));
            if (appKey == null) throw new ArgumentNullException(nameof(appKey));
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var it_content= IntelligentTagging.Utils.ITaggingUtils.GetIntelligentTagging(client, appKey, storyData.Body);
                    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(it_content, Newtonsoft.Json.Formatting.Indented));
                    DumpTagging(it_content);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
        }
```
The sample app will pass the JObject to the DumpTagging method to parse the data and print its values to console output. 

```C#
        private static void DumpTagging(JObject tagObj)
        {
            
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
        }
```

You can download full source codes with projects from [GitHub](https://github)

## Build and Run the sample app

The sample application is a .NET Core console application so that you can build and run it on a platform supported by .NET Core SDK.

Please modify Program.cs and add your username and password in either region TREPCredential or RDPUserCredential. Default mode will use the RDP account, and you can change useRDP to false if you want to test with a local deployed TREP server.

You can open a solution file MRNIntelligentTagging.sln on Visual Studio 2017 or 2019 and then build or publish(menu Build->Publish MRNIntelligentTagging) the console application.

If you do not have Visual Studio, you can install the .NET Core SDK on your OS, and you may follow the following steps to build the application.

1) Run the Windows command line or using the terminal on macOS or Linux. Change folder to the repository from GitHub. You should see MRNIntelligentTagging.sln in that folder. Then change folder to MRNIntelligentTagging folder, which is the primary app project folder. 

2) Make sure that you are running with .NET Core 3 or higher version. Just check by running **dotnet --version**. 

3) Run below **dotnet publish** command where "-c release" is for the release build, and "release_build" is the name of the output folder. Note that the below command is for Windows x64 build.

```
dotnet publish -c release -r win-x64 -o ./release_build
```
You should see folder release_build with an executable file MRNIntelligentTagging.exe and required DLLs under release_build folder.

To build it on Linux or macOS, you can change **win-x64** to another OS, please find the list from [rid-catalog page](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog).

## Sample Output

Below is a sample output when running the sample app. It will show a news content followed by the tag and entity from the JSON output.

```
========================= Story update=======================
GUID:ACSrp01pa_2003246aLL09i9Df78AyzlCInXK43cD1dLg40CzcSOhn
AltId:nACSrp01pa
Headline:Loop Insights Releases Company Outlook for 2020

Body:
VANCOUVER, BC / ACCESSWIRE / March 24, 2020 / Loop Insights Inc. (TSX.V:MTRX)
(the "Company" or "Loop") maintains a positive outlook for the year ahead.
Retailers are already working to rebuild their businesses during these
unprecedented times, and Loop is here to assist them. Loop is focused on
bringing new technologies and tools to brick and mortar retail helping global
businesses recover and create new profitable strategies.

A message from Loop's CEO, Rob Anson:

The Covid-19 crisis has shaken the market unlike we've ever seen. During this
time, Loop's team is working remotely and fully operational. We have created a
strong educational strategy, for our customers, that includes regular delivery
of press releases, blogs, social media content, videos, and podcasts. Over the
next months, expect to see consistent, engaging updates from Loop with a
steady news flow on company progress.

...

This news release contains certain statements which constitute forward-looking
statements or information. Such forward-looking statements are subject to
numerous risks and uncertainties, some of which are beyond Loop's control,
including the impact of general economic conditions, industry conditions, and
competition from other industry participants, stock market volatility and the
ability to access sufficient capital from internal and external sources.
Although Loop believes that the expectations in its forward-looking statements
are reasonable, they are based on factors and assumptions concerning future
events which may prove to be inaccurate. Those factors and assumptions are
based upon currently available information. Such forward-looking statements
are subject to known and unknown risks, uncertainties and other factors that
could influence actual results or events and cause actual results or events to
differ materially from those stated, anticipated or implied in the
forward-looking statements. As such, readers are cautioned not to place undue
reliance on the forward-looking statements, as no assurance can be provided as
to future results, levels of activity or achievements. The forward-looking
statements contained in this news release are made as of the date of this news
release and, except as required by applicable law, Loop does not undertake any
obligation to publicly update or to revise any of the included forward-looking
statements, whether as a result of new information, future events or
otherwise. The forward-looking statements contained in this document are
expressly qualified by this cautionary statement. Trading in the securities of
Loop should be considered highly speculative. There can be no assurance that
Loop will be able to achieve all or any of its proposed objectives.

Neither the TSX Venture Exchange nor it's Regulation Services Provider (as
that term is defined in the policies of the TSX Venture Exchange) accepts
responsibility for the adequacy or accuracy of this release.

SOURCE: Loop Insights Inc.
View source version on accesswire.com:
https://www.accesswire.com/582232/Loop-Insights-Releases-Company-Outlook-for-2020

Copyright 2020 ACCESSWIRE. All Rights Reserved.
==============================================================

============Intelligent Tagging Output =================
=====Language======
TypeGroup:language
Type:
Name:
ForEndUserDisplay:false
language:English
permid:505062

===================

===========Social Tags List==========
TypeGroup:socialTag
Type:
ID:1
Name:[Communication]
ForEndUserDisplay:true
Importance:1
OriginalValue:Communication

TypeGroup:socialTag
Type:
ID:2
Name:[Forward-looking statement]
ForEndUserDisplay:true
Importance:1
OriginalValue:Forward-looking statement

TypeGroup:socialTag
Type:
ID:3
Name:[Microsoft Kin]
ForEndUserDisplay:true
Importance:1
OriginalValue:Microsoft Kin

TypeGroup:socialTag
Type:
ID:4
Name:[Control theory]
ForEndUserDisplay:true
Importance:2
OriginalValue:Control theory

TypeGroup:socialTag
Type:
ID:5
Name:[Academic disciplines]
ForEndUserDisplay:true
Importance:2
OriginalValue:Academic disciplines

TypeGroup:socialTag
Type:
ID:6
Name:[Applied mathematics]
ForEndUserDisplay:true
Importance:2
OriginalValue:Applied mathematics

TypeGroup:socialTag
Type:
ID:7
Name:[Fields of mathematics]
ForEndUserDisplay:true
Importance:2
OriginalValue:Fields of mathematics

=====================================

===========Person Entity List==========
TypeGroup:entities
Type:Person
Name:Cathy Hume
ForEndUserDisplay:true
confidencelevel:0.407
persontype:N/A
FirstName:Cathy
LastName:Hume
nationality:N/A
_typeReference:http://s.opencalais.com/1/type/em/e/Person
permid:https://permid.org/1-404011
commonname:Cathy Hume
relevance:0.2
confidence:
        statisticalfeature:0.507
        dblooku:0.0
        resolutione:0.0
        aggregate:0.407

TypeGroup:entities
Type:Person
Name:Rob Anson
ForEndUserDisplay:true
confidencelevel:0.407
persontype:N/A
FirstName:Rob
LastName:Anson
nationality:N/A
_typeReference:http://s.opencalais.com/1/type/em/e/Person
permid:https://permid.org/1-404011
commonname:Rob Anson
relevance:0.2
confidence:
        statisticalfeature:0.507
        dblooku:0.0
        resolutione:0.0
        aggregate:0.407

=======================================

===============Company Entity===========
TypeGroup:entities
Type:Company
Name:Loop Insights Inc.
ForEndUserDisplay:true
confidencelevel:0.991
recongizedas:name
_typeReference:http://s.opencalais.com/1/type/em/e/Company
relevance:0.8
confidence:
        statisticalfeature:0.999
        dblooku:0.0
        resolutione:0.4679013
        aggregate:0.991

========================================
```
Note that you can set __redirectOutputToFile__ in Program.cs to true if you want to redirect the console output to file instead.

## Summary 

This article explains how to use Intelligent Tagging REST API getting tag metadata from a news content returned by MRN Story data. It also provides a sample application to demonstrate the API usage and provide a sample output from the REST API and how to extract the relevance tag and entity from the JSON output. The sample app uses RDP.NET to retrieve the MRN STORY from ERT in the cloud or local deployed TREP.  The use case can be a solution for the client who needs to implement standard tagging across their news repository, using PermID. Intelligent Tagging API also offers a socialTags with relevance scoring for the clients. The client can feed the output provided by the API to the external system for sentiment analysis.

## References

* [Intelligent Tagging Quick Start Guide](https://developers.refinitiv.com/open-permid/intelligent-tagging-restful-api/quick-start)
* [Intelligent Tagging Case Studies](https://developers.refinitiv.com/open-permid/intelligent-tagging-restful-api/docs?content=59392&type=documentation_item)
* [Intelligent Tagging FAQ](https://developers.refinitiv.com/open-permid/intelligent-tagging-restful-api/docs?content=3575&type=documentation_item)
* [Create MRN Real-Time News Story Consumer app using .NET Core and RDP.NET Library](https://developers.refinitiv.com/article/create-mrn-real-time-news-story-consumer-app-using-net-core-and-rdpnet-library)
* [REFINITIV DATA PLATFORM LIBRARIES - AN INTRODUCTION](https://developers.refinitiv.com/refinitiv-data-platform/refinitiv-data-platform-libraries/docs?content=62446&type=documentation_item)
* [The Refinitiv Data Platform Libraries for .NET (RDP.NET)](https://developers.refinitiv.com/refinitiv-data-platform/refinitiv-data-platform-libraries) 
* [The Introduction to RDP Libraries document](https://developers.refinitiv.com/refinitiv-data-platform/refinitiv-data-platform-libraries/docs?content=62446&type=documentation_item)
* [Refinitiv DataPlatform for .NET Nuget](https://www.nuget.org/packages/Refinitiv.DataPlatform/)
* [Refinitiv DataPlatform Content Nuget](https://www.nuget.org/packages/Refinitiv.DataPlatform.Content/)
* [Elektron Websocket](https://developers.refinitiv.com/elektron/websocket-api/quick-start)
* [MRN DATA MODELS AND ELEKTRON IMPLEMENTATION GUIDE](https://developers.refinitiv.com/sites/default/files/ThomsonReutersMRNElektronDataModelsv210_2.pdf)
* [Newtonsoft JSON.NET document](https://www.newtonsoft.com/json/help/html/Introduction.htm)


