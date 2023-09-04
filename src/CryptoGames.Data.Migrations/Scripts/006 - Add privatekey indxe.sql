﻿delete FROM [Deposits]
GO

/****** Object:  Index [IDX_Unique_PrivateKey]    Script Date: 13.11.2016 12:58:39 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IDX_Unique_PrivateKey] ON [dbo].[Deposits]
(
	[PrivateKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
