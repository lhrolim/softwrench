using FluentMigrator.Runner.Processors;
using System;
using System.Data.Common;
using System.Reflection;

namespace FluentMigratorDB2Port.FluentMigrator.Runner.Processors.DB2
{
    public class Db2WindowsDbFactory : ReflectionBasedDbFactory
    {
        #region Constructors

        public Db2WindowsDbFactory()
            : base("IBM.Data.DB2", "IBM.Data.DB2.DB2Factory"){
        }

        #endregion Constructors

        #region Methods

        protected override DbProviderFactory CreateFactory()
        {
            var assembly = AppDomain.CurrentDomain.Load("IBM.Data.DB2, Version=9.7.4.4, Culture=neutral, PublicKeyToken=7c307b91aa13d208");
            var type = assembly.GetType("IBM.Data.DB2.DB2Factory");
            var field = type.GetField("Instance", BindingFlags.Static | BindingFlags.Public);

            if (field == null)
            {
                return base.CreateFactory();
            }

            return (DbProviderFactory)field.GetValue(null);
        }

        #endregion Methods
    }
}