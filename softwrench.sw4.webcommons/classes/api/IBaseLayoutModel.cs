namespace softwrench.sw4.webcommons.classes.api {
    public interface IBaseLayoutModel {
        string Title { get; }
        string ClientName { get; set; }
        bool PreventPoweredBy { get; }
        string H1Header { get; set; }
        string H1HeaderStyle { get; set; }
    }
}