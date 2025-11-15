using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using KingdomConfeitaria.Models;
using KingdomConfeitaria.Services;

namespace KingdomConfeitaria.paginas
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
            
            // Verificar se � administrador
            if (Session["IsAdmin"] == null || !(bool)Session["IsAdmin"])
            {
                Response.Redirect("../Default.aspx");
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
                    clienteNome.InnerText = "Ol�, " + (Session["ClienteNome"] != null ? Session["ClienteNome"].ToString() : "");
                    clienteNome.Style["display"] = "block";
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

            // Verificar se � uma requisi��o para carregar logs
            string eventTargetLogs = Request["__EVENTTARGET"];
            string eventArgumentLogs = Request["__EVENTARGUMENT"];
            
            if (eventTargetLogs == "carregarLogs" && !string.IsNullOrEmpty(eventArgumentLogs))
            {
                int dias = 7;
                if (int.TryParse(eventArgumentLogs, out int diasParsed))
                {
                    dias = diasParsed;
                }
                CarregarLogs(dias);
                return;
            }

            if (!IsPostBack)
            {
                CarregarResumo();
                CarregarProdutos();
                CarregarClientes();
                CarregarReservas();
                CarregarListaProdutosPermitidos();
                CarregarLogs();
                CarregarStatusReserva();
                CarregarDropdownStatus();
                CarregarConfiguracoes();
            }
            
            // Verificar se � uma requisi��o para editar ou excluir status
            string eventTarget = Request["__EVENTTARGET"];
            string eventArgument = Request["__EVENTARGUMENT"];
            
            if (eventTarget == "editarStatusReserva" && !string.IsNullOrEmpty(eventArgument))
            {
                int statusId = 0;
                if (int.TryParse(eventArgument, out statusId))
                {
                    EditarStatusReserva(statusId);
                }
                return;
            }
            
            if (eventTarget == "excluirStatusReserva" && !string.IsNullOrEmpty(eventArgument))
            {
                int statusId = 0;
                if (int.TryParse(eventArgument, out statusId))
                {
                    ExcluirStatusReserva(statusId);
                }
                return;
            }
            
            if (eventTarget == "excluirReserva" && !string.IsNullOrEmpty(eventArgument))
            {
                int reservaId = 0;
                if (int.TryParse(eventArgument, out reservaId))
                {
                    ExcluirReserva(reservaId);
                }
                return;
            }
            
            if (eventTarget == "carregarDadosReserva" && !string.IsNullOrEmpty(eventArgument))
            {
                int reservaId = 0;
                if (int.TryParse(eventArgument, out reservaId))
                {
                    CarregarDadosReservaParaEdicao(reservaId);
                }
                return;
            }
        }

        private string GerarHtmlProdutosAProduzir(Dictionary<DateTime, Dictionary<int, int>> produtosAProduzirPorData, List<Models.Produto> todosProdutos)
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
                var statusEmProducao = _databaseService.ObterStatusReservaPorNome("Em Produ��o");
                var statusProducaoPronta = _databaseService.ObterStatusReservaPorNome("Produ��o Pronta");
                var statusJaEntregue = _databaseService.ObterStatusReservaPorNome("J� Entregue");
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
                            <div class='card text-white bg-primary' style='cursor: pointer;' onclick='navegarParaAba(""clientes-tab"")'>
                                <div class='card-body'>
                                    <h5 class='card-title'><i class='fas fa-users'></i> Clientes</h5>
                                    <h2 class='mb-0'>{totalClientes}</h2>
                                    <small>Total cadastrados</small>
                                </div>
                            </div>
                        </div>
                        <div class='col-md-3'>
                            <div class='card text-white bg-info' style='cursor: pointer;' onclick='navegarParaAba(""clientes-tab"")'>
                                <div class='card-body'>
                                    <h5 class='card-title'><i class='fas fa-user-shield'></i> Administradores</h5>
                                    <h2 class='mb-0'>{totalAdmins}</h2>
                                    <small>Usu�rios admin</small>
                                </div>
                            </div>
                        </div>
                        <div class='col-md-3'>
                            <div class='card text-white bg-success' style='cursor: pointer;' onclick='navegarParaAba(""reservas-tab"")'>
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
                                        <li class='list-group-item d-flex justify-content-between align-items-center' style='cursor: pointer;' onclick='navegarParaAba(""reservas-tab"")'>
                                            <span class='status-badge status-aberta'>Aberta</span>
                                            <span class='badge bg-primary rounded-pill'>{reservasAbertas}</span>
                                        </li>
                                        <li class='list-group-item d-flex justify-content-between align-items-center' style='cursor: pointer;' onclick='navegarParaAba(""reservas-tab"")'>
                                            <span class='status-badge status-em-produ��o'>Em Produ��o</span>
                                            <span class='badge bg-primary rounded-pill'>{reservasEmProducao}</span>
                                        </li>
                                        <li class='list-group-item d-flex justify-content-between align-items-center' style='cursor: pointer;' onclick='navegarParaAba(""reservas-tab"")'>
                                            <span class='status-badge status-produ��o-pronta'>Produ��o Pronta</span>
                                            <span class='badge bg-primary rounded-pill'>{reservasProducaoPronta}</span>
                                        </li>
                                        <li class='list-group-item d-flex justify-content-between align-items-center' style='cursor: pointer;' onclick='navegarParaAba(""reservas-tab"")'>
                                            <span class='status-badge status-entregue'>Entregue</span>
                                            <span class='badge bg-primary rounded-pill'>{reservasEntregues}</span>
                                        </li>
                                        <li class='list-group-item d-flex justify-content-between align-items-center' style='cursor: pointer;' onclick='navegarParaAba(""reservas-tab"")'>
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
                                            <span>Em Produ��o</span>
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
                            <h5 class='mb-0'><i class='fas fa-info-circle'></i> Informa��es do Sistema</h5>
                        </div>
                        <div class='card-body'>
                            <div class='row'>
                                <div class='col-md-6'>
                                    <p><strong>Clientes Normais:</strong> <span style='cursor: pointer; color: #007bff; text-decoration: underline;' onclick='navegarParaAba(""clientes-tab"")'>{totalClientesNormais}</span></p>
                                    <p><strong>Administradores:</strong> <span style='cursor: pointer; color: #007bff; text-decoration: underline;' onclick='navegarParaAba(""clientes-tab"")'>{totalAdmins}</span></p>
                                </div>
                                <div class='col-md-6'>
                                    <p><strong>Reservas Ativas:</strong> <span style='cursor: pointer; color: #007bff; text-decoration: underline;' onclick='navegarParaAba(""reservas-tab"")'>{totalReservas - reservasCanceladas}</span></p>
                                    <p><strong>Reservas Canceladas:</strong> <span style='cursor: pointer; color: #007bff; text-decoration: underline;' onclick='navegarParaAba(""reservas-tab"")'>{reservasCanceladas}</span></p>
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

        private void CarregarClientes()
        {
            try
            {
                var clientesContainer = FindControl("clientesContainer") as System.Web.UI.HtmlControls.HtmlGenericControl;
                if (clientesContainer == null) return;

                var clientes = _databaseService.ObterTodosClientes();
                clientesContainer.InnerHtml = "";

                if (clientes.Count == 0)
                {
                    clientesContainer.InnerHtml = "<div class='alert alert-info'>Nenhum cliente cadastrado ainda.</div>";
                    return;
                }

                // Ordenar por nome
                clientes = clientes.OrderBy(c => c.Nome).ToList();

                string html = "<div class='table-responsive'><table class='table table-striped table-hover'><thead class='table-dark'><tr><th>ID</th><th>Nome</th><th>Email</th><th>Telefone</th><th>Tipo</th><th>�ltimo Acesso</th><th>Data Cadastro</th><th>A��es</th></tr></thead><tbody>";

                foreach (var cliente in clientes)
                {
                    string tipo = cliente.IsAdmin ? "<span class='badge bg-danger'>Administrador</span>" : "<span class='badge bg-primary'>Cliente</span>";
                    string ultimoAcesso = cliente.UltimoAcesso.HasValue ? cliente.UltimoAcesso.Value.ToString("dd/MM/yyyy HH:mm") : "Nunca";
                    string dataCadastro = cliente.DataCadastro.ToString("dd/MM/yyyy HH:mm");
                    string telefone = !string.IsNullOrEmpty(cliente.Telefone) ? cliente.Telefone : "-";

                    html += $@"
                        <tr>
                            <td>{cliente.Id}</td>
                            <td>{System.Web.HttpUtility.HtmlEncode(cliente.Nome)}</td>
                            <td>{System.Web.HttpUtility.HtmlEncode(cliente.Email ?? "-")}</td>
                            <td>{System.Web.HttpUtility.HtmlEncode(telefone)}</td>
                            <td>{tipo}</td>
                            <td>{ultimoAcesso}</td>
                            <td>{dataCadastro}</td>
                        </tr>";
                }

                html += "</tbody></table></div>";
                clientesContainer.InnerHtml = html;
            }
            catch (Exception ex)
            {
                var clientesContainer = FindControl("clientesContainer") as System.Web.UI.HtmlControls.HtmlGenericControl;
                if (clientesContainer != null)
                {
                    clientesContainer.InnerHtml = string.Format("<div class='alert alert-danger'>Erro ao carregar clientes: {0}</div>", System.Web.HttpUtility.HtmlEncode(ex.Message));
                }
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
                                        <strong>Pre�o:</strong> R$ {4}
                                    </p>
                                    <p><small class='text-muted'>Ordem: {5}</small></p>
                                </div>
                                <div class='col-md-2 text-end'>
                                    <button type='button' class='btn btn-info btn-sm mb-2' 
                                        onclick='mostrarDetalhesProduto({6})' data-produto-id='{6}'>
                                        <i class='fas fa-info-circle'></i> Detalhes
                                    </button>
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
                
                // Valida��o no servidor
                if (string.IsNullOrWhiteSpace(txtNomeProduto.Text))
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        "alert('Por favor, preencha o nome do produto.');", true);
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(txtPreco.Text))
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        "alert('Por favor, preencha o pre�o do produto.');", true);
                    return;
                }
                
                decimal preco;
                if (!decimal.TryParse(txtPreco.Text, out preco))
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        "alert('Por favor, informe um pre�o v�lido.');", true);
                    return;
                }
                
                // Processar upload de imagem se houver
                string imagemUrl = txtImagemUrl.Text.Trim();
                
                var fileUploadImagem = FindControl("fileUploadImagem") as FileUpload;
                if (fileUploadImagem != null && fileUploadImagem.HasFile)
                {
                    try
                    {
                        // Validar imagem
                        if (!ImageService.ValidarImagem(fileUploadImagem.PostedFile))
                        {
                            Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                                "alert('Imagem inv�lida. Verifique o formato e tamanho (m�nimo 200x200px, m�ximo 5MB).');", true);
                            return;
                        }
                        
                        // Processar upload
                        imagemUrl = ImageService.ProcessarUploadImagem(fileUploadImagem.PostedFile, produtoId);
                        
                        if (string.IsNullOrEmpty(imagemUrl))
                        {
                            Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                                "alert('Erro ao fazer upload da imagem. Tente novamente.');", true);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                            $"alert('Erro ao processar imagem: {ex.Message}');", true);
                        return;
                    }
                }
                else if (string.IsNullOrWhiteSpace(imagemUrl))
                {
                    // Se n�o tem upload nem URL, manter a URL atual do produto
                    var produtoAtual = _databaseService.ObterProdutoPorId(produtoId);
                    if (produtoAtual != null)
                    {
                        imagemUrl = produtoAtual.ImagemUrl ?? "";
                    }
                    
                    if (string.IsNullOrWhiteSpace(imagemUrl))
                    {
                        Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                            "alert('Por favor, fa�a upload de uma imagem ou informe a URL da imagem.');", true);
                        return;
                    }
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
                
                var produto = new Models.Produto
                {
                    Id = produtoId,
                    Nome = txtNomeProduto.Text.Trim(),
                    Descricao = txtDescricao.Text.Trim(),
                    Preco = preco,
                    ImagemUrl = imagemUrl,
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
                CarregarListaProdutosPermitidos(); // Recarregar lista para garantir que est� atualizada
                string scriptFecharModalProduto = @"
                    setTimeout(function() {
                        var modalElement = document.getElementById('modalEditarProduto');
                        if (modalElement) {
                            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                                var modal = bootstrap.Modal.getInstance(modalElement);
                                if (modal) {
                                    modal.hide();
                                } else {
                                    modalElement.classList.remove('show');
                                    modalElement.style.display = 'none';
                                    modalElement.setAttribute('aria-hidden', 'true');
                                    document.body.classList.remove('modal-open');
                                    var backdrop = document.querySelector('.modal-backdrop');
                                    if (backdrop) backdrop.remove();
                                }
                            } else {
                                modalElement.classList.remove('show');
                                modalElement.style.display = 'none';
                                modalElement.setAttribute('aria-hidden', 'true');
                                document.body.classList.remove('modal-open');
                                var backdrop = document.querySelector('.modal-backdrop');
                                if (backdrop) backdrop.remove();
                            }
                        }
                    }, 100);";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "FecharModal", scriptFecharModalProduto, true);
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
                // Valida��o no servidor
                if (string.IsNullOrWhiteSpace(txtNovoNome.Text))
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        "alert('Por favor, preencha o nome do produto.');", true);
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(txtNovoPreco.Text))
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        "alert('Por favor, preencha o pre�o do produto.');", true);
                    return;
                }
                
                decimal preco;
                if (!decimal.TryParse(txtNovoPreco.Text, out preco))
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        "alert('Por favor, informe um pre�o v�lido.');", true);
                    return;
                }
                
                // Processar upload de imagem se houver
                string imagemUrl = txtNovaImagemUrl.Text.Trim();
                Models.Produto novoProduto = null;
                bool produtoCriadoComUpload = false;
                
                var fileUploadNovaImagem = FindControl("fileUploadNovaImagem") as FileUpload;
                if (fileUploadNovaImagem != null && fileUploadNovaImagem.HasFile)
                {
                    try
                    {
                        // Validar imagem
                        if (!ImageService.ValidarImagem(fileUploadNovaImagem.PostedFile))
                        {
                            Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                                "alert('Imagem inv�lida. Verifique o formato e tamanho (m�nimo 200x200px, m�ximo 5MB).');", true);
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
                        
                        // Criar produto tempor�rio para obter ID (ser� atualizado depois)
                        var produtoTemp = new Models.Produto
                        {
                            Nome = txtNovoNome.Text.Trim(),
                            Descricao = txtNovaDescricao.Text.Trim(),
                            Preco = preco,
                            ImagemUrl = "",
                            Ativo = true,
                            Ordem = int.Parse(txtNovaOrdem.Text ?? "0"),
                            EhSacoPromocional = chkNovoEhSacoPromocional.Checked,
                            QuantidadeSaco = quantidadeSaco,
                            Produtos = produtosPermitidos,
                            ReservavelAte = !string.IsNullOrEmpty(txtNovoReservavelAte.Text) ? DateTime.Parse(txtNovoReservavelAte.Text) : (DateTime?)null,
                            VendivelAte = !string.IsNullOrEmpty(txtNovoVendivelAte.Text) ? DateTime.Parse(txtNovoVendivelAte.Text) : (DateTime?)null
                        };
                        
                        int produtoIdTemp = _databaseService.AdicionarProduto(produtoTemp);
                        
                        // Processar upload com o ID do produto
                        imagemUrl = ImageService.ProcessarUploadImagem(fileUploadNovaImagem.PostedFile, produtoIdTemp);
                        
                        if (string.IsNullOrEmpty(imagemUrl))
                        {
                            // Se falhar o upload, excluir produto tempor�rio
                            _databaseService.ExcluirProduto(produtoIdTemp);
                            Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                                "alert('Erro ao fazer upload da imagem. Tente novamente.');", true);
                            return;
                        }
                        
                        // Atualizar produto com a URL da imagem
                        produtoTemp.Id = produtoIdTemp;
                        produtoTemp.ImagemUrl = imagemUrl;
                        _databaseService.AtualizarProduto(produtoTemp);
                        
                        novoProduto = produtoTemp;
                        produtoCriadoComUpload = true;
                    }
                    catch (Exception ex)
                    {
                        Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                            $"alert('Erro ao processar imagem: {ex.Message}');", true);
                        return;
                    }
                }
                else if (string.IsNullOrWhiteSpace(imagemUrl))
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        "alert('Por favor, fa�a upload de uma imagem ou informe a URL da imagem.');", true);
                    return;
                }
                
                // Se o produto n�o foi criado com upload, criar normalmente
                if (!produtoCriadoComUpload)
                {
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
                    
                    // Criar novo produto normalmente
                    novoProduto = new Models.Produto
                    {
                        Nome = txtNovoNome.Text.Trim(),
                        Descricao = txtNovaDescricao.Text.Trim(),
                        Preco = preco,
                        ImagemUrl = imagemUrl,
                        Ativo = true,
                        Ordem = int.Parse(txtNovaOrdem.Text ?? "0"),
                        EhSacoPromocional = chkNovoEhSacoPromocional.Checked,
                        QuantidadeSaco = quantidadeSaco,
                        Produtos = produtosPermitidos,
                        ReservavelAte = !string.IsNullOrEmpty(txtNovoReservavelAte.Text) ? DateTime.Parse(txtNovoReservavelAte.Text) : (DateTime?)null,
                        VendivelAte = !string.IsNullOrEmpty(txtNovoVendivelAte.Text) ? DateTime.Parse(txtNovoVendivelAte.Text) : (DateTime?)null
                    };

                    _databaseService.AdicionarProduto(novoProduto);
                }

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
                CarregarListaProdutosPermitidos(); // Recarregar lista para garantir que est� atualizada
                string scriptFecharModalNovoProduto = @"
                    setTimeout(function() {
                        var modalElement = document.getElementById('modalNovoProduto');
                        if (modalElement) {
                            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                                var modal = bootstrap.Modal.getInstance(modalElement);
                                if (modal) {
                                    modal.hide();
                                } else {
                                    modalElement.classList.remove('show');
                                    modalElement.style.display = 'none';
                                    modalElement.setAttribute('aria-hidden', 'true');
                                    document.body.classList.remove('modal-open');
                                    var backdrop = document.querySelector('.modal-backdrop');
                                    if (backdrop) backdrop.remove();
                                }
                            } else {
                                modalElement.classList.remove('show');
                                modalElement.style.display = 'none';
                                modalElement.setAttribute('aria-hidden', 'true');
                                document.body.classList.remove('modal-open');
                                var backdrop = document.querySelector('.modal-backdrop');
                                if (backdrop) backdrop.remove();
                            }
                        }
                    }, 100);";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "FecharModal", scriptFecharModalNovoProduto, true);
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Sucesso", 
                    $"alert('{EscapeJavaScript("Produto adicionado com sucesso!")}');", true);
            }
            catch (Exception ex)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                    string.Format("alert('Erro ao adicionar produto: {0}');", EscapeJavaScript(ex.Message)), true);
            }
        }

        private void CarregarDropdownStatus()
        {
            try
            {
                var statusList = _databaseService.ObterTodosStatusReserva();
                ddlStatus.Items.Clear();
                foreach (var status in statusList)
                {
                    ddlStatus.Items.Add(new ListItem(status.Nome, status.Id.ToString()));
                }
            }
            catch
            {
                // Se houver erro, manter os valores padr�o ou adicionar valores b�sicos
                if (ddlStatus.Items.Count == 0)
                {
                    ddlStatus.Items.Add(new ListItem("Aberta", "1"));
                }
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
                        : "<span class='badge bg-secondary'>Ainda n�o convertido</span>";
                    
                    string canceladoBadge = reserva.Cancelado 
                        ? "<span class='badge bg-danger'>Cancelado</span>" 
                        : "";

                    string previsaoHtml = reserva.PrevisaoEntrega.HasValue 
                        ? string.Format("<p><strong>Previs�o de Entrega:</strong> {0:dd/MM/yyyy HH:mm}</p>", reserva.PrevisaoEntrega.Value)
                        : "<p><strong>Previs�o de Entrega:</strong> N�o definida</p>";

                    string observacoesEscapadas = reserva.Observacoes.Replace("\"", "\\\"").Replace("'", "\\'");
                    string previsaoValue = reserva.PrevisaoEntrega.HasValue 
                        ? reserva.PrevisaoEntrega.Value.ToString("yyyy-MM-ddTHH:mm") 
                        : "";
                    
                    string nomeEscapado = EscapeJavaScript(reserva.Nome);
                    string botaoExcluir = reserva.Cancelado 
                        ? $@"<button type='button' class='btn btn-danger btn-sm ms-2' 
                                    onclick='excluirReserva({reserva.Id}, ""{nomeEscapado}"")'>
                                    <i class='fas fa-trash'></i> Excluir Reserva
                                </button>" 
                        : "";

                    string statusIdValue = reserva.StatusId.HasValue ? reserva.StatusId.Value.ToString() : "0";
                    string valorTotalStr = reserva.ValorTotal.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                    
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
                                <button type='button' class='btn btn-info btn-sm me-2' 
                                    onclick='mostrarDetalhesReservaAdmin({12})' data-reserva-id='{12}'>
                                    <i class='fas fa-info-circle'></i> Detalhes
                                </button>
                                <button type='button' class='btn btn-primary btn-sm' 
                                    onclick='carregarDadosReserva({12})'>
                                    <i class='fas fa-edit'></i> Editar Reserva
                                </button>
                                {20}
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
                        observacoesEscapadas,
                        statusIdValue,
                        valorTotalStr,
                        botaoExcluir);
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
                // Validar se hdnReservaId tem valor
                if (string.IsNullOrEmpty(hdnReservaId.Value) || !int.TryParse(hdnReservaId.Value, out int reservaId) || reservaId <= 0)
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        $"alert('{EscapeJavaScript("Erro: ID da reserva n�o encontrado.")}');", true);
                    return;
                }
                
                var reserva = _databaseService.ObterReservaPorId(reservaId);
                
                if (reserva == null)
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        $"alert('{EscapeJavaScript("Erro: Reserva n�o encontrada.")}');", true);
                    return;
                }
                
                // Obter StatusId do dropdown selecionado
                int statusId = 0;
                if (!string.IsNullOrEmpty(ddlStatus.SelectedValue) && int.TryParse(ddlStatus.SelectedValue, out statusId) && statusId > 0)
                {
                    reserva.StatusId = statusId;
                }
                else if (reserva.StatusId.HasValue)
                {
                    // Manter o StatusId atual se n�o foi selecionado um novo
                    statusId = reserva.StatusId.Value;
                }
                else
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        $"alert('{EscapeJavaScript("Erro: Status n�o selecionado.")}');", true);
                    return;
                }
                
                // Atualizar Status (nome) baseado no StatusId
                if (reserva.StatusId.HasValue)
                {
                    var statusReserva = _databaseService.ObterStatusReservaPorId(reserva.StatusId.Value);
                    if (statusReserva != null)
                    {
                        reserva.Status = statusReserva.Nome;
                    }
                }
                
                reserva.ConvertidoEmPedido = chkConvertidoEmPedido.Checked;
                reserva.Cancelado = chkCancelado.Checked;
                reserva.Observacoes = txtObservacoesReserva.Text ?? "";
                
                if (!string.IsNullOrEmpty(txtValorTotal.Text))
                {
                    if (decimal.TryParse(txtValorTotal.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal valorTotal))
                    {
                        reserva.ValorTotal = valorTotal;
                    }
                }
                
                if (!string.IsNullOrEmpty(txtPrevisaoEntrega.Text))
                {
                    if (DateTime.TryParse(txtPrevisaoEntrega.Text, out DateTime previsaoEntrega))
                    {
                        reserva.PrevisaoEntrega = previsaoEntrega;
                    }
                }
                else
                {
                    reserva.PrevisaoEntrega = null;
                }

                _databaseService.AtualizarReserva(reserva);
                
                // Buscar dados do cliente antes de enviar email
                if (reserva.ClienteId.HasValue)
                {
                    var cliente = _databaseService.ObterClientePorId(reserva.ClienteId.Value);
                    if (cliente != null)
                    {
                        reserva.Nome = cliente.Nome;
                        reserva.Email = cliente.Email;
                        reserva.Telefone = cliente.Telefone;
                    }
                }
                
                // Buscar status atualizado
                if (reserva.StatusId.HasValue)
                {
                    var statusReserva = _databaseService.ObterStatusReservaPorId(reserva.StatusId.Value);
                    if (statusReserva != null)
                    {
                        reserva.Status = statusReserva.Nome;
                    }
                }
                
                // Enviar email de forma ass�ncrona
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        var emailService = new EmailService();
                        emailService.EnviarEmailReservaAlterada(reserva);
                    }
                    catch
                    {
                        // Erro ao enviar email - n�o bloquear
                    }
                });

                CarregarReservas();
                CarregarDropdownStatus(); // Recarregar dropdown para garantir que est� atualizado
                
                // Fechar modal e mostrar mensagem de sucesso
                string scriptFecharModal = @"
                    setTimeout(function() {
                        var modalElement = document.getElementById('modalEditarReserva');
                        if (modalElement) {
                            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                                var modal = bootstrap.Modal.getInstance(modalElement);
                                if (modal) {
                                    modal.hide();
                                } else {
                                    modalElement.classList.remove('show');
                                    modalElement.style.display = 'none';
                                    modalElement.setAttribute('aria-hidden', 'true');
                                    document.body.classList.remove('modal-open');
                                    var backdrop = document.querySelector('.modal-backdrop');
                                    if (backdrop) backdrop.remove();
                                }
                            } else {
                                modalElement.classList.remove('show');
                                modalElement.style.display = 'none';
                                modalElement.setAttribute('aria-hidden', 'true');
                                document.body.classList.remove('modal-open');
                                var backdrop = document.querySelector('.modal-backdrop');
                                if (backdrop) backdrop.remove();
                            }
                        }
                        alert('Reserva atualizada com sucesso!');
                    }, 100);";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "FecharModalESucesso", scriptFecharModal, true);
            }
            catch (Exception ex)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                    string.Format("alert('Erro ao salvar reserva: {0}');", EscapeJavaScript(ex.Message)), true);
            }
        }

        private void CarregarLogs(int dias = 7)
        {
            try
            {
                var logs = LogService.ObterLogs(dias);
                
                // Agrupar por "Quem" e depois por "O que"
                var logsAgrupados = logs
                    .GroupBy(l => l.Quem ?? "An�nimo")
                    .OrderBy(g => g.Key)
                    .ToDictionary(
                        g => g.Key,
                        g => g.GroupBy(l => l.OQue ?? "N/A")
                            .OrderBy(g2 => g2.Key)
                            .ToDictionary(g2 => g2.Key, g2 => g2.OrderByDescending(l => l.Timestamp).ToList())
                    );

                var html = new System.Text.StringBuilder();
                html.AppendLine("<div class='log-tree'>");

                foreach (var usuarioGroup in logsAgrupados)
                {
                    string usuario = usuarioGroup.Key;
                    int totalAcoes = usuarioGroup.Value.Sum(g => g.Value.Count);
                    
                    html.AppendLine($@"
                        <div class='log-user-group'>
                            <div class='log-user-header' onclick='toggleLogGroup(this)'>
                                <span><i class='fas fa-user'></i> {System.Web.HttpUtility.HtmlEncode(usuario)}</span>
                                <span class='log-count'>{totalAcoes} a��o(�es)</span>
                            </div>
                            <div class='log-user-content'>");

                    foreach (var entidadeGroup in usuarioGroup.Value)
                    {
                        string entidade = entidadeGroup.Key;
                        var entradas = entidadeGroup.Value;
                        
                        html.AppendLine($@"
                            <div class='log-entity-group'>
                                <div class='log-entity-header' onclick='toggleLogGroup(this)'>
                                    <span><i class='fas fa-cube'></i> {System.Web.HttpUtility.HtmlEncode(entidade)}</span>
                                    <span class='log-count'>{entradas.Count} registro(s)</span>
                                </div>
                                <div class='log-entity-content'>");

                        foreach (var log in entradas)
                        {
                            string tipoClass = $"log-tipo-{log.Tipo}";
                            string timestamp = log.Timestamp.ToString("dd/MM/yyyy HH:mm:ss");
                            string onde = !string.IsNullOrEmpty(log.Onde) ? log.Onde : "N/A";
                            
                            html.AppendLine($@"
                                <div class='log-entry'>
                                    <span class='log-timestamp'>{timestamp}</span>
                                    <span class='log-tipo {tipoClass}'>{log.Tipo}</span>
                                    <span class='log-onde'>{System.Web.HttpUtility.HtmlEncode(onde)}</span>");

                            if (!string.IsNullOrEmpty(log.Detalhes))
                            {
                                html.AppendLine($@"
                                    <div class='log-detalhes'>
                                        <strong>Detalhes:</strong> {System.Web.HttpUtility.HtmlEncode(log.Detalhes)}
                                    </div>");
                            }

                            html.AppendLine("</div>");
                        }

                        html.AppendLine(@"
                                </div>
                            </div>");
                    }

                    html.AppendLine(@"
                            </div>
                        </div>");
                }

                html.AppendLine("</div>");

                if (logs.Count == 0)
                {
                    html.Clear();
                    html.AppendLine("<div class='alert alert-info'><i class='fas fa-info-circle'></i> Nenhum log encontrado para o per�odo selecionado.</div>");
                }

                logsContainer.InnerHtml = html.ToString();
            }
            catch (Exception ex)
            {
                logsContainer.InnerHtml = $"<div class='alert alert-danger'>Erro ao carregar logs: {EscapeJavaScript(ex.Message)}</div>";
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
                // Log do erro se necess�rio
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

        private void CarregarStatusReserva()
        {
            try
            {
                var statusList = _databaseService.ObterTodosStatusReserva();
                var tableBody = FindControl("statusReservaTableBody") as System.Web.UI.HtmlControls.HtmlGenericControl;
                
                if (tableBody == null) return;
                
                tableBody.InnerHtml = "";
                
                if (statusList.Count == 0)
                {
                    tableBody.InnerHtml = "<tr><td colspan='7' class='text-center'>Nenhum status cadastrado.</td></tr>";
                    return;
                }
                
                foreach (var status in statusList)
                {
                    string permiteAlteracao = status.PermiteAlteracao ? "<i class='fas fa-check text-success'></i> Sim" : "<i class='fas fa-times text-danger'></i> N�o";
                    string permiteExclusao = status.PermiteExclusao ? "<i class='fas fa-check text-success'></i> Sim" : "<i class='fas fa-times text-danger'></i> N�o";
                    bool podeExcluir = _databaseService.PodeExcluirStatusReserva(status.Id);
                    
                    tableBody.InnerHtml += $@"
                        <tr>
                            <td>{status.Id}</td>
                            <td>{System.Web.HttpUtility.HtmlEncode(status.Nome)}</td>
                            <td>{System.Web.HttpUtility.HtmlEncode(status.Descricao)}</td>
                            <td>{permiteAlteracao}</td>
                            <td>{permiteExclusao}</td>
                            <td>{status.Ordem}</td>
                            <td>
                                <button type='button' class='btn btn-sm btn-primary' onclick='editarStatusReserva({status.Id})'>
                                    <i class='fas fa-edit'></i> Editar
                                </button>
                                {(podeExcluir ? $@"<button type='button' class='btn btn-sm btn-danger' onclick='excluirStatusReserva({status.Id}, ""{EscapeJavaScript(status.Nome)}"")'>
                                    <i class='fas fa-trash'></i> Excluir
                                </button>" : "<span class='text-muted'><i class='fas fa-lock'></i> Em uso</span>")}
                            </td>
                        </tr>";
                }
            }
            catch (Exception ex)
            {
                var tableBody = FindControl("statusReservaTableBody") as System.Web.UI.HtmlControls.HtmlGenericControl;
                if (tableBody != null)
                {
                    tableBody.InnerHtml = $"<tr><td colspan='7' class='text-center text-danger'>Erro ao carregar status: {EscapeJavaScript(ex.Message)}</td></tr>";
                }
            }
        }

        protected void btnSalvarStatusReserva_Click(object sender, EventArgs e)
        {
            try
            {
                // Validar campos obrigat�rios
                if (string.IsNullOrWhiteSpace(txtStatusReservaNome.Text))
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        $"alert('{EscapeJavaScript("Nome � obrigat�rio.")}');", true);
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(txtStatusReservaDescricao.Text))
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        $"alert('{EscapeJavaScript("Descri��o � obrigat�ria.")}');", true);
                    return;
                }
                
                int statusId = 0;
                if (!int.TryParse(hdnStatusReservaId.Value, out statusId))
                {
                    statusId = 0;
                }
                
                int ordem = 0;
                if (!int.TryParse(txtStatusReservaOrdem.Text, out ordem))
                {
                    ordem = 0;
                }
                
                var status = new StatusReserva
                {
                    Id = statusId,
                    Nome = txtStatusReservaNome.Text.Trim(),
                    Descricao = txtStatusReservaDescricao.Text.Trim(),
                    PermiteAlteracao = chkStatusReservaPermiteAlteracao.Checked,
                    PermiteExclusao = chkStatusReservaPermiteExclusao.Checked,
                    Ordem = ordem
                };
                
                if (statusId == 0)
                {
                    // Criar novo
                    _databaseService.CriarStatusReserva(status);
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Sucesso", 
                        $"alert('{EscapeJavaScript("Status adicionado com sucesso!")}');", true);
                }
                else
                {
                    // Atualizar existente
                    _databaseService.AtualizarStatusReserva(status);
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Sucesso", 
                        $"alert('{EscapeJavaScript("Status atualizado com sucesso!")}');", true);
                }
                
                // Recarregar lista de status
                CarregarStatusReserva();
                CarregarDropdownStatus(); // Recarregar dropdown tamb�m para garantir que est� atualizado
                
                // Limpar campos
                hdnStatusReservaId.Value = "0";
                txtStatusReservaNome.Text = "";
                txtStatusReservaDescricao.Text = "";
                txtStatusReservaOrdem.Text = "0";
                chkStatusReservaPermiteAlteracao.Checked = true;
                chkStatusReservaPermiteExclusao.Checked = true;
                
                // Fechar modal
                string scriptFecharModalStatus = @"
                    setTimeout(function() {
                        var modalElement = document.getElementById('modalNovoStatusReserva');
                        var titleElement = document.getElementById('modalStatusReservaTitle');
                        if (titleElement) {
                            titleElement.textContent = 'Novo Status de Reserva';
                        }
                        if (modalElement) {
                            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                                var modal = bootstrap.Modal.getInstance(modalElement);
                                if (modal) {
                                    modal.hide();
                                } else {
                                    modalElement.classList.remove('show');
                                    modalElement.style.display = 'none';
                                    modalElement.setAttribute('aria-hidden', 'true');
                                    document.body.classList.remove('modal-open');
                                    var backdrop = document.querySelector('.modal-backdrop');
                                    if (backdrop) backdrop.remove();
                                }
                            } else {
                                modalElement.classList.remove('show');
                                modalElement.style.display = 'none';
                                modalElement.setAttribute('aria-hidden', 'true');
                                document.body.classList.remove('modal-open');
                                var backdrop = document.querySelector('.modal-backdrop');
                                if (backdrop) backdrop.remove();
                            }
                        }
                    }, 100);";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "FecharModalStatus", scriptFecharModalStatus, true);
            }
            catch (Exception ex)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                    string.Format("alert('Erro ao salvar status: {0}');", EscapeJavaScript(ex.Message)), true);
            }
        }

        private void EditarStatusReserva(int statusId)
        {
            try
            {
                var status = _databaseService.ObterStatusReservaPorId(statusId);
                if (status == null)
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        $"alert('{EscapeJavaScript("Status n�o encontrado.")}');", true);
                    return;
                }
                
                // Limpar campos antes de preencher
                hdnStatusReservaId.Value = status.Id.ToString();
                txtStatusReservaNome.Text = status.Nome ?? "";
                txtStatusReservaDescricao.Text = status.Descricao ?? "";
                txtStatusReservaOrdem.Text = status.Ordem.ToString();
                chkStatusReservaPermiteAlteracao.Checked = status.PermiteAlteracao;
                chkStatusReservaPermiteExclusao.Checked = status.PermiteExclusao;
                
                // Atualizar t�tulo do modal
                string scriptAbrirModalStatus = @"
                    setTimeout(function() {
                        var modalElement = document.getElementById('modalNovoStatusReserva');
                        var titleElement = document.getElementById('modalStatusReservaTitle');
                        if (titleElement) {
                            titleElement.textContent = 'Editar Status de Reserva';
                        }
                        if (modalElement) {
                            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                                var modal = new bootstrap.Modal(modalElement);
                                modal.show();
                            } else {
                                modalElement.classList.add('show');
                                modalElement.style.display = 'block';
                                modalElement.setAttribute('aria-hidden', 'false');
                                document.body.classList.add('modal-open');
                                var backdrop = document.createElement('div');
                                backdrop.className = 'modal-backdrop fade show';
                                document.body.appendChild(backdrop);
                            }
                        }
                    }, 100);";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "AbrirModalEditarStatus", scriptAbrirModalStatus, true);
            }
            catch (Exception ex)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                    string.Format("alert('Erro ao carregar status: {0}');", EscapeJavaScript(ex.Message)), true);
            }
        }

        private void CarregarDadosReservaParaEdicao(int reservaId)
        {
            try
            {
                var reserva = _databaseService.ObterReservaPorId(reservaId);
                if (reserva == null)
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        $"alert('{EscapeJavaScript("Reserva n�o encontrada.")}');", true);
                    return;
                }
                
                // Preencher campos do formul�rio
                hdnReservaId.Value = reserva.Id.ToString();
                
                // Garantir que o dropdown de status est� carregado
                CarregarDropdownStatus();
                
                // Preencher Status
                if (reserva.StatusId.HasValue)
                {
                    ddlStatus.SelectedValue = reserva.StatusId.Value.ToString();
                }
                else
                {
                    if (ddlStatus.Items.Count > 0)
                    {
                        ddlStatus.SelectedIndex = 0;
                    }
                }
                
                // Preencher Valor Total
                txtValorTotal.Text = reserva.ValorTotal.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                
                // Preencher checkboxes
                chkConvertidoEmPedido.Checked = reserva.ConvertidoEmPedido;
                chkCancelado.Checked = reserva.Cancelado;
                
                // Preencher Previs�o de Entrega
                if (reserva.PrevisaoEntrega.HasValue)
                {
                    txtPrevisaoEntrega.Text = reserva.PrevisaoEntrega.Value.ToString("yyyy-MM-ddTHH:mm");
                }
                else
                {
                    txtPrevisaoEntrega.Text = "";
                }
                
                // Preencher Observa��es
                txtObservacoesReserva.Text = reserva.Observacoes ?? "";
                
                // Abrir modal
                string scriptAbrirModal = @"
                    setTimeout(function() {
                        var modalElement = document.getElementById('modalEditarReserva');
                        if (modalElement) {
                            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                                var modal = new bootstrap.Modal(modalElement);
                                modal.show();
                            } else {
                                modalElement.classList.add('show');
                                modalElement.style.display = 'block';
                                modalElement.setAttribute('aria-hidden', 'false');
                                document.body.classList.add('modal-open');
                                var backdrop = document.createElement('div');
                                backdrop.className = 'modal-backdrop fade show';
                                document.body.appendChild(backdrop);
                            }
                        }
                    }, 100);";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "AbrirModalEditarReserva", scriptAbrirModal, true);
            }
            catch (Exception ex)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                    string.Format("alert('Erro ao carregar dados da reserva: {0}');", EscapeJavaScript(ex.Message)), true);
            }
        }

        private void ExcluirStatusReserva(int statusId)
        {
            try
            {
                _databaseService.ExcluirStatusReserva(statusId);
                CarregarStatusReserva();
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Sucesso", 
                    $"alert('{EscapeJavaScript("Status exclu�do com sucesso!")}');", true);
            }
            catch (Exception ex)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                    string.Format("alert('Erro ao excluir status: {0}');", EscapeJavaScript(ex.Message)), true);
            }
        }

        private void ExcluirReserva(int reservaId)
        {
            try
            {
                // Buscar reserva antes de excluir para enviar email
                var reserva = _databaseService.ObterReservaPorId(reservaId);
                
                if (reserva == null)
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        $"alert('{EscapeJavaScript("Reserva n�o encontrada.")}');", true);
                    return;
                }
                
                // Verificar se a reserva est� cancelada (apenas administradores podem excluir reservas canceladas)
                if (!reserva.Cancelado)
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        $"alert('{EscapeJavaScript("Apenas reservas canceladas podem ser exclu�das.")}');", true);
                    return;
                }
                
                // Buscar dados do cliente antes de excluir
                if (reserva.ClienteId.HasValue)
                {
                    var cliente = _databaseService.ObterClientePorId(reserva.ClienteId.Value);
                    if (cliente != null)
                    {
                        reserva.Nome = cliente.Nome;
                        reserva.Email = cliente.Email;
                        reserva.Telefone = cliente.Telefone;
                    }
                }
                
                // Buscar status antes de excluir
                if (reserva.StatusId.HasValue)
                {
                    var statusReserva = _databaseService.ObterStatusReservaPorId(reserva.StatusId.Value);
                    if (statusReserva != null)
                    {
                        reserva.Status = statusReserva.Nome;
                    }
                }
                
                // Excluir reserva
                _databaseService.ExcluirReserva(reservaId);
                
                // Enviar email de forma ass�ncrona
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        var emailService = new EmailService();
                        emailService.EnviarEmailReservaExcluida(reserva);
                    }
                    catch
                    {
                        // Erro ao enviar email - n�o bloquear
                    }
                });
                
                CarregarReservas();
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Sucesso", 
                    $"alert('{EscapeJavaScript("Reserva exclu�da com sucesso!")}');", true);
            }
            catch (Exception ex)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                    string.Format("alert('Erro ao excluir reserva: {0}');", EscapeJavaScript(ex.Message)), true);
            }
        }

        private void CarregarConfiguracoes()
        {
            try
            {
                var configuracoes = System.Configuration.ConfigurationManager.AppSettings;
                
                // Preencher campos com valores atuais usando FindControl
                var txtBaseUrl = FindControl("txtBaseUrl") as TextBox;
                var ddlEnvironment = FindControl("ddlEnvironment") as DropDownList;
                var txtSmtpServer = FindControl("txtSmtpServer") as TextBox;
                var txtSmtpPort = FindControl("txtSmtpPort") as TextBox;
                var txtSmtpUsername = FindControl("txtSmtpUsername") as TextBox;
                var txtSmtpPassword = FindControl("txtSmtpPassword") as TextBox;
                var txtEmailFrom = FindControl("txtEmailFrom") as TextBox;
                var txtEmailIsabela = FindControl("txtEmailIsabela") as TextBox;
                var txtEmailCamila = FindControl("txtEmailCamila") as TextBox;
                
                if (txtBaseUrl != null) txtBaseUrl.Text = configuracoes["BaseUrl"] ?? "";
                if (ddlEnvironment != null) ddlEnvironment.SelectedValue = configuracoes["Environment"] ?? "Development";
                if (txtSmtpServer != null) txtSmtpServer.Text = configuracoes["SmtpServer"] ?? "";
                if (txtSmtpPort != null) txtSmtpPort.Text = configuracoes["SmtpPort"] ?? "";
                if (txtSmtpUsername != null) txtSmtpUsername.Text = configuracoes["SmtpUsername"] ?? "";
                if (txtSmtpPassword != null) txtSmtpPassword.Text = configuracoes["SmtpPassword"] ?? "";
                if (txtEmailFrom != null) txtEmailFrom.Text = configuracoes["EmailFrom"] ?? "";
                if (txtEmailIsabela != null) txtEmailIsabela.Text = configuracoes["EmailIsabela"] ?? "";
                if (txtEmailCamila != null) txtEmailCamila.Text = configuracoes["EmailCamila"] ?? "";
            }
            catch (Exception ex)
            {
                // Log do erro
                LogService.Registrar("ERROR", "Sistema", "Carregar Configura��es", "Admin.aspx.cs", $"Erro: {ex.Message}");
            }
        }

        protected void btnSalvarConfiguracoes_Click(object sender, EventArgs e)
        {
            try
            {
                // Nota: Alterar web.config em runtime requer permiss�es especiais
                // Por enquanto, vamos apenas mostrar uma mensagem informativa
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Info", 
                    "alert('Para alterar as configura��es, edite o arquivo web.config manualmente ou use transforma��es de configura��o (Web.Release.config).');", true);
                
                // TODO: Implementar salvamento em banco de dados ou arquivo de configura��o alternativo
            }
            catch (Exception ex)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                    string.Format("alert('Erro ao salvar configura��es: {0}');", EscapeJavaScript(ex.Message)), true);
            }
        }

        private void MostrarDetalhesReserva(int reservaId)
        {
            try
            {
                var reserva = _databaseService.ObterReservaPorId(reservaId);
                if (reserva == null)
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        "alert('Reserva n�o encontrada.');", true);
                    return;
                }
                
                // Buscar dados do cliente
                var cliente = reserva.ClienteId.HasValue ? _databaseService.ObterClientePorId(reserva.ClienteId.Value) : null;
                
                // Preparar HTML dos detalhes
                string html = "<div class='container-fluid'>";
                
                // Informa��es principais
                html += "<div class='row mb-3'>";
                html += "<div class='col-md-6'><div class='detalhe-item'>";
                html += "<strong><i class='fas fa-hashtag'></i> N�mero da Reserva</strong>";
                html += "<div class='detalhe-item-valor'>#" + reserva.Id + "</div></div></div>";
                html += "<div class='col-md-6'><div class='detalhe-item'>";
                html += "<strong><i class='fas fa-tag'></i> Status</strong>";
                html += "<div class='detalhe-item-valor'><span class='status-badge status-" + reserva.Status.ToLower().Replace(" ", "-") + "'>" + System.Web.HttpUtility.HtmlEncode(reserva.Status) + "</span></div></div></div>";
                html += "</div>";
                
                // Cliente
                if (cliente != null)
                {
                    html += "<div class='row mb-3'>";
                    html += "<div class='col-md-6'><div class='detalhe-item'>";
                    html += "<strong><i class='fas fa-user'></i> Cliente</strong>";
                    html += "<div class='detalhe-item-valor'>" + System.Web.HttpUtility.HtmlEncode(cliente.Nome) + "</div></div></div>";
                    html += "<div class='col-md-6'><div class='detalhe-item'>";
                    html += "<strong><i class='fas fa-envelope'></i> Email</strong>";
                    html += "<div class='detalhe-item-valor'>" + System.Web.HttpUtility.HtmlEncode(cliente.Email ?? "N�o informado") + "</div></div></div>";
                    html += "</div>";
                    html += "<div class='row mb-3'>";
                    html += "<div class='col-md-6'><div class='detalhe-item'>";
                    html += "<strong><i class='fas fa-phone'></i> Telefone</strong>";
                    html += "<div class='detalhe-item-valor'>" + System.Web.HttpUtility.HtmlEncode(cliente.Telefone ?? "N�o informado") + "</div></div></div>";
                    html += "<div class='col-md-6'><div class='detalhe-item'>";
                    html += "<strong><i class='fas fa-shield-alt'></i> Tipo</strong>";
                    html += "<div class='detalhe-item-valor'>" + (cliente.IsAdmin ? "<span class='badge bg-danger'>Administrador</span>" : "<span class='badge bg-primary'>Cliente</span>") + "</div></div></div>";
                    html += "</div>";
                }
                
                // Datas
                html += "<div class='row mb-3'>";
                html += "<div class='col-md-6'><div class='detalhe-item'>";
                html += "<strong><i class='fas fa-calendar-plus'></i> Data da Reserva</strong>";
                html += "<div class='detalhe-item-valor'>" + reserva.DataReserva.ToString("dd/MM/yyyy HH:mm") + "</div></div></div>";
                html += "<div class='col-md-6'><div class='detalhe-item'>";
                html += "<strong><i class='fas fa-calendar-check'></i> Data de Retirada</strong>";
                html += "<div class='detalhe-item-valor'>" + reserva.DataRetirada.ToString("dd/MM/yyyy") + "</div></div></div>";
                html += "</div>";
                
                // Valor e Previs�o
                html += "<div class='row mb-3'>";
                html += "<div class='col-md-6'><div class='detalhe-item'>";
                html += "<strong><i class='fas fa-dollar-sign'></i> Valor Total</strong>";
                html += "<div class='detalhe-item-valor' style='font-size: 20px; font-weight: 700; color: #28a745;'>R$ " + reserva.ValorTotal.ToString("F2") + "</div></div></div>";
                html += "<div class='col-md-6'><div class='detalhe-item'>";
                html += "<strong><i class='fas fa-truck'></i> Previs�o de Entrega</strong>";
                html += "<div class='detalhe-item-valor'>" + (reserva.PrevisaoEntrega.HasValue ? reserva.PrevisaoEntrega.Value.ToString("dd/MM/yyyy HH:mm") : "N�o definida") + "</div></div></div>";
                html += "</div>";
                
                // Itens
                html += "<div class='detalhe-itens' style='background: white; border: 1px solid #dee2e6; border-radius: 8px; padding: 16px; margin-top: 12px;'>";
                html += "<h6 class='mb-3'><i class='fas fa-shopping-bag'></i> Itens da Reserva</h6>";
                if (reserva.Itens != null && reserva.Itens.Count > 0)
                {
                    foreach (var item in reserva.Itens)
                    {
                        html += "<div class='detalhe-item-produto' style='padding: 10px; margin-bottom: 8px; background: #f8f9fa; border-radius: 6px; border-left: 3px solid #28a745;'>";
                        html += "<div class='d-flex justify-content-between align-items-start'>";
                        html += "<div>";
                        html += "<strong>" + System.Web.HttpUtility.HtmlEncode(item.NomeProduto) + "</strong>";
                        html += "<div class='text-muted small'>Tamanho: " + System.Web.HttpUtility.HtmlEncode(item.Tamanho) + " | Quantidade: " + item.Quantidade + "</div>";
                        html += "</div>";
                        html += "<div class='text-end'>";
                        html += "<strong class='text-success'>R$ " + item.Subtotal.ToString("F2") + "</strong>";
                        html += "</div>";
                        html += "</div>";
                        html += "</div>";
                    }
                }
                else
                {
                    html += "<p class='text-muted'>Nenhum item encontrado.</p>";
                }
                html += "</div>";
                
                // Observa��es
                html += "<div class='detalhe-item mt-3'>";
                html += "<strong><i class='fas fa-comment-alt'></i> Observa��es</strong>";
                html += "<div class='detalhe-item-valor'>" + System.Web.HttpUtility.HtmlEncode(!string.IsNullOrEmpty(reserva.Observacoes) ? reserva.Observacoes : "Nenhuma observa��o") + "</div>";
                html += "</div>";
                
                // Status adicional
                html += "<div class='row mb-3 mt-3'>";
                html += "<div class='col-md-6'><div class='detalhe-item'>";
                html += "<strong><i class='fas fa-check-circle'></i> Convertido em Pedido</strong>";
                html += "<div class='detalhe-item-valor'>" + (reserva.ConvertidoEmPedido ? "<span class='badge bg-success'>Sim</span>" : "<span class='badge bg-secondary'>N�o</span>") + "</div></div></div>";
                html += "<div class='col-md-6'><div class='detalhe-item'>";
                html += "<strong><i class='fas fa-times-circle'></i> Cancelado</strong>";
                html += "<div class='detalhe-item-valor'>" + (reserva.Cancelado ? "<span class='badge bg-danger'>Sim</span>" : "<span class='badge bg-success'>N�o</span>") + "</div></div></div>";
                html += "</div>";
                
                html += "</div>";
                
                // Script para abrir modal e preencher
                string script = string.Format(@"
                    var modalBody = document.getElementById('modalDetalhesReservaAdminBody');
                    if (modalBody) {{
                        modalBody.innerHTML = {0};
                        var modalElement = document.getElementById('modalDetalhesReservaAdmin');
                        if (modalElement) {{
                            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {{
                                var modal = new bootstrap.Modal(modalElement);
                                modal.show();
                            }}
                        }}
                    }}", 
                    System.Web.HttpUtility.JavaScriptStringEncode(html));
                
                Page.ClientScript.RegisterStartupScript(this.GetType(), "MostrarDetalhesReserva", script, true);
            }
            catch (Exception ex)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                    string.Format("alert('Erro ao carregar detalhes: {0}');", EscapeJavaScript(ex.Message)), true);
            }
        }

        private void MostrarDetalhesCliente(int clienteId)
        {
            try
            {
                var cliente = _databaseService.ObterClientePorId(clienteId);
                if (cliente == null)
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        "alert('Cliente n�o encontrado.');", true);
                    return;
                }
                
                // Buscar reservas do cliente
                var reservas = _databaseService.ObterReservasPorCliente(clienteId);
                
                // Preparar HTML dos detalhes
                string html = "<div class='container-fluid'>";
                
                // Informa��es principais
                html += "<div class='row mb-3'>";
                html += "<div class='col-md-6'><div class='detalhe-item'>";
                html += "<strong><i class='fas fa-hashtag'></i> ID do Cliente</strong>";
                html += "<div class='detalhe-item-valor'>#" + cliente.Id + "</div></div></div>";
                html += "<div class='col-md-6'><div class='detalhe-item'>";
                html += "<strong><i class='fas fa-shield-alt'></i> Tipo</strong>";
                html += "<div class='detalhe-item-valor'>" + (cliente.IsAdmin ? "<span class='badge bg-danger'>Administrador</span>" : "<span class='badge bg-primary'>Cliente</span>") + "</div></div></div>";
                html += "</div>";
                
                // Dados pessoais
                html += "<div class='row mb-3'>";
                html += "<div class='col-md-6'><div class='detalhe-item'>";
                html += "<strong><i class='fas fa-user'></i> Nome</strong>";
                html += "<div class='detalhe-item-valor'>" + System.Web.HttpUtility.HtmlEncode(cliente.Nome) + "</div></div></div>";
                html += "<div class='col-md-6'><div class='detalhe-item'>";
                html += "<strong><i class='fas fa-envelope'></i> Email</strong>";
                html += "<div class='detalhe-item-valor'>" + System.Web.HttpUtility.HtmlEncode(cliente.Email ?? "N�o informado") + "</div></div></div>";
                html += "</div>";
                
                html += "<div class='row mb-3'>";
                html += "<div class='col-md-6'><div class='detalhe-item'>";
                html += "<strong><i class='fas fa-phone'></i> Telefone</strong>";
                html += "<div class='detalhe-item-valor'>" + System.Web.HttpUtility.HtmlEncode(cliente.Telefone ?? "N�o informado") + "</div></div></div>";
                html += "<div class='col-md-6'><div class='detalhe-item'>";
                html += "<strong><i class='fab fa-whatsapp'></i> Tem WhatsApp</strong>";
                html += "<div class='detalhe-item-valor'>" + (cliente.TemWhatsApp ? "<span class='badge bg-success'>Sim</span>" : "<span class='badge bg-secondary'>N�o</span>") + "</div></div></div>";
                html += "</div>";
                
                // Datas
                html += "<div class='row mb-3'>";
                html += "<div class='col-md-6'><div class='detalhe-item'>";
                html += "<strong><i class='fas fa-calendar-plus'></i> Data de Cadastro</strong>";
                html += "<div class='detalhe-item-valor'>" + cliente.DataCadastro.ToString("dd/MM/yyyy HH:mm") + "</div></div></div>";
                html += "<div class='col-md-6'><div class='detalhe-item'>";
                html += "<strong><i class='fas fa-clock'></i> �ltimo Acesso</strong>";
                html += "<div class='detalhe-item-valor'>" + (cliente.UltimoAcesso.HasValue ? cliente.UltimoAcesso.Value.ToString("dd/MM/yyyy HH:mm") : "Nunca") + "</div></div></div>";
                html += "</div>";
                
                // Status de confirma��o
                html += "<div class='row mb-3'>";
                html += "<div class='col-md-6'><div class='detalhe-item'>";
                html += "<strong><i class='fas fa-envelope-check'></i> Email Confirmado</strong>";
                html += "<div class='detalhe-item-valor'>" + (cliente.EmailConfirmado ? "<span class='badge bg-success'>Sim</span>" : "<span class='badge bg-warning'>N�o</span>") + "</div></div></div>";
                html += "<div class='col-md-6'><div class='detalhe-item'>";
                html += "<strong><i class='fab fa-whatsapp'></i> WhatsApp Confirmado</strong>";
                html += "<div class='detalhe-item-valor'>" + (cliente.WhatsAppConfirmado ? "<span class='badge bg-success'>Sim</span>" : "<span class='badge bg-warning'>N�o</span>") + "</div></div></div>";
                html += "</div>";
                
                // Reservas
                html += "<div class='detalhe-itens' style='background: white; border: 1px solid #dee2e6; border-radius: 8px; padding: 16px; margin-top: 12px;'>";
                html += "<h6 class='mb-3'><i class='fas fa-clipboard-list'></i> Reservas do Cliente</h6>";
                if (reservas != null && reservas.Count > 0)
                {
                    html += "<p class='text-muted'>Total de reservas: <strong>" + reservas.Count + "</strong></p>";
                    html += "<ul class='list-group'>";
                    foreach (var reserva in reservas.OrderByDescending(r => r.DataReserva).Take(10))
                    {
                        html += "<li class='list-group-item'>";
                        html += "<strong>Reserva #" + reserva.Id + "</strong> - ";
                        html += reserva.DataReserva.ToString("dd/MM/yyyy") + " - ";
                        html += "R$ " + reserva.ValorTotal.ToString("F2") + " - ";
                        html += "<span class='badge bg-" + (reserva.Cancelado ? "danger" : "success") + "'>" + System.Web.HttpUtility.HtmlEncode(reserva.Status) + "</span>";
                        html += "</li>";
                    }
                    if (reservas.Count > 10)
                    {
                        html += "<li class='list-group-item text-muted'>... e mais " + (reservas.Count - 10) + " reserva(s)</li>";
                    }
                    html += "</ul>";
                }
                else
                {
                    html += "<p class='text-muted'>Nenhuma reserva encontrada.</p>";
                }
                html += "</div>";
                
                html += "</div>";
                
                // Script para abrir modal e preencher
                string script = string.Format(@"
                    var modalBody = document.getElementById('modalDetalhesClienteBody');
                    if (modalBody) {{
                        modalBody.innerHTML = {0};
                        var modalElement = document.getElementById('modalDetalhesCliente');
                        if (modalElement) {{
                            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {{
                                var modal = new bootstrap.Modal(modalElement);
                                modal.show();
                            }}
                        }}
                    }}", 
                    System.Web.HttpUtility.JavaScriptStringEncode(html));
                
                Page.ClientScript.RegisterStartupScript(this.GetType(), "MostrarDetalhesCliente", script, true);
            }
            catch (Exception ex)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                    string.Format("alert('Erro ao carregar detalhes: {0}');", EscapeJavaScript(ex.Message)), true);
            }
        }

        private void MostrarDetalhesProduto(int produtoId)
        {
            try
            {
                var produto = _databaseService.ObterProdutoPorId(produtoId);
                if (produto == null)
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                        "alert('Produto n�o encontrado.');", true);
                    return;
                }
                
                // Preparar HTML dos detalhes
                string html = "<div class='container-fluid'>";
                
                // Imagem e informa��es principais
                html += "<div class='row mb-3'>";
                html += "<div class='col-md-4 text-center'>";
                html += "<img src='" + System.Web.HttpUtility.HtmlAttributeEncode(produto.ImagemUrl ?? "") + "' alt='" + System.Web.HttpUtility.HtmlAttributeEncode(produto.Nome) + "' class='img-fluid rounded' style='max-width: 200px;' onerror='this.src=\"https://via.placeholder.com/200x200?text=" + System.Web.HttpUtility.UrlEncode(produto.Nome) + "\"' />";
                html += "</div>";
                html += "<div class='col-md-8'>";
                html += "<div class='detalhe-item'>";
                html += "<strong><i class='fas fa-hashtag'></i> ID do Produto</strong>";
                html += "<div class='detalhe-item-valor'>#" + produto.Id + "</div>";
                html += "</div>";
                html += "<div class='detalhe-item'>";
                html += "<strong><i class='fas fa-cookie-bite'></i> Nome</strong>";
                html += "<div class='detalhe-item-valor' style='font-size: 18px; font-weight: 700;'>" + System.Web.HttpUtility.HtmlEncode(produto.Nome) + "</div>";
                html += "</div>";
                html += "<div class='detalhe-item'>";
                html += "<strong><i class='fas fa-tag'></i> Status</strong>";
                html += "<div class='detalhe-item-valor'>" + (produto.Ativo ? "<span class='badge bg-success'>Ativo</span>" : "<span class='badge bg-secondary'>Inativo</span>") + "</div>";
                html += "</div>";
                html += "</div>";
                html += "</div>";
                
                // Descri��o
                if (!string.IsNullOrEmpty(produto.Descricao))
                {
                    html += "<div class='detalhe-item'>";
                    html += "<strong><i class='fas fa-align-left'></i> Descri��o</strong>";
                    html += "<div class='detalhe-item-valor'>" + System.Web.HttpUtility.HtmlEncode(produto.Descricao) + "</div>";
                    html += "</div>";
                }
                
                // Pre�o e Ordem
                html += "<div class='row mb-3'>";
                html += "<div class='col-md-6'><div class='detalhe-item'>";
                html += "<strong><i class='fas fa-dollar-sign'></i> Pre�o</strong>";
                html += "<div class='detalhe-item-valor' style='font-size: 20px; font-weight: 700; color: #28a745;'>R$ " + produto.Preco.ToString("F2") + "</div></div></div>";
                html += "<div class='col-md-6'><div class='detalhe-item'>";
                html += "<strong><i class='fas fa-sort-numeric-down'></i> Ordem de Exibi��o</strong>";
                html += "<div class='detalhe-item-valor'>" + produto.Ordem + "</div></div></div>";
                html += "</div>";
                
                // Datas
                html += "<div class='row mb-3'>";
                html += "<div class='col-md-6'><div class='detalhe-item'>";
                html += "<strong><i class='fas fa-calendar-check'></i> Reserv�vel at�</strong>";
                html += "<div class='detalhe-item-valor'>" + (produto.ReservavelAte.HasValue ? produto.ReservavelAte.Value.ToString("dd/MM/yyyy") : "Sem limite") + "</div></div></div>";
                html += "<div class='col-md-6'><div class='detalhe-item'>";
                html += "<strong><i class='fas fa-calendar-times'></i> Vend�vel at�</strong>";
                html += "<div class='detalhe-item-valor'>" + (produto.VendivelAte.HasValue ? produto.VendivelAte.Value.ToString("dd/MM/yyyy") : "Sem limite") + "</div></div></div>";
                html += "</div>";
                
                // Saco Promocional
                if (produto.EhSacoPromocional)
                {
                    html += "<div class='detalhe-item' style='border-left-color: #ffc107;'>";
                    html += "<strong><i class='fas fa-gift'></i> Saco Promocional</strong>";
                    html += "<div class='detalhe-item-valor'><span class='badge bg-warning'>Sim</span></div>";
                    html += "</div>";
                    html += "<div class='detalhe-item'>";
                    html += "<strong><i class='fas fa-shopping-bag'></i> Quantidade de Produtos no Saco</strong>";
                    html += "<div class='detalhe-item-valor'>" + produto.QuantidadeSaco + " produtos</div>";
                    html += "</div>";
                    
                    // Produtos permitidos
                    if (!string.IsNullOrEmpty(produto.Produtos))
                    {
                        try
                        {
                            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                            var produtosIds = serializer.Deserialize<List<int>>(produto.Produtos);
                            var produtosPermitidos = _databaseService.ObterProdutosPorIds(produtosIds);
                            
                            html += "<div class='detalhe-itens' style='background: white; border: 1px solid #dee2e6; border-radius: 8px; padding: 16px; margin-top: 12px;'>";
                            html += "<h6 class='mb-3'><i class='fas fa-list'></i> Produtos Permitidos no Saco</h6>";
                            html += "<ul class='list-group'>";
                            foreach (var prod in produtosPermitidos)
                            {
                                html += "<li class='list-group-item'>" + System.Web.HttpUtility.HtmlEncode(prod.Nome) + " - R$ " + prod.Preco.ToString("F2") + "</li>";
                            }
                            html += "</ul>";
                            html += "</div>";
                        }
                        catch
                        {
                            // Se falhar ao parsear, ignorar
                        }
                    }
                }
                
                html += "</div>";
                
                // Script para abrir modal e preencher
                string script = string.Format(@"
                    var modalBody = document.getElementById('modalDetalhesProdutoBody');
                    if (modalBody) {{
                        modalBody.innerHTML = {0};
                        var modalElement = document.getElementById('modalDetalhesProduto');
                        if (modalElement) {{
                            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {{
                                var modal = new bootstrap.Modal(modalElement);
                                modal.show();
                            }}
                        }}
                    }}", 
                    System.Web.HttpUtility.JavaScriptStringEncode(html));
                
                Page.ClientScript.RegisterStartupScript(this.GetType(), "MostrarDetalhesProduto", script, true);
            }
            catch (Exception ex)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                    string.Format("alert('Erro ao carregar detalhes: {0}');", EscapeJavaScript(ex.Message)), true);
            }
        }

    }
}

