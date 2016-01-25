namespace softWrench.sW4.Web.Util.StaticFileLoad {
    public static class Bundles {

        public static class Local {

            public static readonly string VendorStyles = "~/Content/vendor/css";
            public static readonly string CustomVendorStyles = "~/Content/customVendor/css";
            public static readonly string FontsStyles = "~/Content/fonts";
            public static readonly string AppStyles = "~/Content/styles/client/client-css";
            public static readonly string SharedAppStyles = "~/Content/styles/shared";

            public static readonly string[] Styles = {
                VendorStyles,
                CustomVendorStyles,
                FontsStyles,
                AppStyles,
                SharedAppStyles
            };

            public static readonly string AppScripts = "~/Content/Scripts/client/application";
            public static readonly string VendorScripts = "~/Content/vendor/scripts";
            public static readonly string CustomVendorScripts = "~/Content/customVendor/scripts";
            public static readonly string SharedAppScripts = "~/Content/Scripts/client/application/shared";

            public static readonly string ClientJsScripts = "~/Content/Scripts/client/client-js";

            public static readonly string[] Scripts = {
                VendorScripts,
                CustomVendorScripts,
                AppScripts,
                SharedAppScripts,
                ClientJsScripts
            };

        }

        public static class Distribution {

            public static readonly string VendorStyles = "~/Content/dist/css";

            public static readonly string[] Styles = {
                VendorStyles,
                Local.FontsStyles,
                Local.AppStyles,
                Local.SharedAppStyles
            };

            public static readonly string AllScripts = "~/Content/dist/scripts";

            public static readonly string[] Scripts = {
                AllScripts
            };
        }
    }
}
