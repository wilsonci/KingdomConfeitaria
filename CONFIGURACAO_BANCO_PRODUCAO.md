# Configuração de Banco de Dados para Produção

## Mudanças Implementadas

### 1. Connection String de Produção
- ✅ **Data Source alterado**: De `(LocalDB)\MSSQLLocalDB` para `.\MSSQLLOCALDB`
- ✅ **Arquivo**: `Web.Release.config`

### 2. Lógica de Criação/Attach Refatorada
- ✅ **Verificação inteligente**: Verifica se banco existe ANTES de tentar criar
- ✅ **Verificação de arquivo**: Verifica se arquivo .mdf existe
- ✅ **Lógica condicional**: 
  - Se banco NÃO existe e arquivo NÃO existe → **CRIAR NOVO**
  - Se banco NÃO existe mas arquivo EXISTE → **FAZER ATTACH**
  - Se banco EXISTE e arquivo NÃO existe → **DETACH e criar arquivo** (caso raro)
  - Se banco EXISTE e arquivo EXISTE → **NÃO FAZER NADA** (já está pronto)

### 3. Garantia de Acesso à Pasta App_Data
- ✅ **Criação automática**: Pasta App_Data é criada se não existir
- ✅ **Verificação de permissões**: Testa escrita na pasta antes de usar
- ✅ **Mensagem de erro clara**: Se não tiver permissão, mostra erro específico

### 4. Métodos Auxiliares Criados
- ✅ `CriarMasterConnectionString()`: Normaliza connection string para master usando `.\MSSQLLOCALDB`
- ✅ `ExtrairNomeBanco()`: Extrai nome do banco da connection string
- ✅ `VerificarBancoExiste()`: Verifica se banco já existe no LocalDB
- ✅ `CriarNovoBanco()`: Cria novo banco no arquivo .mdf
- ✅ `FazerAttachBanco()`: Faz attach do arquivo .mdf existente
- ✅ `FazerDetachBanco()`: Faz detach do banco (caso necessário)

## Connection String de Produção

```xml
Data Source=.\MSSQLLOCALDB;
AttachDbFilename=|DataDirectory|\KingdomConfeitaria_Prod.mdf;
Initial Catalog=KingdomConfeitaria_Prod;
Integrated Security=True;
Connect Timeout=30;
MultipleActiveResultSets=True
```

## Fluxo de Inicialização

1. **Verificar se usa AttachDbFilename**
   - Se SIM → Lógica de produção (arquivo .mdf)
   - Se NÃO → Lógica de desenvolvimento (LocalDB padrão)

2. **Para Produção (AttachDbFilename):**
   - Resolver caminho `|DataDirectory|` → `[Pasta da Aplicação]\App_Data`
   - Criar pasta App_Data se não existir
   - Verificar permissões de escrita
   - Verificar se banco existe no LocalDB
   - Verificar se arquivo .mdf existe
   - Executar ação apropriada (criar/attach/nada)

3. **Criar Tabelas**
   - Conectar ao banco
   - Criar tabelas se não existirem

## Permissões Necessárias

A pasta `App_Data` precisa de permissões de escrita para:
- **IIS_IUSRS**: Leitura e escrita
- **IIS AppPool\[Nome do AppPool]**: Leitura e escrita
- **NETWORK SERVICE**: Leitura e escrita (se aplicável)

## Verificação de Erros

A aplicação agora verifica:
- ✅ Se a pasta App_Data existe
- ✅ Se tem permissão de escrita na pasta
- ✅ Se o banco já existe antes de tentar criar
- ✅ Se o arquivo .mdf existe antes de tentar fazer attach

## Mensagens de Erro

Se houver problema de permissões, a aplicação mostrará:
```
Não foi possível escrever na pasta App_Data. 
Verifique as permissões da pasta: [caminho completo]
Erro: [detalhes do erro]
```

## Notas Importantes

1. **Data Source**: Usa `.\MSSQLLOCALDB` (não `(LocalDB)\MSSQLLocalDB`)
2. **Não tenta criar se já existe**: Verifica antes de criar
3. **Attach inteligente**: Tenta ATTACH normal, se falhar tenta ATTACH_REBUILD_LOG
4. **Permissões**: Verifica permissões antes de usar a pasta

## Troubleshooting

### Erro: "Não foi possível escrever na pasta App_Data"
**Solução**: Dar permissões de escrita na pasta App_Data para o usuário do IIS

### Erro: "Banco já existe"
**Solução**: A aplicação agora verifica antes de criar, então esse erro não deve mais ocorrer

### Erro: "Arquivo .mdf não encontrado"
**Solução**: A aplicação criará automaticamente se o banco não existir

