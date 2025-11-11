using System;
using System.Web.UI;
using KingdomConfeitaria.Models;
using KingdomConfeitaria.Services;

namespace KingdomConfeitaria
{
    public partial class MeusDados : System.Web.UI.Page
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
                Response.Redirect("Login.aspx?returnUrl=" + Server.UrlEncode(Request.RawUrl));
                return;
            }

            if (!IsPostBack)
            {
                CarregarDadosCliente();
            }
        }

        private void CarregarDadosCliente()
        {
            try
            {
                int clienteId = (int)Session["ClienteId"];
                Cliente cliente = _databaseService.ObterClientePorId(clienteId);

                if (cliente != null)
                {
                    txtNome.Value = cliente.Nome;
                    txtEmail.Value = cliente.Email ?? "";
                    
                    if (!string.IsNullOrEmpty(cliente.Telefone))
                    {
                        // Formatar telefone
                        string tel = cliente.Telefone;
                        if (tel.Length <= 10)
                        {
                            tel = System.Text.RegularExpressions.Regex.Replace(tel, @"^(\d{2})(\d{4})(\d{0,4}).*", "($1) $2-$3");
                        }
                        else
                        {
                            tel = System.Text.RegularExpressions.Regex.Replace(tel, @"^(\d{2})(\d{5})(\d{0,4}).*", "($1) $2-$3");
                        }
                        txtTelefone.Value = tel;
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarAlerta("Erro ao carregar dados: " + ex.Message, "danger");
            }
        }

        protected void btnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                int clienteId = (int)Session["ClienteId"];
                Cliente cliente = _databaseService.ObterClientePorId(clienteId);

                if (cliente == null)
                {
                    MostrarAlerta("Cliente não encontrado.", "danger");
                    return;
                }

                string nome = txtNome.Value.Trim();
                string email = txtEmail.Value.Trim().ToLowerInvariant();
                string telefone = System.Text.RegularExpressions.Regex.Replace(txtTelefone.Value ?? "", @"[^\d]", "");
                string senhaAtual = txtSenhaAtual.Value ?? "";
                string novaSenha = txtNovaSenha.Value ?? "";
                string confirmarNovaSenha = txtConfirmarNovaSenha.Value ?? "";

                // Validações
                if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(email))
                {
                    MostrarAlerta("Nome e email são obrigatórios.", "warning");
                    return;
                }

                // Verificar se email foi alterado e se já existe
                if (email != cliente.Email)
                {
                    Cliente clienteExistente = _databaseService.ObterClientePorEmail(email);
                    if (clienteExistente != null && clienteExistente.Id != clienteId)
                    {
                        MostrarAlerta("Este email já está cadastrado por outro usuário.", "warning");
                        return;
                    }
                }

                // Verificar se telefone foi alterado e se já existe
                if (!string.IsNullOrEmpty(telefone) && telefone != cliente.Telefone)
                {
                    Cliente clientePorTelefone = _databaseService.ObterClientePorTelefone(telefone);
                    if (clientePorTelefone != null && clientePorTelefone.Id != clienteId)
                    {
                        MostrarAlerta("Este telefone já está cadastrado por outro usuário.", "warning");
                        return;
                    }
                }

                // Validar alteração de senha
                if (!string.IsNullOrEmpty(novaSenha))
                {
                    if (novaSenha.Length < 6)
                    {
                        MostrarAlerta("A nova senha deve ter no mínimo 6 caracteres.", "warning");
                        return;
                    }

                    if (novaSenha != confirmarNovaSenha)
                    {
                        MostrarAlerta("As senhas não coincidem.", "warning");
                        return;
                    }

                    // Verificar senha atual se o cliente tem senha cadastrada
                    if (!string.IsNullOrEmpty(cliente.Senha))
                    {
                        if (string.IsNullOrEmpty(senhaAtual))
                        {
                            MostrarAlerta("Por favor, informe sua senha atual para alterar a senha.", "warning");
                            return;
                        }

                        string hashSenhaAtual = HashSenha(senhaAtual);
                        if (hashSenhaAtual != cliente.Senha)
                        {
                            MostrarAlerta("Senha atual incorreta.", "danger");
                            return;
                        }
                    }

                    // Atualizar senha
                    cliente.Senha = HashSenha(novaSenha);
                }

                // Atualizar dados
                cliente.Nome = nome;
                cliente.Email = email;
                cliente.Telefone = telefone;
                cliente.TemWhatsApp = !string.IsNullOrEmpty(telefone);

                _databaseService.CriarOuAtualizarCliente(cliente);

                // Atualizar sessão
                Session["ClienteNome"] = cliente.Nome;
                Session["ClienteEmail"] = cliente.Email;
                if (!string.IsNullOrEmpty(cliente.Telefone))
                {
                    Session["ClienteTelefone"] = cliente.Telefone;
                }

                MostrarAlerta("Dados atualizados com sucesso!", "success");
            }
            catch (Exception ex)
            {
                MostrarAlerta("Erro ao salvar dados: " + ex.Message, "danger");
            }
        }

        private string HashSenha(string senha)
        {
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
                "<button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button>" +
                "</div>",
                tipo,
                System.Web.HttpUtility.HtmlEncode(mensagem)
            );
        }
    }
}

