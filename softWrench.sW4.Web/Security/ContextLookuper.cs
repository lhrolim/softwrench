using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using log4net;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Util;
using softWrench.sW4.Web.SPF.Filters;
using LogicalThreadContext = Quartz.Util.LogicalThreadContext;

namespace softWrench.sW4.Web.Security {

    public class ContextLookuper : IContextLookuper {

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

        public void SetInternalQueryExecution()
        {
            var ctx = LookupContext();
            ctx.InternalQueryExecution = true;
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
                Log.DebugFormat("adding context {0}", context);
                return context;
            } catch (Exception e) {
                //not logged users
                return context;
            }
        }
    }
}
