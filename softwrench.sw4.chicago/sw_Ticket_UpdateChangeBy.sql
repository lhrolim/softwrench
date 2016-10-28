-- =============================================
-- Author:		Calvin Cheng, Kayono Halim, Edson Mesquita
-- Create date: 20150312
-- Description:	Trigger to update changeby and ownerchangeby field for records created by Softwrench (via MIF)
-- Update: 20151104-KH: check ownergroup to fix issue with Maximo CHAT SR creation.
-- Update: 20161027-EM: status and owner changeby not correctly set
-- =============================================
CREATE TRIGGER [dbo].[sw_Ticket_UpdateChangeBy]
   ON  [dbo].[ticket] 
   AFTER UPDATE
AS 
BEGIN	

	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON

    -- Insert statements for trigger here
    DECLARE @ticketid nvarchar(20)
    DECLARE @class nvarchar(32)
    DECLARE @siteid varchar(20)
	DECLARE @changebyNewValue nvarchar(60)
	DECLARE @ownergroup nvarchar(60)

	SELECT 
	@ticketid = ticketid, 
	@class = class,
	@siteid = siteid,
	@changebyNewValue = swchangeby,
	@ownergroup = ownergroup
	FROM inserted
	
	-- Need to validate that swchange does have some value to begin with
	IF (@changebyNewValue IS NOT NULL AND @ownergroup <> 'CHAT_Q') 
	BEGIN
		UPDATE ticket
		set changeby = @changebyNewValue
		WHERE (ticketid = @ticketid) AND (class = @class) AND (siteid = @siteid)
	END
END