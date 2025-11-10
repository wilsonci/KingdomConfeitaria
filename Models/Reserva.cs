using System;
using System.Collections.Generic;

namespace KingdomConfeitaria.Models
{
    public class Reserva
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public DateTime DataRetirada { get; set; }
        public DateTime DataReserva { get; set; }
        public string Status { get; set; } // "Pendente", "Confirmado", "Pronto", "Entregue", "Cancelado"
        public decimal ValorTotal { get; set; }
        public List<ItemPedido> Itens { get; set; }
        public string Observacoes { get; set; }
        public bool ConvertidoEmPedido { get; set; }
        public DateTime? PrevisaoEntrega { get; set; }
        public bool Cancelado { get; set; }
        public int? ClienteId { get; set; }
        public string TokenAcesso { get; set; } // Token para acesso direto à reserva
    }
}

