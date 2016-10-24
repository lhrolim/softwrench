-- Trigger that set swchangeby on changeby on pr to enable set the correct user on pr
CREATE TRIGGER pr_change_by_trigger ON pr AFTER UPDATE AS 
BEGIN 
DECLARE @prnum varchar(20), @swchangeby varchar(60)
SELECT @prnum = prnum, @swchangeby = swchangeby FROM inserted
WITH (NOLOCK)
IF (EXISTS (SELECT prnum FROM pr WHERE prnum = @prnum AND swchangeby IS NOT NULL))
   UPDATE pr SET changeby = @swchangeby WHERE prnum = @prnum
END;