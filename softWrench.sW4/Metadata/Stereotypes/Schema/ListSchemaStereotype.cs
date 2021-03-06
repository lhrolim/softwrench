﻿using System.Linq;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Metadata.Applications.Schema;
using System.Collections.Generic;

namespace softWrench.sW4.Metadata.Stereotypes.Schema {

    class ListSchemaStereotype : ASchemaStereotype {

        public const string ListClickMode = "list.click.mode";
        public const string ListClickPopup = "list.click.popupmode";
        public const string ListClickService = "list.click.service";
        public const string ListPaginationSize = ApplicationSchemaPropertiesCatalog.DefaultPaginationSize;
        public const string ListPaginationOptions = ApplicationSchemaPropertiesCatalog.PaginationOptions;


        private static ListSchemaStereotype _instance;

        public static ListSchemaStereotype GetInstance() {
            return _instance ?? (_instance = new ListSchemaStereotype());
        }

        private ListSchemaStereotype() { }

        protected override IDictionary<string, string> DefaultValues() {

            var defaultValues = new Dictionary<string, string>();

            if (!defaultValues.Keys.Contains(ListClickMode)) {
                defaultValues.Add(ListClickMode, SchemaMode.input.ToString());
            }
            if (!defaultValues.Keys.Contains(ListClickPopup)) {
                defaultValues.Add(ListClickPopup, PopupMode.none.ToString());
            }
            if (!defaultValues.Keys.Contains(ListPaginationSize)) {
                defaultValues.Add(ListPaginationSize, "30");
            }
            if (!defaultValues.Keys.Contains(ListPaginationOptions)) {
                defaultValues.Add(ListPaginationOptions, "10,30,100");
            }

            var propPaginationSize = MetadataProvider.GlobalProperties.GlobalProperty(ListPaginationSize);
            if (propPaginationSize != null) {
                defaultValues[ListPaginationSize] = propPaginationSize;
            }
            var propPaginationOptions = MetadataProvider.GlobalProperties.GlobalProperty(ListPaginationOptions);
            if (propPaginationOptions != null) {
                defaultValues[ListPaginationOptions] = propPaginationOptions;
            }

            return defaultValues;
        }
    }
}
