# Instalação do SQL Server LocalDB em Produção

## Erro Encontrado

```
Unable to locate a Local Database Runtime installation. 
Verify that SQL Server Express is properly installed and that the Local Database Runtime feature is enabled.
```

Este erro ocorre quando o SQL Server LocalDB não está instalado no servidor de produção.

## Solução 1: Instalar SQL Server Express LocalDB (Recomendado)

### Passo 1: Download
1. Acesse: https://www.microsoft.com/pt-br/sql-server/sql-server-downloads
2. Clique em "Baixar agora" na versão **Express**
3. Escolha o arquivo de instalação (geralmente `SQLEXPR_x64_PTB.exe` ou similar)

### Passo 2: Instalação
1. Execute o instalador baixado
2. Escolha "Instalação básica" ou "Instalação personalizada"
3. **IMPORTANTE**: Na instalação personalizada, certifique-se de selecionar:
   - ✅ **LocalDB** (Local Database Runtime)
   - ✅ **SQL Server Database Engine**
4. Complete a instalação
5. **Reinicie o servidor** após a instalação

### Passo 3: Verificação
Após reiniciar, verifique se o LocalDB está instalado:

**No PowerShell (como Administrador):**
```powershell
sqllocaldb info
```

**Ou verifique se o serviço está rodando:**
```powershell
sqllocaldb start MSSQLLocalDB
sqllocaldb info MSSQLLocalDB
```

### Passo 4: Testar a Aplicação
1. Acesse a aplicação no navegador
2. O banco de dados será criado automaticamente na primeira execução
3. Verifique se a pasta `App_Data` contém o arquivo `KingdomConfeitaria_Prod.mdf`

## Solução 2: Usar SQL Server Express ou SQL Server Completo

Se você já tem SQL Server Express ou SQL Server completo instalado, pode configurar a aplicação para usá-lo.

### Passo 1: Editar web.config
No servidor de produção, edite o arquivo `web.config` e localize a seção `<connectionStrings>`:

**Localizar:**
```xml
<add name="KingdomConfeitariaDB" 
     connectionString="Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\KingdomConfeitaria_Prod.mdf;Initial Catalog=KingdomConfeitaria_Prod;Integrated Security=True;Connect Timeout=30;MultipleActiveResultSets=True" 
     providerName="System.Data.SqlClient" />
```

### Passo 2: Alterar para SQL Server Express
**Para SQL Server Express:**
```xml
<add name="KingdomConfeitariaDB" 
     connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=KingdomConfeitaria_Prod;Integrated Security=True;Connect Timeout=30;MultipleActiveResultSets=True" 
     providerName="System.Data.SqlClient" />
```

**Para SQL Server Completo (instância padrão):**
```xml
<add name="KingdomConfeitariaDB" 
     connectionString="Data Source=localhost;Initial Catalog=KingdomConfeitaria_Prod;Integrated Security=True;Connect Timeout=30;MultipleActiveResultSets=True" 
     providerName="System.Data.SqlClient" />
```

**Para SQL Server Completo (instância nomeada):**
```xml
<add name="KingdomConfeitariaDB" 
     connectionString="Data Source=localhost\NOMEDAINSTANCIA;Initial Catalog=KingdomConfeitaria_Prod;Integrated Security=True;Connect Timeout=30;MultipleActiveResultSets=True" 
     providerName="System.Data.SqlClient" />
```

### Passo 3: Criar o Banco de Dados
Se usar SQL Server Express ou completo, você precisa criar o banco de dados manualmente:

1. Abra o **SQL Server Management Studio (SSMS)**
2. Conecte-se à instância do SQL Server
3. Execute o script SQL para criar o banco (ou deixe a aplicação criar automaticamente na primeira execução)

### Passo 4: Verificar Permissões
Certifique-se de que:
- O usuário do IIS (geralmente `IIS_IUSRS` ou `ApplicationPoolIdentity`) tem permissões para criar/ler/escrever no banco
- O usuário tem permissões na pasta `App_Data` (se usar AttachDbFilename)

## Solução 3: Usar SQL Server na Rede

Se você tem um SQL Server em outro servidor:

```xml
<add name="KingdomConfeitariaDB" 
     connectionString="Data Source=NOMEDOSERVIDOR;Initial Catalog=KingdomConfeitaria_Prod;User Id=usuario;Password=senha;Connect Timeout=30;MultipleActiveResultSets=True" 
     providerName="System.Data.SqlClient" />
```

**Nota**: Neste caso, você precisará criar o banco de dados manualmente no servidor SQL.

## Comparação das Opções

| Opção | Vantagens | Desvantagens |
|-------|-----------|--------------|
| **LocalDB** | ✅ Leve e fácil de instalar<br>✅ Banco na pasta App_Data<br>✅ Criação automática | ❌ Requer instalação<br>❌ Limitado a uma instância por vez |
| **SQL Server Express** | ✅ Mais recursos<br>✅ Pode ter múltiplas instâncias | ❌ Mais pesado<br>❌ Requer configuração manual |
| **SQL Server Completo** | ✅ Máximo de recursos<br>✅ Suporte completo | ❌ Mais caro<br>❌ Requer licenciamento |

## Recomendação

Para produção com poucos usuários (< 100 simultâneos):
- ✅ **LocalDB** é suficiente e mais simples

Para produção com muitos usuários (> 100 simultâneos):
- ✅ **SQL Server Express** ou **SQL Server Completo** é recomendado

## Troubleshooting

### Erro: "Unable to locate a Local Database Runtime installation"
- ✅ Instale o SQL Server Express LocalDB
- ✅ Reinicie o servidor após instalação
- ✅ Verifique se o serviço está rodando: `sqllocaldb start MSSQLLocalDB`

### Erro: "Cannot attach database file"
- ✅ Verifique permissões na pasta `App_Data`
- ✅ Certifique-se de que o IIS tem permissões de leitura/escrita

### Erro: "Login failed for user"
- ✅ Verifique a connection string
- ✅ Verifique se o usuário tem permissões no banco
- ✅ Para SQL Server, verifique se a autenticação Windows está habilitada

## Suporte

Se continuar com problemas:
1. Verifique os logs de erro da aplicação
2. Verifique os logs do Windows Event Viewer
3. Verifique os logs do IIS

