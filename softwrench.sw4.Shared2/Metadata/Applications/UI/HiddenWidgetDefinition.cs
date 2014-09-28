namespace softwrench.sW4.Shared2.Metadata.Applications.UI
{
    public class HiddenWidgetDefinition : IWidgetDefinition
    {
        public string Type
        {
            get { return GetType().Name; }
        }
    }
}