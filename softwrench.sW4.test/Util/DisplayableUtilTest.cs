using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Util;

namespace softwrench.sW4.test.Util {
    [TestClass]
    public class DisplayableUtilTest {

        //using references just to make instantiation easier
        private readonly IEnumerable<IApplicationDisplayable> _listWithSecondary = new List<IApplicationDisplayable>{
                new ReferenceDisplayable {Attribute = "0"},
                new ReferenceDisplayable {Attribute = "1"},

                 new ApplicationSection
                {
                    SecondaryContent = false,
                    Displayables ={
                           new ReferenceDisplayable {Attribute = "2"},
                           new ReferenceDisplayable {Attribute = "3"},
                           new ApplicationSection{
                               Displayables ={
                                   new ReferenceDisplayable {Attribute = "4"},
                                   new ReferenceDisplayable {Attribute = "5"},
                              }
                        }
                    }
                },

                new ApplicationSection
                {
                    SecondaryContent = true,
                    Displayables ={
                           new ReferenceDisplayable {Attribute = "6"},
                           new ReferenceDisplayable {Attribute = "7"},
                           new ApplicationSection{
                               Displayables ={
                                   new ReferenceDisplayable {Attribute = "8"},
                                   new ReferenceDisplayable {Attribute = "9"},
                              }
                        }
                    }
                },
                 new ReferenceDisplayable {Attribute = "10"},
            };


        [TestMethod]
        public void GetDisplayableSecondary() {

            var result = DisplayableUtil.GetDisplayable<ReferenceDisplayable>(typeof(ReferenceDisplayable), _listWithSecondary, SchemaFetchMode.All);
            Assert.AreEqual(11, result.Count);
            int i = 0;
            foreach (var d in result) {
                Assert.AreEqual(d.Attribute, "" + i);
                i++;
            }
        }

        [TestMethod]
        public void GetDisplayableMainContent() {

            var result = DisplayableUtil.GetDisplayable<ReferenceDisplayable>(typeof(ReferenceDisplayable), _listWithSecondary, SchemaFetchMode.MainContent);
            Assert.AreEqual(7, result.Count);

            Assert.IsTrue(result.Any(a => a.Attribute == "5"));
            Assert.IsTrue(result.Any(a=> a.Attribute == "10"));

            Assert.IsFalse(result.Any(a => a.Attribute == "6"));
        }

        [TestMethod]
        public void GetDisplayableSecondaryContent() {

            var result = DisplayableUtil.GetDisplayable<ReferenceDisplayable>(typeof(ReferenceDisplayable), _listWithSecondary, SchemaFetchMode.SecondaryContent);
            Assert.AreEqual(4, result.Count);

            Assert.IsTrue(result.Any(a => a.Attribute == "6"));
            Assert.IsTrue(result.Any(a => a.Attribute == "7"));
            Assert.IsTrue(result.Any(a => a.Attribute == "8"));
            Assert.IsTrue(result.Any(a => a.Attribute == "9"));

        }


        [TestMethod]
        public void GetDisplayableFirstLevelOnly() {

            var result = DisplayableUtil.GetDisplayable<ReferenceDisplayable>(typeof(ReferenceDisplayable), _listWithSecondary, 
                SchemaFetchMode.FirstLevelOnly);
            Assert.AreEqual(3, result.Count);

            Assert.IsTrue(result.Any(a => a.Attribute == "0"));
            Assert.IsTrue(result.Any(a => a.Attribute == "1"));
            Assert.IsTrue(result.Any(a => a.Attribute == "10"));

        }
    }
}
