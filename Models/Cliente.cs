using System;

namespace KingdomConfeitaria.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public bool TemWhatsApp { get; set; }
        public string Provider { get; set; } // "Facebook", "Google", "WhatsApp", "Instagram", "Email"
        public string ProviderId { get; set; } // ID do provedor social
        public string Senha { get; set; }
        public string TokenConfirmacao { get; set; }
        public string TokenRecuperacaoSenha { get; set; }
        public DateTime? DataExpiracaoRecuperacaoSenha { get; set; }
        public bool EmailConfirmado { get; set; }
        public bool WhatsAppConfirmado { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime? UltimoAcesso { get; set; }
    }
}

