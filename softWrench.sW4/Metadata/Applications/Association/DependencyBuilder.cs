using System;
using System.Collections.Generic;
using System.Text;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;

namespace softWrench.sW4.Metadata.Applications.Association {
    internal class DependencyBuilder {

        public static IDictionary<string, ISet<string>> BuildDependantFields(IEnumerable<IApplicationDisplayable> allfields, IEnumerable<IDependableField> dependable) {
            IDictionary<string, ISet<string>> dictionary = new Dictionary<string, ISet<string>>();

            foreach (var association in dependable) {
                var dependantFields = association.DependantFields;
                if (dependantFields.Count > 0) {
                    foreach (var dependantField in dependantFields) {
                        ISet<string> dependentSet;
                        if (!dictionary.TryGetValue(dependantField, out dependentSet)) {
                            dependentSet = new HashSet<string>();
                            dictionary.Add(dependantField, dependentSet);
                        }
                        dependentSet.Add(association.AssociationKey);
                    }
                }
            }
            return dictionary;
        }

        public static HashSet<String> TryParsingDependentFields(String whereClause) {
            if (whereClause == null){
                return new HashSet<string>();
            }
            var depdendencies = new HashSet<string>();
            var parsingExpression = false;
            var sb = new StringBuilder();
            foreach (var charV in whereClause) {
                if (charV == '#') {
                    parsingExpression = true;
                    sb = new StringBuilder();
                } else if (charV != '#' && !parsingExpression) {
                    continue;
                } else if (charV == ' ') {
                    parsingExpression = false;
                    depdendencies.Add(sb.ToString());
                } else {
                    sb.Append(charV);
                }
            }
            if (parsingExpression) {
                depdendencies.Add(sb.ToString());
            }
            return depdendencies;
        }

    }
}
