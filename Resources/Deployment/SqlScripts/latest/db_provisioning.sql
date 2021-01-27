SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

Declare @username nvarchar(128)
Declare @password nvarchar(128)
Declare @sqlstmt nvarchar(512)

SET @password = $(password)
SET @username = $(username)

IF NOT EXISTS (SELECT * FROM sys.sysusers WHERE name = @username)
BEGIN
	Set @sqlstmt	='CREATE USER '+@username +' WITH PASSWORD ='''+@password +''''
	Exec (@sqlstmt)
END

EXEC sp_addrolemember N'db_owner', @username
GO

-- Drop views
IF EXISTS (select * from sys.views tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'agent_subscriptions' AND sch.name = 'dbo')
BEGIN
DROP VIEW [dbo].[agent_subscriptions]
END
GO

IF EXISTS (select * from sys.views tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'agent_apiversions' AND sch.name = 'dbo')
BEGIN
DROP VIEW [dbo].[agent_apiversions]
END
GO

IF EXISTS (select * from sys.views tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'agent_amlworkspaces' AND sch.name = 'dbo')
BEGIN
DROP VIEW [dbo].[agent_amlworkspaces]
END
GO

-- Drop tables

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'WebhookWebhookParameters' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[WebhookWebhookParameters]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'WebhookParameters' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[WebhookParameters]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'AadSecrets' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[AadSecrets]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'AadSecretTmps' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[AadSecretTmps]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'SubscriptionCustomMeterUsages' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[SubscriptionCustomMeterUsages]
END
GO


IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'IpAddresses' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[IpAddresses]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'IpBlocks' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[IpBlocks]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'IpConfigs' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[IpConfigs]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'OfferParameters' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[OfferParameters]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'RestrictedUsers' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[RestrictedUsers]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'ArmTemplateArmTemplateParameters' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[ArmTemplateArmTemplateParameters]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'ArmTemplateParameters' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[ArmTemplateParameters]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'SubscriptionParameters' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[SubscriptionParameters]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'Subscriptions' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[Subscriptions]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'CustomMeterDimensions' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[CustomMeterDimensions]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'Plans' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[Plans]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'CustomMeters' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[CustomMeters]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'TelemetryDataConnectors' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[TelemetryDataConnectors]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'Webhooks' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[Webhooks]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'ArmTemplates' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[ArmTemplates]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'Offers' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[Offers]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'APISubscriptions' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[APISubscriptions]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'APIVersions' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[APIVersions]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'AIServicePlans' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[AIServicePlans]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'AIServices' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[AIServices]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'AMLWorkspaces' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[AMLWorkspaces]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'AIAgents' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[AIAgents]
END
GO

IF EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'Publishers' AND sch.name = 'dbo')
BEGIN
DROP TABLE [dbo].[Publishers]
END
GO


CREATE TABLE [dbo].[Offers](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[OfferName] [nvarchar](50) NOT NULL,
	[OfferAlias] [nvarchar](128) NOT NULL,
	[OfferVersion] [nvarchar](50) NOT NULL,
	[Owners] [nvarchar](512) NOT NULL,
	[HostSubscription] uniqueidentifier NOT NULL,
	[Status] [nvarchar](16) NOT NULL,
	[CreatedTime] [datetime2](7) NOT NULL,
	[LastUpdatedTime] [datetime2](7) NOT NULL,
	[DeletedTime] [datetime2](7),
	[ContainerName] [uniqueidentifier] NOT NULL,
	[ManualActivation] [bit],
	[ManualCompleteOperation] [bit],
	[AIServiceId] [bigint],
	[IsAzureMarketplaceOffer] [bit],
	PRIMARY KEY (Id)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[ArmTemplates](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[OfferId] [bigint] NOT NULL,
	[TemplateName] [nvarchar](128) NOT NULL,
	[TemplateFilePath] [nvarchar](1024) NOT NULL,
	PRIMARY KEY (Id),
	CONSTRAINT FK_offerId_armTemplates FOREIGN KEY (OfferId) REFERENCES offers(Id)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Webhooks](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[OfferId] [bigint] NOT NULL,
	[WebhookName] [nvarchar](128) NOT NULL,
	[WebhookUrl] [nvarchar](1024) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Webhooks]  WITH CHECK ADD  CONSTRAINT [FK_offerId_webhook] FOREIGN KEY([OfferId])
REFERENCES [dbo].[Offers] ([Id])
GO

ALTER TABLE [dbo].[Webhooks] CHECK CONSTRAINT [FK_offerId_webhook]
GO

CREATE TABLE [dbo].[Plans](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[OfferId] [bigint] NOT NULL,
	[PlanName] [nvarchar](50) NOT NULL,
	[DataRetentionInDays] [int] NOT NULL,
	[SubscribeArmTemplateId] bigint NULL,
	[UnsubscribeArmTemplateId] bigint NULL,
	[SuspendArmTemplateId] bigint NULL,
	[DeleteDataArmTemplateId] bigint NULL,
	[SubscribeWebhookId] bigint NULL,
	[UnsubscribeWebhookId] bigint NULL,
	[SuspendWebhookId] bigint NULL,
	[DeleteDataWebhookId] bigint NULL,
	[PriceModel] [nvarchar](16) NOT NULL,
	[MonthlyBase] [float] NULL,
	[AnnualBase] [float] NULL,
	[PrivatePlan] [bit] NOT NULL,
	PRIMARY KEY (Id),
    CONSTRAINT FK_offer_id_plans FOREIGN KEY (OfferId) REFERENCES offers(Id),
	CONSTRAINT FK_subscribeArmTemplateId_plans FOREIGN KEY (SubscribeArmTemplateId) REFERENCES ArmTemplates(Id),
	CONSTRAINT FK_unsubscribeArmTemplateId_plans FOREIGN KEY (UnsubscribeArmTemplateId) REFERENCES ArmTemplates(Id),
	CONSTRAINT FK_suspendArmTemplateId_plans FOREIGN KEY (SuspendArmTemplateId) REFERENCES ArmTemplates(Id),
	CONSTRAINT FK_deleteDataArmTemplateId_plans FOREIGN KEY (DeleteDataArmTemplateId) REFERENCES ArmTemplates(Id),
	CONSTRAINT FK_subscribeWebhookId_plans FOREIGN KEY (SubscribeWebhookId) REFERENCES Webhooks(Id),
	CONSTRAINT FK_unsubscribeWebhookId_plans FOREIGN KEY (UnsubscribeWebhookId) REFERENCES Webhooks(Id),
	CONSTRAINT FK_suspendWebhookId_plans FOREIGN KEY (SuspendWebhookId) REFERENCES Webhooks(Id),
	CONSTRAINT FK_deleteDataWebhookId_plans FOREIGN KEY (DeleteDataWebhookId) REFERENCES Webhooks(Id)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[OfferParameters](
	[Id] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[OfferId] [bigint] NOT NULL,
	[ParameterName] [nvarchar](128) NOT NULL,
	[DisplayName] [nvarchar](128) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[ValueType] [nvarchar](16) NOT NULL,
	[FromList] bit NOT NULL,
	[ValueList] [nvarchar](max),
	[Maximum] bigint,
	[Minimum] bigint,
    CONSTRAINT FK_offer_id_offer_parameters FOREIGN KEY (OfferId)
    REFERENCES Offers(Id) 
)

CREATE TABLE [dbo].[Subscriptions](
	[SubscriptionId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[PublisherId] [nvarchar](50) NULL,
	[OfferId] bigint NOT NULL,
	[PlanId] bigint NOT NULL,
	[Quantity] [int] NOT NULL,
	[BeneficiaryTenantId] [uniqueidentifier] NULL,
	[PurchaserTenantId] [uniqueidentifier] NULL,
	[Status] [nvarchar](128) NOT NULL,
	[IsTest] [bit] NULL,
	[AllowedCustomerOperationsMask] [int] NULL,
	[SessionMode] [nvarchar](128) NULL,
	[SandboxType] [nvarchar](128) NULL,
	[IsFreeTrial] [bit] NULL,
	[CreatedTime] [datetime2](7) NULL,
	[ActivatedTime] [datetime2](7) NULL,
	[LastUpdatedTime] [datetime2](7) NULL,
	[LastSuspendedTime] [datetime2](7) NULL,
	[UnsubscribedTime] [datetime2](7) NULL,
	[DataDeletedTime] [datetime2](7) NULL,
	[OperationId] [uniqueidentifier] NULL,
	[DeploymentName] [nvarchar](128) NULL,
	[DeploymentId] [uniqueidentifier] NULL,
	[ResourceGroup] [nvarchar](128) NULL,
	[Owner] [nvarchar](128) NULL,
	[ActivatedBy] [nvarchar](128) NULL,
	[LastException] [nvarchar](max) NULL,
	[ProvisioningStatus] [nvarchar](64) NULL,
	[ProvisioningType] [nvarchar](64) NULL,
	[RetryCount] int NULL,
	[EntryPointUrl] [nvarchar](1024) NULL,
	[AgentId] [uniqueidentifier] NULL,
	[BaseUrl] [nvarchar](1024) NULL,
	[PrimaryKeySecretName] [nvarchar](64) NULL,
	[SecondaryKeySecretName] [nvarchar](64) NULL,
	[AIServiceId] bigint NULL,
	[AIServicePlanId] bigint NULL,
	[GatewayId] bigint NULL,
	CONSTRAINT FK_offer_id_subscriptions FOREIGN KEY (OfferId) REFERENCES Offers(Id),
	CONSTRAINT FK_plan_id_subscriptions FOREIGN KEY (PlanId) REFERENCES Plans(Id)
	PRIMARY KEY CLUSTERED (
		[SubscriptionId] ASC
	)
	WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[SubscriptionParameters](
	[Id] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[SubscriptionId] uniqueidentifier NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Type] [nvarchar](16) NOT NULL,
	[Value] [nvarchar](max) NOT NULL,
    CONSTRAINT FK_subscription_id_subscription_parameters FOREIGN KEY (SubscriptionId)
    REFERENCES Subscriptions(SubscriptionId)
)
GO

CREATE TABLE [dbo].[IpConfigs](
	[Id] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Name] [nvarchar](50) NOT NULL,
	[IPsPerSub] int NOT NULL,
	[OfferId] bigint NOT NULL,
	CONSTRAINT FK_OfferId_IpConfigs FOREIGN KEY (OfferId)
    REFERENCES Offers(Id)
)

CREATE TABLE [dbo].[IpBlocks](
	[Id] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[CIDR] [nvarchar](32) NOT NULL,
	[IpConfigId] bigint NOT NULL,
	CONSTRAINT FK_IpConfigId_IpBlocks FOREIGN KEY (IpConfigId)
    REFERENCES IpConfigs(Id)
)

CREATE TABLE [dbo].[IpAddresses](
	[Id] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Value] [nvarchar](32) NOT NULL,
	[IsAvailable] bit NOT NULL,
	[IpBlockId] bigint NOT NULL,
	[SubscriptionId] [uniqueidentifier],
	CONSTRAINT FK_IpBlockId_IpAddresses FOREIGN KEY (IpBlockId) REFERENCES IpBlocks(id),
	CONSTRAINT FK_SubscriptionId_IpAddresses FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(SubscriptionId)
)

CREATE TABLE [dbo].[ArmTemplateParameters](
	[Id] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[OfferId] BIGINT NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Type] [nvarchar](16) NOT NULL,
	[Value] [nvarchar](max) NOT NULL,
    CONSTRAINT FK_offer_id_arm_templates_parameters FOREIGN KEY (OfferId)
    REFERENCES Offers(Id) 
)

CREATE TABLE [dbo].[ArmTemplateArmTemplateParameters](
	[ArmTemplateId] [bigint] NOT NULL,
	[ArmTemplateParameterId] [bigint] NOT NULL,
	PRIMARY KEY (ArmTemplateId, ArmTemplateParameterId),
	CONSTRAINT FK_ArmTemplateId_ArmTemplateArmTemplateParameters FOREIGN KEY (ArmTemplateId) REFERENCES ArmTemplates(Id),
	CONSTRAINT FK_ArmTemplateParameterId_ArmTemplateArmTemplateParameters FOREIGN KEY (ArmTemplateParameterId) REFERENCES ArmTemplateParameters(Id)
)

CREATE TABLE [dbo].[RestrictedUsers](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[PlanId] bigint NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[Description] [nchar](50) NULL,
	PRIMARY KEY (Id),
    CONSTRAINT FK_plan_id_restricted_users FOREIGN KEY (PlanId)
    REFERENCES Plans(Id)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[TelemetryDataConnectors](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](64) NOT NULL,
	[Type] [nvarchar](512) NOT NULL,
	[Configuration] [nvarchar](max) NOT NULL,
	PRIMARY KEY (Id)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[CustomMeters](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[OfferId] [bigint] NOT NULL,
	[MeterName] [nvarchar](50) NOT NULL,
	[TelemetryDataConnectorId] [bigint] NOT NULL,
	[TelemetryQuery] [nvarchar](max) NOT NULL,
	PRIMARY KEY (id),
    CONSTRAINT FK_telemetry_data_connector_id_custom_meters FOREIGN KEY (TelemetryDataConnectorId)
    REFERENCES TelemetryDataConnectors(Id)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[CustomMeterDimensions](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[MeterId] bigint NOT NULL,
	[PlanId] bigint NOT NULL,
	[MonthlyUnlimited] [bit] NULL,
	[AnnualUnlimited] [bit] NULL,
	[MonthlyQuantityIncludedInBase] [int] NULL,
	[AnnualQuantityIncludedInBase] [int] NULL,
	PRIMARY KEY (Id),
    CONSTRAINT FK_meter_id_custom_meter_dimensions FOREIGN KEY (MeterId)
    REFERENCES CustomMeters(Id),
	CONSTRAINT FK_plan_id_custom_meter_dimensions FOREIGN KEY (PlanId)
	REFERENCES Plans(Id)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[SubscriptionCustomMeterUsages](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[MeterId] bigint NOT NULL,
	[SubscriptionId] uniqueidentifier NOT NULL,
	[CreatedTime] [datetime2] NOT NULL,
	[LastUpdatedTime] [datetime2],
	[LastErrorReportedTime] [datetime2],
	[LastError] [nvarchar](max),
	[IsEnabled] [bit],
	[UnsubscribedTime] [datetime2],
	[EnabledTime] [datetime2],
	[DisabledTime] [datetime2],
	PRIMARY KEY (Id),
    CONSTRAINT FK_meter_id_subscription_custom_meter_usage FOREIGN KEY (MeterId)
    REFERENCES CustomMeters(Id),
	CONSTRAINT FK_subscription_id_subscription_custom_meter_usage FOREIGN KEY (SubscriptionId)
	REFERENCES Subscriptions(SubscriptionId)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[AadSecretTmps](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[OfferId] bigint NOT NULL,
	[Name] [nvarchar](64) NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[ApplicationId] [uniqueidentifier] NOT NULL,
	[ClientSecret] [nvarchar](64),
	PRIMARY KEY (Id),
    CONSTRAINT FK_offer_id_aad_secret_tmps FOREIGN KEY (OfferId)
    REFERENCES Offers(Id) 
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[AadSecrets](
	[Id] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[SecretType] [nvarchar](64) NOT NULL,
	[SecretName] [nvarchar](128) NOT NULL,
	[KeyVaultName] [nvarchar](128) NOT NULL,
	[OfferId] BIGINT NOT NULL,
    CONSTRAINT FK_offer_id_aad_secrets FOREIGN KEY (OfferId)
    REFERENCES Offers(Id)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[WebhookParameters](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[OfferId] [bigint] NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Value] [nvarchar](max) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[WebhookParameters]  WITH CHECK ADD  CONSTRAINT [FK_offer_id_webhook_parameters] FOREIGN KEY([OfferId])
REFERENCES [dbo].[Offers] ([Id])
GO

ALTER TABLE [dbo].[WebhookParameters] CHECK CONSTRAINT [FK_offer_id_webhook_parameters]
GO

CREATE TABLE [dbo].[WebhookWebhookParameters](
	[WebhookId] [bigint] NOT NULL,
	[WebhookParameterId] [bigint] NOT NULL,
	PRIMARY KEY (WebhookId, WebhookParameterId),
	CONSTRAINT FK_WebhookId_WebhookWebhookParameters FOREIGN KEY (WebhookId) REFERENCES Webhooks(Id),
	CONSTRAINT FK_WebhookParameterId_WebhookWebhookParameters FOREIGN KEY (WebhookParameterId) REFERENCES WebhookParameters(Id)
)
GO

CREATE TABLE [dbo].[AMLWorkspaces](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[WorkspaceName] [nvarchar](50) NOT NULL,
	[ResourceId] [nvarchar](max) NOT NULL,
	[AADApplicationId] [uniqueidentifier] NOT NULL,
	[AADTenantId] [uniqueidentifier] NULL,
	[AADApplicationSecretName] [nvarchar](128) NOT NULL,
	[Region] [nvarchar](32) NOT NULL,
	PRIMARY KEY (Id)
)
GO

CREATE TABLE [dbo].[AzureSynapseWorkspaces](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[WorkspaceName] [nvarchar](50) NOT NULL,
	[ResourceId] [nvarchar](max) NOT NULL,
	[AADApplicationId] [uniqueidentifier] NOT NULL,
	[AADTenantId] [uniqueidentifier] NULL,
	[AADApplicationSecretName] [nvarchar](128) NOT NULL,
	PRIMARY KEY (Id)
)
GO

CREATE TABLE [dbo].[AzureDatabricksWorkspaces](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[WorkspaceName] [nvarchar](50) NOT NULL,
	[ResourceId] [nvarchar](max) NOT NULL,
	[WorkspaceUrl] [nvarchar](max) NOT NULL,
	[AADApplicationId] [uniqueidentifier] NOT NULL,
	[AADTenantId] [uniqueidentifier] NULL,
	[AADApplicationSecretName] [nvarchar](128) NOT NULL,
	PRIMARY KEY (Id)
)
GO

CREATE TABLE [dbo].[GitRepos](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RepoName] [nvarchar](50) NOT NULL,
	[Type] [nvarchar](16) NOT NULL,
	[HttpUrl] [nvarchar](max) NOT NULL,
	[CommitHashOrBranch] [nvarchar](256) NOT NULL,
	[PersonalAccessTokenSecretName] [nvarchar](32) NOT NULL,
	PRIMARY KEY (Id)
)
GO

CREATE TABLE [dbo].[AIServices](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[DisplayName] [nvarchar](128) NULL,
	[AIServiceName] [nvarchar](50) NOT NULL,
	[Owner] [nvarchar](512) NOT NULL,
	[Description] [nvarchar](256) NOT NULL,
	[LogoImageUrl] [nvarchar](max) NULL,
	[DocumentationUrl] [nvarchar](max) NULL,
	[Tags] [nvarchar](max) NULL,
	[SaaSOfferName] [nvarchar](50) NULL,
	[SaaSOfferId] [bigint] NULL,
	[CreatedTime] [datetime2](7) NOT NULL,
	[LastUpdatedTime] [datetime2](7) NOT NULL,
	PRIMARY KEY (Id)
)
GO

CREATE TABLE [dbo].[AIServicePlans](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AIServiceId] [bigint] NOT NULL,
	[AIServicePlanName] [nvarchar](50) NOT NULL,
	[AIServicePlanDisplayName] [nvarchar](128) NOT NULL,
	[Description] [nvarchar](1024) NOT NULL,
	[PlanType] [nvarchar](32) NOT NULL,
	[CreatedTime] [datetime2](7) NOT NULL,
	[LastUpdatedTime] [datetime2](7) NOT NULL,
	PRIMARY KEY (Id),
	CONSTRAINT FK_AIServiceId_AIServicePlans FOREIGN KEY (AIServiceId) REFERENCES AIServices(Id)
)
GO

CREATE TABLE [dbo].[APIVersions](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AIServicePlanId] [bigint] NOT NULL,
	[VersionName] [nvarchar](50) NOT NULL,
	[AMLWorkspaceId] [bigint] NULL,
	[AzureDatabricksWorkspaceId] [bigint] NULL,
	[AzureSynapseWorkspaceId] [bigint] NULL,
	[GitRepoId] [bigint] NULL,
	[ModelName] [nvarchar](128) NULL,
	[ModelVersion] [int] NULL,
	[EndpointName] [nvarchar](128) NULL,
	[EndpointName] [nvarchar](64) NULL,
	[IsManualInputEndpoint] [bit] NULL,
	[EndpointUrl] [nvarchar](max) NULL,
	[EndpointSwaggerUrl] [nvarchar](max) NULL,
	[EndpointAuthType] [nvarchar](32) NULL,
	[EndpointAuthKey] [nvarchar](256) NULL,
	[EndpointAuthAddTo] [nvarchar](16) NULL,
	[EndpointAuthSecretName] [nvarchar](32) NULL,
	[EndpointAuthTenantId] [uniqueidentifier] NULL,
	[EndpointAuthClientId] [uniqueidentifier] NULL,
	[GitVersion] [nvarchar](256) NULL,
	[LinkedServiceType] [nvarchar](16) NULL,
	[RunConfigFile] [nvarchar](256) NULL,
	[IsUseDefaultRunConfig] [bit] NULL,
	[IsRunProjectOnManagedCompute] [bit] NULL,
	[LinkedServiceComputeTarget] [nvarchar](256) NULL,
	[AdvancedSettings] [nvarchar](max) NULL,
	[CreatedTime] [datetime2](7) NOT NULL,
	[LastUpdatedTime] [datetime2](7) NOT NULL,
	PRIMARY KEY (Id),
	CONSTRAINT FK_AIServicePlanId_APIVersions FOREIGN KEY (AIServicePlanId) REFERENCES AIServicePlans(Id)
)
GO

CREATE TABLE [dbo].[AMLPipelineEndpoints](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[APIVersionId] [bigint] NOT NULL,
	[PipelineEndpointName] [nvarchar](128) NULL,
	[PipelineEndpointId] [uniqueidentifier] NULL,
	PRIMARY KEY (Id),
	CONSTRAINT FK_APIVersionId_AMLPipelineEndpoints FOREIGN KEY (APIVersionId) REFERENCES APIVersions(Id)
)
GO

CREATE TABLE [dbo].[MLModels](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[APIVersionId] [bigint] NOT NULL,
	[ModelName] [nvarchar](128) NULL,
	[ModelDisplayName] [nvarchar](128) NULL,
	[ModelVersion] [nvarchar](16) NULL,
	PRIMARY KEY (Id),
	CONSTRAINT FK_APIVersionId_MLModels FOREIGN KEY (APIVersionId) REFERENCES APIVersions(Id)
)
GO

CREATE TABLE [dbo].[APISubscriptions](
	[SubscriptionId] [uniqueidentifier] NOT NULL,
	[DeploymentId] [bigint] NOT NULL,
	[Name] [nvarchar](64) NOT NULL,
	[Owner] [nvarchar](512) NOT NULL,
	[Status] [nvarchar](32) NULL,
	[BaseUrl] [nvarchar](max) NULL,
	[PrimaryKeySecretName] [nvarchar](64) NULL,
	[SecondaryKeySecretName] [nvarchar](64) NULL,
	[CreatedTime] [datetime2](7) NOT NULL,
	[LastUpdatedTime] [datetime2](7) NOT NULL,
	[AgentId] [uniqueidentifier] NULL,
	[HostType] [nvarchar](32) NULL,
	PRIMARY KEY (SubscriptionId),
	CONSTRAINT FK_DeploymentId_APISubscriptions FOREIGN KEY (DeploymentId) REFERENCES AIServicePlans(Id)
)
GO

CREATE TABLE [dbo].[Gateways](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[GatewayId] [uniqueidentifier] NOT NULL,
	[DisplayName] [nvarchar](256) NOT NULL,
	[EndpointUrl] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[Tags] [nvarchar](max) NOT NULL,
	[CreatedBy] [nvarchar](512) NOT NULL,
	[CreatedTime] [datetime2](7) NOT NULL,
	[LastUpdatedTime] [datetime2](7) NOT NULL,
	[IsPrivate] [bit] NOT NULL,
	PRIMARY KEY (Id)
)
GO

CREATE TABLE [AIServicePlanGateways](
	[AIServicePlanId] [bigint] NOT NULL,
	[GatewayId] [bigint] NOT NULL,
	PRIMARY KEY (AIServicePlanId, GatewayId),
	CONSTRAINT FK_AIServicePlanId_AIServicePlanGateways FOREIGN KEY (AIServicePlanId) REFERENCES AIServicePlans(Id),
	CONSTRAINT FK_GatewayId_AIServicePlanGateways FOREIGN KEY (GatewayId) REFERENCES Gateways(Id)
)
GO
	

CREATE TABLE [dbo].[Publishers](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[PublisherId] [uniqueidentifier] NOT NULL,
	[ControlPlaneUrl] [nvarchar](max) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[LandingPageUrl] [nvarchar](max) NOT NULL,
	[PublisherMicrosoftId] [nvarchar](256) NOT NULL,
	PRIMARY KEY (PublisherId)
)
GO

Declare @publisherId nvarchar(64)
Declare @controlPlaneUrl nvarchar(512)
Declare @publisherName nvarchar(256)
Declare @publisherMicrosoftId nvarchar(512)
Declare @landingPageUrl nvarchar(256)

SET @publisherId = $(publisherId)
SET @controlPlaneUrl = $(controlPlaneUrl)
SET @publisherName = $(publisherName)
SET @publisherMicrosoftId = $(publisherMicrosoftId)
SET @landingPageUrl = $(landingPageUrl)

INSERT INTO [dbo].[Publishers] VALUES(@publisherId, @controlPlaneUrl, @publisherName, @landingPageUrl, @publisherMicrosoftId)
GO

Declare @agentId nvarchar(64)
Declare @agentKeySecretName nvarchar(64)
SET @agentId = $(agentId)
SET @agentKeySecretName = $(agentKeySecretName)

INSERT INTO [dbo].[AIAgents] VALUES(@agentId, @agentKeySecretName, 'system', getutcdate(), getutcdate(), 1)
GO



CREATE VIEW [dbo].[vw_subscriptions]
AS
SELECT dbo.Subscriptions.SubscriptionId, dbo.Subscriptions.Name, dbo.Subscriptions.Status, dbo.Subscriptions.CreatedTime, dbo.Subscriptions.LastUpdatedTime, dbo.Subscriptions.primarykeysecretname, dbo.Subscriptions.secondarykeysecretname, dbo.Subscriptions.AIServicePlanId, dbo.Subscriptions.AIServiceId, dbo.AIServices.AIServiceName, dbo.Subscriptions.BaseUrl, 
          dbo.Subscriptions.Owner, '' as AIServicePlanname
FROM
          dbo.AIServices INNER JOIN
          dbo.Subscriptions ON dbo.AIServices.Id = dbo.Subscriptions.AIServiceId
GO


