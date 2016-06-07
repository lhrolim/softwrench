﻿using System;
using System.Collections.Generic;
using System.Linq;
using softwrench.sw4.api.classes.integration;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Data.API.Response {
    public class SchemaChoosingDataResponse : IApplicationResponse {
        private string _mode;
        public string Type { get { return null; } }
        public ApplicationSchemaDefinition Schema
        {
            get { return null; }
            set { }
        }

        public string CachedSchemaId { get; set; }

        public string Mode
        {
            get { return SchemaMode.input.ToString().ToLower(); }
            set { _mode = value; }
        }


        public SchemaChoosingDataResponse(IList<ApplicationSchemaDefinition> schemas, string label, string placeHolder) {
            SchemaSelectionLabel = label;
            Schemas = schemas;
            PlaceHolder = placeHolder;
        }

        public string SchemaSelectionLabel { get; set; }

        public string PlaceHolder { get; set; }

        public string ApplicationName { get { return Schemas.First().ApplicationName; } }
        public string Id { get; private set; }
        public IErrorDto WarningDto { get; set; }

        public IList<ApplicationSchemaDefinition> Schemas { get; set; }
        public string RedirectURL { get; set; }
        public string Title { get; set; }
        public string CrudSubTemplate { get; set; }
        public string SuccessMessage { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
