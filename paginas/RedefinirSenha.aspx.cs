using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using KingdomConfeitaria.Models;
using KingdomConfeitaria.Services;
using KingdomConfeitaria.Helpers;

namespace KingdomConfeitaria.paginas
{
    public partial class RedefinirSenha : BasePage
    {
        private DatabaseService _databaseService;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Configurar encoding UTF-8
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.Charset = "UTF-8";
            
            _databaseService = new DatabaseService();

            // Verificar se j� est� logado
            if (Session["ClienteId"] != null)
            {
                Response.Redirect("../Default.aspx");
                return;
            }

            // Verificar se h� token na URL
            string token = Request.QueryString["token"];
            if (string.IsNullOrEmpty(token))
            {
                MostrarAlerta("Token inv�lido ou ausente. Por favor, solicite uma nova recupera��o de senha.", "danger");
                btnRedefinir.Enabled = false;
                return;
            }

            // Verificar se o token � v�lido
            Cliente cliente = _databaseService.ObterClientePorTokenRecuperacaoSenha(token);
            if (cliente == null)
            {
                MostrarAlerta("Token inv�lido ou expirado. Por favor, solicite uma nova recupera��o de senha.", "danger");
                btnRedefinir.Enabled = false;
                return;
            }

            // Armazenar token na Session para usar no postback (ViewState desabilitado)
            Session["TokenRedefinirSenha"] = token;

            if (!IsPostBack)
            {
                // Restaurar estado dos controles
                SessionStateHelper.RestaurarEstadoPagina(this);
            }
        }

        protected void btnRedefinir_Click(object sender, EventArgs e)
        {
            try
            {
                string token = Session["TokenRedefinirSenha"] != null ? Session["TokenRedefinirSenha"].ToString() : Request.QueryString["token"];

                if (string.IsNullOrEmpty(token))
                {
                    MostrarAlerta("Token inv�lido ou ausente.", "danger");
                    return;
                }

                string novaSenha = txtNovaSenha != null ? txtNovaSenha.Value : "";
                string confirmarSenha = txtConfirmarSenha != null ? txtConfirmarSenha.Value : "";

                if (string.IsNullOrEmpty(novaSenha) || string.IsNullOrEmpty(confirmarSenha))
                {
                    MostrarAlerta("Por favor, preencha ambos os campos de senha.", "warning");
                    return;
                }

                if (novaSenha.Length < 6)
                {
                    MostrarAlerta("A senha deve ter no m�nimo 6 caracteres.", "warning");
                    return;
                }

                if (novaSenha != confirmarSenha)
                {
                    MostrarAlerta("As senhas n�o coincidem.", "warning");
                    return;
                }

                // Verificar se o token ainda � v�lido
                Cliente cliente = _databaseService.ObterClientePorTokenRecuperacaoSenha(token);
                if (cliente == null)
                {
                    MostrarAlerta("Token inv�lido ou expirado. Por favor, solicite uma nova recupera��o de senha.", "danger");
                    return;
                }

                // Hash da nova senha
                string novaSenhaHash = HashSenha(novaSenha);

                // Redefinir senha
                _databaseService.RedefinirSenha(token, novaSenhaHash);

                MostrarAlerta("Senha redefinida com sucesso! Voc� pode fazer login agora.", "success");
                btnRedefinir.Enabled = false;
                
                // Redirecionar para login ap�s 3 segundos
                Page.ClientScript.RegisterStartupScript(this.GetType(), "RedirecionarLogin", 
                    "setTimeout(function() { window.location.href='Login.aspx'; }, 3000);", true);
            }
            catch (Exception ex)
            {
                MostrarAlerta("Erro ao redefinir senha: " + ex.Message, "danger");
            }
        }

        private string HashSenha(string senha)
        {
            // Usar SHA256 para hash da senha (mesmo m�todo usado no Login)
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
                return Convert.ToBase64String(hashedBytes);
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

