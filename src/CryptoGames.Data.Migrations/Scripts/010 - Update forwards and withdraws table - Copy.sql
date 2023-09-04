alter table dbo.Forwards
alter column [TransactionHash] [nvarchar](255) NOT NULL
go
drop index IDX_Unique_TransactionHash ON dbo.Withdraws 
go
alter table dbo.Withdraws
alter column [TransactionHash] [nvarchar](255) NOT NULL
 


GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].Withdraws') AND name = N'IDX_Unique_TransactionHash')
	CREATE UNIQUE NONCLUSTERED INDEX [IDX_Unique_TransactionHash] ON [dbo].[Withdraws]
	(
		[TransactionHash] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].Forwards') AND name = N'IDX_Unique_TransactionHash')
	CREATE UNIQUE NONCLUSTERED INDEX [IDX_Unique_TransactionHash] ON [dbo].Forwards
	(
		[TransactionHash] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	GO
