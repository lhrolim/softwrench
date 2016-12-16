namespace softwrench.sw4.Hapag.Data.WS.Ism.Base {
    public class ISMConstants {
        public const string GlobalWsdlProperty = "globalwsdlpath";
        public const string DefaultAssignedGroup = "I-EUS-DE-CSC-SDK-HLCFRONTDESK";
        public const string ImacAssignedGroup = "I-EUS-DE-CSC-IMC-HLCIMAC";
        public const string ChangeAssignedGroup = "I-SM-DE-SDM-SDM-HLC-SDM";
        public const string DefaultCustomerName = "HLC-DE-HH1";
        public const string HamburgLocation2 = "HLC-DE-HH2";
        public const string DefaultBaseLocation = "HLC-00";
        public const string LocationId = "ESS6";
        public const string OrganizationId = "ITD-ESS6";
        public const string System = "21030000";
        public const string EmailDomain = "@HLAG.COM";
        public const string ChangeRequestRoutingType = "MX::HLCSTCHGBRDG";
        public const string ServiceIncidentRoutingType = "MX::HLCINC";
        
        
        public const string PluspCustomerColumn = "pluspcustomer";



        public static string AddEmailIfNeeded(string personId) {
            if (personId == null) {
                return null;
            }
            if (personId.EndsWith(EmailDomain)) {
                return personId;
            }
            return personId + EmailDomain;
        }

        public static string NormalizeLocation(string location) {
            if (location != null && !location.StartsWith("HLC-DE") && location.Length == 3) {
                return "HLC-DE-" + location;
            }
            return location;
        }



    }
}
