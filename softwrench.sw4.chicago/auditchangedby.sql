CREATE TRIGGER audit_change_by_on_audit ON a_ticket AFTER INSERT AS 
IF EXISTS (SELECT * FROM a_ticket a JOIN inserted i ON a.ticketuid = i.ticketuid JOIN sr s ON a.ticketuid = s.ticketuid WHERE s.changeby IS NOT NULL)
BEGIN
	DECLARE @ticketuid bigint, @changeby varchar(100)
	SELECT @ticketuid = ticketuid FROM inserted
	SELECT @changeby = changeby FROM sr WHERE ticketuid = @ticketuid
	UPDATE a_ticket SET eauditusername = @changeby FROM a_ticket a INNER JOIN inserted i ON (i.ticketuid = a.ticketuid AND i.eaudittransid = a.eaudittransid)
END;

CREATE TRIGGER audit_change_on_sr ON ticket AFTER INSERT AS 
IF EXISTS (SELECT * FROM ticket t JOIN inserted i ON t.ticketuid = i.ticketuid WHERE t.class = 'SR' AND t.changeby IS NOT NULL)
BEGIN
	DECLARE @ticketuid bigint, @changeby varchar(100)
	SELECT @ticketuid = ticketuid, @changeby = changeby FROM inserted
	UPDATE a_ticket SET eauditusername = @changeby WHERE ticketuid = @ticketuid
END;