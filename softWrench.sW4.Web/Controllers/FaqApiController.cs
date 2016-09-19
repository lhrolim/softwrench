using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Metadata.Applications.DataSet.Faq;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Models.Faq;

namespace softWrench.sW4.Web.Controllers {
    [System.Web.Http.Authorize]
    [SPFRedirect(URL = "Faq", Title = "FAQs")]
    // kept for reference
    public class FaqApiController : ApiController {

        private DataController _dataController;

        public FaqApiController(DataController dataController) {
            _dataController = dataController;
        }

        [System.Web.Http.HttpGet]
        public async Task<GenericResponseResult<FaqModel>> Index(string lang = null, string search = null) {
            /*
            var lstFromMetadata = (ApplicationConfiguration.IsProd) ? GetList() : GetMockedList();
            if (lstFromMetadata.Any(treeData => treeData.SolutionId != Convert.ToInt32(treeData.Description.Substring(1, 4))))
            {
                throw new NotImplementedException();
            }
             */
            var listToUse = ApplicationConfiguration.IsProd() ? await new FaqUtils().GetList() : FaqUtils.GetMockedList();
            //var listToUse = ApplicationConfiguration.IsProd() ? new FaqUtils().GetList() : new FaqUtils().GetList();
            var tree = GetTree(listToUse, lang, search);
            var model = new FaqModel(tree) { Lang = lang, Search = search };
            return new GenericResponseResult<FaqModel>(model, null);
        }


        public FileResult Download(string id) {
            string fileName = null;
            var fileBytes = GetFile(id, ref fileName);
            var result = new FileContentResult(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet) {
                FileDownloadName = fileName
            };
            return result;
        }

        private static object GetTree(IEnumerable<FaqUtils.FaqData> treeDataList, string lang, string search) {
            object returnTree;

            var language = FaqUtils.GetLanguageToFilter(lang);

            var classificationNodes = BuildClassificationNodes(treeDataList, language);

            if (!string.IsNullOrEmpty(search)) {
                returnTree = classificationNodes.FindAll(x => x.SolutionId != null && x.Name.IndexOf(search, 0, StringComparison.CurrentCultureIgnoreCase) != -1);
            } else {
                returnTree = BuildLeafNodes(classificationNodes);
            }
            return returnTree;
        }

        private static List<FaqModel> BuildClassificationNodes(IEnumerable<FaqUtils.FaqData> treeDataList, string language) {
            var classificationList = new List<FaqModel>();

            foreach (var item in treeDataList) {
                FaqDescription descriptor;
                try {
                    descriptor = new FaqDescription(item.Description);
                } catch {
                    continue;
                }
                if (!descriptor.IsValid() || descriptor.Language != language) {
                    continue;
                }

                var levelCount = 0;
                int? rootId = null;
                foreach (var level in descriptor.Categories) {
                    var findByLevel = classificationList.Find(x => x.Name == level);
                    if (findByLevel != null) {
                        rootId = findByLevel.Id;
                        levelCount++;
                        continue;
                    }
                    var lastOrDefault = classificationList.LastOrDefault();
                    if (levelCount == 0 || lastOrDefault != null) {
                        var id = lastOrDefault != null ? lastOrDefault.Id + 1 : 1;
                        classificationList.Add(new FaqModel(id, level, descriptor.Language, descriptor.Id, rootId));
                        rootId = id;
                    }
                    levelCount++;
                }
                var lastOrDefaultAux = classificationList.LastOrDefault();
                if (lastOrDefaultAux == null) continue;
                classificationList.Add(new FaqModel(lastOrDefaultAux.Id + 1, descriptor.RealDescription, descriptor.Language, descriptor.Id, rootId, item.SolutionId));
            }

            return classificationList;
        }

        private static object BuildLeafNodes(List<FaqModel> classificationList) {
            var tree = classificationList.Where(x => x.RootId == null);

            foreach (var classification in classificationList) {
                classification.Children = new List<FaqModel>();
                var childreList = classificationList.FindAll(x => x.RootId == classification.Id);
                classification.Children = childreList;
            }

            return tree;
        }



        private byte[] GetFile(string s, ref string fileName) {
            var fs = File.OpenRead(s);
            fileName = fs.Name;
            var data = new byte[fs.Length];
            var br = fs.Read(data, 0, data.Length);
            if (br != fs.Length)
                throw new IOException(s);
            return data;
        }



        private static string GetIdInFolder(string id) {
            string idInFolder;

            switch (id.Length) {
                case 1:
                    idInFolder = "000" + id;
                    break;
                case 2:
                    idInFolder = "00" + id;
                    break;
                case 3:
                    idInFolder = "0" + id;
                    break;
                default:
                    idInFolder = id;
                    break;
            }

            return idInFolder;
        }
    }
}
