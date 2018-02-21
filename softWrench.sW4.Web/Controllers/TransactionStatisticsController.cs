﻿using softWrench.sW4.Security.Attributes;
using softWrench.sW4.Util;
using softWrench.sW4.Util.TransactionStatistics;
using System.Collections.Generic;
using System.Web.Mvc;
using softwrench.sw4.webcommons.classes.api;

namespace softWrench.sW4.Web.Controllers {
    [NoMenuController]
    public class TransactionStatsReportController : Controller {
        private readonly TransactionStatisticsService service;

        /// <summary>
        /// Initializes a new indtance of the <see cref="TransactionStatsReportController"/> class.
        /// </summary>
        /// <param name="service">The <see cref="TransactionStatisticsService"/> instance.</param>
        public TransactionStatsReportController(TransactionStatisticsService service) {
            this.service = service;
        }

        /// <summary>
        /// Gets the transaction statistics report for a given date range
        /// </summary>
        /// <param name="fromDateFilter">The start datetime</param>
        /// <param name="toDateFilter">The end datetime</param>
        /// <returns></returns>
        [SwAdminAuthorizedMvc]
        public ActionResult GetReport(string fromDateFilter, string toDateFilter) {
            var statistics = this.service.ProcessAuditTransactionsForDates(fromDateFilter, toDateFilter);

            return View("Index", new TransactionStatsReportModel() {
                Title = "Transaction Statistics Report",
                Statistics = statistics
            });
        }

    }


    public class TransactionStatsReportModel : ABaseLayoutModel {

        public override bool PreventPoweredBy => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionStatsReportModel"/> class.
        /// </summary>
        public TransactionStatsReportModel() {
            ClientName = ApplicationConfiguration.ClientName;
        }


        public List<UserStatistics> Statistics { get; set; }
    }
}
