using System.IO;
using softwrench.sw4.Hapag.Data.Scheduler.Jobs.Helper;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Scheduler;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.Hapag.Data.Scheduler.Jobs {
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// https://controltechnologysolutions.atlassian.net/browse/HAP-1124
    /// </summary>
    public class RI102ExtractorJob : ASwJob {

        private const string FileName = "PrintList.csv";
        private const string FileBaseDirectoryPath = "E:\\Client List\\";


        private readonly EntityRepository _entityRepository;

        private readonly ExcelUtil _excelUtil;

        public RI102ExtractorJob(EntityRepository entityRepository, ExcelUtil excelUtil) {
            _entityRepository = entityRepository;
            _excelUtil = excelUtil;
        }


        public override string Name() {
            return "RI102 Client Extractor";
        }

        public override string Description() {
            return "Extract all Active Prints from Hapag as described on the requirement";
        }

        public override string Cron() {
            return "0 0 2 * * ?";
        }

        private string NewLocationDelegate(AttributeHolder holder, ApplicationFieldDefinition field, string originalData) {
            if (!field.Attribute.EqualsIc("#rI102newlocation")) {
                return originalData;
            }
            var country = holder.GetAttribute("location_pluspservaddr_.country");
            var loccode = holder.GetAttribute("hlagpluspcustomer");
            var streetaddress = holder.GetAttribute("location_pluspservaddr_.streetaddress");
            var floor = holder.GetAttribute("location_.floor") as string;
            var room = holder.GetAttribute("location_.room") as string;
            if (string.IsNullOrEmpty(floor) || string.IsNullOrEmpty(room)) {
                return country + "/" + loccode + "/" + streetaddress;
            }
            return country + "/" + loccode + "/" + streetaddress + "/" +floor + "/" + room;
        }

        public override void ExecuteJob() {

            var user = SecurityFacade.CurrentUser();
            var app = MetadataProvider.Application("asset").ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("RI102Export"));

            var sliced = MetadataProvider.SlicedEntityMetadata(app);
            var dto = R102ExtractorHelper.BuildDTO();
            var rows = _entityRepository.Get(sliced, dto);

            var csvBytes = _excelUtil.ConvertGridToCsv(user, app.Schema, rows, NewLocationDelegate);
            var outputPath = FileBaseDirectoryPath + FileName;
            if (ApplicationConfiguration.IsLocal()) {
                outputPath = "c:\\softwrench\\hapag\\PrintList.csv";
            }
            File.WriteAllBytes(outputPath, csvBytes);

        }



        public override bool IsScheduled {
            get; set;
        }
        public override bool RunAtStartup() {
            return ApplicationConfiguration.IsLocal();
        }
    }
}
