using System;
using cts.commons.simpleinjector;
using softWrench.sW4.Security.Entities;

namespace softWrench.sW4.Security.Setup {
    public interface IUserLinkManager : ISingletonComponent {

        string GenerateTokenLink(User user);

        User RetrieveUserByLink(string link);

        void DeleteLink(User user);

    }
}