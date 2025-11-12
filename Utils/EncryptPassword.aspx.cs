using System;
using System.Web.UI;
using KingdomConfeitaria.Security;

namespace KingdomConfeitaria
{
    public partial class EncryptPassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs eventArgs)
        {
            // Verificar se está em ambiente de desenvolvimento (opcional)
            // Em produção, você pode adicionar uma verificação de autenticação aqui
        }

        protected void btnCriptografar_Click(object sender, EventArgs eventArgs)
        {
            try
            {
                string senha = txtSenha.Text.Trim();
                
                if (string.IsNullOrEmpty(senha))
                {
                    MostrarErro("Por favor, digite a senha que deseja criptografar.");
                    return;
                }

                // Criptografar a senha
                string senhaCriptografada = PasswordEncryption.Encrypt(senha);
                
                // Exibir resultado
                txtCriptografado.Text = senhaCriptografada;
                litConfig.Text = senhaCriptografada;
                
                divResultado.Visible = true;
                divErro.Visible = false;
                
                // Limpar campo de senha por segurança
                txtSenha.Text = "";
            }
            catch (Exception ex)
            {
                // Usar a variável ex para log do erro
                string mensagemErro = "Erro ao criptografar senha: " + ex.Message;
                MostrarErro(mensagemErro);
            }
        }

        private void MostrarErro(string mensagem)
        {
            litErro.Text = mensagem;
            divErro.Visible = true;
            divResultado.Visible = false;
        }
    }
}

