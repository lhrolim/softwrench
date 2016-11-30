using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using cts.commons.Util;
using softwrench.sw4.api.classes.configuration;

namespace softwrench.sw4.user.classes.config {
    public class UserConfigurationRegistry : ISingletonComponent {

        private readonly IConfigurationFacadeCommons _facade;

        public UserConfigurationRegistry(IConfigurationFacadeCommons facade) {
            _facade = facade;


            AsyncHelper.RunSync(() => DoRegister(facade));
        }

        private async Task DoRegister(IConfigurationFacadeCommons facade) {

            await _facade.RegisterAsync(UserConfigurationConstants.ChangePasswordUponStart, new PropertyDefinitionRegistry {
                Description = "Whether the password for newly created users needs to be set upon first login",
                DataType = "boolean",
                DefaultValue = "false",
            });


            await _facade.RegisterAsync(UserConfigurationConstants.MinPasswordHistorySize, new PropertyDefinitionRegistry {
                Description =
                    "If greater than 0 the users won´t be able to reuse that password if it was used on the last nth times",
                DataType = "int",
                DefaultValue = "0",
            });

            await _facade.RegisterAsync(UserConfigurationConstants.PasswordExpirationTime, new PropertyDefinitionRegistry {
                Description = "Number of days for a password to expire. If set to 0 or negative, it will never expire",
                DataType = "int",
                DefaultValue = "90",
            });

            await _facade.RegisterAsync(UserConfigurationConstants.WrongPasswordAttempts, new PropertyDefinitionRegistry {
                Description = "Number of wrong password before the user gets locked out",
                DataType = "int",
                DefaultValue = "5",
            });


            await _facade.RegisterAsync(UserConfigurationConstants.LdapServer, new PropertyDefinitionRegistry {
                Description = "Ldap Server IP",
                DataType = "string",
                DefaultValue = null,
            });

            await _facade.RegisterAsync(UserConfigurationConstants.LdapPort, new PropertyDefinitionRegistry {
                Description = "Ldap Server Port",
                DataType = "string",
                DefaultValue = null,
            });

            await _facade.RegisterAsync(UserConfigurationConstants.LdapBaseDn, new PropertyDefinitionRegistry {
                Description = "Ldap Server Base Domain",
                DataType = "string",
                DefaultValue = null,
            });
        }
    }
}
