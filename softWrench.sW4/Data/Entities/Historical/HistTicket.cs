using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.Attributes;

namespace softWrench.sW4.Data.Entities.Historical
{
    [Class(Table = "HIST_TICKET", Lazy = false)]
    public class HistTicket
    {
        public const String ByAssetnum = "from HistTicket where AssetNum =?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property]
        public virtual string Ticketid { get; set; }

        [Property]
        public virtual string Description { get; set; }

        [Property]
        public virtual string Reportedby { get; set; }

        [Property]
        public virtual string Affectedperson { get; set; }

        [Property]
        public virtual string Status { get; set; }

        [Property]
        public virtual string Assignedto { get; set; }

        [Property]
        public virtual string Ownergroup { get; set; }

        [Property]
        public virtual DateTime Closedate { get; set; }
        
        [Property]
        public virtual string Assetnum { get; set; }

        [Property]
        public virtual string Assetsiteid { get; set; }

        [Property]
        public virtual string Classification { get; set; }

        public Dictionary<string, Object> toAttributeHolder()
        {
            Dictionary<string, object> attributeHolder = new Dictionary<string, object>();
            attributeHolder["ticketid"] = Ticketid;
            attributeHolder["class"] = Classification;
            attributeHolder["description"] = Description;
            attributeHolder["reportedby"] = Reportedby;
            //TO DO: Verify that the creationdate is to use the closedate from the sample data thomas sent us
            attributeHolder["creationdate"] = Closedate;
            attributeHolder["affectedperson"] = Affectedperson;
            attributeHolder["status"] = Status;
            //TO DO: handle owner attribute
            attributeHolder["ownergroup"] = Ownergroup;
            //TO DO: Verify history logic with thomas 
            attributeHolder["history"] = true;
            return attributeHolder;
        }

    }
}
