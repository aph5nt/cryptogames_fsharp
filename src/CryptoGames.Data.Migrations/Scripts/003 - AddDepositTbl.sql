CREATE TABLE [dbo].[Deposits](
	[Id] [bigint] IDENTITY(1,1)  NOT NULL,
	[Network] nvarchar(10) NOT NULL,
	[UserName] nvarchar(50) NOT NULL,
	[Address] nvarchar(50) NOT NULL,
	[PublicKey] nvarchar(255) NOT NULL,
	[PrivateKey] nvarchar(255) NOT NULL,
 CONSTRAINT [PK_Deposits] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
