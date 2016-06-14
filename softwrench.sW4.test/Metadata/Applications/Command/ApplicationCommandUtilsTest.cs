using System.Collections.Generic;
using System.Linq;
using Iesi.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Metadata.Applications.Command;
using softWrench.sW4.Metadata.Security;
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.entities.security;

namespace softwrench.sW4.test.Metadata.Applications.Command {
    [TestClass]
    public class ApplicationCommandUtilsTest {

        private readonly CommandBarDefinition _commandBarDefinition = new CommandBarDefinition(null, "detail", false, new List<ICommandDisplayable>
            {
                new ResourceCommand("c1","a","role1",null,null),
                new ResourceCommand("c2","b","role2",null,null),
                new ResourceCommand("c3","c",null,null,null),
                new ResourceCommand("c4","d",null,null,null),
            }

        );


        private readonly CommandBarDefinition _commandBarDefinition2 = new CommandBarDefinition(null, "detail", false, new List<ICommandDisplayable>
            {
                new ResourceCommand("c1","a",null,null,null),
                new ResourceCommand("c2","b",null,null,null),
                new ResourceCommand("c3","c",null,null,null),
                new ResourceCommand("c4","d",null,null,null),
            }

        );

        [TestMethod]
        public void SecureBars() {
            var bars = new Dictionary<string, CommandBarDefinition>();
            bars["detail"] = _commandBarDefinition;
            var user = InMemoryUser.TestInstance("test");
            user.Roles.Add(new Role { Name = "role1" });
            var result = ApplicationCommandUtils.SecuredBars(user, bars)["detail"].Commands;
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(result[0].Id, "c1");
            //c2 should go
            Assert.AreEqual(result[1].Id, "c3");
            Assert.AreEqual(result[2].Id, "c4");
        }

        [TestMethod]
        public void SecureBarsNewVersion() {
            var bars = new Dictionary<string, CommandBarDefinition>();
            bars["app_schema_mode#gridtop"] = _commandBarDefinition2;
            var user = InMemoryUser.TestInstance("test");
            var actionPermissions = new HashedSet<ActionPermission>(){
                new ActionPermission(){
                    Schema = "schema",
                    ActionId = "c3"
                }
            };

            var permission = new ApplicationPermission() {
                ApplicationName = "app",
                ActionPermissions = actionPermissions
            };
            var mergedUserProfile = user.MergedUserProfile;
            mergedUserProfile.Permissions.Add(permission);
            


            var result = ApplicationCommandUtils.SecuredBars(user, bars)["app_schema_mode#gridtop"].Commands;
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(result[0].Id, "c1");
            Assert.AreEqual(result[1].Id, "c2");
            Assert.AreEqual(result[2].Id, "c4");

        }
    }
}
