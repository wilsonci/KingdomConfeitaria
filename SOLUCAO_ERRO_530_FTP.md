# Solução: Erro 530 - Home Directory Inaccessible

## Problema
O usuário `ftpkingdom` não consegue conectar e recebe o erro:
```
530 Home directory inaccessible
```

O usuário `ftpcasaimp` conecta normalmente, indicando que o problema é específico do usuário `ftpkingdom`.

## Causas Comuns

1. **Diretório home não existe**
2. **Permissões incorretas no diretório**
3. **Caminho do diretório está incorreto**
4. **Usuário não tem permissão para acessar o diretório**

## Solução no Servidor VPS (via RDP)

### Passo 1: Verificar/Criar Diretório Home

1. Conecte-se ao servidor VPS via RDP
2. Abra o **File Explorer**
3. Navegue até a pasta onde você quer que o usuário FTP acesse
   - Exemplo: `C:\inetpub\wwwroot\KingdomConfeitaria`
   - Ou: `C:\FTP\ftpkingdom`
4. Se a pasta não existir, crie-a

### Passo 2: Configurar Usuário FTP no IIS

#### Opção A: Usar IIS Manager (Recomendado)

1. Abra **IIS Manager**
2. Clique no servidor (não no site)
3. Clique duas vezes em **FTP Authorization Rules**
4. Verifique se há regras configuradas
5. Clique em **Add Allow Rule**
6. Configure:
   - **Specified users**: `ftpkingdom`
   - **Permissions**: Read, Write
   - Clique em **OK**

#### Opção B: Configurar via FTP Site

1. No IIS Manager, localize seu **FTP Site**
2. Clique com botão direito no FTP Site
3. Selecione **Add FTP Publishing** (se ainda não tiver) ou **Edit Permissions**
4. Na aba **Authentication**, certifique-se de que **Basic Authentication** está habilitado
5. Na aba **Authorization**, adicione regra:
   - **Allow access to**: Specified users
   - **User**: `ftpkingdom`
   - **Permissions**: Read, Write

### Passo 3: Configurar Diretório Home do Usuário

#### Método 1: Via IIS Manager (FTP User Isolation)

1. No IIS Manager, selecione o **FTP Site**
2. Clique duas vezes em **FTP User Isolation**
3. Selecione uma das opções:
   - **User name directory (disable global virtual directories)**: Cada usuário tem seu próprio diretório
   - **FTP root directory (enable global virtual directories)**: Todos os usuários compartilham o mesmo diretório raiz
4. Clique em **Apply**

#### Método 2: Configurar Diretório Específico

1. No IIS Manager, selecione o **FTP Site**
2. Clique com botão direito e selecione **Add Virtual Directory**
3. Configure:
   - **Alias**: `ftpkingdom` (ou deixe vazio para raiz)
   - **Physical path**: `C:\inetpub\wwwroot\KingdomConfeitaria` (ou o caminho do seu site)
4. Clique em **OK**
5. Selecione o diretório criado
6. Clique em **FTP Authorization Rules**
7. Adicione regra para o usuário `ftpkingdom` com permissões Read e Write

### Passo 4: Verificar/Criar Usuário Windows

1. Abra **Computer Management** (Gerenciamento do Computador)
2. Expanda **Local Users and Groups**
3. Clique em **Users**
4. Verifique se o usuário `ftpkingdom` existe
5. Se não existir:
   - Clique com botão direito em **Users** > **New User**
   - **User name**: `ftpkingdom`
   - **Password**: [defina uma senha forte]
   - Desmarque **User must change password at next logon**
   - Marque **Password never expires** (ou configure conforme política)
   - Clique em **Create**

### Passo 5: Configurar Permissões do Diretório

1. Navegue até o diretório do site (ex: `C:\inetpub\wwwroot\KingdomConfeitaria`)
2. Clique com botão direito > **Properties**
3. Vá para a aba **Security**
4. Clique em **Edit**
5. Clique em **Add**
6. Digite: `ftpkingdom`
7. Clique em **Check Names** para verificar
8. Clique em **OK**
9. Selecione o usuário `ftpkingdom`
10. Marque as permissões:
    - **Read & Execute**
    - **List folder contents**
    - **Read**
    - **Write** (necessário para publicação)
11. Clique em **OK**

### Passo 6: Verificar Permissões do IIS_IUSRS

Certifique-se de que o grupo `IIS_IUSRS` também tem permissões:

1. Na mesma janela de **Security** do diretório
2. Verifique se `IIS_IUSRS` está listado
3. Se não estiver, adicione com as mesmas permissões
4. Verifique também `IUSR` se necessário

### Passo 7: Configurar FTP User Isolation (Importante)

Para que cada usuário acesse seu próprio diretório:

1. No IIS Manager, selecione o **FTP Site**
2. Clique duas vezes em **FTP User Isolation**
3. Selecione: **User name directory (disable global virtual directories)**
4. Clique em **Apply**

**IMPORTANTE**: Com essa configuração, o diretório home do usuário deve ser:
- `C:\inetpub\ftproot\ftpkingdom\` (ou o caminho configurado no FTP Site)

### Passo 8: Alternativa - Usar Mesmo Diretório para Todos

Se você quer que todos os usuários FTP acessem o mesmo diretório:

1. No IIS Manager, selecione o **FTP Site**
2. Clique duas vezes em **FTP User Isolation**
3. Selecione: **FTP root directory (enable global virtual directories)**
4. Clique em **Apply**
5. Configure o **Physical Path** do FTP Site para o diretório do seu site
6. Adicione regras de autorização para cada usuário

## Verificação Rápida

### Teste 1: Verificar se o diretório existe
```powershell
Test-Path "C:\inetpub\wwwroot\KingdomConfeitaria"
```

### Teste 2: Verificar permissões
```powershell
icacls "C:\inetpub\wwwroot\KingdomConfeitaria"
```

### Teste 3: Verificar usuário
```powershell
Get-LocalUser -Name "ftpkingdom"
```

### Teste 4: Testar conexão FTP
Use FileZilla ou comando FTP:
```cmd
ftp ftp.kingdomconfeitaria.com.br
```

## Solução Rápida (Se nada funcionar)

### Opção 1: Usar o usuário que funciona
Atualize o perfil de publicação para usar `ftpcasaimp`:

```xml
<UserName>ftpcasaimp</UserName>
```

### Opção 2: Criar novo usuário FTP
1. Crie um novo usuário Windows no servidor
2. Configure o diretório home corretamente
3. Configure permissões
4. Use esse novo usuário no perfil de publicação

### Opção 3: Usar Web Deploy
Web Deploy não depende de configuração de usuário FTP e pode ser mais simples:
- Veja instruções no arquivo `GUIA_PUBLICACAO_FTPS.md`

## Comandos PowerShell Úteis

### Criar diretório e configurar permissões
```powershell
# Criar diretório
New-Item -ItemType Directory -Path "C:\inetpub\wwwroot\KingdomConfeitaria" -Force

# Adicionar permissões
$acl = Get-Acl "C:\inetpub\wwwroot\KingdomConfeitaria"
$permission = "ftpkingdom","FullControl","ContainerInherit,ObjectInherit","None","Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($accessRule)
Set-Acl "C:\inetpub\wwwroot\KingdomConfeitaria" $acl
```

### Verificar configuração FTP
```powershell
# Listar sites FTP
Get-Website | Where-Object {$_.ServerAutoStart -eq $true}

# Verificar serviço FTP
Get-Service | Where-Object {$_.Name -like "*FTP*"}
```

## Checklist Final

- [ ] Diretório home existe e está acessível
- [ ] Usuário `ftpkingdom` existe no Windows
- [ ] Permissões do diretório estão corretas
- [ ] FTP User Isolation está configurado corretamente
- [ ] Regras de autorização FTP estão configuradas
- [ ] Firewall permite conexões FTP
- [ ] Serviço FTP está rodando
- [ ] Testou conexão com FileZilla

## Se ainda não funcionar

1. **Use o usuário que funciona** (`ftpcasaimp`) temporariamente
2. **Configure Web Deploy** (mais confiável)
3. **Publique manualmente** via RDP copiando os arquivos

