using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NHibernate.Hql.Ast.ANTLR;
using NHibernate.Linq.Functions;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.Schema;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Web.Controllers;

namespace softWrench.sW4.Web.Models.Faq {
    public class FaqModel {
        private readonly string _faqModelResponseJson;

        public int Id { get; set; }
        public string Name { get; set; }
        public int? RootId { get; set; }
        public int? SolutionId { get; set; }
        public string Lang { get; set; }
        public List<FaqModel> Children { get; set; }
        public string Search { get; set; }
        public string FaqId { get; set; }

        public FaqModel(int id, string name, string lang, string faqId, int? rootId = null, int? solutionId = null) {
            Id = id;
            Name = name;
            Lang = lang;
            RootId = rootId;
            SolutionId = solutionId;
            FaqId = lang + faqId;
        }

        public FaqModel() {

        }

        public FaqModel(object tree) {
            _faqModelResponseJson = JsonConvert.SerializeObject(tree, Newtonsoft.Json.Formatting.None,
            new JsonSerializerSettings() {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            _faqModelResponseJson = _faqModelResponseJson.Replace("name", "Name");

        }

        public override string ToString() {
            return string.Format("Id: {0}, Name: {1}, Lang: {2}, RootId: {3} , SolutionId: {4}, FaqId: {5}",
                Id, Name, Lang, RootId, SolutionId, FaqId);
        }

        public string FaqModelResponseJson {
            get { return _faqModelResponseJson; }
        }
    }
}