using System;
using System.Web.UI;
using KingdomConfeitaria.Models;
using KingdomConfeitaria.Services;
using KingdomConfeitaria.Helpers;

namespace KingdomConfeitaria.paginas
{
    public partial class RecuperarSenha : BasePage
    {
        private DatabaseService _databaseService;
        private EmailService _emailService;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Configurar encoding UTF-8
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.Charset = "UTF-8";
            
            _databaseService = new DatabaseService();
            _emailService = new EmailService();

            // Verificar se j� est� logado
            if (Session["ClienteId"] != null)
            {
                Response.Redirect("../Default.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // Restaurar estado dos controles
                SessionStateHelper.RestaurarEstadoPagina(this);
            }
        }

        protected void btnEnviar_Click(object sender, EventArgs e)
        {
            try
            {
                string email = txtEmail != null ? txtEmail.Value.Trim() : "";

                if (string.IsNullOrEmpty(email))
                {
                    MostrarAlerta("Por favor, preencha o email.", "warning");
                    return;
                }

                // Formatar email
                email = email.ToLowerInvariant().Trim();

                // Buscar cliente por email
                Cliente cliente = _databaseService.ObterClientePorEmail(email);

                if (cliente == null)
                {
                    // Por seguran�a, n�o informar se o email existe ou n�o
                    MostrarAlerta("Se o email estiver cadastrado, voc� receber� um link para redefinir sua senha.", "info");
                    return;
                }

                // Gerar token de recupera��o
                _databaseService.GerarTokenRecuperacaoSenha(email);

                // Buscar cliente novamente para obter o token gerado
                cliente = _databaseService.ObterClientePorEmail(email);

                // Enviar email de recupera��o
                try
                {
                    _emailService.EnviarRecuperacaoSenha(cliente, cliente.TokenRecuperacaoSenha);
                    MostrarAlerta("Um email com o link para redefinir sua senha foi enviado. Verifique sua caixa de entrada.", "success");
                }
                catch
                {
                    MostrarAlerta("Erro ao enviar email. Por favor, tente novamente mais tarde.", "danger");
                }
            }
            catch (Exception ex)
            {
                MostrarAlerta("Erro ao processar solicita��o: " + ex.Message, "danger");
            }
        }

        private void MostrarAlerta(string mensagem, string tipo)
        {
            alertContainer.InnerHtml = string.Format(
                "<div class='alert alert-{0} alert-dismissible fade show' role='alert'>" +
                "{1}" +
                "<button type='button' class='btn-close' data-bs-dismiss='alert'></button>" +
                "</div>",
                tipo, System.Web.HttpUtility.HtmlEncode(mensagem)
            );
        }
    }
}

