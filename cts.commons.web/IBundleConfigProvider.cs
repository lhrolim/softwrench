using System.Web.Optimization;
using cts.commons.simpleinjector;

namespace cts.commons.web
{
    public interface IBundleConfigProvider :IComponent
    {
        void PopulateStyleBundles(BundleCollection bundles);

        void PopulateScriptBundles(BundleCollection bundles);
    }
}