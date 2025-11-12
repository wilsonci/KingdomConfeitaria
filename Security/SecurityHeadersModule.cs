using System;
using System.Web;

namespace KingdomConfeitaria.Security
{
    /// <summary>
    /// Módulo HTTP para adicionar headers de segurança em todas as respostas
    /// </summary>
    public class SecurityHeadersModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PreSendRequestHeaders += OnPreSendRequestHeaders;
        }

        private void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;
            if (app?.Context?.Response != null)
            {
                HttpResponse response = app.Context.Response;

                // Remover headers que podem expor informações do servidor
                response.Headers.Remove("Server");
                response.Headers.Remove("X-Powered-By");
                response.Headers.Remove("X-AspNet-Version");
                response.Headers.Remove("X-AspNetMvc-Version");

                // Adicionar headers de segurança se não estiverem no Web.config
                if (string.IsNullOrEmpty(response.Headers["X-Frame-Options"]))
                {
                    response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                }

                if (string.IsNullOrEmpty(response.Headers["X-Content-Type-Options"]))
                {
                    response.Headers.Add("X-Content-Type-Options", "nosniff");
                }

                if (string.IsNullOrEmpty(response.Headers["X-XSS-Protection"]))
                {
                    response.Headers.Add("X-XSS-Protection", "1; mode=block");
                }

                if (string.IsNullOrEmpty(response.Headers["Referrer-Policy"]))
                {
                    response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
                }

                // Strict Transport Security (HSTS) - apenas se estiver usando HTTPS
                if (app.Context.Request.IsSecureConnection && string.IsNullOrEmpty(response.Headers["Strict-Transport-Security"]))
                {
                    response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
                }
            }
        }

        public void Dispose()
        {
            // Nada a fazer
        }
    }
}

