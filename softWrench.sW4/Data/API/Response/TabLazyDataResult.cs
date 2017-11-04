using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;

namespace softWrench.sW4.Data.API.Response {

    public class TabLazyDataResult : GenericResponseResult<DataMap>{

        public string TabId { get; set; }


        public IDictionary<string, EntityRepository.SearchEntityResult> CompositionResult { get; set; } = new Dictionary<string, EntityRepository.SearchEntityResult>();
    }
}
