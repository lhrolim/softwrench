using System.Threading;
using System.Threading.Tasks;

namespace softWrench.Mobile.Communication.SignIn
{
    internal interface ISignIn
    {
        Task<LocalSignInResult> SignInLocallyAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<RemoteSignInResult> SignInRemotelyAsync (string userName, string password, CancellationToken cancellationToken = default(CancellationToken));
    }
}