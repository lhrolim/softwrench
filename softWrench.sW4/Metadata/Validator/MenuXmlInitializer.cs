using System;
using System.Collections.Generic;
using System.IO;
using log4net;
using softwrench.sw4.Shared2.Metadata.Menu.Containers;
using softWrench.sW4.Metadata.Menu;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Validator {


    class MenuXmlInitializer {


        private const string MenuPattern = "menu.{0}.xml";

        private static readonly ILog Log = LogManager.GetLogger(typeof(MetadataParsingUtils));

        internal MenuDefinition InitializeMenu(ClientPlatform platform, Stream streamValidator = null, Boolean fallbackToDefault = true) {

            var catalog = LoadCatalog(platform);

            try {
                var menuName = String.Format(MenuPattern, platform.ToString().ToLower());
                using (var stream = MetadataParsingUtils.GetStream(streamValidator, menuName, fallbackToDefault)) {
                    return stream == null ? null : new XmlMenuMetadataParser().Parse(catalog, stream);
                }
            } catch (Exception) {
                Log.Warn(String.Format("menu.{0}.xml not found", platform));
            }
            return null;
        }

        private MenuTemplateCatalog LoadCatalog(ClientPlatform platform) {

            using (var stream = MetadataParsingUtils.DoGetStream(MetadataParsingUtils.GetMenuTemplatePath(platform))) {
                return stream == null ? null : new XmlTemplateMenuMetadataParser().Parse(stream);
            }
        }

        internal Dictionary<ClientPlatform, MenuDefinition> Initialize(Stream streamValidator = null) {
            var menus = new Dictionary<ClientPlatform, MenuDefinition>();
            foreach (ClientPlatform platform in Enum.GetValues(typeof(ClientPlatform))) {
                var fallbackToDefault = !platform.Equals(ClientPlatform.Mobile) || "demo".Equals(ApplicationConfiguration.ClientName);
                var menu = InitializeMenu(platform, streamValidator, fallbackToDefault);
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
