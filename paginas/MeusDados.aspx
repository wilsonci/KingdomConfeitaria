<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MeusDados.aspx.cs" Inherits="KingdomConfeitaria.paginas.MeusDados" EnableViewState="false" ViewStateMode="Disabled" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Meus Dados - Kingdom Confeitaria</title>
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
        .header-user-name {
            color: #1a4d2e;
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
        .header-user-name {
            color: #1a4d2e;
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
        .dados-container {
            background: white;
            border-radius: 20px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
            padding: 40px;
            max-width: 450px;
            width: 100%;
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
        .valid-feedback {
            display: block;
            width: 100%;
            margin-top: 0.25rem;
            font-size: 0.875em;
            color: #28a745;
        }
        /* Botão de voltar na lateral esquerda */
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
        <!-- Botão de voltar na lateral esquerda -->
        <a href="javascript:void(0);" class="btn-voltar" title="Voltar" onclick="voltarPagina(); return false;">
            <i class="fas fa-arrow-left"></i>
        </a>
        <div class="header-logo">
            <div class="header-top">
                <a href="../Default.aspx" style="text-decoration: none; display: inline-block;">
                    <img src="../Images/logo-kingdom-confeitaria.svg" alt="Kingdom Confeitaria" style="cursor: pointer;" />
                </a>
                <div class="header-user-name" id="clienteNome" runat="server"></div>
            </div>
            <div class="header-actions">
                <a href="../Default.aspx"><i class="fas fa-home"></i> Home</a>
                <a href="MinhasReservas.aspx" id="linkMinhasReservas" runat="server"><i class="fas fa-clipboard-list"></i> Minhas Reservas</a>
                <a href="MeusDados.aspx" id="linkMeusDados" runat="server"><i class="fas fa-user"></i> Meus Dados</a>
                <a href="Admin.aspx" id="linkAdmin" runat="server"><i class="fas fa-cog"></i> Painel Gestor</a>
                <a href="Logout.aspx" id="linkLogout" runat="server"><i class="fas fa-sign-out-alt"></i> Sair</a>
            </div>
        </div>
        
        <div class="dados-container">
            <h2 class="text-center mb-4" style="color: #1a4d2e;">
                <i class="fas fa-user-circle"></i> Meus Dados
            </h2>
            
            <div id="alertContainer" runat="server"></div>
            
            <div class="mb-3">
                <label for="txtNome" class="form-label">
                    <i class="fas fa-user"></i> Nome Completo
                </label>
                <input type="text" class="form-control" id="txtNome" runat="server" required />
            </div>
            
            <div class="mb-3">
                <label for="txtEmail" class="form-label">
                    <i class="fas fa-envelope"></i> Email
                </label>
                <input type="email" class="form-control" id="txtEmail" runat="server" required />
            </div>
            
            <div class="mb-3">
                <label for="txtTelefone" class="form-label">
                    <i class="fas fa-phone"></i> Telefone / WhatsApp
                </label>
                <input type="tel" class="form-control" id="txtTelefone" runat="server" placeholder="(11) 99999-9999" />
            </div>
            
            <hr class="my-4" />
            
            <h5 class="mb-3" style="color: #1a4d2e;">
                <i class="fas fa-lock"></i> Alterar Senha
            </h5>
            <small class="text-muted d-block mb-3">Deixe em branco se não quiser alterar a senha</small>
            
            <div class="mb-3">
                <label for="txtSenhaAtual" class="form-label">Senha Atual</label>
                <input type="password" class="form-control" id="txtSenhaAtual" runat="server" />
            </div>
            
            <div class="mb-3">
                <label for="txtNovaSenha" class="form-label">Nova Senha</label>
                <input type="password" class="form-control" id="txtNovaSenha" runat="server" minlength="6" />
                <small class="text-muted">Mínimo de 6 caracteres</small>
            </div>
            
            <div class="mb-3">
                <label for="txtConfirmarNovaSenha" class="form-label">Confirmar Nova Senha</label>
                <input type="password" class="form-control" id="txtConfirmarNovaSenha" runat="server" />
            </div>
            
            <div class="d-grid gap-2 mb-3">
                <asp:Button ID="btnSalvar" runat="server" Text="Salvar Altera&#231;&#245;es" CssClass="btn btn-success btn-lg" OnClick="btnSalvar_Click" />
                <a href="../Default.aspx" class="btn btn-secondary">
                    <i class="fas fa-arrow-left"></i> Voltar
                </a>
            </div>
        </div>
    </form>

    <script src="../Scripts/bootstrap/bootstrap.bundle.min.js"></script>
    <script src="../Scripts/app.js"></script>
    <script>
        function voltarPagina() {
            if (window.history.length > 1) {
                window.history.back();
            } else {
                window.location.href = '../Default.aspx';
            }
        }
    </script>
</body>
</html>

