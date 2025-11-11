<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="KingdomConfeitaria.Login" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Login - Kingdom Confeitaria</title>
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
        .login-container {
            background: white;
            border-radius: 20px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
            padding: 40px;
            max-width: 450px;
            width: 100%;
        }
        .header-logo {
            text-align: center;
            margin-bottom: 20px;
            width: 100%;
            padding-top: 0;
        }
        .header-logo img {
            max-width: 20%;
            width: auto;
            height: auto;
            max-height: 80px;
            display: block;
            margin: 0 auto;
        }
        .is-valid {
            border-color: #28a745;
        }
        .is-invalid {
            border-color: #dc3545;
        }
        .invalid-feedback {
            display: block;
            width: 100%;
            margin-top: 0.25rem;
            font-size: 0.875em;
            color: #dc3545;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-container">
            <div class="header-logo">
                <img src="Images/logo-kingdom-confeitaria.svg" alt="Kingdom Confeitaria" style="max-width: 100%; height: auto;" />
            </div>
            
            <h2 class="text-center mb-4" style="color: #1a4d2e;">Entrar</h2>
            
            <div id="alertContainer" runat="server"></div>
            
            <div class="mb-3">
                <label for="txtEmail" class="form-label">Email</label>
                <input type="email" class="form-control form-control-lg" id="txtEmail" runat="server" required style="font-size: 16px; padding: 12px;" />
            </div>
            
            <div class="mb-3">
                <label for="txtSenha" class="form-label">Senha</label>
                <input type="password" class="form-control form-control-lg" id="txtSenha" runat="server" required style="font-size: 16px; padding: 12px;" />
            </div>
            
            <div class="mb-3 text-end">
                <a href="RecuperarSenha.aspx" class="text-decoration-none">Esqueci minha senha</a>
            </div>
            
            <div class="d-grid mb-3">
                <asp:Button ID="btnEntrar" runat="server" Text="Entrar" CssClass="btn btn-success btn-lg" OnClick="btnEntrar_Click" />
            </div>
            
            <div class="text-center mb-3">
                <a href="Cadastro.aspx" class="text-decoration-none">Não tem conta? Cadastre-se</a>
            </div>
            
            <div class="text-center">
                <a href="Default.aspx" class="text-decoration-none">Voltar para produtos</a>
            </div>
        </div>
    </form>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <!-- Scripts comuns da aplicação -->
    <script src="Scripts/app.js"></script>
</body>
</html>
