using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services.Api;
using cts.commons.simpleinjector;
using ctes = softWrench.sW4.Data.Configuration.ConfigurationConstants;

namespace softWrench.sW4.Data.Configuration {
    public class ConfigurationRegistry : ISingletonComponent {

        public ConfigurationRegistry(IConfigurationFacade facade) {
            facade.Register(ctes.MainIconKey, new PropertyDefinition() {
                Description = "Icon that will appear alongside the entire application, after login",
                //represents base64 of logo_hapag.gif. Got from: http://webcodertools.com/imagetobase64converter/Create
                StringValue = "data:image/gif;base64,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",
                Renderer = "attachment"

            });

            facade.Register(ctes.UserRowstampKey, new PropertyDefinition() {
                Description = "current user rowstamp from maximo",
                StringValue = "0",
                DataType = "long",
            });

            facade.Register(ctes.PersonGroupRowstampKey, new PropertyDefinition() {
                Description = "current person group rowstamp from maximo",
                StringValue = "0",
                DataType = "long",
            });

            facade.Register(ctes.PersonGroupAssociationRowstampKey, new PropertyDefinition() {
                Description = "current person group association rowstamp from maximo",
                StringValue = "0",
                DataType = "long",
            });

            facade.Register(ctes.MyProfileEnabled, new PropertyDefinition() {
                Description = "Is My Profile Module Enabled for the system",
                StringValue = "true",
                DataType = "boolean",
            });

            facade.Register(ctes.MyProfileReadOnly, new PropertyDefinition() {
                Description = "Is My Profile Module ReadOnly for the system",
                StringValue = "true",
                DataType = "boolean",
            });


            facade.Register(ctes.ClientSideLogLevel, new PropertyDefinition() {
                Description = "Level of client side logs",
                StringValue = "warn",
                DataType = "string",
            });

            facade.Register(ctes.InvbalancesListScanOrder, new PropertyDefinition() {
                Description = "Inventory grid filter field scan order",
                DataType = "string",
                DefaultValue = "itemnum",
                Visible = false,
            });

            facade.Register(ctes.PhysicalcountListScanOrder, new PropertyDefinition() {
                Description = "Physical count grid filter field scan order",
                DataType = "string",
                DefaultValue = "itemnum",
                Visible = false,
            });

            facade.Register(ctes.PhysicaldeviationListScanOrder, new PropertyDefinition() {
                Description = "Physical deviation grid filter field scan order",
                DataType = "string",
                DefaultValue = "itemnum",
                Visible = false,
            });

            facade.Register(ctes.MatrectransTransfersListScanOrder, new PropertyDefinition() {
                Description = "Inventory transfer grid filter field scan order",
                DataType = "string",
                DefaultValue = "itemnum",
                Visible = false,
            });

            facade.Register(ctes.ReservedMaterialsListScanOrder, new PropertyDefinition() {
                Description = "Reserved materials grid filter field scan order",
                DataType = "string",
                DefaultValue = "itemnum",
                Visible = false,
            });

            facade.Register(ctes.InvIssueListScanOrder, new PropertyDefinition() {
                Description = "Inv issue grid filter field scan order",
                DataType = "string",
                DefaultValue = "itemnum",
                Visible = false,
            });

            facade.Register(ctes.InvIssueListBeringScanOrder, new PropertyDefinition() {
                Description = "Inv issue grid filter field scan order",
                DataType = "string",
                DefaultValue = "assetnum",
                Visible = false,
            });

            facade.Register(ctes.NewInvIssueDetailScanOrder, new PropertyDefinition() {
                Description = "Inv issue detail field scan order",
                DataType = "string",
                DefaultValue = "itemnum",
                Visible = false,
            });

            facade.Register(ctes.NewKeyIssueDetailScanOrder, new PropertyDefinition() {
                Description = "Inv issue detail field scan order",
                DataType = "string",
                DefaultValue = "laborcode,storeroom,itemnum,rotassetnum",
                Visible = false,
            });

//            facade.Register(ConfigurationConstants.LdapAuthNonMaximoUsers, new PropertyDefinition() {
//                Description = "Allow non maximo users to login into the system",
//                StringValue = "true",
//                DataType = "boolean",
//            });

            // default values of gmaps addresses
            facade.Register(ctes.MapsDefaultCityKey, new PropertyDefinition() {
                Description = "The default value to city property to use for locate addresses on google maps",
                DataType = "string",
                CachedOnClient = true
            });

            facade.Register(ctes.MapsDefaultStateKey, new PropertyDefinition() {
                Description = "The default value to state/province property to use for locate addresses on google maps",
                DataType = "string",
                CachedOnClient = true
            });

            facade.Register(ctes.MapsDefaultCountryKey, new PropertyDefinition() {
                Description = "The default value to country property to use for locate addresses on google maps",
                DataType = "string",
                CachedOnClient = true
            });

            facade.Register(ctes.MetadataChangeReportEmailId, new PropertyDefinition() {
                Description = "The default email address for metadata change reporting",
                DataType = "string",
                DefaultValue = "devteam@controltechnologysolutions.com"
            });

            facade.Register(ctes.TransactionStatsReportDuration, new PropertyDefinition() {
                Description = "The default period (days) for which the transaction statistics report will be sent",
                DataType = "int",
                DefaultValue = "7"
            });

            facade.Register(ctes.DateTimeFormat, new PropertyDefinition() {
                Description = "The default format for DateTime",
                DataType = "string",
                DefaultValue = "MM/dd/yyyy hh:mm",
            });

            facade.Register(ctes.JsErrorShowDevKey, new PropertyDefinition()
            {
                Description = "Show JS Error notifications in Dev/QA by default",
                StringValue = "true",
                DataType = "boolean",
                CachedOnClient = true
            });

            facade.Register(ctes.JsErrorShowProdKey, new PropertyDefinition()
            {
                Description = "Hide JS Error notifications in Prod by default",
                StringValue = "false",
                DataType = "boolean",
                CachedOnClient = true
            });


            #region Password Config
            facade.Register(ctes.Password.MinLengthKey, new PropertyDefinition() {
                Description = "Password's minimum required length",
                DataType = "long",
                StringValue = "6",
                CachedOnClient = true
            });
            facade.Register(ctes.Password.MaxAdjacentKey, new PropertyDefinition() {
                Description = "Number of identical adjancent characters allowed in password",
                DataType = "long",
                StringValue = "null",
                CachedOnClient = true
            });
            facade.Register(ctes.Password.RequiresUppercaseKey, new PropertyDefinition() {
                Description = "Password requires uppercase characters ?",
                DataType = "boolean",
                StringValue = "false",
                CachedOnClient = true
            });
            facade.Register(ctes.Password.RequiresLowercaseKey, new PropertyDefinition() {
                Description = "Password requires lowercase characters ?",
                DataType = "boolean",
                StringValue = "false",
                CachedOnClient = true
            });
            facade.Register(ctes.Password.RequiresNumberKey, new PropertyDefinition() {
                Description = "Password requires number characters ?",
                DataType = "boolean",
                StringValue = "false",
                CachedOnClient = true
            });
            facade.Register(ctes.Password.RequiresSpecialKey, new PropertyDefinition() {
                Description = "Password requires special characters ?",
                DataType = "boolean",
                StringValue = "false",
                CachedOnClient = true
            });
            facade.Register(ctes.Password.PlacementNumberFirstKey, new PropertyDefinition() {
                Description = "Password's first character can be a number ?",
                DataType = "boolean",
                StringValue = "false",
                CachedOnClient = true
            });
            facade.Register(ctes.Password.PlacementNumberLastKey, new PropertyDefinition() {
                Description = "Password's last character can be a number ?",
                DataType = "boolean",
                StringValue = "false",
                CachedOnClient = true
            });
            facade.Register(ctes.Password.PlacementSpecialFirstKey, new PropertyDefinition() {
                Description = "Password's first character can be a special character ?",
                DataType = "boolean",
                StringValue = "false",
                CachedOnClient = true
            });
            facade.Register(ctes.Password.PlacementSpecialLastKey, new PropertyDefinition() {
                Description = "Password's last character can be a special character ?",
                DataType = "boolean",
                StringValue = "false",
                CachedOnClient = true
            });
            facade.Register(ctes.Password.BlackListKey, new PropertyDefinition() {
                Description = "Forbidden password list (comma separated)",
                DataType = "string",
                CachedOnClient = true
            });
            facade.Register(ctes.Password.LoginKey, new PropertyDefinition() {
                Description = "Password can contain user's username",
                DataType = "boolean",
                StringValue = "false",
                CachedOnClient = true
            });
            #endregion

            #region Bulletin Board
            facade.Register(ctes.BulletinBoard.Enabled, new PropertyDefinition() {
                Description = "Whether or not the bulletin board feature is enabled in softWrench",
                DataType = "boolean",
                DefaultValue = "false",
                CachedOnClient = true
            });
            facade.Register(ctes.BulletinBoard.JobRefreshRate, new PropertyDefinition() {
                Description = "Interval in minutes to run the job that caches active bulletinboard records from the database",
                DataType = "long",
                DefaultValue = "5",
                CachedOnClient = true
            });
            facade.Register(ctes.BulletinBoard.UiRefreshRate, new PropertyDefinition() {
                Description = "Interval in minutes to refresh the bulletin board messages displayed in the side-panel",
                DataType = "long",
                DefaultValue = "5",
                CachedOnClient = true
            });
            #endregion
        }
    }
}
