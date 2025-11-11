# 🚀 Como Iniciar a Aplicação Kingdom Confeitaria

## ⚠️ Erro: ERR_CONNECTION_REFUSED

Este erro significa que o servidor web não está rodando. Siga os passos abaixo para iniciar a aplicação.

## 📝 Métodos para Iniciar a Aplicação

### Método 1: Visual Studio (Recomendado)

1. **Abra o Visual Studio**
   - Abra o Visual Studio 2019 ou superior
   - Vá em `File` → `Open` → `Project/Solution`
   - Navegue até `C:\Desenv\KingdomConfeitaria`
   - Selecione `KingdomConfeitaria.sln`
   - Clique em `Open`

2. **Inicie a Aplicação**
   - Pressione `F5` ou clique no botão verde "▶ IIS Express" na barra de ferramentas
   - Ou vá em `Debug` → `Start Debugging`
   - O navegador abrirá automaticamente em `http://localhost:8080`

### Método 2: Visual Studio Code (com extensões)

1. **Instale as extensões necessárias**
   - C# (Microsoft)
   - C# Dev Kit (Microsoft)

2. **Abra o projeto**
   - Abra o Visual Studio Code
   - Vá em `File` → `Open Folder`
   - Selecione a pasta `C:\Desenv\KingdomConfeitaria`

3. **Inicie a aplicação**
   - Pressione `F5` ou vá em `Run` → `Start Debugging`
   - Selecione `.NET Core` ou `IIS Express` como ambiente

### Método 3: IIS Express Manual (Linha de Comando)

1. **Abra o PowerShell como Administrador**

2. **Navegue até a pasta do projeto**
   ```powershell
   cd C:\Desenv\KingdomConfeitaria
   ```

3. **Inicie o IIS Express**
   ```powershell
   & "C:\Program Files\IIS Express\iisexpress.exe" /path:"C:\Desenv\KingdomConfeitaria" /port:8080
   ```

   Ou se o IIS Express estiver em outro local:
   ```powershell
   & "${env:ProgramFiles}\IIS Express\iisexpress.exe" /path:"C:\Desenv\KingdomConfeitaria" /port:8080
   ```

4. **Acesse no navegador**
   - Abra o navegador e acesse: `http://localhost:8080`

### Método 4: IIS Local (Configuração Avançada)

Se você tem IIS instalado localmente:

1. **Crie um site no IIS**
   - Abra o IIS Manager
   - Clique com botão direito em `Sites` → `Add Website`
   - Nome: `KingdomConfeitaria`
   - Physical path: `C:\Desenv\KingdomConfeitaria`
   - Binding: `http`, porta `8080`
   - Clique em `OK`

2. **Inicie o site**
   - No IIS Manager, selecione o site
   - Clique em `Start` no painel de ações

3. **Acesse no navegador**
   - Abra: `http://localhost:8080`

## 🔍 Verificações Importantes

### 1. Verificar se o IIS Express está instalado

```powershell
Test-Path "C:\Program Files\IIS Express\iisexpress.exe"
```

Se retornar `False`, instale o IIS Express:
- Baixe em: https://www.microsoft.com/en-us/download/details.aspx?id=48264

### 2. Verificar se a porta 8080 está disponível

```powershell
netstat -ano | findstr :8080
```

Se houver algum processo usando a porta, você pode:
- Parar o processo
- Ou alterar a porta no arquivo `.vs/KingdomConfeitaria/config/applicationhost.config`

### 3. Verificar se o SQL Server LocalDB está rodando

```powershell
sqllocaldb info MSSQLLocalDB
```

Se não estiver rodando, inicie:
```powershell
sqllocaldb start MSSQLLocalDB
```

## 🐛 Solução de Problemas

### Problema: "Porta 8080 já está em uso"

**Solução:**
1. Encontre o processo usando a porta:
   ```powershell
   netstat -ano | findstr :8080
   ```
2. Encerre o processo ou altere a porta no projeto

### Problema: "IIS Express não encontrado"

**Solução:**
1. Instale o IIS Express
2. Ou use o Visual Studio que já inclui o IIS Express

### Problema: "Erro ao conectar ao banco de dados"

**Solução:**
1. Verifique se o SQL Server LocalDB está instalado
2. Inicie o LocalDB:
   ```powershell
   sqllocaldb start MSSQLLocalDB
   ```

### Problema: "Página não encontrada (404)"

**Solução:**
1. Verifique se está acessando `http://localhost:8080/Default.aspx`
2. Ou configure `Default.aspx` como página padrão no `web.config`

## 📌 URL da Aplicação

Após iniciar, a aplicação estará disponível em:
- **URL Principal**: `http://localhost:8080`
- **Página Inicial**: `http://localhost:8080/Default.aspx`
- **Admin**: `http://localhost:8080/Admin.aspx`
- **Login**: `http://localhost:8080/Login.aspx`

## ✅ Checklist de Inicialização

- [ ] Visual Studio ou IIS Express instalado
- [ ] Projeto aberto no Visual Studio
- [ ] SQL Server LocalDB instalado e rodando
- [ ] Porta 8080 disponível
- [ ] Aplicação iniciada (F5 ou botão Play)
- [ ] Navegador aberto em `http://localhost:8080`

## 💡 Dica

A forma mais fácil é usar o **Visual Studio**:
1. Abra o arquivo `KingdomConfeitaria.sln`
2. Pressione `F5`
3. Pronto! A aplicação abrirá automaticamente no navegador

