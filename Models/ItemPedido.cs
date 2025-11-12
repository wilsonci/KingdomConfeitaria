using System;

namespace KingdomConfeitaria.Models
{
    public class ItemPedido
    {
        public int ProdutoId { get; set; }
        public string NomeProduto { get; set; }
        public string Tamanho { get; set; } // "Pequeno" ou "Grande"
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public string Produtos { get; set; } // Formato: "qt:idproduto,qt:idproduto" para sacos/cestas/caixas
    }
}

