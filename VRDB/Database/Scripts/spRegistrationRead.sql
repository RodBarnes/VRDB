CREATE PROCEDURE [dbo].[spRegistrationRead]
	@stateVoterID int
AS

SELECT *
FROM Registration
WHERE StateVoterId = @stateVoterID

RETURN 0
