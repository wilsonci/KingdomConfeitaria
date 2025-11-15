using System;
using System.Web;
using System.Web.UI;

namespace KingdomConfeitaria.Helpers
{
    /// <summary>
    /// Classe base para todas as páginas
    /// Gerencia automaticamente persistência de estado e navegação
    /// </summary>
    public class BasePage : Page
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Garantir que a sessão está inicializada
            if (Session != null && Session["SessionStartTime"] == null)
            {
                Session["SessionStartTime"] = DateTime.Now;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Restaurar estado dos controles se não for postback
            if (!IsPostBack)
            {
                SessionStateHelper.RestaurarEstadoPagina(this);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Salvar estado dos controles antes de renderizar
            // Isso garante que o estado seja salvo mesmo se houver redirecionamento
            SessionStateHelper.SalvarEstadoPagina(this);
        }

        /// <summary>
        /// Salva estado atual e navega para outra página
        /// </summary>
        protected void NavegarPara(string url, bool salvarEstado = true)
        {
            if (salvarEstado)
            {
                SessionStateHelper.SalvarEstadoPagina(this);
            }

            // Salvar página atual no histórico
            string urlAtual = Request.Path;
            NavigationHelper.SalvarPaginaAtual(HttpContext.Current, urlAtual);

            Response.Redirect(url);
        }

        /// <summary>
        /// Volta para página anterior sem salvar alterações
        /// </summary>
        protected void VoltarPagina()
        {
            string urlAnterior = NavigationHelper.ObterPaginaAnterior(HttpContext.Current);
            
            // Remover página atual do histórico
            NavigationHelper.RemoverPaginaAtual(HttpContext.Current);

            if (string.IsNullOrEmpty(urlAnterior) || urlAnterior == Request.Path)
            {
                urlAnterior = "Default.aspx";
            }

            // Ajustar caminho relativo se necessário
            if (Request.Path.Contains("/paginas/") && !urlAnterior.Contains("/paginas/") && !urlAnterior.StartsWith("../"))
            {
                urlAnterior = "../" + urlAnterior;
            }
            else if (!Request.Path.Contains("/paginas/") && urlAnterior.Contains("/paginas/"))
            {
                urlAnterior = urlAnterior.Replace("/paginas/", "paginas/");
            }

            Response.Redirect(urlAnterior);
        }

        /// <summary>
        /// Confirma ação e volta para página anterior
        /// </summary>
        protected void ConfirmarEAcao(Action acao)
        {
            try
            {
                // Executar ação
                acao?.Invoke();

                // Salvar estado antes de voltar
                SessionStateHelper.SalvarEstadoPagina(this);

                // Voltar para página anterior
                VoltarPagina();
            }
            catch (Exception ex)
            {
                // Mostrar erro
                Page.ClientScript.RegisterStartupScript(
                    this.GetType(),
                    "Erro",
                    $"alert('Erro: {ex.Message.Replace("'", "\\'")}');",
                    true
                );
            }
        }

        /// <summary>
        /// Verifica se usuário está logado, redireciona para login se não estiver
        /// </summary>
        protected bool VerificarLogin(bool redirecionarSeNaoLogado = true)
        {
            bool estaLogado = Session["ClienteId"] != null && !Session.IsNewSession;

            if (!estaLogado && redirecionarSeNaoLogado)
            {
                // Salvar página atual para retornar após login
                string urlAtual = Request.Path + Request.QueryString;
                Session["ReturnUrl"] = urlAtual;

                // Salvar estado antes de redirecionar
                SessionStateHelper.SalvarEstadoPagina(this);

                // Redirecionar para login
                string loginUrl = Request.Path.Contains("/paginas/") ? "Login.aspx" : "paginas/Login.aspx";
                Response.Redirect(loginUrl + "?returnUrl=" + HttpUtility.UrlEncode(urlAtual));
            }

            return estaLogado;
        }

        /// <summary>
        /// Redireciona para página de retorno após login
        /// </summary>
        protected void RedirecionarParaRetorno()
        {
            string returnUrl = Session["ReturnUrl"] as string;
            Session.Remove("ReturnUrl");

            if (!string.IsNullOrEmpty(returnUrl))
            {
                Response.Redirect(returnUrl);
            }
            else
            {
                Response.Redirect(Request.Path.Contains("/paginas/") ? "../Default.aspx" : "Default.aspx");
            }
        }
    }
}

