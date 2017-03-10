using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.test.Util;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Dynamic.Model;
using softWrench.sW4.Dynamic.Services;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Dynamic.Services {
    [TestClass]
    public class JavascriptDynamicServiceTest {

        private Mock<ISWDBHibernateDAO> _swdbDAO = TestUtil.CreateMock<ISWDBHibernateDAO>();

        private JavascriptDynamicService _service;

        private static readonly long d1 = DateUtil.BeginOfDay(new DateTime(2017, 01, 01)).ToUnixTimeStamp();
        private static readonly long d2 = DateUtil.BeginOfDay(new DateTime(2017, 01, 02)).ToUnixTimeStamp();

        private static readonly ScriptDeviceInfo BaseBoth = new ScriptDeviceInfo { Platform = ClientPlatform.Both, OfflineDevice = OfflineDevice.ALL, OfflineVersions = "2.1.1,2.1.2" };

        private static readonly IList<JavascriptEntry> mockedDbList = new List<JavascriptEntry>{
                //modified at d2
                new JavascriptEntry {Target = "s1", Lastupdate = d2, Appliestoversion = "5.19", ScriptDeviceCriteria = BaseBoth},
                new JavascriptEntry {Target = "s3", Lastupdate = d1,Appliestoversion = "5.19" , ScriptDeviceCriteria = BaseBoth},
                new JavascriptEntry {Target = "s4", Lastupdate = d2,Appliestoversion = "5.19", ScriptDeviceCriteria = BaseBoth},
                new JavascriptEntry {Target = "s5", Lastupdate = d1, ScriptDeviceCriteria = new ScriptDeviceInfo {Platform = ClientPlatform.Mobile, OfflineDevice = OfflineDevice.ALL, OfflineVersions = "2.1.1"} ,Appliestoversion = "5.19"},
                new JavascriptEntry {Target = "s6", Lastupdate = d1, ScriptDeviceCriteria = new ScriptDeviceInfo {Platform = ClientPlatform.Web},Appliestoversion = "5.19" },
                new JavascriptEntry {Target = "s7", Lastupdate = d1, ScriptDeviceCriteria = BaseBoth,Appliestoversion = "5.19" },
           };

        private static readonly IList<JavascriptEntry> mockedDbList2 = new List<JavascriptEntry>(mockedDbList)
        {
            new JavascriptEntry {Target = "s8", Lastupdate = d1, ScriptDeviceCriteria = new ScriptDeviceInfo {Platform = ClientPlatform.Mobile, OfflineDevice = OfflineDevice.ALL, OfflineVersions = "2.1.1"} ,Appliestoversion = "5.19"},
            new JavascriptEntry {Target = "s9", Lastupdate = d1, ScriptDeviceCriteria = new ScriptDeviceInfo {Platform = ClientPlatform.Mobile, OfflineDevice = OfflineDevice.ALL, OfflineVersions = "2.1.1,2.1.2"} ,Appliestoversion = "5.19"},
            new JavascriptEntry {Target = "s10", Lastupdate = d1, ScriptDeviceCriteria = new ScriptDeviceInfo {Platform = ClientPlatform.Mobile, OfflineDevice = OfflineDevice.ALL, OfflineVersions = "2.1.3"} ,Appliestoversion = "5.19"},
            new JavascriptEntry {Target = "s11", Lastupdate = d1, ScriptDeviceCriteria = new ScriptDeviceInfo {Platform = ClientPlatform.Mobile, OfflineDevice = OfflineDevice.IOS, OfflineVersions = "2.1.1"} ,Appliestoversion = "5.19"},
        };


        public static string ClientStateJSON = @"
            {
            'items': {             
                'xxx': 1488497342,
                'yyy': 1488497442
             }
           }";


        [TestInitialize]
        public void Init() {
            _swdbDAO = TestUtil.CreateMock<ISWDBHibernateDAO>();
            TestUtil.ResetMocks(_swdbDAO);
            _service = new JavascriptDynamicService(_swdbDAO.Object);
            foreach (var javascriptEntry in mockedDbList) {
                javascriptEntry.ScriptDeviceCriteria.Validate(ScriptDeviceInfo.DeviceInfoValMode.Database);
            }
            foreach (var javascriptEntry in mockedDbList2) {
                javascriptEntry.ScriptDeviceCriteria.Validate(ScriptDeviceInfo.DeviceInfoValMode.Database);
            }
        }

        [TestMethod]
        public async Task SyncResultForServer() {

            // s1 update, s2 deletion, s3 intact s4,s6 and s7 creation
            IDictionary<string, long> dict = new Dictionary<string, long>{
                {"s1", d1},
                {"s2", d1},
                {"s3", d1},
            };


            _swdbDAO.Setup(f => f.FindByQueryAsync<JavascriptEntry>(JavascriptEntry.DeployedScripts)).ReturnsAsync(mockedDbList);


            var result = await _service.SyncResult(dict, ScriptDeviceInfo.Web());

            //s3 should be kept intact, not returning it
            Assert.IsFalse(result.Any(r => r.Target.Equals("s3")));
            //s5 is only for offline, do not return it
            Assert.IsFalse(result.Any(r => r.Target.Equals("s5")));

            var names = result.Select(r => r.Target);

            TestUtil.VerifyMocks(_swdbDAO);
            var enumerable = names as IList<string> ?? names.ToList();

            Assert.IsTrue(enumerable.ContainsAll(new[] { "s1", "s2", "s4", "s6", "s7" }));
            Assert.AreEqual(5, enumerable.Count());

            //marked for update
            Assert.AreEqual(d2, GetItem(result, "s1").Rowstamp);
            //marked for deletionn
            Assert.IsTrue(GetItem(result, "s2").ToDelete);
            Assert.AreEqual(d2, GetItem(result, "s4").Rowstamp);
            Assert.AreEqual(d1, GetItem(result, "s6").Rowstamp);
            Assert.AreEqual(d1, GetItem(result, "s7").Rowstamp);


        }


        [TestMethod]
        public async Task SyncResultForOffLine() {

            // s1 update, s2 deletion, s3 intact s4,s6 and s7 creation
            IDictionary<string, long> dict = new Dictionary<string, long>{
                {"s1", d1},
                {"s2", d1},
                {"s3", d1},
            };


            _swdbDAO.Setup(f => f.FindByQueryAsync<JavascriptEntry>(JavascriptEntry.DeployedScripts)).ReturnsAsync(mockedDbList2);


            var result = await _service.SyncResult(dict, new ScriptDeviceInfo { Platform = ClientPlatform.Mobile, OfflineDevice = OfflineDevice.Android, OfflineVersions = "2.1.1" });

            //s3 should be kept intact, not returning it
            Assert.IsFalse(result.Any(r => r.Target.Equals("s3")));
            //s6 is only for web, do not return it
            Assert.IsFalse(result.Any(r => r.Target.Equals("s6")));

            var names = result.Select(r => r.Target);

            TestUtil.VerifyMocks(_swdbDAO);
            Assert.IsTrue(names.ContainsAll(new[] { "s1", "s2", "s4", "s5", "s7", "s8", "s9" }));
            Assert.AreEqual(7, names.Count());
            //marked for update
            Assert.AreEqual(d2, GetItem(result, "s1").Rowstamp);
            //marked for deletionn
            Assert.IsTrue(GetItem(result, "s2").ToDelete);
            Assert.AreEqual(d2, GetItem(result, "s4").Rowstamp);
            Assert.AreEqual(d1, GetItem(result, "s5").Rowstamp);
            Assert.AreEqual(d1, GetItem(result, "s7").Rowstamp);

        }

        [TestMethod]
        public void JsonConverterTest() {
            var ob = JObject.Parse(ClientStateJSON);
            var result= _service.ConvertToRowstampDict(ob);
            Assert.AreEqual(2,result.Count);
            Assert.AreEqual(result["xxx"], 1488497342);
            Assert.AreEqual(result["yyy"], 1488497442);
        }

        private static ScriptSyncResultDTO GetItem(ISet<ScriptSyncResultDTO> result, string name) {
            return result.First(s => s.Target.EqualsIc(name));
        }
    }
}
