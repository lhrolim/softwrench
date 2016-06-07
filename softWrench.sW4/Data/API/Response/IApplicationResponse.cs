using System;
using softwrench.sw4.api.classes.integration;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.SPF;

namespace softWrench.sW4.Data.API.Response {
    public interface IApplicationResponse :IGenericResponseResult {
        string Type { get; }
        ApplicationSchemaDefinition Schema { get; set; }
        String CachedSchemaId { get; set; }

        string Mode { get; set; }

        string ApplicationName { get; }

        string Id { get; }

        /// <summary>
        /// this Dto should be set for scenarios where no exceptions are thrown but a warning must be displayed for the users.
        /// 
        /// It can contain an exception within it, in that case it would bring the more info window
        /// </summary>
        IErrorDto WarningDto { get; set; }

       
    }
}