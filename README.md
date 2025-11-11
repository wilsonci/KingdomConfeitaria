# 🍪 Kingdom Confeitaria - Sistema de Reservas de Ginger Breads

Sistema completo de reservas online para produção de Ginger Breads artesanais, desenvolvido em ASP.NET Web Forms (.NET Framework 4.8).

## 📋 Índice

- [Sobre o Projeto](#sobre-o-projeto)
- [Funcionalidades](#funcionalidades)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Instalação e Configuração](#instalação-e-configuração)
- [Como Iniciar a Aplicação](#como-iniciar-a-aplicação)
- [Fluxo de Autenticação](#fluxo-de-autenticação)
- [Configurações Necessárias](#configurações-necessárias)
- [Banco de Dados](#banco-de-dados)
- [Páginas do Sistema](#páginas-do-sistema)
- [Sistema de Status de Reservas](#sistema-de-status-de-reservas)
- [Desenvolvimento](#desenvolvimento)
- [Troubleshooting](#troubleshooting)

---

## 🎯 Sobre o Projeto

Sistema desenvolvido para gerenciar reservas de Ginger Breads artesanais produzidos pelas filhas Isabela e Camila. O sistema permite que clientes façam reservas online, escolham produtos, tamanhos e quantidades, e acompanhem o status de suas reservas.

### Características Principais

- ✅ Sistema de reservas completo com carrinho de compras
- ✅ Autenticação por email e senha
- ✅ Login dinâmico (modal reutilizável)
- ✅ Área do cliente para gerenciar reservas
- ✅ Painel administrativo completo
- ✅ Sistema de status de reservas (Aberta, Em Produção, Produção Pronta, etc.)
- ✅ Edição e cancelamento de reservas
- ✅ Recuperação de senha
- ✅ Gerenciamento de dados pessoais
- ✅ Notificações por email
- ✅ Compartilhamento nas redes sociais
- ✅ Design responsivo e moderno com Bootstrap 5

---

## ✨ Funcionalidades

### Para Clientes

#### Reserva de Produtos
- Visualização de produtos com imagens
- Seleção de tamanho (Pequeno/Grande)
- Seleção de quantidade
- Produtos promocionais (sacos com 6 pequenos ou 3 grandes)
- Seleção de biscoitos individuais para sacos promocionais
- Cálculo automático de preços
- Seleção de data de retirada (segundas até última segunda antes do Natal)
- Carrinho de compras em tempo real
- Validação de produtos antes de finalizar reserva

#### Autenticação e Cadastro
- Login por email ou telefone
- Cadastro por email/telefone e senha
- Validação dinâmica de formulários (email, telefone, nome)
- Formatação automática de email e telefone
- Prevenção de duplicação de clientes (verificação por email e telefone)
- Confirmação de email com link
- Recuperação de senha por email
- Login dinâmico (modal reutilizável que pode ser chamado de qualquer página)
- Detecção automática de email ou telefone durante digitação
- Normalização automática de dados (email em minúsculas, apenas números no telefone)

#### Gerenciamento de Reservas
- Visualizar todas as reservas
- Ver detalhes completos de cada reserva
- Editar reservas (quando status permitir)
- Cancelar reservas (quando status permitir)
- Acompanhar status da reserva em tempo real
- Compartilhar reservas nas redes sociais
- Adicionar/remover itens de reservas abertas
- Alterar quantidade de itens

#### Meus Dados
- Visualizar dados cadastrais
- Editar nome, email e telefone
- Alterar senha
- Atualizar informações de contato

#### Compartilhamento
- Compartilhar no Facebook
- Compartilhar no WhatsApp
- Compartilhar no Twitter
- Compartilhar por Email

### Para Administradores

#### Gerenciamento de Produtos
- Adicionar novos produtos
- Editar produtos existentes
- Upload/edição de imagens
- Ativar/desativar produtos
- Ordenar produtos
- Gerenciar produtos promocionais
- Definir preços (pequeno e grande)
- Gerenciar descrições e detalhes

#### Gerenciamento de Reservas
- Visualizar todas as reservas
- Filtrar por status
- Editar status da reserva
- Editar itens da reserva
- Marcar como "Convertido em Pedido"
- Definir previsão de entrega
- Cancelar reservas
- Excluir reservas (quando permitido)
- Editar observações
- Visualizar resumo estatístico (total de reservas, valores, etc.)

#### Gerenciamento de Clientes
- Visualizar todos os clientes cadastrados
- Ver detalhes dos clientes
- Verificar status de confirmação de email
- Excluir clientes (com verificação de dependências)

#### Gerenciamento de Status
- Visualizar todos os status disponíveis
- Editar status existentes
- Criar novos status
- Excluir status (com verificação de dependências)
- Configurar permissões (PermiteAlteracao, PermiteExclusao)

---

## 🛠️ Tecnologias Utilizadas

- **Backend**: ASP.NET Web Forms (.NET Framework 4.8)
- **Banco de Dados**: SQL Server Express (LocalDB)
- **Frontend**: HTML5, CSS3, JavaScript (ES6+), Bootstrap 5
- **Ícones**: Font Awesome 6.4.0
- **Email**: SMTP (Gmail configurado)
- **Autenticação**: Sistema próprio com hash SHA256
- **Sessão**: ASP.NET Session State

---

## 📁 Estrutura do Projeto

```
KingdomConfeitaria/
├── Default.aspx              # Página principal - Reserva de produtos
├── Login.aspx                # Página de login/cadastro
├── Logout.aspx               # Logout
├── MinhasReservas.aspx       # Área do cliente
├── MeusDados.aspx            # Gerenciamento de dados pessoais
├── ConfirmarCadastro.aspx    # Confirmação de email
├── RecuperarSenha.aspx       # Solicitação de recuperação de senha
├── RedefinirSenha.aspx       # Redefinição de senha
├── VerReserva.aspx           # Visualização de reserva por token
├── Admin.aspx                # Painel administrativo
├── Models/                   # Modelos de dados
│   ├── Cliente.cs
│   ├── Produto.cs
│   ├── Reserva.cs
│   ├── ItemPedido.cs
│   └── StatusReserva.cs
├── Services/                 # Serviços
│   ├── DatabaseService.cs    # Acesso ao banco de dados
│   ├── EmailService.cs       # Envio de emails
│   └── WhatsAppService.cs    # Envio de mensagens WhatsApp (opcional)
├── Helpers/                  # Utilitários
│   └── DateHelper.cs         # Cálculo de datas de retirada
├── Scripts/                  # JavaScript
│   ├── site.js              # Scripts globais
│   ├── default.js           # Scripts da página principal
│   ├── login.js             # Scripts de login
│   ├── minhasreservas.js    # Scripts de reservas
│   └── admin.js             # Scripts do painel admin
├── Images/                   # Imagens
│   └── logo-kingdom-confeitaria.png
└── web.config                # Configurações
```

---

## 🚀 Instalação e Configuração

### Pré-requisitos

- Windows 10/11 ou Windows Server
- .NET Framework 4.8
- SQL Server Express (LocalDB) - geralmente já vem com Visual Studio
- Visual Studio 2019 ou superior (para desenvolvimento)
- IIS Express (vem com Visual Studio) ou IIS (para produção)

### Passos de Instalação

1. **Clone ou baixe o projeto**
   ```bash
   git clone [url-do-repositorio]
   cd KingdomConfeitaria
   ```

2. **Abra o projeto no Visual Studio**
   - Abra o arquivo `KingdomConfeitaria.sln`
   - Aguarde a restauração dos pacotes NuGet

3. **Configure o banco de dados**
   - O banco de dados é criado automaticamente na primeira execução
   - Certifique-se de que o SQL Server Express (LocalDB) está instalado
   - A string de conexão está em `web.config`

4. **Configure as integrações** (veja seção [Configurações Necessárias](#configurações-necessárias))

5. **Compile o projeto**
   - Pressione `F5` no Visual Studio
   - Ou compile via linha de comando:
   ```bash
   msbuild KingdomConfeitaria.csproj /p:Configuration=Release
   ```

---

## 🖥️ Como Iniciar a Aplicação

### Método 1: Visual Studio (Recomendado)

1. Abra o Visual Studio
2. Abra o arquivo `KingdomConfeitaria.sln`
3. Pressione `F5` ou clique no botão verde "▶ IIS Express"
4. O navegador abrirá automaticamente em `http://localhost:8080`

### Método 2: IIS Express Manual (Linha de Comando)

```powershell
cd C:\Desenv\KingdomConfeitaria
& "C:\Program Files\IIS Express\iisexpress.exe" /path:"C:\Desenv\KingdomConfeitaria" /port:8080
```

### Método 3: IIS Local (Produção)

1. Abra o IIS Manager
2. Clique com botão direito em `Sites` → `Add Website`
3. Configure:
   - Nome: `KingdomConfeitaria`
   - Physical path: `C:\Desenv\KingdomConfeitaria`
   - Binding: `http`, porta `8080`
4. Configure o Application Pool para .NET Framework 4.8
5. Inicie o site

**Para mais detalhes, consulte o arquivo `COMO_INICIAR_APLICACAO.md`**

---

## 🔐 Fluxo de Autenticação

### Login Dinâmico

O sistema possui um modal de login reutilizável que pode ser chamado de qualquer página:

1. **Campo de Login Único**: O cliente digita email ou telefone
2. **Detecção Automática**: O sistema identifica se é email ou telefone
3. **Normalização**: Email convertido para minúsculas, telefone apenas números
4. **Busca no Banco**: Sistema busca o cliente no banco de dados
5. **Solicitação de Senha**: Se encontrado, solicita a senha
6. **Cadastro**: Se não encontrado, oferece opção de cadastro
7. **Preenchimento Automático**: Após login, os dados são preenchidos automaticamente

### Cadastro

1. Cliente preenche nome, email/telefone e senha
2. Sistema valida os dados em tempo real
3. Se email ou telefone já existirem, o sistema informa
4. Caso contrário, cria uma nova conta e envia confirmação por email
5. Cliente confirma o email através do link recebido

### Recuperação de Senha

1. Cliente acessa "Recuperar Senha"
2. Informa email ou telefone
3. Sistema envia email com link de redefinição
4. Cliente acessa o link e define nova senha
5. Link expira após 24 horas

### Validação de Dados

- **Email**: Validado em tempo real, deve conter @ e ponto (.)
- **Telefone**: Máscara automática, deve ter 10 ou 11 dígitos
- **Nome**: Mínimo de 3 caracteres
- Todos os dados são formatados automaticamente antes de salvar

---

## ⚙️ Configurações Necessárias

### 1. Banco de Dados

O banco de dados é criado automaticamente. A string de conexão está em `web.config`:

```xml
<connectionStrings>
  <add name="KingdomConfeitariaDB" 
       connectionString="Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=KingdomConfeitaria;Integrated Security=True;Connect Timeout=30;MultipleActiveResultSets=True" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

### 2. Email (SMTP)

Configure o envio de emails em `web.config`:

```xml
<appSettings>
  <add key="SmtpServer" value="smtp.gmail.com" />
  <add key="SmtpPort" value="587" />
  <add key="SmtpUsername" value="seu-email@gmail.com" />
  <add key="SmtpPassword" value="sua-senha-app" />
  <add key="EmailIsabela" value="isabela@email.com" />
  <add key="EmailCamila" value="camila@email.com" />
  <add key="EmailFrom" value="reservas@kingdomconfeitaria.com" />
</appSettings>
```

**Nota**: Para Gmail, use uma "Senha de App" em vez da senha normal:
1. Acesse: https://myaccount.google.com/apppasswords
2. Selecione "Aplicativo" e "Outro (nome personalizado)"
3. Digite "Kingdom Confeitaria" e clique em "Gerar"
4. Copie a senha gerada (16 caracteres) e use em `SmtpPassword`

### 3. BaseUrl

Configure a URL base do site:

```xml
<add key="BaseUrl" value="https://seudominio.com" />
```

Para desenvolvimento local:
```xml
<add key="BaseUrl" value="http://localhost:8080" />
```

### 4. Administradores

Os administradores são definidos por email na tabela `Clientes`. Os emails padrão são:
- `wilson2071@gmail.com`
- `isanfm@gmail.com`
- `camilafermagalhaes@gmail.com`

O sistema automaticamente define `IsAdmin = true` para esses emails.

---

## 💾 Banco de Dados

### Tabelas Criadas Automaticamente

- **Produtos**: Armazena informações dos produtos
  - Id, Nome, Descricao, PrecoPequeno, PrecoGrande, ImagemUrl, Ativo, Ordem, EhSacoPromocional, QuantidadeSaco, TamanhoSaco

- **Clientes**: Armazena dados dos clientes e autenticação
  - Id, Nome, Email, Senha (hash SHA256), Telefone, TemWhatsApp, EmailConfirmado, IsAdmin, DataCadastro, UltimoAcesso, TokenConfirmacao, TokenRecuperacaoSenha, DataExpiracaoRecuperacaoSenha

- **Reservas**: Armazena as reservas dos clientes
  - Id, ClienteId (FK), DataRetirada, DataReserva, StatusId (FK), ValorTotal, Observacoes, ConvertidoEmPedido, PrevisaoEntrega, Cancelado, TokenAcesso

- **ReservaItens**: Armazena os itens de cada reserva
  - Id, ReservaId (FK), ProdutoId (FK), NomeProduto, Tamanho, Quantidade, PrecoUnitario, Subtotal

- **StatusReserva**: Armazena os status disponíveis para reservas
  - Id, Nome, Descricao, PermiteAlteracao, PermiteExclusao, Ordem

### Seed Inicial

O sistema cria automaticamente na primeira execução:

- **14 produtos individuais** de Ginger Bread (Estrela, Árvore, Boneco, Coração, Floco de Neve, Guirlanda, Meia)
- **2 sacos promocionais** (6 pequenos e 3 grandes)
- **7 status de reserva**:
  1. Aberta - Permite alterações e cancelamento
  2. Em Produção - Não permite alteração nem exclusão
  3. Produção Pronta - Já foi produzido
  4. Preparando Entrega - Sendo preparado para entrega
  5. Saiu para Entrega - Já está entregando
  6. Já Entregue - Produtos já entregues
  7. Cancelado - Reserva cancelada

### Migrações Automáticas

O sistema realiza migrações automáticas do banco de dados:
- Criação de novas colunas quando necessário
- Migração de dados antigos para novos formatos
- Criação de índices e foreign keys
- Normalização de dados (remoção de colunas redundantes)

---

## 📄 Páginas do Sistema

### 1. Default.aspx - Página Principal

Página principal onde os clientes visualizam produtos e fazem reservas.

**Funcionalidades**:
- Listagem de produtos com imagens
- Carrinho de compras em tempo real
- Modal de login dinâmico
- Formulário de reserva com validação
- Seleção de data de retirada
- Validação de campos obrigatórios
- Máscara de telefone automática
- Feedback visual de validação
- Placeholder para imagens não encontradas
- Botão "Fazer Reserva" habilitado apenas com itens no carrinho
- Continuidade de reserva após login

### 2. Login.aspx - Login e Cadastro

Página de login e cadastro com modal dinâmico.

**Funcionalidades**:
- Login por email ou telefone
- Cadastro por email/telefone e senha
- Validação dinâmica de campos
- Máscara de telefone automática
- Feedback visual de validação
- Prevenção de cadastros duplicados
- Redirecionamento para "Minhas Reservas" após login

### 3. MinhasReservas.aspx - Área do Cliente

Área do cliente para gerenciar reservas.

**Funcionalidades**:
- Listar todas as reservas
- Ver detalhes completos
- Cancelar reservas (quando permitido)
- Compartilhar nas redes sociais
- Link para voltar à página de reservas
- Badges de status coloridos
- Filtros e ordenação

### 4. MeusDados.aspx - Gerenciamento de Dados

Página para o cliente gerenciar seus dados pessoais.

**Funcionalidades**:
- Visualizar dados cadastrais
- Editar nome, email e telefone
- Alterar senha
- Validação de dados
- Confirmação de alterações

### 5. VerReserva.aspx - Visualização de Reserva

Visualização de reserva por link único (token).

**Funcionalidades**:
- Acesso direto via token
- Visualização completa da reserva
- Edição de reserva (quando status permitir)
- Adicionar/remover itens (quando status permitir)
- Alterar quantidade de itens
- Login automático (se cliente associado)
- Compartilhamento

### 6. RecuperarSenha.aspx - Recuperação de Senha

Solicitação de recuperação de senha.

**Funcionalidades**:
- Solicitar recuperação por email ou telefone
- Envio de email com link de redefinição
- Validação de dados

### 7. RedefinirSenha.aspx - Redefinição de Senha

Redefinição de senha através do link recebido por email.

**Funcionalidades**:
- Validação de token
- Definição de nova senha
- Confirmação de senha
- Expiração de token (24 horas)

### 8. ConfirmarCadastro.aspx - Confirmação de Email

Confirmação de cadastro via email.

**Funcionalidades**:
- Validação de token
- Confirmação de email
- Login automático após confirmação

### 9. Admin.aspx - Painel Administrativo

Painel administrativo completo.

**Funcionalidades**:
- Dashboard com resumo estatístico
- Gerenciamento de produtos (CRUD completo)
- Gerenciamento de reservas (visualizar, editar, cancelar, excluir)
- Gerenciamento de clientes
- Gerenciamento de status de reservas
- Filtros e buscas
- Exportação de dados (futuro)

### 10. Logout.aspx - Encerramento de Sessão

Encerramento de sessão do usuário.

---

## 📊 Sistema de Status de Reservas

O sistema possui um sistema robusto de status de reservas:

### Status Disponíveis

1. **Aberta** (ID: 1)
   - Descrição: Reserva dentro do período que permite alterações e cancelamento
   - Permite Alteração: Sim
   - Permite Exclusão: Sim
   - Ordem: 1

2. **Em Produção** (ID: 2)
   - Descrição: Já está sendo produzida os produtos da reserva, não permite alteração nem exclusão
   - Permite Alteração: Não
   - Permite Exclusão: Não
   - Ordem: 2

3. **Produção Pronta** (ID: 3)
   - Descrição: Já foi produzido
   - Permite Alteração: Não
   - Permite Exclusão: Não
   - Ordem: 3

4. **Preparando Entrega** (ID: 4)
   - Descrição: Já está sendo preparado para entrega
   - Permite Alteração: Não
   - Permite Exclusão: Não
   - Ordem: 4

5. **Saiu para Entrega** (ID: 5)
   - Descrição: Já está entregando
   - Permite Alteração: Não
   - Permite Exclusão: Não
   - Ordem: 5

6. **Já Entregue** (ID: 6)
   - Descrição: Produtos já entregues
   - Permite Alteração: Não
   - Permite Exclusão: Não
   - Ordem: 6

7. **Cancelado** (ID: 7)
   - Descrição: Reserva cancelada
   - Permite Alteração: Não
   - Permite Exclusão: Não
   - Ordem: 7

### Regras de Negócio

- **Nova Reserva**: Sempre criada com status "Aberta" (ID: 1)
- **Cancelamento**: Altera o status para "Cancelado" (ID: 7)
- **Edição**: Apenas permitida quando `PermiteAlteracao = true`
- **Exclusão**: Apenas permitida quando `PermiteExclusao = true`
- **Cliente**: Pode cancelar reservas com status "Aberta"
- **Administrador**: Pode excluir reservas com status "Aberta"

---

## 👨‍💻 Desenvolvimento

### Compilar o Projeto

```bash
msbuild KingdomConfeitaria.csproj /p:Configuration=Debug /t:Build
```

### Executar Localmente

1. Abra o projeto no Visual Studio
2. Pressione `F5` para executar
3. Acesse `http://localhost:8080/Default.aspx`

### Estrutura de Código

- **Models**: Classes de dados (Cliente, Produto, Reserva, ItemPedido, StatusReserva)
- **Services**: Lógica de negócio e acesso a dados
  - `DatabaseService`: Acesso ao banco de dados, criação de tabelas, migrações
  - `EmailService`: Envio de emails SMTP
  - `WhatsAppService`: Envio de mensagens WhatsApp (opcional)
- **Helpers**: Funções utilitárias (DateHelper)
- **Pages**: Páginas ASP.NET (.aspx) com code-behind (.aspx.cs)
- **Scripts**: JavaScript organizado por página

### Padrões Utilizados

- Separação de responsabilidades
- Repository pattern (DatabaseService)
- Service layer (EmailService, WhatsAppService)
- Validação client-side e server-side
- Normalização de dados
- UTF-8 encoding em toda aplicação
- Scripts externos (não usa ScriptManager)

### Convenções de Código

- **JavaScript**: Arquivos externos, não inline (exceto quando necessário)
- **CSS**: Classes Bootstrap 5, estilos customizados quando necessário
- **C#**: Padrões do .NET Framework, async/await quando aplicável
- **SQL**: Migrações automáticas, validação de dependências antes de exclusão

---

## 🐛 Troubleshooting

### Erro ao criar banco de dados

**Sintomas**: Erro "Erro ao acessar o banco de dados"

**Soluções**:
- Verifique se o SQL Server Express (LocalDB) está instalado
- Inicie o LocalDB:
  ```powershell
  sqllocaldb start MSSQLLocalDB
  ```
- Verifique permissões do usuário
- Verifique a string de conexão em `web.config`

### Emails não são enviados

**Sintomas**: Emails não chegam aos destinatários

**Soluções**:
- Verifique configurações SMTP em `web.config`
- Para Gmail, use "Senha de App" (não a senha normal)
- Verifique firewall/antivírus
- Verifique logs do servidor

### Porta 8080 já está em uso

**Sintomas**: Erro ao iniciar IIS Express

**Soluções**:
- Encontre o processo usando a porta:
  ```powershell
  netstat -ano | findstr :8080
  ```
- Encerre o processo ou altere a porta no projeto
- Altere a porta no Visual Studio: Properties → Web → Project Url

### Página não encontrada (404)

**Sintomas**: Erro 404 ao acessar páginas

**Soluções**:
- Verifique se está acessando `http://localhost:8080/Default.aspx`
- Verifique se o IIS Express está rodando
- Verifique se o arquivo existe no projeto

### Caracteres estranhos na interface

**Sintomas**: Caracteres como "" aparecem na interface

**Soluções**:
- Verifique se o arquivo está salvo em UTF-8
- Verifique se `web.config` tem `globalization` configurado:
  ```xml
  <globalization requestEncoding="utf-8" responseEncoding="utf-8" />
  ```
- Verifique se as páginas têm `<meta charset="utf-8">`

### Reserva não está sendo gravada

**Sintomas**: Mensagem de sucesso mas reserva não aparece

**Soluções**:
- Verifique logs do Debug (Output do Visual Studio)
- Verifique se há itens válidos no carrinho
- Verifique se o cliente está logado
- Verifique se os produtos ainda existem no banco

---

## 📝 Notas Importantes

1. **Banco de Dados**: O banco é criado automaticamente na primeira execução. Certifique-se de ter permissões adequadas.

2. **Emails**: Configure o SMTP corretamente para envio de emails funcionar. Para Gmail, use "Senha de App".

3. **BaseUrl**: Configure com a URL real do site em produção para os links funcionarem corretamente.

4. **Validação de Dados**: 
   - Email e telefone são formatados automaticamente antes de salvar
   - O sistema previne cadastros duplicados verificando email e telefone
   - Validação dinâmica em tempo real nos formulários

5. **Imagens de Produtos**: 
   - Coloque as imagens na pasta `Images/` seguindo os nomes especificados no banco
   - Se uma imagem não for encontrada, o sistema usa um placeholder automático

6. **Segurança**: 
   - Senhas são armazenadas com hash SHA256
   - Tokens de confirmação e recuperação têm expiração
   - Validação server-side de todos os dados
   - Proteção contra SQL Injection (usando parâmetros)

7. **Sessão**: 
   - Sessão mantida enquanto o navegador estiver aberto
   - Timeout de 30 minutos
   - Regeneração de ID de sessão expirada

8. **Administradores**: 
   - Definidos por email na tabela `Clientes`
   - Acesso automático ao painel administrativo
   - Podem excluir dados (com verificação de dependências)

---

## 📞 Suporte

Para dúvidas ou problemas, consulte:
- `COMO_INICIAR_APLICACAO.md` - Guia detalhado de inicialização
- `RESUMO_IMPLEMENTACAO.md` - Resumo da implementação
- `TESTES_FUNCIONALIDADES.md` - Guia de testes

---

## 📄 Licença

Este projeto é privado e de uso exclusivo da Kingdom Confeitaria.

---

## 🎉 Agradecimentos

Desenvolvido para Isabela e Camila - Kingdom Confeitaria 🍪👑

---

**Versão**: 2.0  
**Última atualização**: Dezembro 2024  
**Status**: ✅ Completo e funcional

### Changelog v2.0

- ✅ Sistema de status de reservas (StatusReserva)
- ✅ Login dinâmico (modal reutilizável)
- ✅ Recuperação de senha
- ✅ Gerenciamento de dados pessoais (Meus Dados)
- ✅ Edição de reservas (quando status permitir)
- ✅ Cancelamento de reservas
- ✅ Normalização de banco de dados (remoção de colunas redundantes)
- ✅ Migrações automáticas de banco de dados
- ✅ Verificação de dependências antes de exclusão
- ✅ Scripts organizados em arquivos externos
- ✅ UTF-8 encoding em toda aplicação
- ✅ Melhorias na validação e feedback visual
- ✅ Correções de bugs e melhorias de UX
