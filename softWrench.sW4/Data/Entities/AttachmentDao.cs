using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using cts.commons.persistence;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softwrench.sW4.Shared2.Data;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using softWrench.sW4.Data.Persistence;

namespace softWrench.sW4.Data.Entities {
    public class AttachmentDao : ISingletonComponent {

        private const string EntityName = "DOCINFO";
        private const string ATTACHMENTS_BY_OWNER = @"select L.document,L.ownertable,L.ownerid,L.doctype,L.createdate,L.createby,I.urlname,I.description,I.docinfoid,I.urlname
                                                        from docinfo I
	                                                    inner join doclinks L
		                                                    on I.docinfoid = L.docinfoid
	                                                    where L.ownertable=:OWNERTABLE and L.ownerid=:OWNERID";
        private const string ATTACHMENTS_BY_OWNER_ORDERED = ATTACHMENTS_BY_OWNER + " order by L.createdate desc";

        private EntityRepository _repository;
        private IMaximoHibernateDAO _maxDAO;

        private EntityRepository EntityRepository {
            get {
                return _repository ?? (_repository = SimpleInjectorGenericFactory.Instance.GetObject<EntityRepository>(typeof (EntityRepository)));
            }
        }

        private IMaximoHibernateDAO MaxDAO {
            get {
                return _maxDAO ?? (_maxDAO = SimpleInjectorGenericFactory.Instance.GetObject<IMaximoHibernateDAO>(typeof(IMaximoHibernateDAO)));
            }
        }

        public AttachmentDao(EntityRepository repository, IMaximoHibernateDAO maxDAO) {
            _repository = repository;
            _maxDAO = maxDAO;
        }

        public AttributeHolder ById(string documentId) {
            var entityMetadata = MetadataProvider.Entity(EntityName);
            var searchRequestDto = SearchRequestDto.GetFromDictionary(new Dictionary<string, string>() { { "docinfoid", documentId } });
            searchRequestDto.AppendProjectionField(new ProjectionField("urlname", "urlname"));
            searchRequestDto.AppendProjectionField(new ProjectionField("document", "document"));
            searchRequestDto.AppendProjectionField(new ProjectionField("docinfoid", "docinfoid"));
            var list = EntityRepository.Get(entityMetadata, searchRequestDto);
            var result = list.FirstOrDefault();
            return result;
        }

        /// <summary>
        /// Lists the owned attachments ordered by createdate descending.
        /// </summary>
        /// <param name="owner">owner enitity's name</param>
        /// <param name="ownerId">owner entity's uid</param>
        /// <returns>each entry in the list will have the properties document,ownertable,ownerid,doctype,createdate,createby,urlname,description</returns>
        public IList<dynamic> ByOwner([NotNull]string owner, [NotNull]object ownerId) {
            return FetchByOwner(owner, ownerId);
        }

        /// <summary>
        /// Finds the owned attachment.
        /// </summary>
        /// <param name="owner">owner enitity's name</param>
        /// <param name="ownerId">owner entity's uid</param>
        /// <returns>the result will have the properties document,ownertable,ownerid,doctype,createdate,createby,urlname,description</returns>
        public dynamic SingleByOwner([NotNull]string owner, [NotNull]object ownerId) {
            return FetchByOwner(owner, ownerId, new PaginationData(1, 1, "L.createdate desc")).FirstOrDefault();
        }

        private IList<dynamic> FetchByOwner([NotNull]string owner, [NotNull]object ownerId, IPaginationData pagination = null) {
            var query = pagination == null ? ATTACHMENTS_BY_OWNER_ORDERED : ATTACHMENTS_BY_OWNER;
            
            var parameters = new ExpandoObject();
            var parameterCollection = (ICollection<KeyValuePair<string, object>>)parameters;
            parameterCollection.Add(new KeyValuePair<string, object>("OWNERID", ownerId));
            parameterCollection.Add(new KeyValuePair<string, object>("OWNERTABLE", owner));

            return MaxDAO.FindByNativeQuery(query, parameters, pagination);
        }

    }
}
