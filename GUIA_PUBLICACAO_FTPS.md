# Guia de Publicação FTPS - VPS Vultr com IIS

## Problemas Comuns com FTPS

### 1. Problemas de Conexão FTPS

**Sintomas:**
- Erro de timeout
- Erro de certificado SSL
- Conexão recusada
- Erro de autenticação

**Soluções:**

#### A. Verificar Porta e Protocolo
- **FTPS Implícito**: Porta 990
- **FTPS Explícito**: Porta 21 (com TLS)
- **FTP Simples**: Porta 21 (sem SSL)

#### B. Configurar Modo Passivo
O perfil já está configurado com `FtpPassiveMode=true`, mas verifique:
- Firewall do servidor permite portas passivas (geralmente 50000-51000)
- IIS FTP permite modo passivo

#### C. Testar Conexão Manualmente
Use um cliente FTP como FileZilla para testar:
1. Abra FileZilla
2. Configure:
   - Host: `ftp.kingdomconfeitaria.com.br`
   - Protocolo: FTP - File Transfer Protocol
   - Criptografia: Use FTP explícito sobre TLS se disponível
   - Tipo de logon: Normal
   - Usuário: `ftpkingdom`
   - Senha: [sua senha]
3. Clique em "Conectar"

### 2. Alternativa: Usar FTP Simples (sem SSL)

Se FTPS não funcionar, você pode usar FTP simples temporariamente:

**Atualizar perfil de publicação:**
```xml
<publishUrl>ftp://ftp.kingdomconfeitaria.com.br</publishUrl>
```

**⚠️ AVISO**: FTP simples não é seguro. Use apenas para testes ou em redes privadas.

### 3. Alternativa: Web Deploy (Recomendado)

Web Deploy é mais confiável e seguro que FTP:

#### A. Instalar Web Deploy no Servidor VPS
1. Conecte-se ao servidor via RDP
2. Baixe e instale: [Web Deploy 3.6](https://www.iis.net/downloads/microsoft/web-deploy)
3. Configure o serviço Web Deploy para aceitar conexões remotas

#### B. Configurar IIS para Web Deploy
1. Abra IIS Manager no servidor
2. Clique no servidor (não no site)
3. Clique duas vezes em "Management Service"
4. Marque "Enable remote connections"
5. Selecione "Windows credentials or IIS Manager credentials"
6. Clique em "Apply"

#### C. Criar Perfil de Publicação Web Deploy
1. No Visual Studio, clique com botão direito no projeto
2. Selecione "Publish"
3. Clique em "New Profile"
4. Selecione "Web Deploy"
5. Configure:
   - **Server**: `ftp.kingdomconfeitaria.com.br` (ou IP do servidor)
   - **Site name**: Nome do site no IIS (ex: "Default Web Site" ou "KingdomConfeitaria")
   - **User name**: Usuário do Windows Server ou IIS Manager
   - **Password**: Senha do usuário
   - **Destination URL**: `http://kingdomconfeitaria.com.br` (ou seu domínio)

### 4. Alternativa: Publicação Manual via RDP

Se FTP/Web Deploy não funcionarem:

#### A. Publicar Localmente
1. No Visual Studio, clique com botão direito no projeto
2. Selecione "Publish"
3. Escolha "Folder" como método
4. Escolha uma pasta local (ex: `C:\Publish\KingdomConfeitaria`)
5. Clique em "Publish"

#### B. Copiar Arquivos via RDP
1. Conecte-se ao servidor VPS via RDP
2. Copie os arquivos da pasta de publicação para a pasta do site no IIS
3. Certifique-se de copiar:
   - Todos os arquivos `.aspx`
   - Pasta `bin/` completa
   - Pasta `Scripts/` completa
   - Pasta `Images/` completa
   - `web.config` (transformado)
   - `Global.asax`

### 5. Configuração do IIS no Servidor VPS

#### A. Criar Site no IIS
1. Abra IIS Manager
2. Clique com botão direito em "Sites"
3. Selecione "Add Website"
4. Configure:
   - **Site name**: KingdomConfeitaria
   - **Application pool**: .NET Framework v4.0 ou superior
   - **Physical path**: Caminho onde os arquivos estão (ex: `C:\inetpub\wwwroot\KingdomConfeitaria`)
   - **Binding**: 
     - Type: http ou https
     - IP address: All Unassigned
     - Port: 80 (http) ou 443 (https)
     - Host name: kingdomconfeitaria.com.br (se tiver domínio)

#### B. Configurar Application Pool
1. Clique em "Application Pools"
2. Selecione o pool do seu site
3. Configure:
   - **.NET CLR Version**: v4.0
   - **Managed Pipeline Mode**: Integrated
   - **Identity**: ApplicationPoolIdentity (ou usuário específico)

#### C. Configurar Permissões
1. Clique com botão direito na pasta do site
2. Selecione "Properties" > "Security"
3. Adicione permissões para:
   - **IIS_IUSRS**: Read & Execute, List folder contents, Read
   - **IUSR**: Read & Execute, List folder contents, Read
   - **Application Pool Identity**: Read & Execute, List folder contents, Read
   - Para pasta `App_Data`: Adicione permissão de **Write** para Application Pool Identity

### 6. Configurar FTP no IIS (Se necessário)

#### A. Instalar FTP Server
1. No servidor, abra "Server Manager"
2. Clique em "Add Roles and Features"
3. Selecione "Web Server (IIS)" > "FTP Server"
4. Instale

#### B. Configurar FTP Site
1. No IIS Manager, clique com botão direito no site
2. Selecione "Add FTP Publishing"
3. Configure:
   - **Binding**: 
     - IP: All Unassigned
     - Port: 21
     - SSL: Require SSL (para FTPS) ou No SSL (para FTP simples)
   - **Authentication**: Basic
   - **Authorization**: 
     - Allow access to: Specified users
     - User: `ftpkingdom` (ou crie um usuário específico)
     - Permissions: Read, Write

#### C. Configurar Firewall
1. Abra "Windows Firewall with Advanced Security"
2. Adicione regra de entrada:
   - **Port**: 21 (FTP)
   - **Protocol**: TCP
   - **Action**: Allow
3. Para modo passivo, adicione range de portas: 50000-51000

### 7. Solução de Problemas Específicos

#### Erro: "Não é possível conectar ao servidor FTP"
- Verifique se o serviço FTP está rodando no servidor
- Verifique firewall (porta 21)
- Teste conexão com FileZilla
- Verifique se o IP do servidor está correto

#### Erro: "Certificado SSL inválido"
- No Visual Studio, tente desabilitar validação de certificado (não recomendado para produção)
- Configure certificado SSL válido no servidor
- Use FTP simples temporariamente para testes

#### Erro: "Timeout"
- Aumente timeout no perfil de publicação
- Verifique conexão de rede
- Verifique se o servidor está acessível

#### Erro: "Permissão negada"
- Verifique permissões da pasta no servidor
- Verifique se o usuário FTP tem permissões de escrita
- Verifique se o Application Pool tem permissões adequadas

### 8. Configuração Recomendada para VPS Vultr

#### A. Segurança
- Use FTPS ou Web Deploy (não FTP simples)
- Configure firewall para permitir apenas portas necessárias
- Use senhas fortes
- Configure SSL/TLS no site

#### B. Performance
- Configure Application Pool para reciclar em horários específicos
- Configure cache adequado
- Monitore uso de recursos

#### C. Backup
- Configure backup automático da pasta do site
- Configure backup do banco de dados
- Mantenha backups em local separado

### 9. Checklist de Publicação

Antes de publicar:
- [ ] Projeto compila sem erros
- [ ] Configuração Release está correta
- [ ] Web.Release.config está atualizado
- [ ] Connection string está correta
- [ ] BaseUrl está configurada
- [ ] Senha SMTP está criptografada
- [ ] IIS está configurado no servidor
- [ ] Application Pool está configurado
- [ ] Permissões estão corretas
- [ ] Firewall permite conexões
- [ ] FTP/Web Deploy está configurado
- [ ] Testou conexão manualmente

### 10. Comandos Úteis no Servidor

```powershell
# Verificar se IIS está rodando
Get-Service W3SVC

# Verificar sites configurados
Get-Website

# Verificar Application Pools
Get-WebAppPoolState

# Verificar permissões de pasta
icacls "C:\inetpub\wwwroot\KingdomConfeitaria"

# Verificar portas abertas
netstat -an | findstr :21
netstat -an | findstr :80
```

