namespace softwrench.sw4.dashboard.classes.service.graphic.tableau {
    public static class TableauConstants {
        public const string SYSTEM_NAME = "Tableau";

        public static class Config {
            public const string KEY_SERVER = "tableau_server";
            public const string KEY_SITE = "tableau_site";
            public const string KEY_USERNAME = "tableau_username";
            public const string KEY_PASSWORD = "tableau_password";
        }

        public static class RestApi {
            public const string CONTENT_TYPE = "application/xml;charset=utf-8";
            public const string URL_PATTERN = "{0}api/2.0/{1}";
            public const string DOCUMENT_NAMESPACE = "http://tableausoftware.com/api";

            public const string AUTH_METHOD = "auth/signin";
            public const string AUTH_PAYLOAD = "<tsRequest>" +
                                               "<credentials name=\"{0}\" password=\"{1}\">" +
                                               "<site contentUrl=\"{2}\"/>" +
                                               "</credentials>" +
                                               "</tsRequest>";
            public const string AUTH_HEADER_NAME = "X-Tableau-Auth";

            public const string RESOURCE_WORKBOOK = "workbook";
            public const string RESOURCE_VIEW = "view";
        }

        public static class AuthType {
            public const string KEY = "authtype";
            public const string REST_API = "REST";
            public const string TRUSTED_TICKET = "TICKET";
        }
    }
}
