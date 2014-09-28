using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using softWrench.sW4.Metadata.Menu;
using softWrench.sW4.Metadata.Parsing;
using softWrench.sW4.Metadata.Properties;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Validator {


    class MenuXmlInitializer {


        private const string MenuPattern = "menu.{0}.xml";

        private static readonly ILog Log = LogManager.GetLogger(typeof(MetadataParsingUtils));

        internal MenuDefinition InitializeMenu(ClientPlatform platform, Stream streamValidator = null) {
            try {
                using (var stream = MetadataParsingUtils.GetStream(streamValidator, String.Format(MenuPattern, platform.ToString().ToLower()))) {
                    return stream == null ? null : new XmlMenuMetadataParser().Parse(stream);
                }
            } catch (Exception) {
                Log.Warn(String.Format("menu.{0}.xml not found", platform));
            }
            return null;
        }

        internal Dictionary<ClientPlatform, MenuDefinition> Initialize(Stream streamValidator = null) {
            var menus = new Dictionary<ClientPlatform, MenuDefinition>();
            foreach (ClientPlatform platform in Enum.GetValues(typeof(ClientPlatform))) {
                var menu = InitializeMenu(platform, streamValidator);
                if (menu != null) {
                    menus.Add(platform, menu);
                }
            }
            return menus;
        }

        internal static string GetMenuPath(ClientPlatform platform) {
            return MetadataParsingUtils.GetPath(String.Format(MenuPattern, platform.ToString().ToLower()));
        }


    }
}
