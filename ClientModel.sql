if not exists (select 1 from sys.schemas where [name] = 'Client')
	 exec('CREATE SCHEMA [Client]');
go

-- Tables
if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'AccountType' and TABLE_SCHEMA = 'Client')
	create table Client.AccountType 
	(
		AccountTypeId tinyint not null,
		[Name] varchar(40) not null,
		CreatedDate datetime2(2) not null constraint DF_AccountType_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_AccountType_CreatedBy default system_user,
		constraint PK_AccountType primary key clustered(AccountTypeId),
		constraint UQ_AccountType_Name unique ([Name])
	);
go

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'ClientArchetype' and TABLE_SCHEMA = 'Client')
	create table Client.ClientArchetype 
	(
		ArchetypeId tinyint not null,
		[Name] varchar(40) not null,
		CreatedDate datetime2(2) not null constraint DF_ClientArchetype_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_ClientArchetype_CreatedBy default system_user,
		constraint PK_ClientArchetype primary key clustered(ArchetypeId),
		constraint UQ_ClientArchetype_Name unique ([Name])
	);
go

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'Account' and TABLE_SCHEMA = 'Client')
begin
	create table Client.Account 
	(
		AccountId int not null,
		[Name] varchar(120) not null,
		AccountType tinyint not null,
		Archetype tinyint not null,
		SalesforceId varchar(40) not null,
		[Enabled] bit not null constraint DF_Account_Enabled default 1,
		CreatedDate datetime2(2) not null constraint DF_Account_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_Account_CreatedBy default system_user,
		constraint PK_Account primary key clustered (AccountId),
		constraint UQ_Account_Name unique ([Name]),
		constraint FK_Account_AccountType foreign key (AccountType) references Client.AccountType(AccountTypeId),
		constraint FK_Account_ClientArchetype foreign key (Archetype) references Client.ClientArchetype(ArchetypeId)
	);

	create index IX_Account_AccountType on Client.Account(AccountType);
	create index IX_Account_ClientArchetype on Client.Account(Archetype);
end
go

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'PrimarySubscription' and TABLE_SCHEMA = 'Client')
begin
	create table Client.PrimarySubscription 
	(
		PrimarySubscriptionId int not null,
		AccountId int not null,
		[Name] varchar(120) not null,
		[OrganizationalUnit] nvarchar(120),
		ActivationDate datetime2(2) not null,
		CreatedDate datetime2(2) not null constraint DF_PrimarySubscription_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_PrimarySubscription_CreatedBy default system_user,
		constraint PK_PrimarySubscription primary key clustered (PrimarySubscriptionId),
		constraint FK_PrimarySubscription_Account foreign key (AccountId) references Client.Account(AccountId)
	);

	create index IX_PrimarySubscription_Account on Client.PrimarySubscription(AccountId);
end
go

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'Subscription' and TABLE_SCHEMA = 'Client')
begin
	create table Client.Subscription 
	(
		SubscriptionId int not null,
		PrimarySubscriptionId int not null,
		[Name] varchar(120) not null,
		[Enabled] bit not null constraint DF_Subscription_Enabled default 1,
		CreatedDate datetime2(2) not null constraint DF_Subscription_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_Subscription_CreatedBy default system_user,
		constraint PK_Subscription primary key clustered (SubscriptionId),
		constraint FK_Subscription_PrimarySubscription foreign key (PrimarySubscriptionId) references Client.PrimarySubscription(PrimarySubscriptionId)
	);

	create index IX_Subscription_PrimarySubscription on Client.Subscription(SubscriptionId);
end
go

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'SubscriptionPath' and TABLE_SCHEMA = 'Client')
begin
	create table Client.SubscriptionPath 
	(
		AncestorSubscriptionId int not null,
		DescendantSubscriptionId int not null,
		CreatedDate datetime2(2) not null constraint DF_SubscriptionPath_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_SubscriptionPath_CreatedBy default system_user,
		constraint PK_SubscriptionPath primary key clustered (AncestorSubscriptionId, DescendantSubscriptionId),
		constraint FK_SubscriptionPath_AncestorSubscription foreign key (AncestorSubscriptionId) references Client.Subscription(SubscriptionId),
		constraint FK_SubscriptionPath_DescendantSubscription foreign key (DescendantSubscriptionId) references Client.Subscription(SubscriptionId)
	);

	create index IX_SubscriptionPath_AncestorSubscription on Client.SubscriptionPath(AncestorSubscriptionId);
	create index IX_SubscriptionPath_DescendantSubscription on Client.SubscriptionPath(DescendantSubscriptionId);
end
go

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'IdentitySource' and TABLE_SCHEMA = 'Client')
begin
	create table Client.IdentitySource 
	(
		IdentitySourceId int not null,
		[Name] varchar(120) not null,
		PrimarySubscriptionId int not null,
		CreatedDate datetime2(2) not null constraint DF_IdentitySource_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_IdentitySource_CreatedBy default system_user,
		constraint PK_IdentitySource primary key clustered (IdentitySourceId),
		constraint FK_IdentitySource_PrimarySubscription foreign key (PrimarySubscriptionId) references Client.PrimarySubscription(PrimarySubscriptionId)
	);

	create index IX_IdentitySource_PrimarySubscription on Client.IdentitySource(PrimarySubscriptionId);
end
go

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'DataLinkType' and TABLE_SCHEMA = 'Client')
	create table Client.DataLinkType 
	(
		DataLinkTypeId tinyint not null,
		[Name] varchar(40) not null,
		CreatedDate datetime2(2) not null constraint DF_DataLinkType_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_DataLinkType_CreatedBy default system_user,
		constraint PK_DataLinkType primary key clustered (DataLinkTypeId)
	);
go

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'DataLink' and TABLE_SCHEMA = 'Client')
begin
	create table Client.DataLink 
	(
		FromSubscriptionId int not null,
		ToSubscriptionId int not null,
		[Type] tinyint not null,
		CreatedDate datetime2(2) not null constraint DF_DataLink_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_DataLink_CreatedBy default system_user,
		constraint PK_DataLink primary key clustered (FromSubscriptionId, ToSubscriptionId, [Type]),
		constraint FK_DataLink_FromSubscription foreign key (FromSubscriptionId) references Client.Subscription(SubscriptionId),
		constraint FK_DataLink_ToSubscription foreign key (ToSubscriptionId) references Client.Subscription(SubscriptionId),
		constraint FK_DataLink_DataLinkType foreign key ([Type]) references Client.DataLinkType(DataLinkTypeId)
	);

	create index IX_DataLink_FromSubscription on Client.DataLink(FromSubscriptionId);
	create index IX_DataLink_ToSubscriptionId on Client.DataLink(ToSubscriptionId);
	create index IX_DataLink_DataLinkType on Client.DataLink([Type]);
end
go

declare @removeAllTables bit = 0;

if(@removeAllTables = 1)
begin	
	drop table Client.DataLink;	
	drop table Client.DataLinkType;	
	drop table Client.IdentitySource;
	drop table Client.SubscriptionPath;
	drop table Client.Subscription;
	drop table Client.PrimarySubscription;
	drop table Client.Account;
	drop table Client.ClientArchetype;
	drop table Client.AccountType;
end;
go

-- Sequences
if not exists (select 1 from sys.sequences where [name] = 'AccountId')
begin
	create sequence Client.AccountId as int start with 1 increment by 1 no cycle;
end

if not exists (select 1 from sys.sequences where [name] = 'PrimarySubscriptionId')
begin
	create sequence Client.PrimarySubscriptionId as int start with 1 increment by 1 no cycle;
end

if not exists (select 1 from sys.sequences where [name] = 'SubscriptionId')
begin
	create sequence Client.SubscriptionId as int start with 1 increment by 1 no cycle;
end

if not exists (select 1 from sys.sequences where [name] = 'IdentitySourceId')
begin
	create sequence Client.IdentitySourceId as int start with 1 increment by 1 no cycle;
end

declare @removeAllSequences bit = 0;
if(@removeAllSequences = 1)
begin	
	drop sequence Client.AccountId;		
	drop sequence Client.PrimarySubscriptionId;	
	drop sequence Client.SubscriptionId;	
	drop sequence Client.IdentitySourceId;
end;
go

-- Population
declare @populateCatalogs bit = 1;
if(@populateCatalogs = 1)
begin	
	-- Catalogs
	-- AccountType
	insert into Client.AccountType(AccountTypeId, [Name]) values (1, 'Client');
	insert into Client.AccountType(AccountTypeId, [Name]) values (2, 'Partner');
	insert into Client.AccountType(AccountTypeId, [Name]) values (3, 'Referral');

	-- Archetype
	insert into Client.ClientArchetype(ArchetypeId, [Name]) values (1, 'Basic');
	insert into Client.ClientArchetype(ArchetypeId, [Name]) values (2, 'Segragated');
	insert into Client.ClientArchetype(ArchetypeId, [Name]) values (3, 'Var');
	insert into Client.ClientArchetype(ArchetypeId, [Name]) values (4, 'Hybrid');
	insert into Client.ClientArchetype(ArchetypeId, [Name]) values (5, 'Enterprise');

	-- DataLink
	insert into Client.DataLinkType(DataLinkTypeId, [Name]) values (1, 'Customization');
	insert into Client.DataLinkType(DataLinkTypeId, [Name]) values (2, 'Activity');	
end
go

-- Basic Archetype
declare @populateBasicArchetype bit = 1;
if(@populateBasicArchetype = 1)
begin
	begin try
		begin transaction

		-- Acount
		declare @accountId int;
		select @accountId = next value for Client.AccountId;
		
		insert into Client.Account(AccountId, [Name], SalesforceId, AccountType, Archetype) values (@accountId, 'Health Dialog', 'SF00001', 1 /* Client */, 1 /* Basic */);

		-- Primary Subscription
		declare @primarySubscriptionId int;
		select @primarySubscriptionId = next value for Client.PrimarySubscriptionId;

		insert into Client.PrimarySubscription(PrimarySubscriptionId, AccountId, [Name], OrganizationalUnit, ActivationDate) values (@primarySubscriptionId, @accountId, 'Health Dialog', 'Main', getutcdate());

		-- Identity Source
		insert into Client.IdentitySource(IdentitySourceId, PrimarySubscriptionId, [Name]) values (next value for Client.IdentitySourceId, @primarySubscriptionId, 'Healthwise Managed Directory');

		-- Secondary Subscriptions
		declare @hdProductionSubscriptionId int;
		select @hdProductionSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@hdProductionSubscriptionId, @primarySubscriptionId, 'Production');

		declare @hdTestSubscriptionId int;
		select @hdTestSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@hdTestSubscriptionId, @primarySubscriptionId, 'Test');

		-- Data Links
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@hdTestSubscriptionId, @hdProductionSubscriptionId, 1);

		commit;
	end try
	begin catch
		rollback;
		throw;
	end catch
end
go

-- Segregated Archetype
declare @populateSegregatedArchetype bit = 1;
if(@populateSegregatedArchetype = 1)
begin
	begin try
		begin transaction

		-- Acount
		declare @accountId int;
		select @accountId = next value for Client.AccountId;
		
		insert into Client.Account(AccountId, [Name], SalesforceId, AccountType, Archetype) values (@accountId, 'Iora', 'SF00002', 1 /* Client */, 2 /* Segregated */);

		-- Primary Subscription
		declare @primarySubscriptionId int;
		select @primarySubscriptionId = next value for Client.PrimarySubscriptionId;

		insert into Client.PrimarySubscription(PrimarySubscriptionId, AccountId, [Name], OrganizationalUnit, ActivationDate) values (@primarySubscriptionId, @accountId, 'Iora Health', 'Main', getutcdate());

		-- Identity Source
		insert into Client.IdentitySource(IdentitySourceId, PrimarySubscriptionId, [Name]) values (next value for Client.IdentitySourceId, @primarySubscriptionId, 'Iora IdP');

		-- Secondary Subscriptions
		declare @ioraPrimaryCareSubscriptionId int;
		select @ioraPrimaryCareSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@ioraPrimaryCareSubscriptionId, @primarySubscriptionId, 'Primary Care');

		declare @ioraPrimaryCareStagingSubscriptionId int;
		select @ioraPrimaryCareStagingSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@ioraPrimaryCareStagingSubscriptionId, @primarySubscriptionId, 'Primary Care - Staging');

		declare @ioraBehavioralSubscriptionId int;
		select @ioraBehavioralSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@ioraBehavioralSubscriptionId, @primarySubscriptionId, 'Behavioral Health');

		declare @ioraBehavioralStagingSubscriptionId int;
		select @ioraBehavioralStagingSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@ioraBehavioralStagingSubscriptionId, @primarySubscriptionId, 'Behavioral Health - Staging');

		-- Data Links
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@ioraBehavioralStagingSubscriptionId, @ioraPrimaryCareStagingSubscriptionId, 1);
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@ioraPrimaryCareStagingSubscriptionId, @ioraBehavioralStagingSubscriptionId, 1);

		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@ioraPrimaryCareSubscriptionId, @ioraBehavioralSubscriptionId, 2);
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@ioraBehavioralSubscriptionId, @ioraPrimaryCareSubscriptionId, 2);

		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@ioraPrimaryCareStagingSubscriptionId, @ioraPrimaryCareSubscriptionId, 1);
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@ioraBehavioralStagingSubscriptionId, @ioraBehavioralSubscriptionId, 1);

		commit;
	end try
	begin catch
		rollback;
		throw;
	end catch
end
go

-- VAR Archetype
declare @populateVarArchetype bit = 1;
if(@populateVarArchetype = 1)
begin
	begin try
		begin transaction

		-- Acount
		declare @accountId int;
		select @accountId = next value for Client.AccountId;
		
		insert into Client.Account(AccountId, [Name], SalesforceId, AccountType, Archetype) values (@accountId, 'EClinicalWorks', 'SF00003', 1 /* Client */, 3 /* Var */);

		-- Primary Subscription 1
		declare @primarySubscription1Id int;
		select @primarySubscription1Id = next value for Client.PrimarySubscriptionId;

		insert into Client.PrimarySubscription(PrimarySubscriptionId, AccountId, [Name], OrganizationalUnit, ActivationDate) values (@primarySubscription1Id, @accountId, 'ECW', 'Open Door Medical Center', getutcdate());

		-- Identity Source
		insert into Client.IdentitySource(IdentitySourceId, PrimarySubscriptionId, [Name]) values (next value for Client.IdentitySourceId, @primarySubscription1Id, 'Open Door IdP');

		-- Primary Subscription 2
		declare @primarySubscription2Id int;
		select @primarySubscription2Id = next value for Client.PrimarySubscriptionId;

		insert into Client.PrimarySubscription(PrimarySubscriptionId, AccountId, [Name], OrganizationalUnit, ActivationDate) values (@primarySubscription2Id, @accountId, 'ECW', 'Prime Care Family Practice', getutcdate());

		-- Identity Source
		insert into Client.IdentitySource(IdentitySourceId, PrimarySubscriptionId, [Name]) values (next value for Client.IdentitySourceId, @primarySubscription2Id, 'Prime Care IdP');

		-- Primary Subscription 3
		declare @primarySubscription3Id int;
		select @primarySubscription3Id = next value for Client.PrimarySubscriptionId;

		insert into Client.PrimarySubscription(PrimarySubscriptionId, AccountId, [Name], OrganizationalUnit, ActivationDate) values (@primarySubscription3Id, @accountId, 'ECW', 'Arkansas Heart Hospital', getutcdate());

		-- Identity Source
		insert into Client.IdentitySource(IdentitySourceId, PrimarySubscriptionId, [Name]) values (next value for Client.IdentitySourceId, @primarySubscription3Id, 'AKHH IdP');

		-- Secondary Subscriptions
		declare @secondarySubscriprion1Id int;
		select @secondarySubscriprion1Id = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@secondarySubscriprion1Id, @primarySubscription1Id, 'Open Door');

		declare @secondaryStagingSubscriprion1Id int;
		select @secondaryStagingSubscriprion1Id = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@secondaryStagingSubscriprion1Id, @primarySubscription1Id, 'Open Door - Staging');		

		declare @secondarySubscriprion2Id int;
		select @secondarySubscriprion2Id = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@secondarySubscriprion2Id, @primarySubscription2Id, 'Primary Care');

		declare @secondaryStagingSubscriprion2Id int;
		select @secondaryStagingSubscriprion2Id = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@secondaryStagingSubscriprion2Id, @primarySubscription2Id, 'Primary Care - Staging');

		declare @secondarySubscriprion3Id int;
		select @secondarySubscriprion3Id = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@secondarySubscriprion3Id, @primarySubscription3Id, 'Arkansas Heart');

		declare @secondaryStagingSubscriprion3Id int;
		select @secondaryStagingSubscriprion3Id = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@secondaryStagingSubscriprion3Id, @primarySubscription3Id, 'Arkansas Heart - Staging');

		-- Data Links
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@secondaryStagingSubscriprion1Id, @secondarySubscriprion1Id, 1);
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@secondaryStagingSubscriprion2Id, @secondarySubscriprion2Id, 1);
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@secondaryStagingSubscriprion3Id, @secondarySubscriprion3Id, 1);

		commit;
	end try
	begin catch
		rollback;
		throw;
	end catch
end
go

-- Hybrid Archetype
declare @populateHybridArchetype bit = 1;
if(@populateHybridArchetype = 1)
begin
	begin try
		begin transaction

		-- Acount
		declare @accountId int;
		select @accountId = next value for Client.AccountId;
		
		insert into Client.Account(AccountId, [Name], SalesforceId, AccountType, Archetype) values (@accountId, 'Lumeris', 'SF00004', 1 /* Client */, 4 /* Hybrid */);

		-- Primary Subscription 1
		declare @lumerisPrimarySubscriptionId int;
		select @lumerisPrimarySubscriptionId = next value for Client.PrimarySubscriptionId;

		insert into Client.PrimarySubscription(PrimarySubscriptionId, AccountId, [Name], OrganizationalUnit, ActivationDate) values (@lumerisPrimarySubscriptionId, @accountId, 'Lumeris', 'Main', getutcdate());

		-- Identity Source
		insert into Client.IdentitySource(IdentitySourceId, PrimarySubscriptionId, [Name]) values (next value for Client.IdentitySourceId, @lumerisPrimarySubscriptionId, 'Lumeris IdP');

		-- Primary Subscription 2
		declare @tenetPrimarySubscriptionId int;
		select @tenetPrimarySubscriptionId = next value for Client.PrimarySubscriptionId;

		insert into Client.PrimarySubscription(PrimarySubscriptionId, AccountId, [Name], OrganizationalUnit, ActivationDate) values (@tenetPrimarySubscriptionId, @accountId, 'Tenet Healthcare', 'Tenet', getutcdate());

		-- Identity Source
		insert into Client.IdentitySource(IdentitySourceId, PrimarySubscriptionId, [Name]) values (next value for Client.IdentitySourceId, @tenetPrimarySubscriptionId, 'Tenet IdP');

		-- Primary Subscription 3
		declare @aramarkPrimarySubscriptionId int;
		select @aramarkPrimarySubscriptionId = next value for Client.PrimarySubscriptionId;

		insert into Client.PrimarySubscription(PrimarySubscriptionId, AccountId, [Name], OrganizationalUnit, ActivationDate) values (@aramarkPrimarySubscriptionId, @accountId, 'Aramark Corporation', 'Aramark', getutcdate());

		-- Identity Source
		insert into Client.IdentitySource(IdentitySourceId, PrimarySubscriptionId, [Name]) values (next value for Client.IdentitySourceId, @aramarkPrimarySubscriptionId, 'Aramark IdP');
		
		-- Secondary Subscriptions
		declare @bhcSubscriptionId int;
		select @bhcSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@bhcSubscriptionId, @lumerisPrimarySubscriptionId, 'Behavioral Health Clinic');

		declare @bhcStagingSubscriptionId int;
		select @bhcStagingSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@bhcStagingSubscriptionId, @lumerisPrimarySubscriptionId, 'Behavioral Health Clinic - Staging');		

		declare @ucSubscriptionId int;
		select @ucSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@ucSubscriptionId, @lumerisPrimarySubscriptionId, 'Urology Specialty Clinic');

		declare @ucStagingSubscriptionId int;
		select @ucStagingSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@ucStagingSubscriptionId, @lumerisPrimarySubscriptionId, 'Urology Specialty Clinic - Staging');

		declare @tenetSubscriptionId int;
		select @tenetSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@tenetSubscriptionId, @tenetPrimarySubscriptionId, 'Tenet');

		declare @tenetStagingSubscriptionId int;
		select @tenetStagingSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@tenetStagingSubscriptionId, @tenetPrimarySubscriptionId, 'Tenet - Staging');

		declare @aramarkSubscriptionId int;
		select @aramarkSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@aramarkSubscriptionId, @aramarkPrimarySubscriptionId, 'Aramark');

		declare @aramarkStagingSubscriptionId int;
		select @aramarkStagingSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@aramarkStagingSubscriptionId, @aramarkPrimarySubscriptionId, 'Aramark - Staging');
		
		-- Data Links
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@bhcStagingSubscriptionId, @bhcSubscriptionId, 1);
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@ucStagingSubscriptionId, @ucSubscriptionId, 1);
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@ucStagingSubscriptionId, @bhcStagingSubscriptionId, 1);
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@bhcStagingSubscriptionId, @ucStagingSubscriptionId, 1);

		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@tenetStagingSubscriptionId, @tenetSubscriptionId, 1);
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@aramarkStagingSubscriptionId, @aramarkSubscriptionId, 1);

		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@bhcSubscriptionId, @ucSubscriptionId, 2);
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@ucSubscriptionId, @bhcSubscriptionId, 2);			
		commit;
	end try
	begin catch
		rollback;
		throw;
	end catch
end
go

-- Enterprise Archetype
declare @populateEnterpriseArchetype bit = 1;
if(@populateEnterpriseArchetype = 1)
begin
	begin try
		begin transaction

		-- Acount
		declare @accountId int;
		select @accountId = next value for Client.AccountId;
		
		insert into Client.Account(AccountId, [Name], SalesforceId, AccountType, Archetype) values (@accountId, 'Trinity', 'SF00005', 1 /* Client */, 5 /* Enterprise */);

		-- Primary Subscription 1
		declare @caPrimarySubscriptionId int;
		select @caPrimarySubscriptionId = next value for Client.PrimarySubscriptionId;

		insert into Client.PrimarySubscription(PrimarySubscriptionId, AccountId, [Name], OrganizationalUnit, ActivationDate) values (@caPrimarySubscriptionId, @accountId, 'Trinity California', 'California', getutcdate());

		-- Identity Source
		insert into Client.IdentitySource(IdentitySourceId, PrimarySubscriptionId, [Name]) values (next value for Client.IdentitySourceId, @caPrimarySubscriptionId, 'Trinity IdP');

		-- Primary Subscription 2
		declare @newEnglandPrimarySubscriptionId int;
		select @newEnglandPrimarySubscriptionId = next value for Client.PrimarySubscriptionId;

		insert into Client.PrimarySubscription(PrimarySubscriptionId, AccountId, [Name], OrganizationalUnit, ActivationDate) values (@newEnglandPrimarySubscriptionId, @accountId, 'Trinity New England', 'New England', getutcdate());

		-- Identity Source
		insert into Client.IdentitySource(IdentitySourceId, PrimarySubscriptionId, [Name]) values (next value for Client.IdentitySourceId, @newEnglandPrimarySubscriptionId, 'Trinity IdP');

		-- Primary Subscription 3
		declare @nyPrimarySubscriptionId int;
		select @nyPrimarySubscriptionId = next value for Client.PrimarySubscriptionId;

		insert into Client.PrimarySubscription(PrimarySubscriptionId, AccountId, [Name], OrganizationalUnit, ActivationDate) values (@nyPrimarySubscriptionId, @accountId, 'Trinity New York', 'New York', getutcdate());

		-- Identity Source
		insert into Client.IdentitySource(IdentitySourceId, PrimarySubscriptionId, [Name]) values (next value for Client.IdentitySourceId, @nyPrimarySubscriptionId, 'Trinity IdP');

		-- Saint Agnes Hospital Newtwork
		declare @samcCaliforniaSubscriptionId int;
		select @samcCaliforniaSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@samcCaliforniaSubscriptionId, @caPrimarySubscriptionId, 'Saint Agnes Medical Center');

		declare @sahcCaliforniaSubscriptionId int;
		select @sahcCaliforniaSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@sahcCaliforniaSubscriptionId, @caPrimarySubscriptionId, 'Saint Agnes Home Care and Hospice');

		-- Trinity and Mercy in Connecticut and Massachusetts
		declare @trinityNewEnglandSubscriptionId int;
		select @trinityNewEnglandSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@trinityNewEnglandSubscriptionId, @newEnglandPrimarySubscriptionId, 'Trinity Health of New England');

		declare @sfNewEnglandSubscriptionId int;
		select @sfNewEnglandSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@sfNewEnglandSubscriptionId, @newEnglandPrimarySubscriptionId, 'Saint Francis Hospital and Medical Center');

		declare @msNewEnglandSubscriptionId int;
		select @msNewEnglandSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@msNewEnglandSubscriptionId, @newEnglandPrimarySubscriptionId, 'Mount Sinai Rehabilitation Hospital');

		declare @smNewEnglandSubscriptionId int;
		select @smNewEnglandSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@smNewEnglandSubscriptionId, @newEnglandPrimarySubscriptionId, 'Saint Mary''s Hospital');

		declare @smFamilyNewEnglandSubscriptionId int;
		select @smFamilyNewEnglandSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@smFamilyNewEnglandSubscriptionId, @newEnglandPrimarySubscriptionId, 'Saint Mary''s Family Care Clinic');

		declare @smCardioNewEnglandSubscriptionId int;
		select @smCardioNewEnglandSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@smCardioNewEnglandSubscriptionId, @newEnglandPrimarySubscriptionId, 'Saint Mary''s Cardiology Clinic');

		-- Trinity in New York
		declare @mhNewYorkSubscriptionId int;
		select @mhNewYorkSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@mhNewYorkSubscriptionId, @nyPrimarySubscriptionId, 'Mercy Hospital of Buffalo');

		declare @msmNewYorkSubscriptionId int;
		select @msmNewYorkSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@msmNewYorkSubscriptionId, @nyPrimarySubscriptionId, 'Mount St. Mary''s Hospital');

		declare @sjhNewYorkSubscriptionId int;
		select @sjhNewYorkSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@sjhNewYorkSubscriptionId, @nyPrimarySubscriptionId, 'St. Joseph''s Hospital Health Center');

		declare @spNewYorkSubscriptionId int;
		select @spNewYorkSubscriptionId = next value for Client.SubscriptionId;

		insert into Client.Subscription(SubscriptionId, PrimarySubscriptionId, [Name]) values (@spNewYorkSubscriptionId, @nyPrimarySubscriptionId, 'St. Peter''s Health Care Services');

		-- Subscription Paths
		-- Saint Agnes Network
		insert into Client.SubscriptionPath(AncestorSubscriptionId, DescendantSubscriptionId) values (@saCaliforniaSubscriptionId, @saCaliforniaSubscriptionId);
		insert into Client.SubscriptionPath(AncestorSubscriptionId, DescendantSubscriptionId) values (@samcCaliforniaSubscriptionId, @samcCaliforniaSubscriptionId);
		insert into Client.SubscriptionPath(AncestorSubscriptionId, DescendantSubscriptionId) values (@sahcCaliforniaSubscriptionId, @sahcCaliforniaSubscriptionId);

		insert into Client.SubscriptionPath(AncestorSubscriptionId, DescendantSubscriptionId) values (@saCaliforniaSubscriptionId, @samcCaliforniaSubscriptionId);
		insert into Client.SubscriptionPath(AncestorSubscriptionId, DescendantSubscriptionId) values (@saCaliforniaSubscriptionId, @sahcCaliforniaSubscriptionId);		

		-- Trinity Health of New England
		insert into Client.SubscriptionPath(AncestorSubscriptionId, DescendantSubscriptionId) values (@trinityNewEnglandSubscriptionId, @trinityNewEnglandSubscriptionId);
		insert into Client.SubscriptionPath(AncestorSubscriptionId, DescendantSubscriptionId) values (@sfNewEnglandSubscriptionId, @sfNewEnglandSubscriptionId);
		insert into Client.SubscriptionPath(AncestorSubscriptionId, DescendantSubscriptionId) values (@msNewEnglandSubscriptionId, @msNewEnglandSubscriptionId);
		insert into Client.SubscriptionPath(AncestorSubscriptionId, DescendantSubscriptionId) values (@smNewEnglandSubscriptionId, @smNewEnglandSubscriptionId);
		insert into Client.SubscriptionPath(AncestorSubscriptionId, DescendantSubscriptionId) values (@smFamilyNewEnglandSubscriptionId, @smFamilyNewEnglandSubscriptionId);
		insert into Client.SubscriptionPath(AncestorSubscriptionId, DescendantSubscriptionId) values (@smCardioNewEnglandSubscriptionId, @smCardioNewEnglandSubscriptionId);
	
		insert into Client.SubscriptionPath(AncestorSubscriptionId, DescendantSubscriptionId) values (@trinityNewEnglandSubscriptionId, @sfNewEnglandSubscriptionId);
		insert into Client.SubscriptionPath(AncestorSubscriptionId, DescendantSubscriptionId) values (@trinityNewEnglandSubscriptionId, @msNewEnglandSubscriptionId);
		insert into Client.SubscriptionPath(AncestorSubscriptionId, DescendantSubscriptionId) values (@trinityNewEnglandSubscriptionId, @smNewEnglandSubscriptionId);

		insert into Client.SubscriptionPath(AncestorSubscriptionId, DescendantSubscriptionId) values (@trinityNewEnglandSubscriptionId, @smFamilyNewEnglandSubscriptionId);
		insert into Client.SubscriptionPath(AncestorSubscriptionId, DescendantSubscriptionId) values (@smNewEnglandSubscriptionId, @smFamilyNewEnglandSubscriptionId);
		insert into Client.SubscriptionPath(AncestorSubscriptionId, DescendantSubscriptionId) values (@trinityNewEnglandSubscriptionId, @smCardioNewEnglandSubscriptionId);
		insert into Client.SubscriptionPath(AncestorSubscriptionId, DescendantSubscriptionId) values (@smNewEnglandSubscriptionId, @smCardioNewEnglandSubscriptionId);
		
		-- Trinity New York
		insert into Client.SubscriptionPath(AncestorSubscriptionId, DescendantSubscriptionId) values (@mhNewYorkSubscriptionId, @mhNewYorkSubscriptionId);
		insert into Client.SubscriptionPath(AncestorSubscriptionId, DescendantSubscriptionId) values (@msmNewYorkSubscriptionId, @msmNewYorkSubscriptionId);
		insert into Client.SubscriptionPath(AncestorSubscriptionId, DescendantSubscriptionId) values (@sjhNewYorkSubscriptionId, @sjhNewYorkSubscriptionId);
		insert into Client.SubscriptionPath(AncestorSubscriptionId, DescendantSubscriptionId) values (@spNewYorkSubscriptionId, @spNewYorkSubscriptionId);

		-- Data Links
		-- Saint Agnes Customization
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@saCaliforniaSubscriptionId, @samcCaliforniaSubscriptionId, 1);
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@saCaliforniaSubscriptionId, @sahcCaliforniaSubscriptionId, 1);

		-- Saint Agnes Activity
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@samcCaliforniaSubscriptionId, @saCaliforniaSubscriptionId, 2);
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@sahcCaliforniaSubscriptionId, @saCaliforniaSubscriptionId, 2);
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@sahcCaliforniaSubscriptionId, @samcCaliforniaSubscriptionId, 2);
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@samcCaliforniaSubscriptionId, @sahcCaliforniaSubscriptionId, 2);
	
		-- Saint Mary's Customization
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@smNewEnglandSubscriptionId, @smFamilyNewEnglandSubscriptionId, 1);
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@smNewEnglandSubscriptionId, @smCardioNewEnglandSubscriptionId, 1);

		-- Saint Mary's Activity
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@smNewEnglandSubscriptionId, @smFamilyNewEnglandSubscriptionId, 2);
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@smNewEnglandSubscriptionId, @smCardioNewEnglandSubscriptionId, 2);
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@smFamilyNewEnglandSubscriptionId, @smCardioNewEnglandSubscriptionId, 2);
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@smCardioNewEnglandSubscriptionId, @smFamilyNewEnglandSubscriptionId, 2);

		commit;
	end try
	begin catch
		rollback;
		throw;
	end catch
end
go


/* Questions for the model */
/* ####################### */

/* How many unique clients */
select count(*) as Clients from Client.Account;

/* How many clients per Account Type */
select
	t.[Name] as AccountType,
	count(*) as Clients 
from Client.Account a
	inner join Client.AccountType t on a.AccountType = t.AccountTypeId
group by t.[Name];

/* How many clients per archetype */
select
	t.[Name] as Archetype,
	count(*) as Clients 
from Client.Account a
	inner join Client.ClientArchetype t on a.Archetype = t.ArchetypeId
group by t.[Name];

/* All subscriptions of a given client */
select 
	s.[Name] as Subscription
from Client.Subscription s
where s.PrimarySubscriptionId in (
	select PrimarySubscriptionId 
	from Client.PrimarySubscription p 
		inner join Client.Account a on p.AccountId = a.AccountId
	where a.[Name] = 'EClinicalWorks'
);

/* What are all the defined DataLinks of a given client */
select
	s1.SubscriptionId as FromSubscriptionId,
	s1.[Name] as FromSubscription,
	s2.SubscriptionId as ToSubscriptionId,
	s2.[Name] as ToSubscription,
	t.[Name] as DataLinkType
from Client.DataLink l	
	inner join Client.Subscription s1 on l.FromSubscriptionId = s1.SubscriptionId
	inner join Client.PrimarySubscription p1 on s1.PrimarySubscriptionId = p1.PrimarySubscriptionId
	inner join Client.Subscription s2 on l.ToSubscriptionId = s2.SubscriptionId
	inner join Client.PrimarySubscription p2 on s2.PrimarySubscriptionId = p2.PrimarySubscriptionId
	inner join Client.DataLinkType t on l.[Type] = t.DataLinkTypeId
where p1.AccountId = p2.AccountId
	and p2.AccountId in (
		select AccountId
		from Client.Account a
		where a.[Name] = 'Trinity'
	)
order by t.[Name]

/* What are all the descendants of Trinity Health of New England? */
select	
	a.[Name] as Client,
	ps.[Name] as [Primary],
	ps.ActivationDate,
	s1.SubscriptionId,
	s1.[Name] as Descentand,
	s1.[Enabled]	
from Client.SubscriptionPath p
	inner join Client.Subscription s1 on p.DescendantSubscriptionId = s1.SubscriptionId
	inner join Client.PrimarySubscription ps on s1.PrimarySubscriptionId = ps.PrimarySubscriptionId
	inner join Client.Account a on ps.AccountId = a.AccountId
where AncestorSubscriptionId in
(
	select SubscriptionId 
	from Client.Subscription
	where [Name] = 'Trinity Health of New England'
)
order by a.AccountId, ps.PrimarySubscriptionId, s1.SubscriptionId;

if not exists (select 1 from sys.schemas where [name] = 'Clinical')
	 exec('CREATE SCHEMA [Clinical]');
go

