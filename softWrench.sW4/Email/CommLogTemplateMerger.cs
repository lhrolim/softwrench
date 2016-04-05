using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;

namespace softWrench.sW4.Email {
    public class CommLogTemplateMerger : ISingletonComponent {

        private readonly ISet<string> _phase1FixedVariables = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {"ticketid","classificationid","description","changedate","ownerperson.displayname","DESCRIPTION_LONGDESCRIPTION","LONGDESCRIPTION.ldtext","reportedemail", "affectedperson", "affectedemail","reportdate" };

        private readonly EntityRepository _entityRepository;

        private static string RegexPattern = @":(\w|\.)+";
        private static readonly Regex MaximoVariableRegex = new Regex(RegexPattern);



        public CommLogTemplateMerger(EntityRepository entityRepository) {
            _entityRepository = entityRepository;
        }

        public string MergeTemplateDefinition([NotNull]string rawDatabaseData, IDictionary<string, string> mergedVariables) {
            rawDatabaseData = Regex.Replace(rawDatabaseData, RegexPattern, match => RegexEvaluator(match, mergedVariables));
            return rawDatabaseData;
        }

        public string RegexEvaluator(Match match, IDictionary<string, string> mergedVariables) {
            var variableName = match.Value;
            var normalizedVar = variableName.Trim().Substring(1).ToLower();
            if (_phase1FixedVariables.Contains(normalizedVar)) {
                return mergedVariables[normalizedVar];
            }
            //keep same value if not present on phase1 list
            return variableName;
        }

        public IDictionary<string, string> ApplyVariableResolution(string templateId, IEnumerable<string> templateVariables, Entity data) {
            var result = new Dictionary<string, string>();
            foreach (var variable in templateVariables) {
                var normalizedvariable = NormalizeVariables(variable);
                if (!data.ContainsAttribute(normalizedvariable)) {
                    throw new CommTemplateException(
                        "Template {0} is not yet supported. Variable {1} cannot be resolved. Please contact your administrator".Fmt(templateId, variable));
                }
                var value = data.GetAttribute(normalizedvariable);
                string convertedValue;
                if (value == null) {
                    convertedValue = "";
                } else if (value is DateTime) {
                    convertedValue = ((DateTime)value).ToString("MM/dd/yyyy hh:mm");
                } else {
                    convertedValue = Convert.ToString(value);
                }
                result[variable] = convertedValue;
            }
            return result;
        }

        private static string NormalizeVariables(string variable) {
            //mapping between variable names in Maximo and in softwrench for phase1
            var normalizedvariable = variable;
            if (normalizedvariable.Equals("description_longdescription")) {
                normalizedvariable = "longdescription_.ldtext";
            }
            if (normalizedvariable.Equals("ownerperson.displayname")) {
                normalizedvariable = "ownerperson_.displayname";
            }

            return normalizedvariable;
        }

        [NotNull]
        public HashSet<string> LocateVariables(string databaseData) {
            var variables = new HashSet<string>();

            var collectionMatches = MaximoVariableRegex.Matches(databaseData);
            foreach (Match match in collectionMatches) {
                var variable = match.Value.Trim().Substring(1).ToLower();
                if (_phase1FixedVariables.Contains(variable)) {
                    variables.Add(variable);
                }
            }
            return variables;
        }

        public class CommTemplateException : Exception {

            public CommTemplateException(string message) : base(message) {

            }
        };

    }
}
