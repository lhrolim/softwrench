using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Data.Persistence.WS.Rest;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;

namespace softwrench.sW4.test.Data.Persistence.WS.Rest {
    [TestClass]
    public class RestCrudConnectorTest : BaseOtbMetadataTest {

        private string SRResponse =
            @"<CreateSWSRResponse xmlns=""http://www.ibm.com/maximo"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" creationDateTime=""2016-05-16T13:40:20-07:00"" transLanguage=""EN"" 
                baseLanguage=""EN"" messageID=""1463431220814699507"" maximoVersion=""7 5 20140411-2000 V7511--1"">
                  <SWSRSet>
                    <SR rowstamp=""[0 0 0 0 2 58 -111 5]"">
                      <ACTLABCOST>0.0</ACTLABCOST>
                      <ACTLABHRS>0.0</ACTLABHRS>
                      <AFFECTEDDATE>2016-05-16T13:40:00-07:00</AFFECTEDDATE>
                      <AFFECTEDEMAIL>ahabbu@controltechnologysolutions.com</AFFECTEDEMAIL>
                      <AFFECTEDPERSON>SWADMIN</AFFECTEDPERSON>
                      <ASSETORGID>EAGLENA</ASSETORGID>
                      <ASSETSITEID>BEDFORD</ASSETSITEID>
                      <CHANGEBY>SWADMIN</CHANGEBY>
                      <CHANGEDATE>2016-05-16T13:41:00-07:00</CHANGEDATE>
                      <CLASS>SR</CLASS>
                      <CREATEWOMULTI>MULTI</CREATEWOMULTI>
                      <DESCRIPTION>test2396</DESCRIPTION>
                      <HASACTIVITY>0</HASACTIVITY>
                      <HASSOLUTION>0</HASSOLUTION>
                      <HISTORYFLAG>0</HISTORYFLAG>
                      <INHERITSTATUS>1</INHERITSTATUS>
                      <ISGLOBAL>0</ISGLOBAL>
                      <ISKNOWNERROR>0</ISKNOWNERROR>
                      <ORGID>EAGLENA</ORGID>
                      <RELATEDTOGLOBAL>0</RELATEDTOGLOBAL>
                      <REPORTDATE>2016-05-16T13:40:00-07:00</REPORTDATE>
                      <REPORTEDBY>SWADMIN</REPORTEDBY>
                      <REPORTEDEMAIL>ahabbu@controltechnologysolutions.com</REPORTEDEMAIL>
                      <SELFSERVSOLACCESS>1</SELFSERVSOLACCESS>
                      <SITEID>BEDFORD</SITEID>
                      <SITEVISIT>0</SITEVISIT>
                      <STATUS>NEW</STATUS>
                      <STATUSDATE>2016-05-16T13:40:20-07:00</STATUSDATE>
                      <TEMPLATE>0</TEMPLATE>
                      <TICKETID>2154</TICKETID>
                      <TICKETUID>4682</TICKETUID>
                    </SR>
                  </SWSRSet>
                </CreateSWSRResponse>";


        [TestMethod]
        public void TestParseResponse() {
            var sr = MetadataProvider.Entity("sr");

            var targetResult= RestCrudConnector.ParseResult(sr, SRResponse);
            Assert.AreEqual("BEDFORD",targetResult.SiteId);
            Assert.AreEqual("2154",targetResult.UserId);
            Assert.AreEqual("4682", targetResult.Id);
            Assert.AreEqual(SRResponse, SRResponse);
        }
    }
}
