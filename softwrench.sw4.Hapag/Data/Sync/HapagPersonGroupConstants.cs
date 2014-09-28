
namespace softwrench.sw4.Hapag.Data.Sync {
    public class HapagPersonGroupConstants {


        public const string HlagLocationProperty = "hlaglocation";
        public const string PersonGroupPrefix = "HLC-DE-";

        internal const string BaseHapagPrefix = "C-HLC-WW-%";
        internal const string BaseHapagPrefixNoWildcard = "C-HLC-WW";



        //Location groups, like areas, regions and location itself
        internal const string BaseHapagLocationPrefix = "C-HLC-WW-LC-";
        internal const string BaseHapagRegionPrefix = "C-HLC-WW-RG";
        internal const string HapagRegionAmerica = "C-HLC-WW-RG-AMERICA";
        internal const string BaseHapagAreaPrefix = "C-HLC-WW-AR";
        //this one gives access to all the region groups
        internal const string HapagWWGroup = "C-HLC-WW-AR-WW";

        #region profiles
        internal const string BaseHapagProfilePrefix = "C-HLC-WW-RO-";
        internal const string HEu = "C-HLC-WW-RO-EU";
        internal const string HITC = "C-HLC-WW-RO-ITC";
        internal const string HExternalUser = "C-HLC-WW-RO-EXT";
        #endregion

        #region functionalroles
        internal const string InternalRolesPrefix = "C-HLC-WW-IFU";
        internal const string Tom = "C-HLC-WW-IFU-TOM";
        internal const string Itom = "C-HLC-WW-IFU-ITOM";
        internal const string Ad = "C-HLC-WW-IFU-AD";
        internal const string Change = "C-HLC-WW-IFU-CHANGE";

        internal const string ExternalRolesPrefix = "C-HLC-WW-EFU";
        internal const string SSO = "C-HLC-WW-EFU-SSO";
        internal const string Tui = "C-HLC-WW-EFU-TUI";

        internal const string ActrlRam = "C-HLC-WW-IFU-ACTRL-RAM";
        internal const string Actrl = "C-HLC-WW-IFU-ACTRL";
        internal const string XITC = "C-HLC-WW-IFU-XITC";
        internal const string Purchase = "C-HLC-WW-IFU-PURCHASE";
        #endregion
    }
}
