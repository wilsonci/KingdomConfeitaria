# Nota sobre Serviços SQL no Servidor de Produção

## Configuração Atual

✅ **SQL Server Express**: Serviço **PARADO** (para economizar recursos)
✅ **SQL Server LocalDB**: Serviço **RODANDO** (ativo)

## Connection String de Produção

A aplicação está configurada para usar **LocalDB** em produção:

```xml
Data Source=.\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\KingdomConfeitaria_Prod.mdf;Initial Catalog=KingdomConfeitaria_Prod;Integrated Security=True;Connect Timeout=30;MultipleActiveResultSets=True
```

## Vantagens do LocalDB

- ✅ **Leve**: Consome menos recursos que SQL Server Express
- ✅ **Sem serviço dedicado**: Não precisa de serviço Windows rodando constantemente
- ✅ **Banco na pasta App_Data**: Fácil backup (apenas copiar arquivo .mdf)
- ✅ **Adequado para pequenos/médios volumes**: Suporta até ~10GB de dados

## Quando Usar LocalDB vs SQL Express

### LocalDB (Atual) - Recomendado para:
- ✅ Aplicações com poucos usuários simultâneos (< 100)
- ✅ Servidores com recursos limitados
- ✅ Quando SQL Express está parado para economizar recursos
- ✅ Bancos de dados pequenos/médios (< 10GB)

### SQL Express - Use quando:
- ❌ Precisa de mais de 10GB de dados
- ❌ Muitos usuários simultâneos (> 100)
- ❌ Precisa de recursos avançados (Full-Text Search, etc.)

## Gerenciamento do LocalDB

### Verificar Status
```powershell
sqllocaldb info MSSQLLocalDB
```

### Iniciar LocalDB (se necessário)
```powershell
sqllocaldb start MSSQLLocalDB
```

### Parar LocalDB (se necessário)
```powershell
sqllocaldb stop MSSQLLocalDB
```

## Backup do Banco

O banco está em: `[Pasta da Aplicação]\App_Data\KingdomConfeitaria_Prod.mdf`

Para fazer backup:
1. Pare a aplicação (ou aguarde momento de baixo uso)
2. Copie o arquivo `KingdomConfeitaria_Prod.mdf`
3. Copie também o arquivo `KingdomConfeitaria_Prod_log.ldf` (se existir)

## Performance

Com apenas LocalDB rodando:
- ✅ Menor consumo de memória
- ✅ Menor consumo de CPU
- ✅ Inicialização mais rápida
- ✅ Adequado para a maioria dos casos de uso

## Status

✅ **Configuração correta para o ambiente atual**
✅ **LocalDB é a escolha adequada quando SQL Express está parado**
✅ **Connection string configurada corretamente**

