using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using softwrench.sw4.dynforms.classes.model.entity;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Util;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Parsing;

namespace softwrench.sw4.dynforms.classes.model.metadata {

    public class DynFormSchemaHandler : ISingletonComponent, ISWEventListener<RefreshMetadataEvent> {
        private const string ApplicationName = "_FormDatamap";

        private readonly ConcurrentDictionary<string, ApplicationSchemaDefinition> _cachedSchemas = new ConcurrentDictionary<string, ApplicationSchemaDefinition>();

        [Import]
        private SWDBHibernateDAO SWDAO { get; set; }

        private static string NewFormSchemaDefinition = @"

            <detail title=""New Form"" platform=""web"" stereotype=""detail.noprint"">
                <section resourcepath=""/Content/Shared/dynforms/htmls/dynformdetailblank.html""/>
            </detail>

        ";

        private static string NewGridSchemaDefinition = @"

            <list title=""New Form"" platform=""web"">
                <field label=""New Field"" attribute=""newfield"" />
            </list>

        ";

        private static XmlApplicationMetadataParser _parser;


        private JsonSerializerSettings _jsonSettings =
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };





        [NotNull]
        public async Task<ApplicationSchemaDefinition> LookupSchema(FormMetadata form, bool isList) {
            var formName = form.Name;
            //TODO: new and detail
            var cacheKey = formName + (isList ? ":list" : ":detail");
            if (_cachedSchemas.ContainsKey(cacheKey)) {
                return _cachedSchemas[cacheKey];
            }
            form = form.Definition == null ? await SWDAO.FindByPKAsync<FormMetadata>(form.Name) : form;
            var tuple = ValidateAndCache(form.Name, form.Definition);
            return isList ? tuple.Item1 : tuple.Item2;
        }

        public async Task<ApplicationSchemaDefinition> LookupOrGenerateInitialSchema(FormMetadata form, bool isList) {
            var savedSchema = await LookupSchema(form, isList);
            if (savedSchema != null) {
                return savedSchema;
            }

            //            var definition = FormMetadataDefinition.FromXmls(NewGridSchemaDefinition, NewFormSchemaDefinition);
            var schemas = await SerializeValidateAndCache(form, NewFormSchemaDefinition, NewGridSchemaDefinition);
            return isList ? schemas.Item1 : schemas.Item2;

        }

        public async Task ReplaceDetailDisplayables(string formName, string newFieldsJSON) {
            var formMetadata = await SWDAO.FindByPKAsync<FormMetadata>(formName);



            var newDisplayables = JsonConvert.DeserializeObject<List<IApplicationDisplayable>>(newFieldsJSON, _jsonSettings);
            var linearDisplayable = DisplayableUtil.GetDisplayable<IApplicationAttributeDisplayable>(typeof(IApplicationDisplayable), newDisplayables).Where(x=> !(x is ApplicationSection));
            var duplicateFields = linearDisplayable.GroupBy(d => d.Attribute).Where(g => g.Count() > 1);

            if (duplicateFields.Any()) {
                throw new InvalidOperationException("field attributes must be unique within a form");
            }


            var schema = await LookupSchema(formMetadata, false);
            //keeping the standard items intact
            var baseDisplayables = schema.Displayables.Take(2).ToList();
            baseDisplayables.AddRange(newDisplayables);
            schema.Displayables = baseDisplayables;
            var detailSerialized = JsonConvert.SerializeObject(schema, _jsonSettings);
            formMetadata.Definition.DetailDefinitionStringValue = detailSerialized;
            await SWDAO.SaveAsync(formMetadata.Definition);
            CacheSchema(formName, false, schema);
        }

        public async Task<Tuple<ApplicationSchemaDefinition, ApplicationSchemaDefinition>> SerializeValidateAndCache(FormMetadata metadata, string detailXml, string listXml) {
            var definition = metadata.Definition ?? new FormMetadataDefinition { Metadata = metadata };
            metadata.Definition = definition;

            ApplicationSchemaDefinition listSchema = null;
            ApplicationSchemaDefinition detailSchema = null;

            var listSerialized = definition.ListDefinitionStringValue;
            var detailSerialized = definition.DetailDefinitionStringValue;

            var formTitle = metadata.FormTitle;
            XElement xElement;

            if (listXml != null && listXml.Trim().StartsWith("<")) {
                xElement = XElement.Parse(listXml);
                listSchema = GetParser().DoParseSchema(ApplicationName, formTitle, "FormDatamap_", "id", "id", xElement, new Dictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition>());
            }

            if (detailXml != null && detailXml.Trim().StartsWith("<")) {
                xElement = XElement.Parse(detailXml);
                detailSchema = GetParser().DoParseSchema(ApplicationName, formTitle, "FormDatamap_",
                    "id", "id", xElement, new Dictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition>());
                detailSerialized = JsonConvert.SerializeObject(detailSchema, _jsonSettings);
            }

            if (listSchema != null && detailSchema != null) {
                listSchema.NewSchemaRepresentation = new SchemaRepresentation {
                    Label = "New",
                    SchemaId = detailSchema.SchemaId
                };
                listSerialized = JsonConvert.SerializeObject(listSchema, _jsonSettings);
            }
            metadata.Definition.ListDefinitionStringValue = listSerialized;
            metadata.Definition.DetailDefinitionStringValue = detailSerialized;

            definition = await SWDAO.SaveAsync(metadata.Definition);
            return ValidateAndCache(metadata.Name, definition);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="formName"></param>
        /// <returns>item1 = list schema, item2 = detail schema</returns>
        private Tuple<ApplicationSchemaDefinition, ApplicationSchemaDefinition> ValidateAndCache(string formName, FormMetadataDefinition definition) {

            ApplicationSchemaDefinition listSchema = null;

            if (definition.ListSerialized != null) {
                listSchema = JsonConvert.DeserializeObject<ApplicationSchemaDefinition>(definition.ListDefinitionStringValue, _jsonSettings);
            }

            ApplicationSchemaDefinition detailSchema = null;

            if (definition.DetailSerialized != null) {
                detailSchema = JsonConvert.DeserializeObject<ApplicationSchemaDefinition>(definition.DetailDefinitionStringValue, _jsonSettings);
                detailSchema.IdFieldName = "id";
                detailSchema.Properties["commandbar.top"] = "detail.primary";
                detailSchema.Properties.Remove("toolbar.detail.actions");
                detailSchema.Properties.Remove("dynforms.editionallowed");
            }

            if (listSchema != null && detailSchema != null) {
                listSchema.NewSchemaRepresentation = new SchemaRepresentation() {
                    Label = "New",
                    SchemaId = detailSchema.SchemaId
                };
            }

            CacheSchema(formName, false, detailSchema);
            CacheSchema(formName, true, listSchema);

            return new Tuple<ApplicationSchemaDefinition, ApplicationSchemaDefinition>(listSchema, detailSchema);
        }



        private void CacheSchema(string formName, bool list, ApplicationSchemaDefinition schema) {

            //TODO: new and detail
            var cacheKey = formName + (list ? ":list" : ":detail");

            if (_cachedSchemas.ContainsKey(cacheKey)) {
                ApplicationSchemaDefinition existingSchema = null;
                _cachedSchemas.TryRemove(cacheKey, out existingSchema);
            }
            if (schema != null) {
                _cachedSchemas[cacheKey] = schema;
            }

        }


        private static XmlApplicationMetadataParser GetParser() {
            return _parser ?? (_parser = new XmlApplicationMetadataParser(MetadataProvider.Entities(true),
                       MetadataProvider.CommandBars(), true, false));
        }


        public void HandleEvent(RefreshMetadataEvent eventToDispatch) {
            _cachedSchemas.Clear();
        }
    }
}
