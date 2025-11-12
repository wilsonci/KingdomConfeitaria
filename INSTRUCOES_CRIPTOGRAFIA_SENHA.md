# Instruções - Criptografia de Senha do Gmail

## ✅ Implementação Concluída

A senha do Gmail agora está criptografada no arquivo `Web.config` usando criptografia AES-256.

## Como Funciona

1. **Criptografia**: A senha é criptografada usando AES (Advanced Encryption Standard) com chave de 256 bits
2. **Armazenamento**: A senha criptografada é armazenada no `Web.config` na chave `SmtpPassword`
3. **Descriptografia**: O `EmailService` descriptografa automaticamente a senha ao ler do `Web.config`

## Senha Atual

A senha atual já foi criptografada e está no `Web.config`:
```xml
<add key="SmtpPassword" value="WaNi5IdwRTB5FCiVRmbo1Bui1huwNuaTcINmTBMSF34=" />
```

## Como Criptografar uma Nova Senha

### Opção 1: Usar o Utilitário Web (Recomendado)

1. Acesse: `http://localhost:porta/Utils/EncryptPassword.aspx`
2. Digite a senha do Gmail no campo
3. Clique em "Criptografar"
4. Copie o texto criptografado gerado
5. Cole no `Web.config` na chave `SmtpPassword`
6. **IMPORTANTE**: Remova o arquivo `Utils/EncryptPassword.aspx` após o uso por segurança

### Opção 2: Usar Código C#

```csharp
using KingdomConfeitaria.Security;

string senhaPlana = "sua-senha-aqui";
string senhaCriptografada = PasswordEncryption.Encrypt(senhaPlana);
Console.WriteLine("Senha criptografada: " + senhaCriptografada);
```

### Opção 3: Usar PowerShell

```powershell
# Execute no diretório do projeto
$senha = "sua-senha-aqui"
Add-Type -AssemblyName System.Security
$key = [System.Text.Encoding]::UTF8.GetBytes("KingdomConfeitaria2024!@#SecureKey$%^&*()")
$sha256 = [System.Security.Cryptography.SHA256]::Create()
$keyBytes = $sha256.ComputeHash($key)
$aes = [System.Security.Cryptography.Aes]::Create()
$aes.Key = $keyBytes
$aes.IV = [System.Text.Encoding]::UTF8.GetBytes("1234567890123456")
$aes.Mode = [System.Security.Cryptography.CipherMode]::CBC
$aes.Padding = [System.Security.Cryptography.PaddingMode]::PKCS7
$encryptor = $aes.CreateEncryptor()
$senhaBytes = [System.Text.Encoding]::UTF8.GetBytes($senha)
$encrypted = $encryptor.TransformFinalBlock($senhaBytes, 0, $senhaBytes.Length)
$base64 = [Convert]::ToBase64String($encrypted)
Write-Host "Senha criptografada: $base64"
```

## Compatibilidade

O sistema é compatível com senhas em texto plano (para migração gradual):
- Se a senha no `Web.config` estiver criptografada, será descriptografada automaticamente
- Se estiver em texto plano, será usada diretamente (compatibilidade retroativa)

## Segurança

### ⚠️ Importante

1. **Chave de Criptografia**: A chave de criptografia está no código (`Security/PasswordEncryption.cs`)
   - Em produção, considere mover a chave para:
     - Variável de ambiente do servidor
     - Azure Key Vault
     - Arquivo de configuração separado (não commitado no Git)

2. **Utilitário de Criptografia**: 
   - O arquivo `Utils/EncryptPassword.aspx` deve ser **removido** após o uso
   - Não deixe este arquivo em produção

3. **Web.config**:
   - Não commite o `Web.config` com senhas em texto plano no controle de versão
   - Use `Web.config.example` ou variáveis de ambiente

4. **Permissões**:
   - Configure permissões adequadas no `Web.config` no servidor
   - Apenas o usuário do Application Pool deve ter acesso de leitura

## Arquivos Modificados

- ✅ `Security/PasswordEncryption.cs` - Classe de criptografia/descriptografia
- ✅ `Services/EmailService.cs` - Modificado para descriptografar senha
- ✅ `Web.config` - Senha atualizada para versão criptografada
- ✅ `Utils/EncryptPassword.aspx` - Utilitário web para criptografar senhas (remover após uso)

## Teste

Para testar se a criptografia está funcionando:

1. Verifique se o `EmailService` consegue descriptografar a senha
2. Teste o envio de um email
3. Se funcionar, a criptografia está correta

## Próximos Passos (Opcional)

Para maior segurança em produção:

1. **Mover chave para variável de ambiente**:
   ```csharp
   string encryptionKey = Environment.GetEnvironmentVariable("ENCRYPTION_KEY") 
       ?? "KingdomConfeitaria2024!@#SecureKey$%^&*()";
   ```

2. **Usar DPAPI (Data Protection API)** do Windows:
   - Mais seguro, específico da máquina
   - Requer configuração no servidor

3. **Usar Azure Key Vault** ou similar:
   - Solução mais robusta para produção
   - Requer configuração adicional

## Suporte

Se houver problemas com a descriptografia:
- Verifique se a chave de criptografia está correta
- Verifique se a senha foi criptografada corretamente
- Verifique os logs de erro da aplicação

