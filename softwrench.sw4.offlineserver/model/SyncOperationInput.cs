using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.offlineserver.model {


    [Class(Table = "OFF_SYNCOPERATION_INPUT", Lazy = false)]
    public class SyncOperationInput : IComparable<SyncOperationInput> {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id { get; set; }

        [Property(Column = "_key")]
        public string Key { get; set; }

        [Property]
        public string Value { get; set; }

        public int CompareTo(SyncOperationInput other) {
            return String.Compare(Key, other.Key, StringComparison.Ordinal);
        }
    }
}
