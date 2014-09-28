using System;
using NHibernate.Mapping.Attributes;

namespace softWrench.sW4.Metadata.Entities.Types {

    class Workorder {

        [Id]
        public virtual long workorderid { get; set; }

        [Property]
        [Generator]
        public virtual string wonum { get; set; }

        [Property]
        public virtual string status { get; set; }

        [Property]
        public virtual string orgid { get; set; }

        [Property]
        public virtual string siteid { get; set; }

        [Property]
        public virtual DateTime statusdate { get; set; }

        [Property]
        public virtual string worktype { get; set; }

        [Property]
        public virtual string description { get; set; }

        [Property]
        public virtual string assetnum { get; set; }

        [Property]
        public virtual string location { get; set; }

        [Property]
        public virtual string changeby { get; set; }

        [Property]
        public virtual DateTime changedate { get; set; }

        [Property]
        public virtual float estdur { get; set; }

        [Property]
        public virtual float estlabhrs { get; set; }

        [Property]
        public virtual decimal estmatcost { get; set; }

        [Property]
        public virtual decimal estlabcost { get; set; }


        [Property]
        public virtual float actlabhrs { get; set; }
        [Property]
        public virtual decimal esttoolcost { get; set; }


        [Property]
        public virtual decimal actmatcost { get; set; }

        [Property]
        public virtual decimal acttoolcost { get; set; }

        [Property]
        public virtual decimal actlabcost { get; set; }

        [Property]
        public virtual bool historyflag { get; set; }

        [Property]
        public virtual int wopriority { get; set; }

        [Property]
        public virtual string reportedby { get; set; }
        [Property]
        public virtual string phone { get; set; }
        [Property]
        public virtual string problemcode { get; set; }
        [Property]
        public virtual DateTime reportdate { get; set; }
        [Property]
        public virtual DateTime actstart { get; set; }
        [Property]
        public virtual DateTime actfinish { get; set; }
        [Property]
        public virtual DateTime schedstart { get; set; }
        [Property]
        public virtual DateTime schedfinish { get; set; }
        [Property]
        public virtual string supervisor { get; set; }
        [Property]
        public virtual string failurecode { get; set; }
        [Property]
        public virtual string glaccount { get; set; }
        [Property]
        public virtual string observation { get; set; }
        [Property]
        public virtual string onbehalfof { get; set; }
        [Property]
        public virtual string owner { get; set; }
        [Property]
        public virtual string ownergroup { get; set; }
        [Property]
        public virtual string woclass { get; set; }
        [Property]
        public virtual bool hasfollowupwork { get; set; }
        [Property]
        public virtual string origrecordid { get; set; }
        [Property]
        public virtual string origrecordclass { get; set; }
        [Property]
        public virtual DateTime rowstamp { get; set; }
        

        //        <attribute name="location.description" required="false" type="varchar" label="" />
        //        <attribute name="asset.description" required="false" type="varchar" label="" />
        //        <attribute name="synstatus.maxvalue" required="false" type="varchar" label="" />
        //        <attribute name="synstatus.description" required="false" type="varchar" label="" />
        //        <attribute name="failurecode.description" required="false" type="varchar" label="" />
        //        <attribute name="worktype.wtypedesc" required="false" type="varchar" label="" />
        //        <attribute name="chartofaccounts.accountname" required="false" type="varchar" label="" />
        //        <attribute name="longdescription.ldtext" required="false" type="varchar" label="" />
    }
}
