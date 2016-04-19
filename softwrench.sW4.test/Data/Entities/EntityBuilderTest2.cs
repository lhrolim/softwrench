using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Data.Entities {
    [TestClass]
    public class EntityBuilderTest2 : BaseMetadataTest {


        [TestInitialize]
        public void Init() {
            if (ApplicationConfiguration.TestclientName != "entegra") {
                ApplicationConfiguration.TestclientName = "entegra";
                MetadataProvider.StubReset();
            }
        }


        [TestMethod]
        public void TestCreation() {
            MetadataProvider.StubReset();
            var metadata = MetadataProvider.Entity("workorder");
            var wo = JObject.Parse(new StreamReader("jsons\\workorder\\test1.json").ReadToEnd());
            var entity = EntityBuilder.BuildFromJson<Entity>(typeof(Entity), metadata, null, wo, null);
            Assert.IsNull(entity.GetAttribute("workorderid"));
            var worktype = entity.GetRelationship("worktype") as Entity;
            Assert.IsNotNull(worktype);
            Assert.AreEqual("R0 Blade", worktype.GetAttribute("wtypedesc"));
            Assert.IsNull(worktype.GetAttribute("statusdate"));
            //            Assert.AreEqual("A very long description.", entity.GetAttribute("DESCRIPTION_LONGDESCRIPTION"));

            Assert.AreEqual(100, worktype.GetAttribute("worktypeid"));

            var worklogs = entity.GetRelationship("worklog") as IList<Entity>;
            Assert.IsNotNull(worklogs);
            Assert.AreEqual(worklogs.Count, 2);
            Assert.AreEqual("unknownvalue", entity.GetUnMappedAttribute("unknown"));
            Assert.AreEqual(Convert.ToInt64(100), worklogs[0].GetAttribute("worklogid"));
            Assert.AreEqual("UPP", worklogs[0].GetAttribute("siteid"));
            Assert.AreEqual(Convert.ToInt64(200), worklogs[1].GetAttribute("worklogid"));
        }

        [TestMethod]
        public void TestCreationNewWorkLog() {
            var metadata = MetadataProvider.Entity("workorder");
            var wo = JObject.Parse(new StreamReader("jsons\\workorder\\test4.json").ReadToEnd());
            var entity = EntityBuilder.BuildFromJson<Entity>(typeof(Entity), metadata, null, wo, null);
            Assert.IsNull(entity.GetAttribute("workorderid"));
            var worktype = entity.GetRelationship("worktype") as Entity;
            Assert.IsNotNull(worktype);
            Assert.AreEqual("R0 Blade", worktype.GetAttribute("wtypedesc"));
            //            Assert.AreEqual("A very long description.", entity.GetAttribute("DESCRIPTION_LONGDESCRIPTION"));

            Assert.AreEqual(100, worktype.GetAttribute("worktypeid"));

            var worklogs = entity.GetRelationship("worklog") as IList<Entity>;
            Assert.IsNotNull(worklogs);
            Assert.AreEqual(worklogs.Count, 2);

            Assert.AreEqual("UPP", worklogs[0].GetAttribute("siteid"));

        }





        [TestMethod]
        public void TestUpdate() {
            var metadata = MetadataProvider.Entity("workorder");
            var wo = JObject.Parse(new StreamReader("jsons\\workorder\\test2.json").ReadToEnd());
            var entity = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), metadata, null, wo, "100");
            Assert.AreEqual("10", entity.GetAttribute("wonum"));
            Assert.AreEqual("10", entity.UserId);
            var worktype = entity.GetRelationship("worktype") as Entity;
            Assert.IsNotNull(worktype);
            Assert.IsNotNull(worktype as CrudOperationData);
            Assert.AreEqual("R0 Blade", worktype.GetAttribute("wtypedesc"));
            //            Assert.AreEqual("A very long description.", entity.GetAttribute("DESCRIPTION_LONGDESCRIPTION"));

            Assert.AreEqual(100, worktype.GetAttribute("worktypeid"));

            var worklogs = entity.GetRelationship("worklog") as IList<CrudOperationData>;
            Assert.IsNotNull(worklogs);
            Assert.AreEqual(worklogs.Count, 2);

            Assert.AreEqual(Convert.ToInt64(100), worklogs[0].GetAttribute("worklogid"));
            Assert.AreEqual("UPP", worklogs[0].GetAttribute("siteid"));
            Assert.AreEqual(Convert.ToInt64(200), worklogs[1].GetAttribute("worklogid"));
        }





    }
}
