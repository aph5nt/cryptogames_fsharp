delete FROM [GreedyRunTest].[dbo].[Withdraws]
alter table dbo.Withdraws
alter column TransactionHash nvarchar(255)

GO
 
/****** Object:  Index [IDX_Unique_TransactionHash]    Script Date: 13.11.2016 16:42:12 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IDX_Unique_TransactionHash] ON [dbo].[Withdraws]
(
	[TransactionHash] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO


