PASTA App_Data - BANCO DE DADOS
================================

Esta pasta contém o banco de dados SQL Server Express LocalDB.

ARQUIVOS:
---------
- KingdomConfeitaria.mdf - Arquivo principal do banco de dados
- KingdomConfeitaria_Log.ldf - Arquivo de log do banco de dados
- CreateDatabase.sql - Script SQL para criar o banco de dados

COMO CRIAR O BANCO DE DADOS:
----------------------------

OPÇÃO 1: Via SQL Server Management Studio (SSMS)
1. Abra o SQL Server Management Studio
2. Conecte-se a (LocalDB)\MSSQLLocalDB
3. Abra o arquivo CreateDatabase.sql
4. Execute o script (F5)

OPÇÃO 2: Via Visual Studio
1. Abra o Visual Studio
2. Vá em View > SQL Server Object Explorer
3. Conecte-se a (LocalDB)\MSSQLLocalDB
4. Clique com botão direito em Databases > Add New Database
5. Nome: KingdomConfeitaria
6. Execute o script CreateDatabase.sql

OPÇÃO 3: Via sqlcmd (linha de comando)
1. Abra o PowerShell ou CMD
2. Execute:
   sqlcmd -S "(LocalDB)\MSSQLLocalDB" -i "App_Data\CreateDatabase.sql"

O banco será criado automaticamente na primeira execução se usar LocalDB.

PERMISSÕES:
-----------
Certifique-se de que a pasta App_Data tem permissões de escrita para:
- IIS_IUSRS (se usar IIS)
- Usuário do Application Pool
- Usuário atual (para desenvolvimento)

