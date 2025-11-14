using System;
using System.Web.UI;
using KingdomConfeitaria.Models;
using KingdomConfeitaria.Services;
using KingdomConfeitaria.Security;

namespace KingdomConfeitaria
{
    public partial class Cadastro : System.Web.UI.Page
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
                Response.Redirect("MinhasReservas.aspx");
                return;
            }
            
            // Preencher email ou telefone se vier via query string
            if (!IsPostBack)
            {
                // Validar e sanitizar email para prevenir XSS
                string email = Security.InputValidator.SanitizeString(Request.QueryString["email"] ?? "", 254);
                if (!string.IsNullOrEmpty(email) && !Security.InputValidator.IsValidEmail(email))
                {
                    email = string.Empty; // Invalidar se não for um email válido
                }
                string telefone = Request.QueryString["telefone"];
                
                if (!string.IsNullOrEmpty(email) && txtEmail != null)
                {
                    txtEmail.Value = email;
                }
                
                if (!string.IsNullOrEmpty(telefone) && txtTelefone != null)
                {
                    // Formatar telefone
                    string telFormatado = System.Text.RegularExpressions.Regex.Replace(telefone, @"[^\d]", "");
                    if (telFormatado.Length <= 10)
                    {
                        telFormatado = System.Text.RegularExpressions.Regex.Replace(telFormatado, @"^(\d{2})(\d{4})(\d{0,4}).*", "($1) $2-$3");
                    }
                    else
                    {
                        telFormatado = System.Text.RegularExpressions.Regex.Replace(telFormatado, @"^(\d{2})(\d{5})(\d{0,4}).*", "($1) $2-$3");
                    }
                    txtTelefone.Value = telFormatado;
                }
            }
        }

        protected void btnCadastrar_Click(object sender, EventArgs e)
        {
            try
            {
                string nome = txtNome != null ? txtNome.Value.Trim() : "";
                string email = txtEmail != null ? txtEmail.Value.Trim() : "";
                string senha = txtSenha != null ? txtSenha.Value : "";
                string confirmarSenha = txtConfirmarSenha != null ? txtConfirmarSenha.Value : "";
                string telefone = txtTelefone != null ? txtTelefone.Value.Trim() : "";

                // Validações
                if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha))
                {
                    MostrarAlerta("Por favor, preencha todos os campos obrigatórios.", "warning");
                    return;
                }

                if (senha.Length < 6)
                {
                    MostrarAlerta("A senha deve ter no mínimo 6 caracteres.", "warning");
                    return;
                }

                if (senha != confirmarSenha)
                {
                    MostrarAlerta("As senhas não coincidem.", "warning");
                    return;
                }

                // Formatar email
                email = email.ToLowerInvariant().Trim();

                // Formatar telefone
                telefone = System.Text.RegularExpressions.Regex.Replace(telefone, @"[^\d]", "");

                // Verificar se email já existe
                Cliente clienteExistente = _databaseService.ObterClientePorEmail(email);
                if (clienteExistente != null)
                {
                    MostrarAlerta("Este email já está cadastrado. Por favor, faça login.", "warning");
                    return;
                }
                
                // Verificar se telefone já existe
                if (!string.IsNullOrEmpty(telefone))
                {
                    Cliente clientePorTelefone = _databaseService.ObterClientePorTelefone(telefone);
                    if (clientePorTelefone != null)
                    {
                        MostrarAlerta("Este telefone já está cadastrado. Por favor, faça login.", "warning");
                        return;
                    }
                }

                // Hash da senha
                string senhaHash = HashSenha(senha);

                // Verificar se o email é de administrador
                string[] emailsAdmin = { "wilson2071@gmail.com", "isanfm@gmail.com", "camilafermagalhaes@gmail.com" };
                bool isAdmin = false;
                string emailLower = email.ToLowerInvariant().Trim();
                isAdmin = Array.Exists(emailsAdmin, emailAdmin => emailAdmin.ToLowerInvariant().Trim() == emailLower);
                
                // Criar novo cliente
                Cliente cliente = new Cliente
                {
                    Nome = nome,
                    Email = email,
                    Senha = senhaHash,
                    Telefone = telefone,
                    TemWhatsApp = !string.IsNullOrEmpty(telefone),
                    IsAdmin = isAdmin, // Definir IsAdmin baseado na lista de emails de administradores
                    DataCadastro = DateTime.Now
                };

                cliente.Id = _databaseService.CriarOuAtualizarCliente(cliente);

                // Registrar log de cadastro
                LogService.RegistrarInsercao(
                    $"Cliente ID: {cliente.Id}",
                    "Cliente",
                    "Cadastro.aspx",
                    $"Nome: {cliente.Nome}, Email: {cliente.Email}, Telefone: {cliente.Telefone ?? "N/A"}, IsAdmin: {cliente.IsAdmin}"
                );

                // Enviar email de confirmação (não bloquear se falhar)
                try
                {
                    _emailService.EnviarConfirmacaoCadastro(cliente);
                }
                catch
                {
                    // Erro ao enviar email - não bloquear o processo
                }

                // Buscar cliente atualizado para obter IsAdmin correto
                cliente = _databaseService.ObterClientePorId(cliente.Id);
                
                // Fazer login após cadastro
                Session["ClienteId"] = cliente.Id;
                Session["ClienteNome"] = cliente.Nome;
                Session["ClienteEmail"] = cliente.Email;
                Session["IsAdmin"] = cliente != null ? cliente.IsAdmin : false;
                if (!string.IsNullOrEmpty(cliente.Telefone))
                {
                    Session["ClienteTelefone"] = cliente.Telefone;
                }

                // Registrar log de login após cadastro
                string usuarioLog = LogService.ObterUsuarioAtual(Session);
                LogService.RegistrarLogin(usuarioLog, "Cadastro.aspx", $"Login automático após cadastro - Email: {cliente.Email}");

                Response.Redirect("Default.aspx");
            }
            catch (Exception ex)
            {
                MostrarAlerta("Erro ao processar cadastro: " + ex.Message, "danger");
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

