<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ConfirmarCadastro.aspx.cs" Inherits="KingdomConfeitaria.ConfirmarCadastro" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Confirmar Cadastro - Kingdom Confeitaria</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" rel="stylesheet" />
    <style>
        body {
            background: linear-gradient(135deg, #1a4d2e 0%, #2d5a3d 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }
        .confirm-container {
            background: white;
            border-radius: 20px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
            padding: 40px;
            max-width: 500px;
            width: 100%;
            text-align: center;
        }
        .header-logo {
            text-align: center;
            margin-bottom: 30px;
        }
        .header-logo img {
            max-width: 300px;
            width: 100%;
            height: auto;
        }
        .success-icon {
            font-size: 80px;
            color: #28a745;
            margin-bottom: 20px;
        }
        .error-icon {
            font-size: 80px;
            color: #dc3545;
            margin-bottom: 20px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="confirm-container">
            <div class="header-logo">
                <img src="Images/logo-kingdom-confeitaria.png" alt="Kingdom Confeitaria" onerror="this.style.display='none'; this.nextElementSibling.style.display='block';" />
                <h1 style="display:none; color: #1a4d2e;">Kingdom Confeitaria</h1>
            </div>
            
            <div id="conteudoContainer" runat="server">
                <!-- Conteúdo será carregado aqui -->
            </div>
        </div>
    </form>
</body>
</html>

