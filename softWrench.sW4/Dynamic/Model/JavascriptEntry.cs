using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using softwrench.sw4.user.classes.entities;
using softwrench.sW4.Shared2.Metadata.Applications;

namespace softWrench.sW4.Dynamic.Model {


    public class ClientPlatformTypeConverter : EnumStringType<ClientPlatform> {
    }

    public class OfflineDeviceTypeConverter : EnumStringType<OfflineDevice> {
    }

    public enum OfflineDevice {
        Android, IOS, ALL
    }

    public static class OfflineDeviceExtensions {

        public static bool Match(this OfflineDevice? device, OfflineDevice? toCheck) {
            if (device == null) {
                return toCheck == null;
            }
            if (device == OfflineDevice.ALL) {
                return true;
            }

            return device.Equals(toCheck);
        }
    }


    [Class(Table = "DYN_SCRIPT_JSENTRY", Lazy = false)]
    public class JavascriptEntry : AScriptEntry {

        public const string DeployedScripts = "from JavascriptEntry where Deploy = 1";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }


        [ComponentProperty]
        public ScriptDeviceInfo ScriptDeviceCriteria { get; set; } = ScriptDeviceInfo.DefaultDeploy();

        public override AScriptEntry ShallowCopy() {
            return (JavascriptEntry)MemberwiseClone();
        }

        public override bool Isoncontainer {
            get {
                return true;
            }
            set {
            }
        }
    }
}
