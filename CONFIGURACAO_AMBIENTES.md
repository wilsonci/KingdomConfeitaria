# Configuração de Ambientes - Development e Production

## Estrutura de Configuração

A aplicação suporta dois ambientes:
- **Development**: Ambiente de desenvolvimento local
- **Production**: Ambiente de produção no servidor

## Arquivos de Configuração

### Web.config (Base - Development)
- Configuração padrão para desenvolvimento
- Banco de dados: `KingdomConfeitaria_Dev` (LocalDB)
- Debug habilitado
- Custom errors desabilitados

### Web.Release.config (Transformação - Production)
- Aplicado automaticamente ao publicar com configuração "Release"
- Transforma o Web.config para produção
- Banco de dados: `KingdomConfeitaria_Prod` (SQL Server)
- Debug desabilitado
- Custom errors habilitados
- HTTPS forçado

## Configuração de Connection Strings

### Development (Web.config)
```xml
<connectionStrings>
  <add name="KingdomConfeitariaDB" 
       connectionString="Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=KingdomConfeitaria_Dev;Integrated Security=True;Connect Timeout=30;MultipleActiveResultSets=True" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

### Production (Web.Release.config)
```xml
<connectionStrings>
  <add name="KingdomConfeitariaDB" 
       connectionString="Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\KingdomConfeitaria_Prod.mdf;Initial Catalog=KingdomConfeitaria_Prod;Integrated Security=True;Connect Timeout=30;MultipleActiveResultSets=True" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

**NOTA**: O banco de dados será criado automaticamente na pasta `App_Data` da aplicação na primeira execução.

## Configuração de AppSettings

### Development
- `Environment`: "Development"
- `BaseUrl`: "http://localhost"
- Banco criado automaticamente pelo `DatabaseService`

### Production
- `Environment`: "Production"
- `BaseUrl`: "https://kindomconfeitaria.com.br" (atualize com seu domínio)
- Banco criado automaticamente na pasta `App_Data` na primeira execução
- Arquivo: `App_Data\KingdomConfeitaria_Prod.mdf`

## Como Publicar para Produção

### 1. Preparar Web.Release.config

1. Abra `Web.Release.config`
2. A connection string já está configurada para usar LocalDB na pasta `App_Data`:
   ```xml
   <add name="KingdomConfeitariaDB" 
        connectionString="Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\KingdomConfeitaria_Prod.mdf;Initial Catalog=KingdomConfeitaria_Prod;Integrated Security=True;Connect Timeout=30;MultipleActiveResultSets=True" 
        providerName="System.Data.SqlClient" />
   ```
   **NOTA**: O banco será criado automaticamente na primeira execução.
3. Atualize a `BaseUrl`:
   ```xml
   <add key="BaseUrl" value="https://kindomconfeitaria.com.br" />
   ```
4. Criptografe a senha SMTP de produção e atualize:
   ```xml
   <add key="SmtpPassword" value="SENHA_CRIPTOGRAFADA_PRODUCAO" />
   ```

### 2. Publicar no Visual Studio

1. Clique com botão direito no projeto
2. Selecione "Publish"
3. Escolha ou crie um perfil de publicação
4. Selecione configuração: **Release**
5. Configure o método de publicação (FTP, Web Deploy, etc.)
6. Clique em "Publish"

### 3. Verificar após Publicação

1. Acesse o site em produção
2. Verifique se está usando HTTPS
3. Teste o login
4. Verifique se os dados estão sendo salvos no banco de produção
5. Verifique os logs de erro (se houver)

## Criação do Banco de Dados em Produção

### Primeira Vez

O banco de dados será criado **automaticamente** na primeira execução da aplicação em produção:
- Localização: `[Pasta da Aplicação]\App_Data\KingdomConfeitaria_Prod.mdf`
- A pasta `App_Data` será criada automaticamente se não existir
- As tabelas serão criadas automaticamente

**IMPORTANTE**: Certifique-se de que:
- A pasta `App_Data` tem permissões de escrita para o usuário do Application Pool
- O LocalDB está instalado no servidor de produção
- O usuário do Application Pool tem permissões para criar bancos de dados

### Migrações Futuras

Consulte `Scripts/MigracaoBancoDados.md` para instruções detalhadas sobre migrações.

## Segurança

### Development
- Debug habilitado (para facilitar desenvolvimento)
- Custom errors desabilitados (mostra erros detalhados)
- HTTP permitido

### Production
- Debug desabilitado (melhor performance)
- Custom errors habilitados (não expõe detalhes de erro)
- HTTPS forçado
- Senhas criptografadas

## Variáveis de Ambiente (Alternativa)

Se preferir usar variáveis de ambiente em vez de transformações:

1. Configure variáveis de ambiente no servidor
2. Modifique o código para ler variáveis de ambiente:
   ```csharp
   string connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") 
       ?? ConfigurationManager.ConnectionStrings["KingdomConfeitariaDB"].ConnectionString;
   ```

## Checklist de Deploy

Antes de publicar em produção:

- [ ] Web.Release.config atualizado com dados corretos
- [ ] Connection string de produção testada
- [ ] BaseUrl atualizada para domínio de produção
- [ ] Senha SMTP criptografada e atualizada
- [ ] Banco de dados de produção criado e migrado
- [ ] Backup do banco de produção feito (se já existir)
- [ ] HTTPS configurado no servidor
- [ ] Permissões de arquivos verificadas
- [ ] Application Pool configurado corretamente
- [ ] Testes realizados em ambiente de staging (se houver)

## Troubleshooting

### Erro: "Cannot open database"

- Verifique se a pasta `App_Data` existe e tem permissões de escrita
- Verifique se o LocalDB está instalado no servidor
- Verifique se o arquivo `.mdf` foi criado em `App_Data`

### Erro: "Login failed"

- Verifique se o usuário do Application Pool tem permissões no LocalDB
- Verifique se o LocalDB está configurado corretamente

### Erro: "The database does not exist"

- O banco será criado automaticamente na primeira execução
- Verifique se a pasta `App_Data` tem permissões de escrita
- Verifique os logs de erro da aplicação para mais detalhes

### Erro: "Access denied" ao criar banco

- Verifique permissões da pasta `App_Data`
- O usuário do Application Pool precisa ter permissões de escrita
- No IIS, configure as permissões da pasta para o usuário do Application Pool

### Aplicação não está usando HTTPS

- Verifique se o Web.Release.config tem a regra de rewrite
- Verifique se o SSL está configurado no IIS
- Verifique se a porta 443 está aberta no firewall

## Suporte

Para problemas ou dúvidas:
1. Verifique os logs de erro da aplicação
2. Verifique os logs do IIS
3. Verifique os logs do SQL Server
4. Consulte a documentação de migração

