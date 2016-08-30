using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Mapping {
    public class InternalMappingStructure {

        internal readonly IDictionary<string,ISet<string>> SimpleAssociations = new Dictionary<string, ISet<string>>();

        internal readonly ISet<string> DefaultValues = new HashSet<string>();

        internal readonly IDictionary<int, AndValuesHolder> AndValues = new Dictionary<int, AndValuesHolder>();


        internal class AndValuesHolder {

            internal ISet<string> Keys = new HashSet<string>();

            internal ISet<string> Values = new HashSet<string>();


        }



    }
}
