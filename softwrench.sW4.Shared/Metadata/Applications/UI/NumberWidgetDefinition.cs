namespace softwrench.sW4.Shared.Metadata.Applications.UI
{
    public class NumberWidgetDefinition : IWidgetDefinition
    {
        private readonly int _decimals;
        private readonly decimal? _min;
        private readonly decimal? _max;

        public NumberWidgetDefinition(int decimals, decimal? min, decimal? max)
        {
            _decimals = decimals < 0 ? 0 : decimals;
            _min = min;
            _max = max;
        }

        public string Type
        {
            get { return GetType().Name; }
        }

        public int Decimals
        {
            get { return _decimals; }
        }

        public decimal? Min
        {
            get { return _min; }
        }

        public decimal? Max
        {
            get { return _max; }
        }
    }
}