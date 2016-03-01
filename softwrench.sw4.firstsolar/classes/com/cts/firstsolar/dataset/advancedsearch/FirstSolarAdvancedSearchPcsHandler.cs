using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.simpleinjector;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset.advancedsearch {
    public class FirstSolarAdvancedSearchPcsHandler : ISingletonComponent {
        private readonly FirstSolarBaseLocationFinder _baseLocationFinder;

        public FirstSolarAdvancedSearchPcsHandler(FirstSolarBaseLocationFinder baseLocationFinder) {
            _baseLocationFinder = baseLocationFinder;
        }

        public List<string> GetBaseLocations(List<string> facilities, IEnumerable<string> blocks, IReadOnlyList<string> pcs) {
            var baseLikes = blocks.Select((t, i) => BuildPcsBaseLike(t, pcs[i])).ToList();
            var baseLocations = _baseLocationFinder.FindBaseLocations(facilities, baseLikes);
            var baseLocationList = new List<string>();
            baseLocations.ForEach(l => AddBaseLocation(baseLocationList, l));
            return baseLocationList;
        }

        private static string FormatBlockOrPcs(string parameter) {
            switch (parameter.Length) {
                case 1:
                    return "0" + parameter;
                case 2:
                    return char.IsNumber(parameter[1]) ? parameter : "0" + parameter;
                default:
                    return parameter;
            }
        }

        private static string BuildPcsBaseLike(string block, string pcs) {
            var baseLocationParameter = new StringBuilder("-");
            baseLocationParameter.Append("%-").Append(FormatBlockOrPcs(block));
            baseLocationParameter.Append("-%-").Append(FormatBlockOrPcs(pcs));
            return baseLocationParameter.ToString();
        }

        private static void AddBaseLocation(ICollection<string> baseLocationList, IReadOnlyDictionary<string, string> baseLocationRow) {
            var baseLocation = baseLocationRow["location"];
            if (baseLocation == null) {
                return;
            }
            baseLocationList.Add(baseLocation);
        }
    }
}
