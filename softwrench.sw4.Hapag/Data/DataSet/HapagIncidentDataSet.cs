using softWrench.sW4.Data.API.Response;
using softwrench.sw4.Shared2.Data.Association;
 using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softwrench.sw4.Hapag.Security;
﻿using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;

namespace softwrench.sw4.Hapag.Data.DataSet
{
    class HapagIncidentDataSet : HapagBaseApplicationDataSet
    {
        private const string MISSING = "";
        private const string SITEID = "pluspcustomer";
        private const string COMMODITYGROUP = "#commoditygroup";
        private const string COMMODITY = "#commodity";

        private const string ItemField = "#item";
        private const string ModuleField = "#module";
        private const string SystemField = "#system";
        private const string ComponentField = "#component";
        private const string LOCATIONFIELD = "commodities_.description";
        private const string KeySeparator = "|";

        #region Filter functions

        public HapagIncidentDataSet(IHlagLocationManager locationManager, EntityRepository entityRepository, IMaximoHibernateDAO maxDao) 
            : base(locationManager, entityRepository, maxDao)
        {
        }

        public SearchRequestDto FilterComponent(AssociationPreFilterFunctionParameters parameters)
        {
            var searchDTO = parameters.BASEDto;
            var system = parameters.OriginalEntity.GetAttribute("system") as String;
            searchDTO.AppendSearchEntry("description", system + "%");
            return searchDTO;
        }

        public SearchRequestDto FilterItem(AssociationPreFilterFunctionParameters parameters)
        {
            var searchDTO = parameters.BASEDto;
            var system = parameters.OriginalEntity.GetAttribute("system") as String;
            var component = parameters.OriginalEntity.GetAttribute("component") as String;
            searchDTO.AppendSearchEntry("description", system + '-' + component + "%");
            return searchDTO;
        }

        public SearchRequestDto FilterModule(AssociationPreFilterFunctionParameters parameters)
        {
            var searchDTO = parameters.BASEDto;
            var system = parameters.OriginalEntity.GetAttribute("system") as String;
            var component = parameters.OriginalEntity.GetAttribute("component") as String;
            var item = parameters.OriginalEntity.GetAttribute("item") as String;
            searchDTO.AppendSearchEntry("description", system + '-' + component + '-' + item + "%");
            return searchDTO;
        }

        public new IEnumerable<IAssociationOption> GetHlagUserLocations(OptionFieldProviderParameters parameters)
        {
            var options = base.GetHlagUserLocations(parameters);
            var multivaluedOptions = new List<IAssociationOption>();
            var selected = new Dictionary<string, object> { { "isSelected", true } };

            foreach (var option in options)
            {
                multivaluedOptions.Add(new MultiValueAssociationOption(option.Value, option.Label, selected));
            }
            return multivaluedOptions;
        }
        public IEnumerable<IAssociationOption> GetIncidentStatus(OptionFieldProviderParameters parameters)
        {
            var options = parameters.OptionField.Options;
            var multivaluedOptions = new List<IAssociationOption>();
            var selected = new Dictionary<string, object> { { "isSelected", true } };
            var selectedStatus = new string[] { "CANCELLED", "HISTEDIT", "INPROG", "NEW", "PENDING", "REJECTED", "SLAHOLD", "QUEUED" };

            foreach (var option in options)
            {
                if (selectedStatus.Contains(option.Value))
                {
                    multivaluedOptions.Add(new MultiValueAssociationOption(option.Value, option.Label, selected));
                }
                else
                {
                    multivaluedOptions.Add(option);
                }
            }
            return multivaluedOptions;
        }

        public IEnumerable<IAssociationOption> ParseCommoditiesList(AssociationPostFilterFunctionParameters postParams)
        {
            ISet<IAssociationOption> commodity = new SortedSet<IAssociationOption>();

            foreach (var resultset in postParams.Options)
            {
                AssociationOption result = new AssociationOption(resultset.Value, resultset.Label);
                result.Value = resultset.Value;
                if (resultset.Label == null || resultset.Label.IndexOf('-') == -1)
                {
                    result.Label = MISSING;
                    //commodity.Add(result);
                }
                else
                {
                    string[] arrTemp = resultset.Label.Split('-');
                    if (postParams.Association.Target == "component")
                    {
                        result.Label = (arrTemp.Length > 2) ? arrTemp[2] : MISSING;
                        //Please Confirm Luiz
                        result.Value = result.Label;
                        commodity.Add(result);
                    }
                    else if (postParams.Association.Target == "system")
                    {
                        result.Label = arrTemp[0] + '-' + arrTemp[1];
                        //Please Confirm Luiz
                        result.Value = result.Label;
                        commodity.Add(result);
                    }
                    else if (postParams.Association.Target == "module")
                    {
                        result.Label = (arrTemp.Length > 4) ? ModuleFieldValue(arrTemp) : MISSING;
                        //Please Confirm Luiz
                        result.Value = result.Label;
                        commodity.Add(result);
                    }
                    else if (postParams.Association.Target == "item")
                    {
                        result.Label = (arrTemp.Length > 3) ? arrTemp[3] : MISSING;
                        //Please Confirm Luiz
                        result.Value = result.Label;
                        commodity.Add(result);
                    }
                }
            }

            return commodity;
        }

        /// <summary>
        /// Return the Module field Value
        /// </summary>
        /// <param name="arrTemp">The description field, with System/Component/Module/Item</param>
        /// <returns>Module Filed Value and any trailing values.</returns>
        private static string ModuleFieldValue(string[] arrTemp)
        {
            string strTemp = string.Empty;
            for (int i = 4; i < arrTemp.Length; i++)
            {
                strTemp += arrTemp[i] + '-';
            }
            return strTemp.Substring(0, strTemp.Length - 1);
        }

        #endregion Filter functions

        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request)
        {
            var dbDetail = base.GetApplicationDetail(application, user, request);
            var resultObject = dbDetail.ResultObject;

            if (application.Schema.Mode == SchemaMode.output)
            {
                HandleClosedDate(resultObject, application);
            }
            return dbDetail;
        }

        public override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto)
        {
            var dbList = base.GetList(application, searchDto);
            var resultObject = dbList.ResultObject;
            if ((application.Schema.SchemaId == "hardwarerepair"))
            {
                HandleHardwareRepairFields(resultObject);
            }
            else if (application.Schema.SchemaId == "incidentdetail")
            {
                HandleIncidentDetailsFields(resultObject);
            }
            else if (application.Schema.SchemaId == "incidentperlocation")
            {
                HandleIncidentPerLocationFields(resultObject);
            }
            return dbList;
        }

        private void HandleClosedDate(DataMap resultObject, ApplicationMetadata application)
        {
            var status = resultObject.GetAttribute("status");
            var statusDate = resultObject.GetAttribute("statusdate");
            if ("CLOSED".Equals(status))
            {
                resultObject.Attributes["#closeddate"] = statusDate;
            }
            else
            {
                resultObject.Attributes["#closeddate"] = "";
            }
        }

        public static void HandleIncidentPerLocationFields(IEnumerable<AttributeHolder> resultObject)
        {
            foreach (var attributeHolder in resultObject)
            {
                //Displaying the last 3 characters of pluspcustomer.
                try
                {
                    attributeHolder.Attributes["#currentdatetime"] = DateTime.Now;
                    var splitedSiteId = attributeHolder.Attributes[SITEID].ToString().Split('-');
                    if (splitedSiteId.Length == 3 && splitedSiteId[2].Length == 3)
                    {
                        attributeHolder.Attributes[SITEID] = splitedSiteId[2];
                    }
                    else
                    {
                        attributeHolder.Attributes[SITEID] = attributeHolder.Attributes[SITEID].ToString();
                    }

                    var description = attributeHolder.GetAttribute("commodities_.description") as String;
                    if (description == null || description.IndexOf('-') == -1)
                    {
                        attributeHolder.Attributes[COMMODITY] = MISSING;
                        attributeHolder.Attributes[COMMODITYGROUP] = MISSING;
                    }
                    else
                    {
                        string[] arrTemp = description.Split('-');
                        var commodityGroup = arrTemp[0] + '-' + arrTemp[1];
                        attributeHolder.Attributes[COMMODITYGROUP] = commodityGroup;
                        attributeHolder.Attributes[COMMODITY] = description.Substring(commodityGroup.Length + 1, description.Length - commodityGroup.Length - 1);
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                }
            }
        }

        public static void HandleIncidentDetailsFields(IEnumerable<AttributeHolder> resultObject)
        {
            foreach (var attributeHolder in resultObject)
            {
                //Displaying the last 3 chareacters of pluspcustomer.
                try
                {
                    attributeHolder.Attributes["#currentdatetime"] = DateTime.Now;
                    attributeHolder.Attributes[SITEID] = attributeHolder.Attributes[SITEID].ToString().Split('-').Last().Substring(0, 3);
                }
                catch (ArgumentOutOfRangeException)
                {
                }
            }
        }

        public static void HandleHardwareRepairFields(IEnumerable<AttributeHolder> resultObject)
        {
            foreach (var attributeHolder in resultObject)
            {
                attributeHolder.Attributes["#currentdatetime"] = DateTime.Now;
                var description = attributeHolder.GetAttribute("commodities_.description");
                if (description == null || description.ToString().IndexOf('-') == -1)
                {
                    attributeHolder.Attributes[SystemField] = MISSING;
                    attributeHolder.Attributes[ItemField] = MISSING;
                    attributeHolder.Attributes[ModuleField] = MISSING;
                    attributeHolder.Attributes[ComponentField] = MISSING;
                }
                else
                {
                    string[] arrTemp = description.ToString().Split('-');
                    attributeHolder.Attributes[SystemField] = arrTemp[0] + '-' + arrTemp[1];
                    attributeHolder.Attributes[ComponentField] = (arrTemp.Length > 2) ? arrTemp[2] : MISSING;
                    attributeHolder.Attributes[ItemField] = (arrTemp.Length > 3) ? arrTemp[3] : MISSING;
                    attributeHolder.Attributes[ModuleField] = (arrTemp.Length > 4) ? ModuleFieldValue(arrTemp) : MISSING;
                }
            }
        }

        public override string ApplicationName()
        {
            return "incident";
        }
    }
}
