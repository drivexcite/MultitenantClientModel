namespace ContentModelImporter
{
    public class Concept
    {
        public string ConceptId { get; set; }
        public string Label { get; set; }
        public string TaxonomyJson { get; set; }
        public string AlternativeLabelsJson { get; set; }
        public string KeywordsJson { get; set; }
        public string CodeSystemsJson { get; set; }
        public string NuccSpecialtiesJson { get; set; }
        public string RelationshipsJson { get; set; }
        public string TopicAspectsJson { get; set; }
        public string ArticleAspectsJson { get; set; }
        public string ParentPaths { get; set; }
        public bool IsSpecialty { get; set; }
        public bool IsTopLevel { get; set; }
        public bool IsActive { get; set; }
        public string ConceptStatusId { get; set; }
    }
}
