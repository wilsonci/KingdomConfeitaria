<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="KingdomConfeitaria.Default" %>
<%@ Register Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" Namespace="System.Web.UI" TagPrefix="asp" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Kingdom Confeitaria - Reserva de Ginger Breads</title>
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
            padding: 30px 20px;
            text-align: center;
            border-radius: 20px 20px 0 0;
            margin-bottom: 0;
            position: relative;
        }
        .header-actions {
            position: absolute;
            top: 20px;
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
            max-width: 400px;
            width: 100%;
            height: auto;
            display: block;
            margin: 0 auto;
        }
        .container-main {
            background: white;
            border-radius: 0 0 20px 20px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
            margin: 0 auto 20px auto;
            padding: 30px;
        }
        .produto-card {
            border: 2px solid #e9ecef;
            border-radius: 15px;
            padding: 20px;
            margin-bottom: 20px;
            transition: transform 0.3s, box-shadow 0.3s;
        }
        .produto-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 5px 20px rgba(0,0,0,0.1);
        }
        .produto-imagem {
            width: 100%;
            height: 200px;
            object-fit: cover;
            border-radius: 10px;
            margin-bottom: 15px;
        }
        .btn-tamanho {
            margin: 5px;
            min-width: 100px;
        }
        .btn-tamanho.active {
            background-color: #1a4d2e;
            border-color: #1a4d2e;
            color: white;
        }
        .carrinho-fixo {
            position: sticky;
            top: 20px;
            background: #f8f9fa;
            border-radius: 15px;
            padding: 20px;
            max-height: 80vh;
            overflow-y: auto;
        }
        .item-carrinho {
            background: white;
            padding: 15px;
            border-radius: 10px;
            margin-bottom: 10px;
            border-left: 4px solid #1a4d2e;
        }
        .total-carrinho {
            font-size: 1.5em;
            font-weight: bold;
            color: #1a4d2e;
            margin-top: 20px;
            padding-top: 20px;
            border-top: 2px solid #dee2e6;
        }
        .btn-reservar {
            background: linear-gradient(135deg, #1a4d2e 0%, #2d5a3d 100%);
            border: none;
            padding: 15px 30px;
            font-size: 1.2em;
            border-radius: 10px;
            color: white;
            width: 100%;
            margin-top: 20px;
            transition: all 0.3s;
        }
        .btn-reservar:hover {
            transform: scale(1.05);
            box-shadow: 0 5px 20px rgba(26, 77, 46, 0.4);
            background: linear-gradient(135deg, #2d5a3d 0%, #1a4d2e 100%);
        }
        .header-title {
            text-align: center;
            color: #1a4d2e;
            margin-bottom: 30px;
            font-size: 1.8em;
            font-weight: 600;
        }
        .subtitle {
            color: #6c757d;
            font-size: 0.9em;
            margin-top: 10px;
        }
        .quantidade-input {
            width: 80px;
            text-align: center;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true"></asp:ScriptManager>
        <div class="container-fluid">
            <div class="header-logo">
                <img id="logoImg" src="Images/logo-kingdom-confeitaria.png" alt="Kingdom Confeitaria" onerror="document.getElementById('logoFallback').style.display='block'; this.style.display='none';" />
                <h1 id="logoFallback" class="header-title" style="display: none; color: #d4af37; margin: 0;">
                    <i class="fas fa-crown"></i> Kingdom Confeitaria
                </h1>
            </div>
            <div class="container-main">
                <h2 class="header-title">
                    Reserve seus Ginger Breads
                </h2>

                <div class="row">
                    <div class="col-lg-7">
                        <h3 class="mb-3"><i class="fas fa-cookie-bite"></i> Produtos Disponíveis</h3>
                        <div id="produtosContainer" runat="server">
                            <!-- Produtos serão carregados aqui -->
                        </div>
                    </div>

                    <div class="col-lg-5">
                        <div class="carrinho-fixo">
                            <h3><i class="fas fa-shopping-cart"></i> Seu Pedido</h3>
                            <div id="carrinhoContainer" runat="server">
                                <p class="text-muted">Seu carrinho está vazio</p>
                            </div>
                            <div class="total-carrinho" id="totalContainer" runat="server" style="display: none;">
                                Total: R$ <span id="totalPedido" runat="server">0,00</span>
                            </div>
                            <asp:Button ID="btnFazerReserva" runat="server" 
                                Text="Fazer Reserva" 
                                CssClass="btn btn-reservar" 
                                OnClick="btnFazerReserva_Click" 
                                Enabled="false" />
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal de Reserva -->
        <div class="modal fade" id="modalReserva" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Finalizar Reserva</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <div class="mb-3">
                            <label class="form-label">Nome *</label>
                            <asp:TextBox ID="txtNome" runat="server" CssClass="form-control" required></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Email *</label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" required></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Telefone/WhatsApp *</label>
                            <asp:TextBox ID="txtTelefone" runat="server" CssClass="form-control" required></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Data de Retirada *</label>
                            <asp:DropDownList ID="ddlDataRetirada" runat="server" CssClass="form-select" required></asp:DropDownList>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Observações</label>
                            <asp:TextBox ID="txtObservacoes" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"></asp:TextBox>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                        <asp:Button ID="btnConfirmarReserva" runat="server" 
                            Text="Confirmar Reserva" 
                            CssClass="btn btn-primary" 
                            OnClick="btnConfirmarReserva_Click" />
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal de Sucesso -->
        <div class="modal fade" id="modalSucesso" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header bg-success text-white">
                        <h5 class="modal-title"><i class="fas fa-check-circle"></i> Reserva Confirmada!</h5>
                    </div>
                    <div class="modal-body">
                        <p>Sua reserva foi realizada com sucesso! Você receberá um email de confirmação em breve.</p>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-bs-dismiss="modal" onclick="location.reload();">OK</button>
                    </div>
                </div>
            </div>
        </div>
    </form>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script src="Scripts/site.js"></script>
    <script>
        function obterPrecoDoProduto(produtoId) {
            var tamanhoElement = document.getElementById('tamanho_' + produtoId);
            if (!tamanhoElement) {
                return null;
            }
            
            // Se for um select, pegar a opção selecionada
            if (tamanhoElement.tagName === 'SELECT') {
                var option = tamanhoElement.options[tamanhoElement.selectedIndex];
                if (option) {
                    return option.getAttribute('data-preco');
                }
            }
            // Se for um input hidden, pegar o atributo data-preco
            else if (tamanhoElement.tagName === 'INPUT') {
                return tamanhoElement.getAttribute('data-preco');
            }
            
            return null;
        }

        function adicionarAoCarrinho(produtoId, nome, tamanho, quantidade) {
            quantidade = quantidade || 1;
            
            // Obter o preço do elemento
            var preco = obterPrecoDoProduto(produtoId);
            
            // Validar e normalizar o preço
            if (!preco || preco === '' || preco === 'undefined' || preco === 'null') {
                alert('Erro: Preço inválido. Por favor, selecione um tamanho.');
                return;
            }
            
            // Garantir que o preço use ponto como separador decimal
            var precoNormalizado = String(preco).replace(',', '.').trim();
            
            // Validar se é um número válido
            if (isNaN(parseFloat(precoNormalizado))) {
                alert('Erro: Preço inválido: ' + preco);
                return;
            }
            
            // Obter o tamanho se não foi fornecido
            if (!tamanho) {
                var tamanhoElement = document.getElementById('tamanho_' + produtoId);
                if (tamanhoElement) {
                    tamanho = tamanhoElement.value;
                }
            }
            
            __doPostBack('AdicionarAoCarrinho', produtoId + '|' + nome + '|' + tamanho + '|' + precoNormalizado + '|' + quantidade);
        }

        function atualizarQuantidade(produtoId, tamanho, incremento) {
            __doPostBack('AtualizarQuantidade', produtoId + '|' + tamanho + '|' + incremento);
        }

        function removerItem(produtoId, tamanho) {
            __doPostBack('RemoverItem', produtoId + '|' + tamanho);
        }

        function aumentarQuantidade(produtoId) {
            var input = document.getElementById('quantidade_' + produtoId);
            var valor = parseInt(input.value) || 1;
            input.value = valor + 1;
        }

        function diminuirQuantidade(produtoId) {
            var input = document.getElementById('quantidade_' + produtoId);
            var valor = parseInt(input.value) || 1;
            if (valor > 1) {
                input.value = valor - 1;
            }
        }

        function atualizarPreco(produtoId) {
            var select = document.getElementById('tamanho_' + produtoId);
            if (select && select.tagName === 'SELECT') {
                var option = select.options[select.selectedIndex];
                if (option) {
                    var preco = parseFloat(option.getAttribute('data-preco'));
                    var precoElement = document.getElementById('precoUnitario_' + produtoId);
                    if (precoElement && !isNaN(preco)) {
                        precoElement.textContent = 'R$ ' + preco.toFixed(2).replace('.', ',');
                    }
                }
            }
        }

        function atualizarTotalSelecionado(sacoId) {
            var seletores = document.querySelectorAll('.seletor-produto-saco[data-saco-id="' + sacoId + '"]');
            var total = 0;
            seletores.forEach(function(select) {
                if (select.value && select.value !== '') {
                    total++;
                }
            });
            var totalElement = document.getElementById('totalSelecionado_' + sacoId);
            if (totalElement) {
                totalElement.textContent = total;
            }
        }

        function adicionarSacoAoCarrinho(sacoId, nomeSaco, tamanhoSaco, quantidadeMaxima) {
            var seletores = document.querySelectorAll('.seletor-produto-saco[data-saco-id="' + sacoId + '"]');
            var produtosSelecionados = [];
            var todosPreenchidos = true;
            
            seletores.forEach(function(select) {
                if (select.value && select.value !== '') {
                    produtosSelecionados.push(select.value);
                } else {
                    todosPreenchidos = false;
                }
            });
            
            if (!todosPreenchidos || produtosSelecionados.length !== quantidadeMaxima) {
                alert('Por favor, selecione todos os ' + quantidadeMaxima + ' biscoitos para o saco.');
                return;
            }
            
            // Obter o preço do saco
            var preco = obterPrecoDoProduto(sacoId);
            if (!preco) {
                alert('Erro: Preço inválido.');
                return;
            }
            
            var precoNormalizado = String(preco).replace(',', '.').trim();
            
            // Enviar os dados: sacoId|nomeSaco|tamanhoSaco|preco|quantidade|produtosSelecionados
            var quantidade = document.getElementById('quantidade_' + sacoId) ? document.getElementById('quantidade_' + sacoId).value : 1;
            var dados = sacoId + '|' + nomeSaco + '|' + tamanhoSaco + '|' + precoNormalizado + '|' + quantidade + '|' + produtosSelecionados.join(',');
            
            __doPostBack('AdicionarSacoAoCarrinho', dados);
        }
    </script>
</body>
</html>

