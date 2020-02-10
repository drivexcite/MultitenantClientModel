if not exists (select 1 from sys.schemas where [name] = 'Structured')
	 exec('CREATE SCHEMA [Structured]');
go

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'TopicType' and TABLE_SCHEMA = 'Structured')
	create table Structured.TopicType
	(
		TopicTypeId varchar(30) not null,
		[Name] varchar(40) not null,
		CreatedDate datetime2(2) not null constraint DF_TopicType_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_TopicType_CreatedBy default system_user,
		constraint PK_TopicType primary key clustered(TopicTypeId),
		constraint UQ_TopicType_Name unique ([Name])
	);
go

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'Aspect' and TABLE_SCHEMA = 'Structured')
	create table Structured.Aspect
	(
		AspectId varchar(30) not null,
		[Description] varchar(100) not null,			
		CreatedDate datetime2(2) not null constraint DF_Aspect_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_Aspect_CreatedBy default system_user,
		constraint PK_Aspect primary key clustered(AspectId),
		constraint UQ_Aspect_Name unique (AspectId)
	);
go

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'ConceptStatus' and TABLE_SCHEMA = 'Structured')
	create table Structured.ConceptStatus
	(
		ConceptStatusId varchar(20) not null,
		CreatedDate datetime2(2) not null constraint DF_ConceptStatus_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_ConceptStatus_CreatedBy default system_user,
		constraint PK_ConceptStatus primary key clustered(ConceptStatusId)
	);
go

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'TopicVisibility' and TABLE_SCHEMA = 'Structured')
	create table Structured.TopicVisibility
	(
		VisibilityId varchar(12) not null,
		[Name] varchar(40) not null,
		CreatedDate datetime2(2) not null constraint DF_TopicVisibility_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_TopicVisibility_CreatedBy default system_user,
		constraint PK_TopicVisibility primary key clustered(VisibilityId)
	);
go

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'TopicStatus' and TABLE_SCHEMA = 'Structured')
begin
	create table Structured.TopicStatus
	(
		StatusId varchar(12) not null,
		CreatedDate datetime2(2) not null constraint DF_TopicStatus_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_TopicStatus_CreatedBy default system_user,
		constraint PK_TopicStatus primary key clustered(StatusId)
	);
end
go

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'Audience' and TABLE_SCHEMA = 'Structured')
begin
	create table Structured.Audience
	(
		AudienceId int not null,
		[Name] varchar(40) not null,
		CreatedDate datetime2(2) not null constraint DF_Audience_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_Audience_CreatedBy default system_user,
		constraint PK_Audience primary key clustered(AudienceId)
	);
end
go

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'ModuleType' and TABLE_SCHEMA = 'Structured')
	create table Structured.ModuleType
	(
		ModuleTypeId varchar(30) not null,
		[Description] varchar(100) not null,
		CreatedDate datetime2(2) not null constraint DF_ModuleType_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_ModuleType_CreatedBy default system_user,
		constraint PK_ModuleType primary key clustered(ModuleTypeId)
	);
go

-- Transactional tables
if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'RelationshipType' and TABLE_SCHEMA = 'Structured')
	create table Structured.RelationshipType
	(
		RelationshipTypeId varchar(60) not null,
		[Label] varchar(100) not null,		
		CreatedDate datetime2(2) not null constraint DF_RelationshipType_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_RelationshipType_CreatedBy default system_user,
		constraint PK_RelationshipType primary key clustered(RelationshipTypeId)
	);
go

if not exists (select 1 from sys.sequences where [name] = 'ConceptSurrogateId')
begin
	create sequence Structured.ConceptSurrogateId as int start with 1 increment by 1 no cycle;
end

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'Concept' and TABLE_SCHEMA = 'Structured')
	create table Structured.Concept
	(
		ConceptSurrogateId int not null constraint DF_ConceptSurrogateId default next value for Structured.ConceptSurrogateId,
		ConceptId varchar(40) not null,
		Label nvarchar(max) not null,			
		TaxonomyJson nvarchar(max),
		AlternativeLabelsJson nvarchar(max),
		KeywordsJson nvarchar(max),
		CodeSystemsJson nvarchar(max),
		NuccSpecialtiesJson nvarchar(max),		
		RelationshipsJson nvarchar(max),		
		TopicAspectsJson nvarchar(max),					
		ArticleAspectsJson nvarchar(max),
		ParentPaths nvarchar(max),					
		IsSpecialty bit not null constraint DF_Concept_IsSpecialty default 1,
		IsTopLevel bit not null constraint DF_Concept_IsTopLevel default 0,
		IsActive bit not null constraint DF_Concept_IsActive default 1,
		ConceptStatusId varchar(20) not null,
		CreatedDate datetime2(2) not null constraint DF_Concept_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_Concept_CreatedBy default system_user,
		constraint PK_Concept primary key clustered(ConceptSurrogateId),
		constraint FK_Concept_ConceptStatus foreign key (ConceptStatusId) references Structured.ConceptStatus(ConceptStatusId)
	);

	create unique nonclustered index UI_Concept_ConceptId on Structured.Concept(ConceptId);
go

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'ConceptRelationship' and TABLE_SCHEMA = 'Structured')
	create table Structured.ConceptRelationship
	(
		FromConceptId varchar(40) not null,
		ToConceptId varchar(40) not null,
		RelationshipTypeId varchar(60) not null,
		ReferenceType varchar(30) not null,
		Distance smallint not null constraint DF_ConceptRelationship_Depth default -1,
		CreatedDate datetime2(2) not null constraint DF_ConceptRelationship_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_ConceptRelationship_CreatedBy default system_user,
		constraint PK_ConceptRelationship primary key clustered(FromConceptId, ToConceptId, RelationshipTypeId, ReferenceType),
		constraint FK_ConceptRelationship_FromConcept foreign key (FromConceptId) references Structured.Concept(ConceptId),
		constraint FK_ConceptRelationship_ToConcept foreign key (ToConceptId) references Structured.Concept(ConceptId),
		constraint FK_ConceptRelationship_RelationshipType foreign key (RelationshipTypeId) references Structured.RelationshipType(RelationshipTypeId)
	);

	create nonclustered index IX_ConceptRelationship_ToConceptId on Structured.ConceptRelationship(ToConceptId);
go

if not exists (select 1 from sys.sequences where [name] = 'TopicSurrogateId')
begin
	create sequence Structured.TopicSurrogateId as int start with 1 increment by 1 no cycle;
end

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'Topic' and TABLE_SCHEMA = 'Structured')
begin
	create table Structured.Topic 
	(
		TopicSurrogateId int not null constraint DF_Topic_TopicSurrogateId default next value for Structured.TopicSurrogateId,
		TopicId varchar(40) not null,
		ContentId UNIQUEIDENTIFIER not null,
		Title nvarchar(max) not null,
		Document xml not null,
		Localization varchar(6) not null,
		TopicTypeId varchar(30) not null,		
		VisibilityId varchar(12),
		ModuleTypeId varchar(30) not null,
		AudienceFlags int,
		ReadingLevel decimal(4,1),
		CreatedDate datetime2(2) not null constraint DF_Topic_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_Topic_CreatedBy default system_user,
		constraint PK_Topic primary key clustered(TopicSurrogateId),
		constraint FK_Topic_TopicType foreign key(TopicTypeId) references Structured.TopicType(TopicTypeId),
		constraint FK_Topic_Visibility foreign key(VisibilityId) references Structured.TopicVisibility(VisibilityId),
		constraint FK_Topic_ModuleType foreign key(ModuleTypeId) references Structured.ModuleType(ModuleTypeId)
	);	

	create unique nonclustered index UI_Topic_TopicId on Structured.Topic(TopicId, Localization);
	create nonclustered index IX_Topic_ContentId on Structured.Topic(ContentId);
	create nonclustered index IX_Topic_TopicTypeId on Structured.Topic(TopicTypeId);
	create nonclustered index IX_Topic_VisibilityId on Structured.Topic(VisibilityId);
	create nonclustered index IX_Topic_ModuleTypeId on Structured.Topic(ModuleTypeId);
end
go

if not exists (select 1 from sys.sequences where [name] = 'MappingId')
begin
	create sequence Structured.MappingId as int start with 1 increment by 1 no cycle;
end

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'TopicAspectMapping' and TABLE_SCHEMA = 'Structured')
begin
	create table Structured.TopicAspectMapping 
	(
		MappingId int not null constraint DF_TopicAspectMapping_MappingId default next value for Structured.MappingId,
		TopicId varchar(40) not null,
		Localization varchar(6) not null,
		AspectId varchar(30) not null,
		ConceptId varchar(40) not null,
		CreatedDate datetime2(2) not null constraint DF_TopicAspectMapping_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_TopicAspectMapping_CreatedBy default system_user,
		constraint PK_TopicAspectMapping primary key clustered(MappingId),		
		constraint FK_TopicAspectMapping_Topic foreign key(TopicId, Localization) references Structured.Topic(TopicId, Localization),
		constraint FK_TopicAspectMapping_Aspect foreign key(AspectId) references Structured.Aspect(AspectId),
		constraint FK_TopicAspectMapping_Concept foreign key(ConceptId) references Structured.Concept(ConceptId)	
	);

	create unique nonclustered index UI_TopicAspectMapping on Structured.TopicAspectMapping (TopicId, Localization, ConceptId, AspectId);
end
go

-- Drop transactional tables;
drop table Structured.ConceptRelationship;
drop table Structured.RelationshipType;
drop table Structured.TopicAspectMapping;
drop table Structured.Concept;
drop table Structured.Topic;

drop sequence Structured.ConceptSurrogateId;
drop sequence Structured.TopicSurrogateId;
drop sequence Structured.MappingId;