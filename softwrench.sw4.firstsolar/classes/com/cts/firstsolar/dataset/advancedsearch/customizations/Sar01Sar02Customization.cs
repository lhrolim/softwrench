using System.Collections.Generic;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset.advancedsearch.customizations {
    public class Sar01Sar02Customization : AdvancedSearchCustomization {

        private static readonly List<string> Facilities = new List<string> { "SAR01", "SAR02" };

        protected override string LocationOfInterest(string facility, string baseLike) {
            return facility + "-%-%";
        }

        protected override string Switchgear(string facility, string baseLike) {
            return facility + "-02";
        }

        protected override string Pcs(string facility, string baseLike, string block, string pcs) {
            return string.Format("{0}-%-{1}-{2}", facility, block, pcs);
        }

        public override List<string> FacilitiesToCustomize() {
            return Facilities;
        }
    }
}
