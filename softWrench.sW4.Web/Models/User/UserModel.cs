using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Web.Models.User
{
    public class UserModel
    {
        private readonly string _userDataJson;

        public UserModel(InMemoryUser inMemoryUser)
        {
            if (inMemoryUser != null)
            {
                _userDataJson = JsonConvert.SerializeObject(inMemoryUser, Newtonsoft.Json.Formatting.None,

                    new JsonSerializerSettings()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });
            }
        }

        public string UserDataJSON
        {
            get { return _userDataJson; }
        }
    }
}