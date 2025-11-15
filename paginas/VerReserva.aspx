<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="VerReserva.aspx.cs" Inherits="KingdomConfeitaria.paginas.VerReserva" EnableViewState="false" ViewStateMode="Disabled" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Ver Reserva - Kingdom Confeitaria</title>
    <link href="../Content/bootstrap/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/fontawesome/all.min.css?v=2.0" rel="stylesheet" />
    <link href="../Content/app.css" rel="stylesheet" />
    <style>
        body {
            background: linear-gradient(135deg, #f5f7fa 0%, #e9ecef 100%);
            min-height: 100vh;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
        }
        .header-logo {
            background: #ffffff;
            padding: 16px 20px;
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            width: 100%;
            z-index: 1000;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            border-bottom: 1px solid #e0e0e0;
        }
        .header-top {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 8px;
        }
        .header-logo img {
            max-width: 200px;
            width: 100%;
            height: auto;
            display: block;
        }
        .header-user-name {
            color: white;
            font-weight: 600;
            font-size: 14px;
            text-align: right;
        }
        .header-actions {
            display: flex;
            gap: 8px;
            align-items: center;
            background: rgba(255, 255, 255, 0.95);
            padding: 8px 12px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            flex-wrap: wrap;
            justify-content: flex-end;
            max-width: 100%;
            overflow-x: auto;
            -webkit-overflow-scrolling: touch;
            scrollbar-width: none;
        }
        .header-actions::-webkit-scrollbar {
            display: none;
        }
        .header-actions a {
            color: #1a4d2e;
            text-decoration: none;
            font-size: 13px;
            font-weight: 600;
            padding: 6px 12px;
            border-radius: 6px;
            transition: all 0.2s;
            white-space: nowrap;
            flex-shrink: 0;
        }
        .header-actions a:hover {
            background: #1a4d2e;
            color: #fff;
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
        .status-em-produ��o { background-color: #ffc107; color: #000; }
        .status-produ��o-pronta { background-color: #17a2b8; color: #fff; }
        .status-preparando-entrega { background-color: #007bff; color: #fff; }
        .status-saiu-para-entrega { background-color: #6f42c1; color: #fff; }
        .status-j�-entregue { background-color: #6c757d; color: #fff; }
        .status-cancelado { background-color: #dc3545; color: #fff; }
        /* Compatibilidade com status antigos */
        .status-pendente { background-color: #28a745; color: #fff; }
        .status-confirmado { background-color: #17a2b8; color: #fff; }
        .status-pronto { background-color: #17a2b8; color: #fff; }
        .status-entregue { background-color: #6c757d; color: #fff; }
        .btn-share {
            margin: 5px;
        }
        /* Bot�o de voltar na lateral esquerda */
        .btn-voltar {
            position: fixed;
            left: 20px;
            top: 50%;
            transform: translateY(-50%);
            z-index: 999;
            background: #1a4d2e;
            color: white;
            border: none;
            border-radius: 50px;
            width: 60px;
            height: 60px;
            font-size: 1.5rem;
            box-shadow: 0 4px 15px rgba(0,0,0,0.3);
            transition: all 0.3s;
            display: flex;
            align-items: center;
            justify-content: center;
            text-decoration: none;
        }
        .btn-voltar:hover {
            background: #2d5a3d;
            transform: translateY(-50%) scale(1.1);
            box-shadow: 0 6px 20px rgba(0,0,0,0.4);
            color: white;
            text-decoration: none;
        }
        .btn-voltar i {
            margin-right: 0;
        }
        @media (max-width: 768px) {
            .header-logo {
                padding: 10px 12px;
            }
            .header-logo img {
                max-width: 180px;
                width: 100%;
            }
            .header-user-name {
                font-size: 12px;
            }
            .header-actions {
                gap: 4px;
                padding: 6px 8px;
                overflow-x: auto;
            }
            .header-actions a {
                font-size: 11px;
                padding: 4px 8px;
            }
            .header-actions a i {
                display: none;
            }
            .btn-voltar {
                left: 10px;
                width: 50px;
                height: 50px;
                font-size: 1.2rem;
            }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <!-- ScriptManager removido - usando Handler + fetch (JavaScript puro) -->
        <script src="../Scripts/ajax-helper.js"></script>
        <!-- Bot�o de voltar na lateral esquerda -->
        <a href="javascript:void(0);" class="btn-voltar" title="Voltar" onclick="voltarPagina(); return false;">
            <i class="fas fa-arrow-left"></i>
        </a>
        <div class="container-fluid">
            <div class="header-logo">
                <div class="header-top">
                    <a href="../Default.aspx" style="text-decoration: none; display: inline-block;">
                        <img src="../Images/logo-kingdom-confeitaria.svg" alt="Kingdom Confeitaria" style="cursor: pointer;" />
                    </a>
                    <div class="header-user-name" id="clienteNome" runat="server"></div>
                </div>
                <div class="header-actions">
                    <a href="../Default.aspx"><i class="fas fa-home"></i> Home</a>
                    <a href="#" id="linkLogin" runat="server" onclick="abrirModalLogin(); return false;"><i class="fas fa-sign-in-alt"></i> Entrar</a>
                    <a href="MinhasReservas.aspx" id="linkMinhasReservas" runat="server"><i class="fas fa-clipboard-list"></i> Minhas Reservas</a>
                    <a href="MeusDados.aspx" id="linkMeusDados" runat="server"><i class="fas fa-user"></i> Meus Dados</a>
                    <a href="Admin.aspx" id="linkAdmin" runat="server"><i class="fas fa-cog"></i> Painel Gestor</a>
                    <a href="Logout.aspx" id="linkLogout" runat="server"><i class="fas fa-sign-out-alt"></i> Sair</a>
                </div>
            </div>
            
            <div class="container-main">
                <div id="conteudoContainer" runat="server">
                    <!-- Conte�do ser� carregado aqui -->
                </div>
            </div>
        </div>
    </form>

    <script src="../Scripts/bootstrap/bootstrap.bundle.min.js"></script>
    <script>
        function voltarPagina() {
            if (window.history.length > 1) {
                window.history.back();
            } else {
                window.location.href = '../Default.aspx';
            }
        }
        
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

            // Criar formul�rio para submiss�o
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
            if (confirm('Deseja cancelar a edi��o? As altera��es n�o salvas ser�o perdidas.')) {
                window.location.reload();
            }
        }

        var produtosDisponiveis = [];
        var produtoSelecionado = null;

        // Carregar produtos dispon�veis ao carregar a p�gina e inicializar valor total
        window.addEventListener('DOMContentLoaded', function() {
            // Inicializar valor total com os itens existentes
            setTimeout(function() {
                atualizarValorTotal();
            }, 100);
            
            // Chamar Handler para obter produtos (sem ScriptManager)
            KingdomConfeitaria.Ajax.callHandler(
                'Handlers/CallbackHandler.ashx',
                {
                    acao: 'obterprodutosdisponiveis',
                    produtosPermitidosJson: null
                },
                'POST',
                function(result) {
                    if (result && !result.erro && Array.isArray(result)) {
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
                },
                function(error) {
                    console.error('Erro ao carregar produtos:', error);
                }
            );
        });

        function atualizarSubtotalItem(input) {
            var quantidade = parseInt(input.value) || 0;
            if (quantidade < 1) {
                quantidade = 1;
                input.value = 1;
            }
            
            var precoUnitarioStr = input.getAttribute('data-preco-unitario');
            // Garantir que o valor est� no formato correto (ponto como separador decimal)
            var precoUnitario = parseFloat(precoUnitarioStr.toString().replace(',', '.').replace(/[^\d.]/g, '')) || 0;
            var subtotal = quantidade * precoUnitario;
            
            var itemDiv = input.closest('.item-reserva');
            var subtotalInput = itemDiv.querySelector('.subtotal-item');
            if (subtotalInput) {
                subtotalInput.value = 'R$ ' + subtotal.toFixed(2).replace('.', ',');
            }
            
            // Atualizar tamb�m o campo hidden do pre�o unit�rio se necess�rio
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
                // Usar o pre�o �nico do produto
                var preco = parseFloat(produtoSelecionado.preco) || 0;
                var subtotal = preco * quantidade;
                precoInfo.textContent = 'Pre�o unit�rio: R$ ' + preco.toFixed(2).replace('.', ',') + ' | Subtotal: R$ ' + subtotal.toFixed(2).replace('.', ',');
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
                alert('Produto n�o encontrado.');
                return;
            }
            
            var tamanho = tamanhoSelect.value;
            var quantidade = parseInt(quantidadeInput.value) || 1;
            // Usar o pre�o �nico do produto
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
            // Garantir que o pre�o unit�rio est� no formato correto (ponto como separador decimal)
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
            
            // Limpar formul�rio
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
                    // Remover formata��o: "R$ ", espa�os, e substituir v�rgula por ponto
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

        // Inicializar event listeners para bot�es de editar produtos do saco
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
        
        // Inicializar quando a p�gina carregar
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', inicializarBotoesEditarProdutosSaco);
        } else {
            inicializarBotoesEditarProdutosSaco();
        }
        
        // Re-inicializar ap�s postback
        window.addEventListener('pageshow', function(event) {
            inicializarBotoesEditarProdutosSaco();
        });

        // Atualizar fun��o salvarReserva para incluir itens
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

            // Criar formul�rio para submiss�o
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
                    // Garantir que o pre�o est� no formato correto (ponto como separador decimal)
                    var precoUnitarioValue = precoUnitarioInput ? precoUnitarioInput.value.toString() : '0';
                    // Remover qualquer formata��o e garantir ponto como separador decimal
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
            itemIndexEditando = itemIndex;
            produtosPermitidosAtuais = produtosPermitidos || '';
            
            try {
                if (!produtosJson || produtosJson === '' || produtosJson === 'null' || produtosJson === 'undefined') {
                    produtosSacoAtuais = [];
                } else {
                    // produtosJson j� vem como string do data-attribute
                    produtosSacoAtuais = JSON.parse(produtosJson);
                }
            } catch (e) {
                alert('Erro ao carregar produtos: ' + e.message);
                produtosSacoAtuais = [];
            }
            
            // Carregar produtos dispon�veis filtrados por IDs permitidos
            // Chamar Handler para obter produtos (sem ScriptManager)
            KingdomConfeitaria.Ajax.callHandler(
                'Handlers/CallbackHandler.ashx',
                {
                    acao: 'obterprodutosdisponiveis',
                    produtosPermitidosJson: produtosPermitidosAtuais ? JSON.stringify(produtosPermitidosAtuais) : null
                },
                'POST',
                function(result) {
                    if (result && !result.erro) {
                        window.produtosDisponiveis = result;
                        preencherModalProdutosSaco();
                    } else {
                        alert('Erro ao carregar produtos dispon�veis.');
                    }
                },
                function(error) {
                    console.error('Erro ao carregar produtos:', error);
                    // Fallback: usar produtos j� carregados e filtrar
                    if (window.produtosDisponiveis && window.produtosDisponiveis.length > 0) {
                        filtrarProdutosPorIds(produtosPermitidosAtuais);
                        preencherModalProdutosSaco();
                    } else {
                        alert('Erro: Produtos n�o carregados. Por favor, recarregue a p�gina.');
                    }
                }
            );
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
                // Erro ao filtrar produtos por IDs
            }
        }
        
        function preencherModalProdutosSaco() {
            // Preencher modal com produtos atuais
            var container = document.getElementById('produtosSacoContainer');
            if (!container) {
                alert('Erro: Container de produtos n�o encontrado.');
                return;
            }
            container.innerHTML = '';
            
            if (produtosSacoAtuais && produtosSacoAtuais.length > 0) {
                produtosSacoAtuais.forEach(function(produto, index) {
                    adicionarProdutoSacoNoModal(produto.id, produto.qt, index);
                });
            } else {
                // Se n�o houver produtos, adicionar um campo vazio
                adicionarProdutoSacoNoModal('', 1, 0);
            }
            
            // Verificar se Bootstrap est� dispon�vel
            if (typeof bootstrap === 'undefined') {
                alert('Erro: Bootstrap n�o est� carregado. Por favor, recarregue a p�gina.');
                return;
            }
            
            // Mostrar modal
            var modalElement = document.getElementById('modalEditarProdutosSaco');
            if (!modalElement) {
                alert('Erro: Modal n�o encontrado.');
                return;
            }
            
            try {
                if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                    var modal = bootstrap.Modal.getOrCreateInstance(modalElement);
                    modal.show();
                } else {
                    modalElement.classList.add('show');
                    modalElement.style.display = 'block';
                    modalElement.setAttribute('aria-hidden', 'false');
                    document.body.classList.add('modal-open');
                    var backdrop = document.createElement('div');
                    backdrop.className = 'modal-backdrop fade show';
                    document.body.appendChild(backdrop);
                }
            } catch (e) {
                alert('Erro ao abrir modal: ' + e.message);
            }
        }

        function adicionarProdutoSacoNoModal(produtoId, quantidade, index) {
            var container = document.getElementById('produtosSacoContainer');
            var produtosDisponiveis = window.produtosDisponiveis || [];
            
            // Filtrar produtos por IDs permitidos se necess�rio
            if (produtosPermitidosAtuais) {
                try {
                    var produtosIds = JSON.parse(produtosPermitidosAtuais);
                    produtosDisponiveis = produtosDisponiveis.filter(function(p) {
                        var pId = p.Id || p.id;
                        return produtosIds.indexOf(pId) !== -1;
                    });
                } catch (e) {
                    // Erro ao filtrar produtos por IDs
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
                    // Validar se o produto � permitido
                    var produto = produtosDisponiveis.find(p => (p.Id || p.id) == produtoId);
                    if (!produto) {
                        alert('Erro: O produto selecionado n�o � permitido neste saco.');
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
                    // Atualizar tamb�m a exibi��o do item sem recarregar a p�gina
                    atualizarExibicaoProdutosSaco(itemReserva, produtos);
                }
            }
            
            // Fechar modal
            var modalElement = document.getElementById('modalEditarProdutosSaco');
            if (modalElement) {
                if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                    var modal = bootstrap.Modal.getInstance(modalElement);
                    if (modal) {
                        modal.hide();
                    } else {
                        modal = new bootstrap.Modal(modalElement);
                        modal.hide();
                    }
                } else {
                    modalElement.classList.remove('show');
                    modalElement.style.display = 'none';
                    modalElement.setAttribute('aria-hidden', 'true');
                    document.body.classList.remove('modal-open');
                    var backdrop = document.querySelector('.modal-backdrop');
                    if (backdrop) backdrop.remove();
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
            
            // Atualizar a exibi��o do item
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
            // Pode ser usado para valida��o ou atualiza��o de UI
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
    
    <!-- Scripts comuns da aplica��o -->
    <script src="../Scripts/app.js"></script>
</body>
</html>

