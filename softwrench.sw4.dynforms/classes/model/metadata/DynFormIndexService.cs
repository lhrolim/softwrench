using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using Newtonsoft.Json.Linq;
using softwrench.sw4.dynforms.classes.model.entity;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Data.Pagination;

namespace softwrench.sw4.dynforms.classes.model.metadata {

    public class DynFormIndexService : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {


        [Import]
        public DynFormSchemaHandler SchemaHandler { get; set; }

        [Import] public ISWDBHibernateDAO DAO { get; set; }

        private IDictionary<string, int> _userIdCache = new ConcurrentDictionary<string, int>();

        public string AdjustConsideringIndexes(string formName, PaginatedSearchRequestDto searchDto, ApplicationSchemaDefinition schema) {
            var sb = new StringBuilder(" formdatamapid in (select datamap_id from FORM_DATAMAP_IDX where form_name = '{0}'".Fmt(formName));
            sb.AppendFormat(" and ");
            if (searchDto.QuickSearchDTO != null) {
                sb.AppendFormat(" value_ like '%{0}%'", searchDto.QuickSearchDTO.QuickSearchData);
            } else {
                foreach (var parameter in searchDto.ValuesDictionary) {
                    //                    sb.AppendFormat()
                }
            }

            sb.AppendFormat(")");
            //clean up
            searchDto.QuickSearchDTO = null;
            searchDto.SearchParams = null;
            searchDto.SearchValues = null;
            return sb.ToString();
        }

        [Transactional(DBType.Swdb)]
        public virtual async Task AdjustIndexesForSave(JObject ob, FormMetadata metadata, FormDatamap datamap, bool creation) {
            var schema = await SchemaHandler.LookupSchema(metadata, true);
            if (!creation) {
                //cleaning up old indexes
                await DAO.ExecuteSqlAsync("delete from FORM_DATAMAP_IDX where form_name = ? and datamap_id =? ", metadata.Name, datamap.FormDatamapId);
            }

            if (schema == null) {
                return;
            }

            IList<FormDatamapIndex> indexes = new List<FormDatamapIndex>();

            foreach (var displayable in schema.GetDisplayable<IApplicationAttributeDisplayable>()) {
                var value = ob.GetValue(displayable.Attribute);
                if (value != null) {
                    var stValue = value.ToString();
                    int numericIndex;
                    bool isNumeric = int.TryParse(stValue, out numericIndex);
                    var idx = new FormDatamapIndex {
                        AttributeName = displayable.Attribute,
                        FormDataMap = datamap,
                        Metadata = metadata,
                        Value = value.ToString()
                    };
                    if (isNumeric) {
                        idx.NumValue = numericIndex;
                    }

                    indexes.Add(idx);
                }


            }

            await DAO.BulkSaveAsync(indexes);

        }

        public int GenerateUserId(string formName) {

            lock (string.Intern(formName + "lockmechanism")) {
                if (!_userIdCache.ContainsKey(formName)) {
                    _userIdCache.Add(formName, 1);
                    return 1;
                }
                _userIdCache[formName]++;
                return _userIdCache[formName];
            }


        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            var results = DAO.FindByNativeQuery("select max(userid) as maxid,form_name as fname from FORM_DATAMAP group by form_name");
            foreach (var row in results) {
                var formName = row["fname"];
                var s = row["maxid"];
                int userid;
                if (s.StartsWith(formName)) {
                    userid = int.Parse(s.Substring(formName.Length));
                } else {
                    userid = int.Parse(s);
                }
                //no need to lock upon initialization
                _userIdCache[formName] = userid;
            }
        }
    }
}
