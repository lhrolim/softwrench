using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.api.classes.audit;
using softwrench.sw4.api.classes.configuration;
using softwrench.sw4.api.classes.user;
using softwrench.sW4.audit.classes.Model;
using softwrench.sW4.audit.Interfaces;

namespace softwrench.sW4.audit.classes.Services {
    public class AuditManager : IAuditManager, IQueryObserver, ISWEventListener<ConfigurationChangedEvent>, ISWEventListener<ApplicationStartedEvent> {

        private readonly ISWDBHibernateDAO _dao;

        [Import]
        public ISecurityFacade SecurityFacade { get; set; }

        [Import]
        public IConfigurationFacadeCommons ConfigurationFacade { get; set; }

        private bool _isTurnedOn;

        public AuditManager(ISWDBHibernateDAO dao, IMaximoHibernateDAO maxDao) {
            _dao = dao;
            _dao.RegisterQueryObserver(this);
            maxDao.RegisterQueryObserver(this);
        }

        public AuditEntry CreateAuditEntry(string action, string refApplication, string refId, string refUserId, string data, DateTime createdDate) {
            var user = SecurityFacade.Current();
            var auditEntry = new AuditEntry(action, refApplication, refId, refUserId, data, user.Login);
            return SaveAuditEntry(auditEntry);
        }

        public AuditEntry SaveAuditEntry(AuditEntry auditEntry) {
            return _dao.Save(auditEntry);
        }

        public AuditEntry FindById(int auditId) {
            var auditEntry = _dao.FindByPK<AuditEntry>(typeof(AuditEntry), auditId);
            return auditEntry;
        }

        public ICollection<AuditEntry> SaveAuditEntries(ICollection<AuditEntry> entries) {
            return _dao.BulkSave(entries);
        }

        public void AppendToCurrentTrail(AuditEntry entry) {
            var trail = CallContext.GetData("audittrail") as AuditTrail;
            trail?.Entries.Add(entry);
        }


        public void AppendToCurrentTrail(string action, string refApplication, string refId, string refUserId, string data) {
            var trail = CallContext.GetData("audittrail") as AuditTrail;
            var entry = new AuditEntry {
                Action = action,
                RefApplication = refApplication,
                CreatedBy = SecurityFacade.Current().Login,
                CreatedDate = DateTime.Now,
                RefId = refId,
                RefUserId = refUserId
            };


            entry.DataStringValue = data;


            trail?.Entries.Add(entry);
        }

        public void AppendToCurrentTrail(AuditQuery query) {
            var trail = CallContext.GetData("audittrail") as AuditTrail;
            trail?.Queries.Add(query);
        }

        public AuditTrail CurrentTrail() {
            return CallContext.GetData("audittrail") as AuditTrail;
        }

        public void InitThreadTrail(AuditTrail trail) {
            if (CallContext.GetData("audittrail") != null) {
                return;
            }

            CallContext.LogicalSetData("audittrail", trail);
        }

        public void SaveThreadTrail(bool async = true) {
            var trail = CallContext.GetData("audittrail") as AuditTrail;
            if (trail != null) {
                if (!trail.ShouldPersist) {
                    return;
                }

                trail.EndTime = DateTime.Now;
                if (async) {
                    Task.Run(() => {
                        _dao.Save(trail);
                    });
                } else {
                    _dao.Save(trail);
                }
            }
        }

        void IAuditManagerCommons.CreateAuditEntry(string action, string refApplication, string refId, string refUserId, string data) {
            CreateAuditEntry(action, refApplication, refId, refUserId, data, DateTime.Now);
        }

        public void OnQueryExecution(string query, string queryAlias, int? ellapsedTimeMillis) {
            if (queryAlias == null) {
                return;
            }
            var trail = CurrentTrail();

            trail?.Queries.Add(new AuditQuery {
                Query = query,
                Qualifier = queryAlias,
                Ellapsedmillis = ellapsedTimeMillis,
                RegisterTime = DateTime.Now
            });
        }

        public void MarkQueryResolution(string queryAlias, long ellapsedTimeMillis, int? countResult) {
            if (queryAlias == null) {
                return;
            }
            var trail = CurrentTrail();
            var query = trail?.Queries.FirstOrDefault(q => q.Qualifier.EqualsIc(queryAlias));
            if (query != null) {
                query.CountResult = countResult;
                query.Ellapsedmillis = ellapsedTimeMillis;
                query.RegisterTime = DateTime.Now;
            }
        }

        public bool IsTurnedOn() {
            return _isTurnedOn;
        }

        public void HandleEvent(ConfigurationChangedEvent eventToDispatch) {
            if (eventToDispatch.ConfigKey.Equals(AuditConstants.AuditQueryEnabled)) {
                _isTurnedOn = eventToDispatch.CurrentValue.Equals("true");
            }
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            var isTurnedOn = ConfigurationFacade.Lookup<bool>(AuditConstants.AuditQueryEnabled);
            _isTurnedOn = isTurnedOn;
        }
    }
}
