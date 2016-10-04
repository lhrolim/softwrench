using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sw4.Shared2.Metadata.Applications.UI;

namespace softwrench.sW4.Shared2.Util {
    public class DisplayableUtil {
        public static IList<T> GetDisplayable<T>(Type displayableType, IEnumerable<IApplicationDisplayable> originalDisplayables, Boolean fetchInner = true, bool removeHidden = false) {
            var resultingDisplayables = new List<T>();

            foreach (IApplicationDisplayable displayable in originalDisplayables) {
                if (displayableType.IsInstanceOfType(displayable)) {
                    resultingDisplayables.Add((T)displayable);
                }
                if (displayable is IApplicationDisplayableContainer) {
                    if (fetchInner) {
                        var section = displayable as ApplicationSection;
                        if (section != null && section.ShowExpression.Equals("false") && removeHidden) {
                            continue;
                        }
                        var container = displayable as IApplicationDisplayableContainer;
                        resultingDisplayables.AddRange(GetDisplayable<T>(displayableType, container.Displayables, fetchInner));
                    }
                }

            }
            return resultingDisplayables;
        }

        public static List<IApplicationDisplayable> PerformReferenceReplacement(IEnumerable<IApplicationDisplayable> displayables, ApplicationSchemaDefinition schema,
            ApplicationSchemaDefinition.LazyComponentDisplayableResolver componentDisplayableResolver) {

            var realDisplayables = new List<IApplicationDisplayable>();


            foreach (var displayable in displayables) {
                if (displayable is ReferenceDisplayable) {
                    var reference = componentDisplayableResolver((ReferenceDisplayable)displayable, schema);
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
