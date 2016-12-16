using log4net;
using softwrench.sw4.Hapag.Resources.ImacConfigs;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.Imac;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using softwrench.sw4.Hapag.Data.WS.Ism.Base;

namespace softwrench.sw4.Hapag.Data.Connector.Imac {
    public class ImacServicePlanHelper : IImacServicePlanHelper {

        private static ILog LOG = LogManager.GetLogger(typeof(ImacServicePlanHelper));

        //TODO:discover this
        private const string HamburgLocation = ISMConstants.DefaultCustomerName;
        private const string HamburgLocation2 = ISMConstants.HamburgLocation2;

        public IEnumerable<Activity> LoadFromServicePlan(string schemaid, CrudOperationData jsonObject) {
            if ("installstd".Equals(schemaid)) {
                return HandleIbmTechMatrix(jsonObject, "HLAINPCS", "HLAINPCH", "HLAINPCW");
            }
            if ("installlan".Equals(schemaid)) {
                if ("lan".Equals(jsonObject.GetAttribute("lantype"))) {
                    return HandleIbmTechMatrix(jsonObject, "HLAINPRS", "HLAINPRH", "HLAINPRW");
                }
                return HandleIbmTechMatrix(jsonObject, "HLAINPRSEP", "HLAINPRHHP", "HLAINPRWWP");
            }

            if ("installother".Equals(schemaid)) {
                var classification = jsonObject.GetAttribute("classification") ?? "";
                return classification.Equals("43290200") ? DoLoadFromFile("HLAINVPCS") : DoLoadFromFile("HLAINOTHER");
            }
            var lanType = jsonObject.GetAttribute("lantype") as string ?? "";
            var isLan = lanType.Equals("lan", StringComparison.CurrentCultureIgnoreCase);
            var isLanHostPap = lanType.Equals("lan/host/pap", StringComparison.CurrentCultureIgnoreCase);
            var assetRel = (CrudOperationData)jsonObject.GetRelationship("asset");
            if (assetRel == null) {
                throw ExceptionUtil.InvalidOperation("asset should be selected");
            }
            var assetStatus = assetRel.GetAttribute("status") as string ?? "";
            var isIdleStatus = "150 Idle".Equals(assetStatus, StringComparison.CurrentCultureIgnoreCase);
            if ("move".Equals(schemaid)) {
                var currentITC = GetCurrentITC(jsonObject);
                var fromLocation = jsonObject.GetAttribute("fromlocation");
                var toLocation = jsonObject.GetAttribute("tolocation");
                var classification = (string)(jsonObject.GetAttribute("classification") ?? "");

                var toITC = ISMConstants.AddEmailIfNeeded(jsonObject.GetAttribute("toitc") as string);
                if (toITC != null) {
                    toITC = toITC.ToUpper();
                }
                if (fromLocation.Equals(toLocation)) {
                    if (currentITC.Equals(toITC)) {
                        return HandleIbmTechMatrix(jsonObject, "HLAMVINS", "HLAMVINH", "HLAMVINW");
                    }

                    if (isIdleStatus) {
                        return HandleIbmTechMatrix(jsonObject, "HLAMVINCOS", "HLAMVINCOH", null);
                    }



                    if (classification.StartsWith("432121")) {
                        if (isLan) {
                            return HandleIbmTechMatrix(jsonObject, "HLARMPRSIO", "HLARMPRHIO", "HLARMPRWIO");
                        }
                        if ("lan/host".Equals(lanType) || isLanHostPap) {
                            return HandleIbmTechMatrix(jsonObject, "HLARMPPSIO", "HLARMPPHIO", "HLARMPPWIO");
                        }
                    }
                    return HandleIbmTechMatrix(jsonObject, "HLARMPCSIO", "HLARMPCHIO", "HLARMPCWIO");
                }

                if (currentITC.Equals(toITC)) {
                    if (isIdleStatus) {
                        return DoLoadFromFile("HLAMVOTS");
                    }
                    if (classification.StartsWith("432121")) {
                        if (isLan) {
                            return DoLoadFromFile("HLARMPRSOS");
                        }
                        if (isLanHostPap) {
                            return DoLoadFromFile("HLARMPPSOS");
                        }
                    }
                    return DoLoadFromFile("HLARMPCSOS");
                }
                if (classification.StartsWith("432121")) {
                    if (isLan) {
                        return DoLoadFromFile("HLARMPRSOO");
                    }
                    if (isLanHostPap) {
                        return DoLoadFromFile("HLARMPPSOO");
                    }
                }
                return DoLoadFromFile("HLARMPCSOO");
            }

            if ("update".Equals(schemaid)) {
                return DoLoadFromFile("HLAUPDASSD");
            }

            if ("add".Equals(schemaid)) {
                return HandleIbmTechMatrix(jsonObject, "HLAADDCOMS", "HLAADDCOMH", "HLAADDCOMW");
            }

            if ("removestd".Equals(schemaid)) {
                return HandleIbmTechMatrix(jsonObject, "HLARVPCS", "HLARVPCH", "HLARVPCW");
            }

            if ("removelan".Equals(schemaid)) {
                if (isLan) {
                    return HandleIbmTechMatrix(jsonObject, "HLARVPRS", "HLARVPRH", "HLARVPRW");
                }
                return HandleIbmTechMatrix(jsonObject, "HLARVPRSEP", "HLARVPRHHP", "HLARVPRWWP");
            }

            if ("removeother".Equals(schemaid)) {
                return DoLoadFromFile("HLARVOTHER");
            }

            if ("replacestd".Equals(schemaid)) {
                return HandleIbmTechMatrix(jsonObject, "HLARPPCS", "HLARPPCH", "HLARPPCW");
            }

            if ("replacelan".Equals(schemaid)) {
                if (isLan) {
                    return HandleIbmTechMatrix(jsonObject, "HLARPPRS", "HLARPPRH", "HLARPPRW");
                }
                return HandleIbmTechMatrix(jsonObject, "HLARPPRSEP", "HLARPPRHHP", "HLARPPRWWP");
            }

            if ("replaceother".Equals(schemaid)) {
                return DoLoadFromFile("HLARPOTHER");
            }

            return null;
        }

        private static string GetCurrentITC(CrudOperationData jsonObject) {
            var result = jsonObject.GetAttribute("asset_.primaryuser_.personid") as string;
            return result == null ? "#fakeuserjusttonotthrowexception" : ISMConstants.AddEmailIfNeeded(result).ToUpper();
            //            return SecurityFacade.CurrentUser().MaximoPersonId;
        }

        private IEnumerable<Activity> HandleIbmTechMatrix(CrudOperationData jsonObject, string notechnicianFile, string hamburgLocation, string otherLocation) {
            if ("no".Equals(jsonObject.GetAttribute("ibmtechnician"))) {
                return DoLoadFromFile(notechnicianFile);
            }
            if (IsHamburg(jsonObject)) {
                return DoLoadFromFile(hamburgLocation);
            }
            return DoLoadFromFile(otherLocation);
        }

        private static bool IsHamburg(CrudOperationData jsonObject) {
            var attribute = jsonObject.GetAttribute("fromlocation") as string;
            var hh1Location = ISMConstants.NormalizeLocation(attribute);
            return HamburgLocation.Equals(hh1Location) || HamburgLocation2.Equals(hh1Location);
        }

        public static IEnumerable<Activity> DoLoadFromFile(string path) {
            var businessMatrixPath = MetadataProvider.GlobalProperty("businessmatrixPath");
            if (businessMatrixPath != null) {
                if (!businessMatrixPath.EndsWith("\\")) {
                    businessMatrixPath = businessMatrixPath + "\\";
                }
                var fullPath = businessMatrixPath + path + ".txt";
                LOG.DebugFormat("loading business matrix out of {0}", fullPath);
                try {
                    using (var reader = new StreamReader(fullPath)) {
                        return DoParseXML(reader);
                    }
                } catch {
                    throw ExceptionUtil.InvalidOperation(
                        "unable to load business matrix out of {0}. Check if directory exists or if property businessmatrixPath is properly configured ",
                        fullPath);
                }
            }

            var resourceName = String.Format("softwrench.sw4.Hapag.Resources.ImacConfigs.{0}.txt", path);
            var hapagAssembly = typeof(ImacServicePlanHelper).Assembly;



            using (var stream = hapagAssembly.GetManifestResourceStream(resourceName)) {
                if (stream == null) {
                    throw new InvalidOperationException(resourceName + "not found for hapag config files");
                }
                using (var reader = new StreamReader(stream)) {
                    return DoParseXML(reader);
                }
            }
        }

        private static IEnumerable<Activity> DoParseXML(StreamReader reader) {
            string result = reader.ReadToEnd();
            result = "<RootActivities><Activities>" + result + "</Activities></RootActivities>";
            var serializer = new XmlSerializer(typeof(ActivityGroup));
            var activities = (ActivityGroup)serializer.Deserialize(new XmlTextReader(new StringReader(result)));
            reader.Close();
            return activities.Activity;
        }
    }
}
