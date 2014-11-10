using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DocumentFormat.OpenXml.Office2010.Excel;
using log4net;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Util;
using softWrench.sW4.Web.SPF.Filters;
using LogicalThreadContext = Quartz.Util.LogicalThreadContext;

namespace softWrench.sW4.Web.Security {

    public class ContextLookuper : IContextLookuper {

        private static readonly IDictionary<string, object> _memoryContext = new ConcurrentDictionary<string, object>();

        private static ILog Log = LogManager.GetLogger(typeof(ContextLookuper));

        public ContextHolder LookupContext() {
            var isHttp = System.Web.HttpContext.Current != null;
            var context = isHttp ? System.Web.HttpContext.Current.Items["context"] : LogicalThreadContext.GetData<ContextHolder>("context");
            return (ContextHolder)(context ?? AddContext(new ContextHolder(), isHttp));
        }

        public void FillContext(ApplicationMetadataSchemaKey key) {

            var context = (ContextHolder)ReflectionUtil.Clone(new ContextHolder(), LookupContext());

            Log.DebugFormat("filling {0} into context {1}", key, context);
            if (context.ApplicationLookupContext == null) {
                Log.DebugFormat("no context found");
                context.ApplicationLookupContext = new ApplicationLookupContext();
            }
            var appContext = (ApplicationLookupContext)ReflectionUtil.Clone(new ApplicationLookupContext(), context.ApplicationLookupContext);
            appContext.Schema = key.SchemaId;
            appContext.Mode = key.Mode.ToString();
            context.ApplicationLookupContext = appContext;

            var isHttp = System.Web.HttpContext.Current != null;
            if (isHttp) {
                System.Web.HttpContext.Current.Items["context"] = context;
            } else {
                LogicalThreadContext.SetData("context", context);
            }
        }

        public void SetMemoryContext(string key, object ob, bool userSpecific = false) {
            var user = SecurityFacade.CurrentUser();
            var login = userSpecific ? user.Login : null;
//            var userKey = new UserKey(login, key);
            _memoryContext.Add(key, ob);
        }

        public void RemoveFromMemoryContext(string key, bool userSpecific = false) {
            var user = SecurityFacade.CurrentUser();
            var login = userSpecific ? user.Login : null;
//            var userKey = new UserKey(login, key);
            _memoryContext.Remove(key);
        }

        public T GetFromMemoryContext<T>(string key) {
            var user = SecurityFacade.CurrentUser();
            var userKey = new UserKey(user.Login, key);
            if (!_memoryContext.ContainsKey(key)) {
                Log.WarnFormat("object {0} not found in memory", key);
                return default(T);
            }
            return (T)_memoryContext[key];

        }

        public static ContextHolder AddContext(ContextHolder context, bool isHttp) {
            context.Environment = ApplicationConfiguration.Profile;
            if (isHttp) {
                System.Web.HttpContext.Current.Items["context"] = context;
            } else {
                LogicalThreadContext.SetData("context", context);
            }
            try {
                var user = SecurityFacade.CurrentUser();
                context.UserProfiles = new SortedSet<int?>(user.Profiles.Select(s => s.Id));
                context.Environment = ApplicationConfiguration.Profile;
                context.OrgId = user.OrgId;
                context.SiteId = user.SiteId;
                if (isHttp) {
                    System.Web.HttpContext.Current.Items["context"] = context;
                } else {
                    LogicalThreadContext.SetData("context", context);
                }
                Log.DebugFormat("adding context " + context);
                return context;
            } catch (Exception e) {
                //not logged users
                return context;
            }
        }
    }

    class UserKey {
        public UserKey(string userName, string key) {
            if (userName == null) {
                //To make hash code and equals work fine
                userName = "#null";
            }
            UserName = userName;
            Key = key;
        }

        internal string UserName;
        internal string Key;

        protected bool Equals(UserKey other) {
            return (UserName == null || string.Equals(UserName, other.UserName)) && string.Equals(Key, other.Key);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((UserKey)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((UserName != null ? UserName.GetHashCode() : 0) * 397) ^ (Key != null ? Key.GetHashCode() : 0);
            }
        }
    }
}
