using System;
using System.Collections.Generic;
using System.Linq;
using IntelligentTagging.Model;
using Newtonsoft.Json.Linq;

namespace IntelligentTagging
{
    public class IntelligentTaggingParser
    {
     
        public IntelligentTaggingParser(JObject jsondata)
        {
            Parse(jsondata);
        }
        private readonly IDictionary<string,IMetaData> _elementList=new Dictionary<string, IMetaData>();
        public LanguageElement LanguageTag
        {
            get { return (LanguageElement)_elementList.Select(x => x).FirstOrDefault(y => y.Value.TypeGroup == "language").Value; }
        }

        public IList<string> Versions
        {
            get
            {
                return _elementList.Where(y => y.Value.TypeGroup == "versions").Select(x => ((VersionElement)x.Value).Version).FirstOrDefault();
            }
        }

        public IDictionary<string, IMetaData> ITData => _elementList;
        public IList<PersonEntityElement> PersonElements => _elementList.Where(y => y.Value.TypeGroup == "entities" && y.Value.Type == "Person")
                    .Select(x => (PersonEntityElement)x.Value).ToList<PersonEntityElement>();

        public IList<CompanyEntityElement> CompanyElements => _elementList.Where(y => y.Value.TypeGroup == "entities" && y.Value.Type == "Company")
            .Select(x => (CompanyEntityElement)x.Value).ToList<CompanyEntityElement>();

        public IList<SocialTagElement> SocialTags => _elementList.Where(y => y.Value.TypeGroup == "socialTag").Select(x => (SocialTagElement)x.Value).ToList<SocialTagElement>();
        public IList<string> SocialTagNames => _elementList.Where(y => y.Value.TypeGroup == "socialTag").Select(x => x.Value.Name).ToList<string>();

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
    }
}