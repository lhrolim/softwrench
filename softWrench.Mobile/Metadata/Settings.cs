using System;
using softWrench.Mobile.Communication;
using SQLite;

namespace softWrench.Mobile.Metadata
{
    internal sealed class Settings
    {
        private string _server;

        public Settings(Uri server)
        {
            Server = server.ToString();
            Routes = new ServerRoutes(server);
        }

        public Settings()
        {            
        }

        public Uri ServerAsUri()
        {
            return new Uri(Server);
        }

        [PrimaryKey]
        public int Id
        {
            get { return 1; }
            set { }
        }

        public string Server
        {
            get { return _server; }
            set
            {
                _server = value;
                Routes = new ServerRoutes(new Uri(value));
            }
        }

        [Ignore]
        public ServerRoutes Routes { get; private set; }
    }
}