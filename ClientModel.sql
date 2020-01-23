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

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'Archetype' and TABLE_SCHEMA = 'Client')
	create table Client.Archetype 
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
		constraint FK_Account_ClientArchetype foreign key (Archetype) references Client.Archetype(ArchetypeId)
	);

	create index IX_Account_AccountType on Client.Account(AccountType);
	create index IX_Account_ClientArchetype on Client.Account(Archetype);
end
go

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'SubscriptionType' and TABLE_SCHEMA = 'Client')
begin
	create table Client.SubscriptionType 
	(
		SubscriptionTypeId tinyint not null,			
		[Name] varchar(120) not null,		
		CreatedDate datetime2(2) not null constraint DF_SubscriptionType_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_SubscriptionType_CreatedBy default system_user,
		constraint PK_SubscriptionType primary key clustered (SubscriptionTypeId)
	);

	create index UQ_SubscriptionType_Name on Client.SubscriptionType([Name]);
end
go

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'IdentityProvider' and TABLE_SCHEMA = 'Client')
begin
	create table Client.IdentityProvider 
	(
		IdentityProviderId int not null,
		[Name] varchar(120) not null,
		CreatedDate datetime2(2) not null constraint DF_IdentityProvider_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_IdentityProvider_CreatedBy default system_user,
		constraint PK_IdentityProvider primary key clustered (IdentityProviderId)
	);
end
go

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'Subscription' and TABLE_SCHEMA = 'Client')
begin
	create table Client.Subscription 
	(
		SubscriptionId int not null,	
		AccountId int not null,
		[Name] varchar(120) not null,
		[OrganizationalUnit] nvarchar(120),
		SubscriptionType tinyint not null,
		ActivationDate datetime2(2) not null,
		[Enabled] bit not null constraint DF_Subscription_Enabled default 1,
		CreatedDate datetime2(2) not null constraint DF_Subscription_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_Subscription_CreatedBy default system_user,
		constraint PK_Subscription primary key clustered (SubscriptionId),
		constraint FK_Subscription_Account foreign key (AccountId) references Client.Account(AccountId),
		constraint FK_Subscription_SubscriptionType foreign key (SubscriptionType) references Client.SubscriptionType(SubscriptionTypeId)
	);

	create index IX_Subscription_Account on Client.Subscription(AccountId);
	create index IX_Subscription_SubscriptionType on Client.Subscription(SubscriptionType);
end
go

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'IdentityProviderMapping' and TABLE_SCHEMA = 'Client')
begin
	create table Client.IdentityProviderMapping 
	(
		SubscriptionId int not null,
		IdentityProviderId int not null,
		[Enabled] bit not null constraint DF_IdentityProviderMapping_Enabled default 1,
		CreatedDate datetime2(2) not null constraint DF_IdentityProviderMapping_CreatedDate default getutcdate(),
		CreatedBy nvarchar(128) not null constraint DF_IdentityProviderMapping_CreatedBy default system_user,
		constraint PK_IdentityProviderMapping primary key clustered (SubscriptionId, IdentityProviderId),
		constraint FK_IdentityProviderMapping_Subscription foreign key (SubscriptionId) references Client.Subscription(SubscriptionId),
		constraint FK_IdentityProviderMapping_IdentityProvider foreign key (IdentityProviderId) references Client.IdentityProvider(IdentityProviderId),
	);

	create index IX_IdentityProviderMapping_Subscription on Client.IdentityProviderMapping(SubscriptionId);
	create index IX_IdentityProviderMapping_IdentityProvider on Client.IdentityProviderMapping(IdentityProviderId);
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
	drop table Client.IdentityProviderMapping;
	drop table Client.IdentityProvider;
	drop table Client.Subscription;
	drop table Client.SubscriptionType;
	drop table Client.Account;
	drop table Client.Archetype;
	drop table Client.AccountType;
end;
go

-- Sequences
if not exists (select 1 from sys.sequences where [name] = 'AccountId')
begin
	create sequence Client.AccountId as int start with 1 increment by 1 no cycle;
end

if not exists (select 1 from sys.sequences where [name] = 'SubscriptionId')
begin
	create sequence Client.SubscriptionId as int start with 1 increment by 1 no cycle;
end

if not exists (select 1 from sys.sequences where [name] = 'IdentityProviderId')
begin
	create sequence Client.IdentityProviderId as int start with 1 increment by 1 no cycle;
end

declare @removeAllSequences bit = 0;
if(@removeAllSequences = 1)
begin	
	drop sequence Client.AccountId;		
	drop sequence Client.SubscriptionId;	
	drop sequence Client.IdentityProviderId;
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
	insert into Client.Archetype(ArchetypeId, [Name]) values (1, 'Basic');
	insert into Client.Archetype(ArchetypeId, [Name]) values (2, 'Segragated');
	insert into Client.Archetype(ArchetypeId, [Name]) values (3, 'Var');
	insert into Client.Archetype(ArchetypeId, [Name]) values (4, 'Hybrid');
	insert into Client.Archetype(ArchetypeId, [Name]) values (5, 'Enterprise');

	-- Subscription Type
	insert into Client.SubscriptionType(SubscriptionTypeId, [Name]) values (1, 'Open');
	insert into Client.SubscriptionType(SubscriptionTypeId, [Name]) values (2, 'Closed');	

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
		declare @accountId int = next value for Client.AccountId;		
		insert into Client.Account(AccountId, [Name], SalesforceId, AccountType, Archetype) values (@accountId, 'Health Dialog', 'SF00001', 1 /* Client */, 1 /* Basic */);

		-- Identity Source
		declare @identityProviderId int = next value for Client.IdentityProviderId;
		insert into Client.IdentityProvider(IdentityProviderId, [Name]) values (@identityProviderId, 'Healthwise Managed Directory');

		-- Subscriptions
		declare @hdProductionSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@hdProductionSubscriptionId, @accountId, 'Health Dialog', 'Production', 1, getutcdate());

		declare @hdTestSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@hdTestSubscriptionId, @accountId, 'Health Dialog', 'Test', 1, getutcdate());

		-- IdentityProvider Mappings
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@hdProductionSubscriptionId, @identityProviderId);
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@hdTestSubscriptionId, @identityProviderId);

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
		declare @accountId int = next value for Client.AccountId;		
		insert into Client.Account(AccountId, [Name], SalesforceId, AccountType, Archetype) values (@accountId, 'Iora', 'SF00002', 1 /* Client */, 2 /* Segregated */);

		-- Identity Source
		declare @identityProviderId int = next value for Client.IdentityProviderId;
		insert into Client.IdentityProvider(IdentityProviderId, [Name]) values (@identityProviderId, 'Iora IdP');

		-- Subscriptions
		declare @ioraPrimaryCareSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@ioraPrimaryCareSubscriptionId, @accountId, 'Primary Care', 'Production', 1, getutcdate());

		declare @ioraPrimaryCareStagingSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@ioraPrimaryCareStagingSubscriptionId, @accountId, 'Primary Care', 'Staging', 1, getutcdate());

		declare @ioraBehavioralSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@ioraBehavioralSubscriptionId, @accountId, 'Behavioral Health', 'Production', 1, getutcdate());

		declare @ioraBehavioralStagingSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@ioraBehavioralStagingSubscriptionId, @accountId, 'Behavioral Health', 'Staging', 1, getutcdate());

		-- IdentityProvider Mappings
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@ioraPrimaryCareSubscriptionId, @identityProviderId);
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@ioraPrimaryCareStagingSubscriptionId, @identityProviderId);
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@ioraBehavioralSubscriptionId, @identityProviderId);
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@ioraBehavioralStagingSubscriptionId, @identityProviderId);

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
		declare @accountId int = next value for Client.AccountId;		
		insert into Client.Account(AccountId, [Name], SalesforceId, AccountType, Archetype) values (@accountId, 'EClinicalWorks', 'SF00003', 1 /* Client */, 3 /* Var */);

		-- Identity Source
		declare @openDoorIdentityProviderId int = next value for Client.IdentityProviderId;
		insert into Client.IdentityProvider(IdentityProviderId, [Name]) values (@openDoorIdentityProviderId, 'Open Door IdP');

		declare @primaryCareIdentityProviderId int = next value for Client.IdentityProviderId;
		insert into Client.IdentityProvider(IdentityProviderId, [Name]) values (@primaryCareIdentityProviderId, 'Prime Care IdP');

		declare @arkansasIdentityProviderId int = next value for Client.IdentityProviderId;
		insert into Client.IdentityProvider(IdentityProviderId, [Name]) values (@arkansasIdentityProviderId, 'AKHH IdP');

		-- Subscriptions
		declare @openDoorProductionSubscriptionId int  = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@openDoorProductionSubscriptionId, @accountId, 'Open Door', 'Production', 1, getutcdate());

		declare @openDoorStagingSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@openDoorStagingSubscriptionId, @accountId, 'Open Door', 'Staging', 1, getutcdate());		

		declare @primaryCareProductionSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@primaryCareProductionSubscriptionId, @accountId, 'Primary Care', 'Production', 1, getutcdate());

		declare @primaryCareStagingSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@primaryCareStagingSubscriptionId, @accountId, 'Primary Care', 'Staging', 1, getutcdate());

		declare @arkansasProductionSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@arkansasProductionSubscriptionId, @accountId, 'Arkansas Heart', 'Production', 1, getutcdate());

		declare @arkansasStagingSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@arkansasStagingSubscriptionId, @accountId, 'Arkansas Heart', 'Staging', 1, getutcdate());

		-- IdentityProvider Mappings
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@openDoorProductionSubscriptionId, @openDoorIdentityProviderId);
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@openDoorStagingSubscriptionId, @openDoorIdentityProviderId);

		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@primaryCareProductionSubscriptionId, @primaryCareIdentityProviderId);
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@primaryCareStagingSubscriptionId, @primaryCareIdentityProviderId);

		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@arkansasProductionSubscriptionId, @arkansasIdentityProviderId);
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@arkansasStagingSubscriptionId, @arkansasIdentityProviderId);

		-- Data Links
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@openDoorStagingSubscriptionId, @openDoorProductionSubscriptionId, 1);
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@primaryCareStagingSubscriptionId, @primaryCareProductionSubscriptionId, 1);
		insert into Client.DataLink(FromSubscriptionId, ToSubscriptionId, [Type]) values (@arkansasStagingSubscriptionId, @arkansasProductionSubscriptionId, 1);

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

		-- Identity Source

		declare @lumerisIdentityProviderId int = next value for Client.IdentityProviderId;
		insert into Client.IdentityProvider(IdentityProviderId, [Name]) values (@lumerisIdentityProviderId, 'Lumeris IdP');	
		
		declare @tenetIdentityProviderId int = next value for Client.IdentityProviderId;
		insert into Client.IdentityProvider(IdentityProviderId, [Name]) values (@tenetIdentityProviderId, 'Tenet IdP');

		declare @aramarkIdentityProviderId int = next value for Client.IdentityProviderId;
		insert into Client.IdentityProvider(IdentityProviderId, [Name]) values (@aramarkIdentityProviderId, 'Aramark IdP');
		
		-- Subscriptions
		declare @bhcSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@bhcSubscriptionId, @accountId, 'Behavioral Health Clinic', 'Production', 1, getutcdate());

		declare @bhcStagingSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@bhcStagingSubscriptionId, @accountId, 'Behavioral Health Clinic', 'Staging', 1, getutcdate());		

		declare @ucSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@ucSubscriptionId, @accountId, 'Urology Specialty Clinic', 'Production', 1, getutcdate());

		declare @ucStagingSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@ucStagingSubscriptionId, @accountId, 'Urology Specialty Clinic', 'Staging', 1, getutcdate());

		declare @tenetSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@tenetSubscriptionId, @accountId, 'Tenet', 'Production', 1, getutcdate());

		declare @tenetStagingSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@tenetStagingSubscriptionId, @accountId, 'Tenet', 'Staging', 1, getutcdate());

		declare @aramarkSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@aramarkSubscriptionId, @accountId, 'Aramark', 'Production', 1, getutcdate());

		declare @aramarkStagingSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@aramarkStagingSubscriptionId, @accountId, 'Aramark', 'Staging', 1, getutcdate());
		
		-- IdentityProvider Mappings
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@bhcSubscriptionId, @lumerisIdentityProviderId);
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@bhcStagingSubscriptionId, @lumerisIdentityProviderId);
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@ucSubscriptionId, @lumerisIdentityProviderId);
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@ucStagingSubscriptionId, @lumerisIdentityProviderId);

		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@tenetSubscriptionId, @tenetIdentityProviderId);
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@tenetStagingSubscriptionId, @tenetIdentityProviderId);

		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@aramarkSubscriptionId, @aramarkIdentityProviderId);
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@aramarkStagingSubscriptionId, @aramarkIdentityProviderId);

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
		declare @accountId int = next value for Client.AccountId;		
		insert into Client.Account(AccountId, [Name], SalesforceId, AccountType, Archetype) values (@accountId, 'Trinity', 'SF00005', 1 /* Client */, 5 /* Enterprise */);
		
		-- Identity Source
		declare @identityProviderId int = next value for Client.IdentityProviderId;
		insert into Client.IdentityProvider(IdentityProviderId, [Name]) values (@identityProviderId, 'Trinity IdP');

		-- Saint Agnes Hospital Newtwork
		declare @samcCaliforniaSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@samcCaliforniaSubscriptionId, @accountId, 'Saint Agnes Medical Center', 'Production', 1, getutcdate());

		declare @sahcCaliforniaSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@sahcCaliforniaSubscriptionId, @accountId, 'Saint Agnes Home Care and Hospice', 'Production', 1, getutcdate());

		-- Trinity and Mercy in Connecticut and Massachusetts
		declare @trinityNewEnglandSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@trinityNewEnglandSubscriptionId, @accountId, 'Trinity Health of New England', 'Production', 1, getutcdate());

		declare @sfNewEnglandSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@sfNewEnglandSubscriptionId, @accountId, 'Saint Francis Hospital and Medical Center', 'Production', 1, getutcdate());

		declare @msNewEnglandSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@msNewEnglandSubscriptionId, @accountId, 'Mount Sinai Rehabilitation Hospital', 'Production', 1, getutcdate());

		declare @smNewEnglandSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@smNewEnglandSubscriptionId, @accountId, 'Saint Mary''s Hospital', 'Production', 1, getutcdate());

		declare @smFamilyNewEnglandSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@smFamilyNewEnglandSubscriptionId, @accountId, 'Saint Mary''s Family Care Clinic', 'Production', 1, getutcdate());
		
		declare @smCardioNewEnglandSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@smCardioNewEnglandSubscriptionId, @accountId, 'Saint Mary''s Cardiology Clinic', 'Production', 1, getutcdate());

		-- Trinity in New York
		declare @mhNewYorkSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@mhNewYorkSubscriptionId, @accountId, 'Mercy Hospital of Buffalo', 'Production', 1, getutcdate());

		declare @msmNewYorkSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@msmNewYorkSubscriptionId, @accountId, 'Mount St. Mary''s Hospital', 'Production', 1, getutcdate());

		declare @sjhNewYorkSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@sjhNewYorkSubscriptionId, @accountId, 'St. Joseph''s Hospital Health Center', 'Production', 1, getutcdate());

		declare @spNewYorkSubscriptionId int = next value for Client.SubscriptionId;
		insert into Client.Subscription(SubscriptionId, AccountId, [Name], OrganizationalUnit, SubscriptionType, ActivationDate) values (@spNewYorkSubscriptionId, @accountId, 'St. Peter''s Health Care Services', 'Production', 1, getutcdate());		

		-- IdentityProvider Mappings
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@samcCaliforniaSubscriptionId, @identityProviderId);
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@sahcCaliforniaSubscriptionId, @identityProviderId);
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@trinityNewEnglandSubscriptionId, @identityProviderId);
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@sfNewEnglandSubscriptionId, @identityProviderId);
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@msNewEnglandSubscriptionId, @identityProviderId);
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@smNewEnglandSubscriptionId, @identityProviderId);
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@smFamilyNewEnglandSubscriptionId, @identityProviderId);
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@smCardioNewEnglandSubscriptionId, @identityProviderId);
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@mhNewYorkSubscriptionId, @identityProviderId);		
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@msmNewYorkSubscriptionId, @identityProviderId);
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@sjhNewYorkSubscriptionId, @identityProviderId);
		insert into Client.IdentityProviderMapping(SubscriptionId, IdentityProviderId) values(@spNewYorkSubscriptionId, @identityProviderId);

		-- Data Links		
		-- Saint Agnes Activity
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
	inner join Client.Archetype t on a.Archetype = t.ArchetypeId
group by t.[Name];

/* All subscriptions of a given client */
select 
	s.[Name] as Subscription,
	s.OrganizationalUnit
from Client.Subscription s
where s.SubscriptionId in (
	select SubscriptionId 
	from Client.Subscription s 
		inner join Client.Account a on s.AccountId = a.AccountId
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
	inner join Client.Subscription s2 on l.ToSubscriptionId = s2.SubscriptionId
	inner join Client.DataLinkType t on l.[Type] = t.DataLinkTypeId
where s1.AccountId = s2.AccountId
	and s2.AccountId in (
		select AccountId
		from Client.Account a
		where a.[Name] = 'Trinity'
	)
order by t.[Name]

/* How do the model can be represented as a document? */
/* Output JSON 'model' - nowhere near */
select 
	a.[Name] as [Account.Name],
	a.SalesforceId as [Account.SalesforceId],
	t.[Name] as [Account.AccountType],
	s.[Name] as [Account.Subscription.Name],
	s.OrganizationalUnit as [Account.Subscription.Organization],
	s.[Enabled] as [Account.Subscription.Enabled],
	s.ActivationDate as [Account.Subscription.ActivationDate],
	i.[Name] as [Account.Subscription.IdentityStore]	
from Client.Subscription s
	inner join Client.IdentityProviderMapping m on m.SubscriptionId = s.SubscriptionId
	inner join Client.IdentityProvider i on i.IdentityProviderId = m.IdentityProviderId
	inner join Client.Account a on s.AccountId = a.AccountId
	inner join Client.AccountType t on a.AccountType = t.AccountTypeId				
for json path, root('Subscriptions');

/* Output JSON 'model' - getting closer */
select 
	a.[Name] as [Account.Name],
	a.SalesforceId as [Account.SalesforceId],
	t.[Name] as [Account.AccountType],
	(
		select 
			s.[Name] as [Account.Subscription.Name],
			s.OrganizationalUnit as [Account.Subscription.Organization],
			s.[Enabled] as [Account.Subscription.Enabled],
			s.ActivationDate as [Account.Subscription.ActivationDate],
			i.[Name] as [Account.Subscription.IdentityStore]
		from Client.Subscription s
			inner join Client.IdentityProviderMapping m on m.SubscriptionId = s.SubscriptionId
			inner join Client.IdentityProvider i on i.IdentityProviderId = m.IdentityProviderId
		where s.AccountId = a.AccountId
		for json path
	) as [Account.Subscriptions]
from Client.Account a
	inner join Client.AccountType t on a.AccountType = t.AccountTypeId
for json path, root('Accounts');

/* Output JSON 'model' - got it */
select 
	a.[Name] as [Account.Name],
	a.SalesforceId as [Account.SalesforceId],
	t.[Name] as [Account.AccountType],
	(
		select 
			s.[Name] as [Account.Subscription.Name],
			s.OrganizationalUnit as [Account.Subscription.Organization],
			s.[Enabled] as [Account.Subscription.Enabled],
			s.ActivationDate as [Account.Subscription.ActivationDate],
			i.[Name] as [Account.Subscription.IdentityStore]
		from Client.Subscription s
			inner join Client.IdentityProviderMapping m on m.SubscriptionId = s.SubscriptionId
			inner join Client.IdentityProvider i on i.IdentityProviderId = m.IdentityProviderId
		where s.AccountId = a.AccountId
		for json path
	) as [Account.Subscriptions]
from Client.Account a
	inner join Client.AccountType t on a.AccountType = t.AccountTypeId
for json path, root('Accounts');