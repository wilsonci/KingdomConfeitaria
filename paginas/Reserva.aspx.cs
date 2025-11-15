using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using KingdomConfeitaria.Models;
using KingdomConfeitaria.Services;
using KingdomConfeitaria.Helpers;
using KingdomConfeitaria.Security;

namespace KingdomConfeitaria.paginas
{
    public partial class Reserva : BasePage
    {
        private DatabaseService _databaseService;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.Charset = "UTF-8";

            // Verificar se carrinho está vazio - se estiver, redirecionar para home
            var carrinho = Session["Carrinho"] as List<ItemPedido>;
            if (carrinho == null || carrinho.Count == 0)
            {
                Response.Redirect("../Default.aspx");
                return;
            }

            // Salvar página anterior antes de carregar esta
            string urlAnterior = Request.UrlReferrer?.AbsolutePath ?? "../Default.aspx";
            NavigationHelper.SalvarPaginaAtual(HttpContext.Current, urlAnterior);

            if (!IsPostBack)
            {
                _databaseService = new DatabaseService();
                CarregarDatasRetirada();
                ConfigurarInterface();
                
                // Restaurar estado dos controles
                SessionStateHelper.RestaurarEstadoPagina(this);
            }
        }

        private void ConfigurarInterface()
        {
            bool estaLogado = Session["ClienteId"] != null && !Session.IsNewSession;

            // Carregar resumo do carrinho (sempre, mesmo se não estiver logado)
            CarregarResumoCarrinho();

            if (estaLogado)
            {
                // Preencher dados do cliente
                if (Session["ClienteNome"] != null)
                {
                    txtNome.Text = Session["ClienteNome"].ToString();
                    if (hdnNome != null) hdnNome.Value = Session["ClienteNome"].ToString();
                }
                if (Session["ClienteEmail"] != null)
                {
                    txtEmail.Text = Session["ClienteEmail"].ToString();
                    if (hdnEmail != null) hdnEmail.Value = Session["ClienteEmail"].ToString();
                }
                if (Session["ClienteTelefone"] != null)
                {
                    string telefone = Session["ClienteTelefone"].ToString();
                    txtTelefone.Text = telefone;
                    if (hdnTelefone != null) hdnTelefone.Value = telefone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
                }

                // Mostrar área de reserva
                divLoginDinamico.Visible = false;
                divDadosReserva.Visible = true;
                divDadosReserva.Style["display"] = "block"; // Forçar display block
                btnConfirmarReserva.Style["display"] = "inline-block";
                
                Page.ClientScript.RegisterStartupScript(this.GetType(), "UsuarioLogado", 
                    "document.getElementById('tituloReserva').textContent = 'Finalizar Reserva'; " +
                    "var divDados = document.getElementById('" + divDadosReserva.ClientID + "'); " +
                    "if (divDados) divDados.style.display = 'block';", true);
            }
            else
            {
                // Mostrar área de login
                divLoginDinamico.Visible = true;
                divDadosReserva.Visible = true;
                divDadosReserva.Style["display"] = "none";
                divSenhaReserva.Style["display"] = "none";
                btnConfirmarReserva.Style["display"] = "none";
                
                Page.ClientScript.RegisterStartupScript(this.GetType(), "UsuarioNaoLogado", 
                    "document.getElementById('tituloReserva').textContent = 'Login';", true);
            }
        }

        private void CarregarResumoCarrinho()
        {
            var carrinho = Session["Carrinho"] as List<ItemPedido>;
            var html = new System.Text.StringBuilder();

            if (carrinho == null || carrinho.Count == 0)
            {
                html.Append("<p class='text-muted mb-0'><i class='fas fa-shopping-cart'></i> Carrinho vazio</p>");
                resumoCarrinho.InnerHtml = html.ToString();
                totalReserva.InnerText = "R$ 0,00";
                return;
            }

            decimal total = 0;

            foreach (var item in carrinho)
            {
                decimal subtotal = item.PrecoUnitario * item.Quantidade;
                total += subtotal;

                html.Append("<div class='mb-2 p-2 border-bottom'>");
                html.AppendFormat("<div class='d-flex justify-content-between align-items-start'>");
                html.AppendFormat("<div class='flex-grow-1'>");
                html.AppendFormat("<strong>{0}</strong>", System.Web.HttpUtility.HtmlEncode(item.NomeProduto));
                html.AppendFormat("<div class='text-muted small'>Tamanho: {0} | Quantidade: {1} | R$ {2} cada</div>",
                    System.Web.HttpUtility.HtmlEncode(item.Tamanho),
                    item.Quantidade,
                    item.PrecoUnitario.ToString("F2").Replace(".", ","));
                html.Append("</div>");
                html.AppendFormat("<div class='text-end'><strong class='text-success'>R$ {0}</strong></div>",
                    subtotal.ToString("F2").Replace(".", ","));
                html.Append("</div>");
                html.Append("</div>");
            }

            resumoCarrinho.InnerHtml = html.ToString();
            totalReserva.InnerText = "R$ " + total.ToString("F2").Replace(".", ",");
        }

        private void CarregarDatasRetirada()
        {
            if (_databaseService == null)
                _databaseService = new DatabaseService();

            try
            {
                // Gerar datas disponíveis (próximos 30 dias, excluindo domingos)
                var datas = new List<DateTime>();
                var dataAtual = DateTime.Now.Date;
                var dataLimite = dataAtual.AddDays(30);
                
                for (var data = dataAtual.AddDays(1); data <= dataLimite; data = data.AddDays(1))
                {
                    // Excluir domingos (DayOfWeek.Sunday = 0)
                    if (data.DayOfWeek != DayOfWeek.Sunday)
                    {
                        datas.Add(data);
                    }
                }
                
                radioGroupDatas.Controls.Clear();

                foreach (var data in datas)
                {
                    string dataFormatada = data.ToString("dd/MM/yyyy (dddd)", new System.Globalization.CultureInfo("pt-BR"));
                    string dataValue = data.ToString("yyyy-MM-dd");
                    string radioId = "rbData_" + data.ToString("yyyyMMdd");
                    
                    var div = new System.Web.UI.HtmlControls.HtmlGenericControl("div");
                    div.Attributes["class"] = "form-check mb-2";
                    
                    var radio = new RadioButton
                    {
                        ID = radioId,
                        Text = "", // Text vazio, será exibido no label
                        GroupName = "DataRetirada",
                        CssClass = "form-check-input"
                    };
                    radio.Attributes["value"] = dataValue;
                    radio.Attributes["onchange"] = $"document.getElementById('{hdnDataRetirada.ClientID}').value = '{dataValue}';";
                    
                    var label = new Label
                    {
                        AssociatedControlID = radio.ID,
                        CssClass = "form-check-label",
                        Text = dataFormatada
                    };
                    
                    // Adicionar radio e label ao div
                    div.Controls.Add(radio);
                    div.Controls.Add(label);
                    
                    radioGroupDatas.Controls.Add(div);
                }
            }
            catch
            {
                // Log de erro
            }
        }

        protected void btnConfirmarReserva_Click(object sender, EventArgs e)
        {
            // Verificar se está logado
            if (!VerificarLogin())
            {
                return; // VerificarLogin já redireciona
            }

            if (_databaseService == null)
                _databaseService = new DatabaseService();

            var carrinho = Session["Carrinho"] as List<ItemPedido>;
            if (carrinho == null || carrinho.Count == 0)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CarrinhoVazio", 
                    "alert('Adicione produtos ao carrinho antes de fazer a reserva.');", true);
                return;
            }

            // Obter dados do cliente
            string nome = !string.IsNullOrWhiteSpace(txtNome.Text) ? txtNome.Text.Trim() : 
                         (hdnNome != null && !string.IsNullOrWhiteSpace(hdnNome.Value) ? hdnNome.Value.Trim() : 
                         (Session["ClienteNome"] != null ? Session["ClienteNome"].ToString().Trim() : ""));
            
            string email = !string.IsNullOrWhiteSpace(txtEmail.Text) ? txtEmail.Text.Trim() : 
                          (hdnEmail != null && !string.IsNullOrWhiteSpace(hdnEmail.Value) ? hdnEmail.Value.Trim() : 
                          (Session["ClienteEmail"] != null ? Session["ClienteEmail"].ToString().Trim() : ""));
            
            string telefone = !string.IsNullOrWhiteSpace(txtTelefone.Text) ? txtTelefone.Text.Trim() : 
                             (hdnTelefone != null && !string.IsNullOrWhiteSpace(hdnTelefone.Value) ? hdnTelefone.Value.Trim() : 
                             (Session["ClienteTelefone"] != null ? Session["ClienteTelefone"].ToString().Trim() : ""));

            string dataRetirada = hdnDataRetirada != null ? hdnDataRetirada.Value.Trim() : "";
            string observacoes = txtObservacoes != null ? txtObservacoes.Text.Trim() : "";

            // Validações
            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) || 
                string.IsNullOrWhiteSpace(telefone) || string.IsNullOrWhiteSpace(dataRetirada))
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CamposObrigatorios", 
                    "alert('Por favor, preencha todos os campos obrigatórios.');", true);
                return;
            }

            // Validar e converter data de retirada
            DateTime dataRetiradaParsed;
            if (!DateTime.TryParseExact(dataRetirada, "yyyy-MM-dd", 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, out dataRetiradaParsed))
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "DataInvalida", 
                    "alert('Data de retirada inválida. Por favor, selecione uma data válida.');", true);
                return;
            }

            try
            {
                int? clienteId = Session["ClienteId"] as int?;
                if (!clienteId.HasValue)
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "NaoLogado", 
                        "alert('Você precisa estar logado para fazer uma reserva.');", true);
                    return;
                }

                // Criar reserva
                var reserva = new Models.Reserva
                {
                    ClienteId = clienteId.Value,
                    DataReserva = DateTime.Now,
                    DataRetirada = dataRetiradaParsed,
                    Observacoes = observacoes,
                    StatusId = 1, // Status inicial
                    ValorTotal = carrinho.Sum(item => item.PrecoUnitario * item.Quantidade),
                    Itens = carrinho
                };

                _databaseService.SalvarReserva(reserva);
                int reservaId = reserva.Id;

                // Limpar carrinho
                Session["Carrinho"] = new List<ItemPedido>();

                // Limpar estado desta página
                SessionStateHelper.LimparEstadoPagina(this);

                // Redirecionar para home (não para MinhasReservas, pois o usuário pode voltar e não ter mais produtos)
                Response.Redirect("../Default.aspx?reservaCriada=" + reservaId);
            }
            catch (Exception ex)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                    $"alert('Erro ao criar reserva: {ex.Message}');", true);
            }
        }
    }
}

