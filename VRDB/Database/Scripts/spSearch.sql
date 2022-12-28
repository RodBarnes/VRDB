CREATE PROCEDURE [dbo].[spSearch]
	@lastName NVARCHAR(50),
	@firstName NVARCHAR(50) = NULL,
	@birthYear INT = NULL,
	@gender CHAR = NULL
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
	WHERE (@gender IS NULL OR r.Gender = @gender)
	 AND (@birthYear IS NULL OR r.BirthYear = @birthYear)
	 AND (@firstName IS NULL OR r.FName LIKE @firstName)
	 AND (r.LName LIKE @lastName)
	ORDER BY
		r.LName, 
		r.FName,
		r.MName

RETURN 0
