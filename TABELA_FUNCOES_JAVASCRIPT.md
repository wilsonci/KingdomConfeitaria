# Tabela de Funções JavaScript - Kingdom Confeitaria

## Data: 2024
## Descrição: Relação completa de todas as funções JavaScript da aplicação

---

## 📋 ÍNDICE

1. [Funções Globais (app.js)](#1-funções-globais-appjs)
2. [Funções AJAX (ajax-helper.js)](#2-funções-ajax-ajax-helperjs)
3. [Funções Default.aspx (default.js + inline)](#3-funções-defaultaspx-defaultjs--inline)
4. [Funções Admin.aspx (admin.js + inline)](#4-funções-adminaspx-adminjs--inline)
5. [Funções MinhasReservas.aspx (minhasreservas.js + inline)](#5-funções-minhasreservasaspx-minhasreservasjs--inline)
6. [Funções VerReserva.aspx (inline)](#6-funções-verreservaaspx-inline)

---

## 1. FUNÇÕES GLOBAIS (app.js)

| Função | Arquivo | Descrição | Onde é Usada |
|--------|---------|-----------|--------------|
| `KingdomConfeitaria.Utils.escapeJs(str)` | `Scripts/app.js` | Escapa strings para uso seguro em JavaScript | Funções que precisam escapar strings |
| `KingdomConfeitaria.Utils.ready(fn)` | `Scripts/app.js` | Executa função quando DOM estiver pronto | Inicialização de páginas |
| `KingdomConfeitaria.Utils.verificarSessao()` | `Scripts/app.js` | Verifica se sessão ainda está ativa | Validação de sessão (não implementado) |
| `KingdomConfeitaria.Utils.postBack(target, argument)` | `Scripts/app.js` | Faz postback do ASP.NET sem ScriptManager | Todas as páginas que precisam fazer postback |
| `KingdomConfeitaria.Modal.show(modalId)` | `Scripts/app.js` | Abre um modal Bootstrap | Todas as páginas com modais |
| `KingdomConfeitaria.Modal.hide(modalId)` | `Scripts/app.js` | Fecha um modal Bootstrap | Todas as páginas com modais |
| `KingdomConfeitaria.Modal.initCloseButtons(modalId)` | `Scripts/app.js` | Inicializa botões de fechar de um modal | Inicialização de modais |
| `abrirModalLogin()` | `Scripts/app.js` | Redireciona para Login.aspx | Links de login em várias páginas |

---

## 2. FUNÇÕES AJAX (ajax-helper.js)

| Função | Arquivo | Descrição | Onde é Usada |
|--------|---------|-----------|--------------|
| `KingdomConfeitaria.Ajax.callPageMethod(pagePath, methodName, parameters, onSuccess, onError)` | `Scripts/ajax-helper.js` | Chama Page Method sem ScriptManager | Migração de PageMethods (não usado atualmente) |
| `KingdomConfeitaria.Ajax.callWebService(servicePath, methodName, parameters, onSuccess, onError)` | `Scripts/ajax-helper.js` | Chama Web Service ASMX sem ScriptManager | Migração de Web Services (não usado atualmente) |
| `KingdomConfeitaria.Ajax.callHandler(handlerPath, parameters, method, onSuccess, onError)` | `Scripts/ajax-helper.js` | Chama Generic Handler (.ashx) sem ScriptManager | Default.aspx, VerReserva.aspx |
| `KingdomConfeitaria.Ajax._makeRequest(url, method, data, headers, onSuccess, onError)` | `Scripts/ajax-helper.js` | Função interna para fazer requisição HTTP | Usada internamente por callPageMethod, callWebService, callHandler |
| `KingdomConfeitaria.Ajax._makeFetchRequest(...)` | `Scripts/ajax-helper.js` | Faz requisição usando Fetch API | Usada internamente por _makeRequest |
| `KingdomConfeitaria.Ajax._makeXHRRequest(...)` | `Scripts/ajax-helper.js` | Faz requisição usando XMLHttpRequest (fallback) | Usada internamente por _makeRequest |
| `KingdomConfeitaria.Ajax._objectToQueryString(obj)` | `Scripts/ajax-helper.js` | Converte objeto para query string | Usada internamente por callHandler |
| `KingdomConfeitaria.Ajax.executeJavaScript(javascript)` | `Scripts/ajax-helper.js` | Executa JavaScript retornado do servidor | Respostas do Handler que retornam JavaScript |
| `createPageMethod(pagePath, methodName)` | `Scripts/ajax-helper.js` | Cria método PageMethod dinamicamente | Migração de PageMethods (não usado atualmente) |
| `initPageMethods(pagePath, methodNames)` | `Scripts/ajax-helper.js` | Inicializa PageMethods para uma página | Migração de PageMethods (não usado atualmente) |

---

## 3. FUNÇÕES DEFAULT.ASPX (default.js + inline)

### 3.1. Namespace DefaultPage.Carrinho

| Função | Arquivo | Descrição | Onde é Usada |
|--------|---------|-----------|--------------|
| `DefaultPage.Carrinho.obterPrecoDoProduto(produtoId)` | `Scripts/default.js` | Obtém preço do produto do data attribute do card | Usada internamente por adicionar() |
| `DefaultPage.Carrinho.adicionar(produtoId, nome, tamanho, quantidade, precoFornecido)` | `Scripts/default.js` | Adiciona produto ao carrinho via postback | Botões "Reservar" nos cards de produtos |
| `DefaultPage.Carrinho.atualizarQuantidade(produtoId, tamanho, incremento)` | `Scripts/default.js` | Atualiza quantidade de item no carrinho | Botões +/- no carrinho |
| `DefaultPage.Carrinho.remover(produtoId, tamanho)` | `Scripts/default.js` | Remove item do carrinho | Botão "Remover" no carrinho |

### 3.2. Namespace DefaultPage.Tamanho

| Função | Arquivo | Descrição | Onde é Usada |
|--------|---------|-----------|--------------|
| `DefaultPage.Tamanho.selecionar(btn, produtoId, tamanho, preco)` | `Scripts/default.js` | Seleciona tamanho de um produto | Botões de tamanho no modal de produto |
| `DefaultPage.Tamanho.aumentarQuantidade(produtoId)` | `Scripts/default.js` | Aumenta quantidade no modal de produto | Botão + no modal de produto |
| `DefaultPage.Tamanho.diminuirQuantidade(produtoId)` | `Scripts/default.js` | Diminui quantidade no modal de produto | Botão - no modal de produto |

### 3.3. Namespace DefaultPage.ModalReserva

| Função | Arquivo | Descrição | Onde é Usada |
|--------|---------|-----------|--------------|
| `DefaultPage.ModalReserva.abrir()` | `Scripts/default.js` | Abre modal de reserva e configura visibilidade | Botão "Fazer Reserva" |
| `DefaultPage.ModalReserva.fechar()` | `Scripts/default.js` | Fecha modal de reserva | Botões de fechar do modal |
| `DefaultPage.ModalReserva.validarFormulario()` | `Scripts/default.js` | Valida formulário de reserva | Submit do formulário de reserva |

### 3.4. Namespace DefaultPage.Validacao

| Função | Arquivo | Descrição | Onde é Usada |
|--------|---------|-----------|--------------|
| `DefaultPage.Validacao.validarTelefone(input)` | `Scripts/default.js` | Valida campo de telefone | Eventos blur/change do campo telefone |
| `DefaultPage.Validacao.validarNome(input)` | `Scripts/default.js` | Valida campo de nome | Eventos blur/change do campo nome |
| `DefaultPage.Validacao.validarEmail(input)` | `Scripts/default.js` | Valida campo de email | Eventos blur/change do campo email |

### 3.5. Namespace DefaultPage.Imagens

| Função | Arquivo | Descrição | Onde é Usada |
|--------|---------|-----------|--------------|
| `DefaultPage.Imagens.carregarSilenciosamente()` | `Scripts/default.js` | Carrega imagens de produtos silenciosamente | Inicialização da página |

### 3.6. Funções Globais (Compatibilidade)

| Função | Arquivo | Descrição | Onde é Usada |
|--------|---------|-----------|--------------|
| `obterPrecoDoProduto(produtoId)` | `Scripts/default.js` | Wrapper para DefaultPage.Carrinho.obterPrecoDoProduto | Código inline que precisa obter preço |
| `adicionarAoCarrinho(produtoId, nome, tamanho, quantidade, preco)` | `Scripts/default.js` | Wrapper para DefaultPage.Carrinho.adicionar | Botões "Adicionar ao Carrinho" |
| `atualizarQuantidade(produtoId, tamanho, incremento)` | `Scripts/default.js` | Wrapper para DefaultPage.Carrinho.atualizarQuantidade | Botões +/- no carrinho |
| `removerItem(produtoId, tamanho)` | `Scripts/default.js` | Wrapper para DefaultPage.Carrinho.remover | Botão "Remover" no carrinho |
| `selecionarTamanho(btn, produtoId, tamanho, preco)` | `Scripts/default.js` | Wrapper para DefaultPage.Tamanho.selecionar | Botões de tamanho |
| `aumentarQuantidade(produtoId)` | `Scripts/default.js` | Wrapper para DefaultPage.Tamanho.aumentarQuantidade | Botão + no modal |
| `diminuirQuantidade(produtoId)` | `Scripts/default.js` | Wrapper para DefaultPage.Tamanho.diminuirQuantidade | Botão - no modal |
| `atualizarPreco(produtoId)` | `Scripts/default.js` | Atualiza preço exibido do produto (compatibilidade) | Não usado (mantido para compatibilidade) |
| `atualizarTotalSelecionado(sacoId)` | `Scripts/default.js` | Atualiza total de produtos selecionados no saco | Seletores de produtos no modal de saco |
| `adicionarSacoAoCarrinho(sacoId, nomeSaco, quantidadeMaxima)` | `Scripts/default.js` | Adiciona saco promocional ao carrinho | Botão "Adicionar" no modal de saco |
| `validarCampoTelefone(input)` | `Scripts/default.js` | Wrapper para DefaultPage.Validacao.validarTelefone | Eventos blur/change do campo telefone |
| `validarCampoNome(input)` | `Scripts/default.js` | Wrapper para DefaultPage.Validacao.validarNome | Eventos blur/change do campo nome |
| `validarCampoEmail(input)` | `Scripts/default.js` | Wrapper para DefaultPage.Validacao.validarEmail | Eventos blur/change do campo email |
| `validarFormularioReserva()` | `Scripts/default.js` | Wrapper para DefaultPage.ModalReserva.validarFormulario | Submit do formulário de reserva |
| `fecharModalReserva()` | `Scripts/default.js` | Wrapper para DefaultPage.ModalReserva.fechar | Botões de fechar do modal |

### 3.7. Funções Inline (Default.aspx)

| Função | Arquivo | Descrição | Onde é Usada |
|--------|---------|-----------|--------------|
| `__doPostBack(eventTarget, eventArgument)` | `Default.aspx` | Função para fazer postback do ASP.NET | Todas as funções que fazem postback |
| `escapeJs(str)` | `Default.aspx` | Escapa strings para JavaScript (inline) | Definição de ClientIDs |
| `initLoginDinamico()` | `Default.aspx` | Inicializa sistema de login dinâmico | Inicialização da página |
| `initLoginDinamicoReady()` | `Default.aspx` | Inicializa login dinâmico quando DOM estiver pronto | Inicialização da página |
| `mostrarMensagem(mensagem, tipo)` | `Default.aspx` | Mostra mensagem no modal de login | Callbacks de verificação de cliente |
| `ocultarMensagem()` | `Default.aspx` | Oculta mensagem no modal de login | Timeouts e callbacks |
| `verificarClienteDinamico()` | `Default.aspx` | Verifica se cliente está cadastrado via Handler | Evento keypress do campo de login |
| `mostrarCampoSenha()` | `Default.aspx` | Mostra campo de senha quando cliente tem senha | Callback de verificação de cliente |
| `preencherDadosCliente(cliente)` | `Default.aspx` | Preenche dados do cliente nos campos | Após login bem-sucedido |
| `filtrarEntradaLogin(input)` | `Default.aspx` | Filtra entrada do campo de login (apenas email/telefone) | Evento input do campo de login |
| `abrirModalLogin()` | `Default.aspx` | Abre modal de login standalone | Links de login |
| `fecharModalLogin()` | `Default.aspx` | Fecha modal de login standalone | Botões de fechar |
| `initLoginStandalone()` | `Default.aspx` | Inicializa modal de login standalone | Inicialização do modal standalone |
| `mostrarMensagemStandalone(mensagem, tipo)` | `Default.aspx` | Mostra mensagem no modal standalone | Callbacks de verificação |
| `ocultarMensagemStandalone()` | `Default.aspx` | Oculta mensagem no modal standalone | Timeouts |
| `verificarClienteStandalone(login)` | `Default.aspx` | Verifica cliente no modal standalone | Botão "Verificar" |
| `fazerLoginStandalone(cliente)` | `Default.aspx` | Faz login no modal standalone | Após verificação de senha |
| `validarSenhaStandalone()` | `Default.aspx` | Valida senha no modal standalone | Botão "Entrar" |
| `abrirModalProdutoFromCard(cardElement)` | `Default.aspx` | Abre modal de produto a partir do card | Click na imagem do produto |
| `abrirModalProduto(produtoJson)` | `Default.aspx` | Abre modal de produto com dados JSON | Chamada por abrirModalProdutoFromCard |
| `escapeHtml(text)` | `Default.aspx` | Escapa HTML para exibição segura | Renderização de dados do produto |
| `escapeAttr(text)` | `Default.aspx` | Escapa atributos HTML | Renderização de atributos |
| `fecharModalProduto()` | `Default.aspx` | Fecha modal de produto | Botões de fechar |
| `aumentarQuantidadeModal()` | `Default.aspx` | Aumenta quantidade no modal de produto | Botão + no modal |
| `diminuirQuantidadeModal()` | `Default.aspx` | Diminui quantidade no modal de produto | Botão - no modal |
| `atualizarQuantidadeModal()` | `Default.aspx` | Atualiza exibição da quantidade no modal | Após aumentar/diminuir |
| `adicionarProdutoAoCarrinho()` | `Default.aspx` | Adiciona produto ao carrinho do modal | Botão "Adicionar ao Carrinho" |
| `adicionarSacoAoCarrinhoDoModal()` | `Default.aspx` | Adiciona saco promocional ao carrinho do modal | Botão "Adicionar" no modal de saco |
| `abrirModalCarrinho()` | `Default.aspx` | Abre modal do carrinho (mobile) ou rola até ele (desktop) | Click no ícone do carrinho |
| `ajustarEspacamentoHeader()` | `Default.aspx` | Ajusta espaçamento do header dinamicamente | Resize da janela |
| `ajustarEspacamentoHeaderOnce()` | `Default.aspx` | Ajusta espaçamento do header uma vez | Inicialização |
| `adicionarIconeBotaoReserva()` | `Default.aspx` | Adiciona ícone ao botão de reserva flutuante | Inicialização |
| `adicionarIconeBotaoReservaOnce()` | `Default.aspx` | Adiciona ícone ao botão de reserva uma vez | Inicialização |
| `iniciarAnimacaoMao()` | `Default.aspx` | Inicia animação da mãozinha clicando | Inicialização |
| `iniciarAnimacaoMaoOnce()` | `Default.aspx` | Inicia animação da mãozinha uma vez | Inicialização |
| `atualizarDataRetirada(valor)` | `Default.aspx` | Atualiza data de retirada selecionada | Change do dropdown de datas |
| `inicializarNavegacaoCarrossel()` | `Default.aspx` | Inicializa navegação do carrossel de produtos | Inicialização |
| `atualizarBotoesNavegacao()` | `Default.aspx` | Atualiza estado dos botões de navegação | Scroll do carrossel |
| `navegarEsquerda()` | `Default.aspx` | Navega carrossel para esquerda | Botão esquerda |
| `navegarDireita()` | `Default.aspx` | Navega carrossel para direita | Botão direita |
| `salvarEstadoModalCarrinho()` | `Default.aspx` | Salva estado do modal do carrinho antes do postback | Antes de operações no carrinho |
| `restaurarEstadoModalCarrinho()` | `Default.aspx` | Restaura estado do modal do carrinho após postback | Após postback |
| `aumentarQuantidadeCarrinho(produtoId, tamanho)` | `Default.aspx` | Aumenta quantidade de item no carrinho | Botão + no carrinho |
| `diminuirQuantidadeCarrinho(produtoId, tamanho)` | `Default.aspx` | Diminui quantidade de item no carrinho | Botão - no carrinho |
| `adicionarMaisItem(produtoId, tamanho)` | `Default.aspx` | Adiciona mais um item ao carrinho | Botão "Mais" no carrinho |
| `reservarProdutoRapido(buttonElement)` | `Default.aspx` | Adiciona produto ao carrinho rapidamente (botão Reservar) | Botão "Reservar" nos cards |

---

## 4. FUNÇÕES ADMIN.ASPX (admin.js + inline)

### 4.1. Namespace AdminPage.Modal

| Função | Arquivo | Descrição | Onde é Usada |
|--------|---------|-----------|--------------|
| `AdminPage.Modal.fecharEditarProduto()` | `Scripts/admin.js` | Fecha modal de editar produto | Botões de fechar |
| `AdminPage.Modal.fecharNovoProduto()` | `Scripts/admin.js` | Fecha modal de novo produto | Botões de fechar |
| `AdminPage.Modal.fecharEditarReserva()` | `Scripts/admin.js` | Fecha modal de editar reserva | Botões de fechar |

### 4.2. Namespace AdminPage.Produtos

| Função | Arquivo | Descrição | Onde é Usada |
|--------|---------|-----------|--------------|
| `AdminPage.Produtos.editar(id, nome, descricao, preco, imagemUrl, ordem, ativo, reservavelAte, vendivelAte, ehSacoPromocional, quantidadeSaco, produtosPermitidos)` | `Scripts/admin.js` | Preenche e abre modal de editar produto | Botão "Editar" na lista de produtos |
| `AdminPage.Produtos.atualizarPreview(input)` | `Scripts/admin.js` | Atualiza preview da imagem ao digitar URL | Evento input do campo de URL da imagem |

### 4.3. Namespace AdminPage.Reservas

| Função | Arquivo | Descrição | Onde é Usada |
|--------|---------|-----------|--------------|
| `AdminPage.Reservas.editar(id, statusId, valorTotal, convertidoEmPedido, cancelado, previsaoEntrega, observacoes)` | `Scripts/admin.js` | Preenche e abre modal de editar reserva | Botão "Editar" na lista de reservas |

### 4.4. Funções Globais (Compatibilidade)

| Função | Arquivo | Descrição | Onde é Usada |
|--------|---------|-----------|--------------|
| `editarProduto(...)` | `Scripts/admin.js` | Wrapper para AdminPage.Produtos.editar | Botão "Editar" na lista de produtos |
| `editarReserva(...)` | `Scripts/admin.js` | Wrapper para AdminPage.Reservas.editar | Botão "Editar" na lista de reservas |

### 4.5. Funções Inline (Admin.aspx)

| Função | Arquivo | Descrição | Onde é Usada |
|--------|---------|-----------|--------------|
| `__doPostBack(eventTarget, eventArgument)` | `Admin.aspx` | Função para fazer postback do ASP.NET | Todas as funções que fazem postback |
| `toggleLogGroup(element)` | `Admin.aspx` | Alterna visibilidade de grupo de logs | Click nos headers de log |
| `carregarLogs()` | `Admin.aspx` | Carrega logs do sistema | Botão "Carregar Logs" |
| `toggleSacoPromocional(checkbox)` | `Admin.aspx` | Alterna visibilidade de campos de saco promocional | Checkbox "É Saco Promocional" (editar) |
| `toggleSacoPromocionalNovo(checkbox)` | `Admin.aspx` | Alterna visibilidade de campos de saco promocional | Checkbox "É Saco Promocional" (novo) |
| `editarStatusReserva(statusId)` | `Admin.aspx` | Abre modal para editar status de reserva | Botão "Editar" na lista de status |
| `excluirStatusReserva(statusId, nome)` | `Admin.aspx` | Exclui status de reserva | Botão "Excluir" na lista de status |
| `excluirReserva(reservaId, nome)` | `Admin.aspx` | Exclui reserva | Botão "Excluir" na lista de reservas |
| `atualizarPreviewImagem(input)` | `Admin.aspx` | Atualiza preview da imagem | Evento input do campo de URL |
| `carregarDadosReserva(reservaId)` | `Admin.aspx` | Carrega dados da reserva para edição | Botão "Editar" na lista de reservas |
| `desabilitarValidacaoCamposOcultos()` | `Admin.aspx` | Desabilita validação de campos ocultos | Submit de formulários |
| `validarESalvarReserva()` | `Admin.aspx` | Valida e salva reserva | Botão "Salvar" no modal de reserva |
| `validarESalvarProduto()` | `Admin.aspx` | Valida e salva produto editado | Botão "Salvar" no modal de editar produto |
| `validarESalvarNovoProduto()` | `Admin.aspx` | Valida e salva novo produto | Botão "Salvar" no modal de novo produto |
| `init()` | `Admin.aspx` | Inicializa página admin | Inicialização da página |
| `voltarPagina()` | `Admin.aspx` | Volta para página anterior | Botão "Voltar" |
| `navegarParaAba(tabId)` | `Admin.aspx` | Navega para uma aba específica | Links de navegação |
| `mostrarDetalhesReservaAdmin(reservaId)` | `Admin.aspx` | Mostra detalhes de reserva | Botão "Ver Detalhes" |
| `mostrarDetalhesCliente(clienteId)` | `Admin.aspx` | Mostra detalhes de cliente | Botão "Ver Detalhes" |
| `mostrarDetalhesProduto(produtoId)` | `Admin.aspx` | Mostra detalhes de produto | Botão "Ver Detalhes" |

---

## 5. FUNÇÕES MINHASRESERVAS.ASPX (minhasreservas.js + inline)

### 5.1. Namespace MinhasReservasPage.Compartilhar

| Função | Arquivo | Descrição | Onde é Usada |
|--------|---------|-----------|--------------|
| `MinhasReservasPage.Compartilhar.facebook(url, texto)` | `Scripts/minhasreservas.js` | Compartilha reserva no Facebook | Botão "Compartilhar no Facebook" |
| `MinhasReservasPage.Compartilhar.whatsapp(url, texto)` | `Scripts/minhasreservas.js` | Compartilha reserva no WhatsApp | Botão "Compartilhar no WhatsApp" |
| `MinhasReservasPage.Compartilhar.twitter(url, texto)` | `Scripts/minhasreservas.js` | Compartilha reserva no Twitter | Botão "Compartilhar no Twitter" |

### 5.2. Funções Globais (Compatibilidade)

| Função | Arquivo | Descrição | Onde é Usada |
|--------|---------|-----------|--------------|
| `compartilharFacebook(url, texto)` | `Scripts/minhasreservas.js` | Wrapper para MinhasReservasPage.Compartilhar.facebook | Botão "Compartilhar no Facebook" |
| `compartilharWhatsApp(url, texto)` | `Scripts/minhasreservas.js` | Wrapper para MinhasReservasPage.Compartilhar.whatsapp | Botão "Compartilhar no WhatsApp" |
| `compartilharTwitter(url, texto)` | `Scripts/minhasreservas.js` | Wrapper para MinhasReservasPage.Compartilhar.twitter | Botão "Compartilhar no Twitter" |

### 5.3. Funções Inline (MinhasReservas.aspx)

| Função | Arquivo | Descrição | Onde é Usada |
|--------|---------|-----------|--------------|
| `__doPostBack(eventTarget, eventArgument)` | `MinhasReservas.aspx` | Função para fazer postback do ASP.NET | Todas as funções que fazem postback |
| `mostrarDetalhesReserva(reservaId)` | `MinhasReservas.aspx` | Mostra detalhes de uma reserva | Botão "Ver Detalhes" |
| `escapeHtml(text)` | `MinhasReservas.aspx` | Escapa HTML para exibição segura | Renderização de dados |
| `escapeAttr(text)` | `MinhasReservas.aspx` | Escapa atributos HTML | Renderização de atributos |
| `compartilharEmail(url, texto)` | `MinhasReservas.aspx` | Compartilha reserva por email | Botão "Compartilhar por Email" |
| `cancelarReserva(reservaId)` | `MinhasReservas.aspx` | Cancela uma reserva | Botão "Cancelar" |
| `excluirReserva(reservaId)` | `MinhasReservas.aspx` | Exclui uma reserva | Botão "Excluir" |
| `voltarPagina()` | `MinhasReservas.aspx` | Volta para página anterior | Botão "Voltar" |

---

## 6. FUNÇÕES VERRESERVA.ASPX (inline)

| Função | Arquivo | Descrição | Onde é Usada |
|--------|---------|-----------|--------------|
| `voltarPagina()` | `VerReserva.aspx` | Volta para página anterior | Botão "Voltar" |
| `compartilharFacebook(url, texto)` | `VerReserva.aspx` | Compartilha reserva no Facebook | Botão "Compartilhar no Facebook" |
| `compartilharWhatsApp(url, texto)` | `VerReserva.aspx` | Compartilha reserva no WhatsApp | Botão "Compartilhar no WhatsApp" |
| `compartilharTwitter(url, texto)` | `VerReserva.aspx` | Compartilha reserva no Twitter | Botão "Compartilhar no Twitter" |
| `compartilharEmail(url, texto)` | `VerReserva.aspx` | Compartilha reserva por email | Botão "Compartilhar por Email" |
| `salvarReserva(reservaId)` | `VerReserva.aspx` | Salva alterações na reserva | Botão "Salvar" |
| `cancelarEdicao()` | `VerReserva.aspx` | Cancela edição da reserva | Botão "Cancelar" |
| `atualizarSubtotalItem(input)` | `VerReserva.aspx` | Atualiza subtotal de item ao alterar quantidade | Evento change do input de quantidade |
| `removerItem(button)` | `VerReserva.aspx` | Remove item da reserva | Botão "Remover" |
| `adicionarNovoItem()` | `VerReserva.aspx` | Adiciona novo item à reserva | Botão "Adicionar Item" |
| `selecionarProdutoNovoItem(select)` | `VerReserva.aspx` | Seleciona produto para novo item | Change do select de produtos |
| `atualizarPrecoNovoItem()` | `VerReserva.aspx` | Atualiza preço do novo item | Após selecionar produto |
| `confirmarNovoItem()` | `VerReserva.aspx` | Confirma adição de novo item | Botão "Confirmar" |
| `cancelarNovoItem()` | `VerReserva.aspx` | Cancela adição de novo item | Botão "Cancelar" |
| `atualizarValorTotal()` | `VerReserva.aspx` | Atualiza valor total da reserva | Após alterações nos itens |
| `inicializarBotoesEditarProdutosSaco()` | `VerReserva.aspx` | Inicializa botões de editar produtos do saco | Inicialização |
| `createHiddenInput(name, value)` | `VerReserva.aspx` | Cria input hidden | Usada internamente |
| `editarProdutosSaco(itemIndex, produtosJson, produtosPermitidos)` | `VerReserva.aspx` | Abre modal para editar produtos do saco | Botão "Editar Produtos" |
| `filtrarProdutosPorIds(produtosPermitidosJson)` | `VerReserva.aspx` | Filtra produtos disponíveis por IDs | Carregamento de produtos no modal |
| `preencherModalProdutosSaco()` | `VerReserva.aspx` | Preenche modal com produtos do saco | Ao abrir modal de edição |
| `adicionarProdutoSacoNoModal(produtoId, quantidade, index)` | `VerReserva.aspx` | Adiciona produto ao saco no modal | Botão "Adicionar" no modal |
| `adicionarNovoProdutoSaco()` | `VerReserva.aspx` | Adiciona novo produto ao saco | Botão "Adicionar Produto" |
| `removerProdutoSaco(button)` | `VerReserva.aspx` | Remove produto do saco | Botão "Remover" no modal |
| `salvarProdutosSaco()` | `VerReserva.aspx` | Salva alterações nos produtos do saco | Botão "Salvar" no modal |
| `atualizarExibicaoProdutosSaco(itemReserva, produtos)` | `VerReserva.aspx` | Atualiza exibição dos produtos do saco | Após salvar alterações |
| `atualizarNomeProdutoSaco(select)` | `VerReserva.aspx` | Atualiza nome do produto do saco | Change do select de produtos |

---

## 7. RESUMO POR CATEGORIA

### 7.1. Funções de Carrinho
- `DefaultPage.Carrinho.*` (4 funções)
- `adicionarAoCarrinho`, `atualizarQuantidade`, `removerItem`
- `adicionarSacoAoCarrinho`, `atualizarTotalSelecionado`
- `reservarProdutoRapido`
- **Total: 9 funções**

### 7.2. Funções de Modal
- `KingdomConfeitaria.Modal.*` (3 funções)
- `DefaultPage.ModalReserva.*` (3 funções)
- `AdminPage.Modal.*` (3 funções)
- `abrirModalLogin`, `fecharModalLogin`
- `abrirModalProduto`, `fecharModalProduto`
- `abrirModalCarrinho`
- **Total: 13 funções**

### 7.3. Funções de Login/Autenticação
- `initLoginDinamico`, `initLoginDinamicoReady`
- `verificarClienteDinamico`, `verificarClienteStandalone`
- `fazerLoginStandalone`, `validarSenhaStandalone`
- `preencherDadosCliente`
- `mostrarCampoSenha`
- **Total: 8 funções**

### 7.4. Funções de Validação
- `DefaultPage.Validacao.*` (3 funções)
- `validarCampoTelefone`, `validarCampoNome`, `validarCampoEmail`
- `validarFormularioReserva`
- `validarESalvarReserva`, `validarESalvarProduto`, `validarESalvarNovoProduto`
- **Total: 9 funções**

### 7.5. Funções AJAX/Handler
- `KingdomConfeitaria.Ajax.*` (9 funções)
- **Total: 9 funções**

### 7.6. Funções de Produtos
- `DefaultPage.Tamanho.*` (3 funções)
- `AdminPage.Produtos.*` (2 funções)
- `editarProduto`
- `atualizarPreviewImagem`, `atualizarPreview`
- `toggleSacoPromocional`, `toggleSacoPromocionalNovo`
- **Total: 9 funções**

### 7.7. Funções de Reservas
- `AdminPage.Reservas.*` (1 função)
- `editarReserva`
- `carregarDadosReserva`
- `salvarReserva`, `cancelarReserva`, `excluirReserva`
- `mostrarDetalhesReserva`, `mostrarDetalhesReservaAdmin`
- **Total: 7 funções**

### 7.8. Funções de Compartilhamento
- `MinhasReservasPage.Compartilhar.*` (3 funções)
- `compartilharFacebook`, `compartilharWhatsApp`, `compartilharTwitter`, `compartilharEmail`
- **Total: 7 funções**

### 7.9. Funções Utilitárias
- `KingdomConfeitaria.Utils.*` (4 funções)
- `__doPostBack` (3 implementações)
- `escapeJs`, `escapeHtml`, `escapeAttr`
- `voltarPagina` (3 implementações)
- **Total: 12 funções**

### 7.10. Funções de UI/Interface
- `ajustarEspacamentoHeader`
- `adicionarIconeBotaoReserva`
- `iniciarAnimacaoMao`
- `inicializarNavegacaoCarrossel`
- `atualizarBotoesNavegacao`, `navegarEsquerda`, `navegarDireita`
- `salvarEstadoModalCarrinho`, `restaurarEstadoModalCarrinho`
- `DefaultPage.Imagens.carregarSilenciosamente`
- **Total: 9 funções**

### 7.11. Funções Específicas VerReserva
- Funções de edição de itens e sacos promocionais
- **Total: 15 funções**

### 7.12. Funções Específicas Admin
- Funções de gerenciamento de logs, status, produtos e reservas
- **Total: 12 funções**

---

## 8. ESTATÍSTICAS GERAIS

- **Total de Funções JavaScript: ~120+**
- **Arquivos JavaScript Externos: 4** (app.js, default.js, admin.js, minhasreservas.js, ajax-helper.js)
- **Páginas com Scripts Inline: 4** (Default.aspx, Admin.aspx, MinhasReservas.aspx, VerReserva.aspx)
- **Namespaces Criados: 6** (KingdomConfeitaria, DefaultPage, AdminPage, MinhasReservasPage, PageMethods)

---

## 9. OBSERVAÇÕES IMPORTANTES

1. **Funções Duplicadas**: Algumas funções existem tanto como métodos de namespace quanto como funções globais (wrappers para compatibilidade).

2. **Funções Inline vs Externas**: Funções específicas de uma página estão inline, enquanto funções reutilizáveis estão em arquivos .js externos.

3. **Compatibilidade**: Muitas funções globais são wrappers para manter compatibilidade com código inline que usa onclick.

4. **Postback**: A função `__doPostBack` é implementada manualmente em várias páginas para funcionar sem ScriptManager.

5. **Handler**: Todas as chamadas AJAX agora usam `KingdomConfeitaria.Ajax.callHandler` apontando para `Handlers/CallbackHandler.ashx`.

---

**Documento gerado em:** 2024
**Última atualização:** Após migração para Handler + Fetch API

