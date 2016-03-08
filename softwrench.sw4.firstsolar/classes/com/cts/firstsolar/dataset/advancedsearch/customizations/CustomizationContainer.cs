using System.Collections.Generic;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset.advancedsearch.customizations {
    public class CustomizationContainer {
        private Dictionary<string, AdvancedSearchCustomization> customizationMap = new Dictionary<string, AdvancedSearchCustomization>();

        public CustomizationContainer() {
            AddCustomization(new Sar01Sar02Customization());
            AddCustomization(new Sar03Til08Customization());
        }

        private void AddCustomization(AdvancedSearchCustomization customization) {
            customization.FacilitiesToCustomize().ForEach(f => customizationMap.Add(f, customization));
        }

        public AdvancedSearchCustomization GetCustomization(string facility) {
            var customization = (AdvancedSearchCustomization)null;
            customizationMap.TryGetValue(facility, out customization);
            return customization;
        }
    }
}
