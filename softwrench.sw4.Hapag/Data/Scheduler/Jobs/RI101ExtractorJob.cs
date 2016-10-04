using System.IO;
using softwrench.sw4.Hapag.Data.Scheduler.Jobs.Helper;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Scheduler;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.Hapag.Data.Scheduler.Jobs {
    // ReSharper disable once InconsistentNaming
    public class RI101ExtractorJob : ASwJob {

        private const string FileName = "ClientList.csv";
        private const string FileBaseDirectoryPath = "E:\\Client List\\";


        private readonly EntityRepository _entityRepository;

        private readonly ExcelUtil _excelUtil;

        public RI101ExtractorJob(EntityRepository entityRepository, ExcelUtil excelUtil) {
            _entityRepository = entityRepository;
            _excelUtil = excelUtil;
        }


        public override string Name() {
            return "RI101 Client Extractor";
        }

        public override string Description() {
            return "Extract all Active Clients from Hapag as described on ";
        }

        public override string Cron() {
            return "0 45 * * * ?";
        }

        public override void ExecuteJob() {

            var user = SecurityFacade.CurrentUser();
            var app = MetadataProvider.Application("asset").ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("RI101Export"));

            var sliced = MetadataProvider.SlicedEntityMetadata(app);
            var dto = R101ExtractorHelper.BuildDTO();
            var rows = _entityRepository.Get(sliced, dto);

            var csvBytes = _excelUtil.ConvertGridToCsv(user, app.Schema, rows);
            var outputPath = FileBaseDirectoryPath + FileName;
            if (ApplicationConfiguration.IsLocal()) {
                outputPath = "c:\\softwrench\\hapag\\ClientList.csv";
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
