using softWrench.sW4.Dynamic.Model;

namespace softWrench.sW4.Dynamic.Services {
    public interface IContainerReloader
    {
        void ReloadContainer(ScriptEntry singleDynComponent);
    }
}
