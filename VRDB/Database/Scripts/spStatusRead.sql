CREATE PROCEDURE [dbo].[spStatusRead]
AS

SELECT 
	i.ImportFilename, 
	i.ImportDateTime, 
	i.TimeSpanTicks
FROM ImportStatus i
WHERE i.Id = 1

RETURN 0
