using System;
using System.Web.UI;
using KingdomConfeitaria.Services;

namespace KingdomConfeitaria
{
    public partial class ConfirmarCadastro : System.Web.UI.Page
    {
        private DatabaseService _databaseService;

        protected void Page_Load(object sender, EventArgs e)
        {
            _databaseService = new DatabaseService();

            string token = Request.QueryString["token"];

            if (string.IsNullOrEmpty(token))
            {
                MostrarErro("Token de confirmação não fornecido.");
                return;
            }

            try
            {
                var cliente = _databaseService.ObterClientePorToken(token);

                if (cliente == null)
                {
                    MostrarErro("Token de confirmação inválido ou expirado.");
                    return;
                }

                if (cliente.EmailConfirmado)
                {
                    MostrarSucesso("Seu email já estava confirmado!", cliente);
                    return;
                }

                // Confirmar email
                _databaseService.ConfirmarEmailCliente(token);

                // Fazer login automático
                Session["ClienteId"] = cliente.Id;
                Session["ClienteNome"] = cliente.Nome;
                Session["ClienteEmail"] = cliente.Email;

                MostrarSucesso("Email confirmado com sucesso! Você já está logado.", cliente);
            }
            catch (Exception ex)
            {
                MostrarErro("Erro ao confirmar cadastro: " + ex.Message);
            }
        }

        private void MostrarSucesso(string mensagem, Models.Cliente cliente)
        {
            conteudoContainer.InnerHtml = string.Format(@"
                <div class='success-icon'>
                    <i class='fas fa-check-circle'></i>
                </div>
                <h2 class='text-success mb-3'>Cadastro Confirmado!</h2>
                <p class='mb-4'>{0}</p>
                <p class='mb-4'><strong>Bem-vindo, {1}!</strong></p>
                <div class='d-grid gap-2'>
                    <a href='Default.aspx' class='btn btn-success btn-lg'>
                        <i class='fas fa-shopping-cart'></i> Fazer Minha Primeira Reserva
                    </a>
                    <a href='MinhasReservas.aspx' class='btn btn-outline-primary'>
                        <i class='fas fa-list'></i> Minhas Reservas
                    </a>
                </div>",
                mensagem,
                cliente.Nome
            );
        }

        private void MostrarErro(string mensagem)
        {
            conteudoContainer.InnerHtml = string.Format(@"
                <div class='error-icon'>
                    <i class='fas fa-times-circle'></i>
                </div>
                <h2 class='text-danger mb-3'>Erro na Confirmação</h2>
                <p class='mb-4'>{0}</p>
                <div class='d-grid gap-2'>
                    <a href='Login.aspx' class='btn btn-primary btn-lg'>
                        <i class='fas fa-sign-in-alt'></i> Ir para Login
                    </a>
                    <a href='Default.aspx' class='btn btn-outline-secondary'>
                        <i class='fas fa-home'></i> Voltar para Página Inicial
                    </a>
                </div>",
                mensagem
            );
        }
    }
}

