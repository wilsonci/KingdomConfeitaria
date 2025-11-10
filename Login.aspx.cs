using System;
using System.Web.UI;
using KingdomConfeitaria.Models;
using KingdomConfeitaria.Services;

namespace KingdomConfeitaria
{
    public partial class Login : System.Web.UI.Page
    {
        private DatabaseService _databaseService;
        private EmailService _emailService;
        private WhatsAppService _whatsAppService;

        protected void Page_Load(object sender, EventArgs e)
        {
            _databaseService = new DatabaseService();
            _emailService = new EmailService();
            _whatsAppService = new WhatsAppService();

            // Verificar se já está logado
            if (Session["ClienteId"] != null)
            {
                Response.Redirect("MinhasReservas.aspx");
                return;
            }

            // Processar login social
            string eventTarget = Request["__EVENTTARGET"];
            string eventArgument = Request["__EVENTARGUMENT"];

            if (eventTarget == "LoginSocial" && !string.IsNullOrEmpty(eventArgument))
            {
                ProcessarLoginSocial(eventArgument);
            }

            // Processar login via WhatsApp
            string provider = Request.QueryString["provider"];
            string phone = Request.QueryString["phone"];
            if (provider == "WhatsApp" && !string.IsNullOrEmpty(phone))
            {
                ProcessarLoginWhatsApp(phone);
            }
        }

        private void ProcessarLoginSocial(string dados)
        {
            try
            {
                string[] partes = dados.Split('|');
                if (partes.Length >= 4)
                {
                    string provider = partes[0];
                    string providerId = partes[1];
                    string nome = partes[2];
                    string email = partes[3];

                    // Buscar ou criar cliente
                    var cliente = _databaseService.ObterClientePorProvider(provider, providerId);
                    if (cliente == null)
                    {
                        cliente = new Cliente
                        {
                            Nome = nome,
                            Email = email,
                            Provider = provider,
                            ProviderId = providerId,
                            EmailConfirmado = provider == "Google" || provider == "Facebook" // Confirmado automaticamente
                        };
                        cliente.Id = _databaseService.CriarOuAtualizarCliente(cliente);

                        // Enviar email de confirmação se necessário
                        if (!cliente.EmailConfirmado && !string.IsNullOrEmpty(email))
                        {
                            _emailService.EnviarConfirmacaoCadastro(cliente);
                        }
                    }

                    // Fazer login
                    Session["ClienteId"] = cliente.Id;
                    Session["ClienteNome"] = cliente.Nome;
                    Session["ClienteEmail"] = cliente.Email;

                    Response.Redirect("Default.aspx");
                }
            }
            catch (Exception ex)
            {
                MostrarAlerta("Erro ao processar login: " + ex.Message, "danger");
            }
        }

        private void ProcessarLoginWhatsApp(string telefone)
        {
            try
            {
                // Buscar cliente por telefone
                var clientes = _databaseService.ObterTodosClientes();
                Cliente cliente = null;
                if (clientes != null)
                {
                    cliente = clientes.Find(c => c.Telefone == telefone);
                }

                if (cliente == null)
                {
                    // Criar novo cliente
                    cliente = new Cliente
                    {
                        Nome = "Cliente WhatsApp",
                        Telefone = telefone,
                        Provider = "WhatsApp",
                        ProviderId = telefone,
                        TemWhatsApp = true,
                        WhatsAppConfirmado = true
                    };
                    cliente.Id = _databaseService.CriarOuAtualizarCliente(cliente);
                }

                Session["ClienteId"] = cliente.Id;
                Session["ClienteNome"] = cliente.Nome;
                Session["ClienteTelefone"] = cliente.Telefone;

                Response.Redirect("Default.aspx");
            }
            catch (Exception ex)
            {
                MostrarAlerta("Erro ao processar login WhatsApp: " + ex.Message, "danger");
            }
        }

        protected void btnContinuar_Click(object sender, EventArgs e)
        {
            try
            {
                string email = txtEmail.Value.Trim();
                string nome = txtNome.Value.Trim();
                string telefone = txtTelefone.Value.Trim();
                bool temWhatsApp = chkTemWhatsApp.Checked;

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(nome))
                {
                    MostrarAlerta("Por favor, preencha todos os campos obrigatórios.", "warning");
                    return;
                }

                // Buscar ou criar cliente
                var cliente = _databaseService.ObterClientePorEmail(email);
                if (cliente == null)
                {
                    cliente = new Cliente
                    {
                        Nome = nome,
                        Email = email,
                        Telefone = telefone,
                        TemWhatsApp = temWhatsApp,
                        Provider = "Email"
                    };
                    cliente.Id = _databaseService.CriarOuAtualizarCliente(cliente);

                    // Enviar email de confirmação
                    _emailService.EnviarConfirmacaoCadastro(cliente);

                    // Enviar WhatsApp se tiver
                    if (temWhatsApp && !string.IsNullOrEmpty(telefone))
                    {
                        _whatsAppService.EnviarConfirmacaoCadastro(cliente);
                    }

                    MostrarAlerta("Cadastro realizado! Verifique seu email para confirmar.", "success");
                }
                else
                {
                    // Atualizar dados
                    cliente.Nome = nome;
                    cliente.Telefone = telefone;
                    cliente.TemWhatsApp = temWhatsApp;
                    _databaseService.CriarOuAtualizarCliente(cliente);

                    // Fazer login
                    Session["ClienteId"] = cliente.Id;
                    Session["ClienteNome"] = cliente.Nome;
                    Session["ClienteEmail"] = cliente.Email;

                    Response.Redirect("Default.aspx");
                }
            }
            catch (Exception ex)
            {
                MostrarAlerta("Erro ao processar cadastro: " + ex.Message, "danger");
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

