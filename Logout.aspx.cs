using System;
using System.Web.UI;

namespace KingdomConfeitaria
{
    public partial class Logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Configurar encoding UTF-8
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.Charset = "UTF-8";
            
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Default.aspx");
        }
    }
}

