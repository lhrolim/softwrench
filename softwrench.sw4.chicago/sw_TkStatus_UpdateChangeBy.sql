-- =============================================
-- Author:		Kayono Halim, Edson Mesquita
-- Create date: 20150312
-- Description:	Trigger to update changeby field of TkStatus table for records created by Softwrench (via MIF)
--              which will have a value of MXINTADM
-- Update: 20151104-KH: check ownergroup to fix issue with Maximo CHAT SR creation.
-- Update: 20161027-EM: status and owner changeby not correctly set
-- Update: 20161128-EM: avoid null @changebyNewValue
-- =============================================
CREATE TRIGGER [dbo].[sw_TkStatus_UpdateChangeBy]
   ON  [dbo].[tkstatus] 
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
	DECLARE @tkstatusid bigint
	DECLARE @ownergroup nvarchar(60)

	SELECT @ticketid = ticketid, @class = class, @siteid = siteid,  @tkstatusid = tkstatusid, @changebyOldValue = changeby FROM inserted
	SELECT @changebyNewValue = swchangeby, @ownergroup = ownergroup FROM ticket where (ticketid = @ticketid) AND (class = @class) AND (siteid = @siteid)

	IF (@changebyOldValue = 'MXINTADM' AND @changebyNewValue IS NOT NULL AND @ownergroup <> 'CHAT_Q') 
	BEGIN
		UPDATE tkstatus set changeby = @changebyNewValue where tkstatusid = @tkstatusid
	END
END