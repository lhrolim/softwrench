namespace softWrench.sW4.Data.Configuration {
    public static class ConfigurationConstants {

        public const string UserRowstampKey = "/Global/Rowstamps/User";
        public const string PersonGroupRowstampKey = "/Global/Rowstamps/PersonGroup";
        public const string PersonGroupAssociationRowstampKey = "/Global/Rowstamps/PersonGroupAssociation";
        public const string MainIconKey = "/Global/Icons/Main";

        public class Maximo {
            public const string WsdlPath = "/Global/Maximo/WS/WsdlBasePath";
            public const string IgnoreCertErrors = "/Global/Maximo/WS/IgnoreCertErrors";
            public const string WsProvider = "/Global/Maximo/WS/WsProvider";
            public const string MifUser = "/Global/Maximo/WS/UserName";
            public const string MifPassword = "/Global/Maximo/WS/Password";

            public const string DefaultOrgId = "/Global/Maximo/DefaultOrgId";
            public const string DefaultSiteId = "/Global/Maximo/DefaultSiteId";
            public const string DefaultStoreLoc = "/Global/Maximo/DefaultStoreLoc";
        }



        //        public const string LdapAuthNonMaximoUsers = "/Global/Ldap/AllowNonMaximoUsers";
        //        public const string LdapAuthSyncEveryTime = "/Global/Ldap/SyncEveryTime";

        public const string MyProfileEnabled = "/Global/MyProfile/Enabled";
        public const string MyProfileReadOnly = "/Global/MyProfile/ReadOnly";


        public class Filter {
            public const string ApplyDefaultPreviousFilter = "/Global/Grid/Filter/ApplyDefaultPrevious";
        }



        public const string ClientSideLogLevel = "/Global/Logs/ClientLevel";

        public const string InvbalancesListScanOrder = "/Global/Grids/InvBalances/ScanBar";
        public const string InvIssueListScanOrder = "/Global/Grids/InvIssue/ScanBar";
        public const string NewInvIssueDetailScanOrder = "/Global/Details/InvIssue/ScanBar";
        public const string PhysicalcountListScanOrder = "/Global/Grids/PhysicalCount/ScanBar";
        public const string PhysicaldeviationListScanOrder = "/Global/Grids/PhysicalDeviation/ScanBar";
        public const string MatrectransTransfersListScanOrder = "/Global/Grids/InventoryTransfer/ScanBar";
        public const string ReservedMaterialsListScanOrder = "/Global/Grids/ReservedMaterials/ScanBar";
        public const string InvIssueListBeringScanOrder = "/Global/Grids/InvIssueBering/ScanBar";
        public const string NewKeyIssueDetailScanOrder = "/Global/Details/KeyIssue/ScanBar";

        public const string MetadataChangeReportEmailId = "/Global/Metadata/NotificationEmail";
        public const string TransactionStatsReportDuration = "/Global/Transactions/Report/Period";

        #region caching

        public class Cache
        {
            public const string RedisURL = "/Global/Cache/RedisURL";
        }
        

        #endregion

        #region SW diaplayable formats

        public const string DateTimeFormat = "/Global/Format/DateTimeFormat";

        #endregion

        //        public static class Global {
        //            public static class Rowstamps {
        //                public const string User = "/Global/Rowstamps/User";
        //            }
        //            public static class Icons {
        //                public const string Main = "/Global/Icons/Main";
        //            }
        //        }

        // keys for default values of gmaps addresses
        public const string MapsDefaultCityKey = "/Global/Maps/DefaultCity";
        public const string MapsDefaultStateKey = "/Global/Maps/DefaultState";
        public const string MapsDefaultCountryKey = "/Global/Maps/DefaultCountry";

        public class Password {
            public const string MinLengthKey = "/Global/Password/Min";
            public const string MaxAdjacentKey = "/Global/Password/Adjacent";
            public const string CanContainLoginKey = "/Global/Password/ContainLogin";
            public const string RequiresUppercaseKey = "/Global/Password/Uppercase";
            public const string RequiresLowercaseKey = "/Global/Password/Lowercase";
            public const string RequiresNumberKey = "/Global/Password/Number";
            public const string RequiresSpecialKey = "/Global/Password/Special";
            public const string PlacementNumberFirstKey = "/Global/Password/PlacementNumberFirst";
            public const string PlacementNumberLastKey = "/Global/Password/PlacementNumberLast";
            public const string PlacementSpecialFirstKey = "/Global/Password/PlacementSpecialFirst";
            public const string PlacementSpecialLastKey = "/Global/Password/PlacementSpecialLast";
            public const string BlackListKey = "/Global/Password/BlackList";
            public const string LoginKey = "/Global/Password/Login";
        }

        public class Email {
            public const string Host = "/Global/Email/Smtp/Host";
            public const string Port = "/Global/Email/Smtp/Port";
            public const string Timeout = "/Global/Email/Smtp/Timeout";
            public const string EnableSSL = "/Global/Email/Smtp/EnableSSL";
            public const string UserName = "/Global/Email/Smtp/Username";
            public const string Password = "/Global/Email/Smtp/Password";
            public const string DefaultFromEmail = "/Global/Email/DefaultFrom";
        }


        public class BulletinBoard {
            public const string Enabled = "/Global/BulletinBoard/Enabled";
            public const string JobRefreshRate = "/Global/BulletinBoard/RefreshRate/Job";
            public const string UiRefreshRate = "/Global/BulletinBoard/RefreshRate/Ui";
        }

        public const string JsErrorShowDevKey = "/Global/JsError/ShowDev";
        public const string JsErrorShowProdKey = "/Global/JsError/ShowProd";
    }
}
