using System;
using System.Web.UI;
using KingdomConfeitaria.Services;

namespace KingdomConfeitaria.paginas
{
    public partial class Logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Configurar encoding UTF-8
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.Charset = "UTF-8";
            
            // Registrar log de logout antes de limpar a sessão
            string usuarioLog = LogService.ObterUsuarioAtual(Session);
            LogService.RegistrarLogout(usuarioLog, "Logout.aspx");
            
            Session.Clear();
            Session.Abandon();
            Response.Redirect("../Default.aspx");
        }
    }
}

