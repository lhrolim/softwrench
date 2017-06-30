using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using JetBrains.Annotations;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Util;
using softWrench.sW4.Metadata.Applications.Validator;
using softWrench.sW4.Metadata.Parsing;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Applications.Reference {
    class ReferenceHandler {
        private const string PropReference = "@prop:";

        public static ApplicationSchemaDefinition.LazyComponentDisplayableResolver ComponentDisplayableResolver =
            (reference, schema, components) => DoResolveReferences(schema, reference, components);

        public static IEnumerable<IApplicationDisplayable> DoResolveReferences(ApplicationSchemaDefinition schema, ReferenceDisplayable reference, IEnumerable<DisplayableComponent> components) {
            var componentsSource = components;
            var applicationName = schema.ApplicationName;
            if (components == null) {
                if (schema.Abstract) {
                    return null;
                }
                var application = MetadataProvider.Application(applicationName);
                componentsSource = application.DisplayableComponents;
                if (!MetadataProvider.FinishedParsing && MetadataProvider.ComponentsDictionary.ContainsKey(applicationName)) {
                    componentsSource = MetadataProvider.ComponentsDictionary[applicationName];
                }

            }
            var component = componentsSource.FirstOrDefault(
                f =>
                    f.Id.Equals(reference.Id,
                        StringComparison
                            .CurrentCultureIgnoreCase));
            if (component == null) {
                throw new InvalidOperationException(String.Format("displayable {0} not found in application {1} components",
                    reference.Id, applicationName));
            }
            var resultDisplayables = new List<IApplicationDisplayable>();
            foreach (var declaredDisplayable in component.RealDisplayables) {
                var cloneableDisplayable = declaredDisplayable as IPCLCloneable;
                if (cloneableDisplayable == null) {
                    throw ExceptionUtil.InvalidOperation("trying to refer a non-cloneable field:{0} on component {1}",
                        declaredDisplayable.GetType().Name, reference.Id);
                }
                var clonedDisplayable = CloneAndResolve(cloneableDisplayable, schema, reference);
                resultDisplayables.Add((IApplicationDisplayable)clonedDisplayable);
            }
            if (resultDisplayables.Count() == 1) {
                //overriding showExpression
                var applicationDisplayable = resultDisplayables.First();
                OverrideReferenceValues(schema, reference, applicationDisplayable);
            }

            return resultDisplayables;
        }

        private static void OverrideReferenceValues(ApplicationSchemaDefinition schema, ReferenceDisplayable reference, IApplicationDisplayable applicationDisplayable) {
            if (reference.ShowExpression != null) {
                applicationDisplayable.ShowExpression = GetPropertyValue(schema, reference, reference.ShowExpression);
            }
            if (reference.Label != null) {
                ((IApplicationAttributeDisplayable)applicationDisplayable).Label = GetPropertyValue(schema, reference, reference.Label);
            }
            if (reference.Attribute != null) {
                ((IApplicationAttributeDisplayable)applicationDisplayable).Attribute = GetPropertyValue(schema, reference, reference.Attribute);
            }


            applicationDisplayable.IsReadOnly = reference.IsReadOnly;


            // verify if the real reference is enabled
            XmlEnabledFieldsVerifier.VerifyEnabledField(schema, applicationDisplayable);

            // add real displayable to validate if needed
            ApplicationMetadataValidator.AddDisplaybleToValidateIfNeeded(schema, applicationDisplayable);
        }

        private static object CloneAndResolve([NotNull]IPCLCloneable declaredDisplayable, ApplicationSchemaDefinition schema, ReferenceDisplayable reference) {
            var clonedDisplayable = (declaredDisplayable).Clone();
            if (clonedDisplayable == null) {
                throw ExceptionUtil.InvalidOperation("wrong clone implementation of component. returnin null", declaredDisplayable.GetType().Name);
            }

            foreach (var propertyInfo in clonedDisplayable.GetType().GetProperties()) {

                if (!propertyInfo.CanRead || !propertyInfo.CanWrite) {
                    continue;
                }
                var componentValue = propertyInfo.GetValue(clonedDisplayable);
                var stringValue = componentValue as string;
                if (stringValue == null || !stringValue.StartsWith(PropReference)) {
                    //nothing to do
                    continue;
                }
                var value = GetPropertyValue(schema, reference, stringValue);
                propertyInfo.SetValue(clonedDisplayable, value);
            }

            if (clonedDisplayable is IApplicationDisplayableContainer) {
                //sections
                var innerDisplayables = ((IApplicationDisplayableContainer)clonedDisplayable).Displayables;
                ((IApplicationDisplayableContainer)clonedDisplayable).Displayables = DisplayableUtil.PerformReferenceReplacement(innerDisplayables, schema,
                    ComponentDisplayableResolver);

                var section = clonedDisplayable as ApplicationSection;
                if (section != null) {
                    section.Header = (ApplicationHeader)CloneAndResolve(section.Header, schema, reference);
                }

            }
            return clonedDisplayable;

        }

        private static string GetPropertyValue(ApplicationSchemaDefinition schema, ReferenceDisplayable reference,
            string stringValue) {
            if (!stringValue.StartsWith(PropReference)) {
                return stringValue;
            }

            var propName = stringValue.Substring(PropReference.Length);
            string defaultValue = null;
            var indexOf = propName.IndexOf('=');
            if (indexOf != -1) {
                defaultValue = propName.Substring(indexOf + 1);
                propName = propName.Substring(0, indexOf);
            }


            string value;
            if (!reference.Properties.TryGetValueAsString(propName, out value)) {
                if (!schema.Properties.TryGetValue(propName, out value)) {
                    if (!schema.Abstract) {
                        if (defaultValue == null) {
                            throw ExceptionUtil.InvalidOperation(
                                "property {0} not found on schema {1}, nor reference {2}", propName,
                                schema.SchemaId, reference.Id);
                        } else {
                            return defaultValue;
                        }
                    }
                }
            }
            return value;
        }
    }
}
