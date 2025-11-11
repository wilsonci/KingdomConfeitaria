using System;
using System.Collections.Generic;
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
            clienteNome.InnerText = "Olá, " + (Session["ClienteNome"] != null ? Session["ClienteNome"].ToString() : "");

            // Processar ações
            string eventTarget = Request["__EVENTTARGET"];
            string eventArgument = Request["__EVENTARGUMENT"];

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
                var baseUrl = System.Configuration.ConfigurationManager.AppSettings["BaseUrl"] ?? Request.Url.GetLeftPart(UriPartial.Authority);

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
                        itensHtml += string.Format("<li>{0} ({1}) - Quantidade: {2} - R$ {3}</li>",
                            item.NomeProduto, item.Tamanho, item.Quantidade, item.Subtotal.ToString("F2"));
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

                    string linkReserva = string.Format("{0}/VerReserva.aspx?token={1}", baseUrl, reserva.TokenAcesso);
                    string textoCompartilhar = string.Format("Minha reserva na Kingdom Confeitaria - Valor: R$ {0} - Data de Retirada: {1}",
                        reserva.ValorTotal.ToString("F2"), reserva.DataRetirada.ToString("dd/MM/yyyy"));

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
                                    <a href='{10}' class='btn btn-primary btn-sm' target='_blank'>
                                        <i class='fas fa-eye'></i> Ver Detalhes
                                    </a>
                                    <button type='button' class='btn btn-danger btn-sm' onclick='excluirReserva({0})' {11}>
                                        <i class='fas fa-trash'></i> Excluir
                                    </button>
                                </div>
                                <div>
                                    <strong>Compartilhar:</strong>
                                    <button type='button' class='btn btn-share btn-sm btn-primary' onclick='compartilharFacebook(\""{10}\"", \""{12}\"")'>
                                        <i class='fab fa-facebook'></i>
                                    </button>
                                    <button type='button' class='btn btn-share btn-sm btn-success' onclick='compartilharWhatsApp(\""{10}\"", \""{12}\"")'>
                                        <i class='fab fa-whatsapp'></i>
                                    </button>
                                    <button type='button' class='btn btn-share btn-sm btn-info' onclick='compartilharTwitter(\""{10}\"", \""{12}\"")'>
                                        <i class='fab fa-twitter'></i>
                                    </button>
                                    <button type='button' class='btn btn-share btn-sm btn-secondary' onclick='compartilharEmail(\""{10}\"", \""{12}\"")'>
                                        <i class='fas fa-envelope'></i>
                                    </button>
                                </div>
                            </div>
                        </div>",
                        reserva.Id,
                        reserva.DataReserva.ToString("dd/MM/yyyy HH:mm"),
                        statusBadge,
                        reserva.DataRetirada.ToString("dd/MM/yyyy"),
                        reserva.ValorTotal.ToString("F2"),
                        previsaoHtml,
                        reserva.Email,
                        reserva.Telefone,
                        itensHtml,
                        observacoesHtml,
                        linkReserva,
                        reserva.Cancelado || reserva.Status == "Entregue" ? "disabled" : "",
                        textoCompartilhar.Replace("\"", "&quot;")
                    );
                }

                reservasContainer.InnerHtml = html;
            }
            catch (Exception ex)
            {
                reservasContainer.InnerHtml = string.Format("<div class='alert alert-danger'>Erro ao carregar reservas: {0}</div>", ex.Message);
            }
        }

        private void ExcluirReserva(int reservaId, int clienteId)
        {
            try
            {
                // Verificar se a reserva pertence ao cliente
                var reserva = _databaseService.ObterReservaPorId(reservaId);
                if (reserva != null && reserva.ClienteId == clienteId && !reserva.Cancelado && reserva.Status != "Entregue")
                {
                    _databaseService.ExcluirReserva(reservaId);
                    MostrarAlerta("Reserva excluída com sucesso!", "success");
                    CarregarReservas(clienteId);
                }
                else
                {
                    MostrarAlerta("Não foi possível excluir esta reserva.", "danger");
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

