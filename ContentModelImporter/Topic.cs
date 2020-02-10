using System;
using System.Collections.Generic;
using System.Xml;

namespace ContentModelImporter
{
    public class Topic
    {
        public string TopicId { get; set; }
        public Guid ContentId { get; set; }
        public string Title { get; set; }
        public XmlDocument Document { get; set; }
        public string Localization { get; set; }
        public string TopicTypeId { get; set; }
        public List<(string, string)> AspectsMappings { get; set; }       
        public string VisibilityId { get; set; }
        public string ModuleTypeId { get; set; }
        public int? AudienceFlags { get; set; }
        public decimal? ReadingLevel { get; set; }
    }
}
