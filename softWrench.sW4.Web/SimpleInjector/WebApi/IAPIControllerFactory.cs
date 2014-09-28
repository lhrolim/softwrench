using System.Web.Http;

namespace softWrench.sW4.Web.SimpleInjector.WebApi
{
    public interface IAPIControllerFactory
    {
        ApiController CreateNew(string name);
    }
}