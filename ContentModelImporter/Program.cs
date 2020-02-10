using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Dapper;
using Polly;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using System.Diagnostics;

namespace ContentModelImporter
{
    class Program
    {
        private static HttpClient HttpClient = new HttpClient();
        private static string SolrUrl = "http://solr.test.hwapps.net:8983";

        private static async Task<T> ExtractFromGetResponseAsync<T>(string url, Func<JToken, T> transformerFunction)
        {
            try
            {
                var response = await HttpClient.GetStringAsync(url);
                return transformerFunction(JToken.Parse(response));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while calling {url}: {e}");
                throw;
            }
        }

        public static async Task<HashSet<string>> GetFacetedFieldAsync(string solrPath, string facetField)
        {
            var url = $"{SolrUrl}/{solrPath}";
            var fields = await ExtractFromGetResponseAsync(url, json => json["facet_counts"]?["facet_fields"]?[facetField] ?? new JArray());

            return new HashSet<string>((
                from field in fields
                where field.Type == JTokenType.String
                select field.Value<string>().Trim()
            ).Distinct(), StringComparer.InvariantCultureIgnoreCase);
        }

        public static async Task<HashSet<string>> GetContentTypesAsync(string solrPath)
        {
            return await GetFacetedFieldAsync(solrPath, "contentType_s");
        }

        private static async Task<int> PersistContentTypesAsync(SqlConnection connection, HashSet<string> contentTypes)
        {
            var insertCommand = @"
if not exists (select 1 from Structured.TopicType where TopicTypeId = @topicTypeId)
    insert into Structured.TopicType (TopicTypeId, [Name]) values (@topicTypeId, @topicTypeId);
";
            return await connection.ExecuteAsync(insertCommand, from type in contentTypes select new { topicTypeId = type });
        }

        public static async Task<HashSet<string>> GetAspectsAsync(string solrPath)
        {
            var aspectsFromNewAspectField = GetFacetedFieldAsync(solrPath.Replace("{{facet.field}}", "FHWASPECTS_s"), "FHWASPECTS_s");
            var aspectsFromOldAspectField = GetFacetedFieldAsync(solrPath.Replace("{{facet.field}}", "FHWASPECTID_s"), "FHWASPECTID_s");

            await Task.WhenAll(aspectsFromNewAspectField, aspectsFromOldAspectField);

            var newAspects = (
                from aspectList in aspectsFromNewAspectField.Result
                let parsedAspects = aspectList.Split(',')
                from aspect in parsedAspects
                select aspect.Trim()
            );

            var oldAspects = aspectsFromOldAspectField.Result;

            return new HashSet<string>(newAspects.Concat(oldAspects).Distinct(), StringComparer.OrdinalIgnoreCase);
        }

        private static async Task<int> PersistAspectsAsync(SqlConnection connection, HashSet<string> aspects)
        {
            var insertCommand = @"
if not exists (select 1 from Structured.Aspect where AspectId = @aspectId)
    insert into Structured.Aspect (AspectId, [Description]) values (@aspectId, @aspectId);
";
            return await connection.ExecuteAsync(insertCommand, from aspect in aspects select new { aspectId = aspect });
        }
        public static async Task<HashSet<string>> GetVisibilitiesAsync(string solrPath)
        {
            return await GetFacetedFieldAsync(solrPath, "FHWVISIBILITY_s");
        }

        private static async Task<int> PersistVisibilitiesAsync(SqlConnection connection, HashSet<string> visibilities)
        {
            var insertCommand = @"
if not exists (select 1 from Structured.TopicVisibility where VisibilityId = @visibilityId)
    insert into Structured.TopicVisibility (VisibilityId, [Name]) values (@visibilityId, @visibilityId);
";
            return await connection.ExecuteAsync(insertCommand, from visibility in visibilities select new { visibilityId = visibility });
        }

        public static async Task<HashSet<string>> GetTopicStatusesAsync(string solrPath)
        {
            return await GetFacetedFieldAsync(solrPath, "FSTATUS_s");
        }

        private static async Task<int> PersistTopicStatusesAsync(SqlConnection connection, HashSet<string> topicStatuses)
        {
            var insertCommand = @"
if not exists (select 1 from Structured.TopicStatus where StatusId = @statusId)
    insert into Structured.TopicStatus (StatusId) values (@statusId);
";
            return await connection.ExecuteAsync(insertCommand, from status in topicStatuses select new { statusId = status });
        }

        public static async Task<List<(int, string)>> GetAudiencesAsync(string solrPath)
        {
            var rawAudiences = await GetFacetedFieldAsync(solrPath, "FHWAUDIENCE_s");

            var audiences = new HashSet<string>((
                from rawAudience in rawAudiences
                let audienceCollection = rawAudience.Split(',')
                from audience in audienceCollection
                select audience.Trim()
            ).Distinct(), StringComparer.InvariantCultureIgnoreCase);

            var flagBase = 1;
            return (from audience in audiences let flag = flagBase <<= 1 select (flag, audience)).ToList();
        }

        private static async Task<int> PersistAudiencesAsync(SqlConnection connection, List<(int, string)> audiences)
        {
            var insertCommand = @"
if not exists (select 1 from Structured.Audience where AudienceId = @audienceId)
    insert into Structured.Audience (AudienceId, [Name]) values (@audienceId, @name);
";
            var results = 0;

            foreach (var (audienceId, name) in audiences)
            {
                results += await connection.ExecuteAsync(insertCommand, new { audienceId, name });
            }

            return results;
        }

        public static async Task<HashSet<string>> GetModuleTypesAsync(string solrPath)
        {
            return await GetFacetedFieldAsync(solrPath, "FMODULETYPE_s");
        }

        private static async Task<int> PersistModuleTypesAsync(SqlConnection connection, HashSet<string> moduleTypes)
        {
            var insertCommand = @"
if not exists (select 1 from Structured.ModuleType where ModuleTypeId = @moduleTypeId)
    insert into Structured.ModuleType (ModuleTypeId, [Description]) values (@moduleTypeId, @moduleTypeId);
";
            return await connection.ExecuteAsync(insertCommand, from moduleType in moduleTypes select new { moduleTypeId = moduleType });
        }

        public static async Task<HashSet<string>> GetConceptStatusesAsync(string solrPath)
        {
            return await GetFacetedFieldAsync(solrPath, "status_s");
        }

        private static async Task<int> PersistConceptStatusesAsync(SqlConnection connection, HashSet<string> conceptStatuses)
        {
            var insertCommand = @"
if not exists (select 1 from Structured.ConceptStatus where ConceptStatusId = @conceptStatusId)
    insert into Structured.ConceptStatus (ConceptStatusId) values (@conceptStatusId);
";
            return await connection.ExecuteAsync(insertCommand, from status in conceptStatuses select new { conceptStatusId = status });
        }

        private static Concept ExtractConcept(JToken concept, bool isTopLevel = false)
        {
            var contentBlob = concept["contents_t"].Value<string>();
            var json = JToken.Parse(contentBlob);

            var conceptId = concept["concept_id_s"].Value<string>();

            var taxonomy = json["taxonomy"];
            var alternativeLabels = json["alternativeLabels"];
            var keywords = json["keywords"];
            var codeSystems = json["codeSystems"];
            var nuccSpecialties = json["nucc_specialties"];
            var relationships = json["relationships"];

            var content = json["content"]?.Type == JTokenType.Null ? null : json["content"];
            var topicAspects = content?["topicAspects"];
            var articleAspects = content?["articleAspects"];

            var parentPaths = json["parentPaths"] != null && json["parentPaths"].Type == JTokenType.Array
                ? string.Join(',', (from path in (JArray)json["parentPaths"] select path.Value<string>().Trim()))
                : null;

            return new Concept
            {
                ConceptId = conceptId,
                Label = concept["label_s"].Value<string>(),
                TaxonomyJson = taxonomy?.ToString(),
                AlternativeLabelsJson = alternativeLabels?.ToString(),
                KeywordsJson = keywords?.ToString(),
                CodeSystemsJson = codeSystems?.ToString(),
                NuccSpecialtiesJson = nuccSpecialties?.ToString(),
                RelationshipsJson = relationships?.ToString(),
                TopicAspectsJson = topicAspects?.ToString(),
                ArticleAspectsJson = articleAspects?.ToString(),
                ParentPaths = parentPaths?.ToString(),
                IsSpecialty = false,
                IsTopLevel = isTopLevel,
                IsActive = true,
                ConceptStatusId = concept["status_s"]?.Value<string>() ?? "Active"
            };
        }

        private static async Task<IEnumerable<Concept>> ExtractConcepts(string url, bool isTopLevel = false)
        {
            var concepts = await ExtractFromGetResponseAsync(url, json => json["response"]?["docs"] ?? new JArray());
            return from concept in concepts select ExtractConcept(concept, isTopLevel);
        }

        private static TimeSpan ExponentialBackoffStrategy(int iteration) => TimeSpan.FromSeconds(iteration * 2);

        public static async Task<List<Concept>> GetConceptsAsync(int batchSize, int maxThreads = 5, int batchRetryCount = 4)
        {
            var allConceptsUrl = $"{SolrUrl}/solr/core.1/select?indent=on&q={{!term%20f=taxon_s}}concept&rows=0&wt=json";
            var numberOfConcepts = await ExtractFromGetResponseAsync(allConceptsUrl, json => json["response"]["numFound"].Value<int>());
            var batches = numberOfConcepts / batchSize + (numberOfConcepts % batchSize == 0 ? 0 : 1);

            var executionPolicy = Policy.BulkheadAsync(maxThreads, numberOfConcepts)
                .WrapAsync(Policy.Handle<Exception>().WaitAndRetryAsync(batchRetryCount, ExponentialBackoffStrategy));

            var tasks = (
                from entry in Enumerable.Range(0, batches)
                let skip = entry * batchSize
                let conceptsUrl = $"{SolrUrl}/solr/core.1/select?indent=on&q={{!term%20f=taxon_s}}concept&rows={batchSize}&start={skip}&wt=json"
                select executionPolicy.ExecuteAsync(async () => await ExtractConcepts(conceptsUrl))
            ).ToList();

            var topConceptsUrl = $"{SolrUrl}/solr/core.1/select?indent=on&q={{!term%20f=taxon_s}}topConcept&rows=10000&wt=json";
            tasks.Add(executionPolicy.ExecuteAsync(async () => await ExtractConcepts(topConceptsUrl, true)));

            await Task.WhenAll(tasks);

            return (from t in tasks from concept in t.Result select concept).ToList();
        }

        private static async Task<int> PersistConceptsAsync(SqlConnection connection, List<Concept> concepts)
        {
            var transaction = connection.BeginTransaction();

            var insertCommand = @"
insert into Structured.Concept (ConceptId, Label, TaxonomyJson, AlternativeLabelsJson, KeywordsJson, CodeSystemsJson, NuccSpecialtiesJson, RelationshipsJson, TopicAspectsJson, ArticleAspectsJson, ParentPaths, IsSpecialty, IsTopLevel, IsActive, ConceptStatusId) 
    values (@ConceptId, @Label, @TaxonomyJson, @AlternativeLabelsJson, @KeywordsJson, @CodeSystemsJson, @NuccSpecialtiesJson, @RelationshipsJson, @TopicAspectsJson, @ArticleAspectsJson, @ParentPaths, @IsSpecialty, @IsTopLevel, @IsActive, @ConceptStatusId);
";

            var parameters = (
                from concept in concepts
                select new
                {
                    ConceptId = new DbString { Value = concept.ConceptId, IsFixedLength = false, IsAnsi = true, Length = 16 },
                    Label = new DbString { Value = concept.Label, IsFixedLength = false, IsAnsi = false, Length = -1 },
                    TaxonomyJson = new DbString { Value = concept.TaxonomyJson, IsFixedLength = false, IsAnsi = false, Length = -1 },
                    AlternativeLabelsJson = new DbString { Value = concept.AlternativeLabelsJson, IsFixedLength = false, IsAnsi = false, Length = -1 },
                    KeywordsJson = new DbString { Value = concept.KeywordsJson, IsFixedLength = false, IsAnsi = false, Length = -1 },
                    CodeSystemsJson = new DbString { Value = concept.CodeSystemsJson, IsFixedLength = false, IsAnsi = false, Length = -1 },
                    NuccSpecialtiesJson = new DbString { Value = concept.NuccSpecialtiesJson, IsFixedLength = false, IsAnsi = false, Length = -1 },
                    RelationshipsJson = new DbString { Value = concept.RelationshipsJson, IsFixedLength = false, IsAnsi = false, Length = -1 },
                    TopicAspectsJson = new DbString { Value = concept.TopicAspectsJson, IsFixedLength = false, IsAnsi = false, Length = -1 },
                    ArticleAspectsJson = new DbString { Value = concept.ArticleAspectsJson, IsFixedLength = false, IsAnsi = false, Length = -1 },
                    ParentPaths = new DbString { Value = concept.ParentPaths, IsFixedLength = false, IsAnsi = false, Length = -1 },
                    concept.IsSpecialty,
                    concept.IsTopLevel,
                    concept.IsActive,
                    ConceptStatusId = new DbString { Value = concept.ConceptStatusId, IsFixedLength = false, IsAnsi = true, Length = 20 },
                }
            );

            await connection.ExecuteAsync("alter index UI_Concept_ConceptId on Structured.Concept disable;", transaction: transaction);

            var results = await connection.ExecuteAsync(insertCommand, concepts, transaction);

            await connection.ExecuteAsync("alter index UI_Concept_ConceptId on Structured.Concept rebuild;", transaction: transaction);

            await transaction.CommitAsync();

            return results;
        }

        private static string PopulateRelationshipTypes(string id, string label, Dictionary<string, object> relationshipTypes)
        {
            if (!relationshipTypes.ContainsKey(id))
                relationshipTypes.Add(id, new { RelationshipTypeId = id, Label = label });

            return id;
        }

        private static JToken ValidateReferencedConcept(HashSet<string> validConcepts, List<object> invalidConcepts, string referencingConceptId, JToken relatedConcept)
        {
            var conceptId = relatedConcept["id"].Value<string>();

            if (!validConcepts.Contains(conceptId))
            {
                invalidConcepts.Add(new { InvalidConcept = conceptId, ReferencingConceptId = referencingConceptId });
                return null;
            }

            return relatedConcept;
        }

        private static (List<object>, Dictionary<string, object>, List<object>) GetConceptRelationships(List<Concept> concepts)
        {
            var allConceptRelationships = new List<object>();
            var relationshipTypes = new Dictionary<string, object>();
            var invalidConcepts = new List<object>();
            var validConcepts = new HashSet<string>(concepts.Select(c => c.ConceptId));

            PopulateRelationshipTypes("hasParent", "has parent", relationshipTypes);

            foreach (var concept in concepts)
            {
                var i = 0;
                var parentsSets = $"{concept.ConceptId},{concept.ParentPaths ?? string.Empty}".Split(',', StringSplitOptions.RemoveEmptyEntries);

                allConceptRelationships.AddRange(
                    from entry in parentsSets
                    let parents = entry.Split(' ', StringSplitOptions.RemoveEmptyEntries).Reverse()
                    from parent in parents
                    let level = i++
                    select new
                    {
                        FromConceptId = concept.ConceptId,
                        ToConceptId = parent,
                        RelationshipTypeId = "hasParent",
                        ReferenceType = $"{level:00}",
                        Distance = level
                    }
                );

                if (!string.IsNullOrEmpty(concept.RelationshipsJson))
                {
                    var taxonomy = JToken.Parse(concept.RelationshipsJson);

                    allConceptRelationships.AddRange(
                        from entry in taxonomy
                        let id = entry["id"].Value<string>()
                        let label = entry["label"].Value<string>()
                        let _ = PopulateRelationshipTypes(id, label, relationshipTypes)
                        from relatedConcept in entry["relatedConcepts"] ?? new JArray()
                        let validConcept = ValidateReferencedConcept(validConcepts, invalidConcepts, concept.ConceptId, relatedConcept)
                        where validConcept != null
                        select new
                        {
                            FromConceptId = concept.ConceptId,
                            ToConceptId = validConcept["id"].Value<string>(),
                            RelationshipTypeId = id,
                            ReferenceType = validConcept["reference"]?.Value<string>() ?? "",
                            Distance = -1
                        }
                    );
                }
            }

            return (allConceptRelationships, relationshipTypes, invalidConcepts);
        }

        private static async Task<int> PersistConceptRelationships(SqlConnection connection, List<object> conceptRelationships, Dictionary<string, object> relationshipTypes)
        {
            var insertRelationshipTypeCommand = @"
insert into Structured.RelationshipType(RelationshipTypeId, Label) 
    values (@RelationshipTypeId, @Label);
";

            var insertConceptRelationshipCommand = @"
insert into Structured.ConceptRelationship(FromConceptId, ToConceptId, RelationshipTypeId, ReferenceType, Distance) 
    values (@FromConceptId, @ToConceptId, @RelationshipTypeId, @ReferenceType, @Distance);
";

            var transaction = connection.BeginTransaction();

            var results = await connection.ExecuteAsync(insertRelationshipTypeCommand, relationshipTypes.Values.ToList(), transaction);
            results += await connection.ExecuteAsync(insertConceptRelationshipCommand, conceptRelationships, transaction);

            await transaction.CommitAsync();

            return results;
        }

        private static Topic ExctractTopic(JToken topic, List<(int, string)> audiences, HashSet<string> allowableConcepts, HashSet<string> allowableAspects, List<string> failures)
        {
            try
            {
                var audienceFlagCollection = (
                    from audience in topic["FHWAUDIENCE_s"]?.Value<string>()?.Split(',') ?? new string[] { }
                    select audience.Trim()
                ).ToList();

                int? GetAudienceId(string audience)
                {
                    var matchedAudienceId = (
                        from tuple in audiences
                        let audienceId = tuple.Item1
                        let audienceValue = tuple.Item2
                        where audienceValue == audience
                        select audienceId
                    ).FirstOrDefault();

                    return matchedAudienceId == default
                        ? (int?)null
                        : matchedAudienceId;
                }

                var audienceFlag = (
                    from audience in audienceFlagCollection
                    let audienceId = GetAudienceId(audience)
                    where audienceId != null
                    select audienceId.Value
                ).Aggregate(0, (flags, audienceId) => flags | audienceId);

                var isContentIdAvailable = Guid.TryParse(topic["contentId_s"].Value<string>(), out var contentId);

                var aspects = topic["FHWASPECTS_s"]?.Value<string>()?.Split(',')?.Select(a => a.Trim())?.ToList() ?? new List<string>();
                var concepts = topic["FHWCONCEPTS_s"]?.Value<string>()?.Split(',')?.Select(c => c.Trim())?.Select(c => c.StartsWith("HWCV_", StringComparison.OrdinalIgnoreCase) ? c : $"HWCV_{c}")?.ToList() ?? new List<string>();

                if (topic["FHWCONCEPTID_s"]?.Value<string>() is string conceptId && !concepts.Contains($"HWCV_{conceptId}"))
                    concepts.Add(conceptId.StartsWith("HWCV_", StringComparison.OrdinalIgnoreCase) ? conceptId : $"HWCV_{conceptId}");


                if (topic["FHWASPECTID_s"]?.Value<string>()?.Trim() is string aspectId && !string.IsNullOrEmpty(aspectId) && !aspects.Contains(aspectId))
                    aspects.Add(aspectId);

                if (aspects.Count != concepts.Count)
                {
                    failures.Add($"Topic with id {topic["FHWHWID_s"].Value<string>()} contains an invalid number of Concept/Aspect mappings, Concepts: {concepts.Count}; Aspects: {aspects.Count}.");
                }

                var aspectMappings = new List<(string, string)>();

                for (var i = 0; i < Math.Min(concepts.Count, aspects.Count); i++)
                {
                    var concept = concepts[i];
                    var aspect = aspects[i];

                    var isValidAspect = allowableAspects.Contains(aspect);

                    if (!allowableAspects.Contains(aspect, StringComparer.Ordinal) && isValidAspect)
                    {
                        aspect = allowableAspects.Where(a => string.Equals(a, aspect, StringComparison.OrdinalIgnoreCase)).First();
                    }
                    else if (!isValidAspect)
                    {
                        failures.Add($"Invalid aspect. Topic: {topic["FHWHWID_s"].Value<string>()}. Concept: {concept}, Aspect: {aspect}");
                        continue;
                    }

                    if (!allowableConcepts.Contains(concept))
                    {
                        failures.Add($"Invalid concept. Topic: {topic["FHWHWID_s"].Value<string>()}. Concept: {concept}, Aspect: {aspect}");
                        continue;
                    }

                    aspectMappings.Add((concept, aspect));
                }

                var xml = topic["data_t"]?.Value<string>()?.Replace(@"\""", "\"")?.Replace(@"\r\n", "")?.Replace(@"utf-8", "utf-16", StringComparison.OrdinalIgnoreCase)?.Trim('"');
                XmlDocument xmlDoc = new XmlDocument();

                try
                {
                    xmlDoc.LoadXml(xml);
                }
                catch
                {
                    Console.WriteLine($"Invalid topic: {topic["FHWHWID_s"].Value<string>()}. Xml: [{xml}]");
                    return null;
                }

                var validConcepts = (from concept in concepts.Distinct() where allowableConcepts.Contains(concept) select concept).ToList();

                var result = new Topic
                {
                    TopicId = topic["FHWHWID_s"].Value<string>(),
                    ContentId = isContentIdAvailable ? contentId : Guid.Empty,
                    Title = topic["titleConsumer_t"].Value<string>(),
                    Document = xmlDoc,
                    Localization = topic["language_s"].Value<string>(),
                    TopicTypeId = topic["contentType_s"].Value<string>(),
                    AspectsMappings = aspectMappings,
                    VisibilityId = topic["FHWVISIBILITY_s"]?.Value<string>(),
                    ModuleTypeId = topic["FMODULETYPE_s"].Value<string>(),
                    AudienceFlags = audienceFlag == default ? (int?)null : audienceFlag,
                    ReadingLevel = topic["FHWREADINGLEVELN_s"]?.Value<decimal>() ?? 0.0M
                };

                return result;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"There is something seriously wrong with this entry: {topic}", e);
            }
        }

        private static async Task<IEnumerable<Topic>> ExtractTopics(string url, List<(int, string)> audiences, HashSet<string> concepts, HashSet<string> aspects, List<string> failures)
        {
            var jsonTopics = await ExtractFromGetResponseAsync(url, json => json["response"]?["docs"] ?? new JArray());

            return (
                from topic in jsonTopics
                let materializedTopic = ExctractTopic(topic, audiences, concepts, aspects, failures)
                where materializedTopic != null
                select materializedTopic
            ).ToList();
        }

        public static async Task<List<Topic>> GetTopicsAsync(List<(int, string)> audiences, List<Concept> concepts, HashSet<string> aspects, int batchSize, int maxThreads = 5, int batchRetryCount = 4, int maxRecordCount = int.MaxValue)
        {
            var allTopicsUrl = $"{SolrUrl}/solr/core.1/select?indent=on&q={{!term%20f=taxon_s}}DITAdoc&rows=0&wt=json";
            var numberOfTopics = Math.Min(await ExtractFromGetResponseAsync(allTopicsUrl, json => json["response"]["numFound"].Value<int>()), maxRecordCount);
            var batches = numberOfTopics / batchSize + (numberOfTopics % batchSize == 0 ? 0 : 1);
            var conceptDictionary = new HashSet<string>(from concept in concepts select concept.ConceptId, StringComparer.OrdinalIgnoreCase);
            var failures = new List<string>();

            batchSize = Math.Min(batchSize, maxRecordCount);

            var executionPolicy = Policy.BulkheadAsync(maxThreads, numberOfTopics)
                .WrapAsync(Policy.Handle<Exception>().WaitAndRetryAsync(batchRetryCount, ExponentialBackoffStrategy));

            var topicFieldList = "FHWHWID_s,contentId_s,titleConsumer_t,data_t,language_s,contentType_s,FHWCONCEPTID_s,FHWCONCEPTS_s,FHWASPECTID_s,FHWASPECTS_s,FHWCONCEPTID_s,FHWVISIBILITY_s,FMODULETYPE_s,FHWAUDIENCE_s,FHWREADINGLEVELN_s";

            var tasks = (
                from entry in Enumerable.Range(0, batches)
                let skip = entry * batchSize
                let conceptsUrl = $"{SolrUrl}/solr/core.1/select?fl={topicFieldList}&indent=on&q={{!term%20f=taxon_s}}DITAdoc&rows={batchSize}&start={skip}&wt=json"
                select executionPolicy.ExecuteAsync(async () => await ExtractTopics(conceptsUrl, audiences, conceptDictionary, aspects, failures))
            ).ToList();

            await Task.WhenAll(tasks);
            await File.WriteAllLinesAsync("MissingConceptAspectMappings.txt", failures);

            return (from t in tasks from topic in t.Result select topic).ToList();
        }

        private static async Task<int> PersistTopics(SqlConnection connection, List<Topic> topics, List<Concept> concepts, int batchSize = 1000)
        {
            var insertTopicCommand = @"
insert into Structured.Topic(TopicId, ContentId, Title, Document, Localization, TopicTypeId, VisibilityId, ModuleTypeId, AudienceFlags, ReadingLevel) 
    values (@TopicId, @ContentId, @Title, @Document, @Localization, @TopicTypeId, @VisibilityId, @ModuleTypeId, @AudienceFlags, @ReadingLevel);
";

            var insertTopicAspectMapping = @"
insert into Structured.TopicAspectMapping(TopicId, Localization, AspectId, ConceptId)
    values (@TopicId, @Localization, @AspectId, @ConceptId);
";

            var aspectMappings = (from topic in topics from aspectPair in topic.AspectsMappings select new { topic.TopicId, topic.Localization, AspectId = aspectPair.Item2, ConceptId = aspectPair.Item1 }).Distinct();
            var transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted);

            var dapperTopics = topics.Select((topic, index) => new
            {
                Index = index,
                Topic = new
                {
                    TopicId = new DbString { Value = topic.TopicId, IsFixedLength = false, IsAnsi = true, Length = 40 },
                    topic.ContentId,
                    Title = new DbString { Value = topic.Title, IsFixedLength = false, IsAnsi = false, Length = -1 },
                    Document = topic.Document,
                    Localization = new DbString { Value = topic.Localization, IsFixedLength = false, IsAnsi = true, Length = 6 },
                    TopicTypeId = new DbString { Value = topic.TopicTypeId, IsFixedLength = false, IsAnsi = true, Length = 30 },
                    VisibilityId = new DbString { Value = topic.VisibilityId, IsFixedLength = false, IsAnsi = true, Length = 12 },
                    ModuleTypeId = new DbString { Value = topic.ModuleTypeId, IsFixedLength = false, IsAnsi = true, Length = 30 },
                    topic.AudienceFlags,
                    topic.ReadingLevel
                }
            });

            var topicBatches = (
                from topic in dapperTopics
                group topic.Topic by topic.Index / batchSize into g
                select g.ToList()
            );

            var results = 0;

            await connection.ExecuteAsync("alter index UI_Topic_TopicId on Structured.Topic disable;", transaction: transaction);

            foreach (var topicBatch in topicBatches)
            {
                try
                {
                    results += await connection.ExecuteAsync(insertTopicCommand, topicBatch, transaction: transaction);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"There is something wrong with this batch: {e}");
                }
            }

            await connection.ExecuteAsync("alter index UI_Topic_TopicId on Structured.Topic rebuild;", transaction: transaction);
            await connection.ExecuteAsync("alter index UI_TopicAspectMapping on Structured.TopicAspectMapping disable;", transaction: transaction);

            results += await connection.ExecuteAsync(insertTopicAspectMapping, aspectMappings, transaction: transaction);

            await connection.ExecuteAsync("alter index UI_TopicAspectMapping on Structured.TopicAspectMapping rebuild;", transaction: transaction);
            await transaction.CommitAsync();

            return results;
        }

        static async Task Main(string[] args)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var connectionString = "Server=.;Database=Content;Trusted_Connection=True;";
            using var connection = new SqlConnection(connectionString);

            connection.Open();

            var contentTypes = await GetContentTypesAsync("solr/core.1/select?facet.field=contentType_s&facet=on&indent=on&q={!term%20f=taxon_s}DITAdoc&rows=0&wt=json");
            //var insertedContentTypes = await PersistContentTypesAsync(connection, contentTypes);

            var aspects = await GetAspectsAsync("solr/core.1/select?facet.field={{facet.field}}&facet=on&indent=on&q={!term%20f=taxon_s}DITAdoc&rows=0&wt=json");
            var insertedAspects = await PersistAspectsAsync(connection, aspects);

            var visibilities = await GetVisibilitiesAsync("solr/core.1/select?facet.field=FHWVISIBILITY_s&facet=on&indent=on&q={!term%20f=taxon_s}DITAdoc&rows=0&wt=json");
            //var insertedVisibilities = await PersistVisibilitiesAsync(connection, visibilities);

            var topicStatuses = await GetTopicStatusesAsync("solr/core.1/select?facet.field=FSTATUS_s&facet=on&indent=on&q={!term%20f=taxon_s}DITAdoc&rows=0&wt=json");
            //var insertedStatuses = await PersistTopicStatusesAsync(connection, topicStatuses);

            var audiences = await GetAudiencesAsync("solr/core.1/select?facet.field=FHWAUDIENCE_s&facet=on&indent=on&q={!term%20f=taxon_s}DITAdoc&rows=0&wt=json");
            //var insertAudiences = await PersistAudiencesAsync(connection, audiences);

            var moduleTypes = await GetModuleTypesAsync("solr/core.1/select?facet.field=FMODULETYPE_s&facet=on&indent=on&q={!term%20f=taxon_s}DITAdoc&rows=0&wt=json");
            //var insertedModuleTypes = await PersistModuleTypesAsync(connection, moduleTypes);

            var conceptStatuses = await GetConceptStatusesAsync("solr/core.1/select?facet.field=status_s&facet=on&indent=on&q={!term%20f=taxon_s}concept&rows=0&wt=json");
            //var insertedConceptStatuses = await PersistConceptStatusesAsync(connection, conceptStatuses);

            var concepts = await GetConceptsAsync(300, 6);
            //var insertedConcepts = await PersistConceptsAsync(connection, concepts);

            var (conceptRelationships, relationshipTypes, invalidConcepts) = GetConceptRelationships(concepts);

            //foreach (var invalidConcept in (from dynamic c in invalidConcepts group c.InvalidConcept by c.ReferencingConceptId into g select new { Concept = g.Key, InvalidConcepts = string.Join(", ", g) }))
            //    File.AppendAllText("InvalidConcepts.txt", $"{invalidConcept}\n");

            //var insertedConceptRelationships = await PersistConceptRelationships(connection, conceptRelationships, relationshipTypes);

            var topics = await GetTopicsAsync(audiences, concepts, aspects, 300, 8);
            //var insertedTopics = await PersistTopics(connection, topics, concepts);

            stopWatch.Stop();

            Console.WriteLine($"Elapsed: {stopWatch.Elapsed}");
        }
    }
}
