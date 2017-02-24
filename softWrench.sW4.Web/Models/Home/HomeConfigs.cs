using System;

namespace softWrench.sW4.Web.Models.Home {
    public class HomeConfigs {

        public long InitTimeMillis { get; set; }

        public string Logo { get; set; }
        public Boolean MyProfileEnabled { get; set; }

        public Boolean I18NRequired { get; set; }

        public string Environment { get; set; }
        public Boolean IsLocal { get; set; }
        public Boolean ActivityStreamFlag { get; set; }
        public bool CrudSearchFlag { get; set; }
        public string ClientName { get; set; }
        public string ClientSideLogLevel { get; set; }
        public int SuccessMessageTimeOut { get; set; }
        public string InvbalancesListScanOrder { get; set; }
        public string NewInvIssueDetailScanOrder { get; set; }
        public string InvIssueListScanOrder { get; set; }
        public string PhysicalcountListScanOrder { get; set; }
        public string PhysicaldeviationListScanOrder { get; set; }
        public string MatrectransTransfersListScanOrder { get; set; }
        public string ReservedMaterialsListScanOrder { get; set; }
        public string InvIssueListBeringScanOrder { get; set; }
        public string DefaultEmail { get; set; }
        public Boolean UIShowClassicAdminMenu { get; set; }
        public Boolean UIShowToolbarLabels { get; set; }

        public SWdisplayableFormats DisplayableFormats { get; set; }
    }

    public class SWdisplayableFormats {
        public string DateTimeFormat { get; set; }
    }
}