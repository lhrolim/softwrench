using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using softWrench.Mobile.Communication.Http;
using softWrench.Mobile.Metadata;

namespace softWrench.Mobile.Communication.SignIn
{
    internal sealed class PostSignIn
    {
        private static Result ReadResponse(TextReader response)
        {
            var json = response.ReadToEnd();

            // This json doesn't use camelcase
            // serialization as the rest of the api.
            var user = JsonConvert.DeserializeObject<User>(json);

            return new Result(true, user);
        }

        private static FormUrlEncodedContent CreatePostContent(string userName, string password)
        {
            var parameters = new Dictionary<string, string>
                             {
                                 {"userName", userName},
                                 {"password", password}
                             };

            var content = new FormUrlEncodedContent(parameters);
            return content;
        }

        public async Task<Result> ExecuteAsync(string userName, string password, CancellationToken cancellationToken = default(CancellationToken))
        {
            var content = CreatePostContent(userName, password);

            try
            {
                var signIn = User.Settings.Routes.SignIn();
                using (var reader = await HttpCall.PostStreamAsync(signIn, content, cancellationToken))
                {
                    return ReadResponse(reader);
                }
            }
            catch (HttpRequestException)
            {
                // Although the API provides a
                // "found" property, we'll go
                // old school and rely on the
                // HTTP status code.
                return new Result(false, null);
            }            
        }

        public sealed class Result
        {
            private readonly bool _isSuccess;
            private readonly User _user;

            public Result(bool isSuccess, User user)
            {
                _isSuccess = isSuccess;
                _user = user;
            }

            public bool IsSuccess
            {
                get { return _isSuccess; }
            }

            public User User
            {
                get { return _user; }
            }
        }
    }
}