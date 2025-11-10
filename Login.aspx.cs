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

            // Preencher campos do modal de completar cadastro se houver dados na sessão
            if (Session["LoginSocial_Nome"] != null && txtNomeCompletar != null)
            {
                string nomeSessao = Session["LoginSocial_Nome"].ToString();
                if (!string.IsNullOrEmpty(nomeSessao) && 
                    nomeSessao != "Usuário Facebook" && 
                    nomeSessao != "Usuário Google")
                {
                    txtNomeCompletar.Text = nomeSessao;
                }
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
                if (string.IsNullOrEmpty(dados))
                {
                    MostrarAlerta("Dados de login inválidos.", "danger");
                    return;
                }

                string[] partes = dados.Split('|');
                if (partes.Length >= 4)
                {
                    string provider = partes[0];
                    string providerId = partes[1];
                    string nome = partes[2];
                    string email = partes[3];

                    // Validar dados básicos
                    if (string.IsNullOrEmpty(provider) || string.IsNullOrEmpty(providerId))
                    {
                        MostrarAlerta("Dados incompletos do provedor de login.", "danger");
                        return;
                    }

                    // Formatar email
                    if (!string.IsNullOrEmpty(email))
                    {
                        email = email.Trim().ToLowerInvariant();
                    }

                    // Buscar cliente existente
                    var cliente = _databaseService.ObterClientePorProvider(provider, providerId);
                    
                    // Se não encontrou por Provider, tentar por email
                    if (cliente == null && !string.IsNullOrEmpty(email))
                    {
                        cliente = _databaseService.ObterClientePorEmail(email);
                    }

                    // Se cliente existe, verificar se tem nome e telefone
                    if (cliente != null)
                    {
                        bool precisaCompletar = false;
                        if (string.IsNullOrWhiteSpace(cliente.Nome) || cliente.Nome == "Usuário Facebook" || cliente.Nome == "Usuário Google")
                        {
                            precisaCompletar = true;
                        }
                        if (string.IsNullOrWhiteSpace(cliente.Telefone))
                        {
                            precisaCompletar = true;
                        }

                        if (precisaCompletar)
                        {
                            // Armazenar dados temporários na sessão para completar depois
                            Session["LoginSocial_Provider"] = provider;
                            Session["LoginSocial_ProviderId"] = providerId;
                            Session["LoginSocial_Email"] = email;
                            Session["LoginSocial_Nome"] = nome;
                            Session["LoginSocial_ClienteId"] = cliente.Id;
                            
                            // Mostrar modal para completar dados
                            ScriptManager.RegisterStartupScript(this, GetType(), "MostrarModalCompletar", 
                                "var modal = new bootstrap.Modal(document.getElementById('modalCompletarCadastro')); modal.show();", true);
                            return;
                        }

                        // Atualizar dados se necessário
                        bool atualizado = false;
                        if (!string.IsNullOrEmpty(email) && cliente.Email != email)
                        {
                            cliente.Email = email;
                            atualizado = true;
                        }
                        if (!string.IsNullOrWhiteSpace(nome) && cliente.Nome != nome && 
                            nome != "Usuário Facebook" && nome != "Usuário Google")
                        {
                            cliente.Nome = nome;
                            atualizado = true;
                        }
                        if (atualizado)
                        {
                            _databaseService.CriarOuAtualizarCliente(cliente);
                        }

                        // Fazer login
                        Session["ClienteId"] = cliente.Id;
                        Session["ClienteNome"] = cliente.Nome;
                        Session["ClienteEmail"] = cliente.Email;
                        if (!string.IsNullOrEmpty(cliente.Telefone))
                        {
                            Session["ClienteTelefone"] = cliente.Telefone;
                        }

                        Response.Redirect("Default.aspx");
                        return;
                    }

                    // Cliente novo - verificar se tem nome e telefone
                    bool nomeValido = !string.IsNullOrWhiteSpace(nome) && 
                                     nome != "Usuário Facebook" && 
                                     nome != "Usuário Google";
                    
                    if (!nomeValido || string.IsNullOrEmpty(partes.Length > 4 ? partes[4] : ""))
                    {
                        // Armazenar dados temporários na sessão
                        Session["LoginSocial_Provider"] = provider;
                        Session["LoginSocial_ProviderId"] = providerId;
                        Session["LoginSocial_Email"] = email;
                        Session["LoginSocial_Nome"] = nome;
                        
                        // Mostrar modal para completar dados
                        ScriptManager.RegisterStartupScript(this, GetType(), "MostrarModalCompletar", 
                            "var modal = new bootstrap.Modal(document.getElementById('modalCompletarCadastro')); modal.show();", true);
                        return;
                    }

                    // Tem todos os dados, criar cliente
                    string telefone = partes.Length > 4 ? partes[4] : "";
                    telefone = System.Text.RegularExpressions.Regex.Replace(telefone, @"[^\d]", "");

                    cliente = new Cliente
                    {
                        Nome = nome,
                        Email = email,
                        Telefone = telefone,
                        Provider = provider,
                        ProviderId = providerId,
                        EmailConfirmado = provider == "Google" || provider == "Facebook",
                        TemWhatsApp = !string.IsNullOrEmpty(telefone),
                        DataCadastro = DateTime.Now
                    };
                    cliente.Id = _databaseService.CriarOuAtualizarCliente(cliente);

                    // Enviar email de confirmação se necessário
                    if (!cliente.EmailConfirmado && !string.IsNullOrEmpty(email))
                    {
                        try
                        {
                            _emailService.EnviarConfirmacaoCadastro(cliente);
                        }
                        catch
                        {
                            // Não bloquear se email falhar
                        }
                    }

                    // Fazer login
                    Session["ClienteId"] = cliente.Id;
                    Session["ClienteNome"] = cliente.Nome;
                    Session["ClienteEmail"] = cliente.Email;
                    if (!string.IsNullOrEmpty(cliente.Telefone))
                    {
                        Session["ClienteTelefone"] = cliente.Telefone;
                    }

                    Response.Redirect("Default.aspx");
                }
                else
                {
                    MostrarAlerta("Formato de dados inválido.", "danger");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao processar login social: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack trace: " + ex.StackTrace);
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
                        Email = "", // Email vazio para WhatsApp
                        Telefone = telefone,
                        Provider = "WhatsApp",
                        ProviderId = telefone,
                        TemWhatsApp = true,
                        WhatsAppConfirmado = true,
                        EmailConfirmado = false,
                        DataCadastro = DateTime.Now
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
                string email = txtEmail != null ? txtEmail.Value.Trim() : "";
                string nome = txtNome != null ? txtNome.Value.Trim() : "";
                string telefone = txtTelefone != null ? txtTelefone.Value.Trim() : "";
                bool temWhatsApp = chkTemWhatsApp.Checked;

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(nome))
                {
                    MostrarAlerta("Por favor, preencha todos os campos obrigatórios.", "warning");
                    return;
                }

                // Formatar email e telefone
                email = email.ToLowerInvariant().Trim();
                telefone = System.Text.RegularExpressions.Regex.Replace(telefone, @"[^\d]", "");

                // Verificar se cliente já existe por email OU telefone
                Cliente cliente = null;
                if (!string.IsNullOrEmpty(email))
                {
                    cliente = _databaseService.ObterClientePorEmail(email);
                }
                if (cliente == null && !string.IsNullOrEmpty(telefone))
                {
                    cliente = _databaseService.ObterClientePorTelefone(telefone);
                }

                if (cliente == null)
                {
                    // Cliente não existe, criar novo
                    cliente = new Cliente
                    {
                        Nome = nome,
                        Email = email,
                        Telefone = telefone,
                        TemWhatsApp = temWhatsApp,
                        Provider = "Email",
                        DataCadastro = DateTime.Now
                    };
                    cliente.Id = _databaseService.CriarOuAtualizarCliente(cliente);

                    // Enviar email de confirmação (não bloquear se falhar)
                    try
                    {
                        _emailService.EnviarConfirmacaoCadastro(cliente);
                    }
                    catch (Exception exEmail)
                    {
                        System.Diagnostics.Debug.WriteLine("Erro ao enviar email de confirmação: " + exEmail.Message);
                        // Continuar mesmo se email falhar
                    }

                    // Enviar WhatsApp se tiver (não bloquear se falhar)
                    if (temWhatsApp && !string.IsNullOrEmpty(telefone))
                    {
                        try
                        {
                            _whatsAppService.EnviarConfirmacaoCadastro(cliente);
                        }
                        catch (Exception exWhatsApp)
                        {
                            System.Diagnostics.Debug.WriteLine("Erro ao enviar WhatsApp: " + exWhatsApp.Message);
                            // Continuar mesmo se WhatsApp falhar
                        }
                    }

                    MostrarAlerta("Cadastro realizado com sucesso! " + 
                        (!string.IsNullOrEmpty(email) ? "Verifique seu email para confirmar." : ""), "success");
                }
                else
                {
                    // Cliente já existe - atualizar dados e fazer login
                    cliente.Nome = nome;
                    cliente.Telefone = telefone;
                    cliente.TemWhatsApp = temWhatsApp;
                    // Garantir que email está formatado
                    if (!string.IsNullOrEmpty(email))
                    {
                        cliente.Email = email;
                    }
                    _databaseService.CriarOuAtualizarCliente(cliente);

                    // Fazer login
                    Session["ClienteId"] = cliente.Id;
                    Session["ClienteNome"] = cliente.Nome;
                    Session["ClienteEmail"] = cliente.Email;
                    if (!string.IsNullOrEmpty(cliente.Telefone))
                    {
                        Session["ClienteTelefone"] = cliente.Telefone;
                    }

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

