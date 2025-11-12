using System;

namespace KingdomConfeitaria.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public decimal Preco { get; set; }
        public string ImagemUrl { get; set; }
        public bool Ativo { get; set; }
        public int Ordem { get; set; }
        public bool EhSacoPromocional { get; set; }
        public int QuantidadeSaco { get; set; } // Quantidade de biscoitos no saco (6 ou 3)
        public string Produtos { get; set; } // JSON array de IDs dos produtos permitidos no saco: [1, 2, 3]
        public DateTime? ReservavelAte { get; set; } // Data até quando o produto pode ser reservado
        public DateTime? VendivelAte { get; set; } // Data até quando o produto pode ser vendido
    }
}

