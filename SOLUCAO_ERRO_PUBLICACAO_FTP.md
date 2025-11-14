# Solução: Erro de Publicação FTP no Visual Studio

## Erro Encontrado

```
Este WebPublishMethod específico (FTP) ainda não é suportado na linha de comando de sbuild. 
Usar o Visual Studio para publicar.
```

## Causa

O Visual Studio está tentando usar o MSBuild em linha de comando para publicar, mas o método FTP não é suportado dessa forma. Isso pode acontecer quando:

1. O Visual Studio está configurado para usar MSBuild externo
2. Há um problema com os targets de publicação
3. O perfil de publicação está mal configurado

## Soluções

### Solução 1: Publicar pelo Visual Studio (Recomendado)

1. **Feche todas as instâncias do Visual Studio**
2. **Reabra o Visual Studio**
3. **Abra o projeto**
4. **Clique com botão direito no projeto** no Solution Explorer
5. **Selecione "Publish"**
6. **Selecione o perfil "FTPProfile"**
7. **Clique em "Publish"**

**IMPORTANTE**: Não use "Build > Publish" ou atalhos de teclado. Use o botão "Publish" na janela de publicação.

### Solução 2: Verificar Configurações do Visual Studio

1. No Visual Studio, vá em **Tools > Options**
2. Expanda **Projects and Solutions > Build and Run**
3. Verifique:
   - **MSBuild project build output verbosity**: Normal ou Minimal
   - **On Run, when projects are out of date**: Always build
   - **On Run, when build or deployment errors occur**: Do not launch

### Solução 3: Limpar e Reconstruir

1. **Build > Clean Solution**
2. **Build > Rebuild Solution**
3. Feche o Visual Studio
4. Delete as pastas `bin` e `obj` manualmente
5. Reabra o Visual Studio
6. Tente publicar novamente

### Solução 4: Criar Novo Perfil de Publicação

1. No Visual Studio, clique com botão direito no projeto
2. Selecione **"Publish"**
3. Clique em **"New Profile"** ou **"Edit"**
4. Selecione **"FTP"**
5. Configure:
   - **Server**: `ftp.kingdomconfeitaria.com.br`
   - **Site path**: `/` (ou o caminho do seu site)
   - **User name**: `ftpkingdom` (ou `ftpcasaimp` se o outro não funcionar)
   - **Password**: [sua senha]
   - **Destination URL**: `http://kingdomconfeitaria.com.br` (ou seu domínio)
6. Clique em **"Validate Connection"** para testar
7. Clique em **"Save"**
8. Clique em **"Publish"**

### Solução 5: Usar Publicação Manual (Alternativa)

Se nada funcionar, publique manualmente:

#### Passo 1: Publicar para Pasta Local

1. No Visual Studio, clique com botão direito no projeto
2. Selecione **"Publish"**
3. Clique em **"New Profile"**
4. Selecione **"Folder"**
5. Escolha uma pasta local (ex: `C:\Publish\KingdomConfeitaria`)
6. Clique em **"Publish"**

#### Passo 2: Copiar Arquivos via FTP

Use um cliente FTP como FileZilla:

1. Abra FileZilla
2. Configure conexão:
   - Host: `ftp.kingdomconfeitaria.com.br`
   - Protocolo: FTP - File Transfer Protocol
   - Criptografia: Use FTP explícito sobre TLS (se disponível)
   - Usuário: `ftpkingdom` ou `ftpcasaimp`
   - Senha: [sua senha]
3. Conecte
4. Copie todos os arquivos da pasta de publicação para o servidor

### Solução 6: Usar Web Deploy (Melhor Alternativa)

Web Deploy é mais confiável que FTP:

#### No Servidor (VPS):

1. Instale **Web Deploy 3.6**:
   - Download: https://www.iis.net/downloads/microsoft/web-deploy
2. Configure o serviço:
   - Abra IIS Manager
   - Clique no servidor (não no site)
   - Clique em **"Management Service"**
   - Marque **"Enable remote connections"**
   - Selecione **"Windows credentials or IIS Manager credentials"**
   - Clique em **"Apply"**

#### No Visual Studio:

1. Clique com botão direito no projeto
2. Selecione **"Publish"**
3. Clique em **"New Profile"**
4. Selecione **"Web Deploy"**
5. Configure:
   - **Server**: IP do servidor ou domínio
   - **Site name**: Nome do site no IIS
   - **User name**: Usuário do Windows Server
   - **Password**: Senha do usuário
6. Clique em **"Validate Connection"**
7. Clique em **"Publish"**

### Solução 7: Verificar Logs Detalhados

Para ver mais detalhes do erro:

1. No Visual Studio, vá em **View > Output**
2. Selecione **"Show output from: Build"** ou **"Publish"**
3. Tente publicar novamente
4. Veja os erros detalhados na janela Output

## Checklist de Verificação

Antes de tentar publicar:

- [ ] Visual Studio está atualizado
- [ ] Projeto compila sem erros (Build > Rebuild Solution)
- [ ] Perfil de publicação está configurado corretamente
- [ ] Credenciais FTP estão corretas
- [ ] Servidor FTP está acessível
- [ ] Firewall permite conexões FTP
- [ ] Usuário FTP tem permissões corretas
- [ ] Diretório home do usuário FTP está configurado

## Comandos Úteis

### Verificar se o projeto compila
```powershell
cd C:\Desenv\KingdomConfeitaria
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" KingdomConfeitaria.csproj /t:Build /p:Configuration=Release
```

### Limpar projeto
```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" KingdomConfeitaria.csproj /t:Clean
```

## Se Nada Funcionar

1. **Use publicação manual** (Solução 5)
2. **Configure Web Deploy** (Solução 6) - mais confiável
3. **Copie arquivos via RDP** - conecte ao servidor e copie manualmente

## Nota Importante

O erro indica que o Visual Studio está tentando usar MSBuild em linha de comando para publicar FTP, o que não é suportado. A publicação FTP deve ser feita através da interface gráfica do Visual Studio, não via linha de comando.

Se você está usando o botão "Publish" no Visual Studio e ainda assim recebe esse erro, pode ser um bug ou configuração incorreta. Nesse caso, use uma das alternativas acima.

