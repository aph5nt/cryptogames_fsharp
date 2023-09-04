 DROP TABLE localapi.ObserverQueue
 DROP table localapi.Wallets
 GO
 CREATE SCHEMA freeApi
 GO

CREATE TABLE freeApi.Deposits(
	[Address] [nvarchar](50) NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[Balance] [decimal](18, 8) NOT NULL,
CONSTRAINT [PK_Wallets] PRIMARY KEY NONCLUSTERED 
(
	[Address] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Clustered_Wallets_UserName] UNIQUE CLUSTERED 
(
	UserName ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

--------
DROP TABLE [blockio].[Transactions]
DROP SCHEMA [blockio]
GO

CREATE TABLE dbo.Withdraws(
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Network] [nvarchar](50) NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[FromAddress] [nvarchar](250) NOT NULL,
	[ToAddress] [nvarchar](250) NOT NULL,
	[Amount] [decimal](18, 8) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NOT NULL,
 CONSTRAINT [PK_Transactions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE dbo.Withdraws ADD  CONSTRAINT [DF_Withdraws_CreatedAt]  DEFAULT (getutcdate()) FOR [CreatedAt]
GO

ALTER TABLE dbo.Withdraws ADD  CONSTRAINT [DF_Withdraws_UpdatedAt]  DEFAULT (getutcdate()) FOR [UpdatedAt]
GO
