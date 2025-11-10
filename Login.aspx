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
            margin-bottom: 30px;
        }
        .header-logo img {
            max-width: 300px;
            width: 100%;
            height: auto;
        }
        .btn-social {
            width: 100%;
            padding: 12px;
            margin-bottom: 10px;
            border-radius: 8px;
            font-weight: 500;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 10px;
        }
        .btn-facebook {
            background-color: #1877F2;
            color: white;
            border: none;
        }
        .btn-facebook:hover {
            background-color: #166FE5;
            color: white;
        }
        .btn-google {
            background-color: #DB4437;
            color: white;
            border: none;
        }
        .btn-google:hover {
            background-color: #C23321;
            color: white;
        }
        .btn-whatsapp {
            background-color: #25D366;
            color: white;
            border: none;
        }
        .btn-whatsapp:hover {
            background-color: #20BA5A;
            color: white;
        }
        .divider {
            text-align: center;
            margin: 20px 0;
            position: relative;
        }
        .divider::before,
        .divider::after {
            content: '';
            position: absolute;
            top: 50%;
            width: 45%;
            height: 1px;
            background: #ddd;
        }
        .divider::before {
            left: 0;
        }
        .divider::after {
            right: 0;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-container">
            <div class="header-logo">
                <img src="Images/logo-kingdom-confeitaria.png" alt="Kingdom Confeitaria" onerror="this.style.display='none'; this.nextElementSibling.style.display='block';" />
                <h1 style="display:none; color: #1a4d2e;">Kingdom Confeitaria</h1>
            </div>
            
            <h2 class="text-center mb-4" style="color: #1a4d2e;">Entrar ou Cadastrar</h2>
            
            <div id="alertContainer" runat="server"></div>
            
            <div class="mb-3">
                <button type="button" class="btn btn-facebook btn-social" onclick="loginFacebook()">
                    <i class="fab fa-facebook-f"></i> Continuar com Facebook
                </button>
            </div>
            
            <div class="mb-3">
                <button type="button" class="btn btn-google btn-social" onclick="loginGoogle()">
                    <i class="fab fa-google"></i> Continuar com Google
                </button>
            </div>
            
            <div class="mb-3">
                <button type="button" class="btn btn-whatsapp btn-social" onclick="loginWhatsApp()">
                    <i class="fab fa-whatsapp"></i> Continuar com WhatsApp
                </button>
            </div>
            
            <div class="divider">
                <span style="background: white; padding: 0 10px; color: #666;">ou</span>
            </div>
            
            <div class="mb-3">
                <label for="txtEmail" class="form-label">Email</label>
                <input type="email" class="form-control" id="txtEmail" runat="server" required />
            </div>
            
            <div class="mb-3">
                <label for="txtTelefone" class="form-label">Telefone (com WhatsApp)</label>
                <div class="input-group">
                    <input type="tel" class="form-control" id="txtTelefone" runat="server" placeholder="(11) 99999-9999" />
                    <div class="input-group-text">
                        <input type="checkbox" id="chkTemWhatsApp" runat="server" checked />
                        <label for="chkTemWhatsApp" class="ms-2 mb-0">Tenho WhatsApp</label>
                    </div>
                </div>
            </div>
            
            <div class="mb-3">
                <label for="txtNome" class="form-label">Nome Completo</label>
                <input type="text" class="form-control" id="txtNome" runat="server" required />
            </div>
            
            <div class="d-grid">
                <asp:Button ID="btnContinuar" runat="server" Text="Continuar" CssClass="btn btn-success btn-lg" OnClick="btnContinuar_Click" />
            </div>
            
            <div class="text-center mt-3">
                <a href="Default.aspx" class="text-decoration-none">Voltar para produtos</a>
            </div>
        </div>
    </form>

    <!-- Facebook SDK -->
    <script async defer crossorigin="anonymous" src="https://connect.facebook.net/pt_BR/sdk.js#xfbml=1&version=v18.0&appId=SEU_FACEBOOK_APP_ID&autoLogAppEvents=1"></script>
    
    <!-- Google Sign-In -->
    <script src="https://accounts.google.com/gsi/client" async defer></script>
    
    <script>
        // Facebook Login
        function loginFacebook() {
            FB.login(function(response) {
                if (response.authResponse) {
                    FB.api('/me', {fields: 'name,email'}, function(userInfo) {
                        __doPostBack('LoginSocial', 'Facebook|' + response.authResponse.userID + '|' + userInfo.name + '|' + (userInfo.email || ''));
                    });
                }
            }, {scope: 'email'});
        }

        // Google Login
        function loginGoogle() {
            google.accounts.id.initialize({
                client_id: 'SEU_GOOGLE_CLIENT_ID',
                callback: handleGoogleSignIn
            });
            google.accounts.id.prompt();
        }

        function handleGoogleSignIn(response) {
            // Decodificar o token JWT (simplificado - em produção, validar no servidor)
            var base64Url = response.credential.split('.')[1];
            var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
            var jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
                return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
            }).join(''));
            var userInfo = JSON.parse(jsonPayload);
            
            __doPostBack('LoginSocial', 'Google|' + userInfo.sub + '|' + userInfo.name + '|' + userInfo.email);
        }

        // WhatsApp Login (abre link direto)
        function loginWhatsApp() {
            var phone = prompt('Digite seu número de WhatsApp (com DDD):');
            if (phone) {
                window.location.href = 'Login.aspx?provider=WhatsApp&phone=' + encodeURIComponent(phone);
            }
        }

        // Máscara de telefone
        document.addEventListener('DOMContentLoaded', function() {
            var telefoneInput = document.getElementById('<%= txtTelefone.ClientID %>');
            if (telefoneInput) {
                telefoneInput.addEventListener('input', function(e) {
                    var value = e.target.value.replace(/\D/g, '');
                    if (value.length <= 11) {
                        if (value.length <= 10) {
                            value = value.replace(/^(\d{2})(\d{4})(\d{0,4}).*/, '($1) $2-$3');
                        } else {
                            value = value.replace(/^(\d{2})(\d{5})(\d{0,4}).*/, '($1) $2-$3');
                        }
                        e.target.value = value;
                    }
                });
            }
        });
    </script>
</body>
</html>

