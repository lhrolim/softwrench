using System;
using System.Collections.Generic;
using NHibernate.Mapping.Attributes;

namespace softWrench.sW4.Data.Entities.Historical
{
    [Class(Table = "HIST_WORKORDER", Lazy = false)]
    public class HistWorkorder
    {
        public const String ByAssetnum = "from HistWorkorder where AssetNum =?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property]
        public virtual string Wonum { get; set; }

        [Property]
        public virtual string Description { get; set; }

        [Property]
        public virtual string Location { get; set; }

        [Property]
        public virtual string Assetnum { get; set; }

        [Property]
        public virtual string Status { get; set; }

        [Property]
        public virtual DateTime Statusdate { get; set; }

        public Dictionary<string, Object> toAttributeHolder()
        {
            Dictionary<string, object> attributeHolder = new Dictionary<string, object>();
            attributeHolder["wonum"] = Wonum;
            //TO DO: handle woclass
            attributeHolder["description"] = Description;
            attributeHolder["location"] = Location;
            attributeHolder["assetnum"] = Assetnum;
            attributeHolder["status"] = Status;
            //TO DO: Verify that the reportdate is to use the statusdate from the sample data thomas sent us
            attributeHolder["reportdate"] = Statusdate;
            return attributeHolder;
        }

    }
}
