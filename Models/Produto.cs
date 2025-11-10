using System;

namespace KingdomConfeitaria.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public decimal PrecoPequeno { get; set; }
        public decimal PrecoGrande { get; set; }
        public string ImagemUrl { get; set; }
        public bool Ativo { get; set; }
        public int Ordem { get; set; }
        public bool EhSacoPromocional { get; set; }
        public int QuantidadeSaco { get; set; } // Quantidade de biscoitos no saco (6 ou 3)
        public string TamanhoSaco { get; set; } // "Pequeno" ou "Grande"
    }
}

