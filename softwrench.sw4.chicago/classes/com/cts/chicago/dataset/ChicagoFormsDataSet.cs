using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Engine;

namespace softwrench.sw4.chicago.classes.com.cts.chicago.dataset {
    public class ChicagoFormsDataSet : MaximoApplicationDataSet {

        private readonly ChicagoFormsEngine _engine;

        public ChicagoFormsDataSet(ChicagoFormsEngine engine) {
            _engine = engine;
        }

        protected override IConnectorEngine Engine() {
            return _engine;
        }

        public override string ApplicationName() {
            return "form";
        }

        public override string ClientFilter() {
            return "chicago";
        }
    }
}
