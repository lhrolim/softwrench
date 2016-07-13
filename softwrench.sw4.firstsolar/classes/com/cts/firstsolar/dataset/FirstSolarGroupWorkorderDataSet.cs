using cts.commons.persistence;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset.advancedsearch;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {
    public class FirstSolarGroupWorkorderDataSet : FirstSolarWorkorderDataSet {

        public FirstSolarGroupWorkorderDataSet(ISWDBHibernateDAO swdbDao, FirstSolarAdvancedSearchHandler advancedSearchHandler) : base(swdbDao, advancedSearchHandler) {
            // empty
        }

        public override string ApplicationName() {
            return "groupworkorder";
        }
    }
}