drop table [dbo].[Withdraws]

CREATE TABLE [dbo].[Withdraws](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Network] [nvarchar](50) NOT NULL,
	UserName nvarchar(128) NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[VerifyStatus] [nvarchar](50) NOT NULL,
	 
	[ToAddress] [nvarchar](250) NOT NULL,
	[Amount] [decimal](18, 8) NOT NULL,

	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NOT NULL,

	[TransactionHash] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_Transactions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Withdraws] ADD  CONSTRAINT [DF_Withdraws_CreatedAt]  DEFAULT (getutcdate()) FOR [CreatedAt]
GO

ALTER TABLE [dbo].[Withdraws] ADD  CONSTRAINT [DF_Withdraws_UpdatedAt]  DEFAULT (getutcdate()) FOR [UpdatedAt]
GO


alter table [dbo].[Withdraws]
alter column [TransactionHash] [nvarchar](255) NULL