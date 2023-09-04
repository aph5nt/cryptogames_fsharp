 
CREATE TABLE [dbo].Forwards(
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	UserName nvarchar(50) NOT NULL,
	[Network] [nvarchar](50) NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[Amount] [decimal](18, 8) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[TransactionHash] [nvarchar](255) NULL,
 CONSTRAINT [PK_Forwards] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].Forwards ADD  CONSTRAINT [DF_Forwards_CreatedAt]  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
 