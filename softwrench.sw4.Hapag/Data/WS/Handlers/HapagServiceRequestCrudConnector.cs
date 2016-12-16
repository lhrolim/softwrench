using System.Linq;
using System.Text;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Applications;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softwrench.sw4.Hapag.Data.WS.Handlers {

    class HapagServiceRequestCrudConnector : BaseServiceRequestCrudConnector {
        
        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            var sr = maximoTemplateData.IntegrationObject;
            var crudData = (CrudOperationData)maximoTemplateData.OperationData;
            HandleLongDescription(sr, crudData, maximoTemplateData.ApplicationMetadata);
//            JoinDates(sr, crudData, "affecteddateonly", "affectedtime", "affecteddate");

            base.BeforeCreation(maximoTemplateData);
        }

        private static string FindFormat(ApplicationMetadata metadata, string fieldName, string defaultValue) {
            var field = metadata.Schema.Fields.First(f => f.Attribute == fieldName);
            var dateFormat = field.RendererParameters.FirstOrDefault(f => f.Key == "format").Value;
            return dateFormat as string ?? defaultValue;
        }

        private void HandleLongDescription(object integrationObject, CrudOperationData entity, ApplicationMetadata metadata) {
            var ld = (CrudOperationData)entity.GetRelationship("longdescription");
            var association = entity.EntityMetadata.Associations.First(a => a.To == "longdescription");
            if (ld != null) {
                var originalLd = (string)ld.GetAttribute("ldtext");
                var longDescription = ParseSchemaBasedLD(originalLd, entity, metadata.Schema.SchemaId);
                w.SetValue(integrationObject, "DESCRIPTION_LONGDESCRIPTION", longDescription);
            }
        }

        private string ParseSchemaBasedLD(string originalLd, CrudOperationData entity, string schemaId) {
            var sb = new StringBuilder();

            sb.AppendFormat("Callback Number: {0}\n", entity.GetUnMappedAttribute("callbacknum"));

            if (schemaId == "sd") {

                sb.AppendFormat("Platform Version: {0}\n", entity.GetUnMappedAttribute("platformversion"));
                sb.AppendFormat("Issue: {0}\n", entity.GetUnMappedAttribute("issue"));

            } else if (schemaId == "printer") {

                sb.AppendFormat("Affected Printer: {0}\n", entity.GetUnMappedAttribute("affectedprinter"));
                //sb.AppendFormat("Location of selected printer: {0}\n", entity.GetUnMappedAttribute("printerlocation"));
                sb.AppendFormat("Printer Connection: {0}\n", entity.GetUnMappedAttribute("printerconnection"));
                sb.AppendFormat("Application: {0}\n", entity.GetUnMappedAttribute("application"));
                sb.AppendFormat("Error message on printer / screen: {0}\n", entity.GetUnMappedAttribute("printererrormessage"));
                sb.AppendFormat("Power cable checked: {0}\n", entity.GetUnMappedAttribute("powercablechecked"));
                sb.AppendFormat("Network cable checked: {0}\n", entity.GetUnMappedAttribute("networkcablechecked"));
                sb.AppendFormat("Paper jam: {0}\n", entity.GetUnMappedAttribute("checkedforpaperjam"));
                sb.AppendFormat("Paper empty: {0}\n", entity.GetUnMappedAttribute("checkedforemptytray"));
                sb.AppendFormat("Printing from other PC possible: {0}\n", entity.GetUnMappedAttribute("printingfromotherpc"));
                sb.AppendFormat("Printing using another application possible: {0}\n", entity.GetUnMappedAttribute("printingfromotherapp"));
                sb.AppendFormat("Issue: {0}\n", entity.GetUnMappedAttribute("issue"));

            } else if (schemaId == "outlook") {

                sb.AppendFormat("Problem Affect: {0}\n", entity.GetUnMappedAttribute("problemaffect"));
                sb.AppendFormat("Observe Issue: {0}\n", entity.GetUnMappedAttribute("observedissue"));

            } else if (schemaId == "phone") {

                sb.AppendFormat("Affected Phone: {0}\n", entity.GetUnMappedAttribute("affectedphone"));
                //sb.AppendFormat("Location of selected phone: {0}\n", entity.GetUnMappedAttribute("phonelocation"));
                sb.AppendFormat("Affected Device: {0}\n", entity.GetUnMappedAttribute("affectedDevice"));
                sb.AppendFormat("Problem: {0}\n", entity.GetUnMappedAttribute("whohasproblem"));
                sb.AppendFormat("Symptom: {0}\n", entity.GetUnMappedAttribute("symptomdesc"));
                sb.AppendFormat("Call Direction: {0}\n", entity.GetUnMappedAttribute("calldirection"));
                sb.AppendFormat("Call Type: {0}\n", entity.GetUnMappedAttribute("calltype"));
                sb.AppendFormat("Problem Behavior: {0}\n", entity.GetUnMappedAttribute("problembehavior"));
                sb.AppendFormat("Problem Details: {0}\n", entity.GetUnMappedAttribute("example"));
                sb.AppendFormat("Call Routing: {0}\n", entity.GetUnMappedAttribute("callrouting"));
            }

            sb.AppendFormat("\n\n{0}", originalLd);

            return sb.ToString();
        }
    }
}
