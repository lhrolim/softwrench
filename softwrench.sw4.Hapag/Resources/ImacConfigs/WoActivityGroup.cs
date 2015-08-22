using System;
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
