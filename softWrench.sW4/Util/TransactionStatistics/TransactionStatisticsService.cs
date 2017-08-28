using Common.Logging;
using cts.commons.simpleinjector;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using softwrench.sW4.audit.classes.Model;

namespace softWrench.sW4.Util.TransactionStatistics {
    public class TransactionStatisticsService : ISingletonComponent {
        private const string FileName = "transactions_report.xlsx";
        private const string UserDataMissing = "-- user data not found --";
        private const string AuditTrailsForSessionsQuery = "from AuditTrail where SessionId in (:p0)";
        private const string FetchUsersQuery = "from User where Id in (:p0)";
        private readonly SWDBHibernateDAO _swdbDao;
        private readonly ILog log = LogManager.GetLogger(typeof(TransactionStatisticsService));
        private readonly IConfigurationFacade _configurationFacade;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionStatisticsService" class./>
        /// </summary>
        /// <param name="swdbDao">The <see cref="SWDBHibernateDAO"/> instance</param>
        /// <param name="configurationFacade">The <see cref="IConfigurationFacade"/> instance</param>
        public TransactionStatisticsService(SWDBHibernateDAO swdbDao, IConfigurationFacade configurationFacade) {
            _swdbDao = swdbDao;
            _configurationFacade = configurationFacade;
        }

        /// <summary>
        /// Adds a new <see cref="AuditSession"/>
        /// </summary>
        /// <param name="user">The user information <see cref="InMemoryUser"/></param>
        /// <param name="cookie"></param>
        /// <param name="userTimezoneOffset"></param>
        /// <param name="async"></param>
        public virtual void AddSessionAudit(InMemoryUser user, string cookie, int? userTimezoneOffset, bool async = true) {
            try {
                if (user.DBUser.UserName.Equals("jobuser")) {
                    return;
                }

                var session = new AuditSession {
                    UserId = user.UserId,
                    StartDate = DateTime.Now,
                    TimezoneOffSet = userTimezoneOffset,
                    Cookie = AuditSession.HashCookie(cookie)
                };

                if (async) {
                    Task.Run(() => {
                        var sessionAudit = _swdbDao.Save(session);

                        user.SessionAuditId = sessionAudit.Id.Value;
                    });
                } else {
                    var sessionAudit = _swdbDao.Save(session);
                    user.SessionAuditId = sessionAudit.Id.Value;
                }

            } catch (Exception ex) {
                log.Error("Error while auditing the session audit", ex);
            }
        }

        /// <summary>
        /// Updates the session audit with the endtime in UTC
        /// </summary>
        /// <param name="sessionId">The session ID</param>
        public virtual void CloseSessionAudit(int? sessionId) {
            try {
                if (sessionId == null || sessionId == 0) {
                    return;
                }

                Task.Run(() => {
                    var session = _swdbDao.FindSingleByQuery<AuditSession>("From AuditSession where Id = ?", sessionId);

                    if (session != null) {
                        session.EndDate = DateTime.Now;
                        _swdbDao.Save(session);
                    }
                });
            } catch (Exception ex) {
                log.Error("Error while updating the session audit", ex);
            }
        }

        /// <summary>
        /// Adds a transaction <see cref="AuditTrail" object/>
        /// </summary>
        /// <param name="name">The name of the application</param>
        /// <param name="operation">The operation performed. </param>
        /// <param name="start">The transaction start time</param>
        /// <param name="end">The transaction end time</param>
        public virtual void AuditTransaction(string name, string operation, DateTime start, DateTime end) {
            try {
                // Save the transaction audit
                Task.Run(() => {
                    var audit = new AuditTrail {
                        Session = new AuditSession { Id = SecurityFacade.CurrentUser().SessionAuditId },
                        Operation = operation,
                        Name = name,
                        BeginTime = start,
                        EndTime = end
                    };
                    _swdbDao.Save(audit);
                });
            } catch (Exception ex) {
                log.Error("Error while auditing the transaction", ex);
            }
        }

        /// <summary>
        /// Gives an overview of the transactions performed for a daterange 
        /// </summary>
        /// <param name="startDate">The start time</param>
        /// <param name="endDate">The end time</param>
        /// <returns>A <see cref="Tuple" with the login count and the total transaction count./></returns>
        public Tuple<int, int> GetTransactionsOverview(DateTime startDate, DateTime endDate) {
            var auditSessionData = _swdbDao.FindAll<AuditSession>(typeof(AuditSession)).Where(x => x.StartDate.Value >= startDate && x.StartDate.Value <= endDate);
            var auditTrails = _swdbDao.FindByQuery<AuditTrail>(AuditTrailsForSessionsQuery, auditSessionData.Select(x => x.Id).ToArray());
            var loginCount = auditSessionData.Count();
            var totalTx = auditTrails?.Count() ?? 0;

            return new Tuple<int, int>(loginCount, totalTx);
        }

        /// <summary>
        /// Gets the user statistics information for a given session date range 
        /// </summary>
        /// <param name="from">The from DateTime in string.</param>
        /// <param name="to">The to DateTime in string.</param>
        /// <returns>A collection of <see cref="UserStatistics" objects/></returns>
        public List<UserStatistics> ProcessAuditTransactionsForDates(string from, string to) {
            DateTime fromDate, toDate, now = DateTime.Now;
            var period = _configurationFacade.Lookup<int>(ConfigurationConstants.TransactionStatsReportDuration);

            if (string.IsNullOrWhiteSpace(from) || !DateTime.TryParse(from, out fromDate)) {
                fromDate = now.AddDays(period * -1);
            }

            if (string.IsNullOrWhiteSpace(to) || !DateTime.TryParse(to, out toDate)) {
                toDate = now;
            }

            return ProcessAuditTransactionsForUsers(fromDate.ToUniversalTime(), toDate.ToUniversalTime());
        }

        private List<UserStatistics> ProcessAuditTransactionsForUsers(DateTime startDate, DateTime endDate) {
            var userStatistics = new List<UserStatistics>();
            var auditSessionData = _swdbDao.FindAll<AuditSession>(typeof(AuditSession)).Where(x => x.StartDate.Value >= startDate && x.StartDate.Value <= endDate);
            var userIds = auditSessionData.Select(x => x.UserId).Distinct().Where(x => x.HasValue);
            var sessionIds = auditSessionData.Select(x => x.Id).ToArray();
            var auditTrails = _swdbDao.FindByQuery<AuditTrail>(AuditTrailsForSessionsQuery, sessionIds);
            var users = _swdbDao.FindByQuery<User>(FetchUsersQuery, userIds);

            foreach (var session in auditSessionData) {
                var trails = auditTrails.Where(x => x.Session.Id == session.Id);
                var user = users.FirstOrDefault(x => x.Id == session.UserId);

                if (user == null) {
                    user = new User() {
                        Id = -1
                    };
                }

                var userStat = userStatistics.Find(x => x.UserId == user.Id);
                if (userStat == null) {
                    userStat = new UserStatistics() {
                        UserId = user.Id.Value,
                        FullName = user.Id == -1 ? UserDataMissing : $"{user.FullName}({user.UserName})"
                    };

                    userStatistics.Add(userStat);
                }

                if (trails.Any()) {
                    userStat.Transactions.AddRange(trails);
                }
            }

            return userStatistics;
        }
    }
}
