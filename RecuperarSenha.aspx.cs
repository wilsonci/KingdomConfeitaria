using System;
using System.Web.UI;
using KingdomConfeitaria.Models;
using KingdomConfeitaria.Services;

namespace KingdomConfeitaria
{
    public partial class RecuperarSenha : System.Web.UI.Page
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

            // Verificar se já está logado
            if (Session["ClienteId"] != null)
            {
                Response.Redirect("Default.aspx");
                return;
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
                    // Por segurança, não informar se o email existe ou não
                    MostrarAlerta("Se o email estiver cadastrado, você receberá um link para redefinir sua senha.", "info");
                    return;
                }

                // Gerar token de recuperação
                _databaseService.GerarTokenRecuperacaoSenha(email);

                // Buscar cliente novamente para obter o token gerado
                cliente = _databaseService.ObterClientePorEmail(email);

                // Enviar email de recuperação
                try
                {
                    _emailService.EnviarRecuperacaoSenha(cliente, cliente.TokenRecuperacaoSenha);
                    MostrarAlerta("Um email com o link para redefinir sua senha foi enviado. Verifique sua caixa de entrada.", "success");
                }
                catch (Exception exEmail)
                {
                    System.Diagnostics.Debug.WriteLine("Erro ao enviar email de recuperação: " + exEmail.Message);
                    MostrarAlerta("Erro ao enviar email. Por favor, tente novamente mais tarde.", "danger");
                }
            }
            catch (Exception ex)
            {
                MostrarAlerta("Erro ao processar solicitação: " + ex.Message, "danger");
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

