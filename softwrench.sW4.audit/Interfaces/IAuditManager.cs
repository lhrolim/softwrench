﻿using System;
using System.Collections.Generic;
using cts.commons.simpleinjector;
using softwrench.sW4.audit.classes.Model;

namespace softwrench.sW4.audit.Interfaces {
    public interface IAuditManager : ISingletonComponent
    {
        /// <summary>
        /// Creates a new AuditEntry and saves it to the SWDB
        /// </summary>
        /// <param name="action"></param>
        /// <param name="refApplication"></param>
        /// <param name="refId"></param>
        /// <param name="refUserId"></param>
        /// <param name="data"></param>
        /// <param name="createdDate"></param>
        /// <returns>AuditEntry</returns>
        AuditEntry CreateAuditEntry(string action, string refApplication, string refId, string refUserId, string data, DateTime createdDate);
        /// <summary>
        /// Saves an AuditEntry to the SWDB
        /// </summary>
        /// <param name="auditEntry"></param>
        /// <returns>AuditEntry</returns>
        AuditEntry SaveAuditEntry(AuditEntry auditEntry);
        
        /// <summary>
        /// Finds an AuditEntry by its AUDIT_ENTRY.Id
        /// </summary>
        /// <param name="auditId"></param>
        /// <returns>AuditEntry</returns>
        AuditEntry FindById(int auditId);

        /// <summary>
        /// Saves a collection of AuditEntries to the SWDB in a single transaction
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        ICollection<AuditEntry> SaveAuditEntries(ICollection<AuditEntry> entries);
    }
}