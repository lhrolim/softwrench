using System;
using System.Collections.Generic;
using System.Linq;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sw4.Shared2.Metadata.Applications.UI;

namespace softwrench.sW4.Shared2.Util {

    public enum SchemaFetchMode {
        FirstLevelOnly, MainContent, SecondaryContent, All
    }

    public class DisplayableUtil {



        public static IList<T> GetDisplayable<T>(Type displayableType, IEnumerable<IApplicationDisplayable> originalDisplayables,
            SchemaFetchMode schemaFetchMode = SchemaFetchMode.All, bool insideSecondary = false, Type[] typesToCheck = null) {
            return GetDisplayable<T>(new[] { displayableType }, originalDisplayables, schemaFetchMode, insideSecondary);
        }

        public static IList<T> GetDisplayable<T>(Type[] typesToCheck,
            IEnumerable<IApplicationDisplayable> originalDisplayables, SchemaFetchMode schemaFetchMode,
            bool insideSecondary, string tabOfInterest = null, string tabContext = null) {
            var resultingDisplayables = new List<T>();

            foreach (IApplicationDisplayable displayable in originalDisplayables) {

                if (typesToCheck.Any(typeToCheck => typeToCheck.IsInstanceOfType(displayable))) {
                    if (schemaFetchMode != SchemaFetchMode.SecondaryContent || insideSecondary) {
                        if (tabOfInterest == null || tabOfInterest.Equals(tabContext)) {
                            resultingDisplayables.Add((T)displayable);
                        }
                    }
                }

                if (displayable is IApplicationDisplayableContainer &&
                        SchemaFetchMode.FirstLevelOnly != schemaFetchMode) {
                    var container = displayable as IApplicationDisplayableContainer;
                    var section = container as ApplicationSection;
                    var isSecondarySection = section != null && section.SecondaryContent;
                    if (isSecondarySection && SchemaFetchMode.MainContent.Equals(schemaFetchMode)) {
                        //under some circustances we might not be interested in returning the secondary content displayables
                        continue;
                    }
                    var tabDefinition = container as ApplicationTabDefinition;
                    //                    if (tabDefinition != null && tabOfInterest != null && tabDefinition.TabId != tabOfInterest) {
                    //                        
                    //                    }
                    if (tabDefinition != null) {
                        tabContext = tabDefinition.TabId;
                    }

                    resultingDisplayables.AddRange(GetDisplayable<T>(typesToCheck, container.Displayables,
                        schemaFetchMode,
                        insideSecondary || isSecondarySection, tabOfInterest, tabContext));
                }
            }
            return resultingDisplayables;
        }

        public static List<IApplicationDisplayable> PerformReferenceReplacement(IEnumerable<IApplicationDisplayable> displayables, ApplicationSchemaDefinition schema,
            ApplicationSchemaDefinition.LazyComponentDisplayableResolver componentDisplayableResolver, IEnumerable<DisplayableComponent> components = null) {

            var realDisplayables = new List<IApplicationDisplayable>();


            foreach (var displayable in displayables) {
                if (displayable is ReferenceDisplayable) {
                    var reference = componentDisplayableResolver((ReferenceDisplayable)displayable, schema, components);
                    if (reference == null) {
                        //interrupting things here...
                        return null;
                    }
                    realDisplayables.AddRange(reference);
                } else if (displayable is IApplicationDisplayableContainer) {
                    var container = (IApplicationDisplayableContainer)displayable;
                    var references = PerformReferenceReplacement(container.Displayables, schema,
                        componentDisplayableResolver);
                    if (references == null) {
                        return null;
                    }
                    container.Displayables = references;
                    realDisplayables.Add((IApplicationDisplayable)container);
                } else {
                    realDisplayables.Add(displayable);
                }
            }
            return realDisplayables;
        }



    }
}
