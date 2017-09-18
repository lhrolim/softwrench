using System.Net;

namespace softwrench.sw4.api.classes.exception
{
    public interface IStatusCodeException
    {
        HttpStatusCode StatusCode { get; }
    }
}