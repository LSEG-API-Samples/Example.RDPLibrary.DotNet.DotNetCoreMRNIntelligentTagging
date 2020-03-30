using System;
using System.Collections.Generic;
using System.Linq;
using IntelligentTagging.Model;
using Newtonsoft.Json.Linq;

namespace IntelligentTagging
{
    /// <summary>
    /// IntelligentTaggingParser is a class implement parser to parse a JSON message which represent a tag and entity Intelligent Tagging output.
    /// </summary>
    public class IntelligentTaggingParser
    {
        /// <summary>
        /// A constructor for initialize IntelligentTaggingParser class you need to pass JObject to the constructor.
        /// </summary>
        /// <param name="jsonData">JObject contain JSON message</param>
        public IntelligentTaggingParser(JObject jsonData)
        {
            Parse(jsonData);
        }
        private readonly IDictionary<string,IMetaData> _elementList=new Dictionary<string, IMetaData>();
        /// <summary>
        /// LanguageTag is LanguageElement from Intelligent Tagging output.
        /// </summary>
        public LanguageElement LanguageTag
        {
            get { return (LanguageElement)_elementList.Select(x => x).FirstOrDefault(y => y.Value.TypeGroup == "language").Value; }
        }
        /// <summary>
        /// Versions is a list of version string from Intelligent Tagging output.
        /// </summary>
        public IList<string> Versions
        {
            get
            {
                return _elementList.Where(y => y.Value.TypeGroup == "versions").Select(x => ((VersionElement)x.Value).Version).FirstOrDefault();
            }
        }
        /// <summary>
        /// Return all elements inside Intelligent Tagging JSON message.
        /// </summary>
        public IDictionary<string, IMetaData> ITData => _elementList;

        /// <summary>
        /// PersonElements represent a list of PersonEntity from Intelligent Tagging output.
        /// </summary>
        public IList<PersonEntity> PersonElements => _elementList.Where(y => y.Value.TypeGroup == "entities" && y.Value.Type == "Person")
                    .Select(x => (PersonEntity)x.Value).ToList<PersonEntity>();

        /// <summary>
        /// CompanyElements represent a list of CompanyEntity from Intelligent Tagging output.
        /// </summary>
        public IList<CompanyEntity> CompanyElements => _elementList.Where(y => y.Value.TypeGroup == "entities" && y.Value.Type == "Company")
            .Select(x => (CompanyEntity)x.Value).ToList<CompanyEntity>();

        /// <summary>
        /// SocialTags represent a list of SocialElement or social tag from Intelligent Tagging output.
        /// </summary>
        public IList<SocialTagElement> SocialTags => _elementList.Where(y => y.Value.TypeGroup == "socialTag").Select(x => (SocialTagElement)x.Value).ToList<SocialTagElement>();
        public IList<string> SocialTagNames => _elementList.Where(y => y.Value.TypeGroup == "socialTag").Select(x => x.Value.Name).ToList<string>();

        /// <summary>
        /// Internal function to parse Intelligent Tagging from JObject.
        /// </summary>
        /// <param name="taggingObject"></param>
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
                                    var newObj = element.Value.ToObject<PersonEntity>();
                                    _elementList.Add(element.Key, newObj);
                                    break;
                                }
                                case @"Company":
                                {
                                    var newObj = element.Value.ToObject<CompanyEntity>();
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
    }
}