using System.Net;

namespace softWrench.Mobile.Communication.Http
{
    internal static class CookieContainerProvider
    {
        private static CookieContainer _container;

        public static CookieContainer Container
        {
            get
            {
                var container = _container;

                if (null != _container)
                {
                    return container;
                }

                // If we have no cookie container
                // yet, let's create one on the fly.
                container = new CookieContainer();

                // And keep it for future use.
                _container = container;

                return container;
            }
            set
            {
                _container = value;
            }
        }
    }
}