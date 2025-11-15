using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using KingdomConfeitaria.Helpers;

namespace KingdomConfeitaria.paginas
{
    public partial class Produto : BasePage
    {
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
                string produtoIdParam = Request.QueryString["id"];
                if (!string.IsNullOrEmpty(produtoIdParam))
                {
                    CarregarProduto(produtoIdParam);
                }
                // Restaurar estado dos controles
                SessionStateHelper.RestaurarEstadoPagina(this);
            }
        }

        private void CarregarProduto(string produtoIdStr)
        {
            try
            {
                int produtoId = int.Parse(produtoIdStr);
                var produtoJson = Session["Produto_" + produtoId] as string;
                
                if (string.IsNullOrEmpty(produtoJson))
                {
                    // Se não tiver na sessão, redirecionar para Default
                    Response.Redirect("../Default.aspx");
                    return;
                }

                var serializer = new JavaScriptSerializer();
                var produto = serializer.Deserialize<dynamic>(produtoJson);

                // Renderizar HTML do produto
                var html = new System.Text.StringBuilder();
                
                // Imagem
                html.AppendFormat("<img src=\"{0}\" alt=\"{1}\" class=\"produto-imagem\" />",
                    HttpUtility.HtmlAttributeEncode(produto["imagem"]?.ToString() ?? ""),
                    HttpUtility.HtmlAttributeEncode(produto["nome"]?.ToString() ?? ""));

                // Nome
                html.AppendFormat("<h2 class=\"produto-nome\">{0}</h2>",
                    HttpUtility.HtmlEncode(produto["nome"]?.ToString() ?? ""));

                // Descrição
                if (produto["descricao"] != null && !string.IsNullOrEmpty(produto["descricao"].ToString()))
                {
                    html.AppendFormat("<p class=\"produto-descricao\">{0}</p>",
                        HttpUtility.HtmlEncode(produto["descricao"].ToString()));
                }

                // Preço unitário
                decimal preco = 0;
                if (produto["preco"] != null)
                {
                    decimal.TryParse(produto["preco"].ToString(), out preco);
                }
                html.AppendFormat("<div class=\"produto-preco\">Preço Unitário: R$ {0}</div>",
                    preco.ToString("F2").Replace(".", ","));

                // Se for saco promocional, mostrar seletores
                bool ehSaco = produto["ehSaco"] != null && Convert.ToBoolean(produto["ehSaco"]);
                if (ehSaco && produto["produtosPermitidos"] != null)
                {
                    html.Append("<div class=\"saco-produtos-selector\">");
                    html.Append("<h4 style=\"font-size: 16px; margin-bottom: 15px; color: #1a4d2e;\">Selecione os produtos do saco:</h4>");
                    
                    int quantidadeSaco = produto["quantidadeSaco"] != null ? Convert.ToInt32(produto["quantidadeSaco"]) : 0;
                    html.AppendFormat("<p style=\"font-size: 13px; color: #666; margin-bottom: 15px;\">Selecione {0} produto(s) para incluir no saco promocional.</p>", quantidadeSaco);
                    
                    var produtosPermitidos = produto["produtosPermitidos"] as System.Collections.Generic.List<dynamic>;
                    if (produtosPermitidos != null)
                    {
                        for (int i = 0; i < quantidadeSaco; i++)
                        {
                            html.AppendFormat("<div class=\"mb-3\" style=\"margin-bottom: 12px;\">");
                            html.AppendFormat("<label style=\"display: block; margin-bottom: 5px; font-weight: 600; color: #333;\">Produto {0}:</label>", i + 1);
                            html.AppendFormat("<select class=\"form-control seletor-produto-saco\" data-saco-id=\"{0}\" data-indice=\"{1}\" style=\"width: 100%; padding: 8px; border: 1px solid #ddd; border-radius: 4px;\">",
                                produto["id"], i);
                            html.Append("<option value=\"\">Selecione um produto...</option>");
                            
                            foreach (var prodPermitido in produtosPermitidos)
                            {
                                decimal precoProd = 0;
                                if (prodPermitido["preco"] != null)
                                {
                                    decimal.TryParse(prodPermitido["preco"].ToString(), out precoProd);
                                }
                                html.AppendFormat("<option value=\"{0}\">{1} - R$ {2}</option>",
                                    prodPermitido["id"],
                                    HttpUtility.HtmlEncode(prodPermitido["nome"]?.ToString() ?? ""),
                                    precoProd.ToString("F2").Replace(".", ","));
                            }
                            
                            html.Append("</select>");
                            html.Append("</div>");
                        }
                    }
                    
                    html.Append("</div>");
                }

                // Seletor de quantidade
                html.Append("<div class=\"quantidade-controls\">");
                html.Append("<label style=\"display: block; margin-bottom: 8px; font-weight: 600;\">Quantidade:</label>");
                html.Append("<div style=\"display: flex; align-items: center; gap: 10px;\">");
                html.Append("<button type=\"button\" class=\"btn-quantidade\" onclick=\"diminuirQuantidade()\">-</button>");
                html.Append("<span class=\"quantidade-value\" id=\"quantidadeValue\">1</span>");
                html.Append("<button type=\"button\" class=\"btn-quantidade\" onclick=\"aumentarQuantidade()\">+</button>");
                html.Append("</div>");
                html.Append("</div>");

                // Preço total
                html.AppendFormat("<div class=\"preco-total\">Total: R$ <span id=\"valorTotal\">{0}</span></div>",
                    preco.ToString("F2").Replace(".", ","));

                // Botões de ação
                html.Append("<div class=\"btn-actions\">");
                html.Append("<button type=\"button\" class=\"btn btn-secondary\" onclick=\"voltarPagina()\">");
                html.Append("<i class=\"fas fa-times\"></i> Cancelar");
                html.Append("</button>");
                html.Append("<button type=\"button\" class=\"btn btn-primary\" onclick=\"adicionarAoCarrinho()\">");
                html.Append("<i class=\"fas fa-check\"></i> Adicionar ao Carrinho");
                html.Append("</button>");
                html.Append("</div>");

                conteudoProduto.InnerHtml = html.ToString();

                // Salvar produto JSON em hidden field para JavaScript
                var hdnProduto = new HiddenField { ID = "hdnProdutoJson", Value = produtoJson };
                form1.Controls.Add(hdnProduto);
            }
            catch
            {
                Response.Redirect("../Default.aspx");
            }
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

