using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace KingdomConfeitaria.UserControls
{
    public partial class Footer : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ConfigurarUrls();
            ConfigurarCopyright();
        }

        private void ConfigurarUrls()
        {
            if (footerLinkHome != null)
            {
                footerLinkHome.HRef = ResolveUrl("~/Default.aspx");
            }
            
            if (footerLinkReservas != null)
            {
                footerLinkReservas.HRef = ResolveUrl("~/paginas/MinhasReservas.aspx");
            }
            
            if (footerLinkDados != null)
            {
                footerLinkDados.HRef = ResolveUrl("~/paginas/MeusDados.aspx");
            }
        }

        private void ConfigurarCopyright()
        {
            if (footerCopyright != null)
            {
                footerCopyright.InnerHtml = "&copy; " + DateTime.Now.Year + " Kingdom Confeitaria. Todos os direitos reservados.";
            }
        }
    }
}

