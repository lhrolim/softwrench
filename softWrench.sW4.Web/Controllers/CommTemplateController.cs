using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using cts.commons.portable.Util;
using JetBrains.Annotations;
using log4net.Util;
using Newtonsoft.Json.Linq;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Email;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Controllers {
    public class CommTemplateController : ApiController {

        private readonly MaximoHibernateDAO _maximoDao;
        private readonly CommLogTemplateMerger _templateMerger;



        public CommTemplateController(MaximoHibernateDAO dao, CommLogTemplateMerger templateMerger) {
            _maximoDao = dao;
            _templateMerger = templateMerger;
        }

        [HttpPost]
        public IGenericResponseResult MergeTemplateDefinition([FromBody]TemplateRequestDTO dto) {

            var app = MetadataProvider.Application(dto.ApplicationName).ApplyPoliciesWeb(new ApplicationMetadataSchemaKey(dto.SchemaId));

            var entityMetadata = MetadataProvider.SlicedEntityMetadata(app);
            var crudData = EntityBuilder.BuildFromJson<Entity>(typeof(Entity), entityMetadata, app, dto.Json,
                dto.ApplicationItemId);


            var rows = _maximoDao.FindByNativeQuery(string.Format("SELECT * FROM COMMTEMPLATE WHERE templateid = '{0}'", dto.TemplateId));

            if (!rows.Any()) {
                throw new CommLogTemplateMerger.CommTemplateException("template with id {0} cannot be found".Fmt(dto.TemplateId));
            }
            var template = rows.First();
            var rawMessage = template["message"];
            var rawSubject = template["subject"];

            var templateVariables = _templateMerger.LocateVariables(rawMessage);
            templateVariables.AddAll(_templateMerger.LocateVariables(rawSubject));

            if (!templateVariables.Any()) {
                return new GenericResponseResult<TemplateResponseDTO>(
                new TemplateResponseDTO {
                    Message = rawMessage,
                    Subject = rawSubject
                });
            }

            var mergedVariables = _templateMerger.ApplyVariableResolution(dto.TemplateId,templateVariables, crudData);

            var message = _templateMerger.MergeTemplateDefinition(rawMessage, mergedVariables);
            var subject = _templateMerger.MergeTemplateDefinition(rawSubject, mergedVariables);

            return new GenericResponseResult<TemplateResponseDTO>(
              new TemplateResponseDTO {
                  Message = message,
                  Subject = subject
              });

        }

    }

    internal class TemplateResponseDTO {
        public string Message {
            get; set;
        }
        public string Subject {
            get; set;
        }
    }

    public class TemplateRequestDTO {

        public JObject Json {
            get; set;
        }

        public string ApplicationName {
            get; set;
        }
        public string ApplicationItemId {
            get; set;
        }

        public string SchemaId {
            get; set;
        }

        public string TemplateId {
            get; set;
        }
    }

   
}
