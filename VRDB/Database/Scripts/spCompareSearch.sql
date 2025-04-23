CREATE PROCEDURE [dbo].[spCompareSearch]
(
	@lastName NVARCHAR(50),
	@birthYear INT = NULL,
	@gender NVARCHAR(1) = NULL,
	@firstName NVARCHAR(50) = NULL,
	@middleName NVARCHAR(50) = NULL,
	@streetNumber NVARCHAR(50) = NULL,
	@streetName NVARCHAR(50) = NULL,
	@streetType NVARCHAR(50) = NULL
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
	  AND (@birthYear IS NULL OR r.BirthYear = @birthYear)
	  AND (@gender IS NULL OR (r.Gender = @gender))
      AND ((LEN(@firstName) = 1 AND SUBSTRING(r.FName,1,1) = @firstName) OR (LEN(@firstName) > 1 AND r.FName = @firstName))
	  AND (@middleName IS NULL OR (SUBSTRING(r.MName,1,1) = SUBSTRING(@middleName,1,1)))
	  AND (@streetName IS NULL OR (r.RegStName LIKE @streetName))
	  AND (@streetNumber IS NULL OR (r.RegStNum LIKE @streetNumber))
	  AND (@streetType IS NULL OR (r.RegStType LIKE @streetType))
	ORDER BY
		r.LName, 
		r.FName,
		r.MName

RETURN 0
