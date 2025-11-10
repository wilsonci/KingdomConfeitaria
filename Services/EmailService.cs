using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using KingdomConfeitaria.Models;

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
            _smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"];
            _emailIsabela = ConfigurationManager.AppSettings["EmailIsabela"];
            _emailCamila = ConfigurationManager.AppSettings["EmailCamila"];
            _emailFrom = ConfigurationManager.AppSettings["EmailFrom"];
        }

        public void EnviarConfirmacaoReserva(Reserva reserva)
        {
            // Email para as filhas (Isabela e Camila)
            EnviarEmailParaFilhas(reserva);
            
            // Email para o cliente
            EnviarEmailParaCliente(reserva);
        }

        private void EnviarEmailParaFilhas(Reserva reserva)
        {
            var assunto = string.Format("Nova Reserva - {0}", reserva.Nome);
            var corpo = new StringBuilder();
            corpo.AppendLine("<h2>Nova Reserva Recebida!</h2>");
            corpo.AppendLine(string.Format("<p><strong>Nome:</strong> {0}</p>", reserva.Nome));
            corpo.AppendLine(string.Format("<p><strong>Email:</strong> {0}</p>", reserva.Email));
            corpo.AppendLine(string.Format("<p><strong>Telefone:</strong> {0}</p>", reserva.Telefone));
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

            EnviarEmail(_emailIsabela, assunto, corpo.ToString());
            EnviarEmail(_emailCamila, assunto, corpo.ToString());
        }

        private void EnviarEmailParaCliente(Reserva reserva)
        {
            var assunto = "Confirmação de Reserva - Kingdom Confeitaria";
            var baseUrl = ConfigurationManager.AppSettings["BaseUrl"] ?? "http://localhost";
            var linkReserva = string.Format("{0}/VerReserva.aspx?token={1}", baseUrl, reserva.TokenAcesso);
            
            var corpo = new StringBuilder();
            corpo.AppendLine(string.Format("<h2>Olá {0}!</h2>", reserva.Nome));
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
            corpo.AppendLine(string.Format("<p><strong>Status:</strong> {0}</p>", reserva.Status));
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

        private void EnviarEmail(string para, string assunto, string corpo)
        {
            try
            {
                // Verificar se as configurações estão preenchidas
                if (string.IsNullOrEmpty(_smtpServer) || string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
                {
                    System.Diagnostics.Debug.WriteLine("Configurações SMTP não estão preenchidas. Email não será enviado.");
                    return; // Não lançar exceção, apenas logar
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
                // Log do erro (implementar logging adequado)
                System.Diagnostics.Debug.WriteLine(string.Format("Erro ao enviar email: {0}", ex.Message));
                System.Diagnostics.Debug.WriteLine(string.Format("Stack trace: {0}", ex.StackTrace));
                
                // Não lançar exceção para não interromper o fluxo
                // Em produção, considere implementar um sistema de retry ou fila
            }
        }
    }
}

