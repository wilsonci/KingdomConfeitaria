<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RedefinirSenha.aspx.cs" Inherits="KingdomConfeitaria.RedefinirSenha" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Redefinir Senha - Kingdom Confeitaria</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" rel="stylesheet" />
    <style>
        body {
            background: linear-gradient(135deg, #1a4d2e 0%, #2d5a3d 100%);
            min-height: 100vh;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            padding-top: 90px;
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
            font-weight: 500;
        }
        .header-actions a:hover {
            text-decoration: underline;
            color: #d4af37;
        }
        .header-logo img {
            max-width: 20%;
            width: auto;
            height: auto;
            max-height: 80px;
            display: block;
            margin: 0 auto;
        }
        .reset-container {
            background: white;
            border-radius: 20px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
            padding: 40px;
            max-width: 450px;
            width: 100%;
            margin: 0 auto;
        }
        .password-strength {
            font-size: 0.875em;
            margin-top: 0.25rem;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="header-logo">
            <div class="header-actions">
                <a href="Default.aspx"><i class="fas fa-home"></i> Home</a>
            </div>
            <img src="Images/logo-kingdom-confeitaria.svg" alt="Kingdom Confeitaria" />
        </div>
        
        <div class="reset-container">
            <h2 class="text-center mb-4" style="color: #1a4d2e;">Redefinir Senha</h2>
            
            <div id="alertContainer" runat="server"></div>
            
            <div class="mb-3">
                <label for="txtNovaSenha" class="form-label">Nova Senha</label>
                <input type="password" class="form-control form-control-lg" id="txtNovaSenha" runat="server" required minlength="6" style="font-size: 16px; padding: 12px;" />
                <div class="password-strength text-muted">Mínimo de 6 caracteres</div>
            </div>
            
            <div class="mb-3">
                <label for="txtConfirmarSenha" class="form-label">Confirmar Nova Senha</label>
                <input type="password" class="form-control form-control-lg" id="txtConfirmarSenha" runat="server" required minlength="6" style="font-size: 16px; padding: 12px;" />
            </div>
            
            <div class="d-grid mb-3">
                <asp:Button ID="btnRedefinir" runat="server" Text="Redefinir Senha" CssClass="btn btn-success btn-lg" OnClick="btnRedefinir_Click" />
            </div>
            
            <div class="text-center">
                <a href="Login.aspx" class="text-decoration-none">Voltar para o login</a>
            </div>
        </div>
    </form>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script>
        // Validar se as senhas coincidem
        document.addEventListener('DOMContentLoaded', function() {
            var txtNovaSenha = document.getElementById('<%= txtNovaSenha.ClientID %>');
            var txtConfirmarSenha = document.getElementById('<%= txtConfirmarSenha.ClientID %>');
            var btnRedefinir = document.getElementById('<%= btnRedefinir.ClientID %>');

            function validarSenhas() {
                if (txtNovaSenha.value !== txtConfirmarSenha.value) {
                    txtConfirmarSenha.setCustomValidity('As senhas não coincidem');
                } else {
                    txtConfirmarSenha.setCustomValidity('');
                }
            }

            if (txtNovaSenha && txtConfirmarSenha) {
                txtNovaSenha.addEventListener('input', validarSenhas);
                txtConfirmarSenha.addEventListener('input', validarSenhas);
            }
        });
    </script>
</body>
</html>

