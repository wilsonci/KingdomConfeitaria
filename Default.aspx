<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="KingdomConfeitaria.Default" EnableEventValidation="false" %>
<%@ Register Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" Namespace="System.Web.UI" TagPrefix="asp" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Kingdom Confeitaria - Reserva de Ginger Breads</title>
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
        .header-actions span {
            color: white;
            font-weight: 500;
        }
        .header-logo img {
            max-width: 20%;
            width: auto;
            height: auto;
            max-height: 80px;
            display: block;
            margin: 0 auto;
        }
        .container-main {
            background: white;
            border-radius: 20px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
            margin: 90px auto 20px auto;
            padding: 30px;
        }
        .produto-card {
            border: 2px solid #e9ecef;
            border-radius: 15px;
            padding: 20px;
            margin-bottom: 20px;
            transition: transform 0.3s, box-shadow 0.3s;
        }
        .produto-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 5px 20px rgba(0,0,0,0.1);
        }
        .produto-imagem {
            width: 100%;
            height: 200px;
            object-fit: cover;
            border-radius: 10px;
            margin-bottom: 15px;
        }
        .btn-tamanho {
            margin: 5px;
            min-width: 100px;
        }
        .btn-tamanho.active {
            background-color: #1a4d2e;
            border-color: #1a4d2e;
            color: white;
        }
        .carrinho-fixo {
            position: sticky;
            top: 20px;
            background: #f8f9fa;
            border-radius: 15px;
            padding: 20px;
            max-height: 80vh;
            overflow-y: auto;
        }
        .item-carrinho {
            background: white;
            padding: 15px;
            border-radius: 10px;
            margin-bottom: 10px;
            border-left: 4px solid #1a4d2e;
        }
        .total-carrinho {
            font-size: 1.5em;
            font-weight: bold;
            color: #1a4d2e;
            margin-top: 20px;
            padding-top: 20px;
            border-top: 2px solid #dee2e6;
        }
        .btn-reservar {
            background: linear-gradient(135deg, #1a4d2e 0%, #2d5a3d 100%);
            border: none;
            padding: 15px 30px;
            font-size: 1.2em;
            border-radius: 10px;
            color: white;
            width: 100%;
            margin-top: 20px;
            transition: all 0.3s;
            cursor: pointer;
        }
        .btn-reservar:hover:not(:disabled) {
            transform: scale(1.05);
            box-shadow: 0 5px 20px rgba(26, 77, 46, 0.4);
            background: linear-gradient(135deg, #2d5a3d 0%, #1a4d2e 100%);
        }
        .btn-reservar:disabled {
            opacity: 0.5;
            cursor: not-allowed;
            background: #6c757d;
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
        .header-title {
            text-align: center;
            color: #1a4d2e;
            margin-bottom: 30px;
            font-size: 1.8em;
            font-weight: 600;
        }
        .subtitle {
            color: #6c757d;
            font-size: 0.9em;
            margin-top: 10px;
        }
        .quantidade-input {
            width: 80px;
            text-align: center;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server" novalidate>
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true"></asp:ScriptManager>
        <div class="container-fluid">
            <div class="header-logo">
                <div class="header-actions">
                    <span id="clienteNome" runat="server" style="color: white; margin-right: 15px;"></span>
                    <a href="Login.aspx" id="linkLogin" runat="server">Entrar</a>
                    <a href="MinhasReservas.aspx" id="linkMinhasReservas" runat="server" style="display:none;">Minhas Reservas</a>
                    <a href="MeusDados.aspx" id="linkMeusDados" runat="server" style="display:none;">Meus Dados</a>
                    <a href="Admin.aspx" id="linkAdmin" runat="server" style="display:none;">Painel Gestor</a>
                    <a href="Logout.aspx" id="linkLogout" runat="server" style="display:none;">Sair</a>
                </div>
                <img id="logoImg" src="Images/logo-kingdom-confeitaria.svg" alt="Kingdom Confeitaria" style="max-width: 100%; height: auto;" />
                <h1 id="logoFallback" class="header-title" style="display: none; color: #d4af37; margin: 0;">
                    <i class="fas fa-crown"></i> Kingdom Confeitaria
                </h1>
            </div>
            <div class="container-main">
                <h2 class="header-title">
                    Reserve seus Ginger Breads
                </h2>

                <div class="row">
                    <div class="col-lg-7">
                        <h3 class="mb-3"><i class="fas fa-cookie-bite"></i> Produtos Disponíveis</h3>
                        <div id="produtosContainer" runat="server">
                            <!-- Produtos serão carregados aqui -->
                        </div>
                    </div>

                    <div class="col-lg-5">
                        <div class="carrinho-fixo">
                            <h3><i class="fas fa-shopping-cart"></i> Seu Pedido</h3>
                            <div id="carrinhoContainer" runat="server">
                                <p class="text-muted">Seu carrinho está vazio</p>
                            </div>
                            <div class="total-carrinho" id="totalContainer" runat="server" style="display: none;">
                                Total: R$ <span id="totalPedido" runat="server">0,00</span>
                            </div>
                            <asp:Button ID="btnFazerReserva" runat="server" 
                                Text="Fazer Reserva" 
                                CssClass="btn btn-reservar" 
                                OnClick="btnFazerReserva_Click" 
                                Enabled="false" 
                                UseSubmitBehavior="true"
                                CausesValidation="false" />
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal de Reserva -->
        <div class="modal fade" id="modalReserva" tabindex="-1" aria-labelledby="modalReservaLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="modalReservaLabel">Finalizar Reserva</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Fechar"></button>
                    </div>
                    <div class="modal-body">
                        <!-- ETAPA 1: Área de Login (só aparece se não estiver logado) -->
                        <div id="divLoginDinamico" runat="server">
                            <div class="mb-3">
                                <label class="form-label">Email ou Telefone *</label>
                                <asp:TextBox ID="txtLoginDinamico" runat="server" CssClass="form-control" placeholder="exemplo@email.com ou (11) 99999-9999"></asp:TextBox>
                                <small class="text-muted">Digite seu email ou telefone (apenas números). O sistema identificará automaticamente.</small>
                                <div id="divMensagemLogin" class="mt-2" style="display: none;"></div>
                            </div>
                            <div class="mb-3" id="divSenhaReserva" runat="server" style="display: none;">
                                <label class="form-label">Senha *</label>
                                <asp:TextBox ID="txtSenhaReserva" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                                <small class="text-muted">Digite sua senha para continuar</small>
                                <div class="mt-2">
                                    <a id="linkRecuperarSenha" href="RecuperarSenha.aspx" class="text-decoration-none small" target="_blank">Esqueci minha senha</a>
                                </div>
                            </div>
                            <div id="divOpcaoCadastro" class="mb-3" style="display: none;">
                                <p class="text-info"><i class="fas fa-info-circle"></i> Cliente não encontrado. Deseja se cadastrar?</p>
                                <a id="linkIrCadastro" href="#" class="btn btn-success btn-sm">Ir para Cadastro</a>
                            </div>
                            <!-- Botões da área de login (aparecem quando senha é solicitada) -->
                            <div id="divBotoesLogin" class="modal-footer" style="display: none; border-top: 1px solid #dee2e6; margin-top: 1rem; padding-top: 1rem;">
                                <button type="button" class="btn btn-secondary" id="btnCancelarLogin">Cancelar</button>
                                <button type="button" class="btn btn-primary" id="btnConfirmarLogin">Confirmar</button>
                            </div>
                        </div>
                        
                        <!-- ETAPA 2: Área de Reserva (só aparece após login) -->
                        <div id="divDadosReserva" runat="server" style="display: none;">
                            <div class="mb-3 p-3 bg-success text-white rounded">
                                <p class="small mb-0"><i class="fas fa-check-circle"></i> Você está logado. Complete os dados da reserva.</p>
                            </div>
                            <div class="mb-3">
                                <label class="form-label">Nome *</label>
                                <asp:TextBox ID="txtNome" runat="server" CssClass="form-control" ReadOnly="true" BackColor="#f8f9fa"></asp:TextBox>
                                <small class="text-muted">Para alterar seus dados, acesse "Meus Dados" no menu</small>
                            </div>
                            <div class="mb-3">
                                <label class="form-label">Email *</label>
                                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" ReadOnly="true" BackColor="#f8f9fa"></asp:TextBox>
                            </div>
                            <div class="mb-3">
                                <label class="form-label">Telefone/WhatsApp *</label>
                                <asp:TextBox ID="txtTelefone" runat="server" CssClass="form-control" ReadOnly="true" BackColor="#f8f9fa"></asp:TextBox>
                            </div>
                            <div class="mb-3">
                                <label class="form-label">Data de Retirada *</label>
                                <asp:DropDownList ID="ddlDataRetirada" runat="server" CssClass="form-select"></asp:DropDownList>
                            </div>
                            <div class="mb-3">
                                <label class="form-label">Observações</label>
                                <asp:TextBox ID="txtObservacoes" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer" id="divBotoesReserva">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                        <asp:Button ID="btnConfirmarReserva" runat="server" 
                            Text="Confirmar Reserva" 
                            CssClass="btn btn-primary" 
                            OnClick="btnConfirmarReserva_Click"
                            OnClientClick="return validarFormularioReserva();"
                            Style="display: none;" />
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal de Sucesso -->
        <div class="modal fade" id="modalSucesso" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header bg-success text-white">
                        <h5 class="modal-title"><i class="fas fa-check-circle"></i> Reserva Confirmada!</h5>
                    </div>
                    <div class="modal-body">
                        <p>Sua reserva foi realizada com sucesso! Você receberá um email de confirmação em breve.</p>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-bs-dismiss="modal" onclick="location.reload();">OK</button>
                    </div>
                </div>
            </div>
        </div>
    </form>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <!-- Garantir que __doPostBack esteja disponível antes de carregar os scripts -->
    <script type="text/javascript">
        // Função __doPostBack será gerada pelo ASP.NET automaticamente
        // Se não estiver disponível, criar uma versão básica
        if (typeof __doPostBack === 'undefined') {
            function __doPostBack(eventTarget, eventArgument) {
                if (!eventTarget) return false;
                var form = document.getElementById('form1');
                if (!form) {
                    console.error('Formulário form1 não encontrado');
                    return false;
                }
                
                // Remover inputs anteriores se existirem
                var existingTarget = form.querySelector('input[name="__EVENTTARGET"]');
                if (existingTarget) existingTarget.remove();
                var existingArg = form.querySelector('input[name="__EVENTARGUMENT"]');
                if (existingArg) existingArg.remove();
                
                var targetInput = document.createElement('input');
                targetInput.type = 'hidden';
                targetInput.name = '__EVENTTARGET';
                targetInput.value = eventTarget;
                form.appendChild(targetInput);
                
                if (eventArgument) {
                    var argInput = document.createElement('input');
                    argInput.type = 'hidden';
                    argInput.name = '__EVENTARGUMENT';
                    argInput.value = eventArgument;
                    form.appendChild(argInput);
                }
                
                form.submit();
                return false;
            }
            console.log('__doPostBack criado manualmente');
        } else {
            console.log('__doPostBack já está disponível (gerado pelo ASP.NET)');
        }
    </script>
    <!-- Scripts comuns da aplicação -->
    <script src="Scripts/app.js"></script>
    <!-- Scripts específicos da página principal -->
    <script src="Scripts/default.js"></script>
    <script>
        // Scripts inline apenas para dados dinâmicos do servidor (ClientIDs)
        // Todas as funções JavaScript estão em Scripts/app.js e Scripts/default.js
        
        // Configurar Login Dinâmico
        KingdomConfeitaria.Utils.ready(function() {
            var txtLoginDinamico = document.getElementById('<%= txtLoginDinamico.ClientID %>');
            var txtSenhaReserva = document.getElementById('<%= txtSenhaReserva.ClientID %>');
            var divSenhaReserva = document.getElementById('<%= divSenhaReserva.ClientID %>');
            var divMensagemLogin = document.getElementById('divMensagemLogin');
            var divDadosUsuario = document.getElementById('divDadosUsuario');
            var txtNome = document.getElementById('<%= txtNome.ClientID %>');
            var txtEmail = document.getElementById('<%= txtEmail.ClientID %>');
            var txtTelefone = document.getElementById('<%= txtTelefone.ClientID %>');
            var divDadosReserva = document.getElementById('<%= divDadosReserva.ClientID %>');
            var btnConfirmarReserva = document.getElementById('<%= btnConfirmarReserva.ClientID %>');
            var nomeReserva = txtNome; // Alias para compatibilidade
            var emailReserva = txtEmail; // Alias para compatibilidade
            var telefoneReserva = txtTelefone; // Alias para compatibilidade
            var estaLogado = window.usuarioLogado === true;
            
            var timeoutVerificacao = null;
            var clienteEncontrado = null;
            
            // Função para mostrar mensagem
            function mostrarMensagem(mensagem, tipo) {
                if (!divMensagemLogin) return;
                divMensagemLogin.style.display = 'block';
                divMensagemLogin.className = 'mt-2 alert alert-' + (tipo || 'info');
                divMensagemLogin.innerHTML = mensagem;
            }
            
            // Função para ocultar mensagem
            function ocultarMensagem() {
                if (divMensagemLogin) {
                    divMensagemLogin.style.display = 'none';
                }
            }
            
            // Função para verificar cliente enquanto digita
            function verificarClienteDinamico() {
                if (estaLogado) return;
                
                var login = txtLoginDinamico ? txtLoginDinamico.value.trim() : '';
                if (!login) {
                    ocultarMensagem();
                    var divSenhaReservaElement = document.getElementById('<%= divSenhaReserva.ClientID %>');
                    if (divSenhaReservaElement) divSenhaReservaElement.style.display = 'none';
                    clienteEncontrado = null;
                    return;
                }
                
                // Detectar se é email ou telefone
                var isEmail = login.indexOf('@') > -1;
                var loginLimpo = isEmail ? login.toLowerCase() : login.replace(/\D/g, '');
                
                console.log('Verificando login:', {
                    login: login,
                    isEmail: isEmail,
                    loginLimpo: loginLimpo,
                    length: loginLimpo.length
                });
                
                // Só verificar se tiver informação suficiente
                if (isEmail && login.length < 5) {
                    return;
                }
                if (!isEmail && loginLimpo.length < 10) {
                    console.log('Telefone muito curto, aguardando mais dígitos...');
                    return;
                }
                
                // Chamar PageMethod para verificar cliente
                if (typeof PageMethods !== 'undefined') {
                    mostrarMensagem('<i class="fas fa-spinner fa-spin"></i> Verificando...', 'info');
                    
                    console.log('Chamando PageMethods.VerificarClienteCadastrado com:', login);
                    PageMethods.VerificarClienteCadastrado(login, function(result) {
                        ocultarMensagem();
                        
                        console.log('Resultado da verificação:', result);
                        
                        if (result && result.existe) {
                            clienteEncontrado = result.cliente;
                            
                            // Hide cadastro option
                            var divOpcaoCadastro = document.getElementById('divOpcaoCadastro');
                            if (divOpcaoCadastro) divOpcaoCadastro.style.display = 'none';
                            
                            // Ocultar botões de login inicialmente (serão mostrados se tiver senha)
                            var divBotoesLoginInicial = document.getElementById('divBotoesLogin');
                            if (divBotoesLoginInicial) {
                                divBotoesLoginInicial.style.display = 'none';
                            }
                            
                            if (result.temSenha) {
                                // Cliente encontrado e tem senha - mostrar campo de senha e botões
                                // Usar ClientID para garantir que encontre o elemento correto
                                var divSenhaReservaElement = document.getElementById('<%= divSenhaReserva.ClientID %>');
                                var txtSenhaReservaElement = document.getElementById('<%= txtSenhaReserva.ClientID %>');
                                
                                console.log('Tentando mostrar campo de senha. divSenhaReserva encontrado:', divSenhaReservaElement !== null);
                                
                                if (divSenhaReservaElement) {
                                    divSenhaReservaElement.style.display = 'block';
                                    console.log('Campo de senha exibido');
                                    
                                    if (txtSenhaReservaElement) {
                                        txtSenhaReservaElement.required = true;
                                        setTimeout(function() {
                                            txtSenhaReservaElement.focus();
                                        }, 100);
                                    }
                                } else {
                                    console.error('divSenhaReserva não encontrado! ClientID:', '<%= divSenhaReserva.ClientID %>');
                                }
                                
                                // Atualizar link de recuperar senha com email/telefone
                                var linkRecuperarSenha = document.getElementById('linkRecuperarSenha');
                                if (linkRecuperarSenha) {
                                    var isEmail = login.indexOf('@') > -1;
                                    var loginParam = isEmail ? 'email' : 'telefone';
                                    linkRecuperarSenha.href = 'RecuperarSenha.aspx?' + loginParam + '=' + encodeURIComponent(login);
                                }
                                
                                // Mostrar botões de login (Confirmar, Cancelar)
                                var divBotoesLogin = document.getElementById('divBotoesLogin');
                                if (divBotoesLogin) {
                                    divBotoesLogin.style.display = 'flex';
                                }
                                
                                // Ocultar botões de reserva
                                var divBotoesReserva = document.getElementById('divBotoesReserva');
                                if (divBotoesReserva) {
                                    divBotoesReserva.style.display = 'none';
                                }
                                
                                mostrarMensagem('<i class="fas fa-user-check"></i> Cliente encontrado! Digite sua senha para continuar.', 'success');
                            } else {
                                // Cliente encontrado mas não tem senha - fazer login automático e mostrar área de reserva
                                mostrarMensagem('<i class="fas fa-spinner fa-spin"></i> Fazendo login...', 'info');
                                preencherDadosCliente(result.cliente);
                            }
                        } else {
                            // Cliente não encontrado - mostrar opção de cadastro
                            clienteEncontrado = null;
                            var divSenhaReservaElement = document.getElementById('<%= divSenhaReserva.ClientID %>');
                            var txtSenhaReservaElement = document.getElementById('<%= txtSenhaReserva.ClientID %>');
                            
                            if (divSenhaReservaElement) {
                                divSenhaReservaElement.style.display = 'none';
                                if (txtSenhaReservaElement) {
                                    txtSenhaReservaElement.value = '';
                                    txtSenhaReservaElement.required = false;
                                }
                            }
                            
                            // Ocultar botões de login
                            var divBotoesLogin = document.getElementById('divBotoesLogin');
                            if (divBotoesLogin) {
                                divBotoesLogin.style.display = 'none';
                            }
                            
                            // Mostrar opção de cadastro
                            var divOpcaoCadastro = document.getElementById('divOpcaoCadastro');
                            var linkIrCadastro = document.getElementById('linkIrCadastro');
                            if (divOpcaoCadastro) {
                                divOpcaoCadastro.style.display = 'block';
                            }
                            if (linkIrCadastro) {
                                // Detectar se é email ou telefone
                                var isEmail = login.indexOf('@') > -1;
                                var loginParam = isEmail ? 'email' : 'telefone';
                                linkIrCadastro.href = 'Cadastro.aspx?' + loginParam + '=' + encodeURIComponent(login);
                            }
                            
                            mostrarMensagem('<i class="fas fa-info-circle"></i> Cliente não encontrado. Clique no botão abaixo para se cadastrar.', 'info');
                        }
                    }, function(error) {
                        ocultarMensagem();
                        console.error('Erro ao verificar cliente:', error);
                        mostrarMensagem('<i class="fas fa-exclamation-triangle"></i> Erro ao verificar cliente. Tente novamente.', 'danger');
                    });
                }
            }
            
            // Função para preencher dados do cliente e mostrar área de reserva
            function preencherDadosCliente(cliente) {
                if (!cliente) return;
                
                // Fazer login na sessão via PageMethod
                var login = txtLoginDinamico ? txtLoginDinamico.value.trim() : '';
                
                // Chamar método para fazer login na sessão
                if (typeof PageMethods !== 'undefined') {
                    PageMethods.FazerLoginSessao(cliente.id, function(result) {
                        if (result && result.sucesso) {
                            // Preencher campos do formulário
                            if (txtNome && cliente.nome) txtNome.value = cliente.nome;
                            if (txtEmail && cliente.email) txtEmail.value = cliente.email;
                            if (txtTelefone && cliente.telefone) {
                                // Formatar telefone
                                var tel = cliente.telefone.replace(/\D/g, '');
                                if (tel.length <= 10) {
                                    tel = tel.replace(/^(\d{2})(\d{4})(\d{0,4}).*/, '($1) $2-$3');
                                } else {
                                    tel = tel.replace(/^(\d{2})(\d{5})(\d{0,4}).*/, '($1) $2-$3');
                                }
                                txtTelefone.value = tel;
                            }
                            
                            // Ocultar área de login
                            var divLoginDinamicoElement = document.getElementById('<%= divLoginDinamico.ClientID %>');
                            if (divLoginDinamicoElement) {
                                divLoginDinamicoElement.style.display = 'none';
                            }
                            
                            // Ocultar campo de senha
                            if (divSenhaReserva) {
                                divSenhaReserva.style.display = 'none';
                            }
                            
                            // Ocultar botões de login
                            var divBotoesLogin = document.getElementById('divBotoesLogin');
                            if (divBotoesLogin) {
                                divBotoesLogin.style.display = 'none';
                            }
                            
                            // Ocultar mensagem de login
                            ocultarMensagem();
                            
                            // Mostrar área de reserva
                            if (divDadosReserva) {
                                divDadosReserva.style.display = 'block';
                            }
                            
                            // Mostrar botões de reserva
                            var divBotoesReserva = document.getElementById('divBotoesReserva');
                            if (divBotoesReserva) {
                                divBotoesReserva.style.display = 'flex';
                            }
                            
                            // Mostrar botão Confirmar Reserva
                            if (btnConfirmarReserva) {
                                btnConfirmarReserva.style.display = 'inline-block';
                            }
                            
                            // Atualizar variável global
                            window.usuarioLogado = true;
                            
                            console.log('Login realizado com sucesso. Área de reserva exibida.');
                        } else {
                            mostrarMensagem('<i class="fas fa-exclamation-triangle"></i> Erro ao fazer login: ' + (result.mensagem || 'Erro desconhecido'), 'danger');
                        }
                    }, function(error) {
                        console.error('Erro ao fazer login:', error);
                        mostrarMensagem('<i class="fas fa-exclamation-triangle"></i> Erro ao fazer login. Tente novamente.', 'danger');
                    });
                }
            }
            
            // Função para validar senha (tornada global para ser acessível via onclick)
            window.validarSenha = function() {
                if (!clienteEncontrado) {
                    mostrarMensagem('<i class="fas fa-exclamation-triangle"></i> Cliente não encontrado. Por favor, digite seu email ou telefone primeiro.', 'warning');
                    return;
                }
                
                // Usar ClientID para garantir que encontre o elemento correto
                var txtSenhaReservaElement = document.getElementById('<%= txtSenhaReserva.ClientID %>');
                var txtLoginDinamicoElement = document.getElementById('<%= txtLoginDinamico.ClientID %>');
                
                var senha = txtSenhaReservaElement ? txtSenhaReservaElement.value : '';
                var login = txtLoginDinamicoElement ? txtLoginDinamicoElement.value.trim() : '';
                
                if (!senha) {
                    mostrarMensagem('<i class="fas fa-exclamation-triangle"></i> Por favor, digite sua senha.', 'warning');
                    if (txtSenhaReservaElement) {
                        txtSenhaReservaElement.focus();
                    }
                    return;
                }
                
                if (typeof PageMethods !== 'undefined') {
                    mostrarMensagem('<i class="fas fa-spinner fa-spin"></i> Validando senha...', 'info');
                    
                    PageMethods.ValidarSenhaCliente(login, senha, function(result) {
                        if (result && result.valido) {
                            // Senha válida - fazer login e mostrar área de reserva
                            mostrarMensagem('<i class="fas fa-spinner fa-spin"></i> Fazendo login...', 'info');
                            preencherDadosCliente(result.cliente);
                        } else {
                            // Senha inválida
                            mostrarMensagem('<i class="fas fa-times-circle"></i> ' + (result.mensagem || 'Senha incorreta.'), 'danger');
                            if (txtSenhaReservaElement) {
                                txtSenhaReservaElement.value = '';
                                txtSenhaReservaElement.focus();
                            }
                        }
                    }, function(error) {
                        ocultarMensagem();
                        console.error('Erro ao validar senha:', error);
                        mostrarMensagem('<i class="fas fa-exclamation-triangle"></i> Erro ao validar senha. Tente novamente.', 'danger');
                    });
                } else {
                    mostrarMensagem('<i class="fas fa-exclamation-triangle"></i> Erro: PageMethods não está disponível. Por favor, recarregue a página.', 'danger');
                }
            };
            
            // Função para filtrar e normalizar entrada do login
            function filtrarEntradaLogin(input) {
                var valor = input.value;
                var cursorPos = input.selectionStart;
                
                // Detectar se é email ou telefone
                // Regras de detecção:
                // 1. Se contém @, é email
                // 2. Se contém letras (a-z, A-Z), é email
                // 3. Se contém apenas números, é telefone
                // 4. Se está vazio, aceitar qualquer entrada e detectar pelo primeiro caractere
                var temArroba = valor.indexOf('@') > -1;
                var temLetras = /[a-zA-Z]/.test(valor);
                var temApenasNumeros = valor.length > 0 && /^[0-9]*$/.test(valor);
                var primeiroChar = valor.length > 0 ? valor[0] : '';
                var primeiroCharIsNumero = /^[0-9]$/.test(primeiroChar);
                var primeiroCharIsLetra = /^[a-zA-Z]$/.test(primeiroChar);
                
                // Se tiver @ ou letras, é email
                // Se tiver apenas números, é telefone
                // Se estiver vazio, assumir telefone (vai aceitar apenas números até detectar letra ou @)
                var isEmail = temArroba || temLetras;
                var isTelefone = !isEmail; // Se não é email, é telefone (inclui vazio)
                
                var novoValor = '';
                var novoCursorPos = cursorPos;
                
                if (isEmail) {
                    // É email - aceitar apenas caracteres válidos de email
                    // Caracteres válidos: letras, números, @, ., _, -, +
                    for (var i = 0; i < valor.length; i++) {
                        var char = valor[i];
                        var charCode = char.charCodeAt(0);
                        
                        // Converter maiúsculas para minúsculas imediatamente
                        if (charCode >= 65 && charCode <= 90) {
                            char = String.fromCharCode(charCode + 32);
                            charCode = char.charCodeAt(0);
                        }
                        
                        // Aceitar: letras minúsculas (a-z), números (0-9), @, ., _, -, +
                        if ((charCode >= 97 && charCode <= 122) || // a-z
                            (charCode >= 48 && charCode <= 57) ||   // 0-9
                            char === '@' || char === '.' || char === '_' || char === '-' || char === '+') {
                            novoValor += char;
                        } else if (i < cursorPos) {
                            // Se o caractere foi removido antes da posição do cursor, ajustar posição
                            novoCursorPos--;
                        }
                    }
                } else {
                    // É telefone - aceitar apenas números
                    for (var i = 0; i < valor.length; i++) {
                        var char = valor[i];
                        var charCode = char.charCodeAt(0);
                        
                        // Aceitar apenas números (0-9)
                        if (charCode >= 48 && charCode <= 57) {
                            novoValor += char;
                        } else if (i < cursorPos) {
                            // Se o caractere foi removido antes da posição do cursor, ajustar posição
                            novoCursorPos--;
                        }
                    }
                }
                
                // Atualizar valor e posição do cursor
                input.value = novoValor;
                input.setSelectionRange(novoCursorPos, novoCursorPos);
                
                return novoValor;
            }
            
            // Event listener para campo de login dinâmico
            if (txtLoginDinamico) {
                // Bloquear caracteres inválidos antes de digitar
                txtLoginDinamico.addEventListener('keypress', function(e) {
                    // Permitir teclas especiais (Backspace, Delete, Tab, Arrow keys, etc.)
                    var keyCode = e.which || e.keyCode;
                    if (keyCode === 8 || keyCode === 46 || keyCode === 9 || keyCode === 13 || keyCode === 27 || // Backspace, Delete, Tab, Enter, Esc
                        (keyCode >= 35 && keyCode <= 40) || // Home, End, Arrow keys
                        (e.ctrlKey || e.metaKey)) { // Ctrl/Cmd + qualquer tecla (para copiar, colar, etc.)
                        return true;
                    }
                    
                    var char = String.fromCharCode(keyCode);
                    var charCode = char.charCodeAt(0);
                    var valorAtual = e.target.value;
                    var temArroba = valorAtual.indexOf('@') > -1;
                    var temLetras = /[a-zA-Z]/.test(valorAtual);
                    var isEmail = temArroba || temLetras;
                    
                    // Se o campo está vazio, permitir qualquer caractere válido (será filtrado depois)
                    // Se já tem conteúdo, validar baseado no tipo detectado
                    // Mas permitir mudança de telefone para email se digitar @ ou letras
                    if (valorAtual.length === 0) {
                        // Campo vazio: aceitar letras, números, @ (será detectado automaticamente)
                        var valido = (charCode >= 65 && charCode <= 90) ||  // A-Z
                                     (charCode >= 97 && charCode <= 122) ||  // a-z
                                     (charCode >= 48 && charCode <= 57) ||   // 0-9
                                     char === '@';
                        if (!valido) {
                            e.preventDefault();
                        }
                    } else if (isEmail) {
                        // Email: aceitar letras, números, @, ., _, -, +
                        var valido = (charCode >= 65 && charCode <= 90) ||  // A-Z (será convertido para minúscula)
                                     (charCode >= 97 && charCode <= 122) ||  // a-z
                                     (charCode >= 48 && charCode <= 57) ||   // 0-9
                                     char === '@' || char === '.' || char === '_' || char === '-' || char === '+';
                        if (!valido) {
                            e.preventDefault();
                        }
                    } else {
                        // Telefone: aceitar números, mas também permitir @ ou letras para mudar para email
                        var valido = (charCode >= 48 && charCode <= 57) ||   // 0-9
                                     (charCode >= 65 && charCode <= 90) ||   // A-Z (mudará para email)
                                     (charCode >= 97 && charCode <= 122) ||  // a-z (mudará para email)
                                     char === '@';                            // @ (mudará para email)
                        if (!valido) {
                            e.preventDefault();
                        }
                    }
                });
                
                // Filtrar entrada em tempo real (para casos de colar, drag&drop, etc)
                txtLoginDinamico.addEventListener('input', function(e) {
                    // Filtrar e normalizar entrada
                    var novoValor = filtrarEntradaLogin(e.target);
                    
                    // Limpar timeout anterior
                    if (timeoutVerificacao) {
                        clearTimeout(timeoutVerificacao);
                    }
                    
                    // Aguardar 500ms após parar de digitar para verificar cliente
                    timeoutVerificacao = setTimeout(verificarClienteDinamico, 500);
                });
                
                // Prevenir colar conteúdo inválido
                txtLoginDinamico.addEventListener('paste', function(e) {
                    e.preventDefault();
                    var texto = (e.clipboardData || window.clipboardData).getData('text');
                    
                    // Filtrar texto colado
                    var temArroba = texto.indexOf('@') > -1;
                    var textoFiltrado = '';
                    
                    if (temArroba) {
                        // É email - filtrar caracteres válidos e converter para minúsculas
                        for (var i = 0; i < texto.length; i++) {
                            var char = texto[i].toLowerCase();
                            var charCode = char.charCodeAt(0);
                            if ((charCode >= 97 && charCode <= 122) || // a-z
                                (charCode >= 48 && charCode <= 57) ||   // 0-9
                                char === '@' || char === '.' || char === '_' || char === '-' || char === '+') {
                                textoFiltrado += char;
                            }
                        }
                    } else {
                        // É telefone - apenas números
                        textoFiltrado = texto.replace(/\D/g, '');
                    }
                    
                    // Inserir texto filtrado na posição do cursor
                    var cursorPos = e.target.selectionStart;
                    var valorAtual = e.target.value;
                    var novoValor = valorAtual.substring(0, cursorPos) + textoFiltrado + valorAtual.substring(e.target.selectionEnd);
                    e.target.value = novoValor;
                    e.target.setSelectionRange(cursorPos + textoFiltrado.length, cursorPos + textoFiltrado.length);
                    
                    // Disparar evento input para verificar cliente
                    e.target.dispatchEvent(new Event('input'));
                });
                
                // Permitir Enter para validar senha se campo de senha estiver visível
                txtLoginDinamico.addEventListener('keydown', function(e) {
                    if (e.key === 'Enter') {
                        var divSenhaReservaCheck = document.getElementById('<%= divSenhaReserva.ClientID %>');
                        if (divSenhaReservaCheck && divSenhaReservaCheck.style.display !== 'none') {
                            e.preventDefault();
                            var txtSenhaReservaCheck = document.getElementById('<%= txtSenhaReserva.ClientID %>');
                            if (txtSenhaReservaCheck) {
                                txtSenhaReservaCheck.focus();
                            }
                        }
                    }
                });
            }
            
            // Event listener para campo de senha
            // Usar ClientID para garantir que encontre o elemento correto
            var txtSenhaReservaElement = document.getElementById('<%= txtSenhaReserva.ClientID %>');
            
            if (txtSenhaReservaElement) {
                txtSenhaReservaElement.addEventListener('keypress', function(e) {
                    if (e.key === 'Enter') {
                        e.preventDefault();
                        if (window.validarSenha) {
                            window.validarSenha();
                        }
                    }
                });
                
                // Botão para validar senha (pode ser adicionado depois)
                txtSenhaReservaElement.addEventListener('blur', function() {
                    if (this.value && clienteEncontrado) {
                        if (window.validarSenha) {
                            window.validarSenha();
                        }
                    }
                });
            }
            
            // Event listeners para botões de login
            var btnConfirmarLogin = document.getElementById('btnConfirmarLogin');
            var btnCancelarLogin = document.getElementById('btnCancelarLogin');
            
            if (btnConfirmarLogin) {
                btnConfirmarLogin.addEventListener('click', function(e) {
                    e.preventDefault();
                    if (window.validarSenha) {
                        window.validarSenha();
                    } else {
                        alert('Função de validação não disponível. Por favor, recarregue a página.');
                    }
                });
            }
            
            if (btnCancelarLogin) {
                btnCancelarLogin.addEventListener('click', function(e) {
                    e.preventDefault();
                    if (typeof fecharModalReserva === 'function') {
                        fecharModalReserva();
                    } else if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Modal) {
                        KingdomConfeitaria.Modal.hide('modalReserva');
                    } else {
                        // Fallback: usar Bootstrap diretamente
                        var modalElement = document.getElementById('modalReserva');
                        if (modalElement && typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                            var modal = bootstrap.Modal.getInstance(modalElement);
                            if (modal) {
                                modal.hide();
                            }
                        }
                    }
                });
            }
            
            // Validação dos campos de email e telefone (se preenchidos manualmente)
            if (emailReserva) {
                emailReserva.addEventListener('input', function(e) {
                    if (typeof DefaultPage !== 'undefined' && DefaultPage.Validacao) {
                        DefaultPage.Validacao.validarEmail(e.target);
                    }
                });
            }

            if (telefoneReserva) {
                telefoneReserva.addEventListener('input', function(e) {
                    var value = e.target.value.replace(/\D/g, '');
                    if (value.length <= 11) {
                        if (value.length <= 10) {
                            value = value.replace(/^(\d{2})(\d{4})(\d{0,4}).*/, '($1) $2-$3');
                        } else {
                            value = value.replace(/^(\d{2})(\d{5})(\d{0,4}).*/, '($1) $2-$3');
                        }
                        e.target.value = value;
                    }
                    if (typeof DefaultPage !== 'undefined' && DefaultPage.Validacao) {
                        DefaultPage.Validacao.validarTelefone(e.target);
                    }
                });
            }

            if (nomeReserva) {
                nomeReserva.addEventListener('input', function(e) {
                    if (typeof DefaultPage !== 'undefined' && DefaultPage.Validacao) {
                        DefaultPage.Validacao.validarNome(e.target);
                    }
                });
            }
            
            // Atualizar função de validação do formulário com ClientIDs
            if (typeof DefaultPage !== 'undefined' && DefaultPage.ModalReserva) {
                var originalValidar = DefaultPage.ModalReserva.validarFormulario;
                DefaultPage.ModalReserva.validarFormulario = function() {
                    var modal = document.getElementById('modalReserva');
                    if (!modal || !modal.classList.contains('show')) {
                        return true;
                    }

                    var nome = document.getElementById('<%= txtNome.ClientID %>');
                    var email = document.getElementById('<%= txtEmail.ClientID %>');
                    var telefone = document.getElementById('<%= txtTelefone.ClientID %>');
                    var dataRetirada = document.getElementById('<%= ddlDataRetirada.ClientID %>');

                    var primeiroInvalido = null;
                    var nomeValido = DefaultPage.Validacao.validarNome(nome);
                    if (!nomeValido && !primeiroInvalido) primeiroInvalido = nome;
                    var emailValido = DefaultPage.Validacao.validarEmail(email);
                    if (!emailValido && !primeiroInvalido) primeiroInvalido = email;
                    var telefoneValido = DefaultPage.Validacao.validarTelefone(telefone);
                    if (!telefoneValido && !primeiroInvalido) primeiroInvalido = telefone;
                    var dataValida = dataRetirada && dataRetirada.value && dataRetirada.value !== '';
                    if (!dataValida && !primeiroInvalido) primeiroInvalido = dataRetirada;

                    if (!nomeValido || !emailValido || !telefoneValido || !dataValida) {
                        if (primeiroInvalido) {
                            try {
                                primeiroInvalido.focus();
                                primeiroInvalido.scrollIntoView({ behavior: 'smooth', block: 'center' });
                            } catch (e) {}
                        }
                        alert('Por favor, preencha todos os campos obrigatórios corretamente.');
                        return false;
                    }
                    return true;
                };
            }
        });
    </script>
</body>
</html>


