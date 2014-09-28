using System;
using System.Linq;

namespace softWrench.Mobile.Communication {
    internal sealed class ServerRoutes {
        private const string ClientPlatform = "mobile";
        private readonly Uri _server;
        private readonly String _context;

        public ServerRoutes(Uri server) {
            _server = server;
            _context = BuildContext();
        }
        /// <summary>
        /// By default, softwrench, but if the user specified the full context on settings, use it.
        /// </summary>
        /// <returns></returns>
        private string BuildContext() {
            var context = "softwrench";
            if (_server != null && _server.LocalPath != "") {
                context = _server.LocalPath;
                if (!context.StartsWith("/")) {
                    context = "/" + context;
                }
                if (context.EndsWith("/")) {
                    context = context.Substring(0, context.Length - 1);
                }
            }
            return context;
        }

        private Uri Route(string relativeUriFormat, params string[] unescapedArguments) {
            var escapedArguments = unescapedArguments
                .Select(Uri.EscapeDataString)
                .Cast<object>()
                .ToArray();

            var escapedRelativeUri = string.Format(relativeUriFormat, escapedArguments);
            if (_context != null) {
                escapedRelativeUri = _context + escapedRelativeUri;
            }
            var route = new Uri(_server, escapedRelativeUri);
            return route;
        }

        public Uri SignIn() {
            return Route("/SignIn/SignInReturningUserData");
        }

        public Uri Metadata() {
            return Route("/api/mobile/DownladMetadatas");
        }


        public Uri Sync() {
            return Route("/api/mobile/SyncData");
        }

        public Uri InsertData(string application) {
            return Route("/api/data/{0}?platform={1}&currentSchemaKey=detail.input.mobile", application, ClientPlatform);
        }

        public Uri UpdateData(string application, string id) {
            return Route("/api/data/{0}/{1}?platform={2}&currentSchemaKey=detail.input.mobile", application, id, ClientPlatform);
        }

        public Uri Operation(string application, string operation) {
            return Route("/api/data/operation/{0}/{1}?platform={2}", application, operation, ClientPlatform);
        }
    }
}
