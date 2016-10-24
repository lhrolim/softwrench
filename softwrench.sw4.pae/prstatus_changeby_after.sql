-- Trigger that clears swchangeby on pr after a status update to not interfere on status changes by maximo
CREATE TRIGGER prstatus_change_by_after_trigger ON prstatus AFTER INSERT AS 
BEGIN 
DECLARE @prnum varchar(20)
SELECT @prnum = prnum FROM inserted
WITH (NOLOCK)
IF (EXISTS (SELECT prnum FROM pr WHERE prnum = @prnum AND swchangeby IS NOT NULL))
   UPDATE pr SET swchangeby = null WHERE prnum = @prnum
END;