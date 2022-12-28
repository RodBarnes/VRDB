CREATE TABLE [dbo].[ImportStatus]
(
	[Id] INT NOT NULL PRIMARY KEY,
	[ImportFilename] NVARCHAR(50) NOT NULL,
	[TimeSpanTicks] BIGINT NOT NULL, 
    [ImportDateTime] DATETIME2 NOT NULL
)
