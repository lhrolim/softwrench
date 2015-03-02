using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace softWrench.sW4.Web.UserControls {
    public partial class RichTextBox : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
            Response.Clear();
            Response.ClearHeaders();
            Response.Write(htmlPage());
            Response.End();
        }
        private string htmlPage() {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append("<html xmlns=\"http://www.w3.org/1999/xhtml\">\n");
            sb.Append("<head>\n");
            sb.Append(" <title>Sas Rich TextBox</title>\n");
            sb.Append(" <link href=\"" + ResolveUrl("~/Content/bootstrap/css/bootstrap-min.css") + "\" rel=\"stylesheet\">\n");
            sb.Append(" <style>\n");
            sb.Append("  .form-control { border: none; min-height: 214px; height: 100%; overflow: hidden; z-index: -1; border-radius: 4px; position: relative; z-index: -1;}\n");
            sb.Append(" </style>\n");
            sb.Append(" <script type=\"text/javascript\" language=\"javascript\">\n");
            sb.Append("  function binaryData(){\n");
            sb.Append("   if (typeof myAx === 'undefined') return '';\n");
            sb.Append("   else return myAx.binaryData;\n");
            sb.Append("  }\n");
            sb.Append("  function asciiData(){\n");
            sb.Append("   if (typeof myAx === 'undefined') return '';\n");
            sb.Append("   else return myAx.asciiData;\n");
            sb.Append("  }\n");
            sb.Append(" </script>\n");
            sb.Append("</head>\n");
            sb.Append("<body style=\"margin:0 0 0 0\">\n");
            sb.Append(" <object id=\"myAx\" name=\"myAx\" class=\"form-control\" classid=\"softWrench.sW4.Controls.WebForms.dll#softWrench.sW4.Controls.WebForms.RichTextBox\" ");
            sb.Append("     style=\"border-radius: 4px; border: 1px solid #ccc; box-shadow: inset 0px 1px 1px rgba(0,0,0,0.075);\"");
            sb.Append("     width=\"100%\" height=\"100%\" frameborder=\"0\" border=\"0\">Sorry! Your browser/system does not support Screenshot.</object>\n");
            sb.Append("</body>\n");
            sb.Append("</html>\n");

            return sb.ToString();
        }
    }
}