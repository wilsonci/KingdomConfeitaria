# Scripts de Migração de Banco de Dados

## Estrutura de Bancos de Dados

- **Desenvolvimento**: `KingdomConfeitaria_Dev` (LocalDB)
- **Produção**: `KingdomConfeitaria_Prod` (SQL Server)

## Processo de Migração

### 1. Desenvolvimento → Produção

Quando você fizer alterações estruturais no banco de desenvolvimento, siga estes passos:

#### Passo 1: Gerar Script de Migração

1. Conecte-se ao banco de desenvolvimento
2. Use o SQL Server Management Studio (SSMS) para gerar scripts:
   - Clique com botão direito no banco `KingdomConfeitaria_Dev`
   - Tasks → Generate Scripts
   - Selecione "Script specific database objects"
   - Escolha as tabelas/objetos que foram alterados
   - Em "Set Scripting Options", escolha "Save to file"
   - Salve como `Scripts/Migracao_YYYYMMDD_HHMMSS.sql`

#### Passo 2: Revisar o Script

1. Abra o script gerado
2. Remova comandos de criação de banco (CREATE DATABASE)
3. Adicione verificações de existência antes de criar objetos:
   ```sql
   IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NomeTabela]') AND type in (N'U'))
   BEGIN
       -- Comandos de criação
   END
   ```

#### Passo 3: Testar o Script

1. Crie um banco de teste: `KingdomConfeitaria_Test`
2. Execute o script no banco de teste
3. Verifique se não há erros
4. Teste a aplicação com o banco de teste

#### Passo 4: Aplicar em Produção

1. **FAÇA BACKUP DO BANCO DE PRODUÇÃO ANTES DE QUALQUER ALTERAÇÃO**
   ```sql
   BACKUP DATABASE KingdomConfeitaria_Prod 
   TO DISK = 'C:\Backups\KingdomConfeitaria_Prod_' + CONVERT(VARCHAR, GETDATE(), 112) + '.bak'
   WITH FORMAT, COMPRESSION;
   ```

2. Conecte-se ao banco de produção
3. Execute o script de migração
4. Verifique se não há erros
5. Teste a aplicação em produção

### 2. Estrutura de Tabelas Atual

As tabelas principais são:
- `Clientes`
- `Produtos`
- `Reservas`
- `ReservaItens`
- `StatusReserva`

## Scripts de Migração Disponíveis

### Criar Estrutura Inicial (Primeira Vez)

Use o método `CriarBancoETabelasSeNaoExistirem()` do `DatabaseService` como referência.

### Adicionar Nova Coluna

```sql
-- Exemplo: Adicionar coluna NovaColuna na tabela Produtos
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[Produtos]') 
               AND name = 'NovaColuna')
BEGIN
    ALTER TABLE [dbo].[Produtos]
    ADD [NovaColuna] [nvarchar](100) NULL
END
GO
```

### Remover Coluna

```sql
-- Exemplo: Remover coluna ColunaAntiga da tabela Produtos
IF EXISTS (SELECT * FROM sys.columns 
           WHERE object_id = OBJECT_ID(N'[dbo].[Produtos]') 
           AND name = 'ColunaAntiga')
BEGIN
    ALTER TABLE [dbo].[Produtos]
    DROP COLUMN [ColunaAntiga]
END
GO
```

### Criar Nova Tabela

```sql
IF NOT EXISTS (SELECT * FROM sys.objects 
               WHERE object_id = OBJECT_ID(N'[dbo].[NovaTabela]') 
               AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[NovaTabela](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Nome] [nvarchar](200) NOT NULL,
        CONSTRAINT [PK_NovaTabela] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END
GO
```

### Adicionar Índice

```sql
IF NOT EXISTS (SELECT * FROM sys.indexes 
               WHERE name = 'IX_NomeTabela_Coluna' 
               AND object_id = OBJECT_ID('dbo.NomeTabela'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_NomeTabela_Coluna]
    ON [dbo].[NomeTabela] ([Coluna])
END
GO
```

## Checklist de Migração

Antes de aplicar uma migração em produção:

- [ ] Script foi testado em banco de teste
- [ ] Backup do banco de produção foi criado
- [ ] Script foi revisado e validado
- [ ] Horário de manutenção foi agendado (se necessário)
- [ ] Equipe foi notificada
- [ ] Plano de rollback foi preparado

## Rollback

Se algo der errado:

1. **NÃO ENTRE EM PÂNICO**
2. Restaure o backup do banco de produção
3. Analise os logs de erro
4. Corrija o script
5. Teste novamente em ambiente de teste
6. Reaplique quando estiver seguro

## Comandos Úteis

### Verificar Estrutura de uma Tabela

```sql
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'NomeTabela'
ORDER BY ORDINAL_POSITION
```

### Verificar Índices

```sql
SELECT 
    i.name AS IndexName,
    i.type_desc AS IndexType,
    COL_NAME(ic.object_id, ic.column_id) AS ColumnName
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE i.object_id = OBJECT_ID('dbo.NomeTabela')
```

### Verificar Constraints

```sql
SELECT 
    CONSTRAINT_NAME,
    CONSTRAINT_TYPE
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
WHERE TABLE_NAME = 'NomeTabela'
```

## Notas Importantes

1. **SEMPRE faça backup antes de migrar**
2. **Teste em ambiente de teste primeiro**
3. **Documente todas as alterações**
4. **Mantenha um histórico de migrações**
5. **Use transações quando possível**:
   ```sql
   BEGIN TRANSACTION
   -- Seus comandos aqui
   -- Se tudo estiver OK:
   COMMIT TRANSACTION
   -- Se houver erro:
   ROLLBACK TRANSACTION
   ```

