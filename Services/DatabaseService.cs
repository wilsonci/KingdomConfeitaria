using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using KingdomConfeitaria.Models;
using KingdomConfeitaria.Services;

namespace KingdomConfeitaria.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private static bool _bancoInicializado = false;
        private static readonly object _lockInicializacao = new object();
        private static string _connectionStringNormalizada = null;
        private static readonly object _lockConnectionString = new object();

        public DatabaseService()
        {
            try
            {
                // Cachear connection string normalizada (evita testar múltiplas instâncias a cada requisição)
                if (_connectionStringNormalizada == null)
                {
                    lock (_lockConnectionString)
                    {
                        if (_connectionStringNormalizada == null)
                        {
                            string originalConnectionString = ConfigurationManager.ConnectionStrings["KingdomConfeitariaDB"].ConnectionString;
                            _connectionStringNormalizada = NormalizarConnectionString(originalConnectionString);
                        }
                    }
                }
                
                _connectionString = _connectionStringNormalizada;
                
                // Inicializar banco apenas uma vez (usando lock para thread-safety)
                if (!_bancoInicializado)
                {
                    lock (_lockInicializacao)
                    {
                        if (!_bancoInicializado)
                        {
                            CriarBancoETabelasSeNaoExistirem();
                            _bancoInicializado = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Se o erro for relacionado ao LocalDB não instalado, dar mensagem mais clara
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " " + ex.InnerException.Message;
                }
                
                if (errorMessage.Contains("LocalDB") || errorMessage.Contains("Local Database Runtime") || 
                    errorMessage.Contains("Unable to locate a Local Database Runtime") ||
                    errorMessage.Contains("error: 52"))
                {
                    throw new Exception(
                        "ERRO: SQL Server LocalDB não está instalado ou não está acessível.\n\n" +
                        "SOLUÇÃO 1 - Instalar LocalDB:\n" +
                        "1. Baixe o SQL Server Express LocalDB da Microsoft:\n" +
                        "   https://www.microsoft.com/pt-br/sql-server/sql-server-downloads\n" +
                        "2. Selecione 'Express' e certifique-se de incluir 'LocalDB' na instalação\n" +
                        "3. Reinicie o servidor após a instalação\n" +
                        "4. Verifique se está instalado: sqllocaldb info\n" +
                        "5. Se estiver instalado mas não rodando, inicie: sqllocaldb start MSSQLLocalDB\n\n" +
                        "SOLUÇÃO 2 - Usar SQL Server Express ou SQL Server completo:\n" +
                        "1. Edite o arquivo web.config no servidor\n" +
                        "2. Altere a connection string para usar SQL Server Express ou SQL Server completo\n" +
                        "3. Exemplo para SQL Server Express:\n" +
                        "   Data Source=.\\SQLEXPRESS;Initial Catalog=KingdomConfeitaria_Prod;Integrated Security=True;\n" +
                        "   Ou para SQL Server completo:\n" +
                        "   Data Source=localhost;Initial Catalog=KingdomConfeitaria_Prod;Integrated Security=True;\n\n" +
                        "Erro original: " + ex.Message,
                        ex
                    );
                }
                throw;
            }
        }
        
        private string NormalizarConnectionString(string connectionString)
        {
            // Tentar encontrar uma instância válida do LocalDB e atualizar a connection string
            if (connectionString.Contains("Data Source="))
            {
                int dsStart = connectionString.IndexOf("Data Source=") + "Data Source=".Length;
                int dsEnd = connectionString.IndexOf(";", dsStart);
                if (dsEnd == -1) dsEnd = connectionString.Length;
                string originalDataSource = connectionString.Substring(dsStart, dsEnd - dsStart).Trim();
                
                // Verificar se a connection string original já não é LocalDB
                // Se for SQL Server normal, não tentar LocalDB
                bool isLocalDB = originalDataSource.Contains("(localdb)") || 
                                 originalDataSource.Contains(".\\MSSQLLOCALDB") ||
                                 originalDataSource.Contains(".\\MSSQLLocalDB") ||
                                 originalDataSource == ".\\MSSQLLocalDB" ||
                                 originalDataSource == ".\\MSSQLLOCALDB";
                
                if (!isLocalDB && !originalDataSource.Contains("LocalDB"))
                {
                    // Não é LocalDB, retornar original (pode ser SQL Server Express ou completo)
                    return connectionString;
                }
                
                // Se é LocalDB, tentar encontrar uma instância válida
                // Mas se não encontrar, retornar a original e deixar o erro acontecer na conexão real
                // Lista de instâncias do LocalDB para tentar (ordem de preferência)
                // Priorizar a instância original, depois .\MSSQLLocalDB (produção), depois (localdb)\MSSQLLocalDB
                string[] localDbInstances = {
                    originalDataSource, // Tentar primeiro a instância original
                    ".\\MSSQLLocalDB",  // Produção (prioridade)
                    ".\\MSSQLLOCALDB",  // Produção (alternativa)
                    "(localdb)\\MSSQLLocalDB",
                    "(localdb)\\MSSQLLOCALDB",
                    "(localdb)\\v11.0",
                    "(localdb)\\ProjectsV13",
                    "(localdb)\\ProjectsV12"
                };
                
                // Tentar apenas a primeira instância (a original) para não demorar
                // Se não funcionar, retornar a original e deixar a conexão real tentar
                if (!string.IsNullOrEmpty(originalDataSource))
                {
                    // Se a instância original já é válida, usar ela
                    string testConnectionString = connectionString;
                    string testMasterConnection = connectionString;
                    if (testMasterConnection.Contains("Initial Catalog="))
                    {
                        int catStart = testMasterConnection.IndexOf("Initial Catalog=");
                        int catEnd = testMasterConnection.IndexOf(";", catStart);
                        if (catEnd == -1) catEnd = testMasterConnection.Length;
                        testMasterConnection = testMasterConnection.Remove(catStart, catEnd - catStart);
                    }
                    testMasterConnection = testMasterConnection.TrimEnd(';') + ";Initial Catalog=master";
                    
                    // Testar apenas com timeout muito curto (1 segundo)
                    try
                    {
                        if (!testMasterConnection.Contains("Connect Timeout="))
                        {
                            testMasterConnection = testMasterConnection.TrimEnd(';') + ";Connect Timeout=1";
                        }
                        using (var testConnection = new SqlConnection(testMasterConnection))
                        {
                            testConnection.Open();
                            // Se funcionou, retornar a connection string original
                            return connectionString;
                        }
                    }
                    catch
                    {
                        // Se não funcionou, continuar e retornar original mesmo assim
                        // A conexão real tentará e dará erro mais claro se necessário
                    }
                }
                
                // Se não encontrou nenhuma instância LocalDB válida, tentar iniciar o LocalDB uma última vez
                // Mas apenas se realmente necessário (não bloquear o carregamento)
                try
                {
                    // Tentar iniciar em background sem bloquear
                    System.Threading.Tasks.Task.Run(() => TentarIniciarLocalDB());
                    
                    // Não aguardar - retornar a connection string original e deixar a conexão real tentar
                    // Se o LocalDB não estiver rodando, a conexão real falhará com mensagem clara
                }
                catch
                {
                    // Se ainda não funcionar, continuar
                }
                
                // Se não encontrou nenhuma instância válida, retornar a connection string original
                // O erro real acontecerá na tentativa de conexão, que terá uma mensagem mais clara
                return connectionString;
            }
            
            // Se não encontrou nenhuma instância válida, retornar original
            return connectionString;
        }

        private void CriarBancoETabelasSeNaoExistirem()
        {
            try
            {
                // Verificar se a connection string usa AttachDbFilename (banco na pasta App_Data)
                bool usaAttachDbFilename = _connectionString.Contains("AttachDbFilename=");
                
                if (usaAttachDbFilename)
                {
                    // Banco na pasta App_Data (produção)
                    // Extrair caminho do arquivo .mdf
                    string attachDbPath = "";
                    if (_connectionString.Contains("AttachDbFilename="))
                    {
                        int startIndex = _connectionString.IndexOf("AttachDbFilename=") + "AttachDbFilename=".Length;
                        int endIndex = _connectionString.IndexOf(";", startIndex);
                        if (endIndex == -1) endIndex = _connectionString.Length;
                        attachDbPath = _connectionString.Substring(startIndex, endIndex - startIndex).Trim();
                        
                        // Substituir |DataDirectory| pelo caminho real
                        if (attachDbPath.Contains("|DataDirectory|"))
                        {
                            string dataDirectory = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data");
                            if (string.IsNullOrEmpty(dataDirectory))
                            {
                                dataDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
                            }
                            
                            // Garantir que a pasta App_Data existe e tem permissões
                            if (!System.IO.Directory.Exists(dataDirectory))
                            {
                                System.IO.Directory.CreateDirectory(dataDirectory);
                            }
                            
                            // Garantir permissões de escrita na pasta App_Data
                            try
                            {
                                string testFile = System.IO.Path.Combine(dataDirectory, "test_write.tmp");
                                System.IO.File.WriteAllText(testFile, "test");
                                System.IO.File.Delete(testFile);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"Não foi possível escrever na pasta App_Data. Verifique as permissões da pasta: {dataDirectory}. Erro: {ex.Message}", ex);
                            }
                            
                            attachDbPath = attachDbPath.Replace("|DataDirectory|", dataDirectory);
                        }
                        
                        // Extrair nome do banco primeiro (mais rápido)
                        string databaseName = ExtrairNomeBanco(_connectionString);
                        
                        // Verificar se o arquivo existe primeiro (mais rápido que verificar banco)
                        bool arquivoExiste = System.IO.File.Exists(attachDbPath);
                        
                        // Se o arquivo existe, verificar rapidamente se precisa fazer attach
                        if (arquivoExiste)
                        {
                            // Arquivo existe - verificar se o banco está anexado
                            // Criar connection string para master para verificar
                            string masterConnectionString = CriarMasterConnectionString(_connectionString);
                            
                            if (!string.IsNullOrEmpty(masterConnectionString))
                            {
                                bool bancoExiste = VerificarBancoExiste(masterConnectionString, databaseName);
                                
                                if (!bancoExiste)
                                {
                                    // Arquivo existe mas banco não está anexado - FAZER ATTACH
                                    FazerAttachBanco(masterConnectionString, databaseName, attachDbPath);
                                }
                                else
                                {
                                    // Banco existe - verificar se está anexado ao arquivo correto
                                    // Tentar conectar para verificar
                                    try
                                    {
                                        using (var testConnection = new SqlConnection(_connectionString))
                                        {
                                            testConnection.Open();
                                            // Se conseguiu conectar, está tudo certo
                                        }
                                    }
                                    catch (SqlException testEx)
                                    {
                                        // Se falhar com "already exists", fazer detach e reattach
                                        if (testEx.Message.Contains("already exists") || testEx.Message.Contains("Cannot attach"))
                                        {
                                            try
                                            {
                                                FazerDetachBancoForcado(masterConnectionString, databaseName);
                                                FazerAttachBanco(masterConnectionString, databaseName, attachDbPath);
                                            }
                                            catch
                                            {
                                                // Se falhar, o erro será tratado na conexão principal
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Arquivo não existe - precisa verificar/criar banco
                            // Criar connection string para master (tentando diferentes instâncias do LocalDB)
                            string masterConnectionString = CriarMasterConnectionString(_connectionString);
                            
                            // Se não conseguiu criar connection string válida, tentar usar a connection string original
                            if (string.IsNullOrEmpty(masterConnectionString))
                            {
                                throw new Exception("Não foi possível encontrar uma instância válida do SQL Server LocalDB. Verifique se o LocalDB está instalado e funcionando.");
                            }
                            
                            // Verificar se o banco já existe no LocalDB
                            bool bancoExiste = VerificarBancoExiste(masterConnectionString, databaseName);
                            
                            if (!bancoExiste && !arquivoExiste)
                            {
                                // Banco não existe e arquivo não existe - CRIAR NOVO
                                CriarNovoBanco(masterConnectionString, databaseName, attachDbPath);
                            }
                            else if (bancoExiste && !arquivoExiste)
                            {
                                // Banco existe no LocalDB mas arquivo não existe - DETACH e criar arquivo
                                // Isso acontece quando o banco foi criado antes mas o arquivo foi movido/deletado
                                try
                                {
                                    FazerDetachBanco(masterConnectionString, databaseName);
                                    CriarNovoBanco(masterConnectionString, databaseName, attachDbPath);
                                }
                                catch (Exception detachEx)
                                {
                                    // Se falhar no detach, tentar forçar o detach e depois criar
                                    try
                                    {
                                        // Tentar detach forçado (fecha todas as conexões)
                                        FazerDetachBancoForcado(masterConnectionString, databaseName);
                                        CriarNovoBanco(masterConnectionString, databaseName, attachDbPath);
                                    }
                                    catch
                                    {
                                        // Se ainda falhar, lançar exceção com mensagem clara
                                        throw new Exception(
                                            $"O banco de dados '{databaseName}' já existe no LocalDB, mas o arquivo não está na pasta App_Data. " +
                                            $"Arquivo esperado: {attachDbPath}. " +
                                            $"Para resolver, execute no servidor: sqllocaldb stop MSSQLLocalDB e depois sqllocaldb start MSSQLLocalDB. " +
                                            $"Ou faça detach manual do banco usando SQL Server Management Studio. Erro: {detachEx.Message}",
                                            detachEx
                                        );
                                    }
                                }
                            }
                            else if (!bancoExiste && arquivoExiste)
                            {
                                // Arquivo existe mas banco não está anexado - FAZER ATTACH
                                FazerAttachBanco(masterConnectionString, databaseName, attachDbPath);
                            }
                        }
                    }
                }
                else
                {
                    // Desenvolvimento: Banco no LocalDB padrão
                    // Extrair o nome do banco da connection string
                    string databaseName = "KingdomConfeitaria_Dev";
                    if (_connectionString.Contains("Initial Catalog="))
                    {
                        int startIndex = _connectionString.IndexOf("Initial Catalog=") + "Initial Catalog=".Length;
                        int endIndex = _connectionString.IndexOf(";", startIndex);
                        if (endIndex == -1) endIndex = _connectionString.Length;
                        databaseName = _connectionString.Substring(startIndex, endIndex - startIndex).Trim();
                    }
                    
                    // Primeiro, criar o banco de dados se não existir
                    var masterConnectionString = _connectionString.Replace($"Initial Catalog={databaseName}", "Initial Catalog=master");
                    using (var masterConnection = new SqlConnection(masterConnectionString))
                    {
                        masterConnection.Open();
                        var createDbCommand = new SqlCommand($@"
                            IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '{databaseName}')
                            BEGIN
                                CREATE DATABASE [{databaseName}]
                            END", masterConnection);
                        createDbCommand.ExecuteNonQuery();
                    }
                }

                // Agora conectar ao banco e criar as tabelas
                // Se usar AttachDbFilename, pode haver conflito se o banco já estiver anexado
                try
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        connection.Open();
                        // Se conseguiu conectar, continuar normalmente
                    }
                }
                catch (SqlException sqlEx)
                {
                    // Se o erro for "already exists", o banco está anexado mas com conflito
                    if (sqlEx.Message.Contains("already exists") || sqlEx.Message.Contains("Cannot attach"))
                    {
                        // Tentar fazer detach e reattach
                        if (usaAttachDbFilename)
                        {
                            string masterConnectionString = CriarMasterConnectionString(_connectionString);
                            if (!string.IsNullOrEmpty(masterConnectionString))
                            {
                                string databaseName = ExtrairNomeBanco(_connectionString);
                                try
                                {
                                    // Fazer detach forçado
                                    FazerDetachBancoForcado(masterConnectionString, databaseName);
                                    // Tentar conectar novamente
                                    using (var connection = new SqlConnection(_connectionString))
                                    {
                                        connection.Open();
                                    }
                                }
                                catch (Exception reattachEx)
                                {
                                    throw new Exception(
                                        $"Erro ao anexar banco de dados. O banco '{databaseName}' já existe no LocalDB. " +
                                        $"Tente fazer detach manual: sqllocaldb stop MSSQLLocalDB e depois sqllocaldb start MSSQLLocalDB. " +
                                        $"Erro: {reattachEx.Message}",
                                        reattachEx
                                    );
                                }
                            }
                        }
                        else
                        {
                            throw;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
                
                // Agora conectar ao banco e criar as tabelas
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    
                    // Verificar se a tabela Produtos existe
                    var checkTable = new SqlCommand(@"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Produtos]') AND type in (N'U'))
                        BEGIN
                            CREATE TABLE [dbo].[Produtos](
                                [Id] [int] IDENTITY(1,1) NOT NULL,
                                [Nome] [nvarchar](200) NOT NULL,
                                [Descricao] [nvarchar](1000) NULL,
                                [Preco] [decimal](10, 2) NOT NULL,
                                [ImagemUrl] [nvarchar](500) NULL,
                                [Ativo] [bit] NOT NULL DEFAULT(1),
                                [Ordem] [int] NOT NULL DEFAULT(0),
                                [EhSacoPromocional] [bit] NOT NULL DEFAULT(0),
                                [QuantidadeSaco] [int] NOT NULL DEFAULT(0),
                                [Produtos] [nvarchar](500) NULL,
                                [ReservavelAte] [datetime] NULL,
                                [VendivelAte] [datetime] NULL,
                                CONSTRAINT [PK_Produtos] PRIMARY KEY CLUSTERED ([Id] ASC)
                            )
                        END", connection);
                    checkTable.ExecuteNonQuery();
                    
                    // Migração: Garantir que a coluna Preco existe
                    // Primeiro, verificar se Preco não existe
                    var checkPrecoExists = new SqlCommand("SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Produtos]') AND name = 'Preco'", connection);
                    var precoExists = (int)checkPrecoExists.ExecuteScalar() > 0;
                    
                    if (!precoExists)
                    {
                        // Verificar se PrecoPequeno existe (para migração de dados)
                        var checkPrecoPequenoExists = new SqlCommand("SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Produtos]') AND name = 'PrecoPequeno'", connection);
                        var precoPequenoExists = (int)checkPrecoPequenoExists.ExecuteScalar() > 0;
                        
                        if (precoPequenoExists)
                        {
                            // Migração: Adicionar coluna Preco como nullable primeiro
                            checkTable.CommandText = "ALTER TABLE [dbo].[Produtos] ADD [Preco] [decimal](10, 2) NULL";
                            checkTable.ExecuteNonQuery();
                            
                            // Migrar dados: usar PrecoGrande se > 0, senão PrecoPequeno
                            checkTable.CommandText = @"
                                UPDATE [dbo].[Produtos] 
                                SET [Preco] = CASE 
                                    WHEN [PrecoGrande] > 0 THEN [PrecoGrande]
                                    ELSE [PrecoPequeno]
                                END";
                            checkTable.ExecuteNonQuery();
                            
                            // Tornar NOT NULL após migração
                            checkTable.CommandText = "ALTER TABLE [dbo].[Produtos] ALTER COLUMN [Preco] [decimal](10, 2) NOT NULL";
                            checkTable.ExecuteNonQuery();
                            
                            // Remover colunas antigas
                            try
                            {
                                checkTable.CommandText = "ALTER TABLE [dbo].[Produtos] DROP COLUMN [PrecoPequeno]";
                                checkTable.ExecuteNonQuery();
                            }
                            catch { /* Ignorar se já removida */ }
                            
                            try
                            {
                                checkTable.CommandText = "ALTER TABLE [dbo].[Produtos] DROP COLUMN [PrecoGrande]";
                                checkTable.ExecuteNonQuery();
                            }
                            catch { /* Ignorar se já removida */ }
                        }
                        else
                        {
                            // Se não tem PrecoPequeno, apenas adicionar Preco com valor padrão
                            checkTable.CommandText = "ALTER TABLE [dbo].[Produtos] ADD [Preco] [decimal](10, 2) NOT NULL DEFAULT(0)";
                            checkTable.ExecuteNonQuery();
                        }
                    }

                    // Verificar se a tabela StatusReserva existe (DEVE SER CRIADA ANTES DE Reservas)
                    checkTable.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StatusReserva]') AND type in (N'U'))
                        BEGIN
                            CREATE TABLE [dbo].[StatusReserva](
                                [Id] [int] IDENTITY(1,1) NOT NULL,
                                [Nome] [nvarchar](100) NOT NULL,
                                [Descricao] [nvarchar](500) NOT NULL,
                                [PermiteAlteracao] [bit] NOT NULL DEFAULT(1),
                                [PermiteExclusao] [bit] NOT NULL DEFAULT(1),
                                [Ordem] [int] NOT NULL DEFAULT(0),
                                CONSTRAINT [PK_StatusReserva] PRIMARY KEY CLUSTERED ([Id] ASC),
                                CONSTRAINT [UQ_StatusReserva_Nome] UNIQUE([Nome])
                            )
                        END";
                    checkTable.ExecuteNonQuery();

                    // Inserir status padrão se a tabela estiver vazia
                    checkTable.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM StatusReserva)
                        BEGIN
                            INSERT INTO StatusReserva (Nome, Descricao, PermiteAlteracao, PermiteExclusao, Ordem) VALUES
                            ('Aberta', 'Reserva dentro do período que permite alterações e cancelamento', 1, 1, 1),
                            ('Em Produção', 'Já está sendo produzida os produtos da reserva, não permite alteração nem exclusão', 0, 0, 2),
                            ('Produção Pronta', 'Já foi produzido', 0, 0, 3),
                            ('Preparando Entrega', 'Já está sendo preparado para entrega', 0, 0, 4),
                            ('Saiu para Entrega', 'Já está entregando', 0, 0, 5),
                            ('Já Entregue', 'Produtos já entregues', 0, 0, 6),
                            ('Cancelado', 'Reserva cancelada', 0, 0, 7)
                        END";
                    checkTable.ExecuteNonQuery();
                    
                    // Garantir que o status "Cancelado" existe (caso a tabela já tenha sido criada sem ele)
                    checkTable.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM StatusReserva WHERE Nome = 'Cancelado')
                        BEGIN
                            INSERT INTO StatusReserva (Nome, Descricao, PermiteAlteracao, PermiteExclusao, Ordem)
                            VALUES ('Cancelado', 'Reserva cancelada', 0, 0, 7)
                        END";
                    checkTable.ExecuteNonQuery();

                    // Verificar se a tabela Clientes existe (DEVE SER CRIADA ANTES DE Reservas)
                    checkTable.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Clientes]') AND type in (N'U'))
                        BEGIN
                            CREATE TABLE [dbo].[Clientes](
                                [Id] [int] IDENTITY(1,1) NOT NULL,
                                [Nome] [nvarchar](200) NOT NULL,
                                [Email] [nvarchar](200) NULL,
                                [Senha] [nvarchar](500) NULL,
                                [Telefone] [nvarchar](50) NULL,
                                [TemWhatsApp] [bit] NOT NULL DEFAULT(0),
                                [Provider] [nvarchar](50) NULL,
                                [ProviderId] [nvarchar](200) NULL,
                                [TokenConfirmacao] [nvarchar](100) NULL,
                                [TokenRecuperacaoSenha] [nvarchar](100) NULL,
                                [DataExpiracaoRecuperacaoSenha] [datetime] NULL,
                                [EmailConfirmado] [bit] NOT NULL DEFAULT(0),
                                [WhatsAppConfirmado] [bit] NOT NULL DEFAULT(0),
                                [IsAdmin] [bit] NOT NULL DEFAULT(0),
                                [DataCadastro] [datetime] NOT NULL DEFAULT(GETDATE()),
                                [UltimoAcesso] [datetime] NULL,
                                CONSTRAINT [PK_Clientes] PRIMARY KEY CLUSTERED ([Id] ASC)
                            )
                            CREATE UNIQUE INDEX [IX_Clientes_Email] ON [dbo].[Clientes]([Email]) WHERE [Email] IS NOT NULL
                            CREATE INDEX [IX_Clientes_Provider] ON [dbo].[Clientes]([Provider], [ProviderId])
                        END";
                    checkTable.ExecuteNonQuery();
                    
                    // Criar índices adicionais para otimização (se não existirem)
                    try
                    {
                        checkTable.CommandText = @"
                            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Clientes_Telefone' AND object_id = OBJECT_ID('Clientes'))
                            BEGIN
                                CREATE INDEX [IX_Clientes_Telefone] ON [dbo].[Clientes]([Telefone]) WHERE [Telefone] IS NOT NULL
                            END";
                        checkTable.ExecuteNonQuery();
                    }
                    catch { /* Ignorar se já existe */ }

                    // Atualizar administradores: definir IsAdmin = true para emails específicos
                    checkTable.CommandText = @"
                        UPDATE Clientes 
                        SET IsAdmin = 1 
                        WHERE LOWER(LTRIM(RTRIM(Email))) IN (
                            LOWER(LTRIM(RTRIM('wilson2071@gmail.com'))),
                            LOWER(LTRIM(RTRIM('isanfm@gmail.com'))),
                            LOWER(LTRIM(RTRIM('camilafermagalhaes@gmail.com')))
                        )";
                    try { checkTable.ExecuteNonQuery(); } catch { /* Ignorar se houver erro */ }

                    // Garantir que apenas os emails específicos sejam administradores
                    checkTable.CommandText = @"
                        UPDATE Clientes 
                        SET IsAdmin = 0 
                        WHERE LOWER(LTRIM(RTRIM(Email))) NOT IN (
                            LOWER(LTRIM(RTRIM('wilson2071@gmail.com'))),
                            LOWER(LTRIM(RTRIM('isanfm@gmail.com'))),
                            LOWER(LTRIM(RTRIM('camilafermagalhaes@gmail.com')))
                        ) AND (Email IS NOT NULL AND Email != '')";
                    try { checkTable.ExecuteNonQuery(); } catch { /* Ignorar se houver erro */ }

                    // Verificar se a tabela Reservas existe
                    // Primeiro, obter o ID do status 'Aberta' para usar como DEFAULT
                    checkTable.CommandText = "SELECT Id FROM StatusReserva WHERE Nome = 'Aberta'";
                    object statusAbertaIdObj = null;
                    try
                    {
                        statusAbertaIdObj = checkTable.ExecuteScalar();
                    }
                    catch { /* Ignorar se não encontrar */ }
                    
                    int statusAbertaId = 1; // Valor padrão
                    if (statusAbertaIdObj != null && statusAbertaIdObj != DBNull.Value)
                    {
                        statusAbertaId = Convert.ToInt32(statusAbertaIdObj);
                    }
                    
                    checkTable.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Reservas]') AND type in (N'U'))
                        BEGIN
                            -- Criar tabela
                            CREATE TABLE [dbo].[Reservas](
                                [Id] [int] IDENTITY(1,1) NOT NULL,
                                [ClienteId] [int] NOT NULL,
                                [DataRetirada] [datetime] NOT NULL,
                                [DataReserva] [datetime] NOT NULL,
                                [StatusId] [int] NOT NULL,
                                [ValorTotal] [decimal](10, 2) NOT NULL,
                                [Observacoes] [nvarchar](1000) NULL,
                                [ConvertidoEmPedido] [bit] NOT NULL DEFAULT(0),
                                [PrevisaoEntrega] [datetime] NULL,
                                [Cancelado] [bit] NOT NULL DEFAULT(0),
                                [TokenAcesso] [nvarchar](100) NULL,
                                CONSTRAINT [PK_Reservas] PRIMARY KEY CLUSTERED ([Id] ASC),
                                CONSTRAINT [FK_Reservas_Clientes] FOREIGN KEY([ClienteId]) 
                                    REFERENCES [dbo].[Clientes] ([Id]),
                                CONSTRAINT [FK_Reservas_StatusReserva] FOREIGN KEY([StatusId]) 
                                    REFERENCES [dbo].[StatusReserva] ([Id])
                            )
                        END";
                    checkTable.ExecuteNonQuery();
                    
                    // Adicionar DEFAULT para StatusId após criar a tabela (em comando separado)
                    // Usar o valor já obtido anteriormente (statusAbertaId)
                    checkTable.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Reservas_StatusId')
                        BEGIN
                            ALTER TABLE [dbo].[Reservas] ADD CONSTRAINT [DF_Reservas_StatusId] DEFAULT (" + statusAbertaId + @") FOR [StatusId]
                        END";
                    try
                    {
                        checkTable.ExecuteNonQuery();
                    }
                    catch { /* Ignorar se já existir */ }

                    // Adicionar novas colunas se a tabela já existir
                    checkTable.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Reservas]') AND name = 'ConvertidoEmPedido')
                        BEGIN
                            ALTER TABLE [dbo].[Reservas] ADD [ConvertidoEmPedido] [bit] NOT NULL DEFAULT(0)
                        END
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Reservas]') AND name = 'PrevisaoEntrega')
                        BEGIN
                            ALTER TABLE [dbo].[Reservas] ADD [PrevisaoEntrega] [datetime] NULL
                        END
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Reservas]') AND name = 'Cancelado')
                        BEGIN
                            ALTER TABLE [dbo].[Reservas] ADD [Cancelado] [bit] NOT NULL DEFAULT(0)
                        END
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Produtos]') AND name = 'EhSacoPromocional')
                        BEGIN
                            ALTER TABLE [dbo].[Produtos] ADD [EhSacoPromocional] [bit] NOT NULL DEFAULT(0)
                        END
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Produtos]') AND name = 'QuantidadeSaco')
                        BEGIN
                            ALTER TABLE [dbo].[Produtos] ADD [QuantidadeSaco] [int] NOT NULL DEFAULT(0)
                        END
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Produtos]') AND name = 'Produtos')
                        BEGIN
                            ALTER TABLE [dbo].[Produtos] ADD [Produtos] [nvarchar](500) NULL
                        END
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Produtos]') AND name = 'ReservavelAte')
                        BEGIN
                            ALTER TABLE [dbo].[Produtos] ADD [ReservavelAte] [datetime] NULL
                        END
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Produtos]') AND name = 'VendivelAte')
                        BEGIN
                            ALTER TABLE [dbo].[Produtos] ADD [VendivelAte] [datetime] NULL
                        END
                        -- Migração: Remover TamanhoSaco se existir
                        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Produtos]') AND name = 'TamanhoSaco')
                        BEGIN
                            ALTER TABLE [dbo].[Produtos] DROP COLUMN [TamanhoSaco]
                        END";
                    checkTable.ExecuteNonQuery();
                    
                    // Migração: Remover colunas duplicadas Nome, Email, Telefone se existirem
                    // Executar em comandos separados para evitar problemas de sintaxe
                    checkTable.CommandText = @"
                        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Reservas]') AND name = 'Nome')
                        BEGIN
                            ALTER TABLE [dbo].[Reservas] DROP COLUMN [Nome]
                        END";
                    try { checkTable.ExecuteNonQuery(); } catch { /* Ignorar se já foi removida */ }
                    
                    checkTable.CommandText = @"
                        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Reservas]') AND name = 'Email')
                        BEGIN
                            ALTER TABLE [dbo].[Reservas] DROP COLUMN [Email]
                        END";
                    try { checkTable.ExecuteNonQuery(); } catch { /* Ignorar se já foi removida */ }
                    
                    checkTable.CommandText = @"
                        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Reservas]') AND name = 'Telefone')
                        BEGIN
                            ALTER TABLE [dbo].[Reservas] DROP COLUMN [Telefone]
                        END";
                    try { checkTable.ExecuteNonQuery(); } catch { /* Ignorar se já foi removida */ }

                    // Verificar se a tabela ReservaItens existe
                    checkTable.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReservaItens]') AND type in (N'U'))
                        BEGIN
                            CREATE TABLE [dbo].[ReservaItens](
                                [Id] [int] IDENTITY(1,1) NOT NULL,
                                [ReservaId] [int] NOT NULL,
                                [ProdutoId] [int] NOT NULL,
                                [NomeProduto] [nvarchar](200) NOT NULL,
                                [Tamanho] [nvarchar](50) NOT NULL,
                                [Quantidade] [int] NOT NULL,
                                [PrecoUnitario] [decimal](10, 2) NOT NULL,
                                [Subtotal] [decimal](10, 2) NOT NULL,
                                [Produtos] [nvarchar](500) NULL,
                                CONSTRAINT [PK_ReservaItens] PRIMARY KEY CLUSTERED ([Id] ASC),
                                CONSTRAINT [FK_ReservaItens_Reservas] FOREIGN KEY([ReservaId]) 
                                    REFERENCES [dbo].[Reservas] ([Id]) ON DELETE CASCADE,
                                CONSTRAINT [FK_ReservaItens_Produtos] FOREIGN KEY([ProdutoId]) 
                                    REFERENCES [dbo].[Produtos] ([Id])
                            )
                        END";
                    checkTable.ExecuteNonQuery();
                    
                    // Adicionar coluna Produtos se não existir (migração)
                    checkTable.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ReservaItens]') AND name = 'Produtos')
                        BEGIN
                            ALTER TABLE [dbo].[ReservaItens] ADD [Produtos] [nvarchar](500) NULL
                        END";
                    try { checkTable.ExecuteNonQuery(); } catch { /* Ignorar se já existe */ }
                    
                    // Criar índices adicionais para otimização
                    try
                    {
                        checkTable.CommandText = @"
                            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ReservaItens_ReservaId' AND object_id = OBJECT_ID('ReservaItens'))
                            BEGIN
                                CREATE INDEX [IX_ReservaItens_ReservaId] ON [dbo].[ReservaItens]([ReservaId])
                            END";
                        checkTable.ExecuteNonQuery();
                    }
                    catch { /* Ignorar se já existe */ }
                    
                    try
                    {
                        checkTable.CommandText = @"
                            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Produtos_Ativo' AND object_id = OBJECT_ID('Produtos'))
                            BEGIN
                                CREATE INDEX [IX_Produtos_Ativo] ON [dbo].[Produtos]([Ativo], [Ordem]) WHERE [Ativo] = 1
                            END";
                        checkTable.ExecuteNonQuery();
                    }
                    catch { /* Ignorar se já existe */ }
                    
                    try
                    {
                        checkTable.CommandText = @"
                            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Reservas_ClienteId' AND object_id = OBJECT_ID('Reservas'))
                            BEGIN
                                CREATE INDEX [IX_Reservas_ClienteId] ON [dbo].[Reservas]([ClienteId])
                            END";
                        checkTable.ExecuteNonQuery();
                    }
                    catch { /* Ignorar se já existe */ }

                    // Migração: Alterar coluna Email para permitir NULL se ainda não permitir
                    checkTable.CommandText = @"
                        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Clientes]') AND name = 'Email' AND is_nullable = 0)
                        BEGIN
                            -- Remover índice único se existir
                            IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Clientes]') AND name = 'IX_Clientes_Email')
                            BEGIN
                                DROP INDEX [IX_Clientes_Email] ON [dbo].[Clientes]
                            END
                            
                            -- Alterar coluna para permitir NULL
                            ALTER TABLE [dbo].[Clientes] ALTER COLUMN [Email] [nvarchar](200) NULL
                            
                            -- Recriar índice único filtrado (apenas para emails não nulos)
                            CREATE UNIQUE INDEX [IX_Clientes_Email] ON [dbo].[Clientes]([Email]) WHERE [Email] IS NOT NULL
                        END";
                    checkTable.ExecuteNonQuery();

                    // Adicionar coluna Senha se não existir
                    checkTable.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Clientes]') AND name = 'Senha')
                        BEGIN
                            ALTER TABLE [dbo].[Clientes] ADD [Senha] [nvarchar](500) NULL
                        END";
                    checkTable.ExecuteNonQuery();

                    // Adicionar coluna IsAdmin se não existir
                    checkTable.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Clientes]') AND name = 'IsAdmin')
                        BEGIN
                            ALTER TABLE [dbo].[Clientes] ADD [IsAdmin] [bit] NOT NULL DEFAULT(0)
                        END";
                    checkTable.ExecuteNonQuery();
                    
                    // Garantir que a coluna computada para telefone sem formatação existe
                    checkTable.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.computed_columns WHERE name = 'TelefoneSemFormatacao' AND object_id = OBJECT_ID(N'[dbo].[Clientes]'))
                        BEGIN
                            ALTER TABLE [dbo].[Clientes] ADD [TelefoneSemFormatacao] AS 
                                LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ISNULL(Telefone, ''), '(', ''), ')', ''), '-', ''), ' ', ''), '.', ''), '/', ''))) PERSISTED
                        END";
                    try { checkTable.ExecuteNonQuery(); } catch { /* Ignorar se já existe ou houver erro */ }
                    
                    // Garantir que o índice único de Telefone existe
                    checkTable.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Clientes_Telefone' AND object_id = OBJECT_ID(N'[dbo].[Clientes]'))
                        BEGIN
                            CREATE UNIQUE INDEX [IX_Clientes_Telefone] ON [dbo].[Clientes]([TelefoneSemFormatacao]) 
                            WHERE [TelefoneSemFormatacao] IS NOT NULL AND [TelefoneSemFormatacao] != ''
                        END";
                    try { checkTable.ExecuteNonQuery(); } catch { /* Ignorar se já existe ou houver erro */ }

                    // Adicionar coluna TokenRecuperacaoSenha se não existir
                    checkTable.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Clientes]') AND name = 'TokenRecuperacaoSenha')
                        BEGIN
                            ALTER TABLE [dbo].[Clientes] ADD [TokenRecuperacaoSenha] [nvarchar](100) NULL
                        END";
                    checkTable.ExecuteNonQuery();

                    // Adicionar coluna DataExpiracaoRecuperacaoSenha se não existir
                    checkTable.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Clientes]') AND name = 'DataExpiracaoRecuperacaoSenha')
                        BEGIN
                            ALTER TABLE [dbo].[Clientes] ADD [DataExpiracaoRecuperacaoSenha] [datetime] NULL
                        END";
                    checkTable.ExecuteNonQuery();

                    // Adicionar ClienteId e TokenAcesso na tabela Reservas se não existir (migração para tabelas antigas)
                    checkTable.CommandText = @"
                        IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Reservas]') AND type in (N'U'))
                        BEGIN
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Reservas]') AND name = 'ClienteId')
                            BEGIN
                                ALTER TABLE [dbo].[Reservas] ADD [ClienteId] [int] NULL
                                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Reservas_Clientes')
                                BEGIN
                                    ALTER TABLE [dbo].[Reservas] ADD CONSTRAINT [FK_Reservas_Clientes] 
                                        FOREIGN KEY([ClienteId]) REFERENCES [dbo].[Clientes] ([Id])
                                END
                            END
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Reservas]') AND name = 'TokenAcesso')
                            BEGIN
                                ALTER TABLE [dbo].[Reservas] ADD [TokenAcesso] [nvarchar](100) NULL
                            END
                        END";
                    checkTable.ExecuteNonQuery();

                    // Migração: Adicionar coluna StatusId se não existir (executar sempre que a tabela Reservas existir)
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        // Verificar se a tabela Reservas existe
                        var checkReservasExists = new SqlCommand("SELECT COUNT(*) FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Reservas]') AND type in (N'U')", connection);
                        var reservasExists = (int)checkReservasExists.ExecuteScalar() > 0;
                        
                        if (reservasExists)
                        {
                            // Verificar se StatusId não existe
                            var checkStatusIdExists = new SqlCommand("SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Reservas]') AND name = 'StatusId'", connection);
                            var statusIdExists = (int)checkStatusIdExists.ExecuteScalar() > 0;
                            
                            if (!statusIdExists)
                            {
                                // Garantir que o status 'Aberta' existe
                                checkTable.CommandText = @"
                                    IF NOT EXISTS (SELECT * FROM StatusReserva WHERE Nome = 'Aberta')
                                    BEGIN
                                        INSERT INTO StatusReserva (Nome, Descricao, PermiteAlteracao, PermiteExclusao, Ordem)
                                        VALUES ('Aberta', 'Reserva dentro do período que permite alterações e cancelamento', 1, 1, 1)
                                    END";
                                checkTable.ExecuteNonQuery();
                                
                                // Obter ID do status 'Aberta'
                                checkTable.CommandText = "SELECT Id FROM StatusReserva WHERE Nome = 'Aberta'";
                                object statusAbertaIdObjMig = checkTable.ExecuteScalar();
                                int statusAbertaIdMig = statusAbertaIdObjMig != null && statusAbertaIdObjMig != DBNull.Value ? Convert.ToInt32(statusAbertaIdObjMig) : 1;
                                
                                // Adicionar coluna StatusId
                                checkTable.CommandText = "ALTER TABLE [dbo].[Reservas] ADD [StatusId] [int] NULL";
                                checkTable.ExecuteNonQuery();
                                
                                // Verificar se a coluna Status existe para migrar dados
                                var checkStatusExists = new SqlCommand("SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Reservas]') AND name = 'Status'", connection);
                                var statusExists = (int)checkStatusExists.ExecuteScalar() > 0;
                                
                                if (statusExists)
                                {
                                    // Migrar dados de Status para StatusId
                                    checkTable.CommandText = @"
                                        UPDATE r SET r.StatusId = sr.Id
                                        FROM Reservas r
                                        LEFT JOIN StatusReserva sr ON r.Status = sr.Nome
                                        WHERE r.StatusId IS NULL";
                                    checkTable.ExecuteNonQuery();
                                    
                                    // Se algum Status não foi encontrado, usar 'Aberta' como padrão
                                    checkTable.CommandText = "UPDATE Reservas SET StatusId = " + statusAbertaIdMig + " WHERE StatusId IS NULL";
                                    checkTable.ExecuteNonQuery();
                                }
                                else
                                {
                                    // Se não tem Status, definir todos como 'Aberta'
                                    checkTable.CommandText = "UPDATE Reservas SET StatusId = " + statusAbertaIdMig + " WHERE StatusId IS NULL";
                                    checkTable.ExecuteNonQuery();
                                }
                                
                                // Tornar StatusId NOT NULL
                                checkTable.CommandText = "ALTER TABLE [dbo].[Reservas] ALTER COLUMN [StatusId] [int] NOT NULL";
                                checkTable.ExecuteNonQuery();
                                
                                // Adicionar foreign key
                                checkTable.CommandText = @"
                                    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Reservas_StatusReserva')
                                    BEGIN
                                        ALTER TABLE [dbo].[Reservas] ADD CONSTRAINT [FK_Reservas_StatusReserva] 
                                            FOREIGN KEY([StatusId]) REFERENCES [dbo].[StatusReserva] ([Id])
                                    END";
                                checkTable.ExecuteNonQuery();
                                
                                // Remover coluna Status antiga se existir
                                if (statusExists)
                                {
                                    checkTable.CommandText = "ALTER TABLE [dbo].[Reservas] DROP COLUMN [Status]";
                                    try { checkTable.ExecuteNonQuery(); } catch { /* Ignorar se houver erro */ }
                                }
                            }
                            
                            // Garantir que a foreign key existe
                            checkTable.CommandText = @"
                                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Reservas]') AND name = 'StatusId')
                                AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Reservas_StatusReserva')
                                BEGIN
                                    ALTER TABLE [dbo].[Reservas] ADD CONSTRAINT [FK_Reservas_StatusReserva] 
                                        FOREIGN KEY([StatusId]) REFERENCES [dbo].[StatusReserva] ([Id])
                                END";
                            try { checkTable.ExecuteNonQuery(); } catch { /* Ignorar se já existe */ }
                        }
                    }

                    // Limpar dados das tabelas Reservas e ReservaItens (apenas na primeira inicialização)
                    // COMENTADO: Não limpar dados automaticamente - isso apaga reservas existentes
                    // Se precisar limpar, fazer manualmente
                    // checkTable.CommandText = @"DELETE FROM ReservaItens; DELETE FROM Reservas";
                    // try { checkTable.ExecuteNonQuery(); } catch { /* Ignorar se houver erro */ }

                    // Verificar se há produtos no banco
                    var checkProdutos = new SqlCommand("SELECT COUNT(*) FROM Produtos", connection);
                    var countProdutos = (int)checkProdutos.ExecuteScalar();
                    
                    // Se não houver produtos, inserir produtos padrão
                    if (countProdutos == 0)
                    {
                        // Inserir novos produtos
                        var insertData = new SqlCommand(@"
                        INSERT INTO Produtos (Nome, Descricao, Preco, ImagemUrl, Ativo, Ordem, EhSacoPromocional, QuantidadeSaco, Produtos, ReservavelAte, VendivelAte) VALUES
                        ('Gingerbread Estrela Pequeno', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Estrela Pequeno', 5.00, 'Images/estrela-pequeno.jpg', 1, 1, 0, 0, NULL, NULL, NULL),
                        ('Gingerbread Estrela Grande', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Estrela Grande', 10.00, 'Images/estrela-grande.jpg', 1, 2, 0, 0, NULL, NULL, NULL),
                        ('Gingerbread Floco de Neve Pequeno', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Floco de Neve Pequeno', 5.00, 'Images/floco-neve-pequeno.jpg', 1, 3, 0, 0, NULL, NULL, NULL),
                        ('Gingerbread Floco de Neve Grande', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Floco de Neve Grande', 10.00, 'Images/floco-neve-grande.jpg', 1, 4, 0, 0, NULL, NULL, NULL),
                        ('Gingerbread Guirlanda Pequeno', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Guirlanda Pequeno', 5.00, 'Images/guirlanda-pequeno.jpg', 1, 5, 0, 0, NULL, NULL, NULL),
                        ('Gingerbread Guirlanda Grande', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Guirlanda Grande', 10.00, 'Images/guirlanda-grande.jpg', 1, 6, 0, 0, NULL, NULL, NULL),
                        ('Gingerbread Meia Pequeno', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Meia Pequeno', 5.00, 'Images/meia-pequeno.jpg', 1, 7, 0, 0, NULL, NULL, NULL),
                        ('Gingerbread Meia Grande', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Meia Grande', 10.00, 'Images/meia-grande.jpg', 1, 8, 0, 0, NULL, NULL, NULL),
                        ('Gingerbread Árvore Pequeno', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Árvore Pequeno', 5.00, 'Images/arvore-pequeno.jpg', 1, 9, 0, 0, NULL, NULL, NULL),
                        ('Gingerbread Árvore Grande', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Árvore Grande', 10.00, 'Images/arvore-grande.jpg', 1, 10, 0, 0, NULL, NULL, NULL),
                        ('Gingerbread Coração Pequeno', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Coração Pequeno', 5.00, 'Images/coracao-pequeno.jpg', 1, 11, 0, 0, NULL, NULL, NULL),
                        ('Gingerbread Coração Grande', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Coração Grande', 10.00, 'Images/coracao-grande.jpg', 1, 12, 0, 0, NULL, NULL, NULL),
                        ('Gingerbread Boneco Pequeno', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Boneco Pequeno', 5.00, 'Images/boneco-pequeno.jpg', 1, 13, 0, 0, NULL, NULL, NULL),
                        ('Gingerbread Boneco Grande', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Boneco Grande', 10.00, 'Images/boneco-grande.jpg', 1, 14, 0, 0, NULL, NULL, NULL),
                        ('Saco Promocional - 6 Pequenos', 'Saco com 6 biscoitos pequenos (escolha os formatos). De R$ 30,00 por R$ 21,00 na promoção!', 21.00, 'Images/saco-6-pequenos.jpg', 1, 15, 1, 6, '[1,3,5,7,9,11,13]', NULL, NULL),
                        ('Saco Promocional - 3 Grandes', 'Saco com 3 biscoitos grandes (escolha os formatos). De R$ 30,00 por R$ 21,00 na promoção!', 21.00, 'Images/saco-3-grandes.jpg', 1, 16, 1, 3, '[2,4,6,8,10,12,14]', NULL, NULL)", connection);
                        insertData.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                string mensagemErro = "Erro ao acessar o banco de dados. ";
                
                // Mensagens mais específicas baseadas no erro
                if (sqlEx.Message.Contains("não foi encontrado") || sqlEx.Message.Contains("não estava acessível"))
                {
                    mensagemErro += "O SQL Server LocalDB não foi encontrado ou não está acessível. ";
                    mensagemErro += "Verifique se o LocalDB está instalado. ";
                    mensagemErro += "Instâncias tentadas: (localdb)\\MSSQLLocalDB, .\\MSSQLLOCALDB, (localdb)\\v11.0. ";
                    mensagemErro += "Para instalar o LocalDB, baixe o SQL Server Express LocalDB da Microsoft. ";
                }
                else
                {
                    mensagemErro += "Verifique se o SQL Server LocalDB está instalado e funcionando. ";
                }
                
                mensagemErro += "Erro detalhado: " + sqlEx.Message;
                throw new Exception(mensagemErro, sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao inicializar o banco de dados: " + ex.Message, ex);
            }
        }

        public List<Produto> ObterProdutos()
        {
            // Cache de 10 minutos para lista de produtos ativos (aumentado para melhor performance)
            string cacheKey = "Produtos_Ativos";
            List<Produto> cached = null;
            
            // Verificar se HttpRuntime está disponível (pode não estar em alguns contextos)
            if (HttpRuntime.Cache != null)
            {
                cached = HttpRuntime.Cache.Get(cacheKey) as List<Produto>;
                if (cached != null)
                {
                    return cached;
                }
            }
            
            var produtos = new List<Produto>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                // Adicionar timeout curto na connection string se não tiver
                if (!_connectionString.Contains("Connect Timeout="))
                {
                    connection.ConnectionString = _connectionString.TrimEnd(';') + ";Connect Timeout=5";
                }
                connection.Open();
                // Query otimizada - apenas campos necessários
                var command = new SqlCommand("SELECT Id, Nome, Descricao, Preco, ImagemUrl, Ativo, Ordem, ISNULL(EhSacoPromocional, 0), ISNULL(QuantidadeSaco, 0), ISNULL(Produtos, ''), ReservavelAte, VendivelAte FROM Produtos WHERE Ativo = 1 ORDER BY Ordem", connection);
                command.CommandTimeout = 5; // Timeout curto para não bloquear
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        produtos.Add(new Produto
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Descricao = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Preco = reader.GetDecimal(3),
                            ImagemUrl = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            Ativo = reader.GetBoolean(5),
                            Ordem = reader.GetInt32(6),
                            EhSacoPromocional = reader.IsDBNull(7) ? false : reader.GetBoolean(7),
                            QuantidadeSaco = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                            Produtos = reader.IsDBNull(9) ? null : reader.GetString(9),
                            ReservavelAte = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                            VendivelAte = reader.IsDBNull(11) ? (DateTime?)null : reader.GetDateTime(11)
                        });
                    }
                }
            }
            
            // Armazenar no cache por 10 minutos (se disponível) - aumentado para melhor performance
            if (HttpRuntime.Cache != null)
            {
                HttpRuntime.Cache.Insert(cacheKey, produtos, null, DateTime.Now.AddMinutes(10), System.Web.Caching.Cache.NoSlidingExpiration);
            }
            
            return produtos;
        }

        public List<Produto> ObterTodosProdutos()
        {
            // Cache de 5 minutos para lista de todos os produtos
            string cacheKey = "Produtos_Todos";
            List<Produto> cached = null;
            
            // Verificar se HttpRuntime está disponível
            if (HttpRuntime.Cache != null)
            {
                cached = HttpRuntime.Cache.Get(cacheKey) as List<Produto>;
                if (cached != null)
                {
                    return cached;
                }
            }
            
            var produtos = new List<Produto>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, Nome, Descricao, Preco, ImagemUrl, Ativo, Ordem, ISNULL(EhSacoPromocional, 0), ISNULL(QuantidadeSaco, 0), ISNULL(Produtos, ''), ReservavelAte, VendivelAte FROM Produtos ORDER BY Ordem", connection);
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        produtos.Add(new Produto
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Descricao = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Preco = reader.GetDecimal(3),
                            ImagemUrl = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            Ativo = reader.GetBoolean(5),
                            Ordem = reader.GetInt32(6),
                            EhSacoPromocional = reader.IsDBNull(7) ? false : reader.GetBoolean(7),
                            QuantidadeSaco = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                            Produtos = reader.IsDBNull(9) ? null : reader.GetString(9),
                            ReservavelAte = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                            VendivelAte = reader.IsDBNull(11) ? (DateTime?)null : reader.GetDateTime(11)
                        });
                    }
                }
            }
            
            // Armazenar no cache por 5 minutos (se disponível)
            if (HttpRuntime.Cache != null)
            {
                HttpRuntime.Cache.Insert(cacheKey, produtos, null, DateTime.Now.AddMinutes(5), System.Web.Caching.Cache.NoSlidingExpiration);
            }
            
            return produtos;
        }

        public List<Produto> ObterProdutosPorIds(List<int> ids)
        {
            var produtos = new List<Produto>();
            
            if (ids == null || ids.Count == 0)
            {
                return produtos;
            }
            
            // Validar e sanitizar IDs para prevenir SQL Injection
            var idsValidos = ids.Where(id => id > 0 && id <= int.MaxValue).Distinct().Take(1000).ToList();
            if (idsValidos.Count == 0)
            {
                return produtos;
            }
            
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                // Construir query com parâmetros para prevenir SQL Injection
                var parameters = new List<string>();
                var command = new SqlCommand();
                command.Connection = connection;
                
                for (int i = 0; i < idsValidos.Count; i++)
                {
                    string paramName = $"@Id{i}";
                    parameters.Add(paramName);
                    command.Parameters.AddWithValue(paramName, idsValidos[i]);
                }
                
                string query = $"SELECT Id, Nome, Descricao, Preco, ImagemUrl, Ativo, Ordem, ISNULL(EhSacoPromocional, 0), ISNULL(QuantidadeSaco, 0), ISNULL(Produtos, ''), ReservavelAte, VendivelAte FROM Produtos WHERE Ativo = 1 AND EhSacoPromocional = 0 AND Id IN ({string.Join(",", parameters)}) ORDER BY Ordem";
                command.CommandText = query;
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        produtos.Add(new Produto
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Descricao = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Preco = reader.GetDecimal(3),
                            ImagemUrl = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            Ativo = reader.GetBoolean(5),
                            Ordem = reader.GetInt32(6),
                            EhSacoPromocional = reader.IsDBNull(7) ? false : reader.GetBoolean(7),
                            QuantidadeSaco = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                            Produtos = reader.IsDBNull(9) ? null : reader.GetString(9),
                            ReservavelAte = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                            VendivelAte = reader.IsDBNull(11) ? (DateTime?)null : reader.GetDateTime(11)
                        });
                    }
                }
            }
            
            return produtos;
        }

        public Produto ObterProdutoPorId(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, Nome, Descricao, Preco, ImagemUrl, Ativo, Ordem, ISNULL(EhSacoPromocional, 0), ISNULL(QuantidadeSaco, 0), ISNULL(Produtos, ''), ReservavelAte, VendivelAte FROM Produtos WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Produto
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Descricao = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Preco = reader.GetDecimal(3),
                            ImagemUrl = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            Ativo = reader.GetBoolean(5),
                            Ordem = reader.GetInt32(6),
                            EhSacoPromocional = reader.IsDBNull(7) ? false : reader.GetBoolean(7),
                            QuantidadeSaco = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                            Produtos = reader.IsDBNull(9) ? null : reader.GetString(9),
                            ReservavelAte = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                            VendivelAte = reader.IsDBNull(11) ? (DateTime?)null : reader.GetDateTime(11)
                        };
                    }
                }
            }
            return null;
        }

        public void SalvarReserva(Reserva reserva)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                // Gerar token de acesso se não existir
                if (string.IsNullOrEmpty(reserva.TokenAcesso))
                {
                    reserva.TokenAcesso = Guid.NewGuid().ToString("N");
                }
                
                // Para novas reservas (Id = 0), SEMPRE usar StatusId de "Aberta"
                if (reserva.Id == 0)
                {
                    // Buscar o StatusId de "Aberta" uma única vez e cachear
                    if (!reserva.StatusId.HasValue || reserva.StatusId.Value <= 0)
                    {
                        var statusAberta = ObterStatusReservaPorNome("Aberta");
                        reserva.StatusId = statusAberta != null ? statusAberta.Id : 1;
                    }
                }
                else
                {
                    // Para atualizações, manter o StatusId existente ou obter pelo nome se necessário
                    if (!reserva.StatusId.HasValue)
                    {
                        if (!string.IsNullOrEmpty(reserva.Status))
                        {
                            var statusReserva = ObterStatusReservaPorNome(reserva.Status);
                            reserva.StatusId = statusReserva != null ? statusReserva.Id : 1;
                        }
                        else
                        {
                            // Se não tiver Status nem StatusId, usar "Aberta" como padrão
                            var statusAberta = ObterStatusReservaPorNome("Aberta");
                            reserva.StatusId = statusAberta != null ? statusAberta.Id : 1;
                        }
                    }
                }
                
                // Validação final: garantir que StatusId está definido
                if (!reserva.StatusId.HasValue || reserva.StatusId.Value <= 0)
                {
                    reserva.StatusId = 1; // Usar ID 1 como padrão (Aberta)
                }
                
                var command = new SqlCommand(@"
                    INSERT INTO Reservas (ClienteId, DataRetirada, DataReserva, StatusId, ValorTotal, Observacoes, ConvertidoEmPedido, PrevisaoEntrega, Cancelado, TokenAcesso)
                    VALUES (@ClienteId, @DataRetirada, @DataReserva, @StatusId, @ValorTotal, @Observacoes, @ConvertidoEmPedido, @PrevisaoEntrega, @Cancelado, @TokenAcesso);
                    SELECT CAST(SCOPE_IDENTITY() as int);", connection);
                
                command.Parameters.AddWithValue("@DataRetirada", reserva.DataRetirada);
                command.Parameters.AddWithValue("@DataReserva", reserva.DataReserva);
                command.Parameters.AddWithValue("@StatusId", reserva.StatusId.Value);
                command.Parameters.AddWithValue("@ValorTotal", reserva.ValorTotal);
                command.Parameters.AddWithValue("@Observacoes", reserva.Observacoes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ConvertidoEmPedido", reserva.ConvertidoEmPedido);
                command.Parameters.AddWithValue("@PrevisaoEntrega", reserva.PrevisaoEntrega.HasValue ? (object)reserva.PrevisaoEntrega.Value : DBNull.Value);
                command.Parameters.AddWithValue("@Cancelado", reserva.Cancelado);
                command.Parameters.AddWithValue("@ClienteId", reserva.ClienteId.HasValue ? (object)reserva.ClienteId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@TokenAcesso", reserva.TokenAcesso);
                
                var reservaId = (int)command.ExecuteScalar();
                
                // Atualizar o ID da reserva no objeto
                reserva.Id = reservaId;
                
                // Registrar log
                string usuarioLog = LogService.ObterUsuarioAtual(HttpContext.Current?.Session);
                string detalhes = $"ID: {reservaId}, ClienteId: {reserva.ClienteId}, ValorTotal: R$ {reserva.ValorTotal:F2}, Itens: {reserva.Itens?.Count ?? 0}";
                LogService.RegistrarInsercao(usuarioLog, "Reserva", "DatabaseService.SalvarReserva", detalhes);
                
                // Validar que há itens para gravar
                if (reserva.Itens == null || reserva.Itens.Count == 0)
                {
                    throw new Exception("A reserva não possui itens para gravar.");
                }

                // Validar que ClienteId está definido
                if (!reserva.ClienteId.HasValue || reserva.ClienteId.Value <= 0)
                {
                    throw new Exception("ClienteId não está definido. Apenas clientes logados podem fazer reservas.");
                }

                // Salvar TODOS os itens da reserva na tabela ReservaItens
                // Não validar novamente os produtos aqui, pois já foram validados antes de criar a reserva
                // Apenas tentar gravar todos os itens e tratar erros individualmente
                int itensGravados = 0;
                int itensPulados = 0;
                var produtosNaoEncontrados = new List<int>();
                
                // Otimização: Verificar todos os produtos de uma vez (em vez de um por um)
                var produtosIds = reserva.Itens.Select(i => i.ProdutoId).Distinct().ToList();
                var produtosExistentes = new HashSet<int>();
                
                if (produtosIds.Count > 0)
                {
                    // Query única para verificar todos os produtos
                    var idsParametros = string.Join(",", produtosIds.Select((id, idx) => $"@Id{idx}"));
                    var verificarProdutosCommand = new SqlCommand($"SELECT Id FROM Produtos WHERE Id IN ({idsParametros})", connection);
                    for (int i = 0; i < produtosIds.Count; i++)
                    {
                        verificarProdutosCommand.Parameters.AddWithValue($"@Id{i}", produtosIds[i]);
                    }
                    
                    using (var reader = verificarProdutosCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            produtosExistentes.Add(reader.GetInt32(0));
                        }
                    }
                }
                
                // Preparar comando de inserção uma vez (reutilizar)
                var itemCommand = new SqlCommand(@"
                    INSERT INTO ReservaItens (ReservaId, ProdutoId, NomeProduto, Tamanho, Quantidade, PrecoUnitario, Subtotal, Produtos)
                    VALUES (@ReservaId, @ProdutoId, @NomeProduto, @Tamanho, @Quantidade, @PrecoUnitario, @Subtotal, @Produtos)", connection);
                
                itemCommand.Parameters.Add("@ReservaId", SqlDbType.Int);
                itemCommand.Parameters.Add("@ProdutoId", SqlDbType.Int);
                itemCommand.Parameters.Add("@NomeProduto", SqlDbType.NVarChar, 200);
                itemCommand.Parameters.Add("@Tamanho", SqlDbType.NVarChar, 50);
                itemCommand.Parameters.Add("@Quantidade", SqlDbType.Int);
                itemCommand.Parameters.Add("@PrecoUnitario", SqlDbType.Decimal);
                itemCommand.Parameters.Add("@Subtotal", SqlDbType.Decimal);
                itemCommand.Parameters.Add("@Produtos", SqlDbType.NVarChar, 500);
                
                foreach (var item in reserva.Itens)
                {
                    try
                    {
                        // Verificar se o produto existe (usando HashSet - O(1))
                        if (!produtosExistentes.Contains(item.ProdutoId))
                        {
                            produtosNaoEncontrados.Add(item.ProdutoId);
                            itensPulados++;
                            continue;
                        }
                        
                        // Reutilizar comando preparado (mais rápido)
                        itemCommand.Parameters["@ReservaId"].Value = reservaId;
                        itemCommand.Parameters["@ProdutoId"].Value = item.ProdutoId;
                        itemCommand.Parameters["@NomeProduto"].Value = item.NomeProduto ?? "";
                        itemCommand.Parameters["@Tamanho"].Value = item.Tamanho ?? "";
                        itemCommand.Parameters["@Quantidade"].Value = item.Quantidade;
                        itemCommand.Parameters["@PrecoUnitario"].Value = item.PrecoUnitario;
                        itemCommand.Parameters["@Subtotal"].Value = item.Subtotal;
                        itemCommand.Parameters["@Produtos"].Value = item.Produtos ?? (object)DBNull.Value;
                        
                        itemCommand.ExecuteNonQuery();
                        itensGravados++;
                    }
                    catch (Exception ex)
                    {
                        itensPulados++;
                        // Continuar tentando gravar os outros itens
                        // Se for erro de produto não encontrado, adicionar à lista
                        if (ex.Message.Contains("FOREIGN KEY") || ex.Message.Contains("Produto"))
                        {
                            produtosNaoEncontrados.Add(item.ProdutoId);
                        }
                    }
                }
                
                // Validar que pelo menos um item foi gravado
                if (itensGravados == 0)
                {
                    // Se nenhum item foi gravado, deletar a reserva criada
                    try
                    {
                        var deleteCommand = new SqlCommand("DELETE FROM Reservas WHERE Id = @Id", connection);
                        deleteCommand.Parameters.AddWithValue("@Id", reservaId);
                        deleteCommand.ExecuteNonQuery();
                    }
                    catch (Exception)
                    {
                        // Erro ao deletar reserva vazia - continuar
                    }
                    
                    string mensagemErro = $"Nenhum item foi gravado na reserva. Total de itens: {reserva.Itens.Count}, Itens pulados: {itensPulados}.";
                    if (produtosNaoEncontrados.Count > 0)
                    {
                        mensagemErro += $" Produtos não encontrados no banco de dados (IDs: {string.Join(", ", produtosNaoEncontrados)}).";
                    }
                    mensagemErro += " A reserva foi removida.";
                    
                    throw new Exception(mensagemErro);
                }
            }
        }

        private string CriarMasterConnectionString(string connectionString)
        {
            // Criar connection string para master
            // Tentar diferentes instâncias do LocalDB em ordem de preferência
            string masterConnectionString = connectionString;
            
            // Remover AttachDbFilename
            if (masterConnectionString.Contains("AttachDbFilename="))
            {
                int attachStart = masterConnectionString.IndexOf("AttachDbFilename=");
                int attachEnd = masterConnectionString.IndexOf(";", attachStart);
                if (attachEnd == -1) attachEnd = masterConnectionString.Length;
                masterConnectionString = masterConnectionString.Remove(attachStart, attachEnd - attachStart);
            }
            
            // Remover Initial Catalog
            if (masterConnectionString.Contains("Initial Catalog="))
            {
                int catalogStart = masterConnectionString.IndexOf("Initial Catalog=");
                int catalogEnd = masterConnectionString.IndexOf(";", catalogStart);
                if (catalogEnd == -1) catalogEnd = masterConnectionString.Length;
                masterConnectionString = masterConnectionString.Remove(catalogStart, catalogEnd - catalogStart);
            }
            
            // Extrair Data Source original para tentar diferentes variações
            string originalDataSource = "";
            if (masterConnectionString.Contains("Data Source="))
            {
                int dsStart = masterConnectionString.IndexOf("Data Source=") + "Data Source=".Length;
                int dsEnd = masterConnectionString.IndexOf(";", dsStart);
                if (dsEnd == -1) dsEnd = masterConnectionString.Length;
                originalDataSource = masterConnectionString.Substring(dsStart, dsEnd - dsStart).Trim();
            }
            
            // Lista de instâncias do LocalDB para tentar (em ordem de preferência)
            // Priorizar .\MSSQLLocalDB para produção
            string[] localDbInstances = {
                originalDataSource, // Tentar primeiro a instância original
                ".\\MSSQLLocalDB",  // Produção (prioridade)
                ".\\MSSQLLOCALDB",  // Produção (alternativa)
                "(localdb)\\MSSQLLocalDB",
                "(localdb)\\MSSQLLOCALDB",
                "(localdb)\\v11.0",
                "(localdb)\\ProjectsV13",
                "(localdb)\\ProjectsV12"
            };
            
            // Tentar cada instância até encontrar uma que funcione
            foreach (string instance in localDbInstances)
            {
                if (string.IsNullOrEmpty(instance)) continue;
                
                string testConnectionString = masterConnectionString;
                if (testConnectionString.Contains("Data Source="))
                {
                    int dsStart = testConnectionString.IndexOf("Data Source=");
                    int dsEnd = testConnectionString.IndexOf(";", dsStart);
                    if (dsEnd == -1) dsEnd = testConnectionString.Length;
                    testConnectionString = testConnectionString.Remove(dsStart, dsEnd - dsStart);
                }
                testConnectionString = "Data Source=" + instance + ";" + testConnectionString.TrimStart(';');
                testConnectionString = testConnectionString.TrimEnd(';') + ";Initial Catalog=master";
                
                // Testar conexão
                if (TestarConexaoLocalDB(testConnectionString))
                {
                    return testConnectionString;
                }
            }
            
            // Se nenhuma funcionou, usar a primeira opção (original ou .\MSSQLLOCALDB)
            string finalDataSource = !string.IsNullOrEmpty(originalDataSource) ? originalDataSource : ".\\MSSQLLOCALDB";
            if (masterConnectionString.Contains("Data Source="))
            {
                int dsStart = masterConnectionString.IndexOf("Data Source=");
                int dsEnd = masterConnectionString.IndexOf(";", dsStart);
                if (dsEnd == -1) dsEnd = masterConnectionString.Length;
                masterConnectionString = masterConnectionString.Remove(dsStart, dsEnd - dsStart);
            }
            masterConnectionString = "Data Source=" + finalDataSource + ";" + masterConnectionString.TrimStart(';');
            masterConnectionString = masterConnectionString.TrimEnd(';') + ";Initial Catalog=master";
            
            return masterConnectionString;
        }
        
        private bool TestarConexaoLocalDB(string connectionString)
        {
            try
            {
                // Adicionar timeout muito curto para teste rápido (1 segundo)
                string testConnectionString = connectionString;
                if (!testConnectionString.Contains("Connect Timeout="))
                {
                    testConnectionString = testConnectionString.TrimEnd(';') + ";Connect Timeout=1";
                }
                else
                {
                    // Substituir timeout existente por um menor para teste
                    testConnectionString = System.Text.RegularExpressions.Regex.Replace(
                        testConnectionString, 
                        @"Connect Timeout=\d+", 
                        "Connect Timeout=1");
                }
                
                using (var testConnection = new SqlConnection(testConnectionString))
                {
                    testConnection.Open();
                    return true;
                }
            }
            catch (SqlException)
            {
                // Não tentar iniciar LocalDB durante teste - isso causa lentidão
                // A conexão real tentará e dará erro mais claro se necessário
                return false;
            }
            catch
            {
                return false;
            }
        }
        
        private void TentarIniciarLocalDB()
        {
            try
            {
                // Tentar iniciar o LocalDB usando sqllocaldb
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "sqllocaldb",
                    Arguments = "start MSSQLLocalDB",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                
                using (Process process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        process.WaitForExit(5000); // Aguardar até 5 segundos
                    }
                }
            }
            catch
            {
                // Se não conseguir iniciar, ignorar (pode não ter permissões ou não estar instalado)
            }
        }
        
        private string ExtrairNomeBanco(string connectionString)
        {
            string databaseName = "KingdomConfeitaria_Prod";
            if (connectionString.Contains("Initial Catalog="))
            {
                int startIdx = connectionString.IndexOf("Initial Catalog=") + "Initial Catalog=".Length;
                int endIdx = connectionString.IndexOf(";", startIdx);
                if (endIdx == -1) endIdx = connectionString.Length;
                databaseName = connectionString.Substring(startIdx, endIdx - startIdx).Trim();
            }
            return databaseName;
        }
        
        private bool VerificarBancoExiste(string masterConnectionString, string databaseName)
        {
            try
            {
                using (var masterConnection = new SqlConnection(masterConnectionString))
                {
                    masterConnection.Open();
                    // Usar parâmetro para evitar SQL injection e melhorar performance
                    var checkDbCommand = new SqlCommand("SELECT COUNT(*) FROM sys.databases WHERE name = @DatabaseName", masterConnection);
                    checkDbCommand.Parameters.AddWithValue("@DatabaseName", databaseName);
                    checkDbCommand.CommandTimeout = 5; // Timeout curto para verificação rápida
                    int dbExists = (int)checkDbCommand.ExecuteScalar();
                    return dbExists > 0;
                }
            }
            catch
            {
                return false;
            }
        }
        
        private void CriarNovoBanco(string masterConnectionString, string databaseName, string attachDbPath)
        {
            using (var masterConnection = new SqlConnection(masterConnectionString))
            {
                masterConnection.Open();
                string logPath = attachDbPath.Replace(".mdf", "_log.ldf");
                var createDbCommand = new SqlCommand($@"
                    CREATE DATABASE [{databaseName}]
                    ON (NAME = '{databaseName}', FILENAME = '{attachDbPath.Replace("\\", "\\\\")}')
                    LOG ON (NAME = '{databaseName}_Log', FILENAME = '{logPath.Replace("\\", "\\\\")}')", masterConnection);
                createDbCommand.ExecuteNonQuery();
            }
        }
        
        private void FazerAttachBanco(string masterConnectionString, string databaseName, string attachDbPath)
        {
            // Primeiro, verificar se o banco já existe e fazer detach se necessário
            try
            {
                bool bancoExiste = VerificarBancoExiste(masterConnectionString, databaseName);
                if (bancoExiste)
                {
                    // Banco já existe - fazer detach primeiro
                    try
                    {
                        FazerDetachBanco(masterConnectionString, databaseName);
                    }
                    catch
                    {
                        // Se falhar, tentar detach forçado
                        FazerDetachBancoForcado(masterConnectionString, databaseName);
                    }
                }
            }
            catch
            {
                // Se falhar na verificação, continuar e tentar attach mesmo assim
            }
            
            using (var masterConnection = new SqlConnection(masterConnectionString))
            {
                masterConnection.Open();
                string logPath = attachDbPath.Replace(".mdf", "_log.ldf");
                
                // Tentar ATTACH com log file
                try
                {
                    var attachCommand = new SqlCommand($@"
                        CREATE DATABASE [{databaseName}]
                        ON (FILENAME = '{attachDbPath.Replace("\\", "\\\\")}')
                        FOR ATTACH", masterConnection);
                    attachCommand.ExecuteNonQuery();
                }
                catch
                {
                    // Se falhar, tentar ATTACH sem log file (SQL Server criará novo)
                    try
                    {
                        var attachCommand = new SqlCommand($@"
                            CREATE DATABASE [{databaseName}]
                            ON (FILENAME = '{attachDbPath.Replace("\\", "\\\\")}')
                            FOR ATTACH_REBUILD_LOG", masterConnection);
                        attachCommand.ExecuteNonQuery();
                    }
                    catch
                    {
                        // Se ainda falhar, propagar o erro
                        throw;
                    }
                }
            }
        }
        
        private void FazerDetachBanco(string masterConnectionString, string databaseName)
        {
            using (var masterConnection = new SqlConnection(masterConnectionString))
            {
                masterConnection.Open();
                var detachCommand = new SqlCommand($@"
                    ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    EXEC sp_detach_db '{databaseName}', 'true'", masterConnection);
                detachCommand.ExecuteNonQuery();
            }
        }
        
        private void FazerDetachBancoForcado(string masterConnectionString, string databaseName)
        {
            using (var masterConnection = new SqlConnection(masterConnectionString))
            {
                masterConnection.Open();
                // Fechar todas as conexões e fazer detach forçado
                // Reduzir tempo de espera para acelerar
                var detachCommand = new SqlCommand($@"
                    -- Fechar todas as conexões ao banco
                    ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    -- Aguardar um pouco para garantir que todas as conexões foram fechadas (reduzido para 500ms)
                    WAITFOR DELAY '00:00:00.5';
                    -- Fazer detach
                    EXEC sp_detach_db '{databaseName}', 'true'", masterConnection);
                detachCommand.CommandTimeout = 15; // Timeout reduzido de 30 para 15 segundos
                detachCommand.ExecuteNonQuery();
            }
        }

        private void LimparCacheProdutos()
        {
            // Limpar cache de produtos quando houver atualização (se disponível)
            if (HttpRuntime.Cache != null)
            {
                HttpRuntime.Cache.Remove("Produtos_Ativos");
                HttpRuntime.Cache.Remove("Produtos_Todos");
            }
        }

        public void AtualizarProduto(Produto produto)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                // Buscar dados antigos para comparação
                var produtoAntigo = ObterProdutoPorId(produto.Id);
                var valoresAntigos = new Dictionary<string, string>();
                var valoresNovos = new Dictionary<string, string>();
                
                if (produtoAntigo != null)
                {
                    valoresAntigos["Nome"] = produtoAntigo.Nome ?? "null";
                    valoresAntigos["Descricao"] = produtoAntigo.Descricao ?? "null";
                    valoresAntigos["Preco"] = produtoAntigo.Preco.ToString("F2");
                    valoresAntigos["ImagemUrl"] = produtoAntigo.ImagemUrl ?? "null";
                    valoresAntigos["Ativo"] = produtoAntigo.Ativo.ToString();
                    valoresAntigos["Ordem"] = produtoAntigo.Ordem.ToString();
                    valoresAntigos["EhSacoPromocional"] = produtoAntigo.EhSacoPromocional.ToString();
                    valoresAntigos["QuantidadeSaco"] = produtoAntigo.QuantidadeSaco.ToString();
                    valoresAntigos["ReservavelAte"] = produtoAntigo.ReservavelAte?.ToString("dd/MM/yyyy") ?? "null";
                    valoresAntigos["VendivelAte"] = produtoAntigo.VendivelAte?.ToString("dd/MM/yyyy") ?? "null";
                }
                
                valoresNovos["Nome"] = produto.Nome ?? "null";
                valoresNovos["Descricao"] = produto.Descricao ?? "null";
                valoresNovos["Preco"] = produto.Preco.ToString("F2");
                valoresNovos["ImagemUrl"] = produto.ImagemUrl ?? "null";
                valoresNovos["Ativo"] = produto.Ativo.ToString();
                valoresNovos["Ordem"] = produto.Ordem.ToString();
                valoresNovos["EhSacoPromocional"] = produto.EhSacoPromocional.ToString();
                valoresNovos["QuantidadeSaco"] = produto.QuantidadeSaco.ToString();
                valoresNovos["ReservavelAte"] = produto.ReservavelAte?.ToString("dd/MM/yyyy") ?? "null";
                valoresNovos["VendivelAte"] = produto.VendivelAte?.ToString("dd/MM/yyyy") ?? "null";
                
                var command = new SqlCommand(@"
                    UPDATE Produtos 
                    SET Nome = @Nome, Descricao = @Descricao, Preco = @Preco, 
                        ImagemUrl = @ImagemUrl, Ativo = @Ativo, Ordem = @Ordem,
                        EhSacoPromocional = @EhSacoPromocional, QuantidadeSaco = @QuantidadeSaco, Produtos = @Produtos,
                        ReservavelAte = @ReservavelAte, VendivelAte = @VendivelAte
                    WHERE Id = @Id", connection);
                
                command.Parameters.AddWithValue("@Id", produto.Id);
                command.Parameters.AddWithValue("@Nome", produto.Nome);
                command.Parameters.AddWithValue("@Descricao", produto.Descricao ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Preco", produto.Preco);
                command.Parameters.AddWithValue("@ImagemUrl", produto.ImagemUrl ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Ativo", produto.Ativo);
                command.Parameters.AddWithValue("@Ordem", produto.Ordem);
                command.Parameters.AddWithValue("@EhSacoPromocional", produto.EhSacoPromocional);
                command.Parameters.AddWithValue("@QuantidadeSaco", produto.QuantidadeSaco);
                command.Parameters.AddWithValue("@Produtos", produto.Produtos ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ReservavelAte", produto.ReservavelAte.HasValue ? (object)produto.ReservavelAte.Value : DBNull.Value);
                command.Parameters.AddWithValue("@VendivelAte", produto.VendivelAte.HasValue ? (object)produto.VendivelAte.Value : DBNull.Value);
                
                command.ExecuteNonQuery();
                
                // Limpar cache de produtos após atualização
                LimparCacheProdutos();
                
                // Registrar log com comparação
                string usuarioLog = LogService.ObterUsuarioAtual(HttpContext.Current?.Session);
                LogService.RegistrarAtualizacaoComComparacao(usuarioLog, "Produto", "DatabaseService.AtualizarProduto", produto.Id.ToString(), valoresAntigos, valoresNovos);
            }
        }

        public int AdicionarProduto(Produto produto)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(@"
                    INSERT INTO Produtos (Nome, Descricao, Preco, ImagemUrl, Ativo, Ordem, EhSacoPromocional, QuantidadeSaco, Produtos, ReservavelAte, VendivelAte)
                    OUTPUT INSERTED.Id
                    VALUES (@Nome, @Descricao, @Preco, @ImagemUrl, @Ativo, @Ordem, @EhSacoPromocional, @QuantidadeSaco, @Produtos, @ReservavelAte, @VendivelAte)", connection);
                
                command.Parameters.AddWithValue("@Nome", produto.Nome);
                command.Parameters.AddWithValue("@Descricao", produto.Descricao ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Preco", produto.Preco);
                command.Parameters.AddWithValue("@ImagemUrl", produto.ImagemUrl ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Ativo", produto.Ativo);
                command.Parameters.AddWithValue("@Ordem", produto.Ordem);
                command.Parameters.AddWithValue("@EhSacoPromocional", produto.EhSacoPromocional);
                command.Parameters.AddWithValue("@QuantidadeSaco", produto.QuantidadeSaco);
                command.Parameters.AddWithValue("@Produtos", produto.Produtos ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ReservavelAte", produto.ReservavelAte.HasValue ? (object)produto.ReservavelAte.Value : DBNull.Value);
                command.Parameters.AddWithValue("@VendivelAte", produto.VendivelAte.HasValue ? (object)produto.VendivelAte.Value : DBNull.Value);
                
                int produtoId = (int)command.ExecuteScalar();
                
                // Limpar cache de produtos após adicionar
                LimparCacheProdutos();
                
                // Registrar log
                string usuarioLog = LogService.ObterUsuarioAtual(HttpContext.Current?.Session);
                string detalhes = $"Nome: {produto.Nome}, Preco: R$ {produto.Preco:F2}, Ativo: {produto.Ativo}";
                LogService.RegistrarInsercao(usuarioLog, "Produto", "DatabaseService.AdicionarProduto", detalhes);
                
                return produtoId;
            }
        }

        public List<Reserva> ObterTodasReservas()
        {
            var reservas = new List<Reserva>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(@"
                    SELECT r.Id, r.DataRetirada, r.DataReserva, r.StatusId, r.ValorTotal, r.Observacoes, 
                           r.ConvertidoEmPedido, r.PrevisaoEntrega, r.Cancelado, r.ClienteId, r.TokenAcesso,
                           c.Nome, c.Email, c.Telefone, sr.Nome as StatusNome
                    FROM Reservas r
                    LEFT JOIN Clientes c ON r.ClienteId = c.Id
                    LEFT JOIN StatusReserva sr ON r.StatusId = sr.Id
                    ORDER BY r.DataReserva DESC", connection);
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var reserva = new Reserva
                        {
                            Id = reader.GetInt32(0),
                            DataRetirada = reader.GetDateTime(1),
                            DataReserva = reader.GetDateTime(2),
                            StatusId = reader.GetInt32(3),
                            ValorTotal = reader.GetDecimal(4),
                            Observacoes = reader.IsDBNull(5) ? "" : reader.GetString(5),
                            ConvertidoEmPedido = reader.GetBoolean(6),
                            PrevisaoEntrega = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                            Cancelado = reader.GetBoolean(8),
                            ClienteId = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9),
                            TokenAcesso = reader.IsDBNull(10) ? "" : reader.GetString(10),
                            Nome = reader.IsDBNull(11) ? "" : reader.GetString(11),
                            Email = reader.IsDBNull(12) ? "" : reader.GetString(12),
                            Telefone = reader.IsDBNull(13) ? "" : reader.GetString(13),
                            Status = reader.IsDBNull(14) ? "" : reader.GetString(14),
                            Itens = new List<ItemPedido>()
                        };
                        reservas.Add(reserva);
                    }
                }

                // Carregar itens de cada reserva
                foreach (var reserva in reservas)
                {
                    var itensCommand = new SqlCommand(@"
                        SELECT Id, ReservaId, ProdutoId, NomeProduto, Tamanho, Quantidade, PrecoUnitario, Subtotal, ISNULL(Produtos, '')
                        FROM ReservaItens
                        WHERE ReservaId = @ReservaId", connection);
                    itensCommand.Parameters.AddWithValue("@ReservaId", reserva.Id);
                    
                    using (var itensReader = itensCommand.ExecuteReader())
                    {
                        while (itensReader.Read())
                        {
                            reserva.Itens.Add(new ItemPedido
                            {
                                ProdutoId = itensReader.GetInt32(2),
                                NomeProduto = itensReader.GetString(3),
                                Tamanho = itensReader.GetString(4),
                                Quantidade = itensReader.GetInt32(5),
                                PrecoUnitario = itensReader.GetDecimal(6),
                                Subtotal = itensReader.GetDecimal(7),
                                Produtos = itensReader.IsDBNull(8) ? "" : itensReader.GetString(8)
                            });
                        }
                    }
                }
            }
            
            return reservas;
        }

        public Reserva ObterReservaPorId(int id)
        {
            Reserva reserva = null;
            
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(@"
                    SELECT r.Id, r.DataRetirada, r.DataReserva, r.StatusId, r.ValorTotal, r.Observacoes, 
                           r.ConvertidoEmPedido, r.PrevisaoEntrega, r.Cancelado, r.ClienteId, r.TokenAcesso,
                           c.Nome, c.Email, c.Telefone, sr.Nome as StatusNome
                    FROM Reservas r
                    LEFT JOIN Clientes c ON r.ClienteId = c.Id
                    LEFT JOIN StatusReserva sr ON r.StatusId = sr.Id
                    WHERE r.Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        reserva = new Reserva
                        {
                            Id = reader.GetInt32(0),
                            DataRetirada = reader.GetDateTime(1),
                            DataReserva = reader.GetDateTime(2),
                            StatusId = reader.GetInt32(3),
                            ValorTotal = reader.GetDecimal(4),
                            Observacoes = reader.IsDBNull(5) ? "" : reader.GetString(5),
                            ConvertidoEmPedido = reader.GetBoolean(6),
                            PrevisaoEntrega = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                            Cancelado = reader.GetBoolean(8),
                            ClienteId = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9),
                            TokenAcesso = reader.IsDBNull(10) ? "" : reader.GetString(10),
                            Nome = reader.IsDBNull(11) ? "" : reader.GetString(11),
                            Email = reader.IsDBNull(12) ? "" : reader.GetString(12),
                            Telefone = reader.IsDBNull(13) ? "" : reader.GetString(13),
                            Status = reader.IsDBNull(14) ? "" : reader.GetString(14),
                            Itens = new List<ItemPedido>()
                        };
                    }
                }

                if (reserva != null)
                {
                    // Carregar itens da reserva
                    var itensCommand = new SqlCommand(@"
                        SELECT Id, ReservaId, ProdutoId, NomeProduto, Tamanho, Quantidade, PrecoUnitario, Subtotal, ISNULL(Produtos, '')
                        FROM ReservaItens
                        WHERE ReservaId = @ReservaId", connection);
                    itensCommand.Parameters.AddWithValue("@ReservaId", reserva.Id);
                    
                    using (var itensReader = itensCommand.ExecuteReader())
                    {
                        while (itensReader.Read())
                        {
                            reserva.Itens.Add(new ItemPedido
                            {
                                ProdutoId = itensReader.GetInt32(2),
                                NomeProduto = itensReader.GetString(3),
                                Tamanho = itensReader.GetString(4),
                                Quantidade = itensReader.GetInt32(5),
                                PrecoUnitario = itensReader.GetDecimal(6),
                                Subtotal = itensReader.GetDecimal(7),
                                Produtos = itensReader.IsDBNull(8) ? "" : itensReader.GetString(8)
                            });
                        }
                    }
                }
            }
            
            return reserva;
        }

        public void AtualizarReserva(Reserva reserva)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                // Buscar dados antigos para comparação
                Reserva reservaAntiga = ObterReservaPorId(reserva.Id);
                var valoresAntigos = new Dictionary<string, string>();
                var valoresNovos = new Dictionary<string, string>();
                
                if (reservaAntiga != null)
                {
                    valoresAntigos["StatusId"] = reservaAntiga.StatusId?.ToString() ?? "null";
                    valoresAntigos["Status"] = reservaAntiga.Status ?? "null";
                    valoresAntigos["ValorTotal"] = reservaAntiga.ValorTotal.ToString("F2");
                    valoresAntigos["DataRetirada"] = reservaAntiga.DataRetirada.ToString("dd/MM/yyyy");
                    valoresAntigos["ConvertidoEmPedido"] = reservaAntiga.ConvertidoEmPedido.ToString();
                    valoresAntigos["Cancelado"] = reservaAntiga.Cancelado.ToString();
                    valoresAntigos["Observacoes"] = reservaAntiga.Observacoes ?? "null";
                    valoresAntigos["PrevisaoEntrega"] = reservaAntiga.PrevisaoEntrega?.ToString("dd/MM/yyyy") ?? "null";
                }
                
                // Se StatusId não estiver definido, obter pelo nome do Status
                if (!reserva.StatusId.HasValue && !string.IsNullOrEmpty(reserva.Status))
                {
                    var statusReserva = ObterStatusReservaPorNome(reserva.Status);
                    if (statusReserva != null)
                    {
                        reserva.StatusId = statusReserva.Id;
                    }
                }
                
                // Se ainda não tiver StatusId, manter o atual
                if (!reserva.StatusId.HasValue)
                {
                    if (reservaAntiga != null)
                    {
                        reserva.StatusId = reservaAntiga.StatusId;
                    }
                }
                
                // Preparar valores novos
                valoresNovos["StatusId"] = reserva.StatusId?.ToString() ?? "null";
                valoresNovos["Status"] = reserva.Status ?? "null";
                valoresNovos["ValorTotal"] = reserva.ValorTotal.ToString("F2");
                valoresNovos["DataRetirada"] = reserva.DataRetirada.ToString("dd/MM/yyyy");
                valoresNovos["ConvertidoEmPedido"] = reserva.ConvertidoEmPedido.ToString();
                valoresNovos["Cancelado"] = reserva.Cancelado.ToString();
                valoresNovos["Observacoes"] = reserva.Observacoes ?? "null";
                valoresNovos["PrevisaoEntrega"] = reserva.PrevisaoEntrega?.ToString("dd/MM/yyyy") ?? "null";
                
                var command = new SqlCommand(@"
                    UPDATE Reservas 
                    SET StatusId = @StatusId, ConvertidoEmPedido = @ConvertidoEmPedido, 
                        PrevisaoEntrega = @PrevisaoEntrega, Cancelado = @Cancelado,
                        Observacoes = @Observacoes, ValorTotal = @ValorTotal,
                        DataRetirada = @DataRetirada
                    WHERE Id = @Id", connection);
                
                command.Parameters.AddWithValue("@Id", reserva.Id);
                command.Parameters.AddWithValue("@StatusId", reserva.StatusId.Value);
                command.Parameters.AddWithValue("@ConvertidoEmPedido", reserva.ConvertidoEmPedido);
                command.Parameters.AddWithValue("@PrevisaoEntrega", reserva.PrevisaoEntrega.HasValue ? (object)reserva.PrevisaoEntrega.Value : DBNull.Value);
                command.Parameters.AddWithValue("@Cancelado", reserva.Cancelado);
                command.Parameters.AddWithValue("@Observacoes", reserva.Observacoes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ValorTotal", reserva.ValorTotal);
                command.Parameters.AddWithValue("@DataRetirada", reserva.DataRetirada);
                
                command.ExecuteNonQuery();
                
                // Registrar log com comparação
                string usuarioLog = LogService.ObterUsuarioAtual(HttpContext.Current?.Session);
                LogService.RegistrarAtualizacaoComComparacao(usuarioLog, "Reserva", "DatabaseService.AtualizarReserva", reserva.Id.ToString(), valoresAntigos, valoresNovos);
            }
        }

        // Classe auxiliar para armazenar item com ID do banco
        private class ItemComId
        {
            public int Id { get; set; }
            public ItemPedido Item { get; set; }
            public string Chave { get; set; } // Chave para comparação: ProdutoId_Tamanho_Produtos
        }

        public void AtualizarItensReserva(int reservaId, List<ItemPedido> novosItens)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                // Buscar itens antigos do banco com seus IDs
                var itensAntigos = new List<ItemComId>();
                var itensAntigosCommand = new SqlCommand(@"
                    SELECT Id, ProdutoId, NomeProduto, Tamanho, Quantidade, PrecoUnitario, Subtotal, ISNULL(Produtos, '')
                    FROM ReservaItens
                    WHERE ReservaId = @ReservaId", connection);
                itensAntigosCommand.Parameters.AddWithValue("@ReservaId", reservaId);
                
                using (var reader = itensAntigosCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new ItemPedido
                        {
                            ProdutoId = reader.GetInt32(1),
                            NomeProduto = reader.GetString(2),
                            Tamanho = reader.GetString(3),
                            Quantidade = reader.GetInt32(4),
                            PrecoUnitario = reader.GetDecimal(5),
                            Subtotal = reader.GetDecimal(6),
                            Produtos = reader.IsDBNull(7) ? "" : reader.GetString(7)
                        };
                        
                        string chave = $"{item.ProdutoId}_{item.Tamanho}_{(item.Produtos ?? "")}";
                        itensAntigos.Add(new ItemComId
                        {
                            Id = reader.GetInt32(0),
                            Item = item,
                            Chave = chave
                        });
                    }
                }
                
                // Otimização: Obter apenas IDs dos produtos que estão nos novos itens (em vez de todos)
                var produtosIdsNovos = novosItens.Select(i => i.ProdutoId).Distinct().ToList();
                var produtosExistentes = new HashSet<int>();
                
                if (produtosIdsNovos.Count > 0)
                {
                    // Query única para verificar apenas os produtos necessários
                    var idsParametros = string.Join(",", produtosIdsNovos.Select((id, idx) => $"@Id{idx}"));
                    var produtosCommand = new SqlCommand($"SELECT Id FROM Produtos WHERE Id IN ({idsParametros})", connection);
                    for (int i = 0; i < produtosIdsNovos.Count; i++)
                    {
                        produtosCommand.Parameters.AddWithValue($"@Id{i}", produtosIdsNovos[i]);
                    }
                    
                    using (var produtosReader = produtosCommand.ExecuteReader())
                    {
                        while (produtosReader.Read())
                        {
                            produtosExistentes.Add(produtosReader.GetInt32(0));
                        }
                    }
                }
                
                // Criar dicionário de itens novos por chave
                var novosItensPorChave = new Dictionary<string, ItemPedido>();
                foreach (var item in novosItens)
                {
                    // Validar se o produto existe
                    if (!produtosExistentes.Contains(item.ProdutoId))
                    {
                        continue; // Pular produtos que não existem mais
                    }
                    
                    string chave = $"{item.ProdutoId}_{item.Tamanho}_{(item.Produtos ?? "")}";
                    novosItensPorChave[chave] = item;
                }
                
                // Processar itens: atualizar, inserir ou deletar
                int itensAtualizados = 0;
                int itensInseridos = 0;
                int itensDeletados = 0;
                string usuarioLog = LogService.ObterUsuarioAtual(HttpContext.Current?.Session);
                
                // Processar itens antigos: atualizar ou deletar
                foreach (var itemAntigo in itensAntigos)
                {
                    if (novosItensPorChave.TryGetValue(itemAntigo.Chave, out var itemNovo))
                    {
                        // Item existe nos dois - verificar se precisa atualizar
                        bool precisaAtualizar = 
                            itemAntigo.Item.Quantidade != itemNovo.Quantidade ||
                            itemAntigo.Item.PrecoUnitario != itemNovo.PrecoUnitario ||
                            itemAntigo.Item.Subtotal != itemNovo.Subtotal ||
                            itemAntigo.Item.NomeProduto != itemNovo.NomeProduto ||
                            (itemAntigo.Item.Produtos ?? "") != (itemNovo.Produtos ?? "");
                        
                        if (precisaAtualizar)
                        {
                            var updateCommand = new SqlCommand(@"
                                UPDATE ReservaItens 
                                SET NomeProduto = @NomeProduto, Quantidade = @Quantidade, 
                                    PrecoUnitario = @PrecoUnitario, Subtotal = @Subtotal, Produtos = @Produtos
                                WHERE Id = @Id", connection);
                            
                            updateCommand.Parameters.AddWithValue("@Id", itemAntigo.Id);
                            updateCommand.Parameters.AddWithValue("@NomeProduto", itemNovo.NomeProduto ?? "");
                            updateCommand.Parameters.AddWithValue("@Quantidade", itemNovo.Quantidade);
                            updateCommand.Parameters.AddWithValue("@PrecoUnitario", itemNovo.PrecoUnitario);
                            updateCommand.Parameters.AddWithValue("@Subtotal", itemNovo.Subtotal);
                            updateCommand.Parameters.AddWithValue("@Produtos", itemNovo.Produtos ?? (object)DBNull.Value);
                            
                            updateCommand.ExecuteNonQuery();
                            itensAtualizados++;
                            
                            // Registrar log de atualização
                            var valoresAntigos = new Dictionary<string, string>
                            {
                                ["Quantidade"] = itemAntigo.Item.Quantidade.ToString(),
                                ["PrecoUnitario"] = itemAntigo.Item.PrecoUnitario.ToString("F2"),
                                ["Subtotal"] = itemAntigo.Item.Subtotal.ToString("F2"),
                                ["NomeProduto"] = itemAntigo.Item.NomeProduto ?? "",
                                ["Produtos"] = itemAntigo.Item.Produtos ?? ""
                            };
                            var valoresNovos = new Dictionary<string, string>
                            {
                                ["Quantidade"] = itemNovo.Quantidade.ToString(),
                                ["PrecoUnitario"] = itemNovo.PrecoUnitario.ToString("F2"),
                                ["Subtotal"] = itemNovo.Subtotal.ToString("F2"),
                                ["NomeProduto"] = itemNovo.NomeProduto ?? "",
                                ["Produtos"] = itemNovo.Produtos ?? ""
                            };
                            LogService.RegistrarAtualizacaoComComparacao(usuarioLog, "ReservaItens", 
                                "DatabaseService.AtualizarItensReserva", itemAntigo.Id.ToString(), valoresAntigos, valoresNovos);
                        }
                        
                        // Remover da lista de novos para não inserir novamente
                        novosItensPorChave.Remove(itemAntigo.Chave);
                    }
                    else
                    {
                        // Item não existe mais nos novos - deletar
                        var deleteCommand = new SqlCommand("DELETE FROM ReservaItens WHERE Id = @Id", connection);
                        deleteCommand.Parameters.AddWithValue("@Id", itemAntigo.Id);
                        deleteCommand.ExecuteNonQuery();
                        itensDeletados++;
                        
                        // Registrar log de exclusão
                        LogService.RegistrarExclusao(usuarioLog, "ReservaItens", "DatabaseService.AtualizarItensReserva", 
                            $"Id: {itemAntigo.Id}, ReservaId: {reservaId}, ProdutoId: {itemAntigo.Item.ProdutoId}, Tamanho: {itemAntigo.Item.Tamanho}");
                    }
                }
                
                // Inserir itens novos que não existiam antes
                foreach (var itemNovo in novosItensPorChave.Values)
                {
                    var insertCommand = new SqlCommand(@"
                        INSERT INTO ReservaItens (ReservaId, ProdutoId, NomeProduto, Tamanho, Quantidade, PrecoUnitario, Subtotal, Produtos)
                        VALUES (@ReservaId, @ProdutoId, @NomeProduto, @Tamanho, @Quantidade, @PrecoUnitario, @Subtotal, @Produtos)", connection);
                    
                    insertCommand.Parameters.AddWithValue("@ReservaId", reservaId);
                    insertCommand.Parameters.AddWithValue("@ProdutoId", itemNovo.ProdutoId);
                    insertCommand.Parameters.AddWithValue("@NomeProduto", itemNovo.NomeProduto ?? "");
                    insertCommand.Parameters.AddWithValue("@Tamanho", itemNovo.Tamanho ?? "");
                    insertCommand.Parameters.AddWithValue("@Quantidade", itemNovo.Quantidade);
                    insertCommand.Parameters.AddWithValue("@PrecoUnitario", itemNovo.PrecoUnitario);
                    insertCommand.Parameters.AddWithValue("@Subtotal", itemNovo.Subtotal);
                    insertCommand.Parameters.AddWithValue("@Produtos", itemNovo.Produtos ?? (object)DBNull.Value);
                    
                    insertCommand.ExecuteNonQuery();
                    itensInseridos++;
                    
                    // Registrar log de inserção
                    LogService.RegistrarInsercao(usuarioLog, "ReservaItens", "DatabaseService.AtualizarItensReserva", 
                        $"ReservaId: {reservaId}, ProdutoId: {itemNovo.ProdutoId}, Tamanho: {itemNovo.Tamanho}, Quantidade: {itemNovo.Quantidade}");
                }
                
                // Registrar log resumido se houver alterações
                if (itensAtualizados > 0 || itensInseridos > 0 || itensDeletados > 0)
                {
                    string resumo = $"ReservaId: {reservaId} - Atualizados: {itensAtualizados}, Inseridos: {itensInseridos}, Deletados: {itensDeletados}";
                    // Log já foi registrado individualmente acima, então não precisa registrar novamente
                }
            }
        }

        public List<StatusReserva> ObterTodosStatusReserva()
        {
            var status = new List<StatusReserva>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, Nome, Descricao, PermiteAlteracao, PermiteExclusao, Ordem FROM StatusReserva ORDER BY Ordem", connection);
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        status.Add(new StatusReserva
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Descricao = reader.GetString(2),
                            PermiteAlteracao = reader.GetBoolean(3),
                            PermiteExclusao = reader.GetBoolean(4),
                            Ordem = reader.GetInt32(5)
                        });
                    }
                }
            }
            return status;
        }

        public StatusReserva ObterStatusReservaPorNome(string nome)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, Nome, Descricao, PermiteAlteracao, PermiteExclusao, Ordem FROM StatusReserva WHERE Nome = @Nome", connection);
                command.Parameters.AddWithValue("@Nome", nome);
                
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new StatusReserva
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Descricao = reader.GetString(2),
                            PermiteAlteracao = reader.GetBoolean(3),
                            PermiteExclusao = reader.GetBoolean(4),
                            Ordem = reader.GetInt32(5)
                        };
                    }
                }
            }
            return null;
        }

        public StatusReserva ObterStatusReservaPorId(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, Nome, Descricao, PermiteAlteracao, PermiteExclusao, Ordem FROM StatusReserva WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new StatusReserva
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Descricao = reader.GetString(2),
                            PermiteAlteracao = reader.GetBoolean(3),
                            PermiteExclusao = reader.GetBoolean(4),
                            Ordem = reader.GetInt32(5)
                        };
                    }
                }
            }
            return null;
        }

        public bool StatusPermiteAlteracao(int statusId)
        {
            var status = ObterStatusReservaPorId(statusId);
            return status != null && status.PermiteAlteracao;
        }

        public bool StatusPermiteExclusao(int statusId)
        {
            var status = ObterStatusReservaPorId(statusId);
            return status != null && status.PermiteExclusao;
        }

        public List<Cliente> ObterTodosClientes()
        {
            var clientes = new List<Cliente>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, Nome, Email, Senha, Telefone, TemWhatsApp, Provider, ProviderId, TokenConfirmacao, TokenRecuperacaoSenha, DataExpiracaoRecuperacaoSenha, EmailConfirmado, WhatsAppConfirmado, IsAdmin, DataCadastro, UltimoAcesso FROM Clientes", connection);
                // Nota: Provider e ProviderId mantidos no SELECT para compatibilidade com banco existente, mas não são mais usados no modelo
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clientes.Add(new Cliente
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Email = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Senha = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Telefone = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            TemWhatsApp = reader.GetBoolean(5),
                            // Provider e ProviderId não são mais usados (linhas 6 e 7 ignoradas)
                            TokenConfirmacao = reader.IsDBNull(8) ? null : reader.GetString(8),
                            TokenRecuperacaoSenha = reader.IsDBNull(9) ? null : reader.GetString(9),
                            DataExpiracaoRecuperacaoSenha = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                            EmailConfirmado = reader.GetBoolean(11),
                            WhatsAppConfirmado = reader.GetBoolean(12),
                            IsAdmin = reader.GetBoolean(13),
                            DataCadastro = reader.GetDateTime(14),
                            UltimoAcesso = reader.IsDBNull(15) ? (DateTime?)null : reader.GetDateTime(15)
                        });
                    }
                }
            }
            return clientes;
        }

        // Método auxiliar para formatar email
        private string FormatarEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return "";
            
            return email.Trim().ToLowerInvariant();
        }

        // Método auxiliar para formatar telefone (apenas números)
        private string FormatarTelefone(string telefone)
        {
            if (string.IsNullOrEmpty(telefone))
                return "";
            
            // Remover todos os caracteres não numéricos
            return System.Text.RegularExpressions.Regex.Replace(telefone, @"[^\d]", "");
        }

        public Cliente ObterClientePorEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            string emailFormatado = FormatarEmail(email);
            
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, Nome, Email, Senha, Telefone, TemWhatsApp, Provider, ProviderId, TokenConfirmacao, TokenRecuperacaoSenha, DataExpiracaoRecuperacaoSenha, EmailConfirmado, WhatsAppConfirmado, IsAdmin, DataCadastro, UltimoAcesso FROM Clientes WHERE LOWER(LTRIM(RTRIM(Email))) = @Email", connection);
                // Nota: Provider e ProviderId mantidos no SELECT para compatibilidade com banco existente, mas não são mais usados no modelo
                command.Parameters.AddWithValue("@Email", emailFormatado);
                
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Cliente
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Email = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Senha = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Telefone = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            TemWhatsApp = reader.GetBoolean(5),
                            // Provider e ProviderId não são mais usados (linhas 6 e 7 ignoradas)
                            TokenConfirmacao = reader.IsDBNull(8) ? null : reader.GetString(8),
                            TokenRecuperacaoSenha = reader.IsDBNull(9) ? null : reader.GetString(9),
                            DataExpiracaoRecuperacaoSenha = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                            EmailConfirmado = reader.GetBoolean(11),
                            WhatsAppConfirmado = reader.GetBoolean(12),
                            IsAdmin = reader.GetBoolean(13),
                            DataCadastro = reader.GetDateTime(14),
                            UltimoAcesso = reader.IsDBNull(15) ? (DateTime?)null : reader.GetDateTime(15)
                        };
                    }
                }
            }
            return null;
        }

        public Cliente ObterClientePorId(int clienteId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, Nome, Email, Senha, Telefone, TemWhatsApp, Provider, ProviderId, TokenConfirmacao, TokenRecuperacaoSenha, DataExpiracaoRecuperacaoSenha, EmailConfirmado, WhatsAppConfirmado, IsAdmin, DataCadastro, UltimoAcesso FROM Clientes WHERE Id = @Id", connection);
                // Nota: Provider e ProviderId mantidos no SELECT para compatibilidade com banco existente, mas não são mais usados no modelo
                command.Parameters.AddWithValue("@Id", clienteId);
                
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Cliente
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Email = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Senha = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Telefone = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            TemWhatsApp = reader.GetBoolean(5),
                            // Provider e ProviderId não são mais usados (linhas 6 e 7 ignoradas)
                            TokenConfirmacao = reader.IsDBNull(8) ? null : reader.GetString(8),
                            TokenRecuperacaoSenha = reader.IsDBNull(9) ? null : reader.GetString(9),
                            DataExpiracaoRecuperacaoSenha = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                            EmailConfirmado = reader.GetBoolean(11),
                            WhatsAppConfirmado = reader.GetBoolean(12),
                            IsAdmin = reader.GetBoolean(13),
                            DataCadastro = reader.GetDateTime(14),
                            UltimoAcesso = reader.IsDBNull(15) ? (DateTime?)null : reader.GetDateTime(15)
                        };
                    }
                }
            }
            return null;
        }

        public Cliente ObterClientePorTelefone(string telefone)
        {
            if (string.IsNullOrEmpty(telefone))
                return null;

            string telefoneFormatado = FormatarTelefone(telefone);
            
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                // Otimização: Primeiro tentar busca exata (mais rápida e usa índice)
                var command = new SqlCommand(@"
                    SELECT Id, Nome, Email, Senha, Telefone, TemWhatsApp, Provider, ProviderId, TokenConfirmacao, TokenRecuperacaoSenha, DataExpiracaoRecuperacaoSenha, EmailConfirmado, WhatsAppConfirmado, IsAdmin, DataCadastro, UltimoAcesso 
                    FROM Clientes 
                    WHERE LTRIM(RTRIM(Telefone)) = @Telefone", connection);
                command.Parameters.AddWithValue("@Telefone", telefoneFormatado);
                
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Cliente
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Email = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Senha = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Telefone = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            TemWhatsApp = reader.GetBoolean(5),
                            // Provider e ProviderId não são mais usados (linhas 6 e 7 ignoradas)
                            TokenConfirmacao = reader.IsDBNull(8) ? null : reader.GetString(8),
                            TokenRecuperacaoSenha = reader.IsDBNull(9) ? null : reader.GetString(9),
                            DataExpiracaoRecuperacaoSenha = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                            EmailConfirmado = reader.GetBoolean(11),
                            WhatsAppConfirmado = reader.GetBoolean(12),
                            IsAdmin = reader.GetBoolean(13),
                            DataCadastro = reader.GetDateTime(14),
                            UltimoAcesso = reader.IsDBNull(15) ? (DateTime?)null : reader.GetDateTime(15)
                        };
                    }
                }
                
                // Se não encontrou com busca exata, tentar busca com formatação (fallback)
                // Isso é mais lento, mas necessário para telefones com formatação diferente
                command.CommandText = @"
                    SELECT Id, Nome, Email, Senha, Telefone, TemWhatsApp, Provider, ProviderId, TokenConfirmacao, TokenRecuperacaoSenha, DataExpiracaoRecuperacaoSenha, EmailConfirmado, WhatsAppConfirmado, IsAdmin, DataCadastro, UltimoAcesso 
                    FROM Clientes 
                    WHERE LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ISNULL(Telefone, ''), '(', ''), ')', ''), '-', ''), ' ', ''), '.', ''), '/', ''))) = @Telefone";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@Telefone", telefoneFormatado);
                
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Cliente
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Email = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Senha = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Telefone = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            TemWhatsApp = reader.GetBoolean(5),
                            // Provider e ProviderId não são mais usados (linhas 6 e 7 ignoradas)
                            TokenConfirmacao = reader.IsDBNull(8) ? null : reader.GetString(8),
                            TokenRecuperacaoSenha = reader.IsDBNull(9) ? null : reader.GetString(9),
                            DataExpiracaoRecuperacaoSenha = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                            EmailConfirmado = reader.GetBoolean(11),
                            WhatsAppConfirmado = reader.GetBoolean(12),
                            IsAdmin = reader.GetBoolean(13),
                            DataCadastro = reader.GetDateTime(14),
                            UltimoAcesso = reader.IsDBNull(15) ? (DateTime?)null : reader.GetDateTime(15)
                        };
                    }
                }
            }
            return null;
        }

        public Cliente ObterClientePorEmailOuTelefone(string email, string telefone)
        {
            // Tentar por email primeiro
            if (!string.IsNullOrEmpty(email))
            {
                var cliente = ObterClientePorEmail(email);
                if (cliente != null)
                    return cliente;
            }

            // Tentar por telefone
            if (!string.IsNullOrEmpty(telefone))
            {
                var cliente = ObterClientePorTelefone(telefone);
                if (cliente != null)
                    return cliente;
            }

            return null;
        }

        // Método removido - OAuth não está implementado
        // public Cliente ObterClientePorProvider(string provider, string providerId)

        public Cliente ObterClientePorToken(string token)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, Nome, Email, Senha, Telefone, TemWhatsApp, Provider, ProviderId, TokenConfirmacao, TokenRecuperacaoSenha, DataExpiracaoRecuperacaoSenha, EmailConfirmado, WhatsAppConfirmado, IsAdmin, DataCadastro, UltimoAcesso FROM Clientes WHERE TokenConfirmacao = @Token", connection);
                command.Parameters.AddWithValue("@Token", token);
                
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Cliente
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Email = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Senha = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Telefone = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            TemWhatsApp = reader.GetBoolean(5),
                            // Provider e ProviderId não são mais usados (linhas 6 e 7 ignoradas)
                            TokenConfirmacao = reader.IsDBNull(8) ? null : reader.GetString(8),
                            TokenRecuperacaoSenha = reader.IsDBNull(9) ? null : reader.GetString(9),
                            DataExpiracaoRecuperacaoSenha = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                            EmailConfirmado = reader.GetBoolean(11),
                            WhatsAppConfirmado = reader.GetBoolean(12),
                            IsAdmin = reader.GetBoolean(13),
                            DataCadastro = reader.GetDateTime(14),
                            UltimoAcesso = reader.IsDBNull(15) ? (DateTime?)null : reader.GetDateTime(15)
                        };
                    }
                }
            }
            return null;
        }

        // Lista de emails de administradores
        private readonly string[] _emailsAdministradores = new string[]
        {
            "wilson2071@gmail.com",
            "isanfm@gmail.com",
            "camilafermagalhaes@gmail.com"
        };

        private bool VerificarSeEhAdministrador(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;
            
            string emailFormatado = FormatarEmail(email);
            return _emailsAdministradores.Any(e => FormatarEmail(e) == emailFormatado);
        }

        public int CriarOuAtualizarCliente(Cliente cliente)
        {
            // Formatar email e telefone antes de processar
            if (!string.IsNullOrEmpty(cliente.Email))
            {
                cliente.Email = FormatarEmail(cliente.Email);
            }
            if (!string.IsNullOrEmpty(cliente.Telefone))
            {
                cliente.Telefone = FormatarTelefone(cliente.Telefone);
            }

            // Verificar se o email é de administrador e definir IsAdmin automaticamente
            bool ehAdministrador = VerificarSeEhAdministrador(cliente.Email);
            if (ehAdministrador)
            {
                cliente.IsAdmin = true;
            }
            else
            {
                // Garantir que apenas os emails específicos sejam administradores
                cliente.IsAdmin = false;
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                // Verificar se cliente já existe (por Email OU Telefone)
                Cliente clienteExistente = ObterClientePorEmailOuTelefone(cliente.Email, cliente.Telefone);
                
                if (clienteExistente != null)
                {
                    // Verificar se está tentando atualizar com email/telefone que já pertence a outro cliente
                    string emailFormatado = FormatarEmail(cliente.Email);
                    string telefoneFormatado = FormatarTelefone(cliente.Telefone);
                    
                    // Verificar se o email está sendo usado por outro cliente
                    if (!string.IsNullOrEmpty(emailFormatado))
                    {
                        var clienteComEmail = ObterClientePorEmail(emailFormatado);
                        if (clienteComEmail != null && clienteComEmail.Id != clienteExistente.Id)
                        {
                            throw new Exception($"O email {emailFormatado} já está cadastrado para outro usuário.");
                        }
                    }
                    
                    // Verificar se o telefone está sendo usado por outro cliente
                    if (!string.IsNullOrEmpty(telefoneFormatado))
                    {
                        var clienteComTelefone = ObterClientePorTelefone(telefoneFormatado);
                        if (clienteComTelefone != null && clienteComTelefone.Id != clienteExistente.Id)
                        {
                            throw new Exception($"O telefone {telefoneFormatado} já está cadastrado para outro usuário.");
                        }
                    }
                    
                    // Se o cliente existente tem um dos emails de administrador, garantir que IsAdmin seja true
                    bool clienteExistenteEhAdmin = VerificarSeEhAdministrador(clienteExistente.Email);
                    bool isAdminFinal = ehAdministrador || clienteExistenteEhAdmin;
                    
                    try
                    {
                        // Atualizar cliente existente
                        var command = new SqlCommand(@"
                            UPDATE Clientes 
                            SET Nome = @Nome, Email = @Email, Senha = @Senha, Telefone = @Telefone, TemWhatsApp = @TemWhatsApp, 
                                IsAdmin = @IsAdmin, UltimoAcesso = GETDATE()
                            WHERE Id = @Id", connection);
                        
                        command.Parameters.AddWithValue("@Id", clienteExistente.Id);
                        command.Parameters.AddWithValue("@Nome", cliente.Nome);
                        command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(emailFormatado) ? (object)DBNull.Value : emailFormatado);
                        command.Parameters.AddWithValue("@Senha", string.IsNullOrEmpty(cliente.Senha) ? (object)DBNull.Value : cliente.Senha);
                        command.Parameters.AddWithValue("@Telefone", string.IsNullOrEmpty(telefoneFormatado) ? (object)DBNull.Value : telefoneFormatado);
                        command.Parameters.AddWithValue("@TemWhatsApp", cliente.TemWhatsApp);
                        command.Parameters.AddWithValue("@IsAdmin", isAdminFinal);
                        
                        command.ExecuteNonQuery();
                        return clienteExistente.Id;
                    }
                    catch (System.Data.SqlClient.SqlException sqlEx)
                    {
                        // Capturar violação de constraint única
                        if (sqlEx.Number == 2601 || sqlEx.Number == 2627) // Violação de constraint única
                        {
                            string mensagemErro = "Erro ao atualizar: ";
                            if (sqlEx.Message.Contains("IX_Clientes_Email") || sqlEx.Message.Contains("Email"))
                            {
                                mensagemErro += $"O email {emailFormatado} já está cadastrado para outro usuário.";
                            }
                            else if (sqlEx.Message.Contains("IX_Clientes_Telefone") || sqlEx.Message.Contains("Telefone"))
                            {
                                mensagemErro += $"O telefone {telefoneFormatado} já está cadastrado para outro usuário.";
                            }
                            else
                            {
                                mensagemErro += "Dados duplicados. Verifique email e telefone.";
                            }
                            throw new Exception(mensagemErro, sqlEx);
                        }
                        throw; // Re-lançar outras exceções SQL
                    }
                }
                else
                {
                    // ANTES de criar, verificar novamente se não há duplicatas (race condition)
                    // Verificar email duplicado
                    if (!string.IsNullOrEmpty(cliente.Email))
                    {
                        var clienteComEmail = ObterClientePorEmail(cliente.Email);
                        if (clienteComEmail != null)
                        {
                            throw new Exception($"Já existe um usuário cadastrado com o email {cliente.Email}. Por favor, faça login.");
                        }
                    }
                    
                    // Verificar telefone duplicado
                    if (!string.IsNullOrEmpty(cliente.Telefone))
                    {
                        var clienteComTelefone = ObterClientePorTelefone(cliente.Telefone);
                        if (clienteComTelefone != null)
                        {
                            throw new Exception($"Já existe um usuário cadastrado com o telefone {cliente.Telefone}. Por favor, faça login.");
                        }
                    }
                    
                    // Criar novo cliente
                    if (string.IsNullOrEmpty(cliente.TokenConfirmacao))
                    {
                        cliente.TokenConfirmacao = Guid.NewGuid().ToString("N");
                    }
                    
                    // Garantir que email e telefone estejam formatados
                    string emailFormatado = FormatarEmail(cliente.Email);
                    string telefoneFormatado = FormatarTelefone(cliente.Telefone);
                    
                    try
                    {
                        var command = new SqlCommand(@"
                            INSERT INTO Clientes (Nome, Email, Senha, Telefone, TemWhatsApp, Provider, ProviderId, TokenConfirmacao, EmailConfirmado, WhatsAppConfirmado, IsAdmin, DataCadastro)
                            VALUES (@Nome, @Email, @Senha, @Telefone, @TemWhatsApp, NULL, NULL, @TokenConfirmacao, @EmailConfirmado, @WhatsAppConfirmado, @IsAdmin, GETDATE());
                            SELECT CAST(SCOPE_IDENTITY() as int);", connection);
                        
                        command.Parameters.AddWithValue("@Nome", cliente.Nome);
                        command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(emailFormatado) ? (object)DBNull.Value : emailFormatado);
                        command.Parameters.AddWithValue("@Senha", string.IsNullOrEmpty(cliente.Senha) ? (object)DBNull.Value : cliente.Senha);
                        command.Parameters.AddWithValue("@Telefone", string.IsNullOrEmpty(telefoneFormatado) ? (object)DBNull.Value : telefoneFormatado);
                        command.Parameters.AddWithValue("@TemWhatsApp", cliente.TemWhatsApp);
                        // Provider e ProviderId não são mais usados (definidos como NULL no INSERT)
                        command.Parameters.AddWithValue("@TokenConfirmacao", cliente.TokenConfirmacao);
                        command.Parameters.AddWithValue("@EmailConfirmado", cliente.EmailConfirmado);
                        command.Parameters.AddWithValue("@WhatsAppConfirmado", cliente.WhatsAppConfirmado);
                        command.Parameters.AddWithValue("@IsAdmin", cliente.IsAdmin);
                        
                        int novoClienteId = (int)command.ExecuteScalar();
                        
                        // Registrar log
                        string usuarioLog = LogService.ObterUsuarioAtual(HttpContext.Current?.Session);
                        string detalhes = $"ID: {novoClienteId}, Nome: {cliente.Nome}, Email: {cliente.Email}, Telefone: {cliente.Telefone ?? "N/A"}, IsAdmin: {cliente.IsAdmin}";
                        LogService.RegistrarInsercao(usuarioLog, "Cliente", "DatabaseService.CriarOuAtualizarCliente", detalhes);
                        
                        return novoClienteId;
                    }
                    catch (System.Data.SqlClient.SqlException sqlEx)
                    {
                        // Capturar violação de constraint única
                        if (sqlEx.Number == 2601 || sqlEx.Number == 2627) // Violação de constraint única
                        {
                            string mensagemErro = "Erro ao cadastrar: ";
                            if (sqlEx.Message.Contains("IX_Clientes_Email") || sqlEx.Message.Contains("Email"))
                            {
                                mensagemErro += $"Já existe um usuário cadastrado com o email {cliente.Email}. Por favor, faça login.";
                            }
                            else if (sqlEx.Message.Contains("IX_Clientes_Telefone") || sqlEx.Message.Contains("Telefone"))
                            {
                                mensagemErro += $"Já existe um usuário cadastrado com o telefone {cliente.Telefone}. Por favor, faça login.";
                            }
                            else
                            {
                                mensagemErro += "Dados duplicados. Verifique email e telefone.";
                            }
                            throw new Exception(mensagemErro, sqlEx);
                        }
                        throw; // Re-lançar outras exceções SQL
                    }
                }
            }
        }

        public void ConfirmarEmailCliente(string token)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("UPDATE Clientes SET EmailConfirmado = 1 WHERE TokenConfirmacao = @Token", connection);
                command.Parameters.AddWithValue("@Token", token);
                command.ExecuteNonQuery();
            }
        }

        public Cliente ObterClientePorTokenRecuperacaoSenha(string token)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, Nome, Email, Senha, Telefone, TemWhatsApp, Provider, ProviderId, TokenConfirmacao, TokenRecuperacaoSenha, DataExpiracaoRecuperacaoSenha, EmailConfirmado, WhatsAppConfirmado, IsAdmin, DataCadastro, UltimoAcesso FROM Clientes WHERE TokenRecuperacaoSenha = @Token AND DataExpiracaoRecuperacaoSenha > GETDATE()", connection);
                command.Parameters.AddWithValue("@Token", token);
                
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Cliente
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Email = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Senha = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Telefone = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            TemWhatsApp = reader.GetBoolean(5),
                            // Provider e ProviderId não são mais usados (linhas 6 e 7 ignoradas)
                            TokenConfirmacao = reader.IsDBNull(8) ? null : reader.GetString(8),
                            TokenRecuperacaoSenha = reader.IsDBNull(9) ? null : reader.GetString(9),
                            DataExpiracaoRecuperacaoSenha = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                            EmailConfirmado = reader.GetBoolean(11),
                            WhatsAppConfirmado = reader.GetBoolean(12),
                            IsAdmin = reader.GetBoolean(13),
                            DataCadastro = reader.GetDateTime(14),
                            UltimoAcesso = reader.IsDBNull(15) ? (DateTime?)null : reader.GetDateTime(15)
                        };
                    }
                }
            }
            return null;
        }

        public void GerarTokenRecuperacaoSenha(string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string token = Guid.NewGuid().ToString("N");
                DateTime dataExpiracao = DateTime.Now.AddHours(24); // Token válido por 24 horas
                
                var command = new SqlCommand("UPDATE Clientes SET TokenRecuperacaoSenha = @Token, DataExpiracaoRecuperacaoSenha = @DataExpiracao WHERE LOWER(LTRIM(RTRIM(Email))) = @Email", connection);
                command.Parameters.AddWithValue("@Token", token);
                command.Parameters.AddWithValue("@DataExpiracao", dataExpiracao);
                command.Parameters.AddWithValue("@Email", email.ToLowerInvariant().Trim());
                command.ExecuteNonQuery();
            }
        }

        public void RedefinirSenha(string token, string novaSenhaHash)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("UPDATE Clientes SET Senha = @Senha, TokenRecuperacaoSenha = NULL, DataExpiracaoRecuperacaoSenha = NULL WHERE TokenRecuperacaoSenha = @Token AND DataExpiracaoRecuperacaoSenha > GETDATE()", connection);
                command.Parameters.AddWithValue("@Senha", novaSenhaHash);
                command.Parameters.AddWithValue("@Token", token);
                command.ExecuteNonQuery();
            }
        }

        public List<Reserva> ObterReservasPorCliente(int clienteId)
        {
            var reservas = new List<Reserva>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(@"
                    SELECT r.Id, r.DataRetirada, r.DataReserva, r.StatusId, r.ValorTotal, r.Observacoes, 
                           r.ConvertidoEmPedido, r.PrevisaoEntrega, r.Cancelado, r.ClienteId, r.TokenAcesso,
                           c.Nome, c.Email, c.Telefone, sr.Nome as StatusNome
                    FROM Reservas r
                    LEFT JOIN Clientes c ON r.ClienteId = c.Id
                    LEFT JOIN StatusReserva sr ON r.StatusId = sr.Id
                    WHERE r.ClienteId = @ClienteId
                    ORDER BY r.DataReserva DESC", connection);
                command.Parameters.AddWithValue("@ClienteId", clienteId);
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var reserva = new Reserva
                        {
                            Id = reader.GetInt32(0),
                            DataRetirada = reader.GetDateTime(1),
                            DataReserva = reader.GetDateTime(2),
                            StatusId = reader.GetInt32(3),
                            ValorTotal = reader.GetDecimal(4),
                            Observacoes = reader.IsDBNull(5) ? "" : reader.GetString(5),
                            ConvertidoEmPedido = reader.GetBoolean(6),
                            PrevisaoEntrega = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                            Cancelado = reader.GetBoolean(8),
                            ClienteId = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9),
                            TokenAcesso = reader.IsDBNull(10) ? "" : reader.GetString(10),
                            Nome = reader.IsDBNull(11) ? "" : reader.GetString(11),
                            Email = reader.IsDBNull(12) ? "" : reader.GetString(12),
                            Telefone = reader.IsDBNull(13) ? "" : reader.GetString(13),
                            Status = reader.IsDBNull(14) ? "" : reader.GetString(14),
                            Itens = new List<ItemPedido>()
                        };
                        reservas.Add(reserva);
                    }
                }

                // Carregar itens de cada reserva
                foreach (var reserva in reservas)
                {
                    var itensCommand = new SqlCommand(@"
                        SELECT Id, ReservaId, ProdutoId, NomeProduto, Tamanho, Quantidade, PrecoUnitario, Subtotal, ISNULL(Produtos, '')
                        FROM ReservaItens
                        WHERE ReservaId = @ReservaId", connection);
                    itensCommand.Parameters.AddWithValue("@ReservaId", reserva.Id);
                    
                    using (var itensReader = itensCommand.ExecuteReader())
                    {
                        while (itensReader.Read())
                        {
                            reserva.Itens.Add(new ItemPedido
                            {
                                ProdutoId = itensReader.GetInt32(2),
                                NomeProduto = itensReader.GetString(3),
                                Tamanho = itensReader.GetString(4),
                                Quantidade = itensReader.GetInt32(5),
                                PrecoUnitario = itensReader.GetDecimal(6),
                                Subtotal = itensReader.GetDecimal(7),
                                Produtos = itensReader.IsDBNull(8) ? "" : itensReader.GetString(8)
                            });
                        }
                    }
                }
            }
            
            return reservas;
        }

        public Reserva ObterReservaPorToken(string token)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                // Se o token for numérico, pode ser um ID de reserva antiga sem token
                int reservaId;
                bool isNumeric = int.TryParse(token, out reservaId);
                
                var command = new SqlCommand(@"
                    SELECT r.Id, r.DataRetirada, r.DataReserva, r.StatusId, r.ValorTotal, r.Observacoes, 
                           r.ConvertidoEmPedido, r.PrevisaoEntrega, r.Cancelado, r.ClienteId, r.TokenAcesso,
                           c.Nome, c.Email, c.Telefone, sr.Nome as StatusNome
                    FROM Reservas r
                    LEFT JOIN Clientes c ON r.ClienteId = c.Id
                    LEFT JOIN StatusReserva sr ON r.StatusId = sr.Id
                    WHERE (r.TokenAcesso = @Token OR (@IsNumeric = 1 AND r.Id = @ReservaId))", connection);
                command.Parameters.AddWithValue("@Token", token);
                command.Parameters.AddWithValue("@IsNumeric", isNumeric);
                command.Parameters.AddWithValue("@ReservaId", isNumeric ? reservaId : 0);
                
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var reserva = new Reserva
                        {
                            Id = reader.GetInt32(0),
                            DataRetirada = reader.GetDateTime(1),
                            DataReserva = reader.GetDateTime(2),
                            StatusId = reader.GetInt32(3),
                            ValorTotal = reader.GetDecimal(4),
                            Observacoes = reader.IsDBNull(5) ? "" : reader.GetString(5),
                            ConvertidoEmPedido = reader.GetBoolean(6),
                            PrevisaoEntrega = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                            Cancelado = reader.GetBoolean(8),
                            ClienteId = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9),
                            TokenAcesso = reader.IsDBNull(10) ? "" : reader.GetString(10),
                            Nome = reader.IsDBNull(11) ? "" : reader.GetString(11),
                            Email = reader.IsDBNull(12) ? "" : reader.GetString(12),
                            Telefone = reader.IsDBNull(13) ? "" : reader.GetString(13),
                            Status = reader.IsDBNull(14) ? "" : reader.GetString(14),
                            Itens = new List<ItemPedido>()
                        };

                        // Carregar itens da reserva
                        var itensCommand = new SqlCommand(@"
                            SELECT Id, ReservaId, ProdutoId, NomeProduto, Tamanho, Quantidade, PrecoUnitario, Subtotal, ISNULL(Produtos, '')
                            FROM ReservaItens
                            WHERE ReservaId = @ReservaId", connection);
                        itensCommand.Parameters.AddWithValue("@ReservaId", reserva.Id);
                        
                        using (var itensReader = itensCommand.ExecuteReader())
                        {
                            while (itensReader.Read())
                            {
                                reserva.Itens.Add(new ItemPedido
                                {
                                    ProdutoId = itensReader.GetInt32(2),
                                    NomeProduto = itensReader.GetString(3),
                                    Tamanho = itensReader.GetString(4),
                                    Quantidade = itensReader.GetInt32(5),
                                    PrecoUnitario = itensReader.GetDecimal(6),
                                    Subtotal = itensReader.GetDecimal(7),
                                    Produtos = itensReader.IsDBNull(8) ? "" : itensReader.GetString(8)
                                });
                            }
                        }

                        return reserva;
                    }
                }
            }
            return null;
        }

        public void CancelarReserva(int reservaId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                // Buscar dados da reserva antes de cancelar para log
                var reserva = ObterReservaPorId(reservaId);
                string detalhesReserva = "";
                if (reserva != null)
                {
                    detalhesReserva = $"ID: {reservaId}, ClienteId: {reserva.ClienteId}, ValorTotal: R$ {reserva.ValorTotal:F2}, Status anterior: {reserva.Status ?? "N/A"}";
                }
                else
                {
                    detalhesReserva = $"ID: {reservaId}";
                }
                
                // Buscar o StatusId do status "Cancelado"
                var statusCancelado = ObterStatusReservaPorNome("Cancelado");
                if (statusCancelado == null)
                {
                    throw new Exception("Status 'Cancelado' não encontrado na tabela StatusReserva. Verifique se o banco de dados foi inicializado corretamente.");
                }
                
                // Atualizar o StatusId para "Cancelado" e marcar como cancelado
                var command = new SqlCommand(@"
                    UPDATE Reservas 
                    SET StatusId = @StatusId, Cancelado = 1
                    WHERE Id = @Id", connection);
                
                command.Parameters.AddWithValue("@Id", reservaId);
                command.Parameters.AddWithValue("@StatusId", statusCancelado.Id);
                command.ExecuteNonQuery();
                
                // Registrar log
                string usuarioLog = LogService.ObterUsuarioAtual(HttpContext.Current?.Session);
                LogService.RegistrarCancelamento(usuarioLog, "Reserva", "DatabaseService.CancelarReserva", detalhesReserva);
            }
        }

        public void ExcluirReserva(int reservaId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                // Buscar dados da reserva antes de excluir para log
                var reserva = ObterReservaPorId(reservaId);
                string detalhesReserva = "";
                if (reserva != null)
                {
                    detalhesReserva = $"ID: {reservaId}, ClienteId: {reserva.ClienteId}, ValorTotal: R$ {reserva.ValorTotal:F2}, Status: {reserva.Status ?? "N/A"}, DataRetirada: {reserva.DataRetirada:dd/MM/yyyy}";
                }
                else
                {
                    detalhesReserva = $"ID: {reservaId}";
                }
                
                var command = new SqlCommand("DELETE FROM Reservas WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", reservaId);
                command.ExecuteNonQuery();
                
                // Registrar log
                string usuarioLog = LogService.ObterUsuarioAtual(HttpContext.Current?.Session);
                LogService.RegistrarExclusao(usuarioLog, "Reserva", "DatabaseService.ExcluirReserva", detalhesReserva);
            }
        }

        public int CriarStatusReserva(StatusReserva status)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                var valoresNovos = new Dictionary<string, string>
                {
                    ["Nome"] = status.Nome ?? "",
                    ["Descricao"] = status.Descricao ?? "",
                    ["PermiteAlteracao"] = status.PermiteAlteracao.ToString(),
                    ["PermiteExclusao"] = status.PermiteExclusao.ToString(),
                    ["Ordem"] = status.Ordem.ToString()
                };
                
                var command = new SqlCommand(@"
                    INSERT INTO StatusReserva (Nome, Descricao, PermiteAlteracao, PermiteExclusao, Ordem)
                    VALUES (@Nome, @Descricao, @PermiteAlteracao, @PermiteExclusao, @Ordem);
                    SELECT CAST(SCOPE_IDENTITY() as int);", connection);
                
                command.Parameters.AddWithValue("@Nome", status.Nome);
                command.Parameters.AddWithValue("@Descricao", status.Descricao ?? "");
                command.Parameters.AddWithValue("@PermiteAlteracao", status.PermiteAlteracao);
                command.Parameters.AddWithValue("@PermiteExclusao", status.PermiteExclusao);
                command.Parameters.AddWithValue("@Ordem", status.Ordem);
                
                int novoId = (int)command.ExecuteScalar();
                
                // Registrar log
                string usuarioLog = LogService.ObterUsuarioAtual(HttpContext.Current?.Session);
                LogService.RegistrarInsercao(usuarioLog, "StatusReserva", "DatabaseService.CriarStatusReserva", 
                    $"ID: {novoId}, Nome: {status.Nome}, Descricao: {status.Descricao}, PermiteAlteracao: {status.PermiteAlteracao}, PermiteExclusao: {status.PermiteExclusao}, Ordem: {status.Ordem}");
                
                return novoId;
            }
        }

        public void AtualizarStatusReserva(StatusReserva status)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                // Buscar dados antigos para comparação
                StatusReserva statusAntigo = ObterStatusReservaPorId(status.Id);
                var valoresAntigos = new Dictionary<string, string>();
                var valoresNovos = new Dictionary<string, string>();
                
                if (statusAntigo != null)
                {
                    valoresAntigos["Nome"] = statusAntigo.Nome ?? "";
                    valoresAntigos["Descricao"] = statusAntigo.Descricao ?? "";
                    valoresAntigos["PermiteAlteracao"] = statusAntigo.PermiteAlteracao.ToString();
                    valoresAntigos["PermiteExclusao"] = statusAntigo.PermiteExclusao.ToString();
                    valoresAntigos["Ordem"] = statusAntigo.Ordem.ToString();
                }
                
                valoresNovos["Nome"] = status.Nome ?? "";
                valoresNovos["Descricao"] = status.Descricao ?? "";
                valoresNovos["PermiteAlteracao"] = status.PermiteAlteracao.ToString();
                valoresNovos["PermiteExclusao"] = status.PermiteExclusao.ToString();
                valoresNovos["Ordem"] = status.Ordem.ToString();
                
                var command = new SqlCommand(@"
                    UPDATE StatusReserva 
                    SET Nome = @Nome, Descricao = @Descricao, PermiteAlteracao = @PermiteAlteracao, 
                        PermiteExclusao = @PermiteExclusao, Ordem = @Ordem
                    WHERE Id = @Id", connection);
                
                command.Parameters.AddWithValue("@Id", status.Id);
                command.Parameters.AddWithValue("@Nome", status.Nome);
                command.Parameters.AddWithValue("@Descricao", status.Descricao ?? "");
                command.Parameters.AddWithValue("@PermiteAlteracao", status.PermiteAlteracao);
                command.Parameters.AddWithValue("@PermiteExclusao", status.PermiteExclusao);
                command.Parameters.AddWithValue("@Ordem", status.Ordem);
                
                command.ExecuteNonQuery();
                
                // Registrar log com comparação
                string usuarioLog = LogService.ObterUsuarioAtual(HttpContext.Current?.Session);
                LogService.RegistrarAtualizacaoComComparacao(usuarioLog, "StatusReserva", "DatabaseService.AtualizarStatusReserva", 
                    status.Id.ToString(), valoresAntigos, valoresNovos);
            }
        }

        // Métodos para excluir dados com verificação de dependências
        public bool PodeExcluirStatusReserva(int statusId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT COUNT(*) FROM Reservas WHERE StatusId = @StatusId", connection);
                command.Parameters.AddWithValue("@StatusId", statusId);
                int count = (int)command.ExecuteScalar();
                return count == 0;
            }
        }

        public bool PodeExcluirProduto(int produtoId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT COUNT(*) FROM ReservaItens WHERE ProdutoId = @ProdutoId", connection);
                command.Parameters.AddWithValue("@ProdutoId", produtoId);
                int count = (int)command.ExecuteScalar();
                return count == 0;
            }
        }

        public bool PodeExcluirCliente(int clienteId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT COUNT(*) FROM Reservas WHERE ClienteId = @ClienteId", connection);
                command.Parameters.AddWithValue("@ClienteId", clienteId);
                int count = (int)command.ExecuteScalar();
                return count == 0;
            }
        }

        public void ExcluirStatusReserva(int statusId)
        {
            if (!PodeExcluirStatusReserva(statusId))
            {
                throw new Exception("Não é possível excluir este status pois existem reservas associadas a ele.");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("DELETE FROM StatusReserva WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", statusId);
                command.ExecuteNonQuery();
                
                // Registrar log
                string usuarioLog = LogService.ObterUsuarioAtual(HttpContext.Current?.Session);
                LogService.RegistrarExclusao(usuarioLog, "StatusReserva", "DatabaseService.ExcluirStatusReserva", $"ID: {statusId}");
            }
        }

        public void ExcluirProduto(int produtoId)
        {
            // Buscar nome do produto antes de excluir para o log
            string nomeProduto = "";
            try
            {
                var todosProdutos = ObterTodosProdutos();
                var produto = todosProdutos.FirstOrDefault(p => p.Id == produtoId);
                if (produto != null)
                {
                    nomeProduto = produto.Nome;
                }
            }
            catch
            {
                // Ignorar erro
            }
            
            if (!PodeExcluirProduto(produtoId))
            {
                throw new Exception("Não é possível excluir este produto pois existem reservas associadas a ele.");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("DELETE FROM Produtos WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", produtoId);
                command.ExecuteNonQuery();
                
                // Registrar log
                string usuarioLog = LogService.ObterUsuarioAtual(HttpContext.Current?.Session);
                string detalhes = $"ID: {produtoId}";
                if (!string.IsNullOrEmpty(nomeProduto))
                {
                    detalhes += $", Nome: {nomeProduto}";
                }
                LogService.RegistrarExclusao(usuarioLog, "Produto", "DatabaseService.ExcluirProduto", detalhes);
            }
        }

        public void ExcluirCliente(int clienteId)
        {
            // Buscar nome do cliente antes de excluir para o log
            string nomeCliente = "";
            try
            {
                var cliente = ObterClientePorId(clienteId);
                if (cliente != null)
                {
                    nomeCliente = cliente.Nome;
                }
            }
            catch
            {
                // Ignorar erro
            }
            
            if (!PodeExcluirCliente(clienteId))
            {
                throw new Exception("Não é possível excluir este cliente pois existem reservas associadas a ele.");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("DELETE FROM Clientes WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", clienteId);
                command.ExecuteNonQuery();
                
                // Registrar log
                string usuarioLog = LogService.ObterUsuarioAtual(HttpContext.Current?.Session);
                string detalhes = $"ID: {clienteId}";
                if (!string.IsNullOrEmpty(nomeCliente))
                {
                    detalhes += $", Nome: {nomeCliente}";
                }
                LogService.RegistrarExclusao(usuarioLog, "Cliente", "DatabaseService.ExcluirCliente", detalhes);
            }
        }

        public void LimparTodosClientesEReservas()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                // Desabilitar verificação de chave estrangeira temporariamente
                var disableFK = new SqlCommand("ALTER TABLE Reservas NOCHECK CONSTRAINT ALL", connection);
                disableFK.ExecuteNonQuery();
                
                // Deletar todas as reservas primeiro (devido à chave estrangeira)
                var deleteReservas = new SqlCommand("DELETE FROM ReservaItens; DELETE FROM Reservas", connection);
                deleteReservas.ExecuteNonQuery();
                
                // Deletar todos os clientes
                var deleteClientes = new SqlCommand("DELETE FROM Clientes", connection);
                deleteClientes.ExecuteNonQuery();
                
                // Reabilitar verificação de chave estrangeira
                var enableFK = new SqlCommand("ALTER TABLE Reservas CHECK CONSTRAINT ALL", connection);
                enableFK.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Calcula a quantidade total de produtos a produzir, incluindo os que estão dentro de sacos/cestas/caixas
        /// </summary>
        /// <param name="dataRetirada">Data de retirada para filtrar reservas (null para todas as datas)</param>
        /// <returns>Dicionário com ProdutoId como chave e quantidade total como valor</returns>
        public Dictionary<int, int> CalcularProdutosAProduzir(DateTime? dataRetirada = null)
        {
            var produtosQuantidade = new Dictionary<int, int>();
            
            // Obter todas as reservas não canceladas
            var reservas = ObterTodasReservas().Where(r => !r.Cancelado).ToList();
            
            // Filtrar por data de retirada se especificada
            if (dataRetirada.HasValue)
            {
                reservas = reservas.Where(r => r.DataRetirada.Date == dataRetirada.Value.Date).ToList();
            }
            
            // Obter status que indicam que ainda precisa produzir
            var statusAberta = ObterStatusReservaPorNome("Aberta");
            var statusEmProducao = ObterStatusReservaPorNome("Em Produção");
            var statusPreparandoEntrega = ObterStatusReservaPorNome("Preparando Entrega");
            var statusSaiuParaEntrega = ObterStatusReservaPorNome("Saiu para Entrega");
            
            var statusIds = new List<int>();
            if (statusAberta != null) statusIds.Add(statusAberta.Id);
            if (statusEmProducao != null) statusIds.Add(statusEmProducao.Id);
            if (statusPreparandoEntrega != null) statusIds.Add(statusPreparandoEntrega.Id);
            if (statusSaiuParaEntrega != null) statusIds.Add(statusSaiuParaEntrega.Id);
            
            // Filtrar reservas que ainda precisam ser produzidas
            reservas = reservas.Where(r => r.StatusId.HasValue && statusIds.Contains(r.StatusId.Value)).ToList();
            
            var serializer = new JavaScriptSerializer();
            
            foreach (var reserva in reservas)
            {
                foreach (var item in reserva.Itens)
                {
                    // Adicionar o produto principal
                    if (!produtosQuantidade.ContainsKey(item.ProdutoId))
                    {
                        produtosQuantidade[item.ProdutoId] = 0;
                    }
                    produtosQuantidade[item.ProdutoId] += item.Quantidade;
                    
                    // Se tiver produtos dentro do saco/cesta/caixa, adicionar também
                    if (!string.IsNullOrEmpty(item.Produtos))
                    {
                        try
                        {
                            var produtosJson = serializer.Deserialize<List<Dictionary<string, object>>>(item.Produtos);
                            
                            foreach (var produtoJson in produtosJson)
                            {
                                int qt = Convert.ToInt32(produtoJson["qt"]);
                                int prodId = Convert.ToInt32(produtoJson["id"]);
                                
                                // Multiplicar pela quantidade do item (saco/cesta/caixa)
                                int quantidadeTotal = qt * item.Quantidade;
                                
                                if (!produtosQuantidade.ContainsKey(prodId))
                                {
                                    produtosQuantidade[prodId] = 0;
                                }
                                produtosQuantidade[prodId] += quantidadeTotal;
                            }
                        }
                        catch
                        {
                            // Se falhar ao parsear, ignorar
                        }
                    }
                }
            }
            
            return produtosQuantidade;
        }
    }
}

