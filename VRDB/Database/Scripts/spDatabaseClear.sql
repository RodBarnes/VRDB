﻿CREATE PROCEDURE [dbo].[spDatabaseClear]
AS
TRUNCATE TABLE Registration
TRUNCATE TABLE ImportStatus
RETURN 0
