# Guia de Publicação - Kingdom Confeitaria

## Publicação no IIS via FTP

### Pré-requisitos
1. Visual Studio instalado
2. Acesso FTP ao servidor (ftp.kindomconfeitaria.com.br)
3. IIS configurado no servidor Windows

### Passos para Publicação

#### 1. Preparar o Projeto
- Certifique-se de que o projeto compila sem erros
- Configure `debug="false"` no `Web.config` para produção
- Verifique a connection string no `Web.config` para o servidor de produção

#### 2. Publicar via Visual Studio
1. Clique com o botão direito no projeto no Solution Explorer
2. Selecione "Publicar" (Publish)
3. Escolha "Novo" ou "Editar" perfil
4. Selecione "FTP" como método de publicação
5. Configure:
   - **Servidor**: ftp.kindomconfeitaria.com.br
   - **Porta**: 21 (ou a porta FTP configurada)
   - **Site**: / (ou o caminho do site no servidor)
   - **Nome de usuário**: [seu usuário FTP]
   - **Senha**: [sua senha FTP]
   - **URL de destino**: http://kindomconfeitaria.com.br (ou o domínio configurado)
6. Clique em "Validar Conexão" para testar
7. Clique em "Publicar"

#### 3. Configurações Importantes

**Web.config - Connection String:**
```xml
<connectionStrings>
  <add name="KingdomConfeitariaDB" 
       connectionString="Data Source=[SEU_SERVIDOR_SQL];Initial Catalog=KingdomConfeitaria;User Id=[usuario];Password=[senha];Connect Timeout=30;MultipleActiveResultSets=True" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

**Web.config - BaseUrl:**
```xml
<add key="BaseUrl" value="http://kindomconfeitaria.com.br" />
```

#### 4. Configuração no IIS
1. No servidor, abra o IIS Manager
2. Crie um novo Site ou Application Pool
3. Configure:
   - **Physical Path**: Caminho onde os arquivos foram publicados
   - **Application Pool**: .NET Framework v4.0 ou superior
   - **Binding**: Configure o domínio e porta (geralmente porta 80 ou 443)

#### 5. Permissões
- Certifique-se de que a pasta do site tem permissões para:
  - IIS_IUSRS (leitura e execução)
  - IUSR (leitura)
  - App Pool Identity (leitura e escrita para App_Data se necessário)

#### 6. Banco de Dados
- Execute o script de criação do banco no servidor SQL Server
- Ou configure a connection string para apontar para o banco existente
- Certifique-se de que o SQL Server permite conexões remotas

### Solução de Problemas

**Erro: "Falha no build"**
- Verifique se há erros de compilação no projeto
- Limpe a solução (Build > Clean Solution)
- Recompile (Build > Rebuild Solution)
- Verifique se todas as referências estão corretas

**Erro: "Connection String"**
- Verifique se a connection string está correta
- Teste a conexão com o banco de dados
- Verifique se o SQL Server está acessível

**Erro: "Permissões"**
- Verifique as permissões da pasta no servidor
- Verifique as permissões do Application Pool

### Arquivos Necessários para Publicação
- Todos os arquivos `.aspx` e `.aspx.cs`
- `Web.config`
- `Global.asax` e `Global.asax.cs`
- Pasta `Scripts/` com todos os arquivos JavaScript
- Pasta `Images/` com todas as imagens
- Pasta `bin/` com todos os DLLs compilados
- Arquivos de modelo (`Models/`)
- Arquivos de serviço (`Services/`)

### Notas
- O modo `debug="false"` deve estar configurado em produção para melhor performance
- Certifique-se de que todas as dependências NuGet estão instaladas
- Verifique se o .NET Framework 4.8 está instalado no servidor

