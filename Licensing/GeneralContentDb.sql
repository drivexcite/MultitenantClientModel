if not exists (select 1 from sys.schemas where [name] = 'General')
	 exec('CREATE SCHEMA [General]');
go

-- ##########
declare @deleteAll bit = 0;
if(@deleteAll = 1)
begin
	drop table General.Asset;
	drop sequence General.AssetId;

	drop table General.AssetCategoryMetadataAssignment;

	drop table General.MetadataCategory;
	drop sequence General.MetadataCategoryId;

	drop table General.MetadataType;
	drop sequence General.MetadataTypeId;

	drop table General.AssetCategory;
	drop sequence General.AssetCategoryId;

	drop table General.AssetType;
	drop sequence General.AssetTypeId;
end
go

-- ##########

if not exists (select 1 from sys.sequences where [name] = 'AssetTypeId')
begin
	create sequence General.AssetTypeId as int start with 1 increment by 1 no cycle;
end

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'AssetType' and TABLE_SCHEMA = 'General')
	create table General.AssetType
	(
		AssetTypeId int not null constraint DF_AssetType_AssetTypeId default next value for General.AssetTypeId,
		[Name] varchar(120) not null,
		CreatedDate datetime2(2) not null constraint DF_AssetType_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_AssetType_CreatedBy default system_user,
		constraint PK_AssetType primary key clustered(AssetTypeId),
		constraint UQ_AssetType_Name unique ([Name])
	);
go

if not exists (select 1 from General.AssetType where [Name] = 'Atom')
	insert into General.AssetType([Name]) values ('Atom');

if not exists (select 1 from General.AssetType where [Name] = 'Assembly')
	insert into General.AssetType([Name]) values ('Assembly');

if not exists (select 1 from General.AssetType where [Name] = 'Collection')
	insert into General.AssetType([Name]) values ('Collection');

-- ##########

if not exists (select 1 from sys.sequences where [name] = 'AssetCategoryId')
begin
	create sequence General.AssetCategoryId as int start with 1 increment by 1 no cycle;
end

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'AssetCategory' and TABLE_SCHEMA = 'General')
	create table General.AssetCategory
	(
		AssetCategoryId int not null constraint DF_AssetCategory_AssetCategoryId default next value for General.AssetCategoryId,
		AssetTypeId int not null,
		[Name] varchar(120) not null,
		CreatedDate datetime2(2) not null constraint DF_AssetCategory_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_AssetCategory_CreatedBy default system_user,
		constraint PK_AssetCategory primary key clustered(AssetCategoryId),
		constraint UQ_AssetCategory_Name unique ([Name]),
		constraint FK_AssetCategory_AssetType foreign key (AssetTypeId) references General.AssetType(AssetTypeId)
	);
go

if not exists (select 1 from General.AssetCategory where [Name] = 'Concept')
begin
	declare @typeId int;
	select @typeId = AssetTypeId from General.AssetType where [Name] = 'Atom';

	insert into General.AssetCategory([Name], AssetTypeId) values ('Concept', @typeId);
end
go

if not exists (select 1 from General.AssetCategory where [Name] = 'Topic')
begin
	declare @typeId int;
	select @typeId = AssetTypeId from General.AssetType where [Name] = 'Atom';

	insert into General.AssetCategory([Name], AssetTypeId) values ('Topic', @typeId);
end
go

if not exists (select 1 from General.AssetCategory where [Name] = 'Image')
begin
	declare @typeId int;
	select @typeId = AssetTypeId from General.AssetType where [Name] = 'Atom';

	insert into General.AssetCategory([Name], AssetTypeId) values ('Image', @typeId);
end
go

if not exists (select 1 from General.AssetCategory where [Name] = 'Video')
begin
	declare @typeId int;
	select @typeId = AssetTypeId from General.AssetType where [Name] = 'Atom';

	insert into General.AssetCategory([Name], AssetTypeId) values ('Video', @typeId);
end
go 

if not exists (select 1 from General.AssetCategory where [Name] = 'Module')
begin
	declare @typeId int;
	select @typeId = AssetTypeId from General.AssetType where [Name] = 'Assembly';

	insert into General.AssetCategory([Name], AssetTypeId) values ('Module', @typeId);
end
go

if not exists (select 1 from General.AssetCategory where [Name] = 'Patient Instruction')
begin
	declare @typeId int;
	select @typeId = AssetTypeId from General.AssetType where [Name] = 'Assembly';

	insert into General.AssetCategory([Name], AssetTypeId) values ('Patient Instruction', @typeId);
end
go

if not exists (select 1 from General.AssetCategory where [Name] = 'Program')
begin
	declare @typeId int;
	select @typeId = AssetTypeId from General.AssetType where [Name] = 'Collection';

	insert into General.AssetCategory([Name], AssetTypeId) values ('Program', @typeId);
end
go

-- ##########
if not exists (select 1 from sys.sequences where [name] = 'MetadataTypeId')
begin
	create sequence General.MetadataTypeId as int start with 1 increment by 1 no cycle;
end

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'MetadataType' and TABLE_SCHEMA = 'General')
	create table General.MetadataType
	(
		MetadataTypeId int not null constraint DF_MetadataType_MetadataTypeId default next value for General.MetadataTypeId,
		[Name] varchar(120) not null,	
		CreatedDate datetime2(2) not null constraint DF_MetadataType_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_MetadataType_CreatedBy default system_user,
		constraint PK_MetadataType primary key clustered(MetadataTypeId),
		constraint UQ_MetadataType_Name unique ([Name])
	);
go

if not exists (select 1 from General.MetadataType where [Name] = 'Taxonomy')
	insert into General.MetadataType([Name]) values ('Taxonomy');

if not exists (select 1 from General.MetadataType where [Name] = 'Ontology')
	insert into General.MetadataType([Name]) values ('Ontology');

if not exists (select 1 from General.MetadataType where [Name] = 'Code System')
	insert into General.MetadataType([Name]) values ('Code System');

if not exists (select 1 from General.MetadataType where [Name] = 'Category')
	insert into General.MetadataType([Name]) values ('Category');

if not exists (select 1 from General.MetadataType where [Name] = 'Tag')
	insert into General.MetadataType([Name]) values ('Tag');

if not exists (select 1 from General.MetadataType where [Name] = 'Metric')
	insert into General.MetadataType([Name]) values ('Metric');

if not exists (select 1 from General.MetadataType where [Name] = 'Range')
	insert into General.MetadataType([Name]) values ('Range');

-- ##########
if not exists (select 1 from sys.sequences where [name] = 'MetadataCategoryId')
begin
	create sequence General.MetadataCategoryId as int start with 1 increment by 1 no cycle;
end

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'MetadataCategory' and TABLE_SCHEMA = 'General')
	create table General.MetadataCategory
	(
		MetadataCategoryId int not null constraint DF_MetadataCategory_MetadataCategoryId default next value for General.MetadataTypeId,
		MetadataTypeId int not null,
		[Name] varchar(120) not null,
		CreatedDate datetime2(2) not null constraint DF_MetadataCategory_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_MetadataCategory_CreatedBy default system_user,
		constraint PK_MetadataCategory primary key clustered(MetadataCategoryId),
		constraint FK_MetadataCategory_MetadataType foreign key (MetadataTypeId) references General.MetadataType(MetadataTypeId),
		constraint UQ_MetadataCategory_Name unique ([Name])
	);
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'Localization')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Tag';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'Localization');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'Revision')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Tag';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'Revision');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'Consumer Vocabulary')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Taxonomy';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'Consumer Vocabulary');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'Aspect')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Category';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'Aspect');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'causedBy')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Ontology';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'causedBy');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'diagnosedOrMonitoredUsing')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Ontology';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'diagnosedOrMonitoredUsing');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'evaluatedUsing')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Ontology';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'evaluatedUsing');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'hasComplication')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Ontology';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'hasComplication');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'hasParent')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Ontology';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'hasParent');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'hasPediatricVersion')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Ontology';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'hasPediatricVersion');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'hasRiskFactor')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Ontology';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'hasRiskFactor');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'hasSelfCare')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Ontology';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'hasSelfCare');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'hasSymptom')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Ontology';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'hasSymptom');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'isPediatricVersionOf')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Ontology';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'isPediatricVersionOf');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'isRiskFactorFor')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Ontology';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'isRiskFactorFor');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'manages')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Ontology';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'manages');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'medicalSpecialties')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Ontology';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'medicalSpecialties');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'possibleComplicationOf')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Ontology';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'possibleComplicationOf');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'preventedBy')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Ontology';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'preventedBy');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'screenedWith')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Ontology';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'screenedWith');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'screensFor')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Ontology';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'screensFor');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'treatedBy')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Ontology';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'treatedBy');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'isMedicalSpecialty')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Tag';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'isMedicalSpecialty');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'cpt')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Code System';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'cpt');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'icd10cm')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Code System';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'icd10cm');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'icd10pcs')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Code System';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'icd10pcs');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'icd9cm')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Code System';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'icd9cm');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'lnc')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Code System';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'lnc');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'nucc')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Code System';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'nucc');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'rxnorm')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Code System';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'rxnorm');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'snomedct')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Code System';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'snomedct');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'Keywords')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Tag';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'Keywords');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'Synonyms')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Tag';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'Synonyms');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'Specialty')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Tag';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'Specialty');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'NUCC Specialty')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Tag';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'NUCC Specialty');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'Reading Level')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Metric';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'Reading Level');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'Audience')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Category';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'Audience');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'Age Range')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Range';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'Age Range');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'Topic Type')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Category';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'Topic Type');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'Topic Status')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Category';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'Topic Status');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'Topic Module Type')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Category';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'Topic Module Type');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'Is Master File')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Tag';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'Is Master File');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'PI Type')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Category';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'PI Type');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'Age Code')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Tag';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'Age Code');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'Codeword')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Tag';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'Codeword');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'Delivery Setting')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Tag';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'Delivery Setting');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'Article Facet')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Category';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'Article Facet');
end
go

if not exists (select 1 from General.MetadataCategory where [Name] = 'Article Category')
begin
	declare @type int;
	select @type = MetadataTypeId from General.MetadataType where [Name] = 'Category';

	insert into General.MetadataCategory(MetadataTypeId, [Name]) values (@type, 'Article Category');
end
go

-- ##########

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'AssetCategoryMetadataAssignment' and TABLE_SCHEMA = 'General')
begin
	create table General.AssetCategoryMetadataAssignment
	(
		AssetCategoryId int not null,
		MetadataCategoryId int not null,		
		Implemented bit not null constraint DF_AssetCategoryMetadataAssignment_Implemented default 1,
		CreatedDate datetime2(2) not null constraint DF_AssetCategoryMetadataAssignment_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_AssetCategoryMetadataAssignment_CreatedBy default system_user,
		constraint PK_AssetCategoryMetadataAssignment primary key clustered(AssetCategoryId, MetadataCategoryId),
		constraint FK_AssetCategoryMetadataAssignment_AssetCategory foreign key(AssetCategoryId) references General.AssetCategory(AssetCategoryId),
		constraint FK_AssetCategoryMetadataAssignment_MetadataCategory foreign key(MetadataCategoryId) references General.MetadataCategory(MetadataCategoryId)
	);

	create unique nonclustered index UX_AssetCategoryMetadataAssignment_NaturalKey on General.AssetCategoryMetadataAssignment(AssetCategoryId, MetadataCategoryId);

	-- Load Asset Categories
	declare @topicCategoryId int;
	select @topicCategoryId = AssetCategoryId  from General.AssetCategory where [Name] = 'Topic';

	declare @conceptCategoryId int;
	select @conceptCategoryId = AssetCategoryId  from General.AssetCategory where [Name] = 'Concept';

	declare @videoCategoryId int;
	select @videoCategoryId = AssetCategoryId  from General.AssetCategory where [Name] = 'Video';

	declare @imageCategoryId int;
	select @imageCategoryId = AssetCategoryId  from General.AssetCategory where [Name] = 'Image';

	declare @moduleCategoryId int;
	select @moduleCategoryId = AssetCategoryId  from General.AssetCategory where [Name] = 'Module';

	declare @piCategoryId int;
	select @piCategoryId = AssetCategoryId  from General.AssetCategory where [Name] = 'Patient Instruction';

	declare @programCategoryId int;
	select @programCategoryId = AssetCategoryId  from General.AssetCategory where [Name] = 'Program';

	-- Load Metadata Categories
	declare @localizationCategoryId int;
	select @localizationCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'Localization';

	declare @revisionCategoryId int;
	select @revisionCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'Revision';

	declare @vocabularyCategoryId int;
	select @vocabularyCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'Consumer Vocabulary';

	declare @aspectCategoryId int;
	select @aspectCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'Aspect';

	declare @causedByCategoryId int;
	select @causedByCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'causedBy';

	declare @diagnosedOrMonitoredUsingCategoryId int;
	select @diagnosedOrMonitoredUsingCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'diagnosedOrMonitoredUsing';

	declare @evaluatedUsingCategoryId int;
	select @evaluatedUsingCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'evaluatedUsing';

	declare @hasComplicationCategoryId int;
	select @hasComplicationCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'hasComplication';

	declare @hasParentCategoryId int;
	select @hasParentCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'hasParent';

	declare @hasPediatricVersionCategoryId int;
	select @hasPediatricVersionCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'hasPediatricVersion';

	declare @hasRiskFactorCategoryId int;
	select @hasRiskFactorCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'hasRiskFactor';

	declare @hasSelfCareCareCategoryId int;
	select @hasSelfCareCareCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'hasSelfCare';

	declare @hasSymptomCategoryId int;
	select @hasSymptomCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'hasSymptom';

	declare @isPediatricVersionOfCategoryId int;
	select @isPediatricVersionOfCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'isPediatricVersionOf';

	declare @isRiskFactorForCategoryId int;
	select @isRiskFactorForCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'isRiskFactorFor';

	declare @managesCategoryId int;
	select @managesCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'manages';

	declare @medicalSpecialtiesCategoryId int;
	select @medicalSpecialtiesCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'medicalSpecialties';

	declare @possibleComplicationOfCategoryId int;
	select @possibleComplicationOfCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'possibleComplicationOf';

	declare @preventedByCategoryId int;
	select @preventedByCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'preventedBy';

	declare @screenedWithCategoryId int;
	select @screenedWithCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'screenedWith';

	declare @screensForCategoryId int;
	select @screensForCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'screensFor';

	declare @treatedByCategoryId int;
	select @treatedByCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'treatedBy';

	declare @cptCategoryId int;
	select @cptCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'cpt';

	declare @icd10cmCategoryId int;
	select @icd10cmCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'icd10cm';

	declare @icd10pcsCategoryId int;
	select @icd10pcsCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'icd10pcs';

	declare @icd9cmCategoryId int;
	select @icd9cmCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'icd9cm';

	declare @lncCategoryId int;
	select @lncCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'lnc';

	declare @nuccCategoryId int;
	select @nuccCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'nucc';

	declare @rxnormCategoryId int;
	select @rxnormCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'rxnorm';

	declare @snomedctCategoryId int;
	select @snomedctCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'snomedct';

	declare @keywordsCategoryId int;
	select @keywordsCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'Keywords';

	declare @synonymsCategoryId int;
	select @synonymsCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'Synonyms';

	declare @readingLevelCategoryId int;
	select @readingLevelCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'Reading Level';

	declare @audienceCategoryId int;
	select @audienceCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'Audience';

	declare @ageRangeCategoryId int;
	select @ageRangeCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'Age Range';

	declare @topicTypeCategoryId int;
	select @topicTypeCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'Topic Type';

	declare @topicStatusCategoryId int;
	select @topicStatusCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'Topic Status';

	declare @topicModuleTypeCategoryId int;
	select @topicModuleTypeCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'Topic Module Type';

	declare @isMasterFileCategoryId int;
	select @isMasterFileCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'Is Master File';

	declare @piTypeCategoryId int;
	select @piTypeCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'PI Type';

	declare @ageCodeCategoryId int;
	select @ageCodeCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'Age Code';

	declare @codewordCategoryId int;
	select @codewordCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'Codeword';

	declare @deliverySettingCategoryId int;
	select @deliverySettingCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'Delivery Setting';

	declare @articleFacetCategoryId int;
	select @articleFacetCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'Article Facet';

	declare @articleCategoryCategoryId int;
	select @articleCategoryCategoryId = MetadataCategoryId  from General.MetadataCategory where [Name] = 'Article Category';

	-- Assign metadata categories to asset categories
	-- Concept
	delete from General.AssetCategoryMetadataAssignment where AssetCategoryId = @conceptCategoryId;
	insert into General.AssetCategoryMetadataAssignment(AssetCategoryId, Implemented, MetadataCategoryId)
		values 
				(@conceptCategoryId, 0, @localizationCategoryId),
				(@conceptCategoryId, 0, @revisionCategoryId),
				(@conceptCategoryId, 1, @keywordsCategoryId),
				(@conceptCategoryId, 1, @synonymsCategoryId),
				(@conceptCategoryId, 1, @causedByCategoryId),
				(@conceptCategoryId, 1, @diagnosedOrMonitoredUsingCategoryId),
				(@conceptCategoryId, 1, @evaluatedUsingCategoryId),
				(@conceptCategoryId, 1, @hasComplicationCategoryId),
				(@conceptCategoryId, 1, @hasParentCategoryId),
				(@conceptCategoryId, 1, @hasPediatricVersionCategoryId),
				(@conceptCategoryId, 1, @hasRiskFactorCategoryId),
				(@conceptCategoryId, 1, @hasSelfCareCareCategoryId),
				(@conceptCategoryId, 1, @hasSymptomCategoryId),
				(@conceptCategoryId, 1, @isPediatricVersionOfCategoryId),
				(@conceptCategoryId, 1, @isRiskFactorForCategoryId),
				(@conceptCategoryId, 1, @managesCategoryId),
				(@conceptCategoryId, 1, @medicalSpecialtiesCategoryId),
				(@conceptCategoryId, 1, @possibleComplicationOfCategoryId),
				(@conceptCategoryId, 1, @preventedByCategoryId),
				(@conceptCategoryId, 1, @screenedWithCategoryId),
				(@conceptCategoryId, 1, @screensForCategoryId),
				(@conceptCategoryId, 1, @treatedByCategoryId),
				(@conceptCategoryId, 1, @cptCategoryId),
				(@conceptCategoryId, 1, @icd10cmCategoryId),
				(@conceptCategoryId, 1, @icd10pcsCategoryId),
				(@conceptCategoryId, 1, @icd9cmCategoryId),
				(@conceptCategoryId, 1, @lncCategoryId),
				(@conceptCategoryId, 1, @nuccCategoryId),
				(@conceptCategoryId, 1, @rxnormCategoryId),
				(@conceptCategoryId, 1, @snomedctCategoryId);

	-- Topic
	delete from General.AssetCategoryMetadataAssignment where AssetCategoryId = @topicCategoryId;
	insert into General.AssetCategoryMetadataAssignment(AssetCategoryId, Implemented, MetadataCategoryId)
		values 
			(@topicCategoryId, 1, @localizationCategoryId),
			(@topicCategoryId, 0, @revisionCategoryId),
			(@topicCategoryId, 1, @vocabularyCategoryId),
			(@topicCategoryId, 1, @aspectCategoryId),
			(@topicCategoryId, 1, @readingLevelCategoryId),
			(@topicCategoryId, 1, @audienceCategoryId),
			(@topicCategoryId, 1, @ageRangeCategoryId),
			(@topicCategoryId, 1, @topicTypeCategoryId),
			(@topicCategoryId, 1, @topicStatusCategoryId),
			(@topicCategoryId, 1, @topicModuleTypeCategoryId);

	
	-- Image
	delete from General.AssetCategoryMetadataAssignment where AssetCategoryId = @imageCategoryId;
	insert into General.AssetCategoryMetadataAssignment(AssetCategoryId, Implemented, MetadataCategoryId)
		values 
			(@imageCategoryId, 1, @localizationCategoryId),
			(@imageCategoryId, 0, @revisionCategoryId),
			(@imageCategoryId, 1, @vocabularyCategoryId),
			(@imageCategoryId, 1, @aspectCategoryId),
			(@imageCategoryId, 1, @keywordsCategoryId),
			(@imageCategoryId, 0, @isMasterFileCategoryId);

	-- Video
	delete from General.AssetCategoryMetadataAssignment where AssetCategoryId = @videoCategoryId;
	insert into General.AssetCategoryMetadataAssignment(AssetCategoryId, Implemented, MetadataCategoryId)
		values 
			(@videoCategoryId, 1, @localizationCategoryId),
			(@videoCategoryId, 0, @revisionCategoryId),
			(@videoCategoryId, 1, @vocabularyCategoryId),
			(@videoCategoryId, 1, @aspectCategoryId),
			(@videoCategoryId, 1, @readingLevelCategoryId),
			(@videoCategoryId, 1, @audienceCategoryId),
			(@videoCategoryId, 1, @ageRangeCategoryId),
			(@videoCategoryId, 1, @topicTypeCategoryId),
			(@videoCategoryId, 1, @topicStatusCategoryId),
			(@videoCategoryId, 1, @topicModuleTypeCategoryId);

	-- Modules (for Programs)
	delete from General.AssetCategoryMetadataAssignment where AssetCategoryId = @moduleCategoryId;
	insert into General.AssetCategoryMetadataAssignment(AssetCategoryId, Implemented, MetadataCategoryId)
		values 
			(@moduleCategoryId, 1, @localizationCategoryId),
			(@moduleCategoryId, 1, @revisionCategoryId),
			(@moduleCategoryId, 1, @vocabularyCategoryId);

	-- Patient Instructions
	delete from General.AssetCategoryMetadataAssignment where AssetCategoryId = @piCategoryId;
	insert into General.AssetCategoryMetadataAssignment(AssetCategoryId, Implemented, MetadataCategoryId)
		values 
			(@piCategoryId, 1, @localizationCategoryId),
			(@piCategoryId, 1, @revisionCategoryId),
			(@piCategoryId, 1, @vocabularyCategoryId),
			(@piCategoryId, 1, @aspectCategoryId),
			(@piCategoryId, 1, @piTypeCategoryId),
			(@piCategoryId, 1, @ageRangeCategoryId),
			(@piCategoryId, 1, @ageCodeCategoryId),
			(@piCategoryId, 1, @codewordCategoryId),
			(@piCategoryId, 1, @deliverySettingCategoryId),
			(@piCategoryId, 1, @articleFacetCategoryId),
			(@piCategoryId, 1, @articleCategoryCategoryId);

	-- Programs, as a matter of fact, don't have metadata!
end
go

-- ##########

if not exists (select 1 from sys.sequences where [name] = 'AssetId')
begin
	create sequence General.AssetId as int start with 1 increment by 1 no cycle;
end

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'Asset' and TABLE_SCHEMA = 'General')
	create table General.Asset
	(
		AssetId bigint not null constraint DF_Asset_AssetId default next value for General.AssetId,
		AssetCategoryId int not null,
		Hwid varchar(16) not null,
		Localization varchar(8) not null,
		Revision decimal (5, 2),		
		Title nvarchar(250) not null,
		[Description] nvarchar(max),
		RevisionDate datetime2(2) not null constraint DF_Asset_RevisionDate default getutcdate(),
		DeprecationDate datetime2(2),
		CreatedDate datetime2(2) not null constraint DF_Asset_CreatedDate default getutcdate(),		
		CreatedBy nvarchar(128) not null constraint DF_Asset_CreatedBy default system_user,
		UpdatedDate datetime2(2) not null constraint DF_Asset_UpdatedDate default getutcdate(),
		UpdatedBy nvarchar(128) not null constraint DF_Asset_UpdatedBy default system_user,
		constraint PK_Asset primary key clustered(AssetId),
		constraint FK_Asset_AssetCategory foreign key (AssetCategoryId) references General.AssetCategory(AssetCategoryId)
	);
go

-- ##########

select * from General.AssetType;
select * from General.AssetCategory;
select * from General.MetadataType;
select * from General.MetadataCategory;

select
	t.[Name] as AssetType,
	ac.[Name] as AssetCategory,
	mt.[Name] as MetadataType,
	mc.[Name] as MetadataCategory
from General.AssetCategory ac
	inner join General.AssetType t on ac.AssetTypeId = t.AssetTypeId
	left outer join General.AssetCategoryMetadataAssignment ass on ass.AssetCategoryId = ac.AssetCategoryId
	left outer join General.MetadataCategory mc on ass.MetadataCategoryId = mc.MetadataCategoryId
	left outer join General.MetadataType mt on mc.MetadataTypeId = mt.MetadataTypeId