using System;
using System.Globalization;
using cts.commons.portable.Util;
using cts.commons.Util;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model {


    [Class(Table = "OPT_CALLOUT", Lazy = false)]
    public class CallOut : IFsEmailRequest{

        

        public const string ByStatusAndTime = "from CallOut where status in('Scheduled', 'Error') and sendTime <= ? and actualSendTime = null";


        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }


        [Property]
        public int WorkPackageId { get; set; }



        //actually ONETOONE, but who cares, since NHIB doesnt seem to work fine with it
        //        [ManyToOne(Column = "subcontractorid", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "none")]
        //        public SubContractor SubContractor { get; set; }

        [Property]
        public string SubContractorId { get; set; }

        
        [Property]
        public string SubContractorName { get; set; }

        [Property]
        public string Token { get; set; }


        [Property]
        public DateTime? SendTime { get; set; }

        [Property]
        public DateTime? ContractorStartDate { get; set; }


        [Property]
        public DateTime? ActualSendTime { get; set; }

        /// <summary>
        /// First solar would like to maintain an update to the current status of a call out - the statuses right now would be: 
        ///
        /// "Scheduled" -- When the call out is scheduled to be sent
        ///"Request sent" - when a call out has been emailed 
        ///"Approved" - the subcontractor has clicked the approval link in the email 
        ///"Rejected" - the subcontractor has clicked the rejected link in the email 
        ///"Pending" - the subcontractor has clicked pending while they determine whether they can approve the call out 
        /// </summary>
        [Property(Column = "status", TypeType = typeof(RequestStatusConverter))]
        public RequestStatus Status { get; set; }

        [Property]
        public string Notes { get; set; }

        public string EntityDescription => "subcontractor callout";

        public string ByToken => "from CallOut where Token = ?";

        [Property]
        public DateTime? ExpirationDate { get; set; }

        [Property]
        public string PoNumber { get; set; }

        [Property]
        public string ToNumber { get; set; }

        [Property]
        public string Email { get; set; }
        public string Cc { get; set; }

        [Property]
        public string SiteName { get; set; }

        [Property]
        public string BillingEntity { get; set; }

        [Property]
        public string NotToExceedAmount { get; set; }

        [Property]
        public string RemainingFunds { get; set; }

        [Property]
        public string ScopeOfWork { get; set; }

        [Property]
        public string PlantContacts { get; set; }

        [Property]
        public string OtherInfo { get; set; }

        [Property]
        public bool SendNow { get; set; }

        public bool GenerateToken() {
            if (Token != null) {
                return false;
            }

            var token = "" + new Random(100).Next(10000);
            token += DateTime.Now.TimeInMillis().ToString(CultureInfo.InvariantCulture);
            token += AuthUtils.GetSha1HashData(token);
            Token = token;
            return true;
        }
    }
}
