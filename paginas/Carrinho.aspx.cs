using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using KingdomConfeitaria.Models;
using KingdomConfeitaria.Helpers;

namespace KingdomConfeitaria.paginas
{
    public partial class Carrinho : BasePage
    {
        private List<ItemPedido> CarrinhoSession
        {
            get
            {
                if (Session["Carrinho"] == null)
                    Session["Carrinho"] = new List<ItemPedido>();
                return (List<ItemPedido>)Session["Carrinho"];
            }
            set { Session["Carrinho"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.Charset = "UTF-8";

            // Salvar página anterior
            string urlAnterior = Request.UrlReferrer?.AbsolutePath ?? "../Default.aspx";
            NavigationHelper.SalvarPaginaAtual(HttpContext.Current, urlAnterior);

            // Configurar menu
            ConfigurarMenu();

            if (!IsPostBack)
            {
                AtualizarCarrinho();
                // Restaurar estado dos controles
                SessionStateHelper.RestaurarEstadoPagina(this);
            }
            else
            {
                ProcessarPostback();
            }
        }

        private void ProcessarPostback()
        {
            string eventTarget = Request.Form["__EVENTTARGET"] ?? "";
            string eventArgument = Request.Form["__EVENTARGUMENT"] ?? "";

            if (eventTarget == "AtualizarQuantidade")
            {
                string[] args = eventArgument.Split('|');
                if (args.Length == 3)
                {
                    int produtoId = int.Parse(args[0]);
                    string tamanho = args[1];
                    int incremento = int.Parse(args[2]);
                    
                    var item = CarrinhoSession.FirstOrDefault(i => i.ProdutoId == produtoId && i.Tamanho == tamanho);
                    if (item != null)
                    {
                        item.Quantidade += incremento;
                        if (item.Quantidade <= 0)
                        {
                            CarrinhoSession.Remove(item);
                        }
                    }
                }
            }
            else if (eventTarget == "RemoverItem")
            {
                string[] args = eventArgument.Split('|');
                if (args.Length == 2)
                {
                    int produtoId = int.Parse(args[0]);
                    string tamanho = args[1];
                    var item = CarrinhoSession.FirstOrDefault(i => i.ProdutoId == produtoId && i.Tamanho == tamanho);
                    if (item != null)
                    {
                        CarrinhoSession.Remove(item);
                    }
                }
            }

            AtualizarCarrinho();
        }

        private void AtualizarCarrinho()
        {
            var html = new System.Text.StringBuilder();

            if (CarrinhoSession.Count == 0)
            {
                html.Append("<p class=\"text-muted text-center\" style=\"padding: 40px;\">Seu carrinho está vazio</p>");
                btnFazerReserva.Visible = false;
                totalPedido.InnerText = "R$ 0,00";
            }
            else
            {
                decimal total = 0;

                foreach (var item in CarrinhoSession)
                {
                    total += item.PrecoUnitario * item.Quantidade;

                    html.Append("<div class=\"item-carrinho\">");
                    html.Append("<div class=\"item-info\">");
                    html.AppendFormat("<div class=\"item-nome\">{0}</div>",
                        System.Web.HttpUtility.HtmlEncode(item.NomeProduto));
                    html.AppendFormat("<div class=\"item-detalhes\">Tamanho: {0} | Quantidade: {1} | R$ {2} cada</div>",
                        System.Web.HttpUtility.HtmlEncode(item.Tamanho),
                        item.Quantidade,
                        item.PrecoUnitario.ToString("F2").Replace(".", ","));
                    html.Append("</div>");
                    html.Append("<div class=\"item-acoes\">");
                    html.AppendFormat("<button type=\"button\" class=\"btn btn-sm btn-outline-secondary\" onclick=\"diminuirQuantidade({0}, '{1}')\">-</button>",
                        item.ProdutoId, System.Web.HttpUtility.JavaScriptStringEncode(item.Tamanho));
                    html.AppendFormat("<button type=\"button\" class=\"btn btn-sm btn-outline-secondary\" onclick=\"aumentarQuantidade({0}, '{1}')\">+</button>",
                        item.ProdutoId, System.Web.HttpUtility.JavaScriptStringEncode(item.Tamanho));
                    html.AppendFormat("<button type=\"button\" class=\"btn btn-sm btn-danger\" onclick=\"removerItem({0}, '{1}')\">Remover</button>",
                        item.ProdutoId, System.Web.HttpUtility.JavaScriptStringEncode(item.Tamanho));
                    html.Append("</div>");
                    html.Append("</div>");
                }

                totalPedido.InnerText = "R$ " + total.ToString("F2").Replace(".", ",");
                btnFazerReserva.Visible = true;
            }

            carrinhoContainer.InnerHtml = html.ToString();
        }

        protected void btnFazerReserva_Click(object sender, EventArgs e)
        {
            if (CarrinhoSession.Count == 0)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CarrinhoVazio",
                    "alert('Adicione produtos ao carrinho antes de fazer a reserva.');", true);
                return;
            }

            // Redirecionar para página de reserva
            Response.Redirect("Reserva.aspx");
        }

        private void ConfigurarMenu()
        {
            bool estaLogado = Session["ClienteId"] != null && !Session.IsNewSession;
            
            var clienteNome = FindControl("clienteNome") as System.Web.UI.HtmlControls.HtmlGenericControl;
            var linkLogin = FindControl("linkLogin") as System.Web.UI.HtmlControls.HtmlAnchor;
            var linkMinhasReservas = FindControl("linkMinhasReservas") as System.Web.UI.HtmlControls.HtmlAnchor;
            var linkMeusDados = FindControl("linkMeusDados") as System.Web.UI.HtmlControls.HtmlAnchor;
            var linkAdmin = FindControl("linkAdmin") as System.Web.UI.HtmlControls.HtmlAnchor;
            var linkLogout = FindControl("linkLogout") as System.Web.UI.HtmlControls.HtmlAnchor;
            
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

