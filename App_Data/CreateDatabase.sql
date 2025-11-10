-- Script para criar o banco de dados KingdomConfeitaria
-- Execute este script no SQL Server Management Studio ou via sqlcmd

-- Criar banco de dados
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'KingdomConfeitaria')
BEGIN
    CREATE DATABASE KingdomConfeitaria
    ON 
    ( NAME = 'KingdomConfeitaria',
      FILENAME = 'C:\Desenv\KingdomConfeitaria\App_Data\KingdomConfeitaria.mdf',
      SIZE = 10MB,
      MAXSIZE = 100MB,
      FILEGROWTH = 5MB )
    LOG ON 
    ( NAME = 'KingdomConfeitaria_Log',
      FILENAME = 'C:\Desenv\KingdomConfeitaria\App_Data\KingdomConfeitaria_Log.ldf',
      SIZE = 5MB,
      MAXSIZE = 50MB,
      FILEGROWTH = 5MB )
END
GO

USE KingdomConfeitaria
GO

-- Tabela de Produtos
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
        CONSTRAINT [PK_Produtos] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END
GO

-- Tabela de Reservas
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
        CONSTRAINT [PK_Reservas] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END
GO

-- Tabela de Itens da Reserva
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
END
GO

-- Inserir produtos de exemplo
IF NOT EXISTS (SELECT * FROM Produtos)
BEGIN
    INSERT INTO Produtos (Nome, Descricao, PrecoPequeno, PrecoGrande, ImagemUrl, Ativo, Ordem) VALUES
    ('Ginger Bread Tradicional', 'Biscoito de gengibre decorado com glacê real branco, perfeito para o Natal', 15.00, 25.00, 'https://via.placeholder.com/300x200?text=Ginger+Bread+Tradicional', 1, 1),
    ('Ginger Bread Natalino', 'Biscoito temático de Natal com decorações especiais e cores festivas', 18.00, 30.00, 'https://via.placeholder.com/300x200?text=Ginger+Bread+Natalino', 1, 2),
    ('Ginger Bread Personalizado', 'Biscoito com nome ou desenho personalizado conforme sua escolha', 20.00, 35.00, 'https://via.placeholder.com/300x200?text=Ginger+Bread+Personalizado', 1, 3),
    ('Ginger Bread Família', 'Kit com vários biscoitos para toda a família, ideal para presentear', 45.00, 75.00, 'https://via.placeholder.com/300x200?text=Ginger+Bread+Família', 1, 4)
END
GO

PRINT 'Banco de dados criado com sucesso!'
GO

