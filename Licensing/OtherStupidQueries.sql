select * from Structured.ConceptRelationship r
where r.ToConceptId = 'HWCV_10000'
order by Distance desc

select * from Structured.Concept c
where c.IsTopLevel = 1

select * from Structured.ConceptRelationship r
where r.ToConceptId = 'HWCV_10009'
order by Distance desc

select * from Structured.Concept where ConceptId = 'HWCV_10009'
select * from Structured.Concept where ConceptId = 'HWCV_04736'