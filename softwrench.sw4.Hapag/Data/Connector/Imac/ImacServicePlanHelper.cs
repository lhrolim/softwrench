using log4net;
using softwrench.sw4.Hapag.Resources.ImacConfigs;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Ism.Base;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.Imac;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace softwrench.sw4.Hapag.Data.Connector.Imac {
    public class ImacServicePlanHelper : IImacServicePlanHelper {

        private static readonly ILog Log = LogManager.GetLogger(typeof(ImacServicePlanHelper));

        //TODO:discover this
        private const string HamburgLocation = ISMConstants.DefaultCustomerName;
        private const string HamburgLocation2 = ISMConstants.HamburgLocation2;
        private const string HLULocation = ISMConstants.HAP1071Location;

        public IEnumerable<Activity> LoadFromServicePlan(string schemaid, CrudOperationData jsonObject) {
            if ("installstd".Equals(schemaid)) {
                return HandleIbmTechMatrix(jsonObject, "HLAINPCS", "HLAINPCH", "HLAINPCW", "HLAINPCH_HLU", "HLAINPCS_HLU");
            }
            if ("installlan".Equals(schemaid)) {
                if ("lan".Equals(jsonObject.GetAttribute("lantype"))) {
                    return HandleIbmTechMatrix(jsonObject, "HLAINPRS", "HLAINPRH", "HLAINPRW", "HLAINPRH_HLU", "HLAINPRS_HLU");
                }
                return HandleIbmTechMatrix(jsonObject, "HLAINPRSEP", "HLAINPRHHP", "HLAINPRWWP", "HLAINPRHHP_HLU", "HLAINPRSEP_HLU");
            }

            if ("installother".Equals(schemaid)) {
                var classification = jsonObject.GetAttribute("classification") ?? "";
                if (!classification.Equals("43290200")) {
                    return DoLoadFromFile("HLAINOTHER");
                }
                return HandleIbmTechMatrix(jsonObject, "HLAINVPCS", "HLAINVPCS", "HLAINVPCS", "HLAINVPCS_HLU",
                    "HLAINVPCS_HLU");
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
                var classification = jsonObject.GetUnMappedAttribute("asset_.classstructureid") ?? "";

                var toITC = ISMConstants.AddEmailIfNeeded(jsonObject.GetAttribute("toitc") as string);
                if (toITC != null) {
                    toITC = toITC.ToUpper();
                }
                if (fromLocation.Equals(toLocation)) {
                    if (currentITC.Equals(toITC)) {
                        return HandleIbmTechMatrix(jsonObject, "HLAMVINS", "HLAMVINH", "HLAMVINW", "HLAMVINH_HLU", "HLAMVINS_HLU");
                    }

                    if (isIdleStatus) {
                        return HandleIbmTechMatrix(jsonObject, "HLAMVINCOS", "HLAMVINCOH", null, "HLAMVINCOH_HLU", "HLAMVINCOS_HLU");
                    }



                    if (classification.StartsWith("432121")) {
                        if (isLan) {
                            return HandleIbmTechMatrix(jsonObject, "HLARMPRSIO", "HLARMPRHIO", "HLARMPRWIO", "HLARMPRHIO_HLU", "HLARMPRSIO_HLU");
                        }
                        if ("lan/host".Equals(lanType) || isLanHostPap) {
                            return HandleIbmTechMatrix(jsonObject, "HLARMPPSIO", "HLARMPPHIO", "HLARMPPWIO", "HLARMPPHIO_HLU", "HLARMPPSIO_HLU");
                        }
                    }
                    return HandleIbmTechMatrix(jsonObject, "HLARMPCSIO", "HLARMPCHIO", "HLARMPCWIO", "HLARMPCHIO_HLU", "HLARMPCSIO_HLU");
                }

                if (currentITC.Equals(toITC)) {
                    if (isIdleStatus) {
                        return DoLoadFromFile("HLAMVOTS");
                    }
                    if (classification.StartsWith("432121")) {
                        if (isLan) {
                            return HandleIbmTechMatrix2Cases(jsonObject, "HLARMPRSOS", "HLARMPRSOS_HLU");
                        }
                        if (isLanHostPap) {
                            return HandleIbmTechMatrix2Cases(jsonObject, "HLARMPPSOS", "HLARMPPSOS_HLU");
                        }
                    }
                    return HandleIbmTechMatrix2Cases(jsonObject, "HLARMPCSOS", "HLARMPCSOS_HLU");
                }
                if (classification.StartsWith("432121")) {
                    if (isLan) {
                        return HandleIbmTechMatrix2Cases(jsonObject, "HLARMPRSOO", "HLARMPRSOO_HLU");
                    }
                    if (isLanHostPap) {
                        return HandleIbmTechMatrix2Cases(jsonObject, "HLARMPPSOO", "HLARMPPSOO_HLU");
                    }
                }
                if (!isIdleStatus) {
                    return HandleIbmTechMatrix2Cases(jsonObject, "HLARMPCSOO", "HLARMPCSOO_HLU");
                }
                return DoLoadFromFile("HLAMVOTCOS");
            }

            if ("update".Equals(schemaid)) {
                return DoLoadFromFile("HLAUPDASSD");
            }

            if ("add".Equals(schemaid)) {
                return HandleIbmTechMatrix(jsonObject, "HLAADDCOMS", "HLAADDCOMH", "HLAADDCOMW", "HLAADDCOMH_HLU", "HLAADDCOMS");
            }

            if ("removestd".Equals(schemaid)) {
                return HandleIbmTechMatrix(jsonObject, "HLARVPCS", "HLARVPCH", "HLARVPCW", "HLARVPCH_HLU", "HLARVPCS_HLU");
            }

            if ("removelan".Equals(schemaid)) {
                if (isLan) {
                    return HandleIbmTechMatrix(jsonObject, "HLARVPRS", "HLARVPRH", "HLARVPRW", "HLARVPRH_HLU", "HLARVPRS_HLU");
                }
                return HandleIbmTechMatrix(jsonObject, "HLARVPRSEP", "HLARVPRHHP", "HLARVPRWWP", "HLARVPRHHP_HLU", "HLARVPRSEP_HLU");
            }

            if ("removeother".Equals(schemaid)) {
                return DoLoadFromFile("HLARVOTHER");
            }

            if ("replacestd".Equals(schemaid)) {
                return HandleIbmTechMatrix(jsonObject, "HLARPPCS", "HLARPPCH", "HLARPPCW", "HLARPPCH_HLU", "HLARPPCS_HLU");
            }

            if ("replacelan".Equals(schemaid)) {
                if (isLan) {
                    return HandleIbmTechMatrix(jsonObject, "HLARPPRS", "HLARPPRH", "HLARPPRW", "HLARPPRH_HLU", "HLARPPRS_HLU");
                }
                return HandleIbmTechMatrix(jsonObject, "HLARPPRSEP", "HLARPPRHHP", "HLARPPRWWP", "HLARPPRHHP_HLU", "HLARPPRSEP_HLU");
            }

            if ("replaceother".Equals(schemaid)) {
                return DoLoadFromFile("HLARPOTHER");
            }

            if ("decommission".Equals(schemaid)) {
                return DoLoadFromFile("HLADECOMM");
            }

            return null;
        }

        private static string GetCurrentITC(CrudOperationData jsonObject) {
            var result = jsonObject.GetAttribute("currentitc") as string;
            return result == null ? "#fakeuserjusttonotthrowexception" : ISMConstants.AddEmailIfNeeded(result).ToUpper();
            //            return SecurityFacade.CurrentUser().MaximoPersonId;
        }

        private IEnumerable<Activity> HandleIbmTechMatrix(CrudOperationData jsonObject, string notechnicianFileExceptHlu, string hamburgLocationTechinician,
            string techinicianExceptHamburgHlu, string hluTechinician, string hluNonTechinician) {
            if ("no".Equals(jsonObject.GetAttribute("ibmtechnician"))) {
                if (IsHlu(jsonObject)) {
                    return DoLoadFromFile(hluNonTechinician);
                }
                return DoLoadFromFile(notechnicianFileExceptHlu);
            }
            // techinician
            if (IsHamburg(jsonObject)) {
                return DoLoadFromFile(hamburgLocationTechinician);
            } if (IsHlu(jsonObject)) {
                return DoLoadFromFile(hluTechinician);
            }
            return DoLoadFromFile(techinicianExceptHamburgHlu);
        }

        private IEnumerable<Activity> HandleIbmTechMatrix2Cases(CrudOperationData jsonObject, string nonHlu, string hlu) {

            if (IsHlu(jsonObject)) {
                return DoLoadFromFile(hlu);
            }
            return DoLoadFromFile(nonHlu);
        }

        private static bool IsHamburg(CrudOperationData jsonObject) {
            var attribute = jsonObject.GetAttribute("fromlocation") as string;
            var hh1Location = ISMConstants.NormalizeLocation(attribute);
            return HamburgLocation.Equals(hh1Location) || HamburgLocation2.Equals(hh1Location);
        }

        private static bool IsHlu(CrudOperationData jsonObject) {
            var attribute = jsonObject.GetAttribute("fromlocation") as string;
            var hh1Location = ISMConstants.NormalizeLocation(attribute);
            return HLULocation.Equals(hh1Location);
        }

        public static IEnumerable<Activity> DoLoadFromFile(string path) {
            var businessMatrixPath = MetadataProvider.GlobalProperty("businessmatrixPath");
            if (businessMatrixPath != null) {
                if (!businessMatrixPath.EndsWith("\\")) {
                    businessMatrixPath = businessMatrixPath + "\\";
                }
                var fullPath = businessMatrixPath + path + ".txt";
                Log.DebugFormat("loading business matrix out of {0}", fullPath);
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
