using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using softWrench.sW4.Data.Persistence.Engine;
using cts.commons.simpleinjector;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Entities.Attachment;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {

    public class SWDBApplicationDataset : BaseApplicationDataSet {
        protected override IConnectorEngine Engine() {
            return SimpleInjectorGenericFactory.Instance.GetObject<SWDBConnectorEngine>(typeof(SWDBConnectorEngine));
        }

        protected T GetOrCreate<T>(OperationWrapper operationWrapper, bool populate) where T : new() {
            if (!OperationConstants.CRUD_UPDATE.Equals(operationWrapper.OperationName)) {
                return populate ? EntityBuilder.PopulateTypedEntity((CrudOperationData)operationWrapper.OperationData(), new T()) : new T();
            }
            var id = int.Parse(operationWrapper.Id);
            var item = SWDAO.FindByPK<T>(typeof(T), id);
            return populate ? EntityBuilder.PopulateTypedEntity((CrudOperationData)operationWrapper.OperationData(), item) : item;
        }


        protected IEnumerable<DocLink> HandleFileExplorerDocLinks<T>(T ticket, CrudOperationData crudoperationData, string relationshipName) where T : IBaseEntity {
            var result = new List<DocLink>();
            var user = SecurityFacade.CurrentUser();
            if (crudoperationData.AssociationAttributes.ContainsKey(relationshipName)) {
                var attachList = (IList<CrudOperationData>)crudoperationData.AssociationAttributes[relationshipName];
                var newFiles = attachList.Where(a => a.ContainsAttribute("#newFile") && a.GetBooleanAttribute("#newFile") == true);
                foreach (var newFile in newFiles) {
                    var dl = new DocLink {
                        CreateBy = user.DBId.Value,
                        CreateDate = DateTime.Now,
                        Document = newFile.GetStringAttribute("label"),
                        Extension = newFile.GetStringAttribute("extension"),
                        OwnerTable = ApplicationName(),
                        OwnerId = ticket.Id.Value
                    };
                    var di = DocInfo.FromLink(dl);
                    var baseBytes = FileUtils.ToByteArrayFromHtmlString(newFile.GetStringAttribute("value"));
                    di.Data = CompressionUtil.Compress(baseBytes);
                    di.CheckSum = FileUtils.GenerateCheckSumHash(di.Data);
                    dl.DocInfo = di;
                    result.Add(dl);
                }
            }
            return result;

        }


        protected ISWDBHibernateDAO SWDAO => SimpleInjectorGenericFactory.Instance.GetObject<ISWDBHibernateDAO>(typeof(ISWDBHibernateDAO));
    }
}
