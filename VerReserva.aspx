<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="VerReserva.aspx.cs" Inherits="KingdomConfeitaria.VerReserva" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Ver Reserva - Kingdom Confeitaria</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" rel="stylesheet" />
    <style>
        body {
            background: linear-gradient(135deg, #1a4d2e 0%, #2d5a3d 100%);
            min-height: 100vh;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }
        .header-logo {
            background: #1a4d2e;
            padding: 10px 20px;
            text-align: center;
            border-radius: 0;
            margin-bottom: 0;
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            width: 100%;
            z-index: 1000;
            box-shadow: 0 2px 10px rgba(0,0,0,0.2);
        }
        .header-actions {
            position: absolute;
            top: 10px;
            right: 20px;
        }
        .header-actions a {
            color: white;
            text-decoration: none;
            margin-left: 15px;
            font-size: 14px;
        }
        .header-actions a:hover {
            text-decoration: underline;
        }
        .header-logo img {
            max-width: 20%;
            width: auto;
            height: auto;
            max-height: 80px;
        }
        .container-main {
            background: white;
            border-radius: 20px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
            margin: 90px auto 20px auto;
            padding: 30px;
        }
        .reserva-detalhes {
            border: 2px solid #e9ecef;
            border-radius: 15px;
            padding: 30px;
            margin-bottom: 20px;
        }
        .status-badge {
            padding: 8px 20px;
            border-radius: 20px;
            font-size: 14px;
            font-weight: bold;
        }
        .status-aberta { background-color: #28a745; color: #fff; }
        .status-em-produção { background-color: #ffc107; color: #000; }
        .status-produção-pronta { background-color: #17a2b8; color: #fff; }
        .status-preparando-entrega { background-color: #007bff; color: #fff; }
        .status-saiu-para-entrega { background-color: #6f42c1; color: #fff; }
        .status-já-entregue { background-color: #6c757d; color: #fff; }
        .status-cancelado { background-color: #dc3545; color: #fff; }
        /* Compatibilidade com status antigos */
        .status-pendente { background-color: #28a745; color: #fff; }
        .status-confirmado { background-color: #17a2b8; color: #fff; }
        .status-pronto { background-color: #17a2b8; color: #fff; }
        .status-entregue { background-color: #6c757d; color: #fff; }
        .btn-share {
            margin: 5px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true"></asp:ScriptManager>
        <div class="container-fluid">
            <div class="header-logo">
                <div class="header-actions">
                    <a href="Default.aspx"><i class="fas fa-home"></i> Home</a>
                    <span id="clienteNome" runat="server" style="color: white; margin-right: 15px;"></span>
                    <a href="#" id="linkLogin" runat="server" style="display: inline;" onclick="abrirModalLogin(); return false;">Entrar</a>
                    <a href="MinhasReservas.aspx" id="linkMinhasReservas" runat="server" style="display:none;">Minhas Reservas</a>
                    <a href="MeusDados.aspx" id="linkMeusDados" runat="server" style="display:none;">Meus Dados</a>
                    <a href="Admin.aspx" id="linkAdmin" runat="server" style="display:none;">Painel Gestor</a>
                    <a href="Logout.aspx" id="linkLogout" runat="server" style="display:none;">Sair</a>
                </div>
                <img src="Images/logo-kingdom-confeitaria.svg" alt="Kingdom Confeitaria" style="max-width: 100%; height: auto;" />
                <h1 style="display:none; color: white; margin: 0;">Kingdom Confeitaria</h1>
            </div>
            
            <div class="container-main">
                <div id="conteudoContainer" runat="server">
                    <!-- Conteúdo será carregado aqui -->
                </div>
            </div>
        </div>
    </form>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script>
        function compartilharFacebook(url, texto) {
            window.open('https://www.facebook.com/sharer/sharer.php?u=' + encodeURIComponent(url) + '&quote=' + encodeURIComponent(texto), '_blank', 'width=600,height=400');
        }

        function compartilharWhatsApp(url, texto) {
            window.open('https://wa.me/?text=' + encodeURIComponent(texto + ' ' + url), '_blank');
        }

        function compartilharTwitter(url, texto) {
            window.open('https://twitter.com/intent/tweet?url=' + encodeURIComponent(url) + '&text=' + encodeURIComponent(texto), '_blank', 'width=600,height=400');
        }

        function compartilharEmail(url, texto) {
            window.location.href = 'mailto:?subject=Minha Reserva - Kingdom Confeitaria&body=' + encodeURIComponent(texto + '\n\n' + url);
        }

        function salvarReserva(reservaId) {
            var dataRetirada = document.getElementById('txtDataRetiradaEdicao');
            var observacoes = document.getElementById('txtObservacoes');
            var hdnReservaId = document.getElementById('hdnReservaId');
            var hdnToken = document.getElementById('hdnToken');
            
            if (!dataRetirada || !dataRetirada.value) {
                alert('Por favor, preencha a data de retirada.');
                if (dataRetirada) dataRetirada.focus();
                return;
            }

            // Criar formulário para submissão
            var form = document.createElement('form');
            form.method = 'POST';
            form.action = window.location.href;
            
            var inputReservaId = document.createElement('input');
            inputReservaId.type = 'hidden';
            inputReservaId.name = 'hdnReservaId';
            inputReservaId.value = reservaId;
            form.appendChild(inputReservaId);
            
            var inputToken = document.createElement('input');
            inputToken.type = 'hidden';
            inputToken.name = 'hdnToken';
            inputToken.value = hdnToken ? hdnToken.value : '';
            form.appendChild(inputToken);
            
            var inputDataRetirada = document.createElement('input');
            inputDataRetirada.type = 'hidden';
            inputDataRetirada.name = 'txtDataRetiradaEdicao';
            inputDataRetirada.value = dataRetirada.value;
            form.appendChild(inputDataRetirada);
            
            var inputObservacoes = document.createElement('input');
            inputObservacoes.type = 'hidden';
            inputObservacoes.name = 'txtObservacoes';
            inputObservacoes.value = observacoes ? observacoes.value : '';
            form.appendChild(inputObservacoes);
            
            var inputEventTarget = document.createElement('input');
            inputEventTarget.type = 'hidden';
            inputEventTarget.name = '__EVENTTARGET';
            inputEventTarget.value = 'btnSalvarReserva';
            form.appendChild(inputEventTarget);
            
            document.body.appendChild(form);
            form.submit();
        }

        function cancelarEdicao() {
            if (confirm('Deseja cancelar a edição? As alterações não salvas serão perdidas.')) {
                window.location.reload();
            }
        }

        var produtosDisponiveis = [];
        var produtoSelecionado = null;

        // Carregar produtos disponíveis ao carregar a página e inicializar valor total
        window.addEventListener('DOMContentLoaded', function() {
            // Inicializar valor total com os itens existentes
            setTimeout(function() {
                atualizarValorTotal();
            }, 100);
            
            if (typeof PageMethods !== 'undefined') {
                PageMethods.ObterProdutosDisponiveis(function(result) {
                    if (result && !result.erro) {
                        produtosDisponiveis = result;
                        window.produtosDisponiveis = result; // Disponibilizar globalmente
                        var select = document.getElementById('novoItemProduto');
                        if (select) {
                            result.forEach(function(produto) {
                                var option = document.createElement('option');
                                option.value = produto.id;
                                option.textContent = produto.nome;
                                option.setAttribute('data-preco', produto.preco);
                                select.appendChild(option);
                            });
                        }
                    }
                });
            }
        });

        function atualizarSubtotalItem(input) {
            var quantidade = parseInt(input.value) || 0;
            if (quantidade < 1) {
                quantidade = 1;
                input.value = 1;
            }
            
            var precoUnitarioStr = input.getAttribute('data-preco-unitario');
            // Garantir que o valor está no formato correto (ponto como separador decimal)
            var precoUnitario = parseFloat(precoUnitarioStr.toString().replace(',', '.').replace(/[^\d.]/g, '')) || 0;
            var subtotal = quantidade * precoUnitario;
            
            var itemDiv = input.closest('.item-reserva');
            var subtotalInput = itemDiv.querySelector('.subtotal-item');
            if (subtotalInput) {
                subtotalInput.value = 'R$ ' + subtotal.toFixed(2).replace('.', ',');
            }
            
            // Atualizar também o campo hidden do preço unitário se necessário
            var precoUnitarioHidden = itemDiv.querySelector('.preco-unitario');
            if (precoUnitarioHidden) {
                precoUnitarioHidden.value = precoUnitario.toFixed(2).replace(',', '.');
            }
            
            atualizarValorTotal();
        }

        function removerItem(button) {
            if (confirm('Deseja remover este item da reserva?')) {
                var itemDiv = button.closest('.item-reserva');
                itemDiv.remove();
                atualizarValorTotal();
            }
        }

        function adicionarNovoItem() {
            var container = document.getElementById('novoItemContainer');
            if (container) {
                container.style.display = container.style.display === 'none' ? 'block' : 'none';
            }
        }

        function selecionarProdutoNovoItem(select) {
            var produtoId = select.value;
            if (produtoId) {
                produtoSelecionado = produtosDisponiveis.find(function(p) {
                    return p.id == produtoId;
                });
                atualizarPrecoNovoItem();
            } else {
                produtoSelecionado = null;
                document.getElementById('novoItemPrecoInfo').textContent = '';
            }
        }

        function atualizarPrecoNovoItem() {
            var tamanho = document.getElementById('novoItemTamanho').value;
            var quantidade = parseInt(document.getElementById('novoItemQuantidade').value) || 1;
            var precoInfo = document.getElementById('novoItemPrecoInfo');
            
            if (produtoSelecionado) {
                // Usar o preço único do produto
                var preco = parseFloat(produtoSelecionado.preco) || 0;
                var subtotal = preco * quantidade;
                precoInfo.textContent = 'Preço unitário: R$ ' + preco.toFixed(2).replace('.', ',') + ' | Subtotal: R$ ' + subtotal.toFixed(2).replace('.', ',');
            } else {
                precoInfo.textContent = '';
            }
        }

        function confirmarNovoItem() {
            var produtoSelect = document.getElementById('novoItemProduto');
            var tamanhoSelect = document.getElementById('novoItemTamanho');
            var quantidadeInput = document.getElementById('novoItemQuantidade');
            
            if (!produtoSelect.value || !tamanhoSelect.value || !quantidadeInput.value) {
                alert('Por favor, preencha todos os campos.');
                return;
            }
            
            var produto = produtosDisponiveis.find(function(p) {
                return p.id == produtoSelect.value;
            });
            
            if (!produto) {
                alert('Produto não encontrado.');
                return;
            }
            
            var tamanho = tamanhoSelect.value;
            var quantidade = parseInt(quantidadeInput.value) || 1;
            // Usar o preço único do produto
            var precoUnitario = parseFloat(produto.preco) || 0;
            var subtotal = precoUnitario * quantidade;
            
            // Adicionar item ao container
            var container = document.getElementById('itensReservaContainer');
            if (!container) {
                container = document.createElement('div');
                container.id = 'itensReservaContainer';
                var itensLista = document.getElementById('itensLista');
                if (itensLista) {
                    itensLista.innerHTML = '';
                    itensLista.appendChild(container);
                }
            }
            
            var itemIndex = container.children.length;
            // Garantir que o preço unitário está no formato correto (ponto como separador decimal)
            var precoUnitarioFormatado = precoUnitario.toFixed(2).replace(',', '.');
            var subtotalFormatado = subtotal.toFixed(2).replace(',', '.');
            
            var itemHtml = `
                <div class='item-reserva mb-3 p-3 border rounded' data-item-index='${itemIndex}'>
                    <div class='row align-items-center'>
                        <div class='col-md-4'>
                            <strong>${produto.nome}</strong> (${tamanho})
                        </div>
                        <div class='col-md-3'>
                            <label class='form-label small'>Quantidade:</label>
                            <input type='number' class='form-control form-control-sm quantidade-item' 
                                   value='${quantidade}' min='1' data-produto-id='${produto.id}' data-tamanho='${tamanho}' 
                                   data-preco-unitario='${precoUnitarioFormatado}' onchange='atualizarSubtotalItem(this)' />
                        </div>
                        <div class='col-md-3'>
                            <label class='form-label small'>Subtotal:</label>
                            <input type='text' class='form-control form-control-sm subtotal-item' 
                                   value='R$ ${subtotalFormatado.replace('.', ',')}' readonly />
                        </div>
                        <div class='col-md-2 text-end'>
                            <button type='button' class='btn btn-danger btn-sm' onclick='removerItem(this)'>
                                <i class='fas fa-trash'></i>
                            </button>
                        </div>
                    </div>
                    <input type='hidden' class='produto-id' value='${produto.id}' />
                    <input type='hidden' class='nome-produto' value='${produto.nome.replace(/'/g, "\\'")}' />
                    <input type='hidden' class='tamanho-item' value='${tamanho}' />
                    <input type='hidden' class='preco-unitario' value='${precoUnitarioFormatado}' />
                </div>
            `;
            
            container.insertAdjacentHTML('beforeend', itemHtml);
            
            // Limpar formulário
            produtoSelect.value = '';
            tamanhoSelect.value = '';
            quantidadeInput.value = '1';
            document.getElementById('novoItemPrecoInfo').textContent = '';
            document.getElementById('novoItemContainer').style.display = 'none';
            produtoSelecionado = null;
            
            atualizarValorTotal();
        }

        function cancelarNovoItem() {
            document.getElementById('novoItemProduto').value = '';
            document.getElementById('novoItemTamanho').value = '';
            document.getElementById('novoItemQuantidade').value = '1';
            document.getElementById('novoItemPrecoInfo').textContent = '';
            document.getElementById('novoItemContainer').style.display = 'none';
            produtoSelecionado = null;
        }

        function atualizarValorTotal() {
            var total = 0;
            // Buscar apenas os subtotais dos itens (dentro do container de itens)
            var itensContainer = document.getElementById('itensReservaContainer');
            if (itensContainer) {
                var subtotais = itensContainer.querySelectorAll('.subtotal-item');
                subtotais.forEach(function(input) {
                    // Remover formatação: "R$ ", espaços, e substituir vírgula por ponto
                    var valorStr = input.value.replace(/R\$\s*/g, '').replace(/\s/g, '').replace(',', '.');
                    var valor = parseFloat(valorStr) || 0;
                    if (!isNaN(valor)) {
                        total += valor;
                    }
                });
            }
            
            var valorTotalSpan = document.getElementById('valorTotalReserva');
            if (valorTotalSpan) {
                valorTotalSpan.textContent = 'R$ ' + total.toFixed(2).replace('.', ',');
            }
        }

        // Inicializar event listeners para botões de editar produtos do saco
        function inicializarBotoesEditarProdutosSaco() {
            var botoes = document.querySelectorAll('.btn-editar-produtos-saco');
            botoes.forEach(function(botao) {
                // Remover listeners antigos
                var novoBotao = botao.cloneNode(true);
                botao.parentNode.replaceChild(novoBotao, botao);
                
                novoBotao.addEventListener('click', function() {
                    var itemIndex = parseInt(this.getAttribute('data-item-index'));
                    var produtosJson = this.getAttribute('data-produtos-json') || '';
                    var produtosPermitidos = this.getAttribute('data-produtos-permitidos') || '';
                    editarProdutosSaco(itemIndex, produtosJson, produtosPermitidos);
                });
            });
        }
        
        // Inicializar quando a página carregar
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', inicializarBotoesEditarProdutosSaco);
        } else {
            inicializarBotoesEditarProdutosSaco();
        }
        
        // Re-inicializar após postback
        window.addEventListener('pageshow', function(event) {
            inicializarBotoesEditarProdutosSaco();
        });

        // Atualizar função salvarReserva para incluir itens
        var salvarReservaOriginal = salvarReserva;
        salvarReserva = function(reservaId) {
            var dataRetirada = document.getElementById('txtDataRetiradaEdicao');
            var observacoes = document.getElementById('txtObservacoes');
            var hdnReservaId = document.getElementById('hdnReservaId');
            var hdnToken = document.getElementById('hdnToken');
            
            if (!dataRetirada || !dataRetirada.value) {
                alert('Por favor, preencha a data de retirada.');
                if (dataRetirada) dataRetirada.focus();
                return;
            }

            // Criar formulário para submissão
            var form = document.createElement('form');
            form.method = 'POST';
            form.action = window.location.href;
            
            var inputReservaId = document.createElement('input');
            inputReservaId.type = 'hidden';
            inputReservaId.name = 'hdnReservaId';
            inputReservaId.value = reservaId;
            form.appendChild(inputReservaId);
            
            var inputToken = document.createElement('input');
            inputToken.type = 'hidden';
            inputToken.name = 'hdnToken';
            inputToken.value = hdnToken ? hdnToken.value : '';
            form.appendChild(inputToken);
            
            var inputDataRetirada = document.createElement('input');
            inputDataRetirada.type = 'hidden';
            inputDataRetirada.name = 'txtDataRetiradaEdicao';
            inputDataRetirada.value = dataRetirada.value;
            form.appendChild(inputDataRetirada);
            
            var inputObservacoes = document.createElement('input');
            inputObservacoes.type = 'hidden';
            inputObservacoes.name = 'txtObservacoes';
            inputObservacoes.value = observacoes ? observacoes.value : '';
            form.appendChild(inputObservacoes);
            
            // Adicionar itens
            var itensContainer = document.getElementById('itensReservaContainer');
            if (itensContainer) {
                var itens = itensContainer.querySelectorAll('.item-reserva');
                itens.forEach(function(item, index) {
                    var produtoId = item.querySelector('.produto-id').value;
                    var nomeProduto = item.querySelector('.nome-produto').value;
                    var tamanho = item.querySelector('.tamanho-item').value;
                    var quantidade = item.querySelector('.quantidade-item').value;
                    var precoUnitarioInput = item.querySelector('.preco-unitario');
                    // Garantir que o preço está no formato correto (ponto como separador decimal)
                    var precoUnitarioValue = precoUnitarioInput ? precoUnitarioInput.value.toString() : '0';
                    // Remover qualquer formatação e garantir ponto como separador decimal
                    var precoUnitario = precoUnitarioValue.replace(/[^\d.]/g, '').replace(',', '.');
                    
                    var produtos = item.querySelector('.produtos-item');
                    var produtosValue = produtos ? produtos.value : '';
                    
                    form.appendChild(createHiddenInput('itens[' + index + '].ProdutoId', produtoId));
                    form.appendChild(createHiddenInput('itens[' + index + '].NomeProduto', nomeProduto));
                    form.appendChild(createHiddenInput('itens[' + index + '].Tamanho', tamanho));
                    form.appendChild(createHiddenInput('itens[' + index + '].Quantidade', quantidade));
                    form.appendChild(createHiddenInput('itens[' + index + '].PrecoUnitario', precoUnitario));
                    form.appendChild(createHiddenInput('itens[' + index + '].Produtos', produtosValue));
                });
            }
            
            var inputEventTarget = document.createElement('input');
            inputEventTarget.type = 'hidden';
            inputEventTarget.name = '__EVENTTARGET';
            inputEventTarget.value = 'btnSalvarReserva';
            form.appendChild(inputEventTarget);
            
            document.body.appendChild(form);
            form.submit();
        };

        function createHiddenInput(name, value) {
            var input = document.createElement('input');
            input.type = 'hidden';
            input.name = name;
            input.value = value;
            return input;
        }

        var itemIndexEditando = -1;
        var produtosSacoAtuais = [];

        var produtosPermitidosAtuais = '';
        
        function editarProdutosSaco(itemIndex, produtosJson, produtosPermitidos) {
            console.log('editarProdutosSaco chamado:', itemIndex, produtosJson, produtosPermitidos);
            itemIndexEditando = itemIndex;
            produtosPermitidosAtuais = produtosPermitidos || '';
            
            try {
                if (!produtosJson || produtosJson === '' || produtosJson === 'null' || produtosJson === 'undefined') {
                    produtosSacoAtuais = [];
                } else {
                    // produtosJson já vem como string do data-attribute
                    produtosSacoAtuais = JSON.parse(produtosJson);
                }
            } catch (e) {
                console.error('Erro ao parsear JSON de produtos:', e, produtosJson);
                alert('Erro ao carregar produtos: ' + e.message);
                produtosSacoAtuais = [];
            }
            
            // Carregar produtos disponíveis filtrados por IDs permitidos
            if (typeof PageMethods !== 'undefined') {
                PageMethods.ObterProdutosDisponiveis(produtosPermitidosAtuais, function(result) {
                    if (result && !result.erro) {
                        window.produtosDisponiveis = result;
                        preencherModalProdutosSaco();
                    } else {
                        alert('Erro ao carregar produtos disponíveis.');
                        console.error('Erro ao obter produtos:', result);
                    }
                });
            } else {
                // Se PageMethods não estiver disponível, usar produtos já carregados e filtrar
                if (window.produtosDisponiveis && window.produtosDisponiveis.length > 0) {
                    filtrarProdutosPorIds(produtosPermitidosAtuais);
                    preencherModalProdutosSaco();
                } else {
                    alert('Erro: Produtos não carregados. Por favor, recarregue a página.');
                }
            }
        }
        
        function filtrarProdutosPorIds(produtosPermitidosJson) {
            if (!produtosPermitidosJson || !window.produtosDisponiveis) return;
            
            try {
                var produtosIds = JSON.parse(produtosPermitidosJson);
                window.produtosDisponiveis = window.produtosDisponiveis.filter(function(p) {
                    var pId = p.Id || p.id;
                    return produtosIds.indexOf(pId) !== -1;
                });
            } catch (e) {
                console.error('Erro ao filtrar produtos por IDs:', e);
            }
        }
        
        function preencherModalProdutosSaco() {
            // Preencher modal com produtos atuais
            var container = document.getElementById('produtosSacoContainer');
            if (!container) {
                alert('Erro: Container de produtos não encontrado.');
                console.error('Container produtosSacoContainer não encontrado');
                return;
            }
            container.innerHTML = '';
            
            if (produtosSacoAtuais && produtosSacoAtuais.length > 0) {
                produtosSacoAtuais.forEach(function(produto, index) {
                    adicionarProdutoSacoNoModal(produto.id, produto.qt, index);
                });
            } else {
                // Se não houver produtos, adicionar um campo vazio
                adicionarProdutoSacoNoModal('', 1, 0);
            }
            
            // Verificar se Bootstrap está disponível
            if (typeof bootstrap === 'undefined') {
                alert('Erro: Bootstrap não está carregado. Por favor, recarregue a página.');
                console.error('Bootstrap não está disponível');
                return;
            }
            
            // Mostrar modal
            var modalElement = document.getElementById('modalEditarProdutosSaco');
            if (!modalElement) {
                alert('Erro: Modal não encontrado.');
                console.error('Modal modalEditarProdutosSaco não encontrado');
                return;
            }
            
            try {
                var modal = bootstrap.Modal.getOrCreateInstance(modalElement);
                modal.show();
                console.log('Modal aberto com sucesso');
            } catch (e) {
                console.error('Erro ao abrir modal:', e);
                alert('Erro ao abrir modal: ' + e.message);
            }
        }

        function adicionarProdutoSacoNoModal(produtoId, quantidade, index) {
            var container = document.getElementById('produtosSacoContainer');
            var produtosDisponiveis = window.produtosDisponiveis || [];
            
            // Filtrar produtos por IDs permitidos se necessário
            if (produtosPermitidosAtuais) {
                try {
                    var produtosIds = JSON.parse(produtosPermitidosAtuais);
                    produtosDisponiveis = produtosDisponiveis.filter(function(p) {
                        var pId = p.Id || p.id;
                        return produtosIds.indexOf(pId) !== -1;
                    });
                } catch (e) {
                    console.error('Erro ao filtrar produtos por IDs:', e);
                }
            }
            
            var produto = produtosDisponiveis.find(p => (p.Id || p.id) == produtoId);
            var nomeProduto = produto ? (produto.Nome || produto.nome) : 'Produto #' + produtoId;
            
            var div = document.createElement('div');
            div.className = 'row mb-2 produto-saco-item';
            div.setAttribute('data-index', index);
            var produtosOptions = produtosDisponiveis.map(function(p) {
                var pId = p.Id || p.id;
                var pNome = p.Nome || p.nome;
                var selected = pId == produtoId ? 'selected' : '';
                return `<option value='${pId}' ${selected}>${pNome}</option>`;
            }).join('');
            
            div.innerHTML = `
                <div class='col-md-6'>
                    <select class='form-select form-select-sm produto-saco-select' onchange='atualizarNomeProdutoSaco(this)'>
                        <option value=''>Selecione...</option>
                        ${produtosOptions}
                    </select>
                </div>
                <div class='col-md-3'>
                    <input type='number' class='form-control form-control-sm quantidade-produto-saco' value='${quantidade || 1}' min='1' />
                </div>
                <div class='col-md-3'>
                    <button type='button' class='btn btn-danger btn-sm' onclick='removerProdutoSaco(this)'>
                        <i class='fas fa-trash'></i>
                    </button>
                </div>
            `;
            container.appendChild(div);
        }

        function adicionarNovoProdutoSaco() {
            var index = produtosSacoAtuais.length;
            adicionarProdutoSacoNoModal('', 1, index);
        }

        function removerProdutoSaco(button) {
            button.closest('.produto-saco-item').remove();
        }

        function salvarProdutosSaco() {
            var container = document.getElementById('produtosSacoContainer');
            var itens = container.querySelectorAll('.produto-saco-item');
            var produtos = [];
            var todosPreenchidos = true;
            var produtosDisponiveis = window.produtosDisponiveis || [];
            
            itens.forEach(function(item) {
                var produtoId = item.querySelector('.produto-saco-select').value;
                var quantidade = parseInt(item.querySelector('.quantidade-produto-saco').value) || 1;
                
                if (produtoId && produtoId !== '') {
                    // Validar se o produto é permitido
                    var produto = produtosDisponiveis.find(p => (p.Id || p.id) == produtoId);
                    if (!produto) {
                        alert('Erro: O produto selecionado não é permitido neste saco.');
                        return;
                    }
                    produtos.push({ id: parseInt(produtoId), qt: quantidade });
                } else {
                    todosPreenchidos = false;
                }
            });
            
            if (!todosPreenchidos || produtos.length === 0) {
                alert('Por favor, selecione todos os produtos para o saco/cesta/caixa.');
                return;
            }
            
            // Atualizar o campo hidden do item
            var itemReserva = document.querySelectorAll('.item-reserva')[itemIndexEditando];
            if (itemReserva) {
                var produtosInput = itemReserva.querySelector('.produtos-item');
                if (produtosInput) {
                    produtosInput.value = JSON.stringify(produtos);
                    // Atualizar também a exibição do item sem recarregar a página
                    atualizarExibicaoProdutosSaco(itemReserva, produtos);
                }
            }
            
            // Fechar modal
            var modalElement = document.getElementById('modalEditarProdutosSaco');
            if (modalElement) {
                var modal = bootstrap.Modal.getInstance(modalElement);
                if (modal) {
                    modal.hide();
                } else {
                    modal = new bootstrap.Modal(modalElement);
                    modal.hide();
                }
            }
        }
        
        function atualizarExibicaoProdutosSaco(itemReserva, produtos) {
            // Buscar nomes dos produtos
            var produtosDisponiveis = window.produtosDisponiveis || [];
            var produtosDetalhes = [];
            
            produtos.forEach(function(produtoJson) {
                var produto = produtosDisponiveis.find(p => (p.Id || p.id) == produtoJson.id);
                if (produto) {
                    produtosDetalhes.push(produtoJson.qt + 'x ' + (produto.Nome || produto.nome));
                }
            });
            
            // Atualizar a exibição do item
            var nomeDiv = itemReserva.querySelector('.col-md-4');
            if (nomeDiv) {
                var nomeProduto = itemReserva.querySelector('.nome-produto').value;
                var tamanho = itemReserva.querySelector('.tamanho-item').value;
                var produtosHtml = produtosDetalhes.length > 0 
                    ? "<br/><small class='text-muted'>Produtos: " + produtosDetalhes.join(', ') + "</small>"
                    : "";
                nomeDiv.innerHTML = "<strong>" + nomeProduto + "</strong> (" + tamanho + ")" + produtosHtml;
            }
        }

        function atualizarNomeProdutoSaco(select) {
            // Pode ser usado para validação ou atualização de UI
        }
    </script>
    
    <!-- Modal para editar produtos do saco -->
    <div class='modal fade' id='modalEditarProdutosSaco' tabindex='-1'>
        <div class='modal-dialog'>
            <div class='modal-content'>
                <div class='modal-header'>
                    <h5 class='modal-title'>Editar Produtos do Saco/Cesta/Caixa</h5>
                    <button type='button' class='btn-close' data-bs-dismiss='modal'></button>
                </div>
                <div class='modal-body'>
                    <div id='produtosSacoContainer'></div>
                    <button type='button' class='btn btn-primary btn-sm mt-2' onclick='adicionarNovoProdutoSaco()'>
                        <i class='fas fa-plus'></i> Adicionar Produto
                    </button>
                </div>
                <div class='modal-footer'>
                    <button type='button' class='btn btn-secondary' data-bs-dismiss='modal'>Cancelar</button>
                    <button type='button' class='btn btn-primary' onclick='salvarProdutosSaco()'>Salvar</button>
                </div>
            </div>
        </div>
    </div>
    
    <!-- Scripts comuns da aplicação -->
    <script src="Scripts/app.js"></script>
</body>
</html>

