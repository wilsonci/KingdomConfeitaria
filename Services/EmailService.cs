using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using KingdomConfeitaria.Models;
using KingdomConfeitaria.Security;

namespace KingdomConfeitaria.Services
{
    public class EmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _emailIsabela;
        private readonly string _emailCamila;
        private readonly string _emailFrom;

        public EmailService()
        {
            _smtpServer = ConfigurationManager.AppSettings["SmtpServer"];
            var smtpPortStr = ConfigurationManager.AppSettings["SmtpPort"];
            _smtpPort = !string.IsNullOrEmpty(smtpPortStr) ? int.Parse(smtpPortStr) : 587;
            _smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"];
            
            // Descriptografar senha se estiver criptografada
            string senhaCriptografada = ConfigurationManager.AppSettings["SmtpPassword"] ?? "";
            try
            {
                _smtpPassword = PasswordEncryption.Decrypt(senhaCriptografada);
            }
            catch
            {
                // Se falhar ao descriptografar, usar a senha como está (pode não estar criptografada)
                _smtpPassword = senhaCriptografada;
            }
            
            _emailIsabela = ConfigurationManager.AppSettings["EmailIsabela"];
            _emailCamila = ConfigurationManager.AppSettings["EmailCamila"];
            _emailFrom = ConfigurationManager.AppSettings["EmailFrom"];
        }

        public void EnviarConfirmacaoReserva(Reserva reserva)
        {
            // Buscar dados do cliente se não estiverem preenchidos na reserva
            if (reserva.ClienteId.HasValue && (string.IsNullOrEmpty(reserva.Nome) || string.IsNullOrEmpty(reserva.Email)))
            {
                try
                {
                    var databaseService = new DatabaseService();
                    var clienteData = databaseService.ObterClientePorId(reserva.ClienteId.Value);
                    if (clienteData != null)
                    {
                        reserva.Nome = clienteData.Nome;
                        reserva.Email = clienteData.Email;
                        reserva.Telefone = clienteData.Telefone;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Erro ao buscar dados do cliente para email: " + ex.Message);
                    // Se não conseguir buscar o cliente, continuar com os dados que já existem
                }
            }
            
            // Garantir que o Status está preenchido
            if (string.IsNullOrEmpty(reserva.Status))
            {
                reserva.Status = "Aberta";
            }
            
            // Email para as filhas (Isabela e Camila)
            try
            {
                EnviarEmailParaFilhas(reserva);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao enviar email para filhas: " + ex.Message);
            }
            
            // Email para o cliente
            try
            {
                EnviarEmailParaCliente(reserva);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao enviar email para cliente: " + ex.Message);
            }
        }

        private void EnviarEmailParaFilhas(Reserva reserva)
        {
            string nomeCliente = !string.IsNullOrEmpty(reserva.Nome) ? reserva.Nome : "Cliente não identificado";
            string emailCliente = !string.IsNullOrEmpty(reserva.Email) ? reserva.Email : "Não informado";
            string telefoneCliente = !string.IsNullOrEmpty(reserva.Telefone) ? reserva.Telefone : "Não informado";
            
            var assunto = string.Format("Nova Reserva - {0}", nomeCliente);
            var corpo = new StringBuilder();
            corpo.AppendLine("<h2>Nova Reserva Recebida!</h2>");
            corpo.AppendLine(string.Format("<p><strong>Nome:</strong> {0}</p>", System.Web.HttpUtility.HtmlEncode(nomeCliente)));
            corpo.AppendLine(string.Format("<p><strong>Email:</strong> {0}</p>", System.Web.HttpUtility.HtmlEncode(emailCliente)));
            corpo.AppendLine(string.Format("<p><strong>Telefone:</strong> {0}</p>", System.Web.HttpUtility.HtmlEncode(telefoneCliente)));
            corpo.AppendLine(string.Format("<p><strong>Data de Retirada:</strong> {0}</p>", reserva.DataRetirada.ToString("dd/MM/yyyy")));
            corpo.AppendLine(string.Format("<p><strong>Valor Total:</strong> R$ {0}</p>", reserva.ValorTotal.ToString("F2")));
            corpo.AppendLine("<h3>Itens do Pedido:</h3>");
            corpo.AppendLine("<ul>");
            foreach (var item in reserva.Itens)
            {
                corpo.AppendLine(string.Format("<li>{0} ({1}) - Quantidade: {2} - R$ {3}</li>", 
                    item.NomeProduto, item.Tamanho, item.Quantidade, item.Subtotal.ToString("F2")));
            }
            corpo.AppendLine("</ul>");
            if (!string.IsNullOrEmpty(reserva.Observacoes))
            {
                corpo.AppendLine(string.Format("<p><strong>Observações:</strong> {0}</p>", reserva.Observacoes));
            }

            // Enviar email para Isabela
            if (!string.IsNullOrEmpty(_emailIsabela))
            {
                EnviarEmail(_emailIsabela, assunto, corpo.ToString());
            }
            
            // Enviar email para Camila apenas se for diferente do email de Isabela
            if (!string.IsNullOrEmpty(_emailCamila) && 
                !string.IsNullOrEmpty(_emailIsabela) && 
                _emailCamila.Trim().ToLower() != _emailIsabela.Trim().ToLower())
            {
                EnviarEmail(_emailCamila, assunto, corpo.ToString());
            }
        }

        private void EnviarEmailParaCliente(Reserva reserva)
        {
            // Verificar se tem email para enviar
            if (string.IsNullOrEmpty(reserva.Email))
            {
                return; // Não enviar email se não tiver endereço de email
            }
            
            string nomeCliente = !string.IsNullOrEmpty(reserva.Nome) ? reserva.Nome : "Cliente";
            
            var assunto = "Confirmação de Reserva - Kingdom Confeitaria";
            var baseUrl = ConfigurationManager.AppSettings["BaseUrl"] ?? "http://localhost";
            var linkReserva = string.Format("{0}/VerReserva.aspx?token={1}", baseUrl, reserva.TokenAcesso);
            
            var corpo = new StringBuilder();
            corpo.AppendLine(string.Format("<h2>Olá {0}!</h2>", System.Web.HttpUtility.HtmlEncode(nomeCliente)));
            corpo.AppendLine("<p>Sua reserva foi recebida com sucesso!</p>");
            corpo.AppendLine(string.Format("<p><strong>Data de Retirada:</strong> {0}</p>", reserva.DataRetirada.ToString("dd/MM/yyyy")));
            corpo.AppendLine(string.Format("<p><strong>Valor Total:</strong> R$ {0}</p>", reserva.ValorTotal.ToString("F2")));
            corpo.AppendLine("<h3>Seu Pedido:</h3>");
            corpo.AppendLine("<ul>");
            foreach (var item in reserva.Itens)
            {
                corpo.AppendLine(string.Format("<li>{0} ({1}) - Quantidade: {2} - R$ {3}</li>", 
                    item.NomeProduto, item.Tamanho, item.Quantidade, item.Subtotal.ToString("F2")));
            }
            corpo.AppendLine("</ul>");
            string statusExibicao = !string.IsNullOrEmpty(reserva.Status) ? reserva.Status : "Aberta";
            corpo.AppendLine(string.Format("<p><strong>Status:</strong> {0}</p>", statusExibicao));
            corpo.AppendLine(string.Format("<p><a href='{0}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 10px 0;'>Ver Minha Reserva</a></p>", linkReserva));
            corpo.AppendLine("<p>Você receberá atualizações sobre o status da sua reserva por email.</p>");
            corpo.AppendLine("<p>Obrigado por escolher a Kingdom Confeitaria!</p>");

            EnviarEmail(reserva.Email, assunto, corpo.ToString());
        }

        public void EnviarConfirmacaoCadastro(Cliente cliente)
        {
            var assunto = "Confirme seu cadastro - Kingdom Confeitaria";
            var baseUrl = ConfigurationManager.AppSettings["BaseUrl"] ?? "http://localhost";
            var linkConfirmacao = string.Format("{0}/ConfirmarCadastro.aspx?token={1}", baseUrl, cliente.TokenConfirmacao);
            
            var corpo = new StringBuilder();
            corpo.AppendLine(string.Format("<h2>Olá {0}!</h2>", cliente.Nome));
            corpo.AppendLine("<p>Bem-vindo à Kingdom Confeitaria!</p>");
            corpo.AppendLine("<p>Por favor, confirme seu cadastro clicando no link abaixo:</p>");
            corpo.AppendLine(string.Format("<p><a href='{0}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 10px 0;'>Confirmar Cadastro</a></p>", linkConfirmacao));
            corpo.AppendLine(string.Format("<p>Ou copie e cole este link no seu navegador:</p><p>{0}</p>", linkConfirmacao));
            corpo.AppendLine("<p>Obrigado!</p>");

            EnviarEmail(cliente.Email, assunto, corpo.ToString());
        }

        public void EnviarRecuperacaoSenha(Cliente cliente, string token)
        {
            var assunto = "Recuperação de Senha - Kingdom Confeitaria";
            var baseUrl = ConfigurationManager.AppSettings["BaseUrl"] ?? "http://localhost";
            var linkRecuperacao = string.Format("{0}/RedefinirSenha.aspx?token={1}", baseUrl, token);
            
            var corpo = new StringBuilder();
            corpo.AppendLine(string.Format("<h2>Olá {0}!</h2>", cliente.Nome));
            corpo.AppendLine("<p>Recebemos uma solicitação para redefinir sua senha.</p>");
            corpo.AppendLine("<p>Clique no link abaixo para criar uma nova senha:</p>");
            corpo.AppendLine(string.Format("<p><a href='{0}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 10px 0;'>Redefinir Senha</a></p>", linkRecuperacao));
            corpo.AppendLine(string.Format("<p>Ou copie e cole este link no seu navegador:</p><p>{0}</p>", linkRecuperacao));
            corpo.AppendLine("<p><strong>Este link expira em 24 horas.</strong></p>");
            corpo.AppendLine("<p>Se você não solicitou esta recuperação de senha, ignore este email.</p>");
            corpo.AppendLine("<p>Obrigado!</p>");

            EnviarEmail(cliente.Email, assunto, corpo.ToString());
        }

        public void EnviarEmailReservaCancelada(Reserva reserva)
        {
            // Buscar dados do cliente se não estiverem preenchidos
            if (reserva.ClienteId.HasValue && (string.IsNullOrEmpty(reserva.Nome) || string.IsNullOrEmpty(reserva.Email)))
            {
                try
                {
                    var databaseService = new DatabaseService();
                    var clienteData = databaseService.ObterClientePorId(reserva.ClienteId.Value);
                    if (clienteData != null)
                    {
                        reserva.Nome = clienteData.Nome;
                        reserva.Email = clienteData.Email;
                        reserva.Telefone = clienteData.Telefone;
                    }
                }
                catch
                {
                    // Se não conseguir buscar o cliente, continuar com os dados que já existem
                }
            }

            // Email para as filhas
            try
            {
                string nomeCliente = !string.IsNullOrEmpty(reserva.Nome) ? reserva.Nome : "Cliente não identificado";
                string emailCliente = !string.IsNullOrEmpty(reserva.Email) ? reserva.Email : "Não informado";
                string telefoneCliente = !string.IsNullOrEmpty(reserva.Telefone) ? reserva.Telefone : "Não informado";
                
                var assunto = string.Format("Reserva Cancelada - {0}", nomeCliente);
                var corpo = new StringBuilder();
                corpo.AppendLine("<h2>Reserva Cancelada</h2>");
                corpo.AppendLine(string.Format("<p><strong>Nome:</strong> {0}</p>", System.Web.HttpUtility.HtmlEncode(nomeCliente)));
                corpo.AppendLine(string.Format("<p><strong>Email:</strong> {0}</p>", System.Web.HttpUtility.HtmlEncode(emailCliente)));
                corpo.AppendLine(string.Format("<p><strong>Telefone:</strong> {0}</p>", System.Web.HttpUtility.HtmlEncode(telefoneCliente)));
                corpo.AppendLine(string.Format("<p><strong>Data de Retirada:</strong> {0}</p>", reserva.DataRetirada.ToString("dd/MM/yyyy")));
                corpo.AppendLine(string.Format("<p><strong>Valor Total:</strong> R$ {0}</p>", reserva.ValorTotal.ToString("F2")));
                corpo.AppendLine("<p><strong>Status:</strong> Cancelada</p>");

                if (!string.IsNullOrEmpty(_emailIsabela))
                {
                    EnviarEmail(_emailIsabela, assunto, corpo.ToString());
                }
                
                if (!string.IsNullOrEmpty(_emailCamila) && 
                    !string.IsNullOrEmpty(_emailIsabela) && 
                    _emailCamila.Trim().ToLower() != _emailIsabela.Trim().ToLower())
                {
                    EnviarEmail(_emailCamila, assunto, corpo.ToString());
                }
            }
            catch
            {
                // Erro ao enviar email para filhas - não bloquear
            }

            // Email para o cliente
            if (!string.IsNullOrEmpty(reserva.Email))
            {
                try
                {
                    string nomeCliente = !string.IsNullOrEmpty(reserva.Nome) ? reserva.Nome : "Cliente";
                    var assunto = "Reserva Cancelada - Kingdom Confeitaria";
                    var corpo = new StringBuilder();
                    corpo.AppendLine(string.Format("<h2>Olá {0}!</h2>", System.Web.HttpUtility.HtmlEncode(nomeCliente)));
                    corpo.AppendLine("<p>Sua reserva foi cancelada.</p>");
                    corpo.AppendLine(string.Format("<p><strong>Data de Retirada:</strong> {0}</p>", reserva.DataRetirada.ToString("dd/MM/yyyy")));
                    corpo.AppendLine(string.Format("<p><strong>Valor Total:</strong> R$ {0}</p>", reserva.ValorTotal.ToString("F2")));
                    corpo.AppendLine("<p>Se você não solicitou este cancelamento, entre em contato conosco.</p>");
                    corpo.AppendLine("<p>Obrigado por escolher a Kingdom Confeitaria!</p>");

                    EnviarEmail(reserva.Email, assunto, corpo.ToString());
                }
                catch
                {
                    // Erro ao enviar email para cliente - não bloquear
                }
            }
        }

        public void EnviarEmailReservaExcluida(Reserva reserva)
        {
            // Buscar dados do cliente se não estiverem preenchidos
            if (reserva.ClienteId.HasValue && (string.IsNullOrEmpty(reserva.Nome) || string.IsNullOrEmpty(reserva.Email)))
            {
                try
                {
                    var databaseService = new DatabaseService();
                    var clienteData = databaseService.ObterClientePorId(reserva.ClienteId.Value);
                    if (clienteData != null)
                    {
                        reserva.Nome = clienteData.Nome;
                        reserva.Email = clienteData.Email;
                        reserva.Telefone = clienteData.Telefone;
                    }
                }
                catch
                {
                    // Se não conseguir buscar o cliente, continuar com os dados que já existem
                }
            }

            // Email para as filhas
            try
            {
                string nomeCliente = !string.IsNullOrEmpty(reserva.Nome) ? reserva.Nome : "Cliente não identificado";
                string emailCliente = !string.IsNullOrEmpty(reserva.Email) ? reserva.Email : "Não informado";
                string telefoneCliente = !string.IsNullOrEmpty(reserva.Telefone) ? reserva.Telefone : "Não informado";
                
                var assunto = string.Format("Reserva Excluída - {0}", nomeCliente);
                var corpo = new StringBuilder();
                corpo.AppendLine("<h2>Reserva Excluída</h2>");
                corpo.AppendLine(string.Format("<p><strong>Nome:</strong> {0}</p>", System.Web.HttpUtility.HtmlEncode(nomeCliente)));
                corpo.AppendLine(string.Format("<p><strong>Email:</strong> {0}</p>", System.Web.HttpUtility.HtmlEncode(emailCliente)));
                corpo.AppendLine(string.Format("<p><strong>Telefone:</strong> {0}</p>", System.Web.HttpUtility.HtmlEncode(telefoneCliente)));
                corpo.AppendLine(string.Format("<p><strong>Data de Retirada:</strong> {0}</p>", reserva.DataRetirada.ToString("dd/MM/yyyy")));
                corpo.AppendLine(string.Format("<p><strong>Valor Total:</strong> R$ {0}</p>", reserva.ValorTotal.ToString("F2")));
                corpo.AppendLine("<p><strong>Status:</strong> Excluída</p>");

                if (!string.IsNullOrEmpty(_emailIsabela))
                {
                    EnviarEmail(_emailIsabela, assunto, corpo.ToString());
                }
                
                if (!string.IsNullOrEmpty(_emailCamila) && 
                    !string.IsNullOrEmpty(_emailIsabela) && 
                    _emailCamila.Trim().ToLower() != _emailIsabela.Trim().ToLower())
                {
                    EnviarEmail(_emailCamila, assunto, corpo.ToString());
                }
            }
            catch
            {
                // Erro ao enviar email para filhas - não bloquear
            }

            // Email para o cliente
            if (!string.IsNullOrEmpty(reserva.Email))
            {
                try
                {
                    string nomeCliente = !string.IsNullOrEmpty(reserva.Nome) ? reserva.Nome : "Cliente";
                    var assunto = "Reserva Excluída - Kingdom Confeitaria";
                    var corpo = new StringBuilder();
                    corpo.AppendLine(string.Format("<h2>Olá {0}!</h2>", System.Web.HttpUtility.HtmlEncode(nomeCliente)));
                    corpo.AppendLine("<p>Sua reserva foi excluída pelo administrador.</p>");
                    corpo.AppendLine(string.Format("<p><strong>Data de Retirada:</strong> {0}</p>", reserva.DataRetirada.ToString("dd/MM/yyyy")));
                    corpo.AppendLine(string.Format("<p><strong>Valor Total:</strong> R$ {0}</p>", reserva.ValorTotal.ToString("F2")));
                    corpo.AppendLine("<p>Se você tiver dúvidas sobre esta exclusão, entre em contato conosco.</p>");
                    corpo.AppendLine("<p>Obrigado por escolher a Kingdom Confeitaria!</p>");

                    EnviarEmail(reserva.Email, assunto, corpo.ToString());
                }
                catch
                {
                    // Erro ao enviar email para cliente - não bloquear
                }
            }
        }

        public void EnviarEmailReservaAlterada(Reserva reserva)
        {
            // Buscar dados do cliente se não estiverem preenchidos
            if (reserva.ClienteId.HasValue && (string.IsNullOrEmpty(reserva.Nome) || string.IsNullOrEmpty(reserva.Email)))
            {
                try
                {
                    var databaseService = new DatabaseService();
                    var clienteData = databaseService.ObterClientePorId(reserva.ClienteId.Value);
                    if (clienteData != null)
                    {
                        reserva.Nome = clienteData.Nome;
                        reserva.Email = clienteData.Email;
                        reserva.Telefone = clienteData.Telefone;
                    }
                }
                catch
                {
                    // Se não conseguir buscar o cliente, continuar com os dados que já existem
                }
            }

            // Garantir que o Status está preenchido
            if (string.IsNullOrEmpty(reserva.Status))
            {
                reserva.Status = "Aberta";
            }

            // Email para as filhas
            try
            {
                string nomeCliente = !string.IsNullOrEmpty(reserva.Nome) ? reserva.Nome : "Cliente não identificado";
                string emailCliente = !string.IsNullOrEmpty(reserva.Email) ? reserva.Email : "Não informado";
                string telefoneCliente = !string.IsNullOrEmpty(reserva.Telefone) ? reserva.Telefone : "Não informado";
                
                var assunto = string.Format("Reserva Alterada - {0}", nomeCliente);
                var corpo = new StringBuilder();
                corpo.AppendLine("<h2>Reserva Alterada</h2>");
                corpo.AppendLine(string.Format("<p><strong>Nome:</strong> {0}</p>", System.Web.HttpUtility.HtmlEncode(nomeCliente)));
                corpo.AppendLine(string.Format("<p><strong>Email:</strong> {0}</p>", System.Web.HttpUtility.HtmlEncode(emailCliente)));
                corpo.AppendLine(string.Format("<p><strong>Telefone:</strong> {0}</p>", System.Web.HttpUtility.HtmlEncode(telefoneCliente)));
                corpo.AppendLine(string.Format("<p><strong>Data de Retirada:</strong> {0}</p>", reserva.DataRetirada.ToString("dd/MM/yyyy")));
                corpo.AppendLine(string.Format("<p><strong>Valor Total:</strong> R$ {0}</p>", reserva.ValorTotal.ToString("F2")));
                corpo.AppendLine(string.Format("<p><strong>Status:</strong> {0}</p>", reserva.Status));
                corpo.AppendLine("<h3>Itens do Pedido:</h3>");
                corpo.AppendLine("<ul>");
                foreach (var item in reserva.Itens)
                {
                    corpo.AppendLine(string.Format("<li>{0} ({1}) - Quantidade: {2} - R$ {3}</li>", 
                        item.NomeProduto, item.Tamanho, item.Quantidade, item.Subtotal.ToString("F2")));
                }
                corpo.AppendLine("</ul>");
                if (!string.IsNullOrEmpty(reserva.Observacoes))
                {
                    corpo.AppendLine(string.Format("<p><strong>Observações:</strong> {0}</p>", reserva.Observacoes));
                }

                if (!string.IsNullOrEmpty(_emailIsabela))
                {
                    EnviarEmail(_emailIsabela, assunto, corpo.ToString());
                }
                
                if (!string.IsNullOrEmpty(_emailCamila) && 
                    !string.IsNullOrEmpty(_emailIsabela) && 
                    _emailCamila.Trim().ToLower() != _emailIsabela.Trim().ToLower())
                {
                    EnviarEmail(_emailCamila, assunto, corpo.ToString());
                }
            }
            catch
            {
                // Erro ao enviar email para filhas - não bloquear
            }

            // Email para o cliente
            if (!string.IsNullOrEmpty(reserva.Email))
            {
                try
                {
                    string nomeCliente = !string.IsNullOrEmpty(reserva.Nome) ? reserva.Nome : "Cliente";
                    var assunto = "Reserva Alterada - Kingdom Confeitaria";
                    var baseUrl = ConfigurationManager.AppSettings["BaseUrl"] ?? "http://localhost";
                    var linkReserva = string.Format("{0}/VerReserva.aspx?token={1}", baseUrl, reserva.TokenAcesso);
                    
                    var corpo = new StringBuilder();
                    corpo.AppendLine(string.Format("<h2>Olá {0}!</h2>", System.Web.HttpUtility.HtmlEncode(nomeCliente)));
                    corpo.AppendLine("<p>Sua reserva foi alterada.</p>");
                    corpo.AppendLine(string.Format("<p><strong>Data de Retirada:</strong> {0}</p>", reserva.DataRetirada.ToString("dd/MM/yyyy")));
                    corpo.AppendLine(string.Format("<p><strong>Valor Total:</strong> R$ {0}</p>", reserva.ValorTotal.ToString("F2")));
                    corpo.AppendLine("<h3>Seu Pedido:</h3>");
                    corpo.AppendLine("<ul>");
                    foreach (var item in reserva.Itens)
                    {
                        corpo.AppendLine(string.Format("<li>{0} ({1}) - Quantidade: {2} - R$ {3}</li>", 
                            item.NomeProduto, item.Tamanho, item.Quantidade, item.Subtotal.ToString("F2")));
                    }
                    corpo.AppendLine("</ul>");
                    string statusExibicao = !string.IsNullOrEmpty(reserva.Status) ? reserva.Status : "Aberta";
                    corpo.AppendLine(string.Format("<p><strong>Status:</strong> {0}</p>", statusExibicao));
                    corpo.AppendLine(string.Format("<p><a href='{0}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 10px 0;'>Ver Minha Reserva</a></p>", linkReserva));
                    corpo.AppendLine("<p>Obrigado por escolher a Kingdom Confeitaria!</p>");

                    EnviarEmail(reserva.Email, assunto, corpo.ToString());
                }
                catch
                {
                    // Erro ao enviar email para cliente - não bloquear
                }
            }
        }

        private void EnviarEmail(string para, string assunto, string corpo)
        {
            try
            {
                // Verificar se as configurações estão preenchidas
                if (string.IsNullOrEmpty(_smtpServer) || string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
                {
                    return; // Não lançar exceção
                }

                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Timeout = 30000; // 30 segundos

                    var mensagem = new MailMessage
                    {
                        From = new MailAddress(_emailFrom ?? _smtpUsername, "Kingdom Confeitaria"),
                        Subject = assunto,
                        Body = corpo,
                        IsBodyHtml = true
                    };

                    mensagem.To.Add(para);
                    client.Send(mensagem);
                }
            }
            catch (Exception ex)
            {
                // Log do erro (em produção, usar um sistema de log adequado)
                System.Diagnostics.Debug.WriteLine("Erro ao enviar email: " + ex.Message);
                // Não lançar exceção para não interromper o fluxo
                // Em produção, considere implementar um sistema de retry ou fila
            }
        }
    }
}

