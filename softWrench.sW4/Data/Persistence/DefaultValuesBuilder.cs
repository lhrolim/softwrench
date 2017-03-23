using cts.commons.portable.Util;
using JetBrains.Annotations;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Ajax.Utilities;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence {
    public class DefaultValuesBuilder {

        private const string UserPrefix = "@user.";
        private const string PastFunctionPrefix = "@past(";
        private const string FutureFunctionPrefix = "@future(";
        private const string FunctionSuffix = ")";

        public const string DBDateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        [NotNull]
        public static DataMap BuildDefaultValuesDataMap(ApplicationMetadata application, Entity initialValues, [CanBeNull]Type mappingType) {

            var displayables = application.Schema.GetDisplayable<IDefaultValueApplicationDisplayable>(typeof(IDefaultValueApplicationDisplayable));
            var fields = application.Schema.Fields;
            var dictionary = BuildDictionary(displayables);

            var toReplace = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var o in dictionary) {
                var defaultValue = o.Value;
                if (defaultValue == null) {
                    continue;
                }
                toReplace.Add(o.Key, GetDefaultValue(defaultValue.ToString()));
            }
            foreach (var entry in toReplace) {
                dictionary[entry.Key] = entry.Value;
            }

            var user = SecurityFacade.CurrentUser();
            ChangeFromDBIfNeeded(dictionary, fields, user, "orgid", user.OrgId);
            ChangeFromDBIfNeeded(dictionary, fields, user, "siteid", user.SiteId);
            ChangeFromDBIfNeeded(dictionary, fields, user, "storeloc", user.Storeloc);
            ChangeFromDBIfNeeded(dictionary, fields, user, "fromstoreloc", user.Storeloc);

            var schemaDefaultValues = new DataMap(application.Name, dictionary, mappingType);
            if (initialValues != null) {
                MergeWithPrefilledValues(schemaDefaultValues, initialValues);
            }
            return schemaDefaultValues;
        }

        private static void ChangeFromDBIfNeeded(IDictionary<string, object> dictionary,
            IEnumerable<ApplicationFieldDefinition> fields, InMemoryUser user, string attributeName, object newValue) {
            if (fields.Any(f => f.Attribute == attributeName) && dictionary[attributeName] == null) {
                dictionary[attributeName] = newValue;
            }
        }

        //TODO: Modify to have a flag that will determine whether the existingDataMap attribute should be overwritten if attribute already exist in initialValues
        public static DataMap AddMissingInitialValues(DataMap existingDataMap, DataMap initialValues) {
            foreach (var attribute in initialValues) {
                var key = attribute.Key;
                if (!existingDataMap.ContainsAttribute(key)) {
                    existingDataMap.Add(key, attribute.Value);
                }
            }
            return existingDataMap;
        }

        private static DataMap MergeWithPrefilledValues(DataMap schemaDefaultValues, Entity initialValues) {
            foreach (var attribute in initialValues) {
                schemaDefaultValues.SetAttribute(attribute.Key, attribute.Value);
            }
            foreach (var attribute in initialValues.UnmappedAttributes) {
                schemaDefaultValues.SetAttribute(attribute.Key, attribute.Value);
            }
            return schemaDefaultValues;
        }

        private static Dictionary<string, object> BuildDictionary(IEnumerable<IApplicationDisplayable> displayables) {
            var dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            var defaultdisplayables = displayables.OfType<IDefaultValueApplicationDisplayable>();

            foreach (var defaultDisplayable in defaultdisplayables) {
                var key = defaultDisplayable.Attribute;
                if (dictionary.ContainsKey(key)) {
                    if (defaultDisplayable is ApplicationFieldDefinition) {
                        if (!((ApplicationFieldDefinition)defaultDisplayable).IsHidden) {
                            dictionary[key] = defaultDisplayable.DefaultValue;
                        }
                    }
                } else {
                    var defaultValue = defaultDisplayable.DefaultValue;
                    if (defaultValue == "@now" && !"date".EqualsIc(defaultDisplayable.RendererType)) {
                        defaultValue = "@nowext";
                    }
                    dictionary.Add(key, defaultValue);
                }
            }
            return dictionary;
        }

        public static String ConvertAllValues(String input, InMemoryUser user) {
            // TODO: Update to remove need for properties['xxx'] to make consistant with front end
            string valueParsed = null;

            input = ReadFromUserGenericProperties(input, user);

            foreach (var pastFunction in StringUtil.GetSubStrings(input, PastFunctionPrefix, FunctionSuffix)) {
                valueParsed = ParsePastAndFutureFunction(pastFunction, -1, DBDateTimeFormat);
                input = input.Replace(pastFunction, "'" + valueParsed + "'");
            }

            foreach (var futureFunction in StringUtil.GetSubStrings(input, FutureFunctionPrefix, FunctionSuffix)) {
                valueParsed = ParsePastAndFutureFunction(futureFunction, 1, DBDateTimeFormat);
                input = input.Replace(futureFunction, "'" + valueParsed + "'");
            }

            foreach (var keyword in StringUtil.GetSubStrings(input, "@")) {
                valueParsed = GetDefaultValue(keyword, user);

                // replaces the keyword only if its found
                if (keyword.Equals(valueParsed)) {
                    continue;
                }

                input = input.Replace("'" + keyword + "'", "'" + valueParsed + "'").Replace(keyword, "'" + valueParsed + "'");
            }

            return input;
        }

        public static String GetDefaultValue(string key, InMemoryUser user = null, string dateTimeFormat = null) {

            var value = key;
            if (user == null) {
                user = SecurityFacade.CurrentUser();
            }

            key = key.ToLower();

            if (key.Equals("@orgid")) {
                return user.OrgId ?? ApplicationConfiguration.DefaultOrgId;
            }

            if (key.Equals("@siteid")) {
                return user.SiteId ?? ApplicationConfiguration.DefaultSiteId;
            }

            if (key.Equals("@username")) {
                value = user.Login;
            } else if (key.Equals("@userid")) {
                value = user.DBId.ToString();
            } else if (key.Equals("@personid")) {
                value = user.MaximoPersonId ?? user.Login;
            } else if (key.Equals("@usersite")) {
                value = user.SiteId;
            } else if (key.Equals("@now")) {
                value = DateTime.Now.ToShortDateString();
            } else if (key.Equals("@nowext")) {
                value = DateTime.Now.FromServerToRightKind().ToString();
            } else if (key.StartsWith(UserPrefix)) {
                value = ParseUserProperty(key, user);
            } else if (key.StartsWith(PastFunctionPrefix)) {
                value = ParsePastAndFutureFunction(key, -1, dateTimeFormat);
            } else if (key.StartsWith(FutureFunctionPrefix)) {
                value = ParsePastAndFutureFunction(key, 1, dateTimeFormat);
            } else if (key.Equals("@storeloc")) {
                value = user.Storeloc;
            }

            return value;
        }

        private static string ParseUserProperty(string o, InMemoryUser user) {
            var innerReference = o.Substring(UserPrefix.Length);
            if (innerReference.Equals("phone")) {
                return user.Phone;
            }
            if (innerReference.Equals("siteid")) {
                return user.SiteId;
            }
            if (innerReference.Equals("orgid")) {
                return user.OrgId;
            }
            if (innerReference.Equals("email")) {
                return user.Email;
            }
            if (user.Genericproperties.ContainsKey(innerReference) && user.Genericproperties[innerReference] != null) {
                return user.Genericproperties[innerReference].ToString();
            }
            return null;
        }

        /// <summary>
        /// @user.properties[xxx]
        /// </summary>
        /// <param name="text"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private static string ReadFromUserGenericProperties(string text, InMemoryUser user) {
            try {
                var innerReference = Regex.Split(text, UserPrefix);
                foreach (var s in innerReference) {
                    if (s.StartsWith("properties")) {
                        var start = s.IndexOf('[') + 2;
                        var lenght = (s.IndexOf(']') - 1) - start;
                        var key = s.Substring(start, lenght);
                        if (user.Genericproperties.ContainsKey(key)) {
                            object value;
                            if (user.Genericproperties.TryGetValue(key, out value)) {
                                var property = UserPrefix + "properties['" + key + "']";
                                text = text.Replace(property, value.ToString());
                            }
                        }
                    }
                }
                return text;
            } catch {
                return text;
            }
        }

        private static string ParsePastAndFutureFunction(string function, int pastOrFuture, string format = null) {

            string valueToParse = Regex.Match(function, @"(?<=\()(.*?)(?=\))").Value;
            var value = DateUtil.ParsePastAndFuture(valueToParse, pastOrFuture);

            return GetDateTimeAsString(value, format);
        }

        public static string GetDateTimeAsString(DateTime date, String format = null) {
            if (String.IsNullOrEmpty(format)) {
                format = "MM/dd/yyyy HH:mm";
            }
            return date.ToString(format);
        }

        private static string GetDateAsString(DateTime date, string format = null) {
            if (String.IsNullOrEmpty(format)) {
                format = "MM/dd/yyyy";
            }
            return date.ToString(format);
        }
    }
}
