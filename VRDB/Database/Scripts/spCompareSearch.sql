﻿CREATE PROCEDURE [dbo].[spCompareSearch]
(
	@lastName NVARCHAR(50),
	@birthDate DATETIME2(7) = NULL,
	@gender NVARCHAR(1) = NULL,
	@firstName NVARCHAR(50) = NULL,
	@middleName NVARCHAR(50) = NULL
)
AS
	SELECT
		r.LName,
		r.FName,
		r.MName,
		r.BirthDate,
		r.Gender,
		r.RegStNum,
		r.RegStFrac,
		r.RegStPreDirection,
		r.RegStName,
		r.RegStType,
		r.RegStPostDirection,
		r.RegUnitType,
		r.RegStUnitNum,
		r.RegCity,
		r.RegState,
		r.RegZipCode,
		r.RegistrationDate,
		r.LastVoted,
		r.StatusCode
	FROM Registration r
	WHERE r.LName = @lastName
	  AND (@birthDate IS NULL OR r.BirthDate = @birthDate)
	  AND (@gender IS NULL OR (r.Gender = @gender))
	  AND ((LEN(@firstName) > 1 AND (r.FName = @firstName)) OR (SUBSTRING(r.FName,1,1) = SUBSTRING(@firstName,1,1)))
	  AND (@middleName IS NULL OR (SUBSTRING(r.MName,1,1) = SUBSTRING(@middleName,1,1)))
	ORDER BY
		r.LName, 
		r.FName,
		r.MName

RETURN 0
