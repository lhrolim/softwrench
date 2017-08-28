using cts.commons.simpleinjector;

namespace softwrench.sw4.api.classes.user {
    public interface ISecurityFacade : ISingletonComponent {

        ISWUser Current(bool fetchFromDB = true);

    }
}
