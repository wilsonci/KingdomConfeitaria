<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MeusDados.aspx.cs" Inherits="KingdomConfeitaria.MeusDados" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Meus Dados - Kingdom Confeitaria</title>
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
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="header-logo">
            <div class="header-actions">
                <a href="Default.aspx"><i class="fas fa-home"></i> Home</a>
                <span id="clienteNome" runat="server" style="color: white; margin-right: 15px;"></span>
                <a href="MinhasReservas.aspx" id="linkMinhasReservas" runat="server" style="display: none;">Minhas Reservas</a>
                <a href="MeusDados.aspx" id="linkMeusDados" runat="server" style="display: none;">Meus Dados</a>
                <a href="Admin.aspx" id="linkAdmin" runat="server" style="display: none;">Painel Gestor</a>
                <a href="Logout.aspx" id="linkLogout" runat="server" style="display: none;">Sair</a>
            </div>
            <img src="Images/logo-kingdom-confeitaria.svg" alt="Kingdom Confeitaria" style="max-width: 100%; height: auto;" />
        </div>
        
        <div class="dados-container">
            <h2 class="text-center mb-4" style="color: #1a4d2e;">Meus Dados</h2>
            
            <div id="alertContainer" runat="server"></div>
            
            <div class="mb-3">
                <label for="txtNome" class="form-label">Nome Completo</label>
                <input type="text" class="form-control" id="txtNome" runat="server" required />
            </div>
            
            <div class="mb-3">
                <label for="txtEmail" class="form-label">Email</label>
                <input type="email" class="form-control" id="txtEmail" runat="server" required />
            </div>
            
            <div class="mb-3">
                <label for="txtTelefone" class="form-label">Telefone</label>
                <input type="tel" class="form-control" id="txtTelefone" runat="server" placeholder="(11) 99999-9999" />
            </div>
            
            <hr class="my-4" />
            
            <h5 class="mb-3" style="color: #1a4d2e;">Alterar Senha</h5>
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
            
            <div class="d-grid mb-3">
                <asp:Button ID="btnSalvar" runat="server" Text="Salvar Alterações" CssClass="btn btn-success btn-lg" OnClick="btnSalvar_Click" />
            </div>
            
            <div class="text-center">
                <a href="Default.aspx" class="text-decoration-none">Voltar para a página inicial</a>
            </div>
        </div>
    </form>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script src="Scripts/app.js"></script>
</body>
</html>

