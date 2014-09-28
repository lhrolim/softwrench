using System.ComponentModel;
using System.Runtime.CompilerServices;
using softWrench.Mobile.UI.Binding;

namespace softWrench.iOS.UI.Binding
{
    internal class ValueProvider : IValueProvider, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly bool _isHidden;
        private string _value;

        public ValueProvider(bool isHidden)
        {
            _isHidden = isHidden;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public bool IsHidden
        {
            get { return _isHidden; }
        }
    }
}