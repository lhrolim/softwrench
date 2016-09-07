using cts.commons.simpleinjector;

namespace softWrench.sW4.Dynamic {
    public class ScriptExample : ISingletonComponent {
        protected ScriptExampleCalculator Calculator;

        public ScriptExample(ScriptExampleCalculator calculator) {
            Calculator = calculator;
        }

        public virtual int Calculate() {
            return Calculator.Sum(1, 1);
        }

        public static int GetValueForEvalPage() {
            return SimpleInjectorGenericFactory.Instance.GetObject<ScriptExample>(typeof(ScriptExample)).Calculate();
        }
    }
}
