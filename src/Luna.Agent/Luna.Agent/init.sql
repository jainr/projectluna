/****** Object:  User [lunauserlunaailol]    Script Date: 10/2/2020 10:18:35 AM ******/

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

/****** Object:  Table [dbo].[agent_amlworkspaces]    Script Date: 10/2/2020 10:18:35 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[agent_amlworkspaces](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[WorkspaceName] [nvarchar](50) NOT NULL,
	[ResourceId] [nvarchar](max) NOT NULL,
	[AADApplicationId] [uniqueidentifier] NOT NULL,
	[AADTenantId] [uniqueidentifier] NULL,
	[AADApplicationSecretName] [nvarchar](128) NULL,
	[Region] [nvarchar](32) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[agent_apiversions]    Script Date: 10/2/2020 10:18:35 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[agent_apiversions](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[DeploymentName] [nvarchar](50) NOT NULL,
	[ProductName] [nvarchar](50) NOT NULL,
	[VersionName] [nvarchar](50) NOT NULL,
	[RealTimePredictAPI] [nvarchar](max) NULL,
	[TrainModelAPI] [nvarchar](max) NULL,
	[BatchInferenceAPI] [nvarchar](max) NULL,
	[DeployModelAPI] [nvarchar](max) NULL,
	[AuthenticationType] [nvarchar](50) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[LastUpdatedTime] [datetime2](7) NULL,
	[VersionSourceType] [nvarchar](50) NULL,
	[ProjectFileUrl] [nvarchar](max) NULL,
	[AMLWorkspaceId] [bigint] NULL,
	[AuthenticationKeySecretName] [nchar](10) NULL,
	[PublisherId] [uniqueidentifier] NULL,
	[ConfigFile] [nvarchar](256) NULL,
 CONSTRAINT [PK_agent_apiversions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[agent_apiversions1]    Script Date: 10/2/2020 10:18:35 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[agent_apiversions1](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[DeploymentName] [nvarchar](50) NOT NULL,
	[ProductName] [nvarchar](50) NOT NULL,
	[VersionName] [nvarchar](50) NOT NULL,
	[RealTimePredictAPI] [nvarchar](max) NULL,
	[TrainModelAPI] [nvarchar](max) NULL,
	[BatchInferenceAPI] [nvarchar](max) NULL,
	[DeployModelAPI] [nvarchar](max) NULL,
	[AuthenticationType] [nvarchar](50) NULL,
	[CreatedTime] [datetime2](7) NOT NULL,
	[LastUpdatedTime] [datetime2](7) NOT NULL,
	[VersionSourceType] [nvarchar](50) NOT NULL,
	[ProjectFileUrl] [nvarchar](max) NULL,
	[AMLWorkspaceId] [bigint] NULL,
	[AuthenticationKeySecretName] [nchar](10) NULL,
	[PublisherId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_agent_apiversions1] PRIMARY KEY CLUSTERED 
(
	[DeploymentName] ASC,
	[ProductName] ASC,
	[VersionName] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[agent_offers]    Script Date: 10/2/2020 10:18:35 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[agent_offers](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[OfferId] [nvarchar](50) NOT NULL,
	[OfferName] [nvarchar](128) NOT NULL,
	[PublisherId] [uniqueidentifier] NOT NULL,
	[PublisherMicrosoftId] [nvarchar](128) NULL,
	[PublisherName] [nvarchar](128) NULL,
	[Description] [nvarchar](256) NULL,
	[LogoImageUrl] [nvarchar](max) NULL,
	[DocumentationUrl] [nvarchar](max) NULL,
	[LandingPageUrl] [nvarchar](max) NULL,
	[OfferType] [nvarchar](16) NULL,
	[LastUpdatedTime] [datetime2](7) NULL,
	[CreatedTime] [datetime2](7) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[agent_publishers]    Script Date: 10/2/2020 10:18:35 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[agent_publishers](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[PublisherId] [uniqueidentifier] NOT NULL,
	[ControlPlaneUrl] [nvarchar](max) NOT NULL,
	[Name] [nvarchar](128) NULL,
 CONSTRAINT [PK_agent_publishers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[agent_subscriptions]    Script Date: 10/2/2020 10:18:35 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[agent_subscriptions](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SubscriptionId] [uniqueidentifier] NOT NULL,
	[DeploymentName] [nvarchar](50) NULL,
	[ProductName] [nvarchar](50) NULL,
	[ProductType] [nvarchar](32) NULL,
	[Owner] [nvarchar](256) NULL,
	[Name] [nvarchar](50) NULL,
	[Status] [nvarchar](16) NULL,
	[HostType] [nvarchar](16) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[BaseUrl] [nvarchar](max) NULL,
	[PrimaryKeySecretName] [nvarchar](128) NULL,
	[SecondaryKeySecretName] [nvarchar](128) NULL,
	[AMLWorkspaceId] [bigint] NULL,
	[AMLWorkspaceComputeClusterName] [nvarchar](128) NULL,
	[AMLWorkspaceDeploymentTargetType] [nvarchar](32) NULL,
	[AMLWorkspaceDeploymentClusterName] [nvarchar](128) NULL,
	[AgentId] [uniqueidentifier] NULL,
	[PublisherId] [uniqueidentifier] NULL,
	[OfferName] [nvarchar](50) NULL,
	[PlanName] [nvarchar](50) NULL,
 CONSTRAINT [PK_agent_subscriptions_1] PRIMARY KEY CLUSTERED 
(
	[SubscriptionId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[agent_users]    Script Date: 10/2/2020 10:18:35 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[agent_users](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AADUserId] [nvarchar](256) NULL,
	[Description] [nvarchar](1024) NULL,
	[Role] [nvarchar](8) NULL,
	[SubscriptionId] [uniqueidentifier] NULL,
	[ObjectId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_agent_users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

Declare @adminUserName nvarchar(512)
Declare @adminAADObjectId uniqueidentifier

SET @adminUserName = $(adminUserName)
SET @adminAADObjectId = $(adminAADObjectId)

INSERT INTO agent_users VALUES (@adminUserName, 'Service admin', 'Admin', null, @adminAADObjectId
GO

