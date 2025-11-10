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

        protected void Page_Load(object sender, EventArgs e)
        {
            _databaseService = new DatabaseService();

            if (!IsPostBack)
            {
                CarregarProdutos();
                CarregarReservas();
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
                ScriptManager.RegisterStartupScript(this, GetType(), "FecharModal", 
                    "var modal = bootstrap.Modal.getInstance(document.getElementById('modalEditarProduto')); modal.hide();", true);
                ScriptManager.RegisterStartupScript(this, GetType(), "Sucesso", 
                    "alert('Produto atualizado com sucesso!');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "Erro", 
                    string.Format("alert('Erro ao salvar produto: {0}');", ex.Message.Replace("'", "\\'")), true);
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
                ScriptManager.RegisterStartupScript(this, GetType(), "FecharModal", 
                    "var modal = bootstrap.Modal.getInstance(document.getElementById('modalNovoProduto')); modal.hide();", true);
                ScriptManager.RegisterStartupScript(this, GetType(), "Sucesso", 
                    "alert('Produto adicionado com sucesso!');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "Erro", 
                    string.Format("alert('Erro ao adicionar produto: {0}');", ex.Message.Replace("'", "\\'")), true);
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
                    ScriptManager.RegisterStartupScript(this, GetType(), "FecharModal", 
                        "var modal = bootstrap.Modal.getInstance(document.getElementById('modalEditarReserva')); modal.hide();", true);
                    ScriptManager.RegisterStartupScript(this, GetType(), "Sucesso", 
                        "alert('Reserva atualizada com sucesso!');", true);
                }
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "Erro", 
                    string.Format("alert('Erro ao salvar reserva: {0}');", ex.Message.Replace("'", "\\'")), true);
            }
        }
    }
}

