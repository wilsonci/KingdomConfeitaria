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
            // Otimizações de performance para recursos estáticos
            string path = Request.Path.ToLower();
            string extension = System.IO.Path.GetExtension(path);
            
            // Cache de recursos estáticos (imagens, CSS, JS, fonts)
            if (!string.IsNullOrEmpty(extension))
            {
                string ext = extension.ToLower();
                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".svg" || 
                    ext == ".css" || ext == ".js" || ext == ".woff" || ext == ".woff2" || ext == ".ttf" || ext == ".eot")
                {
                    // Cache de 7 dias para recursos estáticos
                    Response.Cache.SetCacheability(HttpCacheability.Public);
                    Response.Cache.SetExpires(DateTime.Now.AddDays(7));
                    Response.Cache.SetMaxAge(TimeSpan.FromDays(7));
                    Response.Cache.SetLastModified(DateTime.Now);
                    Response.Cache.SetValidUntilExpires(true);
                }
            }
            
            // Redirecionar raiz para Default.aspx se necessário
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
            
            // Só definir ContentType se ainda não foi definido
            if (string.IsNullOrEmpty(Response.ContentType) || Response.ContentType == "text/html")
            {
                Response.ContentType = "text/html; charset=utf-8";
            }
            
            // Garantir que o Request também está em UTF-8
            Request.ContentEncoding = System.Text.Encoding.UTF8;
            
            // Otimizações de cabeçalhos HTTP
            Response.Headers.Remove("X-Powered-By");
            Response.Headers.Remove("Server");
            
            // Adicionar cabeçalhos de performance
            if (Array.IndexOf(Response.Headers.AllKeys, "X-Content-Type-Options") < 0)
            {
                Response.Headers.Add("X-Content-Type-Options", "nosniff");
            }
            
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
                
                // Verificar se "Manter conectado" está ativo e se expirou (1 hora)
                if (Session["ClienteId"] != null && Session["ManterConectado"] != null)
                {
                    bool manterConectado = (bool)Session["ManterConectado"];
                    if (manterConectado && Session["SessionExpirationTime"] != null)
                    {
                        DateTime expirationTime = (DateTime)Session["SessionExpirationTime"];
                        if (DateTime.Now > expirationTime)
                        {
                            // Sessão expirou após 1 hora - limpar e redirecionar para login
                            Session.Clear();
                            // Não redirecionar aqui para evitar loops, apenas limpar a sessão
                            // O redirecionamento será feito nas páginas que verificam autenticação
                        }
                    }
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

