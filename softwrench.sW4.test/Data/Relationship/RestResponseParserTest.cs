using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;

namespace softwrench.sW4.test.Data.Relationship {
    /// <summary>
    /// Summary description for RestEntityRepositoryTest
    /// </summary>
    [TestClass]
    public class RestResponseParserTest : BaseOtbMetadataTest {

        private static EntityMetadata _entity;
        private readonly RestResponseParser _responseParser = new RestResponseParser();

        #region ByIdResponse


        private const string ByIdResponse = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<QuerySWSRResponse xmlns=""http://www.ibm.com/maximo"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" creationDateTime=""2016-08-18T07:23:47-07:00"" transLanguage=""EN"" baseLanguage=""EN"" messageID=""1471530227442348139"" maximoVersion=""7 5 20140411-2000 V7511--1"" rsStart=""0"">
   <SWSRSet>
      <SR rowstamp=""[0 0 0 0 2 16 -10 -66]"">
         <ACTLABCOST>0.0</ACTLABCOST>
         <ACTLABHRS>0.0</ACTLABHRS>
         <AFFECTEDEMAIL>maximo57@hotmail.com</AFFECTEDEMAIL>
         <AFFECTEDPERSON>LIBERI</AFFECTEDPERSON>
         <AFFECTEDPHONE>(617) 555-9087</AFFECTEDPHONE>
         <ASSETNUM>2279</ASSETNUM>
         <CHANGEBY>SWADMIN</CHANGEBY>
         <CHANGEDATE>2016-03-10T05:41:07-07:00</CHANGEDATE>
         <CLASS>SR</CLASS>
         <COMMODITY>PC</COMMODITY>
         <COMMODITYGROUP>IT</COMMODITYGROUP>
         <CREATEWOMULTI>MULTI</CREATEWOMULTI>
         <DESCRIPTION>Request for OS upgrade to Windows XP</DESCRIPTION>
         <DESCRIPTION_LONGDESCRIPTION>User is requesting an upgrade of her OS from Windows 2000 to Windows XP.</DESCRIPTION_LONGDESCRIPTION>
         <HASACTIVITY>0</HASACTIVITY>
         <HASSOLUTION>0</HASSOLUTION>
         <HISTORYFLAG>0</HISTORYFLAG>
         <INHERITSTATUS>1</INHERITSTATUS>
         <ISGLOBAL>0</ISGLOBAL>
         <ISKNOWNERROR>0</ISKNOWNERROR>
         <LOCATION>HARDWARE</LOCATION>
         <ORGID>EAGLENA</ORGID>
         <OWNER>CALDONE</OWNER>
         <RELATEDTOGLOBAL>0</RELATEDTOGLOBAL>
         <REPORTDATE>2004-06-18T14:18:00-07:00</REPORTDATE>
         <REPORTEDBY>AMAN</REPORTEDBY>
         <REPORTEDEMAIL>aman.white@mro.com</REPORTEDEMAIL>
         <REPORTEDPHONE>781-335-0412</REPORTEDPHONE>
         <SELFSERVSOLACCESS>0</SELFSERVSOLACCESS>
         <SITEID>BEDFORD</SITEID>
         <SITEVISIT>0</SITEVISIT>
         <STATUS>QUEUED</STATUS>
         <STATUSDATE>2005-02-02T08:38:28-07:00</STATUSDATE>
         <TEMPLATE>0</TEMPLATE>
         <TICKETID>1001</TICKETID>
         <TICKETUID>2</TICKETUID>
         <WORKLOG rowstamp=""[0 0 0 0 1 20 -121 -51]"">
            <CLIENTVIEWABLE>0</CLIENTVIEWABLE>
            <CREATEBY>ADAMS</CREATEBY>
            <CREATEDATE>2014-08-11T12:01:13-07:00</CREATEDATE>
            <DESCRIPTION>test</DESCRIPTION>
            <DESCRIPTION_LONGDESCRIPTION>fsadfdfsdfsdfdsfsdfsdfdsf&lt;!-- RICH TEXT --&gt;</DESCRIPTION_LONGDESCRIPTION>
            <LOGTYPE>CLIENTNOTE</LOGTYPE>
            <MODIFYBY>MXINTADM</MODIFYBY>
            <MODIFYDATE>2015-02-09T11:42:47-07:00</MODIFYDATE>
            <ORGID>EAGLENA</ORGID>
            <RECORDKEY>1001</RECORDKEY>
            <SITEID>BEDFORD</SITEID>
            <WORKLOGID>222</WORKLOGID>
         </WORKLOG>
         <WORKLOG rowstamp=""[0 0 0 0 1 -47 30 61]"">
            <CLIENTVIEWABLE>1</CLIENTVIEWABLE>
            <CREATEBY>SWADMIN</CREATEBY>
            <CREATEDATE>2015-09-17T17:58:46-07:00</CREATEDATE>
            <DESCRIPTION>1</DESCRIPTION>
            <LOGTYPE>CLIENTNOTE</LOGTYPE>
            <MODIFYBY>MXINTADM</MODIFYBY>
            <MODIFYDATE>2015-09-17T17:58:46-07:00</MODIFYDATE>
            <ORGID>EAGLENA</ORGID>
            <RECORDKEY>1001</RECORDKEY>
            <SITEID>BEDFORD</SITEID>
            <WORKLOGID>2416</WORKLOGID>
         </WORKLOG>
         <RELATEDRECORD rowstamp=""[0 0 0 0 2 33 -58 -29]"">
            <RECORDKEY>1001</RECORDKEY>
            <RELATEDRECCLASS>SR</RELATEDRECCLASS>
            <RELATEDRECKEY>2115</RELATEDRECKEY>
            <RELATEDRECORDID>3011</RELATEDRECORDID>
            <RELATEDRECORGID>EAGLENA</RELATEDRECORGID>
            <RELATEDRECSITEID>BEDFORD</RELATEDRECSITEID>
            <RELATETYPE>RELATED</RELATETYPE>
         </RELATEDRECORD>
         <RELATEDRECORD rowstamp=""[0 0 0 0 0 12 71 -124]"">
            <ORGID>EAGLENA</ORGID>
            <RECORDKEY>1001</RECORDKEY>
            <RELATEDRECCLASS>CHANGE</RELATEDRECCLASS>
            <RELATEDRECKEY>1087</RELATEDRECKEY>
            <RELATEDRECORDID>2</RELATEDRECORDID>
            <RELATEDRECORGID>EAGLENA</RELATEDRECORGID>
            <RELATEDRECSITEID>BEDFORD</RELATEDRECSITEID>
            <RELATEDRECWOCLASS>CHANGE</RELATEDRECWOCLASS>
            <RELATEDRECWONUM>1087</RELATEDRECWONUM>
            <RELATETYPE>FOLLOWUP</RELATETYPE>
            <SITEID>BEDFORD</SITEID>
         </RELATEDRECORD>
         <DOCLINKS rowstamp=""[0 0 0 0 0 -54 124 41]"">
            <ADDINFO>0</ADDINFO>
            <CHANGEBY>MAXADMIN</CHANGEBY>
            <CHANGEDATE>2014-11-25T11:04:53-07:00</CHANGEDATE>
            <COPYLINKTOWO>0</COPYLINKTOWO>
            <CREATEBY>MAXADMIN</CREATEBY>
            <CREATEDATE>2014-11-25T11:03:02-07:00</CREATEDATE>
            <DESCRIPTION>hapagexport.zip</DESCRIPTION>
            <DOCINFOID>778</DOCINFOID>
            <DOCLINKSID>896</DOCLINKSID>
            <DOCTYPE>Attachments</DOCTYPE>
            <DOCUMENT>TEST</DOCUMENT>
            <GETLATESTVERSION>1</GETLATESTVERSION>
            <OWNERID>2</OWNERID>
            <OWNERTABLE>SR</OWNERTABLE>
            <PRINTTHRULINK>0</PRINTTHRULINK>
            <SHOW>0</SHOW>
            <UPLOAD>0</UPLOAD>
            <URLNAME>C:\DOCLINKS\ATTACHMENTS\hapagexport1416938693405.zip</URLNAME>
            <URLTYPE>FILE</URLTYPE>
            <WEBURL>http://10.50.100.125:8080/ATTACHMENTS/hapagexport1416938693405.zip</WEBURL>
         </DOCLINKS>
         <TKSERVICEADDRESS rowstamp=""[0 0 0 0 0 19 -100 -20]"">
            <ADDRESSISCHANGED>0</ADDRESSISCHANGED>
            <ORGID>EAGLENA</ORGID>
            <SITEID>BEDFORD</SITEID>
            <TKSERVICEADDRESSID>181</TKSERVICEADDRESSID>
         </TKSERVICEADDRESS>
         <MULTIASSETLOCCI rowstamp=""[0 0 0 0 1 20 -121 -126]"">
            <ASSETNUM>2279</ASSETNUM>
            <ISPRIMARY>1</ISPRIMARY>
            <LOCATION>HARDWARE</LOCATION>
            <MULTIID>4803</MULTIID>
            <ORGID>EAGLENA</ORGID>
            <PERFORMMOVETO>0</PERFORMMOVETO>
            <PROGRESS>0</PROGRESS>
            <RECORDCLASS>SR</RECORDCLASS>
            <RECORDKEY>1001</RECORDKEY>
            <REPLACEMENTSITE>BEDFORD</REPLACEMENTSITE>
            <SITEID>BEDFORD</SITEID>
            <WORKORGID>EAGLENA</WORKORGID>
            <WORKSITEID>BEDFORD</WORKSITEID>
         </MULTIASSETLOCCI>
      </SR>
   </SWSRSet>
</QuerySWSRResponse>
";
        #endregion

        #region MultipleResponse

        private const string MultipleResponse =
        @"<?xml version=""1.0"" encoding=""UTF-8""?>
        <SRMboSet xmlns = ""http://www.ibm.com/maximo"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" rsStart=""0"">
            <SR>
                <STATUS>QUEUED</STATUS>
                <TICKETUID>2</TICKETUID>
                <TICKETID>1001</TICKETID>
            </SR>
            <SR>
                <STATUS>QUEUED</STATUS>
                <TICKETUID>3</TICKETUID>
                <TICKETID>1002</TICKETID>
            </SR>
        </SRMboSet>";
        #endregion

        [TestInitialize]
        public override void Init() {
            base.Init();
            _entity = MetadataProvider.Entity("sr");
        }


        [TestMethod]
        public void TestQueryById() {
            var result = _responseParser.ConvertXmlToDatamap(_entity,ByIdResponse);
            Assert.AreEqual(result.GetStringAttribute("ticketid"),"1001");
            Assert.AreEqual(result.GetStringAttribute("status"),"QUEUED");
            Assert.IsNotNull(result.GetStringAttribute("rowstamp"));

            var attr = result.GetAttribute("WORKLOG") as IReadOnlyList<DataMap>;
            Assert.IsNotNull(attr);

            Assert.AreEqual(2,attr.Count());
            Assert.AreEqual("ADAMS",attr[0].GetAttribute("CREATEBY"));
            Assert.IsNotNull(attr[0].GetAttribute("Rowstamp"));
            Assert.AreEqual("SWADMIN",attr[1].GetAttribute("CREATEBY"));
            Assert.IsNotNull(attr[1].GetAttribute("Rowstamp"));


            var tkserviceAddr = result.GetAttribute("TKSERVICEADDRESS") as DataMap;
            Assert.IsNotNull(tkserviceAddr);

            Assert.AreEqual("EAGLENA", tkserviceAddr.GetAttribute("ORGID"));

        }

        [TestMethod]
        public void TestQueryMultiple() {
            var result = _responseParser.ConvertXmlToDatamaps(_entity, MultipleResponse);
            Assert.AreEqual(2,result.Count);
            var sr1 = result[0];
            Assert.AreEqual("QUEUED",sr1.GetAttribute("status"));
            Assert.AreEqual("2",sr1.GetAttribute("TICKETUID"));
            Assert.AreEqual("1001",sr1.GetAttribute("TICKETID"));

            var sr2 = result[1];
            Assert.AreEqual("QUEUED", sr2.GetAttribute("status"));
            Assert.AreEqual("3", sr2.GetAttribute("TICKETUID"));
            Assert.AreEqual("1002", sr2.GetAttribute("TICKETID"));


        }


        public override string GetClientName() {
            return "testrest";
        }
    }
}
