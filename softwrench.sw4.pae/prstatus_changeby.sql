-- Trigger that uses swchangeby insted of changeby to enable set the correct user on prstatus
CREATE TRIGGER prstatus_change_by_trigger ON prstatus INSTEAD OF INSERT AS 
BEGIN 
DECLARE @prnum varchar(20), @swchangeby varchar(60)
SELECT @prnum = prnum FROM inserted
SELECT @swchangeby = swchangeby FROM pr 
WITH (NOLOCK)
WHERE prnum = @prnum
IF (EXISTS (SELECT prnum FROM pr WHERE prnum = @prnum AND swchangeby IS NOT NULL))
   INSERT INTO prstatus (prnum,changedate,status,changeby,memo,siteid,orgid,prstatusid) SELECT prnum, changedate, status, @swchangeby as changeby, memo, siteid, orgid, prstatusid FROM inserted
ELSE
   INSERT INTO prstatus (prnum,changedate,status,changeby,memo,siteid,orgid,prstatusid) SELECT prnum, changedate, status, changeby, memo, siteid, orgid, prstatusid FROM inserted 
END;