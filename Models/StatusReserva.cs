namespace KingdomConfeitaria.Models
{
    public class StatusReserva
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public bool PermiteAlteracao { get; set; }
        public bool PermiteExclusao { get; set; }
        public int Ordem { get; set; }
    }
}

