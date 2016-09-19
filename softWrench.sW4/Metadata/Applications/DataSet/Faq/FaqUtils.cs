using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using log4net;
using softwrench.sW4.Shared2.Metadata.Applications;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Security.Services;

namespace softWrench.sW4.Metadata.Applications.DataSet.Faq {
    public class FaqUtils {

        private static readonly ILog Log = LogManager.GetLogger(typeof(FaqUtils));

        private static EntityRepository EntityRepository {
            get {
                return SimpleInjectorGenericFactory.Instance.GetObject<EntityRepository>(typeof(EntityRepository));
            }
        }

        private static IEnumerable<FaqData> GetBuiltList(IEnumerable<FaqData> list, IEnumerable<UsefulFaqLinksUtils> usefulFaqLinksUtils) {
            try {
                var buildedList = new List<FaqData>();
                var usefulFaqLinksUtilAux = usefulFaqLinksUtils as IList<UsefulFaqLinksUtils> ?? usefulFaqLinksUtils.ToList();
                var firstUsefulFaqLink = usefulFaqLinksUtilAux.FirstOrDefault();
                if (firstUsefulFaqLink != null) {

                    foreach (var item in list) {
                        FaqDescription descriptor;
                        try {
                            Log.DebugFormat("faq description: {0}", item.Description);
                            descriptor = new FaqDescription(item.Description);
                        } catch (Exception e) {
                            Log.Error(e);
                            continue;
                        }
                        if (!descriptor.IsValid()) {
                            Log.WarnFormat("faq description not valid for {0}", descriptor.Id);
                            continue;
                        }

                        if (descriptor.Language != firstUsefulFaqLink.Language ||
                            usefulFaqLinksUtilAux.All(x => x.Id != descriptor.Id)) {
                            Log.DebugFormat("faq description {0} not appliable for schema ", descriptor.Id);
                            continue;
                        }
                        buildedList.Add(new FaqData(item.SolutionId, descriptor.Id, descriptor.RealDescription,
                            firstUsefulFaqLink.Language));
                    }
                    return buildedList;
                }
            } catch (Exception e) {
                Log.Error(e);
                return null;
            }
            return null;
        }

        private readonly DataSetProvider _dataSetProvider = DataSetProvider.GetInstance();

        [NotNull]
        private async Task<IApplicationResponse> Get(string application, [FromUri] DataRequestAdapter request) {
            var user = SecurityFacade.CurrentUser();

            if (null == user) {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            var applicationMetadata = MetadataProvider
                .Application(application)
                .ApplyPolicies(request.Key, user, ClientPlatform.Web, null);

            return await _dataSetProvider.LookupDataSet(application, applicationMetadata.Schema.SchemaId).Get(applicationMetadata, user, request);
        }

        #region Public



        public struct FaqData {
            public int SolutionId { get; set; }
            public string Description { get; set; }
            public string Lang { get; set; }
            public string Faqid { get; set; }

            public FaqData(string description)
                : this() {
                SolutionId = Convert.ToInt32(description.Substring(1, 4));
                Description = description;
            }

            public FaqData(int solutionId, string description)
                : this() {
                SolutionId = solutionId;
                Description = description;
            }

            public FaqData(int solutionId, string faqId, string description, string lang)
                : this() {
                SolutionId = solutionId;
                Faqid = lang + faqId;
                Description = description;
                Lang = lang;
            }

            public override string ToString() {
                return string.Format("SolutionId: {0}, Description: {1}, Lang: {2}, Faqid: {3}", SolutionId, Description, Lang, Faqid);
            }
        }


        public static async Task<IEnumerable<FaqData>> GetUsefulFaqLinks(List<UsefulFaqLinksUtils> usefulFaqLinksUtils) {
            var listToUse = await new FaqUtils().GetList();
            var builtList = GetBuiltList(listToUse, usefulFaqLinksUtils);
            return builtList;
        }

        public async Task<IEnumerable<FaqData>> GetList() {
            var dto = new SearchRequestDto();
            dto.AppendSearchEntry("pluspinsertcustomer", "HLC-%");
            dto.AppendSearchEntry("status", "ACTIVE");
            dto.AppendProjectionField(ProjectionField.Default("solutionid"));
            dto.AppendProjectionField(ProjectionField.Default("description"));
            var result = await EntityRepository.GetAsRawDictionary(MetadataProvider.Entity("solution"), dto);
            Log.DebugFormat("db size {0}", result.ResultList.Count());
            var treeDataList = new List<FaqData>();
            foreach (var attributeHolder in result.ResultList) {
                var solutionId = (int)attributeHolder["solutionid"];
                var description = (string)attributeHolder["description"];
                treeDataList.Add(new FaqData(solutionId, description));
            }
            Log.DebugFormat("returned list size {0}", treeDataList.Count());
            return treeDataList;
        }

        public static IEnumerable<FaqData> GetMockedList() {
            return new List<FaqData>
            {
                     new FaqData(1,"E0008_Webmail | Change mailbox folder's language"),
                     new FaqData(2,"E0005_Outlook / Tips | Calendar tips"),
                     new FaqData(3,"E0004_Outlook / General | Deleted BB Mails not synchronized"),
                     new FaqData(4,"E0002_Outlook / General | Import personal address book"),
                     new FaqData(5,"E0003_Outlook / General | List of blocked attachments"),
                     new FaqData(6,"E0001_Outlook / General | Turn off auto correction"),
                     new FaqData(7,"G0001_Outlook / General | Ausschalten der AutoKorrektur"),
                     new FaqData(8,"E0009_SWD | Find Platform Version"),
                     new FaqData(9,"E0010_SWD | How long does the SWD last?"),
                     new FaqData(10,"E0011_Outlook / General | Send 'on behalf' from Group Mailbox"),
                     new FaqData(11,"E0012_Outlook / General | Mails with archived attachments"),
                     new FaqData(12,"                           "),
                     new FaqData(13,"E0014_Outlook / General | Lost Common Store Button"),
                     new FaqData(14,"G0016_Outlook / General | Einladung für Besprechung funktioniert nicht"),
                     new FaqData(15,"E0015_Outlook / General | New Contact/User/Vessel not in GAL"),
                     new FaqData(16,"E0017_Outlook / General | Attachment will not open"),
                     new FaqData(17,"E0019_Printer | Paper tray empty"),
                     new FaqData(18,"E0021_Printer | Doesn't pick up paper correctly"),
                     new FaqData(19,"E0020_Printer | Printed document is too light"),
                     new FaqData(20,"E0022_Printer | printer is not responding"),
                     new FaqData(21,"G0002_Outlook / General | Importieren einses persönlichen Adressbuchs"),
                     new FaqData(22,"G0003_Outlook / General | Liste der blockierten Typen von Anhängen"),
                     new FaqData(23,"G0004_Outlook / General | Gelöschte BB Mails werden nicht syncronisiert"),
                     new FaqData(24,"G0006_Windows 7 | A4 als Standard-Einstellung für Drucker"),
                     new FaqData(25,"G0007_Webmail | Wiederherstellen gelöschter Objekte"),
                     new FaqData(26,"G0008_Webmail | Ändern der mailbox' Ordner Sprache"),
                     new FaqData(27,"G0009_SWD | Finden der Plattformversion"),
                     new FaqData(28,"G0010_SWD | Dauer der SWD"),
                     new FaqData(29,"G0012_Outlook / General | Mails mit achrivierten Anhängen"),
                     new FaqData(30,"G0013_Outlook / General | Mails hängen in Ausgangsordner"),
                     new FaqData(31,"G0014_Outlook / General | Common Store Buttons fehlen"),
                     new FaqData(32,"G0015_Outlook / General | Neuer Kontakt/User/Schiff nicht in GAL"),
                     new FaqData(33,"G0017_Outlook / General | Anhang öffnet sich nicht"),
                     new FaqData(34,"G0018_Webmail | Login Probleme"),
                     new FaqData(35,"G0019_Printer | Papierfach alle"),
                     new FaqData(36,"G0020_Printer | Gedruckter Dokument - Farbe zu schwach"),
                     new FaqData(37,"G0005_Outlook / Tips | Kalender Tipps"),
                     new FaqData(38,"G0021_Printer | Drucker zieht Papier nicht richtig ein"),
                     new FaqData(39,"G0022_Printer | Drucker reagiert/antwortet nicht"),
                     new FaqData(40,"E0016_Outlook / General | invitation for meeting do not work"),
                     new FaqData(41,"S0003_Outlook / General | Lista de archivos adjuntos bloqueados"),
                     new FaqData(42,"S0002_Outlook / General | Importar libreta de direcciones personal"),
                     new FaqData(43,"S0009_SWD | Encontrar versión de plataforma"),
                     new FaqData(44,"S0010_SWD | ¿Cuánto se demora la SWD?"),
                     new FaqData(45,"S0019_Printer | Bandeja de papel vacía"),
                     new FaqData(46,"S0022_Printer | La impresora no responde"),
                     new FaqData(47,"G0011_Outlook / General | Senden als Vertreter einer Gruppenmailbox nicht möglich"),
                     new FaqData(48,"S0001_Outlook / General | Deshabilitar autocorrección"),
                     new FaqData(49,"E0023_Printer | Set A4 as default setting"),
                     new FaqData(50,"G0023_Printer | A4 als Standard-Einstellung für Drucker"),
                     new FaqData(51,"S0004_Outlook / General | Correos eliminados de Blackberry no sincronizada"),
                     new FaqData(52,"S0005_Outlook / Tips | Consejos para el calendario"),
                     new FaqData(53,"S0006_Windows 7 | Establecer A4 como el tamaño predeterminado"),
                     new FaqData(54,"S0007_Webmail | Recuperación de elementos eliminados"),
                     new FaqData(55,"S0008_Webmail | Cambio del idioma de la carpeta de correo"),
                     new FaqData(56,"S0011_Outlook / General | 'Enviar en nombre' de un buzón de grupo"),
                     new FaqData(57,"S0012_Outlook / General | Correos con archivos adjuntos archivados"),
                     new FaqData(58,"S0013_Outlook / General | Correos atascados en Outlook"),
                     new FaqData(59,"S0014_Outlook / General | Botón de CommonStore perdido"),
                     new FaqData(60,"S0015_Outlook / General | Nuevo contacto/usuario/buque no está en la  lista de direcciones global."),
                     new FaqData(61,"S0016_Outlook / General | Invitación para meetings no funciona"),
                     new FaqData(62,"S0017_Outlook / General | Archivo adjunto no se abre"),
                     new FaqData(63,"S0018_Webmail | Problemas de inicio de sesión"),
                     new FaqData(64,"S0020_Printer | El documento impreso es demasiado claro."),
                     new FaqData(65,"S0021_Printer | No coge el papel correctamente"),
                     new FaqData(66,"S0023_Printer | Establecer A4 como el tamaño predeterminado"),
                     new FaqData(67,"DB2 service failed"),
                     new FaqData(68,"                  "),
                     new FaqData(69,"Testticket"),
                     new FaqData(70,"          ")
            };
        }

        public static string GetLanguageToFilter(string lang) {
            var languages = new Dictionary<string, string> { { "en", "E" }, { "de", "G" }, { "es", "S" } };
            var language = "E";
            if (string.IsNullOrEmpty(lang)) {
                return language;
            }
            string selectedLanguage;
            languages.TryGetValue(lang.ToLower(), out selectedLanguage);
            if (selectedLanguage != null) {
                language = selectedLanguage;
            }

            return language;
        }

        #endregion
    }
}
