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
                    <span id="clienteNome" runat="server" style="color: white; margin-right: 15px;"></span>
                    <a href="Default.aspx">Nova Reserva</a>
                    <a href="MinhasReservas.aspx" id="linkMinhasReservas" runat="server" style="display:none;">Minhas Reservas</a>
                    <a href="Login.aspx" id="linkLogin" runat="server">Entrar</a>
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
                        var select = document.getElementById('novoItemProduto');
                        if (select) {
                            result.forEach(function(produto) {
                                var option = document.createElement('option');
                                option.value = produto.id;
                                option.textContent = produto.nome;
                                option.setAttribute('data-preco-pequeno', produto.precoPequeno);
                                option.setAttribute('data-preco-grande', produto.precoGrande);
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
            
            if (produtoSelecionado && tamanho) {
                // Garantir que os preços são números
                var precoPequeno = parseFloat(produtoSelecionado.precoPequeno) || 0;
                var precoGrande = parseFloat(produtoSelecionado.precoGrande) || 0;
                var preco = tamanho === 'Pequeno' ? precoPequeno : precoGrande;
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
            // Garantir que os preços são números, não strings
            var precoPequeno = parseFloat(produto.precoPequeno) || 0;
            var precoGrande = parseFloat(produto.precoGrande) || 0;
            var precoUnitario = tamanho === 'Pequeno' ? precoPequeno : precoGrande;
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
                    
                    form.appendChild(createHiddenInput('itens[' + index + '].ProdutoId', produtoId));
                    form.appendChild(createHiddenInput('itens[' + index + '].NomeProduto', nomeProduto));
                    form.appendChild(createHiddenInput('itens[' + index + '].Tamanho', tamanho));
                    form.appendChild(createHiddenInput('itens[' + index + '].Quantidade', quantidade));
                    form.appendChild(createHiddenInput('itens[' + index + '].PrecoUnitario', precoUnitario));
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
    </script>
</body>
</html>

