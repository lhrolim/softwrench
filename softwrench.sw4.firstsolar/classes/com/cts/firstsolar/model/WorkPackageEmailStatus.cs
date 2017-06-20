using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model {

    [Class(Table = "OPT_WPEMAILSTATUS", Lazy = false)]
    public class WorkPackageEmailStatus : IBaseEntity {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id { get; set; }


        public WpRequestStatus Status {
            get {
                if (AckDate != null) {
                    return WpRequestStatus.Ack;
                }
                return WpRequestStatus.Sent;
            }
        }

        [Property(Column = "email")]
        public string Email { get; set; }

        /// <summary>
        /// The operation on the main workpackage in which the Email was sent, either creation, update, deletion, or a custom one
        /// </summary>
        [Property(Column = "operation")]
        public string Operation { get; set; }

        /// <summary>
        /// An extra qualifier to identify the distribution list in which this email belongs.
        /// 
        /// Ex: Interconected Documents checked
        /// 
        /// Check https://controltechnologysolutions.atlassian.net/browse/SWWEB-3020
        /// 
        /// </summary>
        [Property(Column = "qualifier")]
        public string Qualifier { get; set; }

        [Property(Column = "senddate")]
        public DateTime? SendDate { get; set; }

        [Property(Column = "ackdate")]
        public DateTime? AckDate { get; set; }




    }
}
