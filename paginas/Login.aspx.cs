using System;
using System.Linq;
using System.Web.UI;
using System.Web.Security;
using KingdomConfeitaria.Models;
using KingdomConfeitaria.Services;
using KingdomConfeitaria.Helpers;

namespace KingdomConfeitaria.paginas
{
    public partial class Login : BasePage
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
            
            _databaseService = new DatabaseService();

            // Verificar se j� est� logado
            if (Session["ClienteId"] != null)
            {
                Response.Redirect("MinhasReservas.aspx");
                return;
            }
        }

        protected void btnEntrar_Click(object sender, EventArgs e)
        {
            try
            {
                string email = txtEmail != null ? txtEmail.Value.Trim() : "";
                string senha = txtSenha != null ? txtSenha.Value : "";

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha))
                {
                    MostrarAlerta("Por favor, preencha email e senha.", "warning");
                    return;
                }

                // Formatar email
                email = email.ToLowerInvariant().Trim();

                // Buscar cliente por email
                Cliente cliente = _databaseService.ObterClientePorEmail(email);

                if (cliente == null)
                {
                    MostrarAlerta("Email ou senha incorretos.", "danger");
                    return;
                }

                // Verificar senha
                if (string.IsNullOrEmpty(cliente.Senha))
                {
                    // Cliente n�o tem senha cadastrada (cadastro antigo)
                    MostrarAlerta("Este email n�o possui senha cadastrada. Por favor, cadastre-se novamente.", "warning");
                    return;
                }

                // Verificar hash da senha
                if (!VerificarSenha(senha, cliente.Senha))
                {
                    MostrarAlerta("Email ou senha incorretos.", "danger");
                    return;
                }

                // Login bem-sucedido
                Session["ClienteId"] = cliente.Id;
                Session["ClienteNome"] = cliente.Nome;
                Session["ClienteEmail"] = cliente.Email;
                Session["IsAdmin"] = cliente.IsAdmin;
                if (!string.IsNullOrEmpty(cliente.Telefone))
                {
                    Session["ClienteTelefone"] = cliente.Telefone;
                }
                
                // Configurar timeout padr�o da sess�o (20 minutos)
                Session.Timeout = 20;
                
                // Marcar in�cio da sess�o
                Session["SessionStartTime"] = DateTime.Now;

                // Atualizar �ltimo acesso
                cliente.UltimoAcesso = DateTime.Now;
                _databaseService.CriarOuAtualizarCliente(cliente);
                
                // Registrar log de login (usar informa��es do cliente diretamente para garantir que funcione)
                string usuarioLog = $"{cliente.Nome}{(cliente.IsAdmin ? " (Admin)" : "")} - {cliente.Email} [ID: {cliente.Id}]";
                LogService.RegistrarLogin(usuarioLog, "Login.aspx", $"Email: {email}");

                // Verificar se h� URL de retorno
                string returnUrl = Request.QueryString["returnUrl"];
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    Response.Redirect(returnUrl);
                }
                else
                {
                    // Redirecionar para Default.aspx com par�metro para abrir modal de reserva
                    Response.Redirect("../Default.aspx?abrirReserva=true");
                }
            }
            catch (Exception ex)
            {
                MostrarAlerta("Erro ao fazer login: " + ex.Message, "danger");
            }
        }

        private string HashSenha(string senha)
        {
            // Usar SHA256 para hash da senha
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(senha));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerificarSenha(string senha, string hashArmazenado)
        {
            string hashSenha = HashSenha(senha);
            return hashSenha == hashArmazenado;
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
