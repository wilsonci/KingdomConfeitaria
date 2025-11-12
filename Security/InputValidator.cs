using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace KingdomConfeitaria.Security
{
    /// <summary>
    /// Classe para validação e sanitização de entrada do usuário
    /// </summary>
    public static class InputValidator
    {
        // Padrões de validação
        private static readonly Regex EmailPattern = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);
        private static readonly Regex PhonePattern = new Regex(@"^[\d\s\(\)\-\+]{10,15}$", RegexOptions.Compiled);
        private static readonly Regex NumericPattern = new Regex(@"^\d+$", RegexOptions.Compiled);
        private static readonly Regex DecimalPattern = new Regex(@"^\d+(\.\d{1,2})?$", RegexOptions.Compiled);
        
        // Caracteres perigosos para SQL Injection e XSS
        private static readonly char[] DangerousChars = { '<', '>', '"', '\'', '&', ';', '(', ')', '[', ']', '{', '}', '|', '\\', '/', '%', '*', '?', '`', '$' };
        
        // Palavras-chave SQL perigosas
        private static readonly string[] SqlKeywords = { 
            "SELECT", "INSERT", "UPDATE", "DELETE", "DROP", "CREATE", "ALTER", "EXEC", "EXECUTE", 
            "UNION", "SCRIPT", "SCRIPT>", "<SCRIPT", "JAVASCRIPT", "VBSCRIPT", "ONLOAD", "ONERROR",
            "OR", "AND", "1=1", "1'='1", "--", "/*", "*/", "XP_", "SP_"
        };

        /// <summary>
        /// Valida e sanitiza uma string de entrada
        /// </summary>
        public static string SanitizeString(string input, int maxLength = 1000, bool allowHtml = false)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Remover caracteres de controle
            input = new string(input.Where(c => !char.IsControl(c) || char.IsWhiteSpace(c)).ToArray());

            // Limitar tamanho
            if (input.Length > maxLength)
                input = input.Substring(0, maxLength);

            // Se não permitir HTML, codificar caracteres perigosos
            if (!allowHtml)
            {
                input = HttpUtility.HtmlEncode(input);
            }

            // Remover espaços extras
            input = input.Trim();

            return input;
        }

        /// <summary>
        /// Valida um email
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            email = email.Trim().ToLower();
            
            if (email.Length > 254) // RFC 5321
                return false;

            return EmailPattern.IsMatch(email);
        }

        /// <summary>
        /// Valida um telefone
        /// </summary>
        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            phone = phone.Trim();
            
            // Remover caracteres de formatação para validação
            string digitsOnly = new string(phone.Where(char.IsDigit).ToArray());
            
            if (digitsOnly.Length < 10 || digitsOnly.Length > 15)
                return false;

            return PhonePattern.IsMatch(phone);
        }

        /// <summary>
        /// Valida um número inteiro
        /// </summary>
        public static bool IsValidInteger(string input, out int value, int min = int.MinValue, int max = int.MaxValue)
        {
            value = 0;
            
            if (string.IsNullOrWhiteSpace(input))
                return false;

            if (!NumericPattern.IsMatch(input))
                return false;

            if (!int.TryParse(input, out value))
                return false;

            return value >= min && value <= max;
        }

        /// <summary>
        /// Valida um número decimal
        /// </summary>
        public static bool IsValidDecimal(string input, out decimal value, decimal min = decimal.MinValue, decimal max = decimal.MaxValue)
        {
            value = 0;
            
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Normalizar separador decimal
            input = input.Replace(",", ".");

            if (!DecimalPattern.IsMatch(input))
                return false;

            if (!decimal.TryParse(input, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
                return false;

            return value >= min && value <= max;
        }

        /// <summary>
        /// Detecta possíveis tentativas de SQL Injection
        /// </summary>
        public static bool ContainsSqlInjection(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            string upperInput = input.ToUpper();
            
            foreach (string keyword in SqlKeywords)
            {
                if (upperInput.Contains(keyword))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Detecta possíveis tentativas de XSS
        /// </summary>
        public static bool ContainsXss(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            string lowerInput = input.ToLower();
            
            // Padrões comuns de XSS
            string[] xssPatterns = {
                "<script", "</script>", "javascript:", "onerror=", "onload=", 
                "onclick=", "onmouseover=", "onfocus=", "onblur=",
                "eval(", "expression(", "vbscript:", "data:text/html"
            };

            foreach (string pattern in xssPatterns)
            {
                if (lowerInput.Contains(pattern))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Valida e sanitiza um ID de entidade
        /// </summary>
        public static bool IsValidEntityId(string input, out int id)
        {
            id = 0;
            
            if (!IsValidInteger(input, out id, 1, int.MaxValue))
                return false;

            if (ContainsSqlInjection(input) || ContainsXss(input))
                return false;

            return true;
        }

        /// <summary>
        /// Valida uma data
        /// </summary>
        public static bool IsValidDate(string input, out DateTime date)
        {
            date = DateTime.MinValue;
            
            if (string.IsNullOrWhiteSpace(input))
                return false;

            if (DateTime.TryParse(input, out date))
            {
                // Validar que a data está em um range razoável
                if (date.Year < 1900 || date.Year > 2100)
                    return false;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove caracteres perigosos de uma string
        /// </summary>
        public static string RemoveDangerousChars(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            foreach (char c in DangerousChars)
            {
                input = input.Replace(c.ToString(), "");
            }

            return input;
        }

        /// <summary>
        /// Valida uma lista de IDs
        /// </summary>
        public static List<int> ValidateIdList(string input, int maxItems = 1000)
        {
            var validIds = new List<int>();
            
            if (string.IsNullOrWhiteSpace(input))
                return validIds;

            // Verificar tentativas de SQL Injection
            if (ContainsSqlInjection(input))
                return validIds;

            string[] parts = input.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (string part in parts.Take(maxItems))
            {
                if (IsValidEntityId(part.Trim(), out int id))
                {
                    if (!validIds.Contains(id))
                        validIds.Add(id);
                }
            }

            return validIds;
        }
    }
}

