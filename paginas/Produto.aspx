<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Produto.aspx.cs" Inherits="KingdomConfeitaria.paginas.Produto" EnableViewState="false" ViewStateMode="Disabled" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Detalhes do Produto - Kingdom Confeitaria</title>
    <link href="../Content/bootstrap/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/fontawesome/all.min.css?v=2.0" rel="stylesheet" />
    <link href="../Content/app.css" rel="stylesheet" />
    <style>
        body {
            background: linear-gradient(135deg, #f5f7fa 0%, #e9ecef 100%);
            min-height: 100vh;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            padding: 20px;
            padding-top: 100px;
        }
        .container-produto {
            max-width: 800px;
            margin: 0 auto;
            background: white;
            border-radius: 12px;
            box-shadow: 0 4px 16px rgba(0,0,0,0.1);
            padding: 30px;
        }
        .btn-voltar {
            position: fixed;
            left: 20px;
            top: 50%;
            transform: translateY(-50%);
            z-index: 1000;
            background: linear-gradient(135deg, #1a4d2e 0%, #2d5a3d 100%);
            color: white;
            border: none;
            width: 50px;
            height: 50px;
            border-radius: 50%;
            font-size: 20px;
            cursor: pointer;
            box-shadow: 0 4px 12px rgba(26, 77, 46, 0.3);
            transition: all 0.3s;
        }
        .btn-voltar:hover {
            transform: translateY(-50%) scale(1.1);
            box-shadow: 0 6px 16px rgba(26, 77, 46, 0.4);
        }
        .produto-imagem {
            width: 100%;
            max-width: 400px;
            height: auto;
            border-radius: 8px;
            margin: 0 auto 20px;
            display: block;
        }
        .produto-nome {
            font-size: 28px;
            font-weight: 700;
            color: #1a4d2e;
            margin-bottom: 15px;
        }
        .produto-descricao {
            color: #666;
            margin-bottom: 20px;
            line-height: 1.6;
        }
        .produto-preco {
            font-size: 24px;
            font-weight: 700;
            color: #1a4d2e;
            margin-bottom: 20px;
        }
        .quantidade-controls {
            display: flex;
            align-items: center;
            gap: 15px;
            margin: 20px 0;
        }
        .btn-quantidade {
            width: 40px;
            height: 40px;
            border: 2px solid #1a4d2e;
            background: white;
            border-radius: 8px;
            cursor: pointer;
            font-size: 20px;
            font-weight: 700;
            color: #1a4d2e;
            transition: all 0.2s;
        }
        .btn-quantidade:hover {
            background: #1a4d2e;
            color: white;
        }
        .quantidade-value {
            font-size: 24px;
            font-weight: 700;
            min-width: 40px;
            text-align: center;
        }
        .preco-total {
            margin: 20px 0;
            padding: 20px;
            background: #1a4d2e;
            color: white;
            border-radius: 8px;
            font-size: 24px;
            font-weight: 700;
            text-align: center;
        }
        .saco-produtos-selector {
            margin: 20px 0;
            padding: 20px;
            background: #f8f9fa;
            border-radius: 8px;
        }
        .btn-actions {
            display: flex;
            gap: 12px;
            justify-content: center;
            margin-top: 30px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <!-- Botão flutuante de voltar -->
        <button type="button" class="btn-voltar" onclick="voltarPagina()" title="Voltar">
            <i class="fas fa-arrow-left"></i>
        </button>
        
        <!-- Header com menu -->
        <div class="header-logo" style="position: fixed; top: 0; left: 0; right: 0; z-index: 1000; background: #ffffff; padding: 16px 20px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); border-bottom: 1px solid #e0e0e0;">
            <div class="header-top" style="display: flex; justify-content: space-between; align-items: center; width: 100%; gap: 12px; flex-wrap: wrap;">
                <a href="../Default.aspx" style="text-decoration: none; display: inline-block;">
                    <img src="../Images/logo-kingdom-confeitaria.svg" alt="Kingdom Confeitaria" style="max-width: 200px; width: 100%; height: auto; display: block; cursor: pointer;" />
                </a>
                <div class="header-user-name" id="clienteNome" runat="server" style="color: #1a4d2e; font-weight: 600; font-size: 14px; text-align: right;"></div>
            </div>
            <div class="header-actions" style="display: flex; gap: 8px; align-items: center; background: rgba(255, 255, 255, 0.95); padding: 8px 12px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); flex-wrap: wrap; justify-content: flex-end; max-width: 100%; overflow-x: auto; margin-top: 8px;">
                <a href="../Default.aspx" style="color: #1a4d2e; text-decoration: none; font-size: 13px; font-weight: 600; padding: 6px 12px; border-radius: 6px; transition: all 0.2s; white-space: nowrap;"><i class="fas fa-home"></i> Home</a>
                <a href="Login.aspx" id="linkLogin" runat="server" style="color: #1a4d2e; text-decoration: none; font-size: 13px; font-weight: 600; padding: 6px 12px; border-radius: 6px; transition: all 0.2s; white-space: nowrap;"><i class="fas fa-sign-in-alt"></i> Entrar</a>
                <a href="MinhasReservas.aspx" id="linkMinhasReservas" runat="server" style="color: #1a4d2e; text-decoration: none; font-size: 13px; font-weight: 600; padding: 6px 12px; border-radius: 6px; transition: all 0.2s; white-space: nowrap;"><i class="fas fa-clipboard-list"></i> Minhas Reservas</a>
                <a href="MeusDados.aspx" id="linkMeusDados" runat="server" style="color: #1a4d2e; text-decoration: none; font-size: 13px; font-weight: 600; padding: 6px 12px; border-radius: 6px; transition: all 0.2s; white-space: nowrap;"><i class="fas fa-user"></i> Meus Dados</a>
                <a href="Admin.aspx" id="linkAdmin" runat="server" style="color: #1a4d2e; text-decoration: none; font-size: 13px; font-weight: 600; padding: 6px 12px; border-radius: 6px; transition: all 0.2s; white-space: nowrap;"><i class="fas fa-cog"></i> Painel Gestor</a>
                <a href="Logout.aspx" id="linkLogout" runat="server" style="color: #1a4d2e; text-decoration: none; font-size: 13px; font-weight: 600; padding: 6px 12px; border-radius: 6px; transition: all 0.2s; white-space: nowrap;"><i class="fas fa-sign-out-alt"></i> Sair</a>
            </div>
        </div>

        <div class="container-produto" style="margin-top: 120px;">
            <div id="conteudoProduto" runat="server">
                <!-- Conteúdo será preenchido via code-behind -->
            </div>
        </div>
    </form>

    <script src="../Scripts/bootstrap/bootstrap.bundle.min.js" defer></script>
    <script src="../Scripts/ajax-helper.js" defer></script>
    <script src="../Scripts/app.js" defer></script>
    <script src="../Scripts/navigation.js" defer></script>
    <script>
        var produtoAtual = null;
        var quantidadeAtual = 1;

        function voltarPagina() {
            if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Navigation) {
                KingdomConfeitaria.Navigation.voltar();
            } else {
                if (window.history.length > 1) {
                    window.history.back();
                } else {
                    window.location.href = '../Default.aspx';
                }
            }
        }

        function aumentarQuantidade() {
            if (!produtoAtual) return;
            quantidadeAtual++;
            atualizarQuantidade();
        }

        function diminuirQuantidade() {
            if (!produtoAtual) return;
            if (quantidadeAtual > 1) {
                quantidadeAtual--;
                atualizarQuantidade();
            }
        }

        function atualizarQuantidade() {
            if (!produtoAtual) return;
            
            var qtdElement = document.getElementById('quantidadeValue');
            var valorTotalElement = document.getElementById('valorTotal');
            
            if (qtdElement) {
                qtdElement.textContent = quantidadeAtual;
            }
            
            if (valorTotalElement) {
                var precoUnitario = parseFloat(produtoAtual.preco);
                var total = precoUnitario * quantidadeAtual;
                valorTotalElement.textContent = total.toFixed(2).replace('.', ',');
            }
        }

        function adicionarAoCarrinho() {
            if (!produtoAtual) return;
            
            // Se for saco promocional, usar lógica específica
            if (produtoAtual.ehSaco && produtoAtual.produtosPermitidos && produtoAtual.produtosPermitidos.length > 0) {
                adicionarSacoAoCarrinho();
                return;
            }
            
            var tamanho = produtoAtual.tamanho || 'Único';
            var nome = produtoAtual.nome;
            var preco = produtoAtual.preco;
            var quantidade = quantidadeAtual;
            
            // Validar preço
            if (!preco || preco === '' || preco === 'undefined' || preco === 'null') {
                alert('Erro: Preço inválido. Por favor, verifique os dados do produto.');
                return;
            }
            
            // Adicionar ao carrinho via postback
            if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Utils) {
                KingdomConfeitaria.Utils.postBack('AdicionarAoCarrinho', produtoAtual.id + '|' + nome + '|' + tamanho + '|' + preco + '|' + quantidade);
            }
        }

        function adicionarSacoAoCarrinho() {
            if (!produtoAtual) return;
            
            var seletores = document.querySelectorAll('.seletor-produto-saco');
            var produtosSelecionados = [];
            var todosPreenchidos = true;
            
            seletores.forEach(function(select) {
                if (select.value && select.value !== '') {
                    produtosSelecionados.push(select.value);
                } else {
                    todosPreenchidos = false;
                }
            });
            
            if (!todosPreenchidos || produtosSelecionados.length !== produtoAtual.quantidadeSaco) {
                alert('Por favor, selecione todos os ' + produtoAtual.quantidadeSaco + ' produtos para o saco/cesta/caixa.');
                return;
            }
            
            var preco = produtoAtual.preco;
            var precoNormalizado = String(preco).replace(',', '.').trim();
            var dados = produtoAtual.id + '|' + produtoAtual.nome + '|' + precoNormalizado + '|' + quantidadeAtual + '|' + produtosSelecionados.join(',');
            
            if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Utils) {
                KingdomConfeitaria.Utils.postBack('AdicionarSacoAoCarrinho', dados);
            }
        }

        // Carregar produto da URL
        KingdomConfeitaria.Utils.ready(function() {
            var produtoJson = document.getElementById('hdnProdutoJson');
            if (produtoJson && produtoJson.value) {
                try {
                    produtoAtual = JSON.parse(produtoJson.value);
                    quantidadeAtual = 1;
                } catch (e) {
                    console.error('Erro ao parsear produto:', e);
                }
            }
        });
    </script>
</body>
</html>

