using cts.commons.simpleinjector;

namespace softWrench.sW4.Dynamic.Example {
    public class ScriptExampleCalculator : ISingletonComponent {
        public int Sum(int a, int b) {
            return a + b;
        }
    }
}
