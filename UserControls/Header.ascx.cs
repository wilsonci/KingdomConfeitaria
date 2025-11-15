using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace KingdomConfeitaria.UserControls
{
    public partial class Header : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ConfigurarUrls();
            ConfigurarMenu();
        }

        private void ConfigurarUrls()
        {
            // Configurar URLs usando ResolveUrl no code-behind
            if (linkLogo != null)
            {
                linkLogo.HRef = ResolveUrl("~/Default.aspx");
            }
            
            if (imgLogo != null)
            {
                imgLogo.Src = ResolveUrl("~/Images/logo-kingdom-confeitaria.svg");
            }
            
            if (linkHome != null)
            {
                linkHome.HRef = ResolveUrl("~/Default.aspx");
            }
            
            if (linkLogin != null)
            {
                linkLogin.HRef = ResolveUrl("~/paginas/Login.aspx");
            }
            
            if (linkMinhasReservas != null)
            {
                linkMinhasReservas.HRef = ResolveUrl("~/paginas/MinhasReservas.aspx");
            }
            
            if (linkMeusDados != null)
            {
                linkMeusDados.HRef = ResolveUrl("~/paginas/MeusDados.aspx");
            }
            
            if (linkAdmin != null)
            {
                linkAdmin.HRef = ResolveUrl("~/paginas/Admin.aspx");
            }
            
            if (linkLogout != null)
            {
                linkLogout.HRef = ResolveUrl("~/paginas/Logout.aspx");
            }
        }

        private void ConfigurarMenu()
        {
            bool estaLogado = Session["ClienteId"] != null && !Session.IsNewSession;
            
            if (estaLogado)
            {
                if (clienteNome != null)
                {
                    clienteNome.InnerText = "Olá, " + (Session["ClienteNome"] != null ? Session["ClienteNome"].ToString() : "");
                    clienteNome.Style["display"] = "block";
                }
                if (linkLogin != null) linkLogin.Style["display"] = "none";
                if (linkMinhasReservas != null) linkMinhasReservas.Style["display"] = "inline";
                if (linkMeusDados != null) linkMeusDados.Style["display"] = "inline";
                if (linkLogout != null) linkLogout.Style["display"] = "inline";
                
                bool isAdmin = Session["IsAdmin"] != null && (bool)Session["IsAdmin"];
                if (linkAdmin != null) linkAdmin.Style["display"] = isAdmin ? "inline" : "none";
            }
            else
            {
                if (clienteNome != null) clienteNome.Style["display"] = "none";
                if (linkLogin != null) linkLogin.Style["display"] = "inline";
                if (linkMinhasReservas != null) linkMinhasReservas.Style["display"] = "none";
                if (linkMeusDados != null) linkMeusDados.Style["display"] = "none";
                if (linkAdmin != null) linkAdmin.Style["display"] = "none";
                if (linkLogout != null) linkLogout.Style["display"] = "none";
            }
        }
    }
}

