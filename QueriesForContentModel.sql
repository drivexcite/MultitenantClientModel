-- How many topics per Localization
select Localization, count(TopicId) as [Topics]
from Structured.Topic
group by Localization
order by count(TopicId) desc

-- How many topics per Topic Type?
select TopicTypeId, count(TopicId) as [Topics]
from Structured.Topic
group by TopicTypeId
order by count(TopicId) desc

-- How many topics per Aspect?
select m.AspectId, count(t.TopicId) as [Topics]
from Structured.Topic t
	inner join Structured.TopicAspectMapping m on t.TopicId = m.TopicId and t.Localization = m.Localization
group by m.AspectId
order by count(t.TopicId) desc

-- How many topics per Topic Type and Localization?
select 
	t.TopicTypeId, 
	t.Localization,
	count(TopicId) as [Topics],
	row_number() over(partition by t.TopicTypeId order by count(TopicId) desc) as [LocalRank]
from Structured.Topic t
group by t.TopicTypeId, t.Localization

-- How many topics per Aspect and Localization?
select 
	m.AspectId, 
	t.Localization,
	count(t.TopicId) as [Topics],
	row_number() over(partition by m.AspectId order by count(t.TopicId) desc) as [LocalRank]
from Structured.Topic t
	inner join Structured.TopicAspectMapping m on t.TopicId = m.TopicId and t.Localization = m.Localization
group by m.AspectId, t.Localization

-- What are all the Topics in the intersection of a given Concept and Aspect?
select 
	t.TopicId,
	t.Localization,
	t.Title
from Structured.Topic t
	inner join Structured.TopicAspectMapping m
		on t.TopicId = m.TopicId and t.Localization = m.Localization
where m.ConceptId = 'HWCV_23659'
	and m.AspectId = 'howDone'

-- What are the Top 10 tuples of Concepts, Aspects and Localization with the largest number of Topics?
select top 10
	m.ConceptId,
	c.Label as Concept,
	m.AspectId,
	t.Localization,
	count(t.TopicId) as [Topics]
from Structured.Topic t
	inner join Structured.TopicAspectMapping m
		on t.TopicId = m.TopicId and t.Localization = m.Localization
	inner join Structured.Concept c
		on m.ConceptId = c.ConceptId
where m.AspectId <> 'TBD01'
group by m.ConceptId, c.Label, m.AspectId, t.Localization
order by count(t.TopicId) desc

-- And from the Top 10 Tuples of Concepts, Aspect and Localization, what exactly are the Topics related to the Top 1
select 
	t.TopicId,
	t.Localization,
	t.Title
from Structured.Topic t
	inner join Structured.TopicAspectMapping m
		on t.TopicId = m.TopicId and t.Localization = m.Localization
where m.ConceptId = 'HWCV_22321'
	and m.AspectId = 'howDone'
	and t.Localization = 'en-us'

-- And from the Top 10 Medical Codes per Code System with the most topics
select * from
(
	select
		cm.CodeSystemId,
		cm.Code,
		count(t.TopicSurrogateId) as Topics,
		rank() over(partition by cm.CodeSystemId order by count(t.TopicSurrogateId) desc) as LocalRank
	from Structured.Topic t
		inner join Structured.TopicAspectMapping tm
			on t.TopicId = tm.TopicId and t.Localization = tm.Localization
		inner join Structured.Concept c
			on tm.ConceptId = c.ConceptId
		inner join Structured.MedicalCodeMapping cm
			on c.ConceptId = cm.ConceptId
	group by cm.CodeSystemId, cm.Code	
) t
where LocalRank <= 10
order by CodeSystemId, LocalRank asc


-- Concepts with exactly one children
select
	c2.ConceptId as ParentConceptId, 
	c2.Label as ParentConcept,	
	count(c1.ConceptId) as Children
from Structured.Concept c1
	inner join Structured.ConceptRelationship r on c1.ConceptId = r.FromConceptId
	inner join Structured.Concept c2 on r.ToConceptId = c2.ConceptId
where r.RelationshipTypeId = 'hasParent'
	and r.Distance <> 0
group by c2.ConceptId, c2.Label
having count(c1.ConceptId) = 1

-- Show the hierarchy of a Concept
select 
	c.ConceptId,
	c.Label
from Structured.ConceptRelationship r
	inner join Structured.Concept c
		on r.FromConceptId = c.ConceptId
where r.RelationshipTypeId = 'hasParent'
	and r.Distance > 0
	and r.ToConceptId = 'HWCV_04102'

-- Concepts most children
select
	c2.ConceptId as ParentConceptId, 
	c2.Label as ParentConcept,	
	count(c1.ConceptId) as Children
from Structured.Concept c1
	inner join Structured.ConceptRelationship r on c1.ConceptId = r.FromConceptId
	inner join Structured.Concept c2 on r.ToConceptId = c2.ConceptId
where r.RelationshipTypeId = 'hasParent'
	and r.Distance <> 0
group by c2.ConceptId, c2.Label
order by count(c1.ConceptId) desc

-- Show the direct children of a top level concept
select 
	c.ConceptId,
	c.Label
from Structured.ConceptRelationship r
	inner join Structured.Concept c
		on r.FromConceptId = c.ConceptId
where r.RelationshipTypeId = 'hasParent'
	and r.Distance = 1
	and r.ToConceptId = 'HWCV_10000'

-- How does a Topic shaped as a document look like?
select top 1
	t.TopicId,
	t.ContentId,
	t.Title,
	t.Localization,
	t.TopicTypeId as TopicType,
	t.VisibilityId as Visibility,
	t.ModuleTypeId as ModuleType,
	(
		select 
			a.AspectId as [Aspect.Id],
			c.ConceptId as [Concept.Id],
			c.Label as [Concept.Description],
			json_query(
				case when isjson(c.KeywordsJson) > 0 then c.KeywordsJson else '{}' end, 
				'lax $.' + upper(substring(t.Localization, 0, 3)) + '.R1'
			) as [Concept.Keywords],
			json_query(
				case when isjson(c.CodeSystemsJson) > 0 then c.CodeSystemsJson else '{}' end, 
				'lax $'
			) as [Concept.CodeSystems],
			(
				select
					toConcept.ConceptId as [Parent.ConceptId],
					toConcept.Label as [Parent.Description],
					r.Distance as [Parent.LevelsAbove]
				from Structured.ConceptRelationship r 
					inner join Structured.Concept toConcept 
						on r.ToConceptId = toConcept.ConceptId
				where r.FromConceptId = c.ConceptId		
					and r.RelationshipTypeId = 'hasParent'
					and r.Distance > 0	
				order by r.Distance asc
				for json path
			) as Parents			
		from Structured.TopicAspectMapping m
			inner join Structured.Aspect a
				on m.AspectId = a.AspectId
			inner join Structured.Concept c
				on m.ConceptId = c.ConceptId			
		where m.TopicId = t.TopicId 
			and m.Localization = t.Localization			
		for json path
	) as [Mappings]
from Structured.Topic t
where t.Localization = 'en-us'
	and t.VisibilityId = 'Standalone'	
for json path, root('Topics')