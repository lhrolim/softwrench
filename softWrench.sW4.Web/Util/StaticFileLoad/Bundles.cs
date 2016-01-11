namespace softWrench.sW4.Web.Util.StaticFileLoad {
    public static class Bundles {

        public static class Local {

            public static readonly string[] Styles = {
                "~/Content/vendor/css",
                "~/Content/customVendor/css",
                "~/Content/fonts",
                "~/Content/styles/client/client-css",
                "~/Content/styles/shared"
            };

            public static readonly string[] Scripts = {
                "~/Content/vendor/scripts",
                "~/Content/customVendor/scripts",
                "~/Content/Scripts/client/application",
                "~/Content/Scripts/client/application/shared"
            };
        }

        public static class Distribution {
            public static readonly string[] Styles = {
                "~/Content/dist/css"
            };

            public static readonly string[] Scripts = {
                "~/Content/dist/scripts"
            };
        }
    }
}
