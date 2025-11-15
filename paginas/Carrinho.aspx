<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Carrinho.aspx.cs" Inherits="KingdomConfeitaria.paginas.Carrinho" EnableViewState="false" ViewStateMode="Disabled" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Meu Carrinho - Kingdom Confeitaria</title>
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
        .container-carrinho {
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
        .item-carrinho {
            padding: 15px;
            border-bottom: 1px solid #e9ecef;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }
        .item-info {
            flex: 1;
        }
        .item-nome {
            font-weight: 600;
            color: #1a4d2e;
        }
        .item-detalhes {
            color: #666;
            font-size: 14px;
        }
        .item-acoes {
            display: flex;
            align-items: center;
            gap: 10px;
        }
        .total-pedido {
            margin-top: 20px;
            padding: 20px;
            background: #f8f9fa;
            border-radius: 8px;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }
        .total-pedido strong {
            font-size: 24px;
            color: #1a4d2e;
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

        <div class="container-carrinho" style="margin-top: 120px;">
            <h2 class="mb-4">
                <i class="fas fa-shopping-cart"></i> Meu Carrinho
            </h2>

            <div id="carrinhoContainer" runat="server">
                <!-- Conteúdo será preenchido via code-behind -->
            </div>

            <div class="total-pedido">
                <strong>Total:</strong>
                <strong id="totalPedido" runat="server">R$ 0,00</strong>
            </div>

            <div class="d-flex gap-2 justify-content-center mt-4">
                <button type="button" class="btn btn-secondary" onclick="voltarPagina()">
                    <i class="fas fa-arrow-left"></i> Continuar Comprando
                </button>
                <asp:Button ID="btnFazerReserva" runat="server" 
                    CssClass="btn btn-primary" 
                    OnClick="btnFazerReserva_Click"
                    Text="Fazer Reserva" />
            </div>
        </div>
    </form>

    <script src="../Scripts/bootstrap/bootstrap.bundle.min.js" defer></script>
    <script src="../Scripts/ajax-helper.js" defer></script>
    <script src="../Scripts/app.js" defer></script>
    <script src="../Scripts/navigation.js" defer></script>
    <script>
        // Adicionar ícone ao botão de reserva após carregar a página
        (function() {
            function adicionarIconeBotaoReserva() {
                var btnReserva = document.getElementById('<%= btnFazerReserva.ClientID %>');
                if (btnReserva && !btnReserva.querySelector('i')) {
                    var icon = document.createElement('i');
                    icon.className = 'fas fa-calendar-check';
                    btnReserva.insertBefore(icon, btnReserva.firstChild);
                    btnReserva.insertBefore(document.createTextNode(' '), btnReserva.childNodes[1]);
                }
            }
            
            // Tentar adicionar quando o DOM estiver pronto
            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', adicionarIconeBotaoReserva);
            } else {
                adicionarIconeBotaoReserva();
            }
            
            // Tentar novamente após um pequeno delay (caso o botão seja renderizado depois)
            setTimeout(adicionarIconeBotaoReserva, 100);
        })();
        
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

        function aumentarQuantidade(produtoId, tamanho) {
            if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Utils) {
                KingdomConfeitaria.Utils.postBack('AtualizarQuantidade', produtoId + '|' + tamanho + '|1');
            }
        }

        function diminuirQuantidade(produtoId, tamanho) {
            if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Utils) {
                KingdomConfeitaria.Utils.postBack('AtualizarQuantidade', produtoId + '|' + tamanho + '|-1');
            }
        }

        function removerItem(produtoId, tamanho) {
            if (confirm('Deseja remover este item do carrinho?')) {
                if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Utils) {
                    KingdomConfeitaria.Utils.postBack('RemoverItem', produtoId + '|' + tamanho);
                }
            }
        }
    </script>
</body>
</html>

