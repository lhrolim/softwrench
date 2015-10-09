using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Definitions.WhereClause;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Security.Context;
using System.Collections.Generic;
using System.Linq;

namespace softwrench.sW4.test.Configuration.Service {
    [TestClass]
    public class ConfigurationServiceTest {

        private readonly IEnumerable<PropertyValue> _values = new List<PropertyValue>
            {
                new PropertyValue {Value = "1"},
                new PropertyValue {Value = "2",Module = "xitc"},
                new PropertyValue {Value = "3",Module = "purchase"},
                new PropertyValue {Value = "4",UserProfile= 2},
                new PropertyValue {Value = "5",UserProfile = 3},
                new PropertyValue {Value = "6",UserProfile = 2,Condition = new WhereClauseCondition{AppContext = new ApplicationLookupContext{MetadataId = "xxx"}}},
                new PropertyValue {Value = "7",UserProfile = 3,Condition = new WhereClauseCondition{AppContext = new ApplicationLookupContext{Schema = "detail"}}},
                new PropertyValue {Value = "8",Condition = new WhereClauseCondition{AppContext = new ApplicationLookupContext{Schema = "detail"}}},
                new PropertyValue {Value = "9",Condition = new WhereClauseCondition{AppContext = new ApplicationLookupContext{MetadataId = "zzz"}}},
            };


        private readonly IEnumerable<PropertyValue> _values3 = new List<PropertyValue>
            {
                new PropertyValue {Value = "1"},
                new PropertyValue {Value = "7",UserProfile = 3,Condition = new WhereClauseCondition{AppContext = new ApplicationLookupContext{Schema = "detail"}}},
                new PropertyValue {Value = "8",Condition = new WhereClauseCondition{AppContext = new ApplicationLookupContext{Schema = "detail"}}},
            };

        private readonly IEnumerable<PropertyValue> _values4 = new List<PropertyValue>
            {
                new PropertyValue {Value = "7",UserProfile = 3,Condition = new WhereClauseCondition{AppContext = new ApplicationLookupContext{Schema = "crazynameschema"}}},
            };


        private readonly IEnumerable<PropertyValue> _list2NoDefault = new List<PropertyValue>
            {
                new PropertyValue {Value = "2",Module = "xitc"},
                new PropertyValue {Value = "3",Module = "purchase"},
            };

        private readonly IEnumerable<PropertyValue> _allModulesTest = new List<PropertyValue>
            {
                new PropertyValue {Value = "2",Module = "#all"},
                new PropertyValue {Value = "3",Module = "purchase"},
                new PropertyValue {Value = "4",Module = "sso,tui"},
            };

        private readonly IEnumerable<PropertyValue> _list1ProfileNoModule = new List<PropertyValue>
        {
            new PropertyValue {Value = "1", UserProfile = 2},
        };

        [TestMethod]
        public void _OnlineConditionsOnly() {
            var context = new ContextHolder { OfflineMode = false };
            IEnumerable<PropertyValue> offlineModeValues = new List<PropertyValue>
            {
                new PropertyValue {Value = "1", Condition = new WhereClauseCondition{OfflineOnly = true}},
                new PropertyValue {Value = "2", Condition = new WhereClauseCondition{OfflineOnly = true,AppContext = new ApplicationLookupContext{Schema = "detail"}}},
                new PropertyValue {Value = "3", Condition = new WhereClauseCondition{OfflineOnly = true,AppContext = new ApplicationLookupContext{MetadataId = "zzz"}}}
            };
            var result = ConfigurationService.BuildResultValues<string>(offlineModeValues, context);
            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void _OfflinePartialMatch() {
            var context = new ContextHolder { OfflineMode = true };
            IEnumerable<PropertyValue> offlineModeValues = new List<PropertyValue>
            {
                new PropertyValue {Value = "1", Condition = new WhereClauseCondition{OfflineOnly = false}},
            };
            var result = ConfigurationService.BuildResultValues<string>(offlineModeValues, context);
            Assert.AreEqual("1", result.First().Value.Value);
        }

        [TestMethod]
        public void _MostCorrectCondition() {
            var context = new ContextHolder { OfflineMode = true,ApplicationLookupContext = new ApplicationLookupContext { MetadataId = "zzz" } };
            IEnumerable<PropertyValue> values = new List<PropertyValue>
            {
                new PropertyValue {Value = "1", Condition = new WhereClauseCondition{OfflineOnly = false,AppContext = new ApplicationLookupContext{MetadataId = "zzz", }}},
                new PropertyValue {Value = "2", Condition = new WhereClauseCondition{OfflineOnly = true,AppContext = new ApplicationLookupContext{MetadataId = "zzz"}}}
            };
            var result = ConfigurationService.BuildResultValues<string>(values, context);
            Assert.AreEqual("2", result.First().Value.Value);
        }

        [TestMethod]
        public void _2ConditionsOneWithModuleAnotherWithout_returnDefault() {
            var context = new ContextHolder();
            IEnumerable<PropertyValue> values = new List<PropertyValue>
            {
                new PropertyValue {Value = "1"},
                new PropertyValue {Value = "2",Module = "xxx"}
            };
            var result = ConfigurationService.BuildResultValues<string>(values, context);
            Assert.AreEqual("1", result.First().Value.Value);
        }

        [TestMethod]
        public void AskForModule_DoNotConsiderDefault() {
            var context = new ContextHolder { Module = "yyy" };
            IEnumerable<PropertyValue> values = new List<PropertyValue>
            {
                new PropertyValue {Value = "1"},
                new PropertyValue {Value = "2",Module = "xxx"}
            };
            var result = ConfigurationService.BuildResultValues<string>(values, context);
            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void SR_HAPAG_CONDITION() {
            var context = new ContextHolder {
                UserProfiles = new SortedSet<int?> { 3 },
                ApplicationLookupContext = new ApplicationLookupContext { Schema = "list" }
            };
            var result = ConfigurationService.BuildResultValues<string>(_values, context);
            Assert.AreEqual("5", result.First().Value.Value);

            context = new ContextHolder {
                UserProfiles = new SortedSet<int?> { 3 },
                ApplicationLookupContext = new ApplicationLookupContext { Schema = "detail" }
            };
            result = ConfigurationService.BuildResultValues<string>(_values, context);
            Assert.AreEqual("7", result.First().Value.Value);





            context = new ContextHolder {
                UserProfiles = new SortedSet<int?> { 3 },
            };
            result = ConfigurationService.BuildResultValues<string>(_values, context);
            Assert.AreEqual("5", result.First().Value.Value);
        }

        [TestMethod]
        public void AskForSchemaWithProfile() {
            var context = new ContextHolder {
                UserProfiles = new SortedSet<int?> { 3 },
                ApplicationLookupContext = new ApplicationLookupContext { Schema = "detail" }
            };
            var result = ConfigurationService.BuildResultValues<string>(_values, context);
            Assert.AreEqual("7", result.First().Value.Value);
        }

        [TestMethod]
        public void AskWithCurrentSelectedProfile() {
            var context = new ContextHolder {
                UserProfiles = new SortedSet<int?> { 2,3 },
                CurrentSelectedProfile = 3
            };
            var result = ConfigurationService.BuildResultValues<string>(_values, context);
            Assert.AreEqual("5", result.First().Value.Value);
        }

        [TestMethod]
        public void AskForSchemaWithProfile2() {
            var context = new ContextHolder {
                UserProfiles = new SortedSet<int?> { 4 },
                ApplicationLookupContext = new ApplicationLookupContext { Schema = "detail" }
            };
            var result = ConfigurationService.BuildResultValues<string>(_values3, context);
            Assert.AreEqual("8", result.First().Value.Value);
        }
        [TestMethod]
        public void AskForSchemaDoNotFind() {
            var context = new ContextHolder {
                ApplicationLookupContext = new ApplicationLookupContext { Schema = "list" }
            };
            var result = ConfigurationService.BuildResultValues<string>(_values4, context);
            Assert.IsFalse(result.Any());
        }





        [TestMethod]
        public void MetadataAskTest() {
            var context = new ContextHolder {
                UserProfiles = new SortedSet<int?> { 2 },
                ApplicationLookupContext = new ApplicationLookupContext { MetadataId = "xxx" },
                SiteId = "site"
            };
            var result = ConfigurationService.BuildResultValues<string>(_values, context);
            Assert.AreEqual("6", result.First().Value.Value);
        }

        [TestMethod]
        public void MetadataAskTest2() {
            var context = new ContextHolder {
                UserProfiles = new SortedSet<int?> { 2 },
                ApplicationLookupContext = new ApplicationLookupContext { MetadataId = "zzz" },
                SiteId = "site"
            };
            var result = ConfigurationService.BuildResultValues<string>(_values, context);
            Assert.AreEqual("9", result.First().Value.Value);
        }

        [TestMethod]
        public void ModuleAskTest() {
            var context = new ContextHolder {
                Module = "xitc",
            };
            var result = ConfigurationService.BuildResultValues<string>(_values, context);
            Assert.AreEqual("2", result.First().Value.Value);
        }

        [TestMethod]
        public void ModuleAskTestCombination() {
            IEnumerable<PropertyValue> allModulesTest = new List<PropertyValue>
            {
                new PropertyValue {Value = "2",Module = Conditions.AnyCondition},
                new PropertyValue {Value = "3",Module = "purchase"},
                new PropertyValue {Value = "4",Module = "sso,tui"},
            };

            var context = new ContextHolder {
                Module = "xitc",
            };
            var result = ConfigurationService.BuildResultValues<string>(allModulesTest, context);
            Assert.AreEqual("2", result.First().Value.Value);


            context = new ContextHolder {
                Module = "purchase",
            };
            result = ConfigurationService.BuildResultValues<string>(allModulesTest, context);
            Assert.AreEqual("3", result.First().Value.Value);

            context = new ContextHolder {
                Module = "sso",
            };
            result = ConfigurationService.BuildResultValues<string>(allModulesTest, context);
            Assert.AreEqual("4", result.First().Value.Value);

            context = new ContextHolder {
                Module = null,
            };
            result = ConfigurationService.BuildResultValues<string>(allModulesTest, context);
            Assert.AreEqual("2", result.First().Value.Value);
        }

        /// <summary>
        /// simulating a case where we have a condition for the profile registered but none for the module... the module should be either exact or none...
        /// </summary>
        [TestMethod]
        public void ModuleAskTest2() {
            var context = new ContextHolder {
                Module = "xitc",
                UserProfiles = new SortedSet<int?> { 2 },
            };
            var result = ConfigurationService.BuildResultValues<string>(_list1ProfileNoModule, context);
            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void AskAndDoNotFind() {
            var context = new ContextHolder();
            var result = ConfigurationService.BuildResultValues<string>(_list2NoDefault, context);
            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void GlobalCondition() {
            var context = new ContextHolder { OfflineMode = true };


            IEnumerable<PropertyValue> offlineModeValues = new List<PropertyValue>
            {
                new PropertyValue {Value = "1", Condition = new WhereClauseCondition{ OfflineOnly = false}},
                new PropertyValue {Value = "2", Condition = new WhereClauseCondition{ Global = true,OfflineOnly = true}}
                
            };
            var result = ConfigurationService.BuildResultValues<string>(offlineModeValues, context);
            Assert.AreEqual("2", result.First().Value.Value);
        }
    }
}
