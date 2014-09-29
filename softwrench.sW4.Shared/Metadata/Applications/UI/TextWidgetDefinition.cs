namespace softwrench.sW4.Shared.Metadata.Applications.UI
{
    public class TextWidgetDefinition : IWidgetDefinition
    {
        public string Type
        {
            get { return GetType().Name; }
        }
    }
}