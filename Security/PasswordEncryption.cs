using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace KingdomConfeitaria.Security
{
    /// <summary>
    /// Classe para criptografar e descriptografar senhas usando AES
    /// </summary>
    public static class PasswordEncryption
    {
        // Chave de criptografia (em produção, considere armazenar em variável de ambiente ou arquivo seguro)
        // Esta chave deve ser mantida em segredo e não deve ser commitada no controle de versão
        private static readonly string EncryptionKey = "KingdomConfeitaria2024!@#SecureKey$%^&*()";
        
        // IV (Initialization Vector) - deve ser único para cada criptografia, mas usaremos um fixo para simplicidade
        // Em produção, gere um IV único para cada criptografia e armazene junto com o texto criptografado
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("1234567890123456"); // 16 bytes para AES-128

        /// <summary>
        /// Criptografa uma string usando AES
        /// </summary>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                byte[] encrypted;
                
                using (Aes aes = Aes.Create())
                {
                    aes.Key = DeriveKey(EncryptionKey);
                    aes.IV = IV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                            encrypted = msEncrypt.ToArray();
                        }
                    }
                }

                // Converter para Base64 para armazenamento seguro
                return Convert.ToBase64String(encrypted);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao criptografar senha: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Descriptografa uma string criptografada usando AES
        /// </summary>
        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            try
            {
                // Verificar se começa com prefixo de texto criptografado
                // Se não começar, assumir que é texto plano (para compatibilidade com senhas antigas)
                if (!cipherText.StartsWith("ENC:"))
                {
                    // Tentar descriptografar (pode ser texto criptografado sem prefixo)
                    try
                    {
                        byte[] cipherBytes = Convert.FromBase64String(cipherText);
                        return DecryptBytes(cipherBytes);
                    }
                    catch
                    {
                        // Se falhar, assumir que é texto plano
                        return cipherText;
                    }
                }
                else
                {
                    // Remover prefixo "ENC:" e descriptografar
                    string base64Text = cipherText.Substring(4);
                    byte[] cipherBytes = Convert.FromBase64String(base64Text);
                    return DecryptBytes(cipherBytes);
                }
            }
            catch
            {
                // Se houver erro na descriptografia, retornar o texto original
                // Isso permite compatibilidade com senhas em texto plano durante a migração
                return cipherText;
            }
        }

        private static string DecryptBytes(byte[] cipherBytes)
        {
            string plaintext = null;

            using (Aes aes = Aes.Create())
            {
                aes.Key = DeriveKey(EncryptionKey);
                aes.IV = IV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        /// <summary>
        /// Deriva uma chave de 256 bits (32 bytes) a partir de uma string usando SHA256
        /// </summary>
        private static byte[] DeriveKey(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        /// <summary>
        /// Verifica se uma string está criptografada
        /// </summary>
        public static bool IsEncrypted(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            // Verificar se começa com prefixo "ENC:"
            if (text.StartsWith("ENC:"))
                return true;

            // Tentar decodificar como Base64 e verificar se é válido
            try
            {
                byte[] bytes = Convert.FromBase64String(text);
                // Se for Base64 válido e tiver tamanho razoável, provavelmente está criptografado
                return bytes.Length > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}

