using System;
using System.Data.Common;
using System.Reflection;
using FluentMigrator.Runner.Processors;

namespace softWrench.sW4.Web.DB_Migration.DB2_FluentMigrator {

    public class Db2DbFactory : ReflectionBasedDbFactory {
        #region Constructors

        public Db2DbFactory()
            : base("IBM.Data.DB2", "IBM.Data.DB2.DB2Factory") {
        }

        #endregion Constructors

        #region Methods

        protected override DbProviderFactory CreateFactory() {
            var assembly = AppDomain.CurrentDomain.Load("IBM.Data.DB2, Version=9.0.0.2, Culture=neutral, PublicKeyToken=7c307b91aa13d208");
            var type = assembly.GetType("IBM.Data.DB2.DB2Factory");
            var field = type.GetField("Instance", BindingFlags.Static | BindingFlags.Public);

            if (field == null) {
                return base.CreateFactory();
            }

            return (DbProviderFactory)field.GetValue(null);
        }

        #endregion Methods
    }
}
