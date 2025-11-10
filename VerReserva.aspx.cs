using System;
using System.Web.UI;
using KingdomConfeitaria.Services;

namespace KingdomConfeitaria
{
    public partial class VerReserva : System.Web.UI.Page
    {
        private DatabaseService _databaseService;

        protected void Page_Load(object sender, EventArgs e)
        {
            _databaseService = new DatabaseService();

            string token = Request.QueryString["token"];

            if (string.IsNullOrEmpty(token))
            {
                MostrarErro("Token de acesso não fornecido.");
                return;
            }

            try
            {
                var reserva = _databaseService.ObterReservaPorToken(token);

                if (reserva == null)
                {
                    MostrarErro("Reserva não encontrada ou token inválido.");
                    return;
                }

                // Verificar se cliente está logado e fazer login automático se necessário
                VerificarEAutenticarCliente(reserva);

                // Exibir detalhes da reserva
                ExibirReserva(reserva);
            }
            catch (Exception ex)
            {
                MostrarErro("Erro ao carregar reserva: " + ex.Message);
            }
        }

        private void VerificarEAutenticarCliente(Models.Reserva reserva)
        {
            // Se a reserva tem cliente associado e não está logado, fazer login automático
            if (reserva.ClienteId.HasValue && Session["ClienteId"] == null)
            {
                var cliente = _databaseService.ObterClientePorEmail(reserva.Email);
                if (cliente != null && cliente.Id == reserva.ClienteId.Value)
                {
                    Session["ClienteId"] = cliente.Id;
                    Session["ClienteNome"] = cliente.Nome;
                    Session["ClienteEmail"] = cliente.Email;
                }
            }

            // Atualizar header
            if (Session["ClienteId"] != null)
            {
                clienteNome.InnerText = "Olá, " + (Session["ClienteNome"] != null ? Session["ClienteNome"].ToString() : "");
                linkLogin.Visible = false;
                linkMinhasReservas.Visible = true;
                linkLogout.Visible = true;
            }
            else
            {
                clienteNome.InnerText = "";
                linkLogin.Visible = true;
                linkMinhasReservas.Visible = false;
                linkLogout.Visible = false;
            }
        }

        private void ExibirReserva(Models.Reserva reserva)
        {
            var baseUrl = System.Configuration.ConfigurationManager.AppSettings["BaseUrl"] ?? Request.Url.GetLeftPart(UriPartial.Authority);
            string linkReserva = Request.Url.AbsoluteUri;
            
            string statusClass = "status-" + reserva.Status.ToLower().Replace(" ", "-");
            string statusBadge = string.Format("<span class='status-badge {0}'>{1}</span>", statusClass, reserva.Status);

            string itensHtml = "";
            foreach (var item in reserva.Itens)
            {
                itensHtml += string.Format("<li class='mb-2'>{0} ({1}) - Quantidade: {2} - R$ {3}</li>",
                    item.NomeProduto, item.Tamanho, item.Quantidade, item.Subtotal.ToString("F2"));
            }

            string previsaoHtml = "";
            if (reserva.PrevisaoEntrega.HasValue)
            {
                previsaoHtml = string.Format("<div class='alert alert-info'><i class='fas fa-calendar'></i> <strong>Previsão de Entrega:</strong> {0}</div>",
                    reserva.PrevisaoEntrega.Value.ToString("dd/MM/yyyy"));
            }

            string observacoesHtml = "";
            if (!string.IsNullOrEmpty(reserva.Observacoes))
            {
                observacoesHtml = string.Format("<div class='alert alert-secondary'><strong>Observações:</strong> {0}</div>", reserva.Observacoes);
            }

            string textoCompartilhar = string.Format("Minha reserva na Kingdom Confeitaria - Valor: R$ {0} - Data de Retirada: {1}",
                reserva.ValorTotal.ToString("F2"), reserva.DataRetirada.ToString("dd/MM/yyyy"));

            string acoesHtml = "";
            if (Session["ClienteId"] != null)
            {
                acoesHtml = "<div class='mt-3'><a href='MinhasReservas.aspx' class='btn btn-primary'><i class='fas fa-list'></i> Ver Todas Minhas Reservas</a></div>";
            }
            else
            {
                acoesHtml = "<div class='mt-3'><a href='Login.aspx' class='btn btn-primary'><i class='fas fa-sign-in-alt'></i> Fazer Login para Gerenciar Reservas</a></div>";
            }

            conteudoContainer.InnerHtml = string.Format(@"
                <h2 class='mb-4'><i class='fas fa-receipt'></i> Detalhes da Reserva #{0}</h2>
                
                <div class='reserva-detalhes'>
                    <div class='d-flex justify-content-between align-items-start mb-4'>
                        <div>
                            <h4>Reserva #{0}</h4>
                            <p class='text-muted mb-0'>Data da Reserva: {1}</p>
                        </div>
                        <div>
                            {2}
                        </div>
                    </div>
                    
                    <div class='row mb-4'>
                        <div class='col-md-6'>
                            <h5><i class='fas fa-calendar-alt'></i> Informações da Reserva</h5>
                            <p><strong>Data de Retirada:</strong> {3}</p>
                            <p><strong>Valor Total:</strong> <span class='h4 text-success'>R$ {4}</span></p>
                            {5}
                        </div>
                        <div class='col-md-6'>
                            <h5><i class='fas fa-user'></i> Dados do Cliente</h5>
                            <p><strong>Nome:</strong> {6}</p>
                            <p><strong>Email:</strong> {7}</p>
                            <p><strong>Telefone:</strong> {8}</p>
                        </div>
                    </div>
                    
                    <div class='mb-4'>
                        <h5><i class='fas fa-shopping-bag'></i> Itens da Reserva</h5>
                        <ul class='list-unstyled'>
                            {9}
                        </ul>
                    </div>
                    
                    {10}
                    
                    <div class='border-top pt-4 mt-4'>
                        <h5><i class='fas fa-share-alt'></i> Compartilhar Reserva</h5>
                        <p>Compartilhe sua reserva nas redes sociais:</p>
                        <div>
                            <button type='button' class='btn btn-share btn-primary' onclick='compartilharFacebook(\""{11}\"", \""{12}\"")'>
                                <i class='fab fa-facebook'></i> Facebook
                            </button>
                            <button type='button' class='btn btn-share btn-success' onclick='compartilharWhatsApp(\""{11}\"", \""{12}\"")'>
                                <i class='fab fa-whatsapp'></i> WhatsApp
                            </button>
                            <button type='button' class='btn btn-share btn-info' onclick='compartilharTwitter(\""{11}\"", \""{12}\"")'>
                                <i class='fab fa-twitter'></i> Twitter
                            </button>
                            <button type='button' class='btn btn-share btn-secondary' onclick='compartilharEmail(\""{11}\"", \""{12}\"")'>
                                <i class='fas fa-envelope'></i> Email
                            </button>
                        </div>
                    </div>
                    
                    {13}
                </div>",
                reserva.Id,
                reserva.DataReserva.ToString("dd/MM/yyyy HH:mm"),
                statusBadge,
                reserva.DataRetirada.ToString("dd/MM/yyyy"),
                reserva.ValorTotal.ToString("F2"),
                previsaoHtml,
                reserva.Nome,
                reserva.Email,
                reserva.Telefone,
                itensHtml,
                observacoesHtml,
                linkReserva,
                textoCompartilhar.Replace("\"", "&quot;"),
                acoesHtml
            );
        }

        private void MostrarErro(string mensagem)
        {
            conteudoContainer.InnerHtml = string.Format(@"
                <div class='alert alert-danger'>
                    <h4><i class='fas fa-exclamation-triangle'></i> Erro</h4>
                    <p>{0}</p>
                    <div class='mt-3'>
                        <a href='Default.aspx' class='btn btn-primary'>Voltar para Página Inicial</a>
                    </div>
                </div>",
                mensagem
            );
        }
    }
}

