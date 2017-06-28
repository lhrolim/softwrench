using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using cts.commons.persistence;
using softwrench.sw4.user.classes.entities.security;
using System.Linq;
using System.Text;
using cts.commons.persistence.Transaction;
using cts.commons.portable.Util;
using cts.commons.simpleinjector.Events;
using cts.commons.Util;
using log4net;

namespace softwrench.sw4.user.Migration {
    //    [Migration(201706162232)]
    public class MigrationSwweb3026 : ISWEventListener<ApplicationStartedEvent> {


        [Import]
        public ISWDBHibernateDAO DAO { get; set; }

        private ILog Log = LogManager.GetLogger(typeof(MigrationSwweb3026));

        public static string GenerateInString(IEnumerable<int?> items) {
            var enumerable = items as ISet<int?> ?? items.ToHashSet();
            Validate.NotEmpty(enumerable, "items");

            var sb = new StringBuilder();
            foreach (var item in enumerable) {
                sb.Append("'").Append(item).Append("'");
                sb.Append(",");
            }
            return sb.ToString(0, sb.Length - 1);
        }


        [Transactional(DBType.Swdb)]
        public virtual void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            //Cannot run this now as several customers would fail



            var result = DAO.FindSingleByNativeQuery<object>("select 1 from versioninfo where version = '201706162232'");
            if (result != null) {
                return;
            }

            Log.InfoFormat("running sec_app_permission cleanup");

            var permissions = DAO.FindAll<ApplicationPermission>(typeof(ApplicationPermission));

            var dict = new Dictionary<Tuple<int?, string>, List<int?>>();

            foreach (var permission in permissions) {
                var tuple = new Tuple<int?, string>(permission.Profile.Id, permission.ApplicationName);
                if (!dict.ContainsKey(tuple)) {
                    dict[tuple] = new List<int?>();
                }
                dict[tuple].Add(permission.Id);
            }

            var toDelete = new List<int?>();

            foreach (var key in dict.Keys) {
                var items = dict[key];
                if (items.Count >= 2) {
                    //deleting all but last
                    toDelete.AddRange(items.Take(items.Count - 1));
                }
            }

            if (toDelete.Any()) {
                DAO.ExecuteSql("delete from SEC_FIELD_PER where schema_id in (select id from SEC_CONTAINER_PER where app_id in ({0}))".Fmt(GenerateInString(toDelete)));
                DAO.ExecuteSql("delete from SEC_CONTAINER_PER where app_id in ({0})".Fmt(GenerateInString(toDelete)));
                DAO.ExecuteSql("delete from SEC_COMPOSITION_PER where app_id in ({0})".Fmt(GenerateInString(toDelete)));
                DAO.ExecuteSql("delete from SEC_ACTION_PER where app_id in ({0})".Fmt(GenerateInString(toDelete)));
                DAO.ExecuteSql("delete from SEC_APPLICATION_PER where id in ({0})".Fmt(GenerateInString(toDelete)));
            }

            DAO.ExecuteSql("ALTER TABLE SEC_APPLICATION_PER ADD CONSTRAINT uq_app_profile UNIQUE (profile_id,applicationname)");

            DAO.ExecuteSql("insert into VersionInfo (version, appliedOn, Description) values ('{0}','{1}','{2}')".Fmt("201706162232", DateTime.Now.ToShortDateString(), "MigrationSwweb3026"));

        }
    }




}
