using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector.app;
using JetBrains.Annotations;
using NHibernate;
using NHibernate.Context;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Util;

namespace cts.commons.persistence {


    public interface ISessionManager {


        ISession OpenSession();

        void Restart();

    }


    public class SessionManagerWrapper : ISessionManager {

        private readonly string _connectionString;
        private readonly string _driverName;
        private readonly string _dialect;

        private readonly IEnumerable<Assembly> _assembliesToLookupForMappings;

        private ISessionFactory _sessionFactory;
        private readonly bool _isReadOnly;
        private readonly IApplicationConfiguration _applicationConfiguration;

        public SessionManagerWrapper(string connectionString, string driverName, string dialect, [CanBeNull] IEnumerable<Assembly> assembliesToLookupForMappings, IApplicationConfiguration applicationConfiguration) {
            _connectionString = connectionString;
            _driverName = driverName;
            _dialect = dialect;
            // if we have no mapping, then we are using this as a readonly wrapper, since no entity is mapped
            _isReadOnly = assembliesToLookupForMappings == null;
            _assembliesToLookupForMappings = assembliesToLookupForMappings;
            _applicationConfiguration = applicationConfiguration;
            _sessionFactory = InitSessionFactory();
        }


        private ISessionFactory InitSessionFactory() {
            var configuration = new NHibernate.Cfg.Configuration();
            configuration.AddAssembly(Assembly.GetCallingAssembly());
            IDictionary<string, string> properties = new Dictionary<string, string>();
            properties[NHibernate.Cfg.Environment.ConnectionString] = _connectionString;
            properties.Add(NHibernate.Cfg.Environment.ConnectionDriver, _driverName);
            properties.Add(NHibernate.Cfg.Environment.Dialect, _dialect);
            properties.Add(NHibernate.Cfg.Environment.ShowSql, "false");
            properties.Add(NHibernate.Cfg.Environment.ConnectionProvider, "NHibernate.Connection.DriverConnectionProvider");
            properties.Add(NHibernate.Cfg.Environment.ProxyFactoryFactoryClass, "NHibernate.Bytecode.DefaultProxyFactoryFactory, NHibernate");
            if (_applicationConfiguration.IsLocal()) {
                //overriding to allow better debugging
                properties.Add(NHibernate.Cfg.Environment.CommandTimeout, "600");
            }
            properties.Add(NHibernate.Cfg.Environment.CurrentSessionContextClass, "managed_web");
            configuration.SetProperties(properties);

            if (_assembliesToLookupForMappings != null) {
                var findTypesAnnotattedWith = AttributeUtil.FindTypesAnnotattedWith(_assembliesToLookupForMappings, typeof(ClassAttribute), typeof(JoinedSubclassAttribute));
                foreach (var nHibernateType in findTypesAnnotattedWith) {
                    configuration.AddInputStream(HbmSerializer.Default.Serialize(nHibernateType));
                }
            }

            return configuration.BuildSessionFactory();
        }

        public ISession OpenSession() {

            //nhibernate is always readonly
            var openSession = _sessionFactory.OpenSession();
            if (_isReadOnly){
                openSession.FlushMode = NHibernate.FlushMode.Never;
            }
            return openSession;
        }

        public void Restart() {
            _sessionFactory = InitSessionFactory();
        }
    }
}
