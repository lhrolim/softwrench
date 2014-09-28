<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845DCD8080CC91" 
    Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<script runat="server">
    
    private void Page_Load(object sender, System.EventArgs e) {
        
        var report = this.Model as softWrench.sW4.Web.Models.Report.ReportModel;

        repViewer.ProcessingMode = ProcessingMode.Local;
        if (report != null) {
            repViewer.LocalReport.ReportPath = Server.MapPath(report.ReportDefinition);
            repViewer.LocalReport.DataSources.Clear();
            repViewer.LocalReport.DataSources.Add(new ReportDataSource(report.ReportDataSourceName, report.ReportData));
            //Adding parameters to the report
            if (report.ReportParameters != null) {
                foreach (var parameter in report.ReportParameters) {
                    repViewer.LocalReport.SetParameters(new ReportParameter(parameter.Key, parameter.Value));
                }
            }
        }
        repViewer.LocalReport.Refresh();        
    }
    
</script>

<form id="reportForm" runat="server">
    <asp:ScriptManager ID="scriptManager" runat="server"></asp:ScriptManager>
    <rsweb:ReportViewer ID="repViewer" runat="server" AsyncRendering="false" Width="100%" Height="100%"></rsweb:ReportViewer>
</form>