﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Metadata.Applications.Command;
using softWrench.sW4.Metadata.Security;
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using softwrench.sw4.user.classes.entities;

namespace softwrench.sW4.test.Metadata.Applications.Command {
    [TestClass]
    public class ApplicationCommandUtilsTest {

        private readonly CommandBarDefinition _commandBarDefinition = new CommandBarDefinition(null, "detail",false, new List<ICommandDisplayable>
            {
                new ResourceCommand("c1","a","role1",null,null),
                new ResourceCommand("c2","b","role2",null,null),
                new ResourceCommand("c3","c",null,null,null),
                new ResourceCommand("c4","d",null,null,null),
            });

        [TestMethod]
        public void SecureBars()
        {
            var bars = new Dictionary<string, CommandBarDefinition>();
            bars["detail"] = _commandBarDefinition;
            var user = InMemoryUser.TestInstance("test");
            user.Roles.Add(new Role {Name = "role1"});
            var result = ApplicationCommandUtils.SecuredBars(user, bars)["detail"].Commands;
            Assert.AreEqual(3,result.Count);
            Assert.IsNotNull(result.FirstOrDefault(f => f.Id == "c1"));
            Assert.IsNull(result.FirstOrDefault(f=> f.Id == "c2"));
        }
    }
}