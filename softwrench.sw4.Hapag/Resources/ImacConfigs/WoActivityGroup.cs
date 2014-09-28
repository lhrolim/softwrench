using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;

namespace softwrench.sw4.Hapag.Resources.ImacConfigs {
    [Serializable()]
    [System.Xml.Serialization.XmlRoot("RootActivities")]
    public class ActivityGroup {
        [XmlArray("Activities")]
        [XmlArrayItem("Activity", typeof(Activity))]
        public Activity[] Activity { get; set; }
    }
}
