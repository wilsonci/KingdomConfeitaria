using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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

        public DatabaseService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["KingdomConfeitariaDB"].ConnectionString;
            
            // Verificar se está em ambiente de desenvolvimento
            string environment = ConfigurationManager.AppSettings["Environment"] ?? "Development";
            bool isDevelopment = environment.Equals("Development", StringComparison.OrdinalIgnoreCase);
            
            // Criar banco e tabelas automaticamente
            // Em desenvolvimento: cria banco no LocalDB padrão
            // Em produção: cria banco na pasta App_Data usando AttachDbFilename
            CriarBancoETabelasSeNaoExistirem();
        }

        private void CriarBancoETabelasSeNaoExistirem()
        {
            try
            {
                // Verificar se a connection string usa AttachDbFilename (banco na pasta App_Data)
                bool usaAttachDbFilename = _connectionString.Contains("AttachDbFilename=");
                
                if (usaAttachDbFilename)
                {
                    // Banco na pasta App_Data (desenvolvimento e produção)
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
                            
                            // Garantir que a pasta App_Data existe
                            if (!System.IO.Directory.Exists(dataDirectory))
                            {
                                System.IO.Directory.CreateDirectory(dataDirectory);
                            }
                            
                            attachDbPath = attachDbPath.Replace("|DataDirectory|", dataDirectory);
                        }
                        
                        // Criar connection string para master
                        string masterConnectionString = _connectionString;
                        if (masterConnectionString.Contains("AttachDbFilename="))
                        {
                            // Remover AttachDbFilename e Initial Catalog, adicionar master
                            int attachStart = masterConnectionString.IndexOf("AttachDbFilename=");
                            int attachEnd = masterConnectionString.IndexOf(";", attachStart);
                            if (attachEnd == -1) attachEnd = masterConnectionString.Length;
                            masterConnectionString = masterConnectionString.Remove(attachStart, attachEnd - attachStart);
                            
                            if (masterConnectionString.Contains("Initial Catalog="))
                            {
                                int catalogStart = masterConnectionString.IndexOf("Initial Catalog=");
                                int catalogEnd = masterConnectionString.IndexOf(";", catalogStart);
                                if (catalogEnd == -1) catalogEnd = masterConnectionString.Length;
                                masterConnectionString = masterConnectionString.Remove(catalogStart, catalogEnd - catalogStart);
                            }
                            
                            masterConnectionString = masterConnectionString.TrimEnd(';') + ";Initial Catalog=master";
                        }
                        
                        // Extrair nome do banco
                        string databaseName = "KingdomConfeitaria_Prod";
                        if (_connectionString.Contains("Initial Catalog="))
                        {
                            int startIdx = _connectionString.IndexOf("Initial Catalog=") + "Initial Catalog=".Length;
                            int endIdx = _connectionString.IndexOf(";", startIdx);
                            if (endIdx == -1) endIdx = _connectionString.Length;
                            databaseName = _connectionString.Substring(startIdx, endIdx - startIdx).Trim();
                        }
                        
                        // Verificar se o arquivo .mdf já existe
                        if (!System.IO.File.Exists(attachDbPath))
                        {
                            // Verificar se o banco já existe no LocalDB (pode ter sido criado antes sem AttachDbFilename)
                            using (var masterConnection = new SqlConnection(masterConnectionString))
                            {
                                masterConnection.Open();
                                
                                // Verificar se o banco existe
                                var checkDbCommand = new SqlCommand($"SELECT COUNT(*) FROM sys.databases WHERE name = '{databaseName}'", masterConnection);
                                int dbExists = (int)checkDbCommand.ExecuteScalar();
                                
                                if (dbExists > 0)
                                {
                                    // Banco existe no LocalDB, fazer DETACH para poder criar o arquivo .mdf
                                    try
                                    {
                                        var detachCommand = new SqlCommand($@"
                                            ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                                            EXEC sp_detach_db '{databaseName}', 'true'", masterConnection);
                                        detachCommand.ExecuteNonQuery();
                                    }
                                    catch
                                    {
                                        // Se não conseguir fazer detach, tentar continuar mesmo assim
                                    }
                                }
                                
                                // Criar banco no arquivo .mdf
                                string logPath = attachDbPath.Replace(".mdf", "_log.ldf");
                                var createDbCommand = new SqlCommand($@"
                                    CREATE DATABASE [{databaseName}]
                                    ON (NAME = '{databaseName}', FILENAME = '{attachDbPath.Replace("\\", "\\\\")}')
                                    LOG ON (NAME = '{databaseName}_Log', FILENAME = '{logPath.Replace("\\", "\\\\")}')", masterConnection);
                                createDbCommand.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // Arquivo .mdf existe, verificar se o banco está anexado
                            using (var masterConnection = new SqlConnection(masterConnectionString))
                            {
                                masterConnection.Open();
                                
                                // Verificar se o banco está anexado
                                var checkDbCommand = new SqlCommand($"SELECT COUNT(*) FROM sys.databases WHERE name = '{databaseName}'", masterConnection);
                                int dbExists = (int)checkDbCommand.ExecuteScalar();
                                
                                if (dbExists == 0)
                                {
                                    // Arquivo existe mas banco não está anexado, fazer ATTACH
                                    try
                                    {
                                        string logPath = attachDbPath.Replace(".mdf", "_log.ldf");
                                        var attachCommand = new SqlCommand($@"
                                            CREATE DATABASE [{databaseName}]
                                            ON (FILENAME = '{attachDbPath.Replace("\\", "\\\\")}')
                                            FOR ATTACH", masterConnection);
                                        attachCommand.ExecuteNonQuery();
                                    }
                                    catch
                                    {
                                        // Se não conseguir fazer attach, tentar criar novamente
                                        // (pode ser que o arquivo esteja corrompido)
                                        try
                                        {
                                            System.IO.File.Delete(attachDbPath);
                                            string logPath = attachDbPath.Replace(".mdf", "_log.ldf");
                                            if (System.IO.File.Exists(logPath))
                                            {
                                                System.IO.File.Delete(logPath);
                                            }
                                            
                                            // Recriar o banco
                                            var createDbCommand = new SqlCommand($@"
                                                CREATE DATABASE [{databaseName}]
                                                ON (NAME = '{databaseName}', FILENAME = '{attachDbPath.Replace("\\", "\\\\")}')
                                                LOG ON (NAME = '{databaseName}_Log', FILENAME = '{logPath.Replace("\\", "\\\\")}')", masterConnection);
                                            createDbCommand.ExecuteNonQuery();
                                        }
                                        catch
                                        {
                                            // Se falhar, deixar o erro ser propagado
                                        }
                                    }
                                }
                                // Se o banco já está anexado, não fazer nada (já está pronto para uso)
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
                throw new Exception("Erro ao acessar o banco de dados. Verifique se o SQL Server LocalDB está instalado e funcionando. Erro: " + sqlEx.Message, sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao inicializar o banco de dados: " + ex.Message, ex);
            }
        }

        public List<Produto> ObterProdutos()
        {
            var produtos = new List<Produto>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, Nome, Descricao, Preco, ImagemUrl, Ativo, Ordem, ISNULL(EhSacoPromocional, 0), ISNULL(QuantidadeSaco, 0), ISNULL(Produtos, ''), ReservavelAte, VendivelAte FROM Produtos WHERE Ativo = 1 ORDER BY Ordem", connection);
                
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

        public List<Produto> ObterTodosProdutos()
        {
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
                
                foreach (var item in reserva.Itens)
                {
                    try
                    {
                        // Verificar se o produto existe antes de inserir (validação rápida)
                        var verificarProduto = new SqlCommand("SELECT COUNT(*) FROM Produtos WHERE Id = @ProdutoId", connection);
                        verificarProduto.Parameters.AddWithValue("@ProdutoId", item.ProdutoId);
                        var produtoExiste = (int)verificarProduto.ExecuteScalar() > 0;
                        
                        if (!produtoExiste)
                        {
                            produtosNaoEncontrados.Add(item.ProdutoId);
                            itensPulados++;
                            continue;
                        }
                        
                        var itemCommand = new SqlCommand(@"
                            INSERT INTO ReservaItens (ReservaId, ProdutoId, NomeProduto, Tamanho, Quantidade, PrecoUnitario, Subtotal, Produtos)
                            VALUES (@ReservaId, @ProdutoId, @NomeProduto, @Tamanho, @Quantidade, @PrecoUnitario, @Subtotal, @Produtos)", connection);
                        
                        itemCommand.Parameters.AddWithValue("@ReservaId", reservaId);
                        itemCommand.Parameters.AddWithValue("@ProdutoId", item.ProdutoId);
                        itemCommand.Parameters.AddWithValue("@NomeProduto", item.NomeProduto ?? "");
                        itemCommand.Parameters.AddWithValue("@Tamanho", item.Tamanho ?? "");
                        itemCommand.Parameters.AddWithValue("@Quantidade", item.Quantidade);
                        itemCommand.Parameters.AddWithValue("@PrecoUnitario", item.PrecoUnitario);
                        itemCommand.Parameters.AddWithValue("@Subtotal", item.Subtotal);
                        itemCommand.Parameters.AddWithValue("@Produtos", item.Produtos ?? (object)DBNull.Value);
                        
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

        public void AtualizarProduto(Produto produto)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
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
                
                // Registrar log
                string usuarioLog = LogService.ObterUsuarioAtual(HttpContext.Current?.Session);
                string detalhes = $"ID: {produto.Id}, Nome: {produto.Nome}, Preco: R$ {produto.Preco:F2}, Ativo: {produto.Ativo}";
                LogService.RegistrarAtualizacao(usuarioLog, "Produto", "DatabaseService.AtualizarProduto", detalhes);
            }
        }

        public void AdicionarProduto(Produto produto)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(@"
                    INSERT INTO Produtos (Nome, Descricao, Preco, ImagemUrl, Ativo, Ordem, EhSacoPromocional, QuantidadeSaco, Produtos, ReservavelAte, VendivelAte)
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
                
                command.ExecuteNonQuery();
                
                // Registrar log
                string usuarioLog = LogService.ObterUsuarioAtual(HttpContext.Current?.Session);
                string detalhes = $"Nome: {produto.Nome}, Preco: R$ {produto.Preco:F2}, Ativo: {produto.Ativo}";
                LogService.RegistrarInsercao(usuarioLog, "Produto", "DatabaseService.AdicionarProduto", detalhes);
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
                    var reservaAtual = ObterReservaPorId(reserva.Id);
                    if (reservaAtual != null)
                    {
                        reserva.StatusId = reservaAtual.StatusId;
                    }
                }
                
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
                
                // Registrar log
                string usuarioLog = LogService.ObterUsuarioAtual(HttpContext.Current?.Session);
                string detalhes = $"ID: {reserva.Id}, StatusId: {reserva.StatusId}, Status: {reserva.Status}, ValorTotal: R$ {reserva.ValorTotal:F2}";
                LogService.RegistrarAtualizacao(usuarioLog, "Reserva", "DatabaseService.AtualizarReserva", detalhes);
            }
        }

        public void AtualizarItensReserva(int reservaId, List<ItemPedido> novosItens)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                // Deletar todos os itens antigos
                var deleteCommand = new SqlCommand("DELETE FROM ReservaItens WHERE ReservaId = @ReservaId", connection);
                deleteCommand.Parameters.AddWithValue("@ReservaId", reservaId);
                deleteCommand.ExecuteNonQuery();
                
                // Obter lista de produtos existentes
                var produtosExistentes = new HashSet<int>();
                var produtosCommand = new SqlCommand("SELECT Id FROM Produtos", connection);
                using (var produtosReader = produtosCommand.ExecuteReader())
                {
                    while (produtosReader.Read())
                    {
                        produtosExistentes.Add(produtosReader.GetInt32(0));
                    }
                }
                
                // Inserir novos itens
                foreach (var item in novosItens)
                {
                    // Validar se o produto existe
                    if (!produtosExistentes.Contains(item.ProdutoId))
                    {
                        continue; // Pular produtos que não existem mais
                    }
                    
                    var insertCommand = new SqlCommand(@"
                        INSERT INTO ReservaItens (ReservaId, ProdutoId, NomeProduto, Tamanho, Quantidade, PrecoUnitario, Subtotal, Produtos)
                        VALUES (@ReservaId, @ProdutoId, @NomeProduto, @Tamanho, @Quantidade, @PrecoUnitario, @Subtotal, @Produtos)", connection);
                    
                    insertCommand.Parameters.AddWithValue("@ReservaId", reservaId);
                    insertCommand.Parameters.AddWithValue("@ProdutoId", item.ProdutoId);
                    insertCommand.Parameters.AddWithValue("@NomeProduto", item.NomeProduto ?? "");
                    insertCommand.Parameters.AddWithValue("@Tamanho", item.Tamanho ?? "");
                    insertCommand.Parameters.AddWithValue("@Quantidade", item.Quantidade);
                    insertCommand.Parameters.AddWithValue("@PrecoUnitario", item.PrecoUnitario);
                    insertCommand.Parameters.AddWithValue("@Subtotal", item.Subtotal);
                    insertCommand.Parameters.AddWithValue("@Produtos", item.Produtos ?? (object)DBNull.Value);
                    
                    insertCommand.ExecuteNonQuery();
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
                            Provider = reader.IsDBNull(6) ? null : reader.GetString(6),
                            ProviderId = reader.IsDBNull(7) ? null : reader.GetString(7),
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
                            Provider = reader.IsDBNull(6) ? null : reader.GetString(6),
                            ProviderId = reader.IsDBNull(7) ? null : reader.GetString(7),
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
                            Provider = reader.IsDBNull(6) ? null : reader.GetString(6),
                            ProviderId = reader.IsDBNull(7) ? null : reader.GetString(7),
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
                // Buscar por telefone formatado (apenas números)
                // A query remove formatação do telefone no banco e compara com o telefone formatado
                // Usar LTRIM e RTRIM para remover espaços e garantir comparação correta
                var command = new SqlCommand(@"
                    SELECT Id, Nome, Email, Senha, Telefone, TemWhatsApp, Provider, ProviderId, TokenConfirmacao, TokenRecuperacaoSenha, DataExpiracaoRecuperacaoSenha, EmailConfirmado, WhatsAppConfirmado, IsAdmin, DataCadastro, UltimoAcesso 
                    FROM Clientes 
                    WHERE LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ISNULL(Telefone, ''), '(', ''), ')', ''), '-', ''), ' ', ''), '.', ''), '/', ''))) = @Telefone", connection);
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
                            Provider = reader.IsDBNull(6) ? null : reader.GetString(6),
                            ProviderId = reader.IsDBNull(7) ? null : reader.GetString(7),
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

        public Cliente ObterClientePorProvider(string provider, string providerId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, Nome, Email, Senha, Telefone, TemWhatsApp, Provider, ProviderId, TokenConfirmacao, TokenRecuperacaoSenha, DataExpiracaoRecuperacaoSenha, EmailConfirmado, WhatsAppConfirmado, IsAdmin, DataCadastro, UltimoAcesso FROM Clientes WHERE Provider = @Provider AND ProviderId = @ProviderId", connection);
                command.Parameters.AddWithValue("@Provider", provider);
                command.Parameters.AddWithValue("@ProviderId", providerId);
                
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
                            Provider = reader.IsDBNull(6) ? null : reader.GetString(6),
                            ProviderId = reader.IsDBNull(7) ? null : reader.GetString(7),
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
                            Provider = reader.IsDBNull(6) ? null : reader.GetString(6),
                            ProviderId = reader.IsDBNull(7) ? null : reader.GetString(7),
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
                
                // Verificar se cliente já existe (por Provider, Email OU Telefone)
                Cliente clienteExistente = null;
                if (!string.IsNullOrEmpty(cliente.Provider) && !string.IsNullOrEmpty(cliente.ProviderId))
                {
                    clienteExistente = ObterClientePorProvider(cliente.Provider, cliente.ProviderId);
                }
                
                // Se não encontrou por Provider, tentar por Email ou Telefone
                if (clienteExistente == null)
                {
                    clienteExistente = ObterClientePorEmailOuTelefone(cliente.Email, cliente.Telefone);
                }
                
                if (clienteExistente != null)
                {
                    // Se o cliente existente tem um dos emails de administrador, garantir que IsAdmin seja true
                    bool clienteExistenteEhAdmin = VerificarSeEhAdministrador(clienteExistente.Email);
                    bool isAdminFinal = ehAdministrador || clienteExistenteEhAdmin;
                    
                    // Atualizar cliente existente
                    var command = new SqlCommand(@"
                        UPDATE Clientes 
                        SET Nome = @Nome, Email = @Email, Senha = @Senha, Telefone = @Telefone, TemWhatsApp = @TemWhatsApp, 
                            IsAdmin = @IsAdmin, UltimoAcesso = GETDATE()
                        WHERE Id = @Id", connection);
                    
                    command.Parameters.AddWithValue("@Id", clienteExistente.Id);
                    command.Parameters.AddWithValue("@Nome", cliente.Nome);
                    command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(cliente.Email) ? (object)DBNull.Value : cliente.Email);
                    command.Parameters.AddWithValue("@Senha", string.IsNullOrEmpty(cliente.Senha) ? (object)DBNull.Value : cliente.Senha);
                    command.Parameters.AddWithValue("@Telefone", string.IsNullOrEmpty(cliente.Telefone) ? (object)DBNull.Value : cliente.Telefone);
                    command.Parameters.AddWithValue("@TemWhatsApp", cliente.TemWhatsApp);
                    command.Parameters.AddWithValue("@IsAdmin", isAdminFinal);
                    
                    command.ExecuteNonQuery();
                    return clienteExistente.Id;
                }
                else
                {
                    // Criar novo cliente
                    if (string.IsNullOrEmpty(cliente.TokenConfirmacao))
                    {
                        cliente.TokenConfirmacao = Guid.NewGuid().ToString("N");
                    }
                    
                    var command = new SqlCommand(@"
                        INSERT INTO Clientes (Nome, Email, Senha, Telefone, TemWhatsApp, Provider, ProviderId, TokenConfirmacao, EmailConfirmado, WhatsAppConfirmado, IsAdmin, DataCadastro)
                        VALUES (@Nome, @Email, @Senha, @Telefone, @TemWhatsApp, @Provider, @ProviderId, @TokenConfirmacao, @EmailConfirmado, @WhatsAppConfirmado, @IsAdmin, GETDATE());
                        SELECT CAST(SCOPE_IDENTITY() as int);", connection);
                    
                    // Garantir que email e telefone estejam formatados
                    string emailFormatado = FormatarEmail(cliente.Email);
                    string telefoneFormatado = FormatarTelefone(cliente.Telefone);
                    
                    command.Parameters.AddWithValue("@Nome", cliente.Nome);
                    command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(emailFormatado) ? (object)DBNull.Value : emailFormatado);
                    command.Parameters.AddWithValue("@Senha", string.IsNullOrEmpty(cliente.Senha) ? (object)DBNull.Value : cliente.Senha);
                    command.Parameters.AddWithValue("@Telefone", string.IsNullOrEmpty(telefoneFormatado) ? (object)DBNull.Value : telefoneFormatado);
                    command.Parameters.AddWithValue("@TemWhatsApp", cliente.TemWhatsApp);
                    command.Parameters.AddWithValue("@Provider", string.IsNullOrEmpty(cliente.Provider) ? (object)DBNull.Value : cliente.Provider);
                    command.Parameters.AddWithValue("@ProviderId", string.IsNullOrEmpty(cliente.ProviderId) ? (object)DBNull.Value : cliente.ProviderId);
                    command.Parameters.AddWithValue("@TokenConfirmacao", cliente.TokenConfirmacao);
                    command.Parameters.AddWithValue("@EmailConfirmado", cliente.EmailConfirmado);
                    command.Parameters.AddWithValue("@WhatsAppConfirmado", cliente.WhatsAppConfirmado);
                    command.Parameters.AddWithValue("@IsAdmin", cliente.IsAdmin);
                    
                    int novoClienteId = (int)command.ExecuteScalar();
                    
                    // Registrar log
                    string usuarioLog = LogService.ObterUsuarioAtual(HttpContext.Current?.Session);
                    string detalhes = $"ID: {novoClienteId}, Nome: {cliente.Nome}, Email: {cliente.Email}, IsAdmin: {cliente.IsAdmin}";
                    LogService.RegistrarInsercao(usuarioLog, "Cliente", "DatabaseService.CriarOuAtualizarCliente", detalhes);
                    
                    return novoClienteId;
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
                            Provider = reader.IsDBNull(6) ? null : reader.GetString(6),
                            ProviderId = reader.IsDBNull(7) ? null : reader.GetString(7),
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
                LogService.RegistrarCancelamento(usuarioLog, "Reserva", "DatabaseService.CancelarReserva", $"ID: {reservaId}");
            }
        }

        public void ExcluirReserva(int reservaId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("DELETE FROM Reservas WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", reservaId);
                command.ExecuteNonQuery();
                
                // Registrar log
                string usuarioLog = LogService.ObterUsuarioAtual(HttpContext.Current?.Session);
                LogService.RegistrarExclusao(usuarioLog, "Reserva", "DatabaseService.ExcluirReserva", $"ID: {reservaId}");
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

