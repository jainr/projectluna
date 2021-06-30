/****** Object:  Schema [partner]    Script Date: 4/29/2021 11:07:14 AM ******/
CREATE SCHEMA [partner]
GO
/****** Object:  Schema [publish]    Script Date: 4/29/2021 11:07:14 AM ******/
CREATE SCHEMA [publish]
GO
/****** Object:  Schema [rbac]    Script Date: 4/29/2021 11:07:14 AM ******/
CREATE SCHEMA [rbac]
GO
/****** Object:  Schema [routing]    Script Date: 4/29/2021 11:07:14 AM ******/
CREATE SCHEMA [routing]
GO
/****** Object:  Schema [gallery]    Script Date: 4/29/2021 11:07:14 AM ******/
CREATE SCHEMA [gallery]
GO
/****** Object:  Schema [provision]    Script Date: 4/29/2021 11:07:14 AM ******/
CREATE SCHEMA [provision]
GO
/****** Object:  Table [partner].[PartnerServices]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [partner].[PartnerServices](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[uniqueName] [nvarchar](128) NOT NULL,
	[displayName] [nvarchar](128) NOT NULL,
	[type] [nvarchar](128) NOT NULL,
	[description] [nvarchar](1024) NOT NULL,
	[configurationSecretName] [nvarchar](64) NOT NULL,
	[tags] [nvarchar](1024) NOT NULL,
	[createdTime] [datetime2](7) NOT NULL,
	[lastUpdatedTime] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [routing].[PartnerServices]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [routing].[PartnerServices]
AS
SELECT uniqueName, configurationSecretName, id, type
FROM   partner.PartnerServices
GO

/****** Object:  Table [publish].[AutomationWebhooks]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [publish].[AutomationWebhooks](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](128) NOT NULL,
	[description] [nvarchar](1024) NOT NULL,
	[webhookUrl] [nvarchar](1024) NOT NULL,
	[isEnabled] [bit] NOT NULL,
	[createdTime] [datetime2](7) NOT NULL,
	[lastUpdatedTime] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [publish].[AzureMarketplaceOffers]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [publish].[AzureMarketplaceOffers](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[marketplaceOfferId] [nvarchar](50) NOT NULL,
	[displayName] [nvarchar](128) NOT NULL,
	[description] [nvarchar](1024) NOT NULL,
	[status] [nvarchar](64) NOT NULL,
	[isManualActivation] [bit] NOT NULL,
	[createdTime] [datetime2](7) NOT NULL,
	[lastUpdatedTime] [datetime2](7) NOT NULL,
	[deletedTime] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [publish].[AzureMarketplacePlans]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [publish].[AzureMarketplacePlans](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[offerId] [bigint] NOT NULL,
	[marketplacePlanId] [nvarchar](50) NOT NULL,
	[description] [nvarchar](1024) NOT NULL,
	[isLocalDeployment] [bit] NOT NULL,
	[managementKitDownloadUrlSecretName] [nvarchar](64) NOT NULL,
	[createdTime] [datetime2](7) NOT NULL,
	[lastUpdatedTime] [datetime2](7) NOT NULL,
    CONSTRAINT FK_marketplace_offer_id_plans FOREIGN KEY (offerId) REFERENCES publish.AzureMarketplaceOffers(id),
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [publish].[LunaAPIs]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [publish].[LunaAPIs](
	[applicationName] [nvarchar](128) NOT NULL,
	[apiName] [nvarchar](128) NOT NULL,
	[apiType] [nvarchar](64) NOT NULL,
	[createdTime] [datetime2](7) NOT NULL,
	[lastUpdatedTime] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[applicationName] ASC,
	[apiName] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [publish].[LunaAPIVersions]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [publish].[LunaAPIVersions](
	[applicationName] [nvarchar](128) NOT NULL,
	[apiName] [nvarchar](128) NOT NULL,
	[versionName] [nvarchar](128) NOT NULL,
	[versionType] [nvarchar](64) NOT NULL,
	[createdTime] [datetime2](7) NOT NULL,
	[lastUpdatedTime] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[applicationName] ASC,
	[apiName] ASC,
	[versionName] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [publish].[LunaApplications]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [publish].[LunaApplications](
	[applicationName] [nvarchar](128) NOT NULL,
	[status] [nvarchar](64) NOT NULL,
	[createdTime] [datetime2](7) NOT NULL,
	[lastUpdatedTime] [datetime2](7) NULL,
	[lastPublishedTime] [datetime2](7) NULL,
	[PrimaryMasterKeySecretName] [nvarchar](64) NOT NULL,
	[SecondaryMasterKeySecretName] [nvarchar](64) NOT NULL,
	[OwnerUserId] [nvarchar](256) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[applicationName] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [publish].[ApplicationSnapshots]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [publish].[ApplicationSnapshots](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[snapshotId] [uniqueidentifier] NOT NULL,
	[lastAppliedEventId] [bigint] NOT NULL,
	[applicationName] [nvarchar](128) NOT NULL,
	[snapshotContent] [nvarchar](max) NOT NULL,
	[status] [nvarchar](64) NOT NULL,
	[tags] [nvarchar](1024) NOT NULL,
	[createdTime] [datetime2](7) NOT NULL,
	[deletedTime] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/****** Object:  Table [publish].[ApplicationEvents]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [publish].[ApplicationEvents](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[eventId] [uniqueidentifier] NOT NULL,
	[eventType] [nvarchar](64) NOT NULL,
	[resourceName] [nvarchar](128) NOT NULL,
	[eventContent] [nvarchar](max) NOT NULL,
	[createdBy] [nvarchar](128) NOT NULL,
	[tags] [nvarchar](1024) NOT NULL,
	[createdTime] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/****** Object:  Table [publish].[MarketplaceOfferSnapshots]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [publish].[MarketplaceOfferSnapshots](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[snapshotId] [uniqueidentifier] NOT NULL,
	[lastAppliedEventId] [bigint] NOT NULL,
	[offerId] [nvarchar](50) NOT NULL,
	[snapshotContent] [nvarchar](max) NOT NULL,
	[status] [nvarchar](64) NOT NULL,
	[tags] [nvarchar](1024) NULL,
	[createdTime] [datetime2](7) NOT NULL,
	[deletedTime] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/****** Object:  Table [publish].[MarketplaceOfferEvents]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [publish].[MarketplaceOfferEvents](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[eventId] [uniqueidentifier] NOT NULL,
	[eventType] [nvarchar](64) NOT NULL,
	[resourceName] [nvarchar](128) NOT NULL,
	[eventContent] [nvarchar](max) NOT NULL,
	[createdBy] [nvarchar](128) NOT NULL,
	[tags] [nvarchar](1024) NULL,
	[createdTime] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/****** Object:  Table [publish].[MarketplaceOffers]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [publish].[MarketplaceOffers](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[offerId] [nvarchar](50) NOT NULL,
	[status] [nvarchar](64) NOT NULL,
	[displayName] [nvarchar](128) NOT NULL,
	[description] [nvarchar](1024) NOT NULL,
	[createdTime] [datetime2](7) NOT NULL,
	[lastUpdatedTime] [datetime2](7) NOT NULL,
	[lastPublishedTime] [datetime2](7) NULL,
	[deletedTime] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [rbac].[ownerships]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [rbac].[ownerships](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[uid] [nvarchar](128) NOT NULL,
	[resourceId] [nvarchar](1024) NOT NULL,
	[createdTime] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [rbac].[roleassignments]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [rbac].[roleassignments](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[uid] [nvarchar](128) NOT NULL,
	[userName] [nvarchar](128) NULL,
	[role] [nvarchar](64) NOT NULL,
	[createdTime] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [routing].[PublishedAPIVersions]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [routing].[PublishedAPIVersions](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ApplicationName] [nvarchar](128) NULL,
	[APIName] [nvarchar](128) NULL,
	[APIType] [nvarchar](64) NULL,
	[VersionName] [nvarchar](128) NULL,
	[VersionType] [nvarchar](64) NULL,
	[VersionProperties] [nvarchar](max) NULL,
	[LastAppliedEventId] [bigint] NULL,
	[PrimaryMasterKeySecretName] [nvarchar](64) NULL,
	[SecondaryMasterKeySecretName] [nvarchar](64) NULL,
	[IsEnabled] [bit] NULL,
	[CreatedTime] [datetime2](7) NULL,
	[LastUpdatedTime] [datetime2](7) NULL,
 CONSTRAINT [PK_routing.PublishedAPIVersions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/****** Object:  Table [routing].[SubscriptionEvents]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [routing].[SubscriptionEvents](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[SubscriptionId] [nvarchar](128) NOT NULL,
	[LastAppliedEventId] [bigint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [gallery].[ApplicationPublishers]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gallery].[ApplicationPublishers](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](128) NOT NULL,
	[type] [nvarchar](64) NOT NULL,
	[displayName] [nvarchar](128) NOT NULL,
	[description] [nvarchar](1024) NOT NULL,
	[endpointUrl] [nvarchar](1024) NOT NULL,
	[websiteUrl] [nvarchar](1024) NOT NULL,
	[isEnabled] [bit] NOT NULL,
	[publisherKeySecretName] [nvarchar](64) NOT NULL,
	[createdTime] [datetime2](7) NOT NULL,
	[lastUpdatedTime] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [gallery].[PublishedLunaAppliations]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gallery].[PublishedLunaAppliations](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[UniqueName] [nvarchar](128) NOT NULL,
	[DisplayName] [nvarchar](128) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[LogoImageUrl] [nvarchar](1024) NOT NULL,
	[DocumentationUrl] [nvarchar](1024) NOT NULL,
	[Publisher] [nvarchar](128) NOT NULL,
	[Details] [nvarchar](max) NOT NULL,
	[LastAppliedEventId] [bigint] NULL,
	[Tags] [nvarchar](1024) NOT NULL,
	[CreatedTime] [datetime2](7) NULL,
	[LastUpdatedTime] [datetime2](7) NULL,
	[IsEnabled] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [gallery].[LunaApplicationSubscriptions]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gallery].[LunaApplicationSubscriptions](
	[SubscriptionId] [uniqueidentifier] NOT NULL,
	[SubscriptionName] [nvarchar](128) NOT NULL,
	[ApplicationName] [nvarchar](128) NOT NULL,
	[Status] [nvarchar](64) NOT NULL,
	[Notes] [nvarchar](1024) NOT NULL,
	[PrimaryKeySecretName] [nvarchar](64) NOT NULL,
	[SecondaryKeySecretName] [nvarchar](64) NOT NULL,
	[CreatedTime] [datetime2](7) NOT NULL,
	[LastUpdatedTime] [datetime2](7) NOT NULL,
	[UnsubscribedTime] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[SubscriptionId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [gallery].[LunaApplicationSubscriptionOwners]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gallery].[LunaApplicationSubscriptionOwners](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[SubscriptionId] [uniqueidentifier] NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[UserName] [nvarchar](128) NOT NULL,
	[CreatedTime] [datetime2](7) NOT NULL,
    CONSTRAINT FK_subscription_id_owners FOREIGN KEY (SubscriptionId) REFERENCES gallery.LunaApplicationSubscriptions(SubscriptionId),
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [gallery].[PublishedAzureMarketplacePlans]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gallery].[PublishedAzureMarketplacePlans](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[marketplaceOfferId] [nvarchar](50) NOT NULL,
	[marketplacePlanId] [nvarchar](50) NOT NULL,
	[offerDisplayName] [nvarchar](128) NOT NULL,
	[offerDescription] [nvarchar](1024) NOT NULL,
	[mode] [nvarchar](64) NOT NULL,
	[parameters] [nvarchar](max) NOT NULL,
	[LastAppliedEventId] [bigint] NULL,
	[IsEnabled] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [gallery].[AzureMarketplaceSubscriptions]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gallery].[AzureMarketplaceSubscriptions](
	[SubscriptionId] [uniqueidentifier] NOT NULL,
	[SubscriptionName] [nvarchar](50) NOT NULL,
	[OwnerId] [nvarchar](128) NOT NULL,
	[SaaSSubscriptionStatus] [nvarchar](64) NOT NULL,
	[OfferId] [nvarchar](50) NOT NULL,
	[PlanId] [nvarchar](50) NOT NULL,
	[PlanCreatedByEventId] [bigint] NOT NULL,
	[Publisher] [nvarchar](128) NOT NULL,
	[ParameterSecretName] [nvarchar](64) NOT NULL,
	[CreatedTime] [datetime2](7) NOT NULL,
	[LastUpdatedTime] [datetime2](7) NOT NULL,
	[ActivatedTime] [datetime2](7) NULL,
	[UnsubscribedTime] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[SubscriptionId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [provision].[MarketplaceSubProvisionJobs]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE provision.[MarketplaceSubProvisionJobs](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[SubscriptionId] [uniqueidentifier] NOT NULL,
	[OfferId] [nvarchar](50) NOT NULL,
	[PlanId] [nvarchar](50) NOT NULL,
	[PlanCreatedByEventId] [bigint] NOT NULL,
	[Mode] [nvarchar](64) NOT NULL,
	[Status] [nvarchar](64) NOT NULL,
	[EventType] [nvarchar](64) NOT NULL,
	[ProvisioningStepIndex] [int] NOT NULL,
	[IsSynchronizedStep] [bit] NOT NULL,
	[ProvisioningStepStatus] [nvarchar](64) NOT NULL,
	[ParametersSecretName] [nvarchar](64) NOT NULL,
	[ProvisionStepsSecretName] [nvarchar](64) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[retryCount] [int] NOT NULL,
	[lastErrorMessage] [nvarchar](1024) NULL,
	[provisionSteps] [nvarchar](1024) NULL,
	[CreatedByEventId] [bigint] NOT NULL,
	[CreatedTime] [datetime2](7) NOT NULL,
	[LastUpdatedTime] [datetime2](7) NOT NULL,
	[CompletedTime] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [provision].[LunaApplicationSwaggers]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [provision].[LunaApplicationSwaggers](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[ApplicationName] [nvarchar](128) NOT NULL,
	[SwaggerContent] [nvarchar](max) NOT NULL,
	[SwaggerEventId] [bigint] NULL,
	[LastAppliedEventId] [bigint] NULL,
	[IsEnabled] [bit] NOT NULL,
	[CreatedTime] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [provision].[MarketplacePlans]    Script Date: 4/29/2021 11:07:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [provision].[MarketplacePlans](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[offerId] [nvarchar](50) NOT NULL,
	[planId] [nvarchar](50) NOT NULL,
	[parameters] [nvarchar](max) NOT NULL,
	[mode] [nvarchar](64) NOT NULL,
	[properties] [nvarchar](max) NOT NULL,
	[provisioningStepsSecretName] [nvarchar](64) NOT NULL,
	[createdByEventId] [bigint] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  View [gallery].[LunaApplicationSwaggers]    Script Date: 6/17/2021 3:12:40 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [gallery].[LunaApplicationSwaggers]
AS
SELECT   Id,SwaggerEventId, SwaggerContent, ApplicationName
FROM     provision.LunaApplicationSwaggers
WHERE   (IsEnabled = 1)
GO

/****** Object:  View [routing].[Subscriptions]    Script Date: 5/17/2021 9:18:23 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [routing].[Subscriptions]
AS
SELECT SubscriptionId, ApplicationName, PrimaryKeySecretName, SecondaryKeySecretName, Status
FROM   gallery.LunaApplicationSubscriptions
WHERE  (Status = N'Subscribed')
GO


SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

Declare @adminUserId nvarchar(128)
Declare @adminUserName nvarchar(128)
Declare @createdDate datetime2(7)

SET @adminUserId = '$(adminUserId)'
SET @adminUserName = '$(adminUserName)'
SET @createdDate = GETUTCDATE()

INSERT INTO [rbac].[roleassignments] VALUES (@adminUserId, @adminUserName, 'SystemAdmin', @createdDate)
GO
