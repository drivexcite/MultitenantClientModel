create schema [Provision]; 
go

create table [Provision].[Application]
(
    ApplicationId int not null,
    Code varchar(30) not null,
    [Name] varchar(120) not null,
    [Description] varchar(120) not null,
    OwnerEmail varchar(100) not null,
    CreatedDate datetime2 not null constraint DF_CreatedDate default getutcdate(),
    constraint PK_ApplicationId primary key clustered (ApplicationId),
    constraint UQ_Code unique (Code)
);
go

create table [Provision].[Version]
(
    VersionId int not null,    
    ApplicationId int not null,
    VersionNumber varchar(10) not null,
    AvailableFrom datetime2(2) not null,
    Active bit not null constraint DF_Active default 1,
    constraint PK_VersionId primary key clustered (VersionId),
    constraint FK_Version_ApplicationId foreign key (ApplicationId) references [Provision].[Application](ApplicationId),
    constraint UQ_ApplicationId_Version unique (ApplicationId, VersionNumber)
);
go

create table [Provision].Feature
(
    FeatureId int not null,
    VersionId int not null,
    [Name] varchar(120) not null,
    [Description] varchar(max) not null,
    [Parameters] nvarchar(max),
    constraint PK_FeatureId primary key clustered (FeatureId),
    constraint FK_Feature_Version foreign key (VersionId) references [Provision].[Version](VersionId)
);
go

create table [Provision].Tier
(
    TierId int not null,
    FeatureId int not null,
    [Name] varchar(120) not null,
    [Description] varchar(max) not null,
    [Parameters] nvarchar(max),
    constraint PK_TierId primary key clustered (TierId),
    constraint FK_Tier_Feature foreign key (FeatureId) references [Provision].[Feature](FeatureId)
);
go

create table [Provision].[ContractType]
(
    ContractType tinyint not null,
    [Text] varchar(30) not null,
    [Description] varchar(120),
	CreatedDate datetime2(2) not null constraint DF_ContractType_CreatedDate default getutcdate(),
    constraint PK_ContractType primary key clustered (ContractType)    
);
go

create unique nonclustered index IX_ContractType_Text on [Provision].[ContractType]([Text]);
go

create table [Provision].[Contract]
(
    ContractId int not null,
    ClientId uniqueidentifier not null,
    CreatedDate datetime2(2) not null constraint DF_Contract_CreatedDate default getutcdate(),
    EffectiveDate datetime2(2) not null,
    ParentContractId int,
    ContractType tinyint not null,
    constraint PK_ContractId primary key clustered (ContractId)
);
go

create table [Provision].ProvisioningStatus
(
    [Status] tinyint not null,
    [Text] varchar(30) not null,
    [Description] varchar(120),
	CreatedDate datetime2(2) not null constraint DF_ProvisioningStatus_CreatedDate default getutcdate(),
    constraint PK_Status primary key clustered ([Status])
);
go

create unique nonclustered index IX_ProvisioningStatus_Text on [Provision].ProvisioningStatus([Text]);

create table [Provision].[Delivery]
(
    DeliveryId int not null,
    ContractId int not null,
    CreatedDate datetime2(2) not null constraint DF_Delivery_CreatedDate default getutcdate(),
    [Status] tinyint not null,
    ApplicationTree nvarchar(max) not null,
    constraint PK_DeliveryId primary key clustered (DeliveryId),
    constraint FK_Delivery_Contract foreign key (ContractId) references [Provision].[Contract](ContractId),
    constraint FK_Delivery_Status foreign key ([Status]) references  [Provision].ProvisioningStatus([Status])
);
go

create table [Provision].ProvisioningStep
(
    DeliveryId int not null,
    VersionId int not null, 
    FeatureId int not null,
    TierId int not null,
    Attempt tinyint not null constraint DF_ProvisioningStep_Attempt default 1,
    CreatedDate datetime2(2) not null constraint DF_ProvisioningStep_CreatedDate default getutcdate(),
    WasSuccessful bit not null constraint DF_ProvisioningStep_WasSuccessful default 1,
    ProvisionerStatusUrl varchar(max) not null,
    constraint PK_ProvisioningStep primary key clustered (DeliveryId, VersionId, FeatureId, TierId, Attempt),
    constraint FK_ProvisioningStep_Delivery foreign key (DeliveryId) references [Provision].[Delivery](DeliveryId),
    constraint FK_ProvisioningStep_Version foreign key (VersionId) references [Provision].[Version](VersionId),
    constraint FK_ProvisioningStep_Feature foreign key (FeatureId) references [Provision].[Feature](FeatureId),
    constraint FK_ProvisioningStep_Tier foreign key (TierId) references [Provision].[Tier](TierId)
);
go

DROP TABLE [Provision].ProvisioningStep;
DROP TABLE [Provision].[Delivery];
DROP TABLE [Provision].ProvisioningStatus;
DROP TABLE [Provision].[Contract];
DROP TABLE [Provision].[ContractType];
DROP TABLE [Provision].Tier;
DROP TABLE [Provision].Feature;
DROP TABLE [Provision].[Version];
DROP TABLE [Provision].[Application];
DROP SCHEMA [Provision];
go


insert into [Provision].[ContractType](ContractType, [Text], [Description]) values (1, 'Master', 'Initial Client contract for Licensing of Applications and Content');
insert into [Provision].[ContractType](ContractType, [Text], [Description]) values (2, 'Addendum', 'Modification of the Initial Contract for the addition or supression of Applications and Content');
insert into [Provision].[ContractType](ContractType, [Text], [Description]) values (3, 'Termination', 'Removal of all Applications and Content');

insert into [Provision].ProvisioningStatus([Status], [Text], [Description]) values (1, 'Requested', 'The provisioning routine has been invoked, but the result of the operation is unknown');
insert into [Provision].ProvisioningStatus([Status], [Text], [Description]) values (2, 'Completed', 'The provisioning routine has completed the work');
insert into [Provision].ProvisioningStatus([Status], [Text], [Description]) values (3, 'Failed', 'The provisioning routine could not run to completion due to errors');
insert into [Provision].ProvisioningStatus([Status], [Text], [Description]) values (4, 'Canceled', 'The provisioning routine was instructed to cancel the operation before completion');

-- Definition for Triage Engine.
insert into [Provision].[Application](ApplicationId, Code, [Name], [Description], [OwnerEmail]) values (1, 'triagesx', 'Triage Engine', 'Web API and rule engine to drive question-answer based implentations of Symptom Topics', 'platform@healthwise.org');
insert into [Provision].[Version](VersionId, ApplicationId, VersionNumber, AvailableFrom) values (1, 1, '1.0', '2019-11-01');
insert into [Provision].Feature(FeatureId, VersionId, [Name], [Description], [Parameters]) values (1, 1, 'Start Encounter', 'Allows the client to initialize the rules engine for a given symptom topic', '{ "allowedOrigins": "@@request:[Allowed Origins]:list" }');

insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (1, 1, 'Demo', 'Up to 100 encounters per month', '{ "maxEncounters": 100, "period": "monthly", "priority": "none" }');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (2, 1, 'Basic', 'Up to 10,000 encounters per month', '{ "maxEncounters": 10_000, "period": "monthly", "priority": "medium" }');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (3, 1, 'Midrange', 'Up to 1,000,000 encounters per month', '{ "maxEncounters": 1_000_000, "period": "monthly", "priority": "medium" }');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (4, 1, 'High', 'Up to 1,000,000 encounters per day', '{ "maxEncounters": 1_000_000, "period": "daily", "priority": "high" }');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (5, 1, 'Unlimited', 'Unlimited encounters', '{ "maxEncounters": -1, "period": "yearly", "priority": "medium" }');

-- Definition for Programs.
insert into [Provision].[Application](ApplicationId, Code, [Name], [Description], [OwnerEmail]) values (2, 'programs', 'Programs', 'Set of applications to create modular pieces of content based on Healthwise Structured Content Library', 'programs@healthwise.org');
insert into [Provision].[Version](VersionId, ApplicationId, VersionNumber, AvailableFrom) values (2, 2, '1.0', '2019-12-31');
insert into [Provision].Feature(FeatureId, VersionId, [Name], [Description], [Parameters]) values (2, 2, 'Program Manager', 'Allows the client a read only view of pre-defined Healthwise Curated Programs', '');
insert into [Provision].Feature(FeatureId, VersionId, [Name], [Description], [Parameters]) values (3, 2, 'Program Builder', 'Allows the client access to all Healthwise Curated Programs plus the ability to create their own', '{ "showTips": "true" }');
insert into [Provision].Feature(FeatureId, VersionId, [Name], [Description], [Parameters]) values (4, 2, 'Assessment Builder', 'Allow the clients the ability to incorporate questionnaires and assessments after delivering content', '');
insert into [Provision].Feature(FeatureId, VersionId, [Name], [Description], [Parameters]) values (5, 2, 'Electronic Delivery - Email', 'Allows the client the ability to Prescribe Programs via Email', '{ "forwardTo": "@@request:[Forward Prescriptions To]:email" }');
insert into [Provision].Feature(FeatureId, VersionId, [Name], [Description], [Parameters]) values (6, 2, 'Electronic Delivery - SMS', 'Allows the client the ability to Prescribe Programs via SMS', '');

insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (6, 2, 'Demo', 'Restricted access to Ashtma and Diabetes pre-defined Programs', '');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (7, 2, 'Unrestricted', 'Unrestricted access to Healthwise Curated Programs (subject to Content licensing)', '');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (8, 3, 'Demo', 'Allows client the ability to create Programs with the pre-defined Programs Demo Content Set', '{ "restrictedContentSet": "@@request:[Demo Content Set]:string ?? programsDemo001" }');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (9, 3, 'Unrestricted', 'Unrestricted access to Programs Builder', '');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (10, 4, 'Demo', 'Allows client the ability to create Assessments and asign them to Programs with a limited persistence option', '{"persistence.period": "days", "persistence.allowedUnits": 30}');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (11, 4, 'Unrestricted', 'Allows client the ability to create Assessments and asign them to Programs', '');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (12, 5, 'Demo', 'Up to 50 emails for the entire duration of the Programs Trial', '{ "maxEmails": 50, "allowedRecipients": "@@request:[Allowed Email Recipients]:email[]" }');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (13, 5, 'Basic', 'Up to 10,000 emails per month', '{ "maxEmails": 10_000, "period": "monthly" }');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (14, 5, 'Midrange', 'Up to 1,000,000 emails per month', '{ "maxEmails": 1_000_000, "period": "monthly" }');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (15, 5, 'Unlimited', 'Unlimited amount of emails deliveries', '{ "maxEmails": -1 }');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (16, 6, 'Demo', 'Up to 50 text messages for the entire duration of the Programs Trial', '{ "maxTextMessages": 50, "allowedRecipients": "@@request:[Allowed SMS Recipients]:phoneNumber[]" }');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (17, 5, 'Unlimited', 'Unlimited amount of SMS deliveries', '{ "maxTextMessages": -1 }');

-- Definition for Coach
insert into [Provision].[Application](ApplicationId, Code, [Name], [Description], [OwnerEmail]) values (3, 'coach', 'Clinical Content Manager', 'Web Application to deliver content in a Standalone Environment', 'solutions@healthwise.org');
insert into [Provision].[Version](VersionId, ApplicationId, VersionNumber, AvailableFrom) values (3, 3, '1.0', '2010-1-1');
insert into [Provision].[Version](VersionId, ApplicationId, VersionNumber, AvailableFrom) values (4, 3, '2.0', '2020-1-1');

-- for Coach 1.0
insert into [Provision].Feature(FeatureId, VersionId, [Name], [Description], [Parameters]) values (7, 3, 'Prescription', 'Allows clinicias to prescribe Content from the Legacy Content Library', '{ "hwKey": "@@request:[Client Hw Key]:string" }');
insert into [Provision].Feature(FeatureId, VersionId, [Name], [Description], [Parameters]) values (8, 3, 'Print Manager', 'Allows clinicians to print patient instructions from the Legacy Content Library', '');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (18, 7, 'Unrestricted', 'Clinicians can prescribe an unlimited amount of Articles per day', '');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (19, 8, 'Unrestricted', 'Clinicians can print an unlimited amount of Articles per day', '');

-- for Coach 2.0
insert into [Provision].Feature(FeatureId, VersionId, [Name], [Description], [Parameters]) values (9, 4, 'Prescriptions', 'Allows clinicias to prescribe Content from its license', '{ "contentLicense": "types:(videos,images) && specialties:(HWCV_FOOBAR1,HWCV_FOOBAR2) && icd10:(A00-B99,C00-D49)" }');
insert into [Provision].Feature(FeatureId, VersionId, [Name], [Description], [Parameters]) values (10, 4, 'Bookmarks', 'Allows clinicians to save an organized, custom curated, speed-dial collection of individual Content items', '');
insert into [Provision].Feature(FeatureId, VersionId, [Name], [Description], [Parameters]) values (11, 4, 'Print Manager', 'Allows clinicias to print patient instructions', '');
insert into [Provision].Feature(FeatureId, VersionId, [Name], [Description], [Parameters]) values (12, 4, 'Longitudinal Campaign', 'Allows clinicias the ability to prescribe Content Collections one item at a time on a defined schedule', '');

insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (20, 9, 'Demo', 'Up to 50 prescriptions for the duration of the Trial', '{ "expiration": "@@request:[Trial Expiration Date]:date ?? TimeSpan.FromDays(30)" }');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (21, 9, 'Basic', 'Up to 10,000 prescriptions per month', '{ "prescriptionQuota": 10_000, "quotaPeriod: "monthly", "onQuotaExhausted": "reject" }');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (22, 9, 'Midrange', 'Up to 1,000,000 prescriptions per month', '{ "prescriptionQuota": 1_000_000, "quotaPeriod: "monthly", "onQuotaExhausted": "degrade" }');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (23, 9, 'Consumption Plan', 'Billed by operation', '{ "prescriptionQuota": 1, "quotaPeriod: "monthly", "onQuotaExhausted": "bill" }');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (24, 9, 'Unlimited', 'Unlimited Prescriptions', '{ "prescriptionQuota": -1, "quotaPeriod: "annualy"');

insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (25, 10, 'Unrestricted', 'No service levels defined for Bookmarks', '');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (26, 11, 'Unrestricted', 'No service levels defined for Print Manager', '');
insert into [Provision].Tier(TierId, FeatureId, [Name], [Description], [Parameters]) values (27, 12, 'Unrestricted', 'No service levels defined for Longitudinal Campaigns', '');

-- Query all!
select 
	a.Code as ApplicationCode, 
	a.[Name] as ApplicationName, 
	v.[VersionNumber],
	v.[Active], 
	f.[Name] as FeatureName,
	f.[Description] as FeatureDescription,
	f.[Parameters] as FeatureParameters,
	t.[Name] as TierName,
	t.[Description] as TierDescription,
	t.[Parameters] as TierParameters
from [Provision].[Application] a
	inner join [Provision].[Version] v on a.ApplicationId = v.ApplicationId
	inner join [Provision].[Feature] f on v.VersionId = f.VersionId
	inner join [Provision].Tier t on f.FeatureId = t.FeatureId