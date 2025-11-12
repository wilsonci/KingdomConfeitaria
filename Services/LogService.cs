using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace KingdomConfeitaria.Services
{
    public class LogService
    {
        private static readonly object _lockObject = new object();
        private static string _logDirectory;

        static LogService()
        {
            // Obter caminho do diretório do sistema
            string appPath = HttpRuntime.AppDomainAppPath ?? AppDomain.CurrentDomain.BaseDirectory;
            _logDirectory = Path.Combine(appPath, "log");
            
            // Criar diretório se não existir
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        /// <summary>
        /// Registra uma ação no log
        /// </summary>
        /// <param name="tipo">Tipo de ação (INSERT, UPDATE, DELETE, LOGIN, LOGOUT, etc.)</param>
        /// <param name="quem">Quem executou a ação (nome do usuário/cliente, email, ou "Sistema")</param>
        /// <param name="oque">O que foi afetado (ex: "Reserva", "Cliente", "Produto")</param>
        /// <param name="onde">Onde foi executado (ex: "MinhasReservas.aspx", "Admin.aspx")</param>
        /// <param name="detalhes">Detalhes da alteração (opcional)</param>
        public static void Registrar(string tipo, string quem, string oque, string onde, string detalhes = "")
        {
            try
            {
                lock (_lockObject)
                {
                    string dataAtual = DateTime.Now.ToString("yyyy-MM-dd");
                    string arquivoLog = Path.Combine(_logDirectory, $"log_{dataAtual}.txt");
                    
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string linha = $"[{timestamp}] [{tipo}] Quem: {quem} | O que: {oque} | Onde: {onde}";
                    
                    if (!string.IsNullOrEmpty(detalhes))
                    {
                        linha += $" | Detalhes: {detalhes}";
                    }
                    
                    linha += Environment.NewLine;
                    
                    File.AppendAllText(arquivoLog, linha);
                }
            }
            catch
            {
                // Silenciosamente falhar se não conseguir escrever o log
                // Não queremos que erros de log interrompam o funcionamento do sistema
            }
        }

        /// <summary>
        /// Registra uma inserção
        /// </summary>
        public static void RegistrarInsercao(string quem, string oque, string onde, string detalhes = "")
        {
            Registrar("INSERT", quem, oque, onde, detalhes);
        }

        /// <summary>
        /// Registra uma atualização
        /// </summary>
        public static void RegistrarAtualizacao(string quem, string oque, string onde, string detalhes = "")
        {
            Registrar("UPDATE", quem, oque, onde, detalhes);
        }

        /// <summary>
        /// Registra uma atualização com comparação de valores antigos vs novos
        /// </summary>
        public static void RegistrarAtualizacaoComComparacao(string quem, string oque, string onde, string id, Dictionary<string, string> valoresAntigos, Dictionary<string, string> valoresNovos)
        {
            var alteracoes = new List<string>();
            var todasChaves = new HashSet<string>();
            
            if (valoresAntigos != null)
            {
                foreach (var chave in valoresAntigos.Keys)
                {
                    todasChaves.Add(chave);
                }
            }
            
            if (valoresNovos != null)
            {
                foreach (var chave in valoresNovos.Keys)
                {
                    todasChaves.Add(chave);
                }
            }
            
            foreach (var chave in todasChaves)
            {
                string valorAntigo = valoresAntigos?.ContainsKey(chave) == true ? valoresAntigos[chave] : "(não existia)";
                string valorNovo = valoresNovos?.ContainsKey(chave) == true ? valoresNovos[chave] : "(removido)";
                
                if (valorAntigo != valorNovo)
                {
                    alteracoes.Add($"{chave}: '{valorAntigo}' -> '{valorNovo}'");
                }
            }
            
            string detalhes = $"ID: {id}";
            if (alteracoes.Count > 0)
            {
                detalhes += $" | Alterações: {string.Join("; ", alteracoes)}";
            }
            else
            {
                detalhes += " | (sem alterações detectadas)";
            }
            
            Registrar("UPDATE", quem, oque, onde, detalhes);
        }

        /// <summary>
        /// Registra uma exclusão
        /// </summary>
        public static void RegistrarExclusao(string quem, string oque, string onde, string detalhes = "")
        {
            Registrar("DELETE", quem, oque, onde, detalhes);
        }

        /// <summary>
        /// Registra um login
        /// </summary>
        public static void RegistrarLogin(string quem, string onde, string detalhes = "")
        {
            Registrar("LOGIN", quem, "Login", onde, detalhes);
        }

        /// <summary>
        /// Registra um logout
        /// </summary>
        public static void RegistrarLogout(string quem, string onde)
        {
            Registrar("LOGOUT", quem, "Logout", onde);
        }

        /// <summary>
        /// Registra uma ação de cancelamento
        /// </summary>
        public static void RegistrarCancelamento(string quem, string oque, string onde, string detalhes = "")
        {
            Registrar("CANCEL", quem, oque, onde, detalhes);
        }

        /// <summary>
        /// Obtém informações do usuário atual da sessão
        /// </summary>
        public static string ObterUsuarioAtual(System.Web.SessionState.HttpSessionState session)
        {
            if (session == null)
            {
                return "Anônimo";
            }

            string nome = session["ClienteNome"]?.ToString();
            string email = session["ClienteEmail"]?.ToString();
            int? clienteId = session["ClienteId"] as int?;
            bool? isAdmin = session["IsAdmin"] as bool?;

            if (!string.IsNullOrEmpty(nome))
            {
                string usuario = nome;
                if (isAdmin == true)
                {
                    usuario += " (Admin)";
                }
                if (!string.IsNullOrEmpty(email))
                {
                    usuario += $" - {email}";
                }
                if (clienteId.HasValue)
                {
                    usuario += $" [ID: {clienteId.Value}]";
                }
                return usuario;
            }

            if (clienteId.HasValue)
            {
                return $"Cliente ID: {clienteId.Value}";
            }

            return "Anônimo";
        }

        /// <summary>
        /// Obtém todos os logs dos últimos N dias
        /// </summary>
        public static List<LogEntry> ObterLogs(int dias = 7)
        {
            var logs = new List<LogEntry>();
            
            try
            {
                if (!Directory.Exists(_logDirectory))
                {
                    return logs;
                }

                var arquivosLog = Directory.GetFiles(_logDirectory, "log_*.txt")
                    .OrderByDescending(f => f)
                    .Take(dias)
                    .ToList();

                foreach (var arquivo in arquivosLog)
                {
                    try
                    {
                        var linhas = File.ReadAllLines(arquivo);
                        foreach (var linha in linhas)
                        {
                            if (string.IsNullOrWhiteSpace(linha))
                                continue;

                            var logEntry = ParseLogLine(linha);
                            if (logEntry != null)
                            {
                                logs.Add(logEntry);
                            }
                        }
                    }
                    catch
                    {
                        // Ignorar erros ao ler arquivos individuais
                    }
                }
            }
            catch
            {
                // Retornar lista vazia em caso de erro
            }

            return logs.OrderByDescending(l => l.Timestamp).ToList();
        }

        /// <summary>
        /// Faz o parse de uma linha de log
        /// </summary>
        private static LogEntry ParseLogLine(string linha)
        {
            try
            {
                // Formato: [YYYY-MM-DD HH:mm:ss] [TIPO] Quem: [quem] | O que: [oque] | Onde: [onde] | Detalhes: [detalhes]
                
                var entry = new LogEntry();
                
                // Extrair timestamp
                int timestampStart = linha.IndexOf('[');
                int timestampEnd = linha.IndexOf(']', timestampStart);
                if (timestampStart >= 0 && timestampEnd > timestampStart)
                {
                    string timestampStr = linha.Substring(timestampStart + 1, timestampEnd - timestampStart - 1);
                    if (DateTime.TryParse(timestampStr, out DateTime timestamp))
                    {
                        entry.Timestamp = timestamp;
                    }
                }

                // Extrair tipo
                int tipoStart = linha.IndexOf('[', timestampEnd);
                int tipoEnd = linha.IndexOf(']', tipoStart);
                if (tipoStart >= 0 && tipoEnd > tipoStart)
                {
                    entry.Tipo = linha.Substring(tipoStart + 1, tipoEnd - tipoStart - 1);
                }

                // Extrair "Quem"
                int quemIndex = linha.IndexOf("Quem: ", tipoEnd);
                if (quemIndex >= 0)
                {
                    int quemStart = quemIndex + 6;
                    int quemEnd = linha.IndexOf(" | O que:", quemStart);
                    if (quemEnd < 0) quemEnd = linha.Length;
                    entry.Quem = linha.Substring(quemStart, quemEnd - quemStart).Trim();
                }

                // Extrair "O que"
                int oqueIndex = linha.IndexOf("O que: ", tipoEnd);
                if (oqueIndex >= 0)
                {
                    int oqueStart = oqueIndex + 7;
                    int oqueEnd = linha.IndexOf(" | Onde:", oqueStart);
                    if (oqueEnd < 0) oqueEnd = linha.Length;
                    entry.OQue = linha.Substring(oqueStart, oqueEnd - oqueStart).Trim();
                }

                // Extrair "Onde"
                int ondeIndex = linha.IndexOf("Onde: ", tipoEnd);
                if (ondeIndex >= 0)
                {
                    int ondeStart = ondeIndex + 6;
                    int ondeEnd = linha.IndexOf(" | Detalhes:", ondeStart);
                    if (ondeEnd < 0) ondeEnd = linha.Length;
                    entry.Onde = linha.Substring(ondeStart, ondeEnd - ondeStart).Trim();
                }

                // Extrair "Detalhes"
                int detalhesIndex = linha.IndexOf("Detalhes: ", tipoEnd);
                if (detalhesIndex >= 0)
                {
                    entry.Detalhes = linha.Substring(detalhesIndex + 10).Trim();
                }

                return entry;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Classe para representar uma entrada de log
        /// </summary>
        public class LogEntry
        {
            public DateTime Timestamp { get; set; }
            public string Tipo { get; set; }
            public string Quem { get; set; }
            public string OQue { get; set; }
            public string Onde { get; set; }
            public string Detalhes { get; set; }
        }
    }
}


