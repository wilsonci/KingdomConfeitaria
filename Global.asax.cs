using System;
using System.Web;

namespace KingdomConfeitaria
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            // Inicializar sessão
            Session["SessionStartTime"] = DateTime.Now;
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // Configurar encoding UTF-8 para todas as requisições
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.Charset = "UTF-8";
            
            // Verificar se a sessão expirou e limpar dados de autenticação se necessário
            if (Context.Session != null && !Context.Session.IsNewSession)
            {
                // Verificar se há ClienteId mas a sessão foi recriada (timeout)
                if (Session["ClienteId"] != null && Session["SessionStartTime"] == null)
                {
                    // Sessão foi recriada mas ainda tem dados antigos - limpar
                    Session.Clear();
                }
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
        }

        protected void Application_Error(object sender, EventArgs e)
        {
        }

        protected void Session_End(object sender, EventArgs e)
        {
        }

        protected void Application_End(object sender, EventArgs e)
        {
        }
    }
}

