using System;
using System.Configuration;
using System.Net;
using System.Text;
using KingdomConfeitaria.Models;

namespace KingdomConfeitaria.Services
{
    public class WhatsAppService
    {
        private readonly string _whatsAppApiUrl;
        private readonly string _whatsAppApiKey;
        private readonly string _whatsAppPhoneNumber;

        public WhatsAppService()
        {
            _whatsAppApiUrl = ConfigurationManager.AppSettings["WhatsAppApiUrl"];
            _whatsAppApiKey = ConfigurationManager.AppSettings["WhatsAppApiKey"];
            _whatsAppPhoneNumber = ConfigurationManager.AppSettings["WhatsAppPhoneNumber"];
        }

        public void EnviarConfirmacaoReserva(Reserva reserva, string telefone)
        {
            if (string.IsNullOrEmpty(telefone) || string.IsNullOrEmpty(_whatsAppApiUrl))
            {
                return; // WhatsApp não configurado ou telefone não informado
            }

            var baseUrl = ConfigurationManager.AppSettings["BaseUrl"] ?? "http://localhost";
            var linkReserva = string.Format("{0}/VerReserva.aspx?token={1}", baseUrl, reserva.TokenAcesso);
            
            var mensagem = string.Format(
                "Olá {0}! Sua reserva foi confirmada na Kingdom Confeitaria.\n\n" +
                "Data de Retirada: {1}\n" +
                "Valor Total: R$ {2}\n\n" +
                "Acompanhe sua reserva: {3}\n\n" +
                "Obrigado por escolher a Kingdom Confeitaria!",
                reserva.Nome,
                reserva.DataRetirada.ToString("dd/MM/yyyy"),
                reserva.ValorTotal.ToString("F2"),
                linkReserva
            );

            EnviarMensagem(telefone, mensagem);
        }

        public void EnviarConfirmacaoCadastro(Cliente cliente)
        {
            if (string.IsNullOrEmpty(cliente.Telefone) || string.IsNullOrEmpty(_whatsAppApiUrl))
            {
                return;
            }

            var baseUrl = ConfigurationManager.AppSettings["BaseUrl"] ?? "http://localhost";
            var linkConfirmacao = string.Format("{0}/ConfirmarCadastro.aspx?token={1}", baseUrl, cliente.TokenConfirmacao);
            
            var mensagem = string.Format(
                "Olá {0}! Bem-vindo à Kingdom Confeitaria!\n\n" +
                "Confirme seu cadastro clicando no link:\n{1}\n\n" +
                "Obrigado!",
                cliente.Nome,
                linkConfirmacao
            );

            EnviarMensagem(cliente.Telefone, mensagem);
        }

        private void EnviarMensagem(string telefone, string mensagem)
        {
            try
            {
                // Formatar telefone (remover caracteres especiais)
                telefone = telefone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
                if (!telefone.StartsWith("55"))
                {
                    telefone = "55" + telefone;
                }

                // Exemplo de integração com API de WhatsApp (Twilio, Evolution API, etc.)
                // Esta é uma estrutura básica - você precisará adaptar para a API escolhida
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Headers[HttpRequestHeader.Authorization] = "Bearer " + _whatsAppApiKey;
                    
                    var json = string.Format(
                        "{{\"phone\":\"{0}\",\"message\":\"{1}\"}}",
                        telefone,
                        mensagem.Replace("\"", "\\\"").Replace("\n", "\\n")
                    );

                    // Descomente quando configurar a API real
                    // var response = client.UploadString(_whatsAppApiUrl, "POST", json);
                    
                    // Por enquanto, apenas log
                    System.Diagnostics.Debug.WriteLine(string.Format("WhatsApp enviado para {0}: {1}", telefone, mensagem));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Erro ao enviar WhatsApp: {0}", ex.Message));
                // Não lançar exceção para não interromper o fluxo
            }
        }
    }
}

