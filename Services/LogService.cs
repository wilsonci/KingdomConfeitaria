using System;
using System.IO;
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
    }
}

