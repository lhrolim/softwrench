-- =============================================
-- Author:		Calvin Cheng, Edson Mesquita
-- Create date: 20150312
-- Description:	Trigger to update ownerchangeby field for records created by Softwrench (via MIF)
-- Update: 20151104-KH: check ownergroup to fix issue with Maximo CHAT SR creation.
-- Update: 20161027-EM: status and owner changeby not correctly set
-- =============================================
CREATE TRIGGER [dbo].[sw_TkOwner_UpdateChangeBy] 
   ON  [dbo].[tkownerhistory] 
   AFTER INSERT
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON

    DECLARE @changebyOldValue nvarchar(60)
	DECLARE @changebyNewValue nvarchar(60)
	DECLARE @ticketid nvarchar(20)
	DECLARE @class varchar(32)
	DECLARE @siteid varchar(20)
	DECLARE @tkwonerhistoryid bigint
	DECLARE @ownergroup nvarchar(60)
	
	SELECT @ticketid = ticketid, @class = class, @siteid = siteid, @tkwonerhistoryid = tkownerhistoryid, @changebyOldValue = ownerchangeby  FROM inserted
	SELECT @changebyNewValue = swchangeby, @ownergroup = ownergroup FROM Ticket where (ticketid = @ticketid) AND (class = @class) AND (siteid = @siteid)

	IF (@changebyOldValue = 'MXINTADM' AND @changebyNewValue IS NOT NULL AND @ownergroup <> 'CHAT_Q') 
	BEGIN
		UPDATE tkownerhistory set ownerchangeby = @changebyNewValue where tkownerhistoryid = @tkwonerhistoryid
	END
END