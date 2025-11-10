# 🍪 Kingdom Confeitaria - Sistema de Reservas de Ginger Breads

Sistema completo de reservas online para produção de Ginger Breads artesanais, desenvolvido em ASP.NET Web Forms.

## 📋 Índice

- [Sobre o Projeto](#sobre-o-projeto)
- [Funcionalidades](#funcionalidades)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Instalação e Configuração](#instalação-e-configuração)
- [Como Usar](#como-usar)
- [Configurações Necessárias](#configurações-necessárias)
- [Banco de Dados](#banco-de-dados)
- [Páginas do Sistema](#páginas-do-sistema)
- [Integrações](#integrações)
- [Desenvolvimento](#desenvolvimento)

---

## 🎯 Sobre o Projeto

Sistema desenvolvido para gerenciar reservas de Ginger Breads artesanais produzidos pelas filhas Isabela e Camila. O sistema permite que clientes façam reservas online, escolham produtos, tamanhos e quantidades, e acompanhem o status de suas reservas.

### Características Principais

- ✅ Sistema de reservas completo
- ✅ Carrinho de compras em tempo real
- ✅ Autenticação social (Facebook, Google, WhatsApp)
- ✅ Área do cliente para gerenciar reservas
- ✅ Painel administrativo
- ✅ Notificações por email e WhatsApp
- ✅ Compartilhamento nas redes sociais
- ✅ Design responsivo e moderno

---

## ✨ Funcionalidades

### Para Clientes

- **Reserva de Produtos**
  - Visualização de produtos com imagens
  - Seleção de tamanho (Pequeno/Grande)
  - Seleção de quantidade
  - Produtos promocionais (sacos com 6 pequenos ou 3 grandes)
  - Seleção de biscoitos individuais para sacos promocionais
  - Cálculo automático de preços
  - Seleção de data de retirada (segundas até última segunda antes do Natal)

- **Autenticação e Cadastro**
  - Login com Facebook
  - Login com Google
  - Login com WhatsApp
  - Cadastro por email/telefone
  - Confirmação de email com link
  - Confirmação por WhatsApp (se configurado)

- **Gerenciamento de Reservas**
  - Visualizar todas as reservas
  - Ver detalhes completos de cada reserva
  - Excluir reservas (se não canceladas/entregues)
  - Acompanhar status da reserva
  - Compartilhar reservas nas redes sociais

- **Compartilhamento**
  - Compartilhar no Facebook
  - Compartilhar no WhatsApp
  - Compartilhar no Twitter
  - Compartilhar por Email

### Para Administradores

- **Gerenciamento de Produtos**
  - Adicionar novos produtos
  - Editar produtos existentes
  - Upload/edição de imagens
  - Ativar/desativar produtos
  - Ordenar produtos
  - Gerenciar produtos promocionais

- **Gerenciamento de Reservas**
  - Visualizar todas as reservas
  - Editar status da reserva
  - Marcar como "Convertido em Pedido"
  - Definir previsão de entrega
  - Cancelar reservas
  - Editar observações

---

## 🛠️ Tecnologias Utilizadas

- **Backend**: ASP.NET Web Forms (.NET Framework 4.8)
- **Banco de Dados**: SQL Server Express (LocalDB)
- **Frontend**: HTML5, CSS3, JavaScript, Bootstrap 5
- **Ícones**: Font Awesome 6.4.0
- **Email**: SMTP (configurável)
- **Autenticação**: OAuth 2.0 (Facebook, Google)
- **WhatsApp**: API de terceiros (configurável)

---

## 📁 Estrutura do Projeto

```
KingdomConfeitaria/
├── Default.aspx              # Página principal - Reserva de produtos
├── Login.aspx                # Página de login/cadastro
├── Logout.aspx               # Logout
├── MinhasReservas.aspx       # Área do cliente
├── ConfirmarCadastro.aspx    # Confirmação de email
├── VerReserva.aspx           # Visualização de reserva por token
├── Admin.aspx                # Painel administrativo
├── Models/                   # Modelos de dados
│   ├── Cliente.cs
│   ├── Produto.cs
│   ├── Reserva.cs
│   └── ItemPedido.cs
├── Services/                 # Serviços
│   ├── DatabaseService.cs    # Acesso ao banco de dados
│   ├── EmailService.cs       # Envio de emails
│   └── WhatsAppService.cs    # Envio de mensagens WhatsApp
├── Helpers/                  # Utilitários
│   └── DateHelper.cs         # Cálculo de datas de retirada
├── Images/                   # Imagens
│   └── logo-kingdom-confeitaria.png
├── Scripts/                  # JavaScript
│   └── site.js
└── web.config                # Configurações
```

---

## 🚀 Instalação e Configuração

### Pré-requisitos

- Windows Server com IIS
- .NET Framework 4.8
- SQL Server Express (LocalDB)
- Visual Studio 2019 ou superior (para desenvolvimento)

### Passos de Instalação

1. **Clone ou baixe o projeto**
   ```bash
   git clone [url-do-repositorio]
   cd KingdomConfeitaria
   ```

2. **Configure o banco de dados**
   - O banco de dados é criado automaticamente na primeira execução
   - Certifique-se de que o SQL Server Express está instalado
   - A string de conexão está em `web.config`

3. **Configure as integrações** (veja seção [Configurações Necessárias](#configurações-necessárias))

4. **Compile o projeto**
   ```bash
   msbuild KingdomConfeitaria.csproj /p:Configuration=Release
   ```

5. **Publique no IIS**
   - Configure um site no IIS
   - Aponte para a pasta do projeto
   - Configure o Application Pool para .NET Framework 4.8

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

**Nota**: Para Gmail, use uma "Senha de App" em vez da senha normal.

### 3. BaseUrl

Configure a URL base do site:

```xml
<add key="BaseUrl" value="https://seudominio.com" />
```

### 4. OAuth - Facebook (Opcional)

1. Crie um app no [Facebook Developers](https://developers.facebook.com/)
2. Configure as chaves em `web.config`:

```xml
<add key="FacebookAppId" value="SEU_APP_ID" />
<add key="FacebookAppSecret" value="SEU_APP_SECRET" />
```

3. Atualize o App ID no `Login.aspx` (linha do script do Facebook)

### 5. OAuth - Google (Opcional)

1. Crie um projeto no [Google Cloud Console](https://console.cloud.google.com/)
2. Configure OAuth 2.0
3. Configure as chaves em `web.config`:

```xml
<add key="GoogleClientId" value="SEU_CLIENT_ID" />
<add key="GoogleClientSecret" value="SEU_CLIENT_SECRET" />
```

4. Atualize o Client ID no `Login.aspx` (linha do script do Google)

### 6. WhatsApp API (Opcional)

Configure o provedor de API de WhatsApp (Twilio, Evolution API, etc.):

```xml
<add key="WhatsAppApiUrl" value="https://api.whatsapp.com/send" />
<add key="WhatsAppApiKey" value="SUA_CHAVE" />
<add key="WhatsAppPhoneNumber" value="5511999999999" />
```

**Nota**: Atualmente o serviço está apenas logando. Descomente o código de envio real em `WhatsAppService.cs` quando configurar a API.

---

## 💾 Banco de Dados

### Tabelas Criadas Automaticamente

- **Produtos**: Armazena informações dos produtos
- **Reservas**: Armazena as reservas dos clientes
- **ReservaItens**: Armazena os itens de cada reserva
- **Clientes**: Armazena dados dos clientes e autenticação

### Seed Inicial

O sistema cria automaticamente 14 produtos individuais de Ginger Bread e 2 sacos promocionais na primeira execução.

---

## 📄 Páginas do Sistema

### 1. Default.aspx
Página principal onde os clientes visualizam produtos e fazem reservas.

**Funcionalidades**:
- Listagem de produtos
- Carrinho de compras
- Formulário de reserva
- Seleção de data de retirada

### 2. Login.aspx
Página de login e cadastro.

**Funcionalidades**:
- Login social (Facebook, Google, WhatsApp)
- Cadastro por email/telefone
- Envio de confirmação

### 3. MinhasReservas.aspx
Área do cliente para gerenciar reservas.

**Funcionalidades**:
- Listar todas as reservas
- Ver detalhes
- Excluir reservas
- Compartilhar nas redes sociais

### 4. ConfirmarCadastro.aspx
Confirmação de cadastro via email.

**Funcionalidades**:
- Validação de token
- Confirmação de email
- Login automático

### 5. VerReserva.aspx
Visualização de reserva por link único.

**Funcionalidades**:
- Acesso direto via token
- Visualização completa
- Login automático (se cliente associado)
- Compartilhamento

### 6. Admin.aspx
Painel administrativo.

**Funcionalidades**:
- Gerenciamento de produtos
- Gerenciamento de reservas
- Edição de status
- Controle de entregas

### 7. Logout.aspx
Encerramento de sessão.

---

## 🔗 Integrações

### Email Service
- Envio de confirmação de cadastro
- Envio de confirmação de reserva
- Notificações para administradores

### WhatsApp Service
- Envio de confirmação de cadastro
- Envio de confirmação de reserva
- Links para visualização

### OAuth
- Facebook Login
- Google Login
- Criação automática de conta

---

## 👨‍💻 Desenvolvimento

### Compilar o Projeto

```bash
msbuild KingdomConfeitaria.csproj /p:Configuration=Debug /t:Build
```

### Executar Localmente

1. Abra o projeto no Visual Studio
2. Pressione F5 para executar
3. Acesse `http://localhost:porta/Default.aspx`

### Estrutura de Código

- **Models**: Classes de dados
- **Services**: Lógica de negócio e acesso a dados
- **Helpers**: Funções utilitárias
- **Pages**: Páginas ASP.NET (.aspx)

### Padrões Utilizados

- Separação de responsabilidades
- Injeção de dependências (simples)
- Repository pattern (DatabaseService)
- Service layer (EmailService, WhatsAppService)

---

## 📝 Notas Importantes

1. **Banco de Dados**: O banco é criado automaticamente na primeira execução. Certifique-se de ter permissões adequadas.

2. **Emails**: Configure o SMTP corretamente para envio de emails funcionar.

3. **OAuth**: As integrações sociais precisam ser configuradas com chaves válidas.

4. **WhatsApp**: A API de WhatsApp precisa ser configurada com um provedor real.

5. **BaseUrl**: Configure com a URL real do site em produção para os links funcionarem corretamente.

6. **Segurança**: Em produção, considere:
   - Proteger Admin.aspx com autenticação
   - Usar HTTPS
   - Validar todos os inputs
   - Implementar rate limiting

---

## 🐛 Troubleshooting

### Erro ao criar banco de dados
- Verifique se o SQL Server Express está instalado
- Verifique permissões do usuário
- Verifique a string de conexão

### Emails não são enviados
- Verifique configurações SMTP
- Para Gmail, use "Senha de App"
- Verifique firewall/antivírus

### Login social não funciona
- Verifique se as chaves estão configuradas
- Verifique URLs de callback
- Verifique console do navegador para erros

---

## 📞 Suporte

Para dúvidas ou problemas, consulte:
- `ANALISE_FUNCIONALIDADES.md` - Análise completa das funcionalidades
- `RESUMO_IMPLEMENTACAO.md` - Resumo da implementação
- `TESTES_FUNCIONALIDADES.md` - Guia de testes

---

## 📄 Licença

Este projeto é privado e de uso exclusivo da Kingdom Confeitaria.

---

## 🎉 Agradecimentos

Desenvolvido para Isabela e Camila - Kingdom Confeitaria 🍪👑

---

**Versão**: 1.0  
**Última atualização**: 2024  
**Status**: ✅ Completo e funcional

