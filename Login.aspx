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
        .is-valid {
            border-color: #28a745 !important;
        }
        .is-invalid {
            border-color: #dc3545 !important;
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

        <!-- Modal para completar cadastro social -->
        <div class="modal fade" id="modalCompletarCadastro" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Complete seu Cadastro</h5>
                    </div>
                    <div class="modal-body">
                        <p class="text-muted">Para finalizar seu cadastro, precisamos de algumas informações adicionais:</p>
                        <div class="mb-3">
                            <label for="txtNomeCompletar" class="form-label">Nome Completo *</label>
                            <asp:TextBox ID="txtNomeCompletar" runat="server" CssClass="form-control" required></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label for="txtTelefoneCompletar" class="form-label">Telefone/WhatsApp *</label>
                            <div class="input-group">
                                <asp:TextBox ID="txtTelefoneCompletar" runat="server" CssClass="form-control" required></asp:TextBox>
                                <div class="input-group-text">
                                    <input type="checkbox" id="chkTemWhatsAppCompletar" runat="server" checked />
                                    <label for="chkTemWhatsAppCompletar" class="ms-2 mb-0">Tenho WhatsApp</label>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <asp:Button ID="btnCompletarCadastro" runat="server" 
                            Text="Completar Cadastro" 
                            CssClass="btn btn-primary" 
                            OnClick="btnCompletarCadastro_Click" />
                    </div>
                </div>
            </div>
        </div>
    </form>

    <!-- Facebook SDK -->
    <script>
        window.fbAsyncInit = function() {
            var fbAppId = '<%= System.Configuration.ConfigurationManager.AppSettings["FacebookAppId"] ?? "" %>';
            if (fbAppId && fbAppId !== '' && fbAppId !== 'SEU_FACEBOOK_APP_ID') {
                if (typeof FB !== 'undefined') {
                    FB.init({
                        appId: fbAppId,
                        cookie: true,
                        xfbml: true,
                        version: 'v18.0'
                    });
                }
            }
        };
    </script>
    <script async defer crossorigin="anonymous" src="https://connect.facebook.net/pt_BR/sdk.js"></script>
    
    <!-- Google Sign-In -->
    <script src="https://accounts.google.com/gsi/client" async defer></script>
    
    <script>
        var facebookAppId = '<%= System.Configuration.ConfigurationManager.AppSettings["FacebookAppId"] ?? "" %>';
        var googleClientId = '<%= System.Configuration.ConfigurationManager.AppSettings["GoogleClientId"] ?? "" %>';
        var baseUrl = '<%= System.Configuration.ConfigurationManager.AppSettings["BaseUrl"] ?? Request.Url.GetLeftPart(UriPartial.Authority) %>';

        // Inicializar Google Sign-In quando SDK carregar
        window.addEventListener('load', function() {
            if (googleClientId && typeof google !== 'undefined' && google.accounts) {
                google.accounts.id.initialize({
                    client_id: googleClientId,
                    callback: handleGoogleSignIn
                });
            }
        });

        // Facebook Login
        function loginFacebook() {
            if (!facebookAppId || facebookAppId === '' || facebookAppId === 'SEU_FACEBOOK_APP_ID') {
                alert('Facebook login não está configurado. Por favor, configure o FacebookAppId no web.config ou use email/WhatsApp.');
                return;
            }

            if (typeof FB === 'undefined') {
                alert('Facebook SDK não carregou. Por favor, recarregue a página e verifique sua conexão com a internet.');
                return;
            }

            FB.login(function(response) {
                if (response.authResponse) {
                    FB.api('/me', {fields: 'name,email'}, function(userInfo) {
                        if (userInfo && !userInfo.error) {
                            var nome = userInfo.name || '';
                            var email = userInfo.email || '';
                            __doPostBack('LoginSocial', 'Facebook|' + response.authResponse.userID + '|' + 
                                nome + '|' + email);
                        } else {
                            alert('Erro ao obter informações do Facebook: ' + (userInfo.error ? userInfo.error.message : 'Erro desconhecido'));
                        }
                    });
                } else {
                    alert('Login com Facebook cancelado ou falhou.');
                }
            }, {scope: 'email,public_profile'});
        }

        // Google Login
        function loginGoogle() {
            if (!googleClientId || googleClientId === '' || googleClientId === 'SEU_GOOGLE_CLIENT_ID') {
                alert('Google login não está configurado. Por favor, configure o GoogleClientId no web.config ou use email/WhatsApp.');
                return;
            }

            if (typeof google === 'undefined' || !google.accounts) {
                alert('Google SDK não carregou. Por favor, recarregue a página e verifique sua conexão com a internet.');
                return;
            }

            // Criar um botão temporário e clicar nele
            var buttonContainer = document.createElement('div');
            buttonContainer.id = 'temp-google-button';
            buttonContainer.style.position = 'fixed';
            buttonContainer.style.left = '-9999px';
            document.body.appendChild(buttonContainer);

            google.accounts.id.initialize({
                client_id: googleClientId,
                callback: handleGoogleSignIn
            });

            google.accounts.id.renderButton(buttonContainer, {
                theme: 'outline',
                size: 'large',
                type: 'standard',
                text: 'signin_with',
                shape: 'rectangular'
            });

            // Aguardar um pouco e clicar no botão
            setTimeout(function() {
                var button = buttonContainer.querySelector('div[role="button"]');
                if (button) {
                    button.click();
                } else {
                    // Fallback: usar popup manual
                    var client = google.accounts.oauth2.initTokenClient({
                        client_id: googleClientId,
                        scope: 'email profile',
                        callback: function(tokenResponse) {
                            if (tokenResponse && tokenResponse.access_token) {
                                // Obter informações do usuário
                                fetch('https://www.googleapis.com/oauth2/v2/userinfo?access_token=' + tokenResponse.access_token)
                                    .then(function(response) {
                                        return response.json();
                                    })
                                    .then(function(data) {
                                        if (data && data.id) {
                                            __doPostBack('LoginSocial', 'Google|' + data.id + '|' + 
                                                (data.name || 'Usuário Google') + '|' + (data.email || ''));
                                        } else {
                                            alert('Erro ao obter informações do Google.');
                                        }
                                    })
                                    .catch(function(error) {
                                        console.error('Erro:', error);
                                        alert('Erro ao obter informações do Google: ' + (error.message || 'Erro desconhecido'));
                                    });
                            } else {
                                alert('Login com Google cancelado.');
                            }
                        }
                    });
                    client.requestAccessToken();
                }
            }, 100);
        }

        function handleGoogleSignIn(response) {
            try {
                if (!response || !response.credential) {
                    alert('Erro: Resposta inválida do Google.');
                    return;
                }

                // Decodificar o token JWT
                var base64Url = response.credential.split('.')[1];
                var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
                var jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
                    return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
                }).join(''));
                var userInfo = JSON.parse(jsonPayload);
                
                var nome = userInfo.name || '';
                var email = userInfo.email || '';
                __doPostBack('LoginSocial', 'Google|' + userInfo.sub + '|' + nome + '|' + email);
            } catch (e) {
                console.error('Erro ao processar login Google:', e);
                alert('Erro ao processar login do Google. Por favor, tente novamente.');
            }
        }

        // WhatsApp Login (abre link direto)
        function loginWhatsApp() {
            var phone = prompt('Digite seu número de WhatsApp (com DDD, apenas números):\nExemplo: 11999999999');
            if (phone) {
                // Remover caracteres não numéricos
                phone = phone.replace(/\D/g, '');
                if (phone.length >= 10 && phone.length <= 11) {
                    window.location.href = 'Login.aspx?provider=WhatsApp&phone=' + encodeURIComponent(phone);
                } else {
                    alert('Número inválido. Digite o DDD + número (10 ou 11 dígitos).');
                }
            }
        }

        // Instagram Login (via Facebook - Instagram não tem OAuth direto)
        function loginInstagram() {
            alert('Instagram não oferece login direto. Por favor, use Facebook (que dá acesso ao Instagram) ou faça login com email/WhatsApp.');
            // Opcional: redirecionar para Facebook
            // loginFacebook();
        }

        // Validação dinâmica de email e telefone
        document.addEventListener('DOMContentLoaded', function() {
            var emailInput = document.getElementById('<%= txtEmail.ClientID %>');
            var telefoneInput = document.getElementById('<%= txtTelefone.ClientID %>');
            var nomeInput = document.getElementById('<%= txtNome.ClientID %>');
            var btnContinuar = document.getElementById('<%= btnContinuar.ClientID %>');

            // Validação de Email
            if (emailInput) {
                var emailFeedback = document.createElement('div');
                emailFeedback.className = 'invalid-feedback';
                emailInput.parentElement.appendChild(emailFeedback);

                emailInput.addEventListener('input', function(e) {
                    var email = e.target.value.trim();
                    var isValid = true;
                    var message = '';

                    if (email.length === 0) {
                        isValid = false;
                        message = 'Email é obrigatório';
                    } else if (!email.includes('@')) {
                        isValid = false;
                        message = 'Email deve conter @';
                    } else if (!email.includes('.')) {
                        isValid = false;
                        message = 'Email deve conter um ponto (.)';
                    } else if (email.indexOf('@') === 0 || email.indexOf('@') === email.length - 1) {
                        isValid = false;
                        message = 'Email inválido';
                    } else if (email.split('@').length !== 2) {
                        isValid = false;
                        message = 'Email deve conter apenas um @';
                    } else {
                        var parts = email.split('@');
                        if (parts[0].length === 0 || parts[1].length === 0) {
                            isValid = false;
                            message = 'Email inválido';
                        } else if (!parts[1].includes('.')) {
                            isValid = false;
                            message = 'Domínio do email deve conter um ponto';
                        }
                    }

                    if (isValid) {
                        e.target.classList.remove('is-invalid');
                        e.target.classList.add('is-valid');
                        emailFeedback.textContent = '';
                    } else {
                        e.target.classList.remove('is-valid');
                        e.target.classList.add('is-invalid');
                        emailFeedback.textContent = message;
                    }

                    validarFormulario();
                });
            }

            // Validação e máscara de Telefone
            if (telefoneInput) {
                var telefoneFeedback = document.createElement('div');
                telefoneFeedback.className = 'invalid-feedback';
                telefoneInput.parentElement.appendChild(telefoneFeedback);

                telefoneInput.addEventListener('input', function(e) {
                    var value = e.target.value.replace(/\D/g, '');
                    var isValid = true;
                    var message = '';

                    if (value.length === 0) {
                        isValid = false;
                        message = 'Telefone é obrigatório';
                    } else if (value.length < 10) {
                        isValid = false;
                        message = 'Faltam ' + (10 - value.length) + ' dígitos';
                    } else if (value.length > 11) {
                        isValid = false;
                        message = 'Telefone deve ter no máximo 11 dígitos';
                    } else if (value.length === 10) {
                        value = value.replace(/^(\d{2})(\d{4})(\d{0,4}).*/, '($1) $2-$3');
                    } else if (value.length === 11) {
                        value = value.replace(/^(\d{2})(\d{5})(\d{0,4}).*/, '($1) $2-$3');
                    }

                    e.target.value = value;

                    if (isValid && value.length >= 14) { // (XX) XXXXX-XXXX ou (XX) XXXX-XXXX
                        e.target.classList.remove('is-invalid');
                        e.target.classList.add('is-valid');
                        telefoneFeedback.textContent = '';
                    } else {
                        e.target.classList.remove('is-valid');
                        e.target.classList.add('is-invalid');
                        telefoneFeedback.textContent = message;
                    }

                    validarFormulario();
                });
            }

            // Validação de Nome
            if (nomeInput) {
                var nomeFeedback = document.createElement('div');
                nomeFeedback.className = 'invalid-feedback';
                nomeInput.parentElement.appendChild(nomeFeedback);

                nomeInput.addEventListener('input', function(e) {
                    var nome = e.target.value.trim();
                    var isValid = true;
                    var message = '';

                    if (nome.length === 0) {
                        isValid = false;
                        message = 'Nome é obrigatório';
                    } else if (nome.length < 3) {
                        isValid = false;
                        message = 'Nome deve ter pelo menos 3 caracteres';
                    }

                    if (isValid) {
                        e.target.classList.remove('is-invalid');
                        e.target.classList.add('is-valid');
                        nomeFeedback.textContent = '';
                    } else {
                        e.target.classList.remove('is-valid');
                        e.target.classList.add('is-invalid');
                        nomeFeedback.textContent = message;
                    }

                    validarFormulario();
                });
            }

            function validarFormulario() {
                if (btnContinuar) {
                    var emailValido = !emailInput || (emailInput.classList.contains('is-valid'));
                    var telefoneValido = !telefoneInput || (telefoneInput.classList.contains('is-valid'));
                    var nomeValido = !nomeInput || (nomeInput.classList.contains('is-valid'));

                    if (emailValido && telefoneValido && nomeValido) {
                        btnContinuar.disabled = false;
                        btnContinuar.classList.remove('disabled');
                    } else {
                        btnContinuar.disabled = true;
                        btnContinuar.classList.add('disabled');
                    }
                }
            }
        });
    </script>
</body>
</html>

