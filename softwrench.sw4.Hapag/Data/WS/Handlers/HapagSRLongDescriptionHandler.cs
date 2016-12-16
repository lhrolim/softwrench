using System.Text;
using softWrench.sW4.Data.Persistence.Operation;

namespace softwrench.sw4.Hapag.Data.WS.Handlers {
    class HapagSRLongDescriptionHandler {


        public static string ParseSchemaBasedLongDescription(string originalLd, CrudOperationData entity, string schemaId) {
            var sb = new StringBuilder();

            sb.AppendFormat("Affected Location: {0}\n", entity.GetAttribute("#pluspcustomer_label"));
            sb.AppendFormat("Callback Number: {0}\n", entity.GetAttribute("callbacknum"));
            
            if (schemaId == "sd") {

                sb.AppendFormat("Platform Version: {0}\n", entity.GetAttribute("platformversion"));
                sb.AppendFormat("Issue: {0}\n", entity.GetAttribute("issue"));

            } else if (schemaId == "printer") {

                sb.AppendFormat("Affected Printer: {0}\n", entity.GetAttribute("#assetnum_label"));
                //sb.AppendFormat("Location of selected printer: {0}\n", entity.GetUnMappedAttribute("printerlocation"));
                sb.AppendFormat("Printer Connection: {0}\n", entity.GetAttribute("printerconnection"));
                sb.AppendFormat("Application: {0}\n", entity.GetAttribute("application"));
                sb.AppendFormat("Error message on printer / screen: {0}\n", entity.GetAttribute("printererrormessage"));
                sb.AppendFormat("Power cable checked: {0}\n", entity.GetAttribute("powercablechecked"));
                sb.AppendFormat("Network cable checked: {0}\n", entity.GetAttribute("networkcablechecked"));
                sb.AppendFormat("Paper jam: {0}\n", entity.GetAttribute("checkedforpaperjam"));
                sb.AppendFormat("Paper empty: {0}\n", entity.GetAttribute("checkedforemptytray"));
                sb.AppendFormat("Printing from other PC possible: {0}\n", entity.GetAttribute("printingfromotherpc"));
                sb.AppendFormat("Printing using another application possible: {0}\n", entity.GetAttribute("printingfromotherapp"));
                sb.AppendFormat("Issue: {0}\n", entity.GetAttribute("issue"));

            } else if (schemaId == "outlook") {

                sb.AppendFormat("Problem Affect: {0}\n", entity.GetAttribute("problemaffect"));
                sb.AppendFormat("Observe Issue: {0}\n", entity.GetAttribute("observedissue"));

            } else if (schemaId == "phone") {

                sb.AppendFormat("Affected Phone: {0}\n", entity.GetAttribute("#assetnum_label"));
                //sb.AppendFormat("Location of selected phone: {0}\n", entity.GetUnMappedAttribute("phonelocation"));
                sb.AppendFormat("Affected Device: {0}\n", entity.GetAttribute("affectedDevice"));
                sb.AppendFormat("Problem: {0}\n", entity.GetAttribute("whohasproblem"));
                sb.AppendFormat("Symptom: {0}\n", entity.GetAttribute("symptomdesc"));
                sb.AppendFormat("Call Direction: {0}\n", entity.GetAttribute("calldirection"));
                sb.AppendFormat("Call Type: {0}\n", entity.GetAttribute("calltype"));
                sb.AppendFormat("Problem Behavior: {0}\n", entity.GetAttribute("problembehavior"));
                sb.AppendFormat("Problem Details: {0}\n", entity.GetAttribute("example"));
                sb.AppendFormat("Call Routing: {0}\n", entity.GetAttribute("callrouting"));
            }

            sb.AppendFormat("\n\n{0}", originalLd);

            return sb.ToString();
        }

    }
}
