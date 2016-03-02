using System.Collections.Generic;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset.advancedsearch.customizations {
    public class Sar03Til08Customization : AdvancedSearchCustomization {

        private static readonly List<string> Facilities = new List<string> { "SAR03", "SAR04", "SAR05", "SAR06", "SAR07", "SAR08" };

        protected override string Switchgear(string facility, string baseLike) {
            return facility + "-%-%";
        }

        public override List<string> FacilitiesToCustomize() {
            return Facilities;
        }
    }
}
