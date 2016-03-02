using System.Collections.Generic;
using softWrench.sW4.Data.Search;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset.advancedsearch {
    public class FirstSolarAdvancedSearchDtoUtils {

        private const string FsLocSearchIncludeSublocAttribute = "fsincludesubloc";
        private const string FsLocSearchFacilityAttribute = "fsfacility";
        private const string FsLocSearchLocOfInterestAttribute = "fslocint";
        private const string FsLocSearchSwitchgearsAttribute = "fsswitchgear";
        private const string FsLocSearchBlockAttribute = "fsblock";
        private const string FsLocSearchPcsAttribute = "fspcs";

        /// <summary>
        /// Recovers the facility list from searchDto.
        /// </summary>
        public static List<string> GetFacilityList(SearchRequestDto searchDto) {
            var facility = searchDto.RemoveSearchParam(FsLocSearchFacilityAttribute);
            var facilityList = facility.Value as List<string>;
            var facilityString = facility.Value as string;
            if (!string.IsNullOrEmpty(facilityString)) {
                facilityList = new List<string> { facilityString };
            }
            return facilityList;
        }

        /// <summary>
        /// Recovers the include sublocations value from searchDto.
        /// </summary>
        public static bool GetIncludeSubLocations(SearchRequestDto searchDto) {
            var includeSublocSp = searchDto.RemoveSearchParam(FsLocSearchIncludeSublocAttribute);
            return "TRUE".Equals(includeSublocSp.Value);
        }

        /// <summary>
        /// Recovers the list of locations of interest from searchDto.
        /// </summary>
        public static List<string> GetLocationsOfInterest(SearchRequestDto searchDto) {
            return AttributeToList(FsLocSearchLocOfInterestAttribute, searchDto);
        }

        /// <summary>
        /// Recovers the list of switchgears from searchDto.
        /// </summary>
        public static List<string> GetSwitchgears(SearchRequestDto searchDto) {
            return AttributeToList(FsLocSearchSwitchgearsAttribute, searchDto);
        }

        /// <summary>
        /// Recovers the list of blocks from searchDto.
        /// </summary>
        public static List<string> GetBlocks(SearchRequestDto searchDto) {
            return AttributeToList(FsLocSearchBlockAttribute, searchDto);
        }

        /// <summary>
        /// Recovers the list of pcs from searchDto.
        /// </summary>
        public static List<string> GetPcsList(SearchRequestDto searchDto) {
            return AttributeToList(FsLocSearchPcsAttribute, searchDto);
        }

        private static List<string> AttributeToList(string attribute, SearchRequestDto searchDto) {
            var sp = searchDto.RemoveSearchParam(attribute);
            if (sp == null || sp.Value == null) {
                return new List<string>();
            }
            var resultList = sp.Value as List<string>;
            var resultString = sp.Value as string;
            if (resultString != null) {
                resultList = new List<string>() { resultString };
            }
            return resultList;
        }
    }
}
