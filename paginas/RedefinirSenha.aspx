<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RedefinirSenha.aspx.cs" Inherits="KingdomConfeitaria.paginas.RedefinirSenha" EnableViewState="false" ViewStateMode="Disabled" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Redefinir Senha - Kingdom Confeitaria</title>
    <link href="../Content/bootstrap/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/fontawesome/all.min.css?v=2.0" rel="stylesheet" />
    <link href="../Content/app.css" rel="stylesheet" />
    <style>
        body {
            background: linear-gradient(135deg, #f5f7fa 0%, #e9ecef 100%);
            min-height: 100vh;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            padding-top: 90px;
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
        <!-- Bot�o de voltar na lateral esquerda -->
        <a href="javascript:void(0);" class="btn-voltar" title="Voltar" onclick="voltarPagina(); return false;">
            <i class="fas fa-arrow-left"></i>
        </a>
        <div class="header-logo">
            <div class="header-top">
                <a href="../Default.aspx" style="text-decoration: none; display: inline-block;">
                    <img src="../Images/logo-kingdom-confeitaria.svg" alt="Kingdom Confeitaria" style="cursor: pointer;" />
                </a>
            </div>
            <div class="header-actions">
                <a href="../Default.aspx"><i class="fas fa-home"></i> Home</a>
            </div>
        </div>
        
        <div class="reset-container">
            <h2 class="text-center mb-4" style="color: #1a4d2e;">Redefinir Senha</h2>
            
            <div id="alertContainer" runat="server"></div>
            
            <div class="mb-3">
                <label for="txtNovaSenha" class="form-label">Nova Senha</label>
                <input type="password" class="form-control form-control-lg" id="txtNovaSenha" runat="server" required minlength="6" style="font-size: 16px; padding: 12px;" />
                <div class="password-strength text-muted">M�nimo de 6 caracteres</div>
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

    <script src="../Scripts/bootstrap/bootstrap.bundle.min.js"></script>
    <script>
        function voltarPagina() {
            if (window.history.length > 1) {
                window.history.back();
            } else {
                window.location.href = '../Default.aspx';
            }
        }
    </script>
    <script>
        // Validar se as senhas coincidem
        document.addEventListener('DOMContentLoaded', function() {
            var txtNovaSenha = document.getElementById('<%= txtNovaSenha.ClientID %>');
            var txtConfirmarSenha = document.getElementById('<%= txtConfirmarSenha.ClientID %>');
            var btnRedefinir = document.getElementById('<%= btnRedefinir.ClientID %>');

            function validarSenhas() {
                if (txtNovaSenha.value !== txtConfirmarSenha.value) {
                    txtConfirmarSenha.setCustomValidity('As senhas n�o coincidem');
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

