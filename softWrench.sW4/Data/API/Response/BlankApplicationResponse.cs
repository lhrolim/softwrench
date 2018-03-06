﻿using System;
using softwrench.sw4.api.classes.integration;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Data.API.Response {

    /// <summary>
    /// Use this response when no action is required on the client side.
    /// </summary>
    public class BlankApplicationResponse : IApplicationResponse {

        public string AliasURL { get; set; }
        public string RedirectURL { get; set; }
        public string Title { get; set; }
        public string CrudSubTemplate { get; set; }
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public bool FullRefresh { get; set; } = false;

        public IErrorDto ErrorDto { get; set; }

        public DateTime TimeStamp { get; set; }
        public virtual string Type { get { return typeof (BlankApplicationResponse).Name; } }
        public ApplicationSchemaDefinition Schema { get; set; }
        public string CachedSchemaId { get; set; }
        public string Mode { get; set; }
        public string ApplicationName { get; private set; }
        public string Id { get; set; }
        public string UserId { get; set; }
        public IErrorDto WarningDto { get; set; }
    }
}
