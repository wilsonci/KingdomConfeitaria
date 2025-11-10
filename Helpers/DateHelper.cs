using System;
using System.Collections.Generic;
using System.Linq;

namespace KingdomConfeitaria.Helpers
{
    public static class DateHelper
    {
        public static List<DateTime> ObterSegundasAteNatal(int ano)
        {
            var segundas = new List<DateTime>();
            var natal = new DateTime(ano, 12, 25);
            var hoje = DateTime.Now;

            // Encontrar a primeira segunda-feira a partir de hoje
            var diasParaProximaSegunda = ((int)DayOfWeek.Monday - (int)hoje.DayOfWeek + 7) % 7;
            if (diasParaProximaSegunda == 0 && hoje.DayOfWeek != DayOfWeek.Monday)
                diasParaProximaSegunda = 7;

            var proximaSegunda = hoje.AddDays(diasParaProximaSegunda);

            // Adicionar todas as segundas até a última segunda antes do Natal
            var dataAtual = proximaSegunda;
            while (dataAtual < natal)
            {
                segundas.Add(dataAtual);
                dataAtual = dataAtual.AddDays(7);
            }

            return segundas;
        }

        public static DateTime ObterUltimaSegundaAntesNatal(int ano)
        {
            var natal = new DateTime(ano, 12, 25);
            var diasParaSegunda = ((int)DayOfWeek.Monday - (int)natal.DayOfWeek + 7) % 7;
            
            if (diasParaSegunda == 0)
                diasParaSegunda = 7;

            return natal.AddDays(-diasParaSegunda);
        }
    }
}

