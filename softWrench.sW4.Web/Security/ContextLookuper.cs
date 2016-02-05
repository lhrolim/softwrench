using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using cts.commons.simpleinjector;
using Microsoft.Ajax.Utilities;
using softwrench.sw4.api.classes.fwk.context;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;
using LogicalThreadContext = Quartz.Util.LogicalThreadContext;

namespace softWrench.sW4.Web.Security {

    public class ContextLookuper : IContextLookuper {

        private static readonly IDictionary<string, object> MemoryContext = new ConcurrentDictionary<string, object>();

        private static readonly ILog Log = LogManager.GetLogger(typeof(ContextLookuper));

        private IWhereClauseFacade _whereClauseFacade;

        //To avoid circular dependencies
        private IWhereClauseFacade WhereClauseFacade {
            get {
                if (_whereClauseFacade != null) {
                    return _whereClauseFacade;
                }
                _whereClauseFacade =
                    SimpleInjectorGenericFactory.Instance.GetObject<IWhereClauseFacade>(typeof(IWhereClauseFacade));
                return _whereClauseFacade;
            }
        }

        public static ContextLookuper GetInstance() {
            return SimpleInjectorGenericFactory.Instance.GetObject<ContextLookuper>(typeof(ContextLookuper));
        }


        public ContextHolder LookupContext() {
            var isHttp = HttpContext.Current != null;
            var context = isHttp ? HttpContext.Current.Items["context"] : LogicalThreadContext.GetData<ContextHolder>("context");
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

            var isHttp = HttpContext.Current != null;
            if (isHttp) {
                HttpContext.Current.Items["context"] = context;
            } else {
                LogicalThreadContext.SetData("context", context);
            }
        }

        public void FillGridContext(string applicationName, InMemoryUser user) {
            var context = (ContextHolder)ReflectionUtil.Clone(new ContextHolder(), LookupContext());

            var availableProfiles = WhereClauseFacade.ProfilesByApplication(applicationName, user);
            context.AvailableProfilesForGrid = availableProfiles;
            if (availableProfiles.Any() && context.CurrentSelectedProfile == null) {
                //if the profile was already set at client side, let´s not change it
                context.CurrentSelectedProfile = availableProfiles.First().Id;
            }
            var isHttp = HttpContext.Current != null;
            if (isHttp) {
                HttpContext.Current.Items["context"] = context;
            } else {
                LogicalThreadContext.SetData("context", context);
            }
        }

        public void SetMemoryContext(string key, object ob, bool userSpecific = false) {
            if (key == null) {
                return;
            }
            var user = SecurityFacade.CurrentUser();
            var login = userSpecific ? user.Login : null;
            //            var userKey = new UserKey(login, key);
            MemoryContext.Add(key, ob);
        }

        public void RemoveFromMemoryContext(string key, bool userSpecific = false) {
            if (key == null) {
                return;
            }
            var user = SecurityFacade.CurrentUser();
            var login = userSpecific ? user.Login : null;
            //            var userKey = new UserKey(login, key);
            MemoryContext.Remove(key);
        }

        public T GetFromMemoryContext<T>(string key, bool userSpecific = false) {
            if (!MemoryContext.ContainsKey(key)) {
                Log.WarnFormat("object {0} not found in memory", key);
                return default(T);
            }
            return (T)MemoryContext[key];

        }

        public ContextHolder AddContext(ContextHolder context, bool isHttp) {
            context.Environment = ApplicationConfiguration.Profile;
            if (isHttp) {
                HttpContext.Current.Items["context"] = context;
            } else {
                LogicalThreadContext.SetData("context", context);
            }
            try {
                var user = SecurityFacade.CurrentUser();
                if (user == null) {
                    return context;
                }

                context.UserProfiles = new SortedSet<int?>(user.Profiles.Select(s => s.Id));
                context.Environment = ApplicationConfiguration.Profile;
                context.OrgId = user.OrgId;
                context.SiteId = user.SiteId;
                if (isHttp) {
                    HttpContext.Current.Items["context"] = context;
                } else {
                    LogicalThreadContext.SetData("context", context);
                }
                Log.DebugFormat("adding context " + context);
                return context;
            } catch (Exception) {
                //not logged users
                return context;
            }
        }


        public void RegisterHttpContext(HttpRequest request) {
            if (MemoryContext.ContainsKey("httpcontext")) {
                //already registered
                return;
            }
            var uri = request.Url;
            if (!MemoryContext.ContainsKey("httpcontext"))
            {
                string uriProperty = MetadataProvider.GlobalProperty("iiscontextpath ");
                if (!uriProperty.IsNullOrWhiteSpace()) {
                    uri = new Uri(uriProperty);
                }
                MemoryContext.Add("httpcontext", new SwHttpContext(uri.Scheme, uri.Host, uri.Port, uri.AbsolutePath));
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
            if (obj.GetType() != GetType()) return false;
            return Equals((UserKey)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((UserName != null ? UserName.GetHashCode() : 0) * 397) ^ (Key != null ? Key.GetHashCode() : 0);
            }
        }
    }
}
