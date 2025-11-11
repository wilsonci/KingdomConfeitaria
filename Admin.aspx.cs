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

            if (!IsPostBack)
            {
                CarregarResumo();
                CarregarProdutos();
                CarregarReservas();
            }
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
                    string precoPequenoStr = produto.PrecoPequeno.ToString("F2").Replace(",", ".");
                    string precoGrandeStr = produto.PrecoGrande.ToString("F2").Replace(",", ".");
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
                                        <strong>Pequeno:</strong> R$ {4} | 
                                        <strong>Grande:</strong> R$ {5}
                                    </p>
                                    <p><small class='text-muted'>Ordem: {6}</small></p>
                                </div>
                                <div class='col-md-2 text-end'>
                                    <button type='button' class='btn btn-primary btn-sm mb-2' 
                                        onclick='editarProduto({7}, ""{8}"", ""{9}"", {10}, {11}, ""{0}"", {6}, {12})'>
                                        <i class='fas fa-edit'></i> Editar
                                    </button>
                                </div>
                            </div>
                        </div>",
                        produto.ImagemUrl,
                        produto.Nome,
                        statusBadge,
                        produto.Descricao,
                        produto.PrecoPequeno.ToString("F2"),
                        produto.PrecoGrande.ToString("F2"),
                        produto.Ordem,
                        produto.Id,
                        nomeEscapado,
                        descricaoEscapada,
                        precoPequenoStr,
                        precoGrandeStr,
                        produto.Ativo.ToString().ToLower());
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
                var produto = new Produto
                {
                    Id = produtoId,
                    Nome = txtNomeProduto.Text,
                    Descricao = txtDescricao.Text,
                    PrecoPequeno = decimal.Parse(txtPrecoPequeno.Text),
                    PrecoGrande = decimal.Parse(txtPrecoGrande.Text),
                    ImagemUrl = txtImagemUrl.Text,
                    Ordem = int.Parse(txtOrdem.Text),
                    Ativo = chkAtivo.Checked
                };

                _databaseService.AtualizarProduto(produto);

                CarregarProdutos();
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
                var novoProduto = new Produto
                {
                    Nome = txtNovoNome.Text,
                    Descricao = txtNovaDescricao.Text,
                    PrecoPequeno = decimal.Parse(txtNovoPrecoPequeno.Text),
                    PrecoGrande = decimal.Parse(txtNovoPrecoGrande.Text),
                    ImagemUrl = txtNovaImagemUrl.Text,
                    Ativo = true,
                    Ordem = int.Parse(txtNovaOrdem.Text ?? "0")
                };

                _databaseService.AdicionarProduto(novoProduto);

                // Limpar campos
                txtNovoNome.Text = "";
                txtNovaDescricao.Text = "";
                txtNovoPrecoPequeno.Text = "";
                txtNovoPrecoGrande.Text = "";
                txtNovaImagemUrl.Text = "";
                txtNovaOrdem.Text = "0";

                CarregarProdutos();
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

        protected void btnLimparDados_Click(object sender, EventArgs e)
        {
            try
            {
                _databaseService.LimparTodosClientesEReservas();
                
                // Recarregar dados
                CarregarResumo();
                CarregarProdutos();
                CarregarReservas();
                
                // Mostrar mensagem de sucesso
                alertContainer.InnerHtml = "<div class='alert alert-success alert-dismissible fade show' role='alert'>" +
                    "Todos os clientes e reservas foram removidos com sucesso!" +
                    "<button type='button' class='btn-close' data-bs-dismiss='alert'></button>" +
                    "</div>";
            }
            catch (Exception ex)
            {
                alertContainer.InnerHtml = "<div class='alert alert-danger alert-dismissible fade show' role='alert'>" +
                    "Erro ao limpar dados: " + System.Web.HttpUtility.HtmlEncode(ex.Message) +
                    "<button type='button' class='btn-close' data-bs-dismiss='alert'></button>" +
                    "</div>";
            }
        }
    }
}

