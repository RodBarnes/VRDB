CREATE PROCEDURE [dbo].[spStatusUpdate]
	@filename NVARCHAR(50),
	@ticks BIGINT
AS

DECLARE @rows INT

SELECT @rows = COUNT(*) FROM Registration

IF EXISTS (SELECT * FROM ImportStatus WHERE Id = 1)
BEGIN
   UPDATE ImportStatus
   SET ImportFilename = @filename, TimeSpanTicks = @ticks
   WHERE Id = 1
END
ELSE
BEGIN
    INSERT INTO ImportStatus(Id, ImportFilename, TimeSpanTicks, ImportDateTime)
	VALUES(1, @filename, @ticks, GETDATE())
END

RETURN 0
