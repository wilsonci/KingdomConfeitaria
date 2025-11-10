using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using KingdomConfeitaria.Models;

namespace KingdomConfeitaria.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["KingdomConfeitariaDB"].ConnectionString;
            CriarBancoETabelasSeNaoExistirem();
        }

        private void CriarBancoETabelasSeNaoExistirem()
        {
            try
            {
                // Primeiro, criar o banco de dados se não existir
                var masterConnectionString = _connectionString.Replace("Initial Catalog=KingdomConfeitaria", "Initial Catalog=master");
                using (var masterConnection = new SqlConnection(masterConnectionString))
                {
                    masterConnection.Open();
                    var createDbCommand = new SqlCommand(@"
                        IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'KingdomConfeitaria')
                        BEGIN
                            CREATE DATABASE KingdomConfeitaria
                        END", masterConnection);
                    createDbCommand.ExecuteNonQuery();
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
                                [PrecoPequeno] [decimal](10, 2) NOT NULL,
                                [PrecoGrande] [decimal](10, 2) NOT NULL,
                                [ImagemUrl] [nvarchar](500) NULL,
                                [Ativo] [bit] NOT NULL DEFAULT(1),
                                [Ordem] [int] NOT NULL DEFAULT(0),
                                [EhSacoPromocional] [bit] NOT NULL DEFAULT(0),
                                [QuantidadeSaco] [int] NOT NULL DEFAULT(0),
                                [TamanhoSaco] [nvarchar](50) NULL,
                                CONSTRAINT [PK_Produtos] PRIMARY KEY CLUSTERED ([Id] ASC)
                            )
                        END", connection);
                    checkTable.ExecuteNonQuery();

                    // Verificar se a tabela Reservas existe
                    checkTable.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Reservas]') AND type in (N'U'))
                        BEGIN
                            CREATE TABLE [dbo].[Reservas](
                                [Id] [int] IDENTITY(1,1) NOT NULL,
                                [Nome] [nvarchar](200) NOT NULL,
                                [Email] [nvarchar](200) NOT NULL,
                                [Telefone] [nvarchar](50) NULL,
                                [DataRetirada] [datetime] NOT NULL,
                                [DataReserva] [datetime] NOT NULL,
                                [Status] [nvarchar](50) NOT NULL DEFAULT('Pendente'),
                                [ValorTotal] [decimal](10, 2) NOT NULL,
                                [Observacoes] [nvarchar](1000) NULL,
                                [ConvertidoEmPedido] [bit] NOT NULL DEFAULT(0),
                                [PrevisaoEntrega] [datetime] NULL,
                                [Cancelado] [bit] NOT NULL DEFAULT(0),
                                CONSTRAINT [PK_Reservas] PRIMARY KEY CLUSTERED ([Id] ASC)
                            )
                        END";
                    checkTable.ExecuteNonQuery();

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
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Produtos]') AND name = 'TamanhoSaco')
                        BEGIN
                            ALTER TABLE [dbo].[Produtos] ADD [TamanhoSaco] [nvarchar](50) NULL
                        END";
                    checkTable.ExecuteNonQuery();

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
                                CONSTRAINT [PK_ReservaItens] PRIMARY KEY CLUSTERED ([Id] ASC),
                                CONSTRAINT [FK_ReservaItens_Reservas] FOREIGN KEY([ReservaId]) 
                                    REFERENCES [dbo].[Reservas] ([Id]) ON DELETE CASCADE,
                                CONSTRAINT [FK_ReservaItens_Produtos] FOREIGN KEY([ProdutoId]) 
                                    REFERENCES [dbo].[Produtos] ([Id])
                            )
                        END";
                    checkTable.ExecuteNonQuery();

                    // Verificar se a tabela Clientes existe
                    checkTable.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Clientes]') AND type in (N'U'))
                        BEGIN
                            CREATE TABLE [dbo].[Clientes](
                                [Id] [int] IDENTITY(1,1) NOT NULL,
                                [Nome] [nvarchar](200) NOT NULL,
                                [Email] [nvarchar](200) NULL,
                                [Telefone] [nvarchar](50) NULL,
                                [TemWhatsApp] [bit] NOT NULL DEFAULT(0),
                                [Provider] [nvarchar](50) NULL,
                                [ProviderId] [nvarchar](200) NULL,
                                [TokenConfirmacao] [nvarchar](100) NULL,
                                [EmailConfirmado] [bit] NOT NULL DEFAULT(0),
                                [WhatsAppConfirmado] [bit] NOT NULL DEFAULT(0),
                                [DataCadastro] [datetime] NOT NULL DEFAULT(GETDATE()),
                                [UltimoAcesso] [datetime] NULL,
                                CONSTRAINT [PK_Clientes] PRIMARY KEY CLUSTERED ([Id] ASC)
                            )
                            CREATE UNIQUE INDEX [IX_Clientes_Email] ON [dbo].[Clientes]([Email]) WHERE [Email] IS NOT NULL
                            CREATE INDEX [IX_Clientes_Provider] ON [dbo].[Clientes]([Provider], [ProviderId])
                        END";
                    checkTable.ExecuteNonQuery();

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

                    // Adicionar ClienteId e TokenAcesso na tabela Reservas se não existir
                    checkTable.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Reservas]') AND name = 'ClienteId')
                        BEGIN
                            ALTER TABLE [dbo].[Reservas] ADD [ClienteId] [int] NULL
                            ALTER TABLE [dbo].[Reservas] ADD CONSTRAINT [FK_Reservas_Clientes] 
                                FOREIGN KEY([ClienteId]) REFERENCES [dbo].[Clientes] ([Id])
                        END
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Reservas]') AND name = 'TokenAcesso')
                        BEGIN
                            ALTER TABLE [dbo].[Reservas] ADD [TokenAcesso] [nvarchar](100) NULL
                        END";
                    checkTable.ExecuteNonQuery();

                    // Limpar produtos existentes e inserir novos produtos
                    var clearData = new SqlCommand("DELETE FROM Produtos", connection);
                    clearData.ExecuteNonQuery();
                    
                    // Inserir novos produtos
                    var insertData = new SqlCommand(@"
                        INSERT INTO Produtos (Nome, Descricao, PrecoPequeno, PrecoGrande, ImagemUrl, Ativo, Ordem, EhSacoPromocional, QuantidadeSaco, TamanhoSaco) VALUES
                        ('Gingerbread Estrela Pequeno', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Estrela Pequeno', 5.00, 0.00, 'Images/estrela-pequeno.jpg', 1, 1, 0, 0, NULL),
                        ('Gingerbread Estrela Grande', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Estrela Grande', 0.00, 10.00, 'Images/estrela-grande.jpg', 1, 2, 0, 0, NULL),
                        ('Gingerbread Floco de Neve Pequeno', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Floco de Neve Pequeno', 5.00, 0.00, 'Images/floco-neve-pequeno.jpg', 1, 3, 0, 0, NULL),
                        ('Gingerbread Floco de Neve Grande', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Floco de Neve Grande', 0.00, 10.00, 'Images/floco-neve-grande.jpg', 1, 4, 0, 0, NULL),
                        ('Gingerbread Guirlanda Pequeno', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Guirlanda Pequeno', 5.00, 0.00, 'Images/guirlanda-pequeno.jpg', 1, 5, 0, 0, NULL),
                        ('Gingerbread Guirlanda Grande', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Guirlanda Grande', 0.00, 10.00, 'Images/guirlanda-grande.jpg', 1, 6, 0, 0, NULL),
                        ('Gingerbread Meia Pequeno', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Meia Pequeno', 5.00, 0.00, 'Images/meia-pequeno.jpg', 1, 7, 0, 0, NULL),
                        ('Gingerbread Meia Grande', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Meia Grande', 0.00, 10.00, 'Images/meia-grande.jpg', 1, 8, 0, 0, NULL),
                        ('Gingerbread Árvore Pequeno', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Árvore Pequeno', 5.00, 0.00, 'Images/arvore-pequeno.jpg', 1, 9, 0, 0, NULL),
                        ('Gingerbread Árvore Grande', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Árvore Grande', 0.00, 10.00, 'Images/arvore-grande.jpg', 1, 10, 0, 0, NULL),
                        ('Gingerbread Coração Pequeno', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Coração Pequeno', 5.00, 0.00, 'Images/coracao-pequeno.jpg', 1, 11, 0, 0, NULL),
                        ('Gingerbread Coração Grande', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Coração Grande', 0.00, 10.00, 'Images/coracao-grande.jpg', 1, 12, 0, 0, NULL),
                        ('Gingerbread Boneco Pequeno', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Boneco Pequeno', 5.00, 0.00, 'Images/boneco-pequeno.jpg', 1, 13, 0, 0, NULL),
                        ('Gingerbread Boneco Grande', 'Gingerbread biscoito artesanal de gengibre com especiarias com pouca açúcar - Boneco Grande', 0.00, 10.00, 'Images/boneco-grande.jpg', 1, 14, 0, 0, NULL),
                        ('Saco Promocional - 6 Pequenos', 'Saco com 6 biscoitos pequenos (escolha os formatos). De R$ 30,00 por R$ 21,00 na promoção!', 21.00, 0.00, 'Images/saco-6-pequenos.jpg', 1, 15, 1, 6, 'Pequeno'),
                        ('Saco Promocional - 3 Grandes', 'Saco com 3 biscoitos grandes (escolha os formatos). De R$ 30,00 por R$ 21,00 na promoção!', 0.00, 21.00, 'Images/saco-3-grandes.jpg', 1, 16, 1, 3, 'Grande')", connection);
                    insertData.ExecuteNonQuery();
                }
            }
            catch (SqlException sqlEx)
            {
                // Log do erro SQL
                System.Diagnostics.Debug.WriteLine("Erro SQL ao criar banco/tabelas: " + sqlEx.Message);
                System.Diagnostics.Debug.WriteLine("Número do erro: " + sqlEx.Number);
                throw new Exception("Erro ao acessar o banco de dados. Verifique se o SQL Server LocalDB está instalado e funcionando. Erro: " + sqlEx.Message, sqlEx);
            }
            catch (Exception ex)
            {
                // Log do erro (em produção, use um sistema de logging adequado)
                System.Diagnostics.Debug.WriteLine("Erro ao criar banco/tabelas: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack trace: " + ex.StackTrace);
                throw new Exception("Erro ao inicializar o banco de dados: " + ex.Message, ex);
            }
        }

        public List<Produto> ObterProdutos()
        {
            var produtos = new List<Produto>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, Nome, Descricao, PrecoPequeno, PrecoGrande, ImagemUrl, Ativo, Ordem, ISNULL(EhSacoPromocional, 0), ISNULL(QuantidadeSaco, 0), TamanhoSaco FROM Produtos WHERE Ativo = 1 ORDER BY Ordem", connection);
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        produtos.Add(new Produto
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Descricao = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            PrecoPequeno = reader.GetDecimal(3),
                            PrecoGrande = reader.GetDecimal(4),
                            ImagemUrl = reader.IsDBNull(5) ? "" : reader.GetString(5),
                            Ativo = reader.GetBoolean(6),
                            Ordem = reader.GetInt32(7),
                            EhSacoPromocional = reader.IsDBNull(8) ? false : reader.GetBoolean(8),
                            QuantidadeSaco = reader.IsDBNull(9) ? 0 : reader.GetInt32(9),
                            TamanhoSaco = reader.IsDBNull(10) ? null : reader.GetString(10)
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
                var command = new SqlCommand("SELECT Id, Nome, Descricao, PrecoPequeno, PrecoGrande, ImagemUrl, Ativo, Ordem, ISNULL(EhSacoPromocional, 0), ISNULL(QuantidadeSaco, 0), TamanhoSaco FROM Produtos ORDER BY Ordem", connection);
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        produtos.Add(new Produto
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Descricao = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            PrecoPequeno = reader.GetDecimal(3),
                            PrecoGrande = reader.GetDecimal(4),
                            ImagemUrl = reader.IsDBNull(5) ? "" : reader.GetString(5),
                            Ativo = reader.GetBoolean(6),
                            Ordem = reader.GetInt32(7),
                            EhSacoPromocional = reader.IsDBNull(8) ? false : reader.GetBoolean(8),
                            QuantidadeSaco = reader.IsDBNull(9) ? 0 : reader.GetInt32(9),
                            TamanhoSaco = reader.IsDBNull(10) ? null : reader.GetString(10)
                        });
                    }
                }
            }
            
            return produtos;
        }

        public List<Produto> ObterProdutosPorTamanho(string tamanho)
        {
            var produtos = new List<Produto>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = tamanho == "Pequeno" 
                    ? "SELECT Id, Nome, Descricao, PrecoPequeno, PrecoGrande, ImagemUrl, Ativo, Ordem, ISNULL(EhSacoPromocional, 0), ISNULL(QuantidadeSaco, 0), TamanhoSaco FROM Produtos WHERE Ativo = 1 AND PrecoPequeno > 0 AND EhSacoPromocional = 0 ORDER BY Ordem"
                    : "SELECT Id, Nome, Descricao, PrecoPequeno, PrecoGrande, ImagemUrl, Ativo, Ordem, ISNULL(EhSacoPromocional, 0), ISNULL(QuantidadeSaco, 0), TamanhoSaco FROM Produtos WHERE Ativo = 1 AND PrecoGrande > 0 AND EhSacoPromocional = 0 ORDER BY Ordem";
                
                var command = new SqlCommand(query, connection);
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        produtos.Add(new Produto
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Descricao = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            PrecoPequeno = reader.GetDecimal(3),
                            PrecoGrande = reader.GetDecimal(4),
                            ImagemUrl = reader.IsDBNull(5) ? "" : reader.GetString(5),
                            Ativo = reader.GetBoolean(6),
                            Ordem = reader.GetInt32(7),
                            EhSacoPromocional = reader.IsDBNull(8) ? false : reader.GetBoolean(8),
                            QuantidadeSaco = reader.IsDBNull(9) ? 0 : reader.GetInt32(9),
                            TamanhoSaco = reader.IsDBNull(10) ? null : reader.GetString(10)
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
                
                var command = new SqlCommand(@"
                    INSERT INTO Reservas (Nome, Email, Telefone, DataRetirada, DataReserva, Status, ValorTotal, Observacoes, ConvertidoEmPedido, PrevisaoEntrega, Cancelado, ClienteId, TokenAcesso)
                    VALUES (@Nome, @Email, @Telefone, @DataRetirada, @DataReserva, @Status, @ValorTotal, @Observacoes, @ConvertidoEmPedido, @PrevisaoEntrega, @Cancelado, @ClienteId, @TokenAcesso);
                    SELECT CAST(SCOPE_IDENTITY() as int);", connection);
                
                command.Parameters.AddWithValue("@Nome", reserva.Nome);
                command.Parameters.AddWithValue("@Email", reserva.Email);
                command.Parameters.AddWithValue("@Telefone", reserva.Telefone ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@DataRetirada", reserva.DataRetirada);
                command.Parameters.AddWithValue("@DataReserva", reserva.DataReserva);
                command.Parameters.AddWithValue("@Status", reserva.Status);
                command.Parameters.AddWithValue("@ValorTotal", reserva.ValorTotal);
                command.Parameters.AddWithValue("@Observacoes", reserva.Observacoes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ConvertidoEmPedido", reserva.ConvertidoEmPedido);
                command.Parameters.AddWithValue("@PrevisaoEntrega", reserva.PrevisaoEntrega.HasValue ? (object)reserva.PrevisaoEntrega.Value : DBNull.Value);
                command.Parameters.AddWithValue("@Cancelado", reserva.Cancelado);
                command.Parameters.AddWithValue("@ClienteId", reserva.ClienteId.HasValue ? (object)reserva.ClienteId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@TokenAcesso", reserva.TokenAcesso);
                
                var reservaId = (int)command.ExecuteScalar();
                
                // Salvar itens da reserva
                foreach (var item in reserva.Itens)
                {
                    var itemCommand = new SqlCommand(@"
                        INSERT INTO ReservaItens (ReservaId, ProdutoId, NomeProduto, Tamanho, Quantidade, PrecoUnitario, Subtotal)
                        VALUES (@ReservaId, @ProdutoId, @NomeProduto, @Tamanho, @Quantidade, @PrecoUnitario, @Subtotal)", connection);
                    
                    itemCommand.Parameters.AddWithValue("@ReservaId", reservaId);
                    itemCommand.Parameters.AddWithValue("@ProdutoId", item.ProdutoId);
                    itemCommand.Parameters.AddWithValue("@NomeProduto", item.NomeProduto);
                    itemCommand.Parameters.AddWithValue("@Tamanho", item.Tamanho);
                    itemCommand.Parameters.AddWithValue("@Quantidade", item.Quantidade);
                    itemCommand.Parameters.AddWithValue("@PrecoUnitario", item.PrecoUnitario);
                    itemCommand.Parameters.AddWithValue("@Subtotal", item.Subtotal);
                    
                    itemCommand.ExecuteNonQuery();
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
                    SET Nome = @Nome, Descricao = @Descricao, PrecoPequeno = @PrecoPequeno, 
                        PrecoGrande = @PrecoGrande, ImagemUrl = @ImagemUrl, Ativo = @Ativo, Ordem = @Ordem,
                        EhSacoPromocional = @EhSacoPromocional, QuantidadeSaco = @QuantidadeSaco, TamanhoSaco = @TamanhoSaco
                    WHERE Id = @Id", connection);
                
                command.Parameters.AddWithValue("@Id", produto.Id);
                command.Parameters.AddWithValue("@Nome", produto.Nome);
                command.Parameters.AddWithValue("@Descricao", produto.Descricao ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PrecoPequeno", produto.PrecoPequeno);
                command.Parameters.AddWithValue("@PrecoGrande", produto.PrecoGrande);
                command.Parameters.AddWithValue("@ImagemUrl", produto.ImagemUrl ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Ativo", produto.Ativo);
                command.Parameters.AddWithValue("@Ordem", produto.Ordem);
                command.Parameters.AddWithValue("@EhSacoPromocional", produto.EhSacoPromocional);
                command.Parameters.AddWithValue("@QuantidadeSaco", produto.QuantidadeSaco);
                command.Parameters.AddWithValue("@TamanhoSaco", produto.TamanhoSaco ?? (object)DBNull.Value);
                
                command.ExecuteNonQuery();
            }
        }

        public void AdicionarProduto(Produto produto)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(@"
                    INSERT INTO Produtos (Nome, Descricao, PrecoPequeno, PrecoGrande, ImagemUrl, Ativo, Ordem, EhSacoPromocional, QuantidadeSaco, TamanhoSaco)
                    VALUES (@Nome, @Descricao, @PrecoPequeno, @PrecoGrande, @ImagemUrl, @Ativo, @Ordem, @EhSacoPromocional, @QuantidadeSaco, @TamanhoSaco)", connection);
                
                command.Parameters.AddWithValue("@Nome", produto.Nome);
                command.Parameters.AddWithValue("@Descricao", produto.Descricao ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PrecoPequeno", produto.PrecoPequeno);
                command.Parameters.AddWithValue("@PrecoGrande", produto.PrecoGrande);
                command.Parameters.AddWithValue("@ImagemUrl", produto.ImagemUrl ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Ativo", produto.Ativo);
                command.Parameters.AddWithValue("@Ordem", produto.Ordem);
                command.Parameters.AddWithValue("@EhSacoPromocional", produto.EhSacoPromocional);
                command.Parameters.AddWithValue("@QuantidadeSaco", produto.QuantidadeSaco);
                command.Parameters.AddWithValue("@TamanhoSaco", produto.TamanhoSaco ?? (object)DBNull.Value);
                
                command.ExecuteNonQuery();
            }
        }

        public List<Reserva> ObterTodasReservas()
        {
            var reservas = new List<Reserva>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(@"
                    SELECT Id, Nome, Email, Telefone, DataRetirada, DataReserva, Status, ValorTotal, Observacoes, 
                           ConvertidoEmPedido, PrevisaoEntrega, Cancelado, ClienteId, TokenAcesso
                    FROM Reservas 
                    ORDER BY DataReserva DESC", connection);
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var reserva = new Reserva
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Email = reader.GetString(2),
                            Telefone = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            DataRetirada = reader.GetDateTime(4),
                            DataReserva = reader.GetDateTime(5),
                            Status = reader.GetString(6),
                            ValorTotal = reader.GetDecimal(7),
                            Observacoes = reader.IsDBNull(8) ? "" : reader.GetString(8),
                            ConvertidoEmPedido = reader.GetBoolean(9),
                            PrevisaoEntrega = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                            Cancelado = reader.GetBoolean(11),
                            ClienteId = reader.IsDBNull(12) ? (int?)null : reader.GetInt32(12),
                            TokenAcesso = reader.IsDBNull(13) ? "" : reader.GetString(13),
                            Itens = new List<ItemPedido>()
                        };
                        reservas.Add(reserva);
                    }
                }

                // Carregar itens de cada reserva
                foreach (var reserva in reservas)
                {
                    var itensCommand = new SqlCommand(@"
                        SELECT Id, ReservaId, ProdutoId, NomeProduto, Tamanho, Quantidade, PrecoUnitario, Subtotal
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
                                Subtotal = itensReader.GetDecimal(7)
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
                    SELECT Id, Nome, Email, Telefone, DataRetirada, DataReserva, Status, ValorTotal, Observacoes, 
                           ConvertidoEmPedido, PrevisaoEntrega, Cancelado, ClienteId, TokenAcesso
                    FROM Reservas 
                    WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        reserva = new Reserva
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Email = reader.GetString(2),
                            Telefone = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            DataRetirada = reader.GetDateTime(4),
                            DataReserva = reader.GetDateTime(5),
                            Status = reader.GetString(6),
                            ValorTotal = reader.GetDecimal(7),
                            Observacoes = reader.IsDBNull(8) ? "" : reader.GetString(8),
                            ConvertidoEmPedido = reader.GetBoolean(9),
                            PrevisaoEntrega = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                            Cancelado = reader.GetBoolean(11),
                            ClienteId = reader.IsDBNull(12) ? (int?)null : reader.GetInt32(12),
                            TokenAcesso = reader.IsDBNull(13) ? "" : reader.GetString(13),
                            Itens = new List<ItemPedido>()
                        };
                    }
                }

                if (reserva != null)
                {
                    // Carregar itens da reserva
                    var itensCommand = new SqlCommand(@"
                        SELECT Id, ReservaId, ProdutoId, NomeProduto, Tamanho, Quantidade, PrecoUnitario, Subtotal
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
                                Subtotal = itensReader.GetDecimal(7)
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
                var command = new SqlCommand(@"
                    UPDATE Reservas 
                    SET Status = @Status, ConvertidoEmPedido = @ConvertidoEmPedido, 
                        PrevisaoEntrega = @PrevisaoEntrega, Cancelado = @Cancelado,
                        Observacoes = @Observacoes, ValorTotal = @ValorTotal
                    WHERE Id = @Id", connection);
                
                command.Parameters.AddWithValue("@Id", reserva.Id);
                command.Parameters.AddWithValue("@Status", reserva.Status);
                command.Parameters.AddWithValue("@ConvertidoEmPedido", reserva.ConvertidoEmPedido);
                command.Parameters.AddWithValue("@PrevisaoEntrega", reserva.PrevisaoEntrega.HasValue ? (object)reserva.PrevisaoEntrega.Value : DBNull.Value);
                command.Parameters.AddWithValue("@Cancelado", reserva.Cancelado);
                command.Parameters.AddWithValue("@Observacoes", reserva.Observacoes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ValorTotal", reserva.ValorTotal);
                
                command.ExecuteNonQuery();
            }
        }

        public List<Cliente> ObterTodosClientes()
        {
            var clientes = new List<Cliente>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, Nome, Email, Telefone, TemWhatsApp, Provider, ProviderId, TokenConfirmacao, EmailConfirmado, WhatsAppConfirmado, DataCadastro, UltimoAcesso FROM Clientes", connection);
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clientes.Add(new Cliente
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Email = reader.GetString(2),
                            Telefone = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            TemWhatsApp = reader.GetBoolean(4),
                            Provider = reader.IsDBNull(5) ? null : reader.GetString(5),
                            ProviderId = reader.IsDBNull(6) ? null : reader.GetString(6),
                            TokenConfirmacao = reader.IsDBNull(7) ? null : reader.GetString(7),
                            EmailConfirmado = reader.GetBoolean(8),
                            WhatsAppConfirmado = reader.GetBoolean(9),
                            DataCadastro = reader.GetDateTime(10),
                            UltimoAcesso = reader.IsDBNull(11) ? (DateTime?)null : reader.GetDateTime(11)
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
                var command = new SqlCommand("SELECT Id, Nome, Email, Telefone, TemWhatsApp, Provider, ProviderId, TokenConfirmacao, EmailConfirmado, WhatsAppConfirmado, DataCadastro, UltimoAcesso FROM Clientes WHERE LOWER(LTRIM(RTRIM(Email))) = @Email", connection);
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
                            Telefone = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            TemWhatsApp = reader.GetBoolean(4),
                            Provider = reader.IsDBNull(5) ? null : reader.GetString(5),
                            ProviderId = reader.IsDBNull(6) ? null : reader.GetString(6),
                            TokenConfirmacao = reader.IsDBNull(7) ? null : reader.GetString(7),
                            EmailConfirmado = reader.GetBoolean(8),
                            WhatsAppConfirmado = reader.GetBoolean(9),
                            DataCadastro = reader.GetDateTime(10),
                            UltimoAcesso = reader.IsDBNull(11) ? (DateTime?)null : reader.GetDateTime(11)
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
                var command = new SqlCommand(@"
                    SELECT Id, Nome, Email, Telefone, TemWhatsApp, Provider, ProviderId, TokenConfirmacao, EmailConfirmado, WhatsAppConfirmado, DataCadastro, UltimoAcesso 
                    FROM Clientes 
                    WHERE REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Telefone, '(', ''), ')', ''), '-', ''), ' ', ''), '.', '') = @Telefone", connection);
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
                            Telefone = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            TemWhatsApp = reader.GetBoolean(4),
                            Provider = reader.IsDBNull(5) ? null : reader.GetString(5),
                            ProviderId = reader.IsDBNull(6) ? null : reader.GetString(6),
                            TokenConfirmacao = reader.IsDBNull(7) ? null : reader.GetString(7),
                            EmailConfirmado = reader.GetBoolean(8),
                            WhatsAppConfirmado = reader.GetBoolean(9),
                            DataCadastro = reader.GetDateTime(10),
                            UltimoAcesso = reader.IsDBNull(11) ? (DateTime?)null : reader.GetDateTime(11)
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
                var command = new SqlCommand("SELECT Id, Nome, Email, Telefone, TemWhatsApp, Provider, ProviderId, TokenConfirmacao, EmailConfirmado, WhatsAppConfirmado, DataCadastro, UltimoAcesso FROM Clientes WHERE Provider = @Provider AND ProviderId = @ProviderId", connection);
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
                            Email = reader.GetString(2),
                            Telefone = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            TemWhatsApp = reader.GetBoolean(4),
                            Provider = reader.GetString(5),
                            ProviderId = reader.GetString(6),
                            TokenConfirmacao = reader.IsDBNull(7) ? null : reader.GetString(7),
                            EmailConfirmado = reader.GetBoolean(8),
                            WhatsAppConfirmado = reader.GetBoolean(9),
                            DataCadastro = reader.GetDateTime(10),
                            UltimoAcesso = reader.IsDBNull(11) ? (DateTime?)null : reader.GetDateTime(11)
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
                var command = new SqlCommand("SELECT Id, Nome, Email, Telefone, TemWhatsApp, Provider, ProviderId, TokenConfirmacao, EmailConfirmado, WhatsAppConfirmado, DataCadastro, UltimoAcesso FROM Clientes WHERE TokenConfirmacao = @Token", connection);
                command.Parameters.AddWithValue("@Token", token);
                
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Cliente
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Email = reader.GetString(2),
                            Telefone = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            TemWhatsApp = reader.GetBoolean(4),
                            Provider = reader.IsDBNull(5) ? null : reader.GetString(5),
                            ProviderId = reader.IsDBNull(6) ? null : reader.GetString(6),
                            TokenConfirmacao = reader.GetString(7),
                            EmailConfirmado = reader.GetBoolean(8),
                            WhatsAppConfirmado = reader.GetBoolean(9),
                            DataCadastro = reader.GetDateTime(10),
                            UltimoAcesso = reader.IsDBNull(11) ? (DateTime?)null : reader.GetDateTime(11)
                        };
                    }
                }
            }
            return null;
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
                    // Atualizar cliente existente
                    var command = new SqlCommand(@"
                        UPDATE Clientes 
                        SET Nome = @Nome, Email = @Email, Telefone = @Telefone, TemWhatsApp = @TemWhatsApp, 
                            UltimoAcesso = GETDATE()
                        WHERE Id = @Id", connection);
                    
                    command.Parameters.AddWithValue("@Id", clienteExistente.Id);
                    command.Parameters.AddWithValue("@Nome", cliente.Nome);
                    command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(cliente.Email) ? (object)DBNull.Value : cliente.Email);
                    command.Parameters.AddWithValue("@Telefone", string.IsNullOrEmpty(cliente.Telefone) ? (object)DBNull.Value : cliente.Telefone);
                    command.Parameters.AddWithValue("@TemWhatsApp", cliente.TemWhatsApp);
                    
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
                        INSERT INTO Clientes (Nome, Email, Telefone, TemWhatsApp, Provider, ProviderId, TokenConfirmacao, EmailConfirmado, WhatsAppConfirmado, DataCadastro)
                        VALUES (@Nome, @Email, @Telefone, @TemWhatsApp, @Provider, @ProviderId, @TokenConfirmacao, @EmailConfirmado, @WhatsAppConfirmado, GETDATE());
                        SELECT CAST(SCOPE_IDENTITY() as int);", connection);
                    
                    // Garantir que email e telefone estejam formatados
                    string emailFormatado = FormatarEmail(cliente.Email);
                    string telefoneFormatado = FormatarTelefone(cliente.Telefone);
                    
                    command.Parameters.AddWithValue("@Nome", cliente.Nome);
                    command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(emailFormatado) ? (object)DBNull.Value : emailFormatado);
                    command.Parameters.AddWithValue("@Telefone", string.IsNullOrEmpty(telefoneFormatado) ? (object)DBNull.Value : telefoneFormatado);
                    command.Parameters.AddWithValue("@TemWhatsApp", cliente.TemWhatsApp);
                    command.Parameters.AddWithValue("@Provider", string.IsNullOrEmpty(cliente.Provider) ? (object)DBNull.Value : cliente.Provider);
                    command.Parameters.AddWithValue("@ProviderId", string.IsNullOrEmpty(cliente.ProviderId) ? (object)DBNull.Value : cliente.ProviderId);
                    command.Parameters.AddWithValue("@TokenConfirmacao", cliente.TokenConfirmacao);
                    command.Parameters.AddWithValue("@EmailConfirmado", cliente.EmailConfirmado);
                    command.Parameters.AddWithValue("@WhatsAppConfirmado", cliente.WhatsAppConfirmado);
                    
                    return (int)command.ExecuteScalar();
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

        public List<Reserva> ObterReservasPorCliente(int clienteId)
        {
            var reservas = new List<Reserva>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(@"
                    SELECT Id, Nome, Email, Telefone, DataRetirada, DataReserva, Status, ValorTotal, Observacoes, 
                           ConvertidoEmPedido, PrevisaoEntrega, Cancelado, ClienteId, TokenAcesso
                    FROM Reservas 
                    WHERE ClienteId = @ClienteId
                    ORDER BY DataReserva DESC", connection);
                command.Parameters.AddWithValue("@ClienteId", clienteId);
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var reserva = new Reserva
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Email = reader.GetString(2),
                            Telefone = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            DataRetirada = reader.GetDateTime(4),
                            DataReserva = reader.GetDateTime(5),
                            Status = reader.GetString(6),
                            ValorTotal = reader.GetDecimal(7),
                            Observacoes = reader.IsDBNull(8) ? "" : reader.GetString(8),
                            ConvertidoEmPedido = reader.GetBoolean(9),
                            PrevisaoEntrega = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                            Cancelado = reader.GetBoolean(11),
                            ClienteId = reader.IsDBNull(12) ? (int?)null : reader.GetInt32(12),
                            TokenAcesso = reader.IsDBNull(13) ? "" : reader.GetString(13),
                            Itens = new List<ItemPedido>()
                        };
                        reservas.Add(reserva);
                    }
                }

                // Carregar itens de cada reserva
                foreach (var reserva in reservas)
                {
                    var itensCommand = new SqlCommand(@"
                        SELECT Id, ReservaId, ProdutoId, NomeProduto, Tamanho, Quantidade, PrecoUnitario, Subtotal
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
                                Subtotal = itensReader.GetDecimal(7)
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
                var command = new SqlCommand(@"
                    SELECT Id, Nome, Email, Telefone, DataRetirada, DataReserva, Status, ValorTotal, Observacoes, 
                           ConvertidoEmPedido, PrevisaoEntrega, Cancelado, ClienteId, TokenAcesso
                    FROM Reservas 
                    WHERE TokenAcesso = @Token", connection);
                command.Parameters.AddWithValue("@Token", token);
                
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var reserva = new Reserva
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Email = reader.GetString(2),
                            Telefone = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            DataRetirada = reader.GetDateTime(4),
                            DataReserva = reader.GetDateTime(5),
                            Status = reader.GetString(6),
                            ValorTotal = reader.GetDecimal(7),
                            Observacoes = reader.IsDBNull(8) ? "" : reader.GetString(8),
                            ConvertidoEmPedido = reader.GetBoolean(9),
                            PrevisaoEntrega = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                            Cancelado = reader.GetBoolean(11),
                            ClienteId = reader.IsDBNull(12) ? (int?)null : reader.GetInt32(12),
                            TokenAcesso = reader.GetString(13),
                            Itens = new List<ItemPedido>()
                        };

                        // Carregar itens da reserva
                        var itensCommand = new SqlCommand(@"
                            SELECT Id, ReservaId, ProdutoId, NomeProduto, Tamanho, Quantidade, PrecoUnitario, Subtotal
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
                                    Subtotal = itensReader.GetDecimal(7)
                                });
                            }
                        }

                        return reserva;
                    }
                }
            }
            return null;
        }

        public void ExcluirReserva(int reservaId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("DELETE FROM Reservas WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", reservaId);
                command.ExecuteNonQuery();
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
    }
}

