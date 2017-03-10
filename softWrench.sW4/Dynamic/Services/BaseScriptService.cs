using System.Collections.Generic;
using System.Text.RegularExpressions;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using softWrench.sW4.Dynamic.Model;
using softWrench.sW4.Util;

namespace softWrench.sW4.Dynamic.Services {

    public abstract class BaseScriptService : IComponent {

        protected readonly Regex IsImport = new Regex(@"^\s*using\s+.*$", RegexOptions.Multiline);
   

        protected readonly ISWDBHibernateDAO DAO;
        
        protected string SystemVersion;

        protected BaseScriptService(ISWDBHibernateDAO dao) {
            DAO = dao;
        }

        public virtual string GetSystemVersion() {
            if (!string.IsNullOrEmpty(SystemVersion)) {
                return SystemVersion;
            }
            SystemVersion = ApplicationConfiguration.SystemVersionIgnoreDash;
            return SystemVersion;
        }

        public virtual bool SameScript(string scriptA, string scriptB) {
            var spacelessScriptA = Regex.Replace(scriptA, "\\s", "");
            var spacelessScriptB = Regex.Replace(scriptB, "\\s", "");
            return spacelessScriptA.Equals(spacelessScriptB);
        }


        public abstract void ReloadContainer(AScriptEntry entry);
    }
}
