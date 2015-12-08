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


        public static IList<T> GetDisplayable<T>(Type displayableType, IEnumerable<IApplicationDisplayable> originalDisplayables, bool fetchInner = true, bool fetchSecondaryContent = true) {
            var resultingDisplayables = new List<T>();

            foreach (IApplicationDisplayable displayable in originalDisplayables) {
                if (displayableType.IsInstanceOfType(displayable)) {
                    resultingDisplayables.Add((T)displayable);
                }
                if (displayable is IApplicationDisplayableContainer && fetchInner) {
                    var container = displayable as IApplicationDisplayableContainer;
                    var section = container as ApplicationSection;
                    if (section != null && (section.SecondaryContent && !fetchSecondaryContent)) {
                        //under some circustances we might not be interested in returning the secondary content displayables
                        continue;
                    }
                    resultingDisplayables.AddRange(GetDisplayable<T>(displayableType, container.Displayables, true,
                        fetchSecondaryContent));
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
