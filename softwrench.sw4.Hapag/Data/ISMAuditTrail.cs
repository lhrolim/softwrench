using System;
using NHibernate.Mapping.Attributes;
using softwrench.sw4.Hapag.Data.WS.Ism.Base;
using softwrench.sW4.audit.classes.Model;
using softWrench.sW4.Metadata.Applications;

namespace softwrench.sw4.Hapag.Data {

    [JoinedSubclass(NameType = typeof(ISMAuditTrail), Lazy = false, ExtendsType = typeof(AuditTrail), Table = "audi_ismtrail")]
    public class ISMAuditTrail : AuditTrail {

        [Key(-1, Column = "ISMTrailId")]
        public virtual int? ISMTrailId { get; set; }

        [Property]
        public virtual int Type { get; set; }


        [Property]
        public virtual string Routing { get; set; }

        public static ISMAuditTrail GetInstance(TrailType type, ApplicationMetadata name) {
            var typestring = type == TrailType.Creation ? 2 : 6;
            var namestring = type == TrailType.Creation ? "Problem_Submittal" : "Provide_Problem_Information";
            var routing = GetRouting(name);
            return new ISMAuditTrail {
                BeginTime = DateTime.Now.ToUniversalTime(),
                EndTime = DateTime.Now.ToUniversalTime(), //TODO: Change so that this can be updated in the DB after the request is called
                Type = typestring,
                Name = namestring,
                Routing = routing
            };

        }

        private static string GetRouting(ApplicationMetadata metadata) {
            if (metadata.Name.Equals("CHANGE", StringComparison.CurrentCultureIgnoreCase)) {
                return ISMConstants.ChangeRequestRoutingType;
            }
            return ISMConstants.ServiceIncidentRoutingType;
        }

        public enum TrailType {
            Creation, Update
        }

    }
}
