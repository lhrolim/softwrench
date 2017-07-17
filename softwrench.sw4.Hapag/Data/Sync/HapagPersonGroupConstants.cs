
namespace softwrench.sw4.Hapag.Data.Sync {
    public class HapagPersonGroupConstants {


        public const string HlagLocationProperty = "hlaglocation";
        public const string HlagLocationXITCProperty = "hlaglocationxitc";
        public const string PersonGroupPrefix = "HLC-DE-";

        internal const string BaseHapagPrefix = "C-HLC-WW-%";
        internal const string BaseHapagPrefixNoWildcard = "C-HLC-WW";



        //Location groups, like areas, regions and location itself
        internal const string BaseHapagLocationPrefix = "C-HLC-WW-LC-";
        internal const string BaseHapagRegionPrefix = "C-HLC-WW-RG";

        internal const string HapagRegionAsia = "C-HLC-WW-RG-ASIA";
        internal const string HapagRegionEurope = "C-HLC-WW-RG-EUROPE";
        internal const string HapagRegionGSC = "C-HLC-WW-RG-GSC";
        internal const string HapagRegionHeadQuarter = "C-HLC-WW-RG-HQ";
        internal const string HapagRegionAmerica = "C-HLC-WW-RG-NAMERICA";
        internal const string HapagRegionLAmerica = "C-HLC-WW-RG-LAMERICA";
        internal const string HapagRegionMidEast = "C-HLC-WW-RG-MIDEAST";
        
        
 
        


        internal const string BaseHapagAreaPrefix = "C-HLC-WW-AR";
        //this one gives access to all the region groups
        internal const string HapagWWGroup = "C-HLC-WW-AR-WW";

        internal const string HamburgGroup = "C-HLC-WW-LC-HH1";
        internal const string Hamburg2Group = "C-HLC-WW-LC-HH2";

        #region profiles
        public const string BaseHapagProfilePrefix = "C-HLC-WW-RO-";
        public const string HEu = "C-HLC-WW-RO-EU";
        public const string HITC = "C-HLC-WW-RO-ITC";
        public const string HExternalUser = "C-HLC-WW-RO-EXT";
        #endregion

        #region functionalroles
        public const string InternalRolesPrefix = "C-HLC-WW-IFU";
        internal const string Tom = "C-HLC-WW-IFU-TOM";
        internal const string Itom = "C-HLC-WW-IFU-ITOM";
        internal const string Ad = "C-HLC-WW-IFU-AD";
        internal const string Change = "C-HLC-WW-IFU-CHANGE";
        internal const string Offering = "C-HLC-WW-IFU-OFFERING";

        public const string ExternalRolesPrefix = "C-HLC-WW-EFU";
        internal const string SSO = "C-HLC-WW-EFU-SSO";
        internal const string Tui = "C-HLC-WW-EFU-TUI";

        internal const string ActrlRam = "C-HLC-WW-IFU-ACTRL-RAM";
        internal const string Actrl = "C-HLC-WW-IFU-ACTRL";
        internal const string XITC = "C-HLC-WW-IFU-XITC";
        internal const string Purchase = "C-HLC-WW-IFU-PURCHASE";
        #endregion
    }
}
