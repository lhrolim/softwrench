DECLARE @ORGID varchar
DECLARE @SITEID varchar

SET @ORGID  = 'DPWORG'
SET @SITEID = 'DPW'

-- Create user profile 'laborers'
IF NOT EXISTS (SELECT 1 FROM dbo.SW_USERPROFILE WHERE NAME = 'Laborers')
BEGIN
INSERT INTO [dbo].[SW_USERPROFILE] ([name], [deletable], [description]) VALUES (N'Laborers', 0, N'Laborers')
END

-- Create user 'swadmin'
IF NOT EXISTS (SELECT 1 FROM dbo.SW_USER2 WHERE USERNAME = 'swadmin')
BEGIN
INSERT INTO [dbo].[SW_USER2] (username, password, firstname, lastname, isactive, orgid, siteid, phone, language)
VALUES ('swadmin', '16193128416889160822141461911951451401273111117118200', 'admin', 'admin', 1, @ORGID, @SITEID, '1-800-433-7300', 'EN')
END

DECLARE @USERID int
SELECT  @USERID = id FROM dbo.SW_USER2 WHERE USERNAME = 'swadmin'

-- Define Dashboard if not exist
IF NOT EXISTS (SELECT 1 FROM dbo.DASH_DASHBOARD WHERE TITLE = 'SRs and WOs')
BEGIN

-- Assign all user with default orgid and siteid
UPDATE SW_USER2
SET ORGID = @ORGID, SITEID = @SITEID

DECLARE @DASHBOARDID int
DECLARE @WO int
DECLARE @SR int

INSERT INTO [dbo].[DASH_DASHBOARD] (layout, title, createdby, creationdate, updatedate)
VALUES ('1,1', 'SRs and WOs', @USERID, GETDATE(), GETDATE());

SELECT @DASHBOARDID = @@IDENTITY;

INSERT INTO [dbo].[DASH_BASEPANEL] (alias_, title, createdby, creationdate, updatedate)
VALUES ('ActiveSRs', 'Active Service Requests', @USERID, GETDATE(), GETDATE())

SELECT @SR = @@IDENTITY;

INSERT INTO [dbo].[DASH_BASEPANEL] (alias_, title, createdby, creationdate, updatedate)
VALUES ('ActiveWOs', 'Active Work Orders', @USERID, GETDATE(), GETDATE())

SELECT @WO = @@IDENTITY;

INSERT INTO [dbo].[DASH_GRIDPANEL] (gpid, application, schemaref, defaultsortfield)
VALUES (@SR, 'servicerequest', 'list', 'ticketid')

INSERT INTO [dbo].[DASH_GRIDPANEL] (gpid, application, schemaref, defaultsortfield)
VALUES (@WO, 'workorder', 'list', 'wonum')

INSERT INTO [dbo].[DASH_DASHBOARDREL] (position, panel_id, dashboard_id)
VALUES(0, @WO, @DASHBOARDID)

INSERT INTO [dbo].[DASH_DASHBOARDREL] (position, panel_id, dashboard_id)
VALUES(1, @SR, @DASHBOARDID)

END

-- Define Whereclause if not exist
IF NOT EXISTS (SELECT 1 FROM [dbo].[CONF_CONDITION] WHERE ALIAS_ = 'active_sr_dashboard')
BEGIN

DECLARE @CONDITIONID int

INSERT INTO [dbo].[CONF_CONDITION] (alias_, fullkey, global)
VALUES ('active_sr_dashboard', '/_whereclauses/servicerequest/', 0)

SELECT @CONDITIONID = @@IDENTITY;

INSERT INTO [dbo].[CONF_WCCONDITION] (wcwcid, metadataid)
VALUES (@CONDITIONID, 'ActiveSRs')

	IF NOT EXISTS (SELECT 1 FROM [dbo].[CONF_PROPERTYDEFINITION] WHERE FULLKEY LIKE '/_whereclauses/servicerequest/whereclause')
	BEGIN
		INSERT INTO [dbo].[CONF_PROPERTYDEFINITION] (fullkey, key_, defaultvalue, datatype, renderer, visible, contextualized, alias_)
		VALUES ('/_whereclauses/servicerequest/whereclause', 'whereclause', '', 'String', 'whereclause', 1, 1, '')
	END

INSERT INTO [dbo].[CONF_PROPERTYVALUE] (value, definition_id, condition_id)
VALUES ('((status in (''INPROG'',''SR WO COMP'',''WO CREATED'' ,''NEW'',''PENDING'',''QUEUED''))) and ((owner = @username  or upper(reportedby) = @username ) or ((ownergroup is not null) and ownergroup  in (select persongroup from persongroupteam where respparty = @username ))) and siteid = ''DPW''', '/_whereclauses/servicerequest/whereclause', @CONDITIONID)

END 
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[CONF_CONDITION] WHERE ALIAS_ = 'active_wo_dashboard')
BEGIN

DECLARE @CONDITIONID int

INSERT INTO [dbo].[CONF_CONDITION] (alias_, fullkey, global)
VALUES ('active_wo_dashboard', '/_whereclauses/workorder/', 0)

SELECT @CONDITIONID = @@IDENTITY;

INSERT INTO [dbo].[CONF_WCCONDITION] (wcwcid, metadataid)
VALUES (@CONDITIONID, 'ActiveWOs')

	IF NOT EXISTS (SELECT 1 FROM [dbo].[CONF_PROPERTYDEFINITION] WHERE FULLKEY LIKE '/_whereclauses/workorder/whereclause')
	BEGIN
		INSERT INTO [dbo].[CONF_PROPERTYDEFINITION] (fullkey, key_, defaultvalue, datatype, renderer, visible, contextualized, alias_)
		VALUES ('/_whereclauses/workorder/whereclause', 'whereclause', '', 'String', 'whereclause', 1, 1, '')
	END

INSERT INTO [dbo].[CONF_PROPERTYVALUE] (value, definition_id, condition_id)
VALUES ('((workorder.status in (''APPR'',''BASE'',''FIELD WORK COMP'',''NEW'',''QUEUED'',''INPRG'',''INV'',''POSTPONED'',''RFI'',''SAW'',''WAPPR'',''WMATL'',''WPCOND'',''WSCH'')) and ((owner = ''swadmin'' or upper(reportedby) = ''swadmin'') or ((ownergroup is not null) and ownergroup  in (select persongroup from persongroupteam where respparty = ''swadmin''))) and historyflag = 0 and istask = 0 and workorder.siteid = ''DPW'')', '/_whereclauses/workorder/whereclause', @CONDITIONID)
END 
GO