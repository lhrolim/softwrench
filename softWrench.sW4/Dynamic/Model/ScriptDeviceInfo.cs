using NHibernate.Mapping.Attributes;
using softwrench.sw4.api.classes.exception;
using softwrench.sW4.Shared2.Metadata.Applications;

namespace softWrench.sW4.Dynamic.Model {

    [Component]
    public class ScriptDeviceInfo {

        [Property(Column = "platform", TypeType = typeof(ClientPlatformTypeConverter))]
        public ClientPlatform Platform { get; set; } = ClientPlatform.Both;

        [Property(Column = "offlinedevice", TypeType = typeof(OfflineDeviceTypeConverter))]
        public OfflineDevice? OfflineDevice {
            get; set;
        }

        [Property]
        public string OfflineVersions {
            get; set;
        }

        public enum DeviceInfoValMode {
            Request, Database
        }

        public ScriptDeviceInfo Validate(DeviceInfoValMode mode) {

            if (mode.Equals(DeviceInfoValMode.Request)) {
                if (Platform.Equals(ClientPlatform.Both)) {
                    throw new InvalidStateException("specific platform has to be informed");
                }
                if (Platform.Equals(ClientPlatform.Mobile) && (OfflineDevice == null || OfflineDevice.Value.Equals(Model.OfflineDevice.ALL))) {
                    throw new InvalidStateException("specific device has to be informed");
                }
            }

            if (ClientPlatform.Mobile.Equals(Platform) || ClientPlatform.Both.Equals(Platform)) {
                if (OfflineVersions == null) {
                    throw new InvalidStateException("Offline (or Both) plataform must inform the version(s)");
                }
                if (OfflineDevice == null) {
                    throw new InvalidStateException("Offline (or Both) plataform must inform the valid devices(s)");
                }

            }
            if (ClientPlatform.Web.Equals(Platform)) {

                if (mode.Equals(DeviceInfoValMode.Request)) {
                    if (OfflineDevice != null) {
                        throw new InvalidStateException("Web plataform cannot set an offline device constraint");
                    }
                } else {
                    OfflineDevice = null;
                    OfflineVersions = null;
                }


            }
            return this;
        }


        public static ScriptDeviceInfo Web() {
            return new ScriptDeviceInfo { Platform = ClientPlatform.Web };
        }

        public static ScriptDeviceInfo DefaultDeploy() {
            return new ScriptDeviceInfo { Platform = ClientPlatform.Both };
        }
    }
}