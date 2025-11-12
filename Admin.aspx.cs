using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using KingdomConfeitaria.Models;
using KingdomConfeitaria.Services;

namespace KingdomConfeitaria
{
    public partial class Admin : System.Web.UI.Page
    {
        private DatabaseService _databaseService;
        
        // Helper para escapar strings JavaScript com caracteres especiais
        private string EscapeJavaScript(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            
            return input
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Configurar encoding UTF-8
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.Charset = "UTF-8";
            
            // Verificar se é administrador
            if (Session["IsAdmin"] == null || !(bool)Session["IsAdmin"])
            {
                Response.Redirect("Default.aspx");
                return;
            }
            
            _databaseService = new DatabaseService();
            
            // Atualizar menu
            var clienteNome = FindControl("clienteNome") as System.Web.UI.HtmlControls.HtmlGenericControl;
            var linkLogin = FindControl("linkLogin") as System.Web.UI.HtmlControls.HtmlAnchor;
            var linkMinhasReservas = FindControl("linkMinhasReservas") as System.Web.UI.HtmlControls.HtmlAnchor;
            var linkMeusDados = FindControl("linkMeusDados") as System.Web.UI.HtmlControls.HtmlAnchor;
            var linkAdmin = FindControl("linkAdmin") as System.Web.UI.HtmlControls.HtmlAnchor;
            var linkLogout = FindControl("linkLogout") as System.Web.UI.HtmlControls.HtmlAnchor;
            
            if (Session["ClienteId"] != null)
            {
                if (clienteNome != null)
                {
                    clienteNome.InnerText = "Olá, " + (Session["ClienteNome"] != null ? Session["ClienteNome"].ToString() : "");
                    clienteNome.Style["display"] = "inline";
                }
                if (linkLogin != null) linkLogin.Style["display"] = "none";
                if (linkMinhasReservas != null) linkMinhasReservas.Style["display"] = "inline";
                if (linkMeusDados != null) linkMeusDados.Style["display"] = "inline";
                if (linkAdmin != null) linkAdmin.Style["display"] = "inline";
                if (linkLogout != null) linkLogout.Style["display"] = "inline";
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

            if (!IsPostBack)
            {
                CarregarResumo();
                CarregarProdutos();
                CarregarReservas();
                CarregarListaProdutosPermitidos();
            }
        }

        private string GerarHtmlProdutosAProduzir(Dictionary<DateTime, Dictionary<int, int>> produtosAProduzirPorData, List<Produto> todosProdutos)
        {
            if (produtosAProduzirPorData.Count == 0)
            {
                return "<p class='text-muted'>Nenhum produto a produzir no momento.</p>";
            }
            
            var html = "";
            var datasOrdenadas = produtosAProduzirPorData.Keys.OrderBy(d => d).ToList();
            
            foreach (var data in datasOrdenadas)
            {
                var produtosData = produtosAProduzirPorData[data];
                html += $@"
                    <div class='mb-4'>
                        <h6 class='border-bottom pb-2'><i class='fas fa-calendar'></i> {data.ToString("dd/MM/yyyy")}</h6>
                        <ul class='list-group'>";
                
                foreach (var kvp in produtosData.OrderBy(p => todosProdutos.FirstOrDefault(pr => pr.Id == p.Key)?.Nome ?? ""))
                {
                    var produto = todosProdutos.FirstOrDefault(p => p.Id == kvp.Key);
                    if (produto != null)
                    {
                        html += $@"
                            <li class='list-group-item d-flex justify-content-between align-items-center'>
                                <span>{System.Web.HttpUtility.HtmlEncode(produto.Nome)}</span>
                                <span class='badge bg-primary rounded-pill'>{kvp.Value}</span>
                            </li>";
                    }
                }
                
                html += @"
                        </ul>
                    </div>";
            }
            
            return html;
        }

        private void CarregarResumo()
        {
            try
            {
                var clientes = _databaseService.ObterTodosClientes();
                var reservas = _databaseService.ObterTodasReservas();
                
                int totalClientes = clientes.Count;
                int totalAdmins = clientes.Count(c => c.IsAdmin);
                int totalClientesNormais = totalClientes - totalAdmins;
                
                int totalReservas = reservas.Count;
                // Obter status por nome para contagem
                var statusAberta = _databaseService.ObterStatusReservaPorNome("Aberta");
                var statusEmProducao = _databaseService.ObterStatusReservaPorNome("Em Produção");
                var statusProducaoPronta = _databaseService.ObterStatusReservaPorNome("Produção Pronta");
                var statusJaEntregue = _databaseService.ObterStatusReservaPorNome("Já Entregue");
                var statusEntregue = _databaseService.ObterStatusReservaPorNome("Entregue");
                
                int reservasAbertas = reservas.Count(r => statusAberta != null && r.StatusId == statusAberta.Id);
                int reservasEmProducao = reservas.Count(r => statusEmProducao != null && r.StatusId == statusEmProducao.Id);
                int reservasProducaoPronta = reservas.Count(r => statusProducaoPronta != null && r.StatusId == statusProducaoPronta.Id);
                int reservasEntregues = reservas.Count(r => 
                    (statusJaEntregue != null && r.StatusId == statusJaEntregue.Id) || 
                    (statusEntregue != null && r.StatusId == statusEntregue.Id));
                int reservasCanceladas = reservas.Count(r => r.Cancelado);
                
                decimal valorTotalReservas = reservas.Where(r => !r.Cancelado).Sum(r => r.ValorTotal);
                decimal valorAberto = reservas.Where(r => statusAberta != null && r.StatusId == statusAberta.Id && !r.Cancelado).Sum(r => r.ValorTotal);
                decimal valorEmProducao = reservas.Where(r => statusEmProducao != null && r.StatusId == statusEmProducao.Id && !r.Cancelado).Sum(r => r.ValorTotal);
                
                // Calcular produtos a produzir (agrupados por data de retirada)
                var produtosAProduzir = _databaseService.CalcularProdutosAProduzir();
                var produtosAProduzirPorData = new Dictionary<DateTime, Dictionary<int, int>>();
                
                // Agrupar por data de retirada
                var reservasAProduzir = reservas.Where(r => 
                    (statusAberta != null && r.StatusId == statusAberta.Id) ||
                    (statusEmProducao != null && r.StatusId == statusEmProducao.Id) ||
                    (statusProducaoPronta != null && r.StatusId == statusProducaoPronta.Id) ||
                    (_databaseService.ObterStatusReservaPorNome("Preparando Entrega") != null && r.StatusId == _databaseService.ObterStatusReservaPorNome("Preparando Entrega").Id) ||
                    (_databaseService.ObterStatusReservaPorNome("Saiu para Entrega") != null && r.StatusId == _databaseService.ObterStatusReservaPorNome("Saiu para Entrega").Id)
                ).Where(r => !r.Cancelado).ToList();
                
                var todosProdutos = _databaseService.ObterTodosProdutos();
                
                foreach (var reserva in reservasAProduzir)
                {
                    var dataRetirada = reserva.DataRetirada.Date;
                    if (!produtosAProduzirPorData.ContainsKey(dataRetirada))
                    {
                        produtosAProduzirPorData[dataRetirada] = new Dictionary<int, int>();
                    }
                    
                    foreach (var item in reserva.Itens)
                    {
                        // Adicionar produto principal
                        if (!produtosAProduzirPorData[dataRetirada].ContainsKey(item.ProdutoId))
                        {
                            produtosAProduzirPorData[dataRetirada][item.ProdutoId] = 0;
                        }
                        produtosAProduzirPorData[dataRetirada][item.ProdutoId] += item.Quantidade;
                        
                        // Adicionar produtos dentro de sacos/cestas/caixas
                        if (!string.IsNullOrEmpty(item.Produtos))
                        {
                            try
                            {
                                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                                var produtosJson = serializer.Deserialize<List<Dictionary<string, object>>>(item.Produtos);
                                
                                foreach (var produtoJson in produtosJson)
                                {
                                    int qt = Convert.ToInt32(produtoJson["qt"]);
                                    int prodId = Convert.ToInt32(produtoJson["id"]);
                                    int quantidadeTotal = qt * item.Quantidade;
                                    
                                    if (!produtosAProduzirPorData[dataRetirada].ContainsKey(prodId))
                                    {
                                        produtosAProduzirPorData[dataRetirada][prodId] = 0;
                                    }
                                    produtosAProduzirPorData[dataRetirada][prodId] += quantidadeTotal;
                                }
                            }
                            catch { }
                        }
                    }
                }
                
                string html = $@"
                    <div class='row mb-4'>
                        <div class='col-md-3'>
                            <div class='card text-white bg-primary'>
                                <div class='card-body'>
                                    <h5 class='card-title'><i class='fas fa-users'></i> Clientes</h5>
                                    <h2 class='mb-0'>{totalClientes}</h2>
                                    <small>Total cadastrados</small>
                                </div>
                            </div>
                        </div>
                        <div class='col-md-3'>
                            <div class='card text-white bg-info'>
                                <div class='card-body'>
                                    <h5 class='card-title'><i class='fas fa-user-shield'></i> Administradores</h5>
                                    <h2 class='mb-0'>{totalAdmins}</h2>
                                    <small>Usuários admin</small>
                                </div>
                            </div>
                        </div>
                        <div class='col-md-3'>
                            <div class='card text-white bg-success'>
                                <div class='card-body'>
                                    <h5 class='card-title'><i class='fas fa-clipboard-list'></i> Reservas</h5>
                                    <h2 class='mb-0'>{totalReservas}</h2>
                                    <small>Total de reservas</small>
                                </div>
                            </div>
                        </div>
                        <div class='col-md-3'>
                            <div class='card text-white bg-warning'>
                                <div class='card-body'>
                                    <h5 class='card-title'><i class='fas fa-dollar-sign'></i> Valor Total</h5>
                                    <h2 class='mb-0'>R$ {valorTotalReservas:F2}</h2>
                                    <small>Em reservas</small>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    <div class='row mb-4'>
                        <div class='col-md-6'>
                            <div class='card'>
                                <div class='card-header bg-primary text-white'>
                                    <h5 class='mb-0'><i class='fas fa-chart-pie'></i> Status das Reservas</h5>
                                </div>
                                <div class='card-body'>
                                    <ul class='list-group list-group-flush'>
                                        <li class='list-group-item d-flex justify-content-between align-items-center'>
                                            <span class='status-badge status-aberta'>Aberta</span>
                                            <span class='badge bg-primary rounded-pill'>{reservasAbertas}</span>
                                        </li>
                                        <li class='list-group-item d-flex justify-content-between align-items-center'>
                                            <span class='status-badge status-em-produção'>Em Produção</span>
                                            <span class='badge bg-primary rounded-pill'>{reservasEmProducao}</span>
                                        </li>
                                        <li class='list-group-item d-flex justify-content-between align-items-center'>
                                            <span class='status-badge status-produção-pronta'>Produção Pronta</span>
                                            <span class='badge bg-primary rounded-pill'>{reservasProducaoPronta}</span>
                                        </li>
                                        <li class='list-group-item d-flex justify-content-between align-items-center'>
                                            <span class='status-badge status-entregue'>Entregue</span>
                                            <span class='badge bg-primary rounded-pill'>{reservasEntregues}</span>
                                        </li>
                                        <li class='list-group-item d-flex justify-content-between align-items-center'>
                                            <span class='status-badge status-cancelado'>Cancelado</span>
                                            <span class='badge bg-primary rounded-pill'>{reservasCanceladas}</span>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                        <div class='col-md-6'>
                            <div class='card'>
                                <div class='card-header bg-success text-white'>
                                    <h5 class='mb-0'><i class='fas fa-money-bill-wave'></i> Valores por Status</h5>
                                </div>
                                <div class='card-body'>
                                    <ul class='list-group list-group-flush'>
                                        <li class='list-group-item d-flex justify-content-between align-items-center'>
                                            <span>Aberta</span>
                                            <strong>R$ {valorAberto:F2}</strong>
                                        </li>
                                        <li class='list-group-item d-flex justify-content-between align-items-center'>
                                            <span>Em Produção</span>
                                            <strong>R$ {valorEmProducao:F2}</strong>
                                        </li>
                                        <li class='list-group-item d-flex justify-content-between align-items-center'>
                                            <span>Total Geral</span>
                                            <strong class='text-success'>R$ {valorTotalReservas:F2}</strong>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    <div class='card'>
                        <div class='card-header bg-info text-white'>
                            <h5 class='mb-0'><i class='fas fa-info-circle'></i> Informações do Sistema</h5>
                        </div>
                        <div class='card-body'>
                            <div class='row'>
                                <div class='col-md-6'>
                                    <p><strong>Clientes Normais:</strong> {totalClientesNormais}</p>
                                    <p><strong>Administradores:</strong> {totalAdmins}</p>
                                </div>
                                <div class='col-md-6'>
                                    <p><strong>Reservas Ativas:</strong> {totalReservas - reservasCanceladas}</p>
                                    <p><strong>Reservas Canceladas:</strong> {reservasCanceladas}</p>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    <div class='card mt-4'>
                        <div class='card-header bg-warning text-dark'>
                            <h5 class='mb-0'><i class='fas fa-box'></i> Produtos a Produzir por Data de Retirada</h5>
                        </div>
                        <div class='card-body'>
                            {GerarHtmlProdutosAProduzir(produtosAProduzirPorData, todosProdutos)}
                        </div>
                    </div>";
                
                resumoContainer.InnerHtml = html;
            }
            catch (Exception ex)
            {
                resumoContainer.InnerHtml = $"<div class='alert alert-danger'>Erro ao carregar resumo: {System.Web.HttpUtility.HtmlEncode(ex.Message)}</div>";
            }
        }

        private void CarregarProdutos()
        {
            try
            {
                var produtos = _databaseService.ObterTodosProdutos();
                produtosAdminContainer.InnerHtml = "";

                if (produtos.Count == 0)
                {
                    produtosAdminContainer.InnerHtml = "<div class='alert alert-info'>Nenhum produto cadastrado ainda.</div>";
                    return;
                }

                foreach (var produto in produtos)
                {
                    string statusBadge = produto.Ativo 
                        ? "<span class='badge bg-success'>Ativo</span>" 
                        : "<span class='badge bg-secondary'>Inativo</span>";

                    string nomeEscapado = produto.Nome.Replace("\"", "\\\"").Replace("'", "\\'");
                    string descricaoEscapada = produto.Descricao.Replace("\"", "\\\"").Replace("'", "\\'");
                    string precoStr = produto.Preco.ToString("F2").Replace(",", ".");
                    string reservavelAteStr = produto.ReservavelAte.HasValue ? produto.ReservavelAte.Value.ToString("yyyy-MM-dd") : "";
                    string vendivelAteStr = produto.VendivelAte.HasValue ? produto.VendivelAte.Value.ToString("yyyy-MM-dd") : "";
                    string html = string.Format(@"
                        <div class='produto-admin-card'>
                            <div class='row'>
                                <div class='col-md-3'>
                                    <img src='{0}' alt='{1}' 
                                         class='produto-imagem-admin' 
                                         onerror='this.src=""https://via.placeholder.com/200x200?text={1}""' />
                                </div>
                                <div class='col-md-7'>
                                    <h4>{1} {2}</h4>
                                    <p class='text-muted'>{3}</p>
                                    <p>
                                        <strong>Preço:</strong> R$ {4}
                                    </p>
                                    <p><small class='text-muted'>Ordem: {5}</small></p>
                                </div>
                                <div class='col-md-2 text-end'>
                                    <button type='button' class='btn btn-primary btn-sm mb-2' 
                                        onclick='editarProduto({6}, ""{7}"", ""{8}"", {9}, ""{0}"", {5}, {10}, ""{11}"", ""{12}"", {13}, {14}, ""{15}"")'>
                                        <i class='fas fa-edit'></i> Editar
                                    </button>
                                </div>
                            </div>
                        </div>",
                        produto.ImagemUrl,
                        produto.Nome,
                        statusBadge,
                        produto.Descricao,
                        produto.Preco.ToString("F2"),
                        produto.Ordem,
                        produto.Id,
                        nomeEscapado,
                        descricaoEscapada,
                        precoStr,
                        produto.Ativo.ToString().ToLower(),
                        reservavelAteStr,
                        vendivelAteStr,
                        produto.EhSacoPromocional.ToString().ToLower(),
                        produto.QuantidadeSaco,
                        System.Web.HttpUtility.JavaScriptStringEncode(produto.Produtos ?? ""));
                    produtosAdminContainer.InnerHtml += html;
                }
            }
            catch (Exception ex)
            {
                produtosAdminContainer.InnerHtml = string.Format("<div class='alert alert-danger'>Erro ao carregar produtos: {0}</div>", ex.Message);
            }
        }

        protected void btnSalvarProduto_Click(object sender, EventArgs e)
        {
            try
            {
                int produtoId = int.Parse(hdnProdutoId.Value);
                
                // Validação no servidor
                if (string.IsNullOrWhiteSpace(txtNomeProduto.Text))
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        "alert('Por favor, preencha o nome do produto.');", true);
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(txtPreco.Text))
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        "alert('Por favor, preencha o preço do produto.');", true);
                    return;
                }
                
                decimal preco;
                if (!decimal.TryParse(txtPreco.Text, out preco))
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        "alert('Por favor, informe um preço válido.');", true);
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(txtImagemUrl.Text))
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        "alert('Por favor, preencha a URL da imagem do produto.');", true);
                    return;
                }
                
                // Validar e parsear quantidade do saco
                int quantidadeSaco = 0;
                if (chkEhSacoPromocional.Checked)
                {
                    if (!string.IsNullOrEmpty(txtQuantidadeSaco.Text))
                    {
                        if (!int.TryParse(txtQuantidadeSaco.Text, out quantidadeSaco))
                        {
                            quantidadeSaco = 0;
                        }
                    }
                }
                
                // Obter produtos permitidos apenas se for saco promocional
                string produtosPermitidos = null;
                if (chkEhSacoPromocional.Checked)
                {
                    produtosPermitidos = ObterProdutosPermitidosJson(lstProdutosPermitidos);
                }
                
                var produto = new Produto
                {
                    Id = produtoId,
                    Nome = txtNomeProduto.Text.Trim(),
                    Descricao = txtDescricao.Text.Trim(),
                    Preco = preco,
                    ImagemUrl = txtImagemUrl.Text.Trim(),
                    Ordem = int.Parse(txtOrdem.Text ?? "0"),
                    Ativo = chkAtivo.Checked,
                    EhSacoPromocional = chkEhSacoPromocional.Checked,
                    QuantidadeSaco = quantidadeSaco,
                    Produtos = produtosPermitidos,
                    ReservavelAte = !string.IsNullOrEmpty(txtReservavelAte.Text) ? DateTime.Parse(txtReservavelAte.Text) : (DateTime?)null,
                    VendivelAte = !string.IsNullOrEmpty(txtVendivelAte.Text) ? DateTime.Parse(txtVendivelAte.Text) : (DateTime?)null
                };

                _databaseService.AtualizarProduto(produto);

                CarregarProdutos();
                CarregarListaProdutosPermitidos(); // Recarregar lista para garantir que está atualizada
                Page.ClientScript.RegisterStartupScript(this.GetType(), "FecharModal", 
                    "var modal = bootstrap.Modal.getInstance(document.getElementById('modalEditarProduto')); modal.hide();", true);
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Sucesso", 
                    $"alert('{EscapeJavaScript("Produto atualizado com sucesso!")}');", true);
            }
            catch (Exception ex)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                    string.Format("alert('Erro ao salvar produto: {0}');", EscapeJavaScript(ex.Message)), true);
            }
        }

        protected void btnSalvarNovoProduto_Click(object sender, EventArgs e)
        {
            try
            {
                // Validação no servidor
                if (string.IsNullOrWhiteSpace(txtNovoNome.Text))
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        "alert('Por favor, preencha o nome do produto.');", true);
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(txtNovoPreco.Text))
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        "alert('Por favor, preencha o preço do produto.');", true);
                    return;
                }
                
                decimal preco;
                if (!decimal.TryParse(txtNovoPreco.Text, out preco))
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        "alert('Por favor, informe um preço válido.');", true);
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(txtNovaImagemUrl.Text))
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        "alert('Por favor, preencha a URL da imagem do produto.');", true);
                    return;
                }
                
                // Validar e parsear quantidade do saco
                int quantidadeSaco = 0;
                if (chkNovoEhSacoPromocional.Checked)
                {
                    if (!string.IsNullOrEmpty(txtNovaQuantidadeSaco.Text))
                    {
                        if (!int.TryParse(txtNovaQuantidadeSaco.Text, out quantidadeSaco))
                        {
                            quantidadeSaco = 0;
                        }
                    }
                }
                
                // Obter produtos permitidos apenas se for saco promocional
                string produtosPermitidos = null;
                if (chkNovoEhSacoPromocional.Checked)
                {
                    produtosPermitidos = ObterProdutosPermitidosJson(lstNovosProdutosPermitidos);
                }
                
                var novoProduto = new Produto
                {
                    Nome = txtNovoNome.Text.Trim(),
                    Descricao = txtNovaDescricao.Text.Trim(),
                    Preco = preco,
                    ImagemUrl = txtNovaImagemUrl.Text.Trim(),
                    Ativo = true,
                    Ordem = int.Parse(txtNovaOrdem.Text ?? "0"),
                    EhSacoPromocional = chkNovoEhSacoPromocional.Checked,
                    QuantidadeSaco = quantidadeSaco,
                    Produtos = produtosPermitidos,
                    ReservavelAte = !string.IsNullOrEmpty(txtNovoReservavelAte.Text) ? DateTime.Parse(txtNovoReservavelAte.Text) : (DateTime?)null,
                    VendivelAte = !string.IsNullOrEmpty(txtNovoVendivelAte.Text) ? DateTime.Parse(txtNovoVendivelAte.Text) : (DateTime?)null
                };

                _databaseService.AdicionarProduto(novoProduto);

                // Limpar campos
                txtNovoNome.Text = "";
                txtNovaDescricao.Text = "";
                txtNovoPreco.Text = "";
                txtNovaImagemUrl.Text = "";
                txtNovaOrdem.Text = "0";
                txtNovoReservavelAte.Text = "";
                txtNovoVendivelAte.Text = "";
                chkNovoEhSacoPromocional.Checked = false;
                txtNovaQuantidadeSaco.Text = "0";
                lstNovosProdutosPermitidos.ClearSelection();

                CarregarProdutos();
                CarregarListaProdutosPermitidos(); // Recarregar lista para garantir que está atualizada
                Page.ClientScript.RegisterStartupScript(this.GetType(), "FecharModal", 
                    "var modal = bootstrap.Modal.getInstance(document.getElementById('modalNovoProduto')); modal.hide();", true);
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Sucesso", 
                    $"alert('{EscapeJavaScript("Produto adicionado com sucesso!")}');", true);
            }
            catch (Exception ex)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                    string.Format("alert('Erro ao adicionar produto: {0}');", EscapeJavaScript(ex.Message)), true);
            }
        }

        private void CarregarReservas()
        {
            try
            {
                var reservas = _databaseService.ObterTodasReservas();
                reservasContainer.InnerHtml = "";

                if (reservas.Count == 0)
                {
                    reservasContainer.InnerHtml = "<div class='alert alert-info'>Nenhuma reserva encontrada.</div>";
                    return;
                }

                foreach (var reserva in reservas)
                {
                    string statusClass = "status-" + reserva.Status.ToLower();
                    string statusBadge = string.Format("<span class='status-badge {0}'>{1}</span>", statusClass, reserva.Status);
                    
                    string itensHtml = "";
                    foreach (var item in reserva.Itens)
                    {
                        itensHtml += string.Format("<li>{0} x {1} ({2}) - R$ {3:F2}</li>", 
                            item.Quantidade, item.NomeProduto, item.Tamanho, item.Subtotal);
                    }

                    string convertidoBadge = reserva.ConvertidoEmPedido 
                        ? "<span class='badge bg-success'>Convertido em Pedido</span>" 
                        : "<span class='badge bg-secondary'>Ainda não convertido</span>";
                    
                    string canceladoBadge = reserva.Cancelado 
                        ? "<span class='badge bg-danger'>Cancelado</span>" 
                        : "";

                    string previsaoHtml = reserva.PrevisaoEntrega.HasValue 
                        ? string.Format("<p><strong>Previsão de Entrega:</strong> {0:dd/MM/yyyy HH:mm}</p>", reserva.PrevisaoEntrega.Value)
                        : "<p><strong>Previsão de Entrega:</strong> Não definida</p>";

                    string observacoesEscapadas = reserva.Observacoes.Replace("\"", "\\\"").Replace("'", "\\'");
                    string previsaoValue = reserva.PrevisaoEntrega.HasValue 
                        ? reserva.PrevisaoEntrega.Value.ToString("yyyy-MM-ddTHH:mm") 
                        : "";

                    string html = string.Format(@"
                        <div class='reserva-card'>
                            <div class='d-flex justify-content-between align-items-start mb-3'>
                                <div>
                                    <h4>{0} {1} {2}</h4>
                                    <p class='mb-1'><strong>Email:</strong> {3}</p>
                                    <p class='mb-1'><strong>Telefone:</strong> {4}</p>
                                    <p class='mb-1'><strong>Data da Reserva:</strong> {5:dd/MM/yyyy HH:mm}</p>
                                    <p class='mb-1'><strong>Data de Retirada:</strong> {6:dd/MM/yyyy}</p>
                                    {7}
                                </div>
                                <div class='text-end'>
                                    {8} {9}
                                    <p class='mt-2'><strong>Valor Total:</strong> R$ {10:F2}</p>
                                </div>
                            </div>
                            <div class='mb-3'>
                                <strong>Itens:</strong>
                                <ul>
                                    {11}
                                </ul>
                            </div>
                            <div class='text-end'>
                                <button type='button' class='btn btn-primary btn-sm' 
                                    onclick='editarReserva({12}, ""{13}"", {10}, {14}, {15}, ""{16}"", ""{17}"")'>
                                    <i class='fas fa-edit'></i> Editar Reserva
                                </button>
                            </div>
                        </div>",
                        reserva.Nome,
                        statusBadge,
                        canceladoBadge,
                        reserva.Email,
                        reserva.Telefone,
                        reserva.DataReserva,
                        reserva.DataRetirada,
                        previsaoHtml,
                        convertidoBadge,
                        statusBadge,
                        reserva.ValorTotal,
                        itensHtml,
                        reserva.Id,
                        reserva.Status,
                        reserva.ConvertidoEmPedido.ToString().ToLower(),
                        reserva.Cancelado.ToString().ToLower(),
                        previsaoValue,
                        observacoesEscapadas);
                    reservasContainer.InnerHtml += html;
                }
            }
            catch (Exception ex)
            {
                reservasContainer.InnerHtml = string.Format("<div class='alert alert-danger'>Erro ao carregar reservas: {0}</div>", ex.Message);
            }
        }

        protected void btnSalvarReserva_Click(object sender, EventArgs e)
        {
            try
            {
                int reservaId = int.Parse(hdnReservaId.Value);
                var reserva = _databaseService.ObterReservaPorId(reservaId);
                
                if (reserva != null)
                {
                    reserva.Status = ddlStatus.SelectedValue;
                    reserva.ConvertidoEmPedido = chkConvertidoEmPedido.Checked;
                    reserva.Cancelado = chkCancelado.Checked;
                    reserva.Observacoes = txtObservacoesReserva.Text;
                    
                    if (!string.IsNullOrEmpty(txtValorTotal.Text))
                    {
                        reserva.ValorTotal = decimal.Parse(txtValorTotal.Text);
                    }
                    
                    if (!string.IsNullOrEmpty(txtPrevisaoEntrega.Text))
                    {
                        reserva.PrevisaoEntrega = DateTime.Parse(txtPrevisaoEntrega.Text);
                    }
                    else
                    {
                        reserva.PrevisaoEntrega = null;
                    }

                    _databaseService.AtualizarReserva(reserva);

                    CarregarReservas();
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "FecharModal", 
                        "var modal = bootstrap.Modal.getInstance(document.getElementById('modalEditarReserva')); modal.hide();", true);
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Sucesso", 
                        $"alert('{EscapeJavaScript("Reserva atualizada com sucesso!")}');", true);
                }
            }
            catch (Exception ex)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                    string.Format("alert('Erro ao salvar reserva: {0}');", EscapeJavaScript(ex.Message)), true);
            }
        }

        private void CarregarListaProdutosPermitidos()
        {
            try
            {
                var produtos = _databaseService.ObterTodosProdutos().Where(p => !p.EhSacoPromocional && p.Ativo).OrderBy(p => p.Nome).ToList();
                
                lstProdutosPermitidos.Items.Clear();
                lstNovosProdutosPermitidos.Items.Clear();
                
                foreach (var produto in produtos)
                {
                    lstProdutosPermitidos.Items.Add(new System.Web.UI.WebControls.ListItem(produto.Nome, produto.Id.ToString()));
                    lstNovosProdutosPermitidos.Items.Add(new System.Web.UI.WebControls.ListItem(produto.Nome, produto.Id.ToString()));
                }
            }
            catch
            {
                // Log do erro se necessário
            }
        }
        
        private string ObterProdutosPermitidosJson(System.Web.UI.WebControls.ListBox listBox)
        {
            if (listBox == null) return null;
            
            var produtosIds = new List<int>();
            foreach (System.Web.UI.WebControls.ListItem item in listBox.Items)
            {
                if (item.Selected)
                {
                    int produtoId;
                    if (int.TryParse(item.Value, out produtoId))
                    {
                        produtosIds.Add(produtoId);
                    }
                }
            }
            
            if (produtosIds.Count == 0) return null;
            
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return serializer.Serialize(produtosIds);
        }

    }
}

