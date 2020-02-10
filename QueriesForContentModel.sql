select Localization, count(TopicId) as [Count]
from Structured.Topic
group by Localization
order by count(TopicId) desc

select TopicTypeId, count(TopicId)
from Structured.Topic
group by TopicTypeId
order by count(TopicId) desc

select * 
from Structured.Aspect

select 
	RelationshipsJson
from Structured.Concept
where ConceptId = 'HWCV_04025'

select 
	ConceptId, len(RelationshipsJson), RelationshipsJson
from Structured.Concept
order by len(RelationshipsJson) desc

select m.AspectId, count(t.TopicId)
from Structured.Topic t
	inner join Structured.TopicAspectMapping m on t.TopicId = m.TopicId and t.Localization = m.Localization
group by m.AspectId
order by count(t.TopicId) desc

select sum(t.Count) as Concepts
from
(
	select 
		m.ConceptId, 
		c.Label, 
		count(t.TopicId) as [Count]
	from Structured.Topic t
		inner join Structured.TopicConceptMapping m on t.TopicId = m.TopicId and t.Localization = m.Localization
		inner join Structured.Concept c on m.ConceptId = c.ConceptId
	group by m.ConceptId, c.Label
	having count(t.TopicId) = 1	
) t

select * from Structured.Concept where ConceptId = 'HWCV_23659'

select *
from Structured.Topic t
	inner join Structured.TopicConceptMapping cm on t.TopicId = cm.TopicId and t.Localization = cm.Localization
	inner join Structured.TopicAspectMapping am on t.TopicId = am.TopicId and t.Localization = am.Localization
where cm.ConceptId = 'HWCV_23659'
	and am.AspectId = 'howDone'
	

select top 100
	t.TopicId,
	cm.ConceptId,
	am.AspectId
from Structured.Topic t
	inner join Structured.TopicConceptMapping cm on t.TopicId = cm.TopicId and t.Localization = cm.Localization
	inner join Structured.TopicAspectMapping am on t.TopicId = am.TopicId and t.Localization = am.Localization

select * from Structured.Concept where IsTopLevel = 1

select
	t.TopicId, 
	t.Localization, 
	count(cm.ConceptId) as Concepts, 
	count(am.AspectId) as Aspects
from Structured.Topic t
	inner join Structured.TopicConceptMapping cm on t.TopicId = cm.TopicId and t.Localization = cm.Localization
	inner join Structured.TopicAspectMapping am on t.TopicId = am.TopicId and t.Localization = am.Localization
group by t.TopicId,	t.Localization
having count(cm.ConceptId) > 1 or count(am.AspectId) > 1

select c1.ConceptId, c1.Label, r.RelationshipTypeId, r.ReferenceType, c2.ConceptId, c2.Label
from Structured.Concept c1
	inner join Structured.ConceptRelationship r on c1.ConceptId = r.FromConceptId
	inner join Structured.Concept c2 on r.ToConceptId = c2.ConceptId
where c2.ConceptId = 'HWCV_10000'
	and r.ReferenceType = 'level1'

-- Concepts with more total children
select
	c2.ConceptId as ParentConceptId, 
	c2.Label as ParentConcept,	
	r.Distance,
	count(c1.ConceptId) as Children
from Structured.Concept c1
	inner join Structured.ConceptRelationship r on c1.ConceptId = r.FromConceptId
	inner join Structured.Concept c2 on r.ToConceptId = c2.ConceptId
where r.RelationshipTypeId = 'hasParent'
group by c2.ConceptId, c2.Label, r.Distance
order by count(c1.ConceptId) desc, c2.ConceptId, r.Distance


