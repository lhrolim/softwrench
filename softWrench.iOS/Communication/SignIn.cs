using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using softWrench.iOS.Utilities;
using softWrench.Mobile.Communication;
using softWrench.Mobile.Communication.Http;
using softWrench.Mobile.Communication.SignIn;
using softWrench.Mobile.Exceptions;
using softWrench.Mobile.Metadata;
using softWrench.Mobile.Persistence;

namespace softWrench.iOS.Communication {

    internal sealed class SignIn : ISignIn
    {
        private static async Task CacheRemoteAuthenticationData(User user)
        {
            var repository = new UserRepository();

            // First let's save the cookies used
            // for the remote authentication.            
            var cookies = CookieContainerProvider
                .Container
                .GetCookies(User.Settings.ServerAsUri());

            await repository.SaveAsync(cookies);

            // And now saves all user-specific data.
            await repository.SaveAsync(user);

            SetAuthenticatedUser(user, cookies);
        }

        private static void SetAuthenticatedUser(User user, CookieCollection cookies)
        {
            var cookieContainer = new CookieContainer();

            // Recreates the cookie container
            // for the current user;            
            if (null != cookies)
            {
                cookieContainer.Add(cookies);
            }

            CookieContainerProvider.Container = cookieContainer;

            // Stores the current user for
            // convenient access later.
            User.Current = user;
        }

        private static async Task EnsureSettings(UserRepository repository)
        {
            var settings = await repository.LoadSettingsAsync();

            if (null == settings)
            {
                throw new InvalidSettingsException();
            }

            User.Settings = settings;
        }

        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(1);

        private PostSignIn.Result SignInRemotely(string userName, string password, CancellationToken cancellationToken)
        {
            // If there is no network, we won't
            // even bother trying.
            if (false == NetworkStatus.IsReachable())
            {
                return null;
            }

            var postSignIn = new PostSignIn().ExecuteAsync(userName, password, cancellationToken);

            // Let's wait for the synchronization
            // to complete, but within a timeout.           
            var completed = postSignIn
                .Wait(_timeout);

            // If the task did not complete, we'll
            // report the network is unreachable.
            if (false == completed)
            {
                return null;
            }

            return postSignIn.Result;
        }

        public async Task<LocalSignInResult> SignInLocallyAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var userRepository = new UserRepository();
            await EnsureSettings(userRepository);

            // Do we already have a user
            // stored locally?            
            var user = await userRepository.LoadAsync();

            if (null == user)
            {
                return LocalSignInResult.Expired;
            }

            // Now we need to retrieve and restore
            // the authentication cookies in use
            // by the current user.
            var cookieCollection = await userRepository
                .LoadCookieCollectionAsync();

            SetAuthenticatedUser(user, cookieCollection);

            // As we don't store the passwords
            // locally yet,
            return LocalSignInResult.Success;
        }

        public async Task<RemoteSignInResult> SignInRemotelyAsync(string userName, string password, CancellationToken cancellationToken = default(CancellationToken))
        {
            await EnsureSettings(new UserRepository());
            
            var result = await Task
                .Factory
                .StartNew(() => SignInRemotely(userName, password, cancellationToken), cancellationToken);

            if (null == result)
            {
                return RemoteSignInResult.Unreachable;
            }

            // TODO: this is actually wrong. Maybe the
            // auth failed because of some exception.
            if (false == result.IsSuccess)
            {
                return RemoteSignInResult.BadCredentials;
            }

            // Stores the user locally so he won't
            // need to sign in for some time.            
            await CacheRemoteAuthenticationData(result.User);

            return RemoteSignInResult.Success;
        }
    }
}