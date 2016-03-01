using System.Collections.Generic;
using System.Linq;
using cts.commons.simpleinjector;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset.advancedsearch.customizations {
    public abstract class AdvancedSearchCustomization : IComponent {
        public string BuildLikeParameter(string facility, string baseLike) {
            var dashNumber = GetDashNumber(baseLike);
            switch (dashNumber) {
                case 2:
                    return LocationOfInterest(facility, baseLike);
                case 3:
                    return Switchgear(facility, baseLike);
                default:
                    var tokens = baseLike.Split('-');
                    return Pcs(facility, baseLike, tokens[2], tokens[4]);
            }
        }

        protected virtual string LocationOfInterest(string facility, string baseLike) {
            return facility + baseLike;
        }

        protected virtual string Switchgear(string facility, string baseLike) {
            return facility + baseLike;
        }

        protected virtual string Pcs(string facility, string baseLike, string block, string pcs) {
            return facility + baseLike;
        }

        public abstract List<string> FacilitiesToCustomize();

        private static int GetDashNumber(string baseLike) {
            return baseLike.Count(c => c.Equals('-'));
        }
    }
}
