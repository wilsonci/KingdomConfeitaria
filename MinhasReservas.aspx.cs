using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using KingdomConfeitaria.Models;
using KingdomConfeitaria.Services;

namespace KingdomConfeitaria
{
    public partial class MinhasReservas : System.Web.UI.Page
    {
        private DatabaseService _databaseService;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Configurar encoding UTF-8
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.Charset = "UTF-8";
            
            _databaseService = new DatabaseService();

            // Verificar se está logado
            if (Session["ClienteId"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            int clienteId = (int)Session["ClienteId"];
            
            // Atualizar menu
            clienteNome.InnerText = "Olá, " + (Session["ClienteNome"] != null ? Session["ClienteNome"].ToString() : "");
            clienteNome.Style["display"] = "inline";
            
            // Atualizar visibilidade dos links do menu
            var linkLogin = FindControl("linkLogin") as System.Web.UI.HtmlControls.HtmlAnchor;
            var linkMinhasReservas = FindControl("linkMinhasReservas") as System.Web.UI.HtmlControls.HtmlAnchor;
            var linkMeusDados = FindControl("linkMeusDados") as System.Web.UI.HtmlControls.HtmlAnchor;
            var linkAdmin = FindControl("linkAdmin") as System.Web.UI.HtmlControls.HtmlAnchor;
            var linkLogout = FindControl("linkLogout") as System.Web.UI.HtmlControls.HtmlAnchor;
            
            if (linkLogin != null) linkLogin.Style["display"] = "none";
            if (linkMinhasReservas != null) linkMinhasReservas.Style["display"] = "inline";
            if (linkMeusDados != null) linkMeusDados.Style["display"] = "inline";
            if (linkLogout != null) linkLogout.Style["display"] = "inline";
            
            // Verificar se é admin
            bool isAdmin = Session["IsAdmin"] != null && (bool)Session["IsAdmin"];
            if (linkAdmin != null) linkAdmin.Style["display"] = isAdmin ? "inline" : "none";

            // Processar ações
            string eventTarget = Request["__EVENTTARGET"];
            string eventArgument = Request["__EVENTARGUMENT"];

            if (eventTarget == "CancelarReserva" && !string.IsNullOrEmpty(eventArgument))
            {
                int reservaId;
                if (int.TryParse(eventArgument, out reservaId))
                {
                    CancelarReserva(reservaId, clienteId);
                }
            }

            if (eventTarget == "ExcluirReserva" && !string.IsNullOrEmpty(eventArgument))
            {
                int reservaId;
                if (int.TryParse(eventArgument, out reservaId))
                {
                    ExcluirReserva(reservaId, clienteId);
                }
            }

            if (!IsPostBack)
            {
                CarregarReservas(clienteId);
            }
        }

        private void CarregarReservas(int clienteId)
        {
            try
            {
                var reservas = _databaseService.ObterReservasPorCliente(clienteId);
                // Usar a URL atual da requisição para garantir que a porta esteja correta
                var baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);

                if (reservas == null || reservas.Count == 0)
                {
                    reservasContainer.InnerHtml = "<div class='alert alert-info'><i class='fas fa-info-circle'></i> Você ainda não possui reservas. <a href='Default.aspx'>Faça sua primeira reserva!</a></div>";
                    return;
                }

                string html = "";
                foreach (var reserva in reservas)
                {
                    string statusClass = "status-" + reserva.Status.ToLower().Replace(" ", "-");
                    string statusBadge = string.Format("<span class='status-badge {0}'>{1}</span>", statusClass, reserva.Status);
                    
                    string itensHtml = "";
                    foreach (var item in reserva.Itens)
                    {
                        string itemTexto = string.Format("{0} ({1}) - Quantidade: {2} - R$ {3}",
                            System.Web.HttpUtility.HtmlEncode(item.NomeProduto), 
                            System.Web.HttpUtility.HtmlEncode(item.Tamanho), 
                            item.Quantidade, 
                            item.Subtotal.ToString("F2"));
                        
                        // Se tiver produtos do saco, mostrar detalhes
                        if (!string.IsNullOrEmpty(item.Produtos))
                        {
                            try
                            {
                                var todosProdutos = _databaseService.ObterTodosProdutos();
                                var produtosDetalhes = new List<string>();
                                
                                // Parsear JSON de produtos no formato [{"qt": quantidade, "id": idProduto}, ...]
                                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                                var produtosJson = serializer.Deserialize<List<Dictionary<string, object>>>(item.Produtos);
                                
                                foreach (var produtoJson in produtosJson)
                                {
                                    int qt = Convert.ToInt32(produtoJson["qt"]);
                                    int prodId = Convert.ToInt32(produtoJson["id"]);
                                    var produto = todosProdutos.Where(p => p.Id == prodId).FirstOrDefault();
                                    if (produto != null)
                                    {
                                        produtosDetalhes.Add($"{qt}x {produto.Nome}");
                                    }
                                }
                                
                                if (produtosDetalhes.Count > 0)
                                {
                                    itemTexto += "<br/><small class='text-muted'>Produtos: " + string.Join(", ", produtosDetalhes) + "</small>";
                                }
                            }
                            catch
                            {
                                // Se falhar ao parsear, ignorar
                            }
                        }
                        
                        itensHtml += "<li>" + itemTexto + "</li>";
                    }

                    string previsaoHtml = "";
                    if (reserva.PrevisaoEntrega.HasValue)
                    {
                        previsaoHtml = string.Format("<p><strong>Previsão de Entrega:</strong> {0}</p>",
                            reserva.PrevisaoEntrega.Value.ToString("dd/MM/yyyy"));
                    }

                    string observacoesHtml = "";
                    if (!string.IsNullOrEmpty(reserva.Observacoes))
                    {
                        observacoesHtml = string.Format("<p><strong>Observações:</strong> {0}</p>", reserva.Observacoes);
                    }

                    // Verificar se TokenAcesso existe, se não, gerar um
                    string token = reserva.TokenAcesso;
                    if (string.IsNullOrEmpty(token))
                    {
                        // Se não tem token, usar o ID da reserva como fallback (não ideal, mas funcional)
                        token = reserva.Id.ToString();
                    }
                    
                    // Usar URL relativa para o link (evita problemas com porta/domínio)
                    string linkReserva = ResolveUrl("~/VerReserva.aspx?token=" + System.Web.HttpUtility.UrlEncode(token));
                    
                    // URL completa para compartilhamento (precisa ser absoluta)
                    string linkReservaCompleto = baseUrl.TrimEnd('/') + "/VerReserva.aspx?token=" + System.Web.HttpUtility.UrlEncode(token);
                    
                    string textoCompartilhar = string.Format("Minha reserva na Kingdom Confeitaria - Valor: R$ {0} - Data de Retirada: {1}",
                        reserva.ValorTotal.ToString("F2"), reserva.DataRetirada.ToString("dd/MM/yyyy"));

                    // Verificar se a reserva está cancelada (verificando status "Cancelado")
                    var statusCancelado = _databaseService.ObterStatusReservaPorNome("Cancelado");
                    bool jaCancelada = reserva.Cancelado || (reserva.StatusId.HasValue && statusCancelado != null && reserva.StatusId.Value == statusCancelado.Id);
                    
                    // Determinar se pode cancelar
                    bool podeCancelar = !jaCancelada && reserva.StatusId.HasValue && _databaseService.StatusPermiteExclusao(reserva.StatusId.Value);
                    string botaoCancelar = podeCancelar 
                        ? string.Format("<button type='button' class='btn btn-warning btn-sm' onclick='cancelarReserva({0})'><i class='fas fa-times-circle'></i> Cancelar</button>", reserva.Id)
                        : "";
                    
                    // Determinar se pode excluir (apenas para administradores)
                    bool isAdmin = Session["IsAdmin"] != null && (bool)Session["IsAdmin"];
                    bool podeExcluir = isAdmin && !jaCancelada && reserva.StatusId.HasValue && _databaseService.StatusPermiteExclusao(reserva.StatusId.Value);
                    string botaoExcluir = podeExcluir
                        ? string.Format("<button type='button' class='btn btn-danger btn-sm' onclick='excluirReserva({0})'><i class='fas fa-trash'></i> Excluir</button>", reserva.Id)
                        : "";
                    
                    // Botões de compartilhamento (apenas se não estiver cancelada)
                    string botoesCompartilharHtml = "";
                    if (!jaCancelada)
                    {
                        botoesCompartilharHtml = string.Format(@"
                                <div>
                                    <strong>Compartilhar:</strong>
                                    <button type='button' class='btn btn-share btn-sm btn-primary' onclick='compartilharFacebook(\""{0}\"", \""{1}\"")'>
                                        <i class='fab fa-facebook'></i>
                                    </button>
                                    <button type='button' class='btn btn-share btn-sm btn-success' onclick='compartilharWhatsApp(\""{0}\"", \""{1}\"")'>
                                        <i class='fab fa-whatsapp'></i>
                                    </button>
                                    <button type='button' class='btn btn-share btn-sm btn-info' onclick='compartilharTwitter(\""{0}\"", \""{1}\"")'>
                                        <i class='fab fa-twitter'></i>
                                    </button>
                                    <button type='button' class='btn btn-share btn-sm btn-secondary' onclick='compartilharEmail(\""{0}\"", \""{1}\"")'>
                                        <i class='fas fa-envelope'></i>
                                    </button>
                                </div>",
                                linkReservaCompleto,
                                textoCompartilhar.Replace("\"", "&quot;"));
                    }

                    html += string.Format(@"
                        <div class='reserva-card'>
                            <div class='d-flex justify-content-between align-items-start mb-3'>
                                <div>
                                    <h4>Reserva #{0}</h4>
                                    <p class='text-muted mb-0'>Data da Reserva: {1}</p>
                                </div>
                                <div>
                                    {2}
                                </div>
                            </div>
                            
                            <div class='row mb-3'>
                                <div class='col-md-6'>
                                    <p><strong>Data de Retirada:</strong> {3}</p>
                                    <p><strong>Valor Total:</strong> R$ {4}</p>
                                    {5}
                                </div>
                                <div class='col-md-6'>
                                    <p><strong>Email:</strong> {6}</p>
                                    <p><strong>Telefone:</strong> {7}</p>
                                </div>
                            </div>
                            
                            <div class='mb-3'>
                                <strong>Itens:</strong>
                                <ul>
                                    {8}
                                </ul>
                            </div>
                            
                            {9}
                            
                            <div class='d-flex justify-content-between align-items-center mt-3'>
                                <div>
                                    <a href='{10}' class='btn btn-primary btn-sm'>
                                        <i class='fas fa-eye'></i> Ver Detalhes
                                    </a>
                                    {14}
                                    {15}
                                </div>
                                {16}
                            </div>
                        </div>",
                        reserva.Id,
                        reserva.DataReserva.ToString("dd/MM/yyyy HH:mm"),
                        statusBadge,
                        reserva.DataRetirada.ToString("dd/MM/yyyy"),
                        reserva.ValorTotal.ToString("F2"),
                        previsaoHtml,
                        !string.IsNullOrEmpty(reserva.Email) ? System.Web.HttpUtility.HtmlEncode(reserva.Email) : "Não informado",
                        !string.IsNullOrEmpty(reserva.Telefone) ? System.Web.HttpUtility.HtmlEncode(reserva.Telefone) : "Não informado",
                        itensHtml,
                        observacoesHtml,
                        linkReserva, // URL relativa para o link "Ver Detalhes" - {10}
                        "", // {11} - removido (era disabled do botão excluir)
                        textoCompartilhar.Replace("\"", "&quot;"), // {12}
                        linkReservaCompleto, // {13} - URL completa para compartilhamento
                        botaoCancelar, // {14} - Botão de cancelar
                        botaoExcluir, // {15} - Botão de excluir
                        botoesCompartilharHtml // {16} - Botões de compartilhamento (vazio se cancelada)
                    );
                }

                reservasContainer.InnerHtml = html;
            }
            catch (Exception ex)
            {
                reservasContainer.InnerHtml = string.Format("<div class='alert alert-danger'>Erro ao carregar reservas: {0}</div>", ex.Message);
            }
        }

        private void CancelarReserva(int reservaId, int clienteId)
        {
            try
            {
                // Verificar se a reserva pertence ao cliente
                var reserva = _databaseService.ObterReservaPorId(reservaId);
                
                // Verificar se a reserva já está cancelada (verificando status "Cancelado")
                var statusCancelado = _databaseService.ObterStatusReservaPorNome("Cancelado");
                bool jaCancelada = reserva != null && 
                                   (reserva.Cancelado || 
                                    (reserva.StatusId.HasValue && statusCancelado != null && reserva.StatusId.Value == statusCancelado.Id));
                
                // Verificar se pode cancelar (usando StatusId para verificar PermiteExclusao)
                bool podeCancelar = reserva != null && 
                                  reserva.ClienteId == clienteId && 
                                  !jaCancelada && 
                                  reserva.StatusId.HasValue &&
                                  _databaseService.StatusPermiteExclusao(reserva.StatusId.Value);
                
                if (podeCancelar)
                {
                    _databaseService.CancelarReserva(reservaId);
                    MostrarAlerta("Reserva cancelada com sucesso!", "success");
                    CarregarReservas(clienteId);
                }
                else
                {
                    string motivo = "Não foi possível cancelar a reserva.";
                    if (reserva != null)
                    {
                        if (jaCancelada)
                        {
                            motivo = "Esta reserva já foi cancelada.";
                        }
                        else if (reserva.StatusId.HasValue && !_databaseService.StatusPermiteExclusao(reserva.StatusId.Value))
                        {
                            motivo = "Esta reserva não pode ser cancelada pois já está em " + reserva.Status + ".";
                        }
                    }
                    MostrarAlerta(motivo, "danger");
                }
            }
            catch (Exception ex)
            {
                MostrarAlerta("Erro ao cancelar reserva: " + ex.Message, "danger");
            }
        }

        private void ExcluirReserva(int reservaId, int clienteId)
        {
            try
            {
                // Verificar se o usuário é administrador
                bool isAdmin = Session["IsAdmin"] != null && (bool)Session["IsAdmin"];
                if (!isAdmin)
                {
                    MostrarAlerta("Apenas administradores podem excluir reservas.", "danger");
                    return;
                }

                // Verificar se a reserva existe
                var reserva = _databaseService.ObterReservaPorId(reservaId);
                // Verificar se pode excluir (usando StatusId para verificar PermiteExclusao)
                bool podeExcluir = reserva != null && 
                                  !reserva.Cancelado && 
                                  reserva.StatusId.HasValue &&
                                  _databaseService.StatusPermiteExclusao(reserva.StatusId.Value);
                
                if (podeExcluir)
                {
                    _databaseService.ExcluirReserva(reservaId);
                    MostrarAlerta("Reserva excluída com sucesso!", "success");
                    CarregarReservas(clienteId);
                }
                else
                {
                    string motivo = "Não foi possível excluir a reserva.";
                    if (reserva != null)
                    {
                        if (reserva.StatusId.HasValue && !_databaseService.StatusPermiteExclusao(reserva.StatusId.Value))
                        {
                            motivo = "Esta reserva não pode ser excluída pois já está em " + reserva.Status + ".";
                        }
                        else if (reserva.Cancelado)
                        {
                            motivo = "Esta reserva já foi cancelada.";
                        }
                    }
                    MostrarAlerta(motivo, "danger");
                }
            }
            catch (Exception ex)
            {
                MostrarAlerta("Erro ao excluir reserva: " + ex.Message, "danger");
            }
        }

        private void MostrarAlerta(string mensagem, string tipo)
        {
            alertContainer.InnerHtml = string.Format(
                "<div class='alert alert-{0} alert-dismissible fade show' role='alert'>" +
                "{1}" +
                "<button type='button' class='btn-close' data-bs-dismiss='alert'></button>" +
                "</div>",
                tipo, mensagem
            );
        }
    }
}

