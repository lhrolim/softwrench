using softWrench.Mobile.Data;

namespace softWrench.iOS.UI.Eventing
{
    public class DataMapSaved
    {
        private readonly DataMap _dataMap;

        public DataMapSaved(DataMap dataMap)
        {
            _dataMap = dataMap;
        }

        public DataMap DataMap
        {
            get
            {
                return _dataMap;
            }
        }
    }
}