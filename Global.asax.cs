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
            // Redirecionar raiz para Default.aspx se necessário
            string path = Request.Path.ToLower();
            if (path == "/" || path == "")
            {
                // Redirecionar explicitamente para Default.aspx
                Response.Redirect("~/Default.aspx", true);
                return;
            }
            
            // Bloquear acesso direto a Cadastro.aspx se não vier de um link válido
            // Permitir apenas se vier de query string ou referrer válido
            if (path.Contains("/cadastro.aspx") && Request.HttpMethod == "GET")
            {
                string referrer = Request.UrlReferrer != null ? Request.UrlReferrer.ToString().ToLower() : "";
                bool temQueryString = !string.IsNullOrEmpty(Request.QueryString["email"]) || !string.IsNullOrEmpty(Request.QueryString["telefone"]);
                
                // Se não tem query string e não veio de uma página válida, redirecionar para Default
                if (!temQueryString && (string.IsNullOrEmpty(referrer) || (!referrer.Contains("/default.aspx") && !referrer.Contains("/login.aspx"))))
                {
                    Response.Redirect("~/Default.aspx", true);
                    return;
                }
            }
            
            // Configurar encoding UTF-8 para todas as requisições
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.Charset = "UTF-8";
            Response.ContentType = "text/html; charset=utf-8";
            
            // Garantir que o Request também está em UTF-8
            Request.ContentEncoding = System.Text.Encoding.UTF8;
            
            // Proteção CSRF - Validar ViewState
            if (Request.HttpMethod == "POST" && !Request.Path.Contains(".asmx"))
            {
                // Verificar se é um postback válido
                if (Request.Form["__VIEWSTATE"] != null || Request.Form["__EVENTVALIDATION"] != null)
                {
                    // ViewState será validado automaticamente pelo ASP.NET
                    // Mas podemos adicionar validações adicionais aqui se necessário
                }
            }
            
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
            
            // Rate limiting básico - prevenir abuso
            string clientIp = GetClientIpAddress();
            if (!string.IsNullOrEmpty(clientIp))
            {
                // Implementar rate limiting se necessário
                // Por enquanto, apenas registrar para monitoramento
            }
        }
        
        private string GetClientIpAddress()
        {
            try
            {
                string ip = Request.Headers["X-Forwarded-For"];
                if (string.IsNullOrEmpty(ip))
                {
                    ip = Request.Headers["X-Real-IP"];
                }
                if (string.IsNullOrEmpty(ip))
                {
                    ip = Request.UserHostAddress;
                }
                return ip;
            }
            catch
            {
                return string.Empty;
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

