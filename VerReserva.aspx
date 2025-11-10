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
            max-width: 300px;
            width: 100%;
            height: auto;
        }
        .container-main {
            background: white;
            border-radius: 0 0 20px 20px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
            margin: 0 auto 20px auto;
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
        .status-pendente { background-color: #ffc107; color: #000; }
        .status-confirmado { background-color: #17a2b8; color: #fff; }
        .status-pronto { background-color: #28a745; color: #fff; }
        .status-entregue { background-color: #6c757d; color: #fff; }
        .status-cancelado { background-color: #dc3545; color: #fff; }
        .btn-share {
            margin: 5px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container-fluid">
            <div class="header-logo">
                <div class="header-actions">
                    <span id="clienteNome" runat="server" style="color: white; margin-right: 15px;"></span>
                    <a href="Default.aspx">Nova Reserva</a>
                    <a href="MinhasReservas.aspx" id="linkMinhasReservas" runat="server" style="display:none;">Minhas Reservas</a>
                    <a href="Login.aspx" id="linkLogin" runat="server">Entrar</a>
                    <a href="Logout.aspx" id="linkLogout" runat="server" style="display:none;">Sair</a>
                </div>
                <img src="Images/logo-kingdom-confeitaria.png" alt="Kingdom Confeitaria" onerror="this.style.display='none'; this.nextElementSibling.style.display='block';" />
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
    </script>
</body>
</html>

