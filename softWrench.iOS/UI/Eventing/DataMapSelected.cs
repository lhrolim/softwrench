using softWrench.Mobile.Data;

namespace softWrench.iOS.UI.Eventing
{
    public class DataMapSelected
    {
        private readonly DataMap _dataMap;
        private readonly bool _isNew;

        public DataMapSelected(DataMap dataMap, bool isNew)
        {
            _dataMap = dataMap;
            _isNew = isNew;
        }

        public DataMap DataMap
        {
            get { return _dataMap; }
        }

        public bool IsNew
        {
            get { return _isNew; }
        }
    }
}