using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Data.Persistence.WS.Rest;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;

namespace softwrench.sW4.test.Data.Persistence.WS.Rest {
    [TestClass]
    public class RestCrudConnectorTest : BaseOtbMetadataTest {

        private const string MBOSrResponse =
            @"<SR xmlns=""http://www.ibm.com/maximo"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" rowstamp=""[0 0 0 0 0 81 106 -17]"">
      <TICKETID>1034</TICKETID>
      <CLASS>SR</CLASS>
      <DESCRIPTION>test</DESCRIPTION>
      <STATUS>NEW</STATUS>
      <STATUSDATE>2016-05-27T08:16:26-05:00</STATUSDATE>
      <REPORTEDBY>SWADMIN</REPORTEDBY>
      <REPORTDATE>2016-05-27T07:16:27-05:00</REPORTDATE>
      <ISGLOBAL>0</ISGLOBAL>
      <RELATEDTOGLOBAL>0</RELATEDTOGLOBAL>
      <SITEVISIT>0</SITEVISIT>
      <INHERITSTATUS>1</INHERITSTATUS>
      <ISKNOWNERROR>0</ISKNOWNERROR>
      <SITEID>NASP</SITEID>
      <ORGID>ITDNA</ORGID>
      <CHANGEDATE>2016-05-27T07:16:27-05:00</CHANGEDATE>
      <CHANGEBY>SWADMIN</CHANGEBY>
      <HISTORYFLAG>0</HISTORYFLAG>
      <TEMPLATE>0</TEMPLATE>
      <HASACTIVITY>0</HASACTIVITY>
      <ACTLABHRS>0.0</ACTLABHRS>
      <ACTLABCOST>0.0</ACTLABCOST>
      <DESCRIPTION_LONGDESCRIPTION>&lt;p&gt;test&lt;/p&gt;</DESCRIPTION_LONGDESCRIPTION>
      <TICKETUID>717</TICKETUID>
      <DUPFLAG>DUPLICATE</DUPFLAG>
      <LANGCODE>EN</LANGCODE>
      <ASSETFILTERBY>USERCUST</ASSETFILTERBY>
      <REPORTEDBYNAME>SWADMIN</REPORTEDBYNAME>
      <SLAAPPLIED>0</SLAAPPLIED>
      <HASLD>1</HASLD>
      <STATUSIFACE>0</STATUSIFACE>
      <CREATEWOMULTI>MULTI</CREATEWOMULTI>
      <SELFSERVSOLACCESS>1</SELFSERVSOLACCESS>
      <HASSOLUTION>0</HASSOLUTION>
      <PLUSPPOREQ>0</PLUSPPOREQ>
      <CREATEDBY>MXINTADM</CREATEDBY>
      <CREATIONDATE>2016-05-27T08:16:26-05:00</CREATIONDATE>
      <VIRTUALENV>0</VIRTUALENV>
      <OUTAGEDURATION>0.0</OUTAGEDURATION>
      <OUTAGEDURATIONEXT>00:00:00:00</OUTAGEDURATIONEXT>
      <PMSCINVALID>0</PMSCINVALID>
      <ACCUMULATESLAHOLDTIME>0</ACCUMULATESLAHOLDTIME>
      <TDFIRSTCALLRESOLUTION>0</TDFIRSTCALLRESOLUTION>
      <ITDFLOWCONTROLLED>0</ITDFLOWCONTROLLED>
    </SR>";


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


        private string SRResponse2 =
          @"<CreateMXISSRResponse xmlns=""http://www.ibm.com/maximo"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" creationDateTime=""2016-05-16T13:40:20-07:00"" transLanguage=""EN"" 
                baseLanguage=""EN"" messageID=""1463431220814699507"" maximoVersion=""7 5 20140411-2000 V7511--1"">
                  <MXISSRSet>
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
                  </MXISSRSet>
                </CreateMXISSRResponse>";


        [TestMethod]
        public void TestParseResponse() {
            var sr = MetadataProvider.Entity("sr");

            var targetResult = RestCrudConnector.ParseResult(sr, SRResponse);
            Assert.AreEqual("BEDFORD", targetResult.SiteId);
            Assert.AreEqual("2154", targetResult.UserId);
            Assert.AreEqual("4682", targetResult.Id);
            Assert.AreEqual(SRResponse, SRResponse);
        }


        [TestMethod]
        public void TestParseResponse2() {
            var sr = MetadataProvider.Entity("sr");

            var targetResult = RestCrudConnector.ParseResult(sr, SRResponse2, "MXISSR");
            Assert.AreEqual("BEDFORD", targetResult.SiteId);
            Assert.AreEqual("2154", targetResult.UserId);
            Assert.AreEqual("4682", targetResult.Id);
            Assert.AreEqual(SRResponse, SRResponse);
        }

        [TestMethod]
        public void TestParseResponseMbo() {
            var sr = MetadataProvider.Entity("sr");

            var targetResult = RestCrudConnector.ParseResult(sr, MBOSrResponse);
            Assert.AreEqual("NASP", targetResult.SiteId);
            Assert.AreEqual("1034", targetResult.UserId);
            Assert.AreEqual("717", targetResult.Id);
            Assert.AreEqual(SRResponse, SRResponse);
        }


    }
}
