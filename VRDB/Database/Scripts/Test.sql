/*
select count(*) from Registration

exec spRegistrationsClear

exec spRegistrationRead 3082359

exec spSearch'Barnes', 'Rodney' --, 1957, 'M'

select @@version

*/

--SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'

/*
select * from Registration
where LName = 'BARNES'
  --and FName = 'ANTHONY'
  --and BirthDate = '3/3/1957'
  and RegCity = 'PORT ORCHARD'
  and RegStName = 'ARTHUR'
  and RegStType = 'CT'
*/

-- general 
exec spCompareSearch 'FETT', '4/20/1984', NULL, 'C', NULL -- no middle name in VRDB
exec spCompareSearch 'JONES', '9/1/1994', NULL, 'J', NULL -- gender is X in VRDB
exec spCompareSearch 'LITCHFIELD', '3/21/1953', NULL, 'S', NULL -- no middle name in LCR
exec spCompareSearch 'LUBAHN', '7/5/1948', NULL, 'S', NULL -- no middle name in VRDB
exec spCompareSearch 'LYBBERT', '8/12/1944', NULL, 'L', NULL -- no middle name in VRDB
exec spCompareSearch 'MOORE', '2/24/1954', NULL, 'D', NULL -- no middle name in VRDB
exec spCompareSearch 'MORALES', '8/9/1974', NULL, 'J', NULL -- middle 'R' in VRDB; 'KAE' in LCR
exec spCompareSearch 'MOSS', '1/25/1963', NULL, 'C', NULL -- no middle name in VRDB
exec spCompareSearch 'MOSS', '4/30/1967', NULL, 'M', NULL -- no middle name in VRDB
exec spCompareSearch 'STOTT', '11/19/1974', NULL, 'G', NULL -- no middle name in VRDB
exec spCompareSearch 'STRANG', '4/5/1999', NULL, 'J', NULL -- no middle name in VRDB
exec spCompareSearch 'VAUGHN', '6/20/1936', NULL, 'J', NULL -- no middle name in LCR
exec spCompareSearch 'WEBER', '1/3/1950', NULL, 'L', NULL -- more than one match
exec spCompareSearch 'WEEKS', '2/28/1957', NULL, 'P', NULL -- no middle name in VRDB

-- gender specific
exec spCompareSearch 'CAVIGLIA', '12/14/1950', NULL, 'MARY', NULL -- gender is M in VRDB
exec spCompareSearch 'LYBBERT', '8/12/1944', NULL, 'LYLE', NULL -- gender is X in VRDB

SELECT definition  
FROM sys.sql_modules  
WHERE object_id = (OBJECT_ID(N'dbo.spCompareSearch'));


