<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="KingdomConfeitaria.Default" EnableEventValidation="false" EnableViewState="true" %>
<%@ Register Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" Namespace="System.Web.UI" TagPrefix="asp" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Kingdom Confeitaria - Reserva de Ginger Breads</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" crossorigin="anonymous" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" rel="stylesheet" crossorigin="anonymous" />
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
            max-width: 150px;
            height: 67px;
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
        /* Estilos para o modal de reserva - aumentar tamanho */
        #modalReserva .modal-dialog {
            max-width: 650px;
            min-height: 450px;
        }
        #modalReserva .modal-body {
            min-height: 350px;
            max-height: 70vh;
            padding: 2rem;
            overflow-y: auto;
        }
        #modalReserva .modal-content {
            min-height: 450px;
        }
        /* Garantir que o campo de senha tenha espaço e seja visível */
        #divSenhaReserva {
            margin-top: 1.5rem !important;
            margin-bottom: 1.5rem !important;
            padding: 1rem !important;
            background-color: #f8f9fa !important;
            border-radius: 8px !important;
            border: 1px solid #dee2e6 !important;
        }
        #divSenhaReserva[style*="display: block"],
        #divSenhaReserva.show {
            display: block !important;
        }
        #divSenhaReserva .form-control {
            font-size: 1.1rem;
            padding: 0.875rem;
            min-height: 48px;
        }
        #divSenhaReserva label {
            font-weight: 600;
            margin-bottom: 0.5rem;
        }
        /* Espaçamento para os botões de login */
        #divBotoesLogin {
            margin-top: 2rem;
            padding-top: 1.5rem;
            border-top: 2px solid #dee2e6;
        }
        /* Garantir que a área de login tenha espaço suficiente */
        #divLoginDinamico {
            min-height: 250px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server" novalidate>
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true"></asp:ScriptManager>
        <div class="container-fluid">
            <div class="header-logo">
                <div class="header-actions">
                    <span id="clienteNome" runat="server" style="color: white; margin-right: 15px; display: none;"></span>
                    <a href="Default.aspx"><i class="fas fa-home"></i> Home</a>
                    <a href="#" id="linkLogin" runat="server" style="display: inline;" onclick="abrirModalLogin(); return false;"><i class="fas fa-sign-in-alt"></i> Entrar</a>
                    <a href="MinhasReservas.aspx" id="linkMinhasReservas" runat="server" style="display: none;"><i class="fas fa-clipboard-list"></i> Minhas Reservas</a>
                    <a href="MeusDados.aspx" id="linkMeusDados" runat="server" style="display: none;"><i class="fas fa-user"></i> Meus Dados</a>
                    <a href="Admin.aspx" id="linkAdmin" runat="server" style="display: none;"><i class="fas fa-cog"></i> Painel Gestor</a>
                    <a href="Logout.aspx" id="linkLogout" runat="server" style="display: none;"><i class="fas fa-sign-out-alt"></i> Sair</a>
                </div>
                <img id="logoImg" src="Images/logo-kingdom-confeitaria.svg" alt="Kingdom Confeitaria" style="max-width: 100%; height: auto;" loading="eager" decoding="async" />
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

        <!-- Modal de Login Dinâmico (Componente Reutilizável) -->
        <div class="modal fade" id="modalLoginDinamico" tabindex="-1" aria-labelledby="modalLoginDinamicoLabel" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered modal-dialog-scrollable">
                <div class="modal-content">
                    <div class="modal-header">
                        <div class="text-center w-100">
                            <img src="Images/logo-kingdom-confeitaria.svg" alt="Kingdom Confeitaria" style="max-width: 150px; height: auto; margin-bottom: 10px;" loading="lazy" decoding="async" />
                            <h5 class="modal-title mt-2" id="modalLoginDinamicoLabel">Login</h5>
                        </div>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Fechar"></button>
                    </div>
                    <div class="modal-body">
                        <!-- Área de Login Dinâmico -->
                        <div id="divLoginDinamicoStandalone">
                            <div class="mb-3">
                                <label for="txtLoginDinamicoStandalone" class="form-label">Email ou Telefone *</label>
                                <input type="text" class="form-control" id="txtLoginDinamicoStandalone" name="txtLoginDinamicoStandalone" placeholder="exemplo@email.com ou (11) 99999-9999" aria-label="Email ou Telefone" />
                            </div>
                            <div class="mb-3" id="divSenhaStandalone" style="display: none;">
                                <label for="txtSenhaStandalone" class="form-label">Senha *</label>
                                <input type="password" class="form-control" id="txtSenhaStandalone" name="txtSenhaStandalone" aria-label="Senha" />
                            </div>
                            
                            <!-- Mensagens aparecem aqui, abaixo dos campos mas antes dos botões -->
                            <div id="divMensagemLoginStandalone" class="mb-3" style="display: none;"></div>
                            
                            <div id="divOpcaoCadastroStandalone" class="mb-3" style="display: none;">
                                <div class="alert alert-info mb-3">
                                    <i class="fas fa-info-circle"></i> Cliente não encontrado. Deseja se cadastrar?
                                </div>
                                <a id="linkIrCadastroStandalone" href="#" class="btn btn-success btn-sm w-100">
                                    <i class="fas fa-user-plus"></i> Ir para Cadastro
                                </a>
                            </div>
                            
                            <!-- Botões da área de login (aparecem quando senha é solicitada) -->
                            <div id="divBotoesLoginStandalone" class="d-flex gap-2 mb-3" style="display: none;">
                                <button type="button" class="btn btn-secondary flex-fill" onclick="fecharModalLogin();">Cancelar</button>
                                <button type="button" class="btn btn-primary flex-fill" id="btnConfirmarLoginStandalone">Confirmar</button>
                            </div>
                            
                            <!-- Mensagens aparecem abaixo dos botões -->
                            <div id="divMensagensAbaixoBotoesStandalone" class="mb-3" style="display: none;"></div>
                            
                            <!-- Opções de ajuda sempre visíveis, abaixo de tudo -->
                            <div class="mt-3 pt-3 border-top">
                                <div class="d-flex flex-column gap-2">
                                    <a href="RecuperarSenha.aspx" class="text-decoration-none small text-center" target="_blank">
                                        <i class="fas fa-key"></i> Recuperar Senha
                                    </a>
                                    <a href="Cadastro.aspx" class="text-decoration-none small text-center" target="_blank">
                                        <i class="fas fa-user-plus"></i> Cadastrar Novo Usuário
                                    </a>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal de Reserva -->
        <div class="modal fade" id="modalReserva" tabindex="-1" aria-labelledby="modalReservaLabel" aria-hidden="true">
            <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="modalReservaLabel">Login</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Fechar"></button>
                    </div>
                    <div class="modal-body">
                        <!-- ETAPA 1: Área de Login (só aparece se não estiver logado) -->
                        <div id="divLoginDinamico" runat="server">
                            <div class="mb-3">
                                <asp:Label ID="lblLoginDinamico" runat="server" AssociatedControlID="txtLoginDinamico" CssClass="form-label">Email ou Telefone *</asp:Label>
                                <asp:TextBox ID="txtLoginDinamico" runat="server" CssClass="form-control" placeholder="exemplo@email.com ou (11) 99999-9999" aria-label="Email ou Telefone"></asp:TextBox>
                            </div>
                            <div class="mb-3" id="divSenhaReserva" runat="server" style="display: none;">
                                <asp:Label ID="lblSenhaReserva" runat="server" AssociatedControlID="txtSenhaReserva" CssClass="form-label">Senha *</asp:Label>
                                <asp:TextBox ID="txtSenhaReserva" runat="server" CssClass="form-control" TextMode="Password" aria-label="Senha"></asp:TextBox>
                            </div>
                            
                            <!-- Mensagens aparecem aqui, abaixo dos campos mas antes dos botões -->
                            <div id="divMensagemLogin" class="mb-3" style="display: none;"></div>
                            
                            <div id="divOpcaoCadastro" class="mb-3" style="display: none;">
                                <div class="alert alert-info mb-3">
                                    <i class="fas fa-info-circle"></i> Cliente não encontrado. Deseja se cadastrar?
                                </div>
                                <a id="linkIrCadastro" href="#" class="btn btn-success btn-sm w-100">
                                    <i class="fas fa-user-plus"></i> Ir para Cadastro
                                </a>
                            </div>
                            
                            <!-- Botões da área de login (aparecem quando senha é solicitada) -->
                            <div id="divBotoesLogin" class="d-flex gap-2 mb-3" style="display: none;">
                                <button type="button" class="btn btn-secondary flex-fill" id="btnCancelarLogin">Cancelar</button>
                                <button type="button" class="btn btn-primary flex-fill" id="btnConfirmarLogin">Confirmar</button>
                            </div>
                            
                            <!-- Mensagens aparecem abaixo dos botões -->
                            <div id="divMensagensAbaixoBotoes" class="mb-3" style="display: none;"></div>
                            
                            <!-- Opções de ajuda sempre visíveis, abaixo de tudo -->
                            <div class="mt-3 pt-3 border-top">
                                <div class="d-flex flex-column gap-2">
                                    <a href="RecuperarSenha.aspx" class="text-decoration-none small text-center" target="_blank">
                                        <i class="fas fa-key"></i> Recuperar Senha
                                    </a>
                                    <a href="Cadastro.aspx" class="text-decoration-none small text-center" target="_blank">
                                        <i class="fas fa-user-plus"></i> Cadastrar Novo Usuário
                                    </a>
                                </div>
                            </div>
                        </div>
                        
                        <!-- ETAPA 2: Área de Reserva (só aparece após login) -->
                        <div id="divDadosReserva" runat="server" style="display: none;">
                            <div class="mb-3 p-3 bg-success text-white rounded">
                                <p class="small mb-0"><i class="fas fa-check-circle"></i> Você está logado. Complete os dados da reserva.</p>
                            </div>
                            <div class="mb-3">
                                <asp:Label ID="lblNome" runat="server" AssociatedControlID="txtNome" CssClass="form-label">Nome *</asp:Label>
                                <asp:TextBox ID="txtNome" runat="server" CssClass="form-control" ReadOnly="true" BackColor="#f8f9fa" aria-label="Nome"></asp:TextBox>
                                <asp:HiddenField ID="hdnNome" runat="server" />
                                <small class="text-muted">Para alterar seus dados, acesse "Meus Dados" no menu</small>
                            </div>
                            <div class="mb-3">
                                <asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail" CssClass="form-label">Email *</asp:Label>
                                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" ReadOnly="true" BackColor="#f8f9fa" aria-label="Email"></asp:TextBox>
                                <asp:HiddenField ID="hdnEmail" runat="server" />
                            </div>
                            <div class="mb-3">
                                <asp:Label ID="lblTelefone" runat="server" AssociatedControlID="txtTelefone" CssClass="form-label">Telefone/WhatsApp *</asp:Label>
                                <asp:TextBox ID="txtTelefone" runat="server" CssClass="form-control" ReadOnly="true" BackColor="#f8f9fa" aria-label="Telefone/WhatsApp"></asp:TextBox>
                                <asp:HiddenField ID="hdnTelefone" runat="server" />
                            </div>
                            <div class="mb-3">
                                <asp:Label ID="lblDataRetirada" runat="server" AssociatedControlID="ddlDataRetirada" CssClass="form-label">Data de Retirada *</asp:Label>
                                <asp:DropDownList ID="ddlDataRetirada" runat="server" CssClass="form-select" aria-label="Data de Retirada"></asp:DropDownList>
                            </div>
                            <div class="mb-3">
                                <asp:Label ID="lblObservacoes" runat="server" AssociatedControlID="txtObservacoes" CssClass="form-label">Observações</asp:Label>
                                <asp:TextBox ID="txtObservacoes" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" aria-label="Observações"></asp:TextBox>
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
        <div class="modal fade" id="modalSucesso" tabindex="-1" aria-hidden="true" data-bs-backdrop="static" data-bs-keyboard="false">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header bg-success text-white">
                        <h5 class="modal-title"><i class="fas fa-check-circle"></i> Reserva Confirmada!</h5>
                    </div>
                    <div class="modal-body text-center">
                        <i class="fas fa-check-circle text-success" style="font-size: 4rem; margin-bottom: 1rem;"></i>
                        <p class="h5 mb-3">Sua reserva foi realizada com sucesso!</p>
                        <p>Você receberá um email de confirmação em breve.</p>
                        <p class="text-muted small mt-3">Redirecionando para Minhas Reservas em <span id="contadorRedirecionamento">3</span> segundos...</p>
                    </div>
                    <div class="modal-footer justify-content-center">
                        <button type="button" class="btn btn-primary" onclick="window.location.href='MinhasReservas.aspx';">Ir para Minhas Reservas</button>
                        <button type="button" class="btn btn-secondary" onclick="location.reload();">Fazer Nova Reserva</button>
                    </div>
                </div>
            </div>
        </div>
    </form>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js" defer crossorigin="anonymous"></script>
    <!-- Garantir que __doPostBack esteja disponível antes de carregar os scripts -->
    <script type="text/javascript">
        // Função __doPostBack será gerada pelo ASP.NET automaticamente
        // Se não estiver disponível, criar uma versão básica
        if (typeof __doPostBack === 'undefined') {
            function __doPostBack(eventTarget, eventArgument) {
                if (!eventTarget) return false;
                var form = document.getElementById('form1');
                if (!form) {
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
        }
    </script>
    <!-- Scripts comuns da aplicação (sem defer - necessário para código inline) -->
    <script src="Scripts/app.js"></script>
    <!-- Scripts específicos da página principal -->
    <script src="Scripts/default.js" defer></script>
    <script>
        // Scripts inline apenas para dados dinâmicos do servidor (ClientIDs)
        // Todas as funções JavaScript estão em Scripts/app.js e Scripts/default.js
        
        // Aguardar carregamento do app.js antes de executar
        (function() {
            function initLoginDinamico() {
                if (typeof KingdomConfeitaria === 'undefined' || !KingdomConfeitaria.Utils) {
                    setTimeout(initLoginDinamico, 50);
                    return;
                }
                
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
            if (!divDadosReserva) {
                divDadosReserva = document.querySelector('[id*="divDadosReserva"]');
            }
            var btnConfirmarReserva = document.getElementById('<%= btnConfirmarReserva.ClientID %>');
            if (!btnConfirmarReserva) {
                btnConfirmarReserva = document.querySelector('[id*="btnConfirmarReserva"]');
            }
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
                
                // Verificando login
                
                // Só verificar se tiver informação suficiente
                if (isEmail && login.length < 5) {
                    return;
                }
                if (!isEmail && loginLimpo.length < 10) {
                    return;
                }
                
                // Chamar PageMethod para verificar cliente
                if (typeof PageMethods !== 'undefined') {
                    mostrarMensagem('<i class="fas fa-spinner fa-spin"></i> Verificando...', 'info');
                    
                    PageMethods.VerificarClienteCadastrado(login, function(result) {
                        ocultarMensagem();
                        
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
                                // Cliente encontrado e tem senha - MOSTRAR CAMPO DE SENHA IMEDIATAMENTE
                                
                                // Função para mostrar campo de senha
                                function mostrarCampoSenha() {
                                    // Garantir que o divLoginDinamico esteja visível
                                    var divLoginDinamicoElement = document.getElementById('<%= divLoginDinamico.ClientID %>');
                                    if (!divLoginDinamicoElement) {
                                        divLoginDinamicoElement = document.querySelector('[id*="divLoginDinamico"]');
                                    }
                                    if (divLoginDinamicoElement) {
                                        divLoginDinamicoElement.style.display = 'block';
                                    }
                                    
                                    // Encontrar o campo de senha - tentar múltiplas formas
                                    var divSenhaReservaElement = document.getElementById('<%= divSenhaReserva.ClientID %>');
                                    
                                    if (!divSenhaReservaElement) {
                                        divSenhaReservaElement = document.querySelector('[id*="divSenhaReserva"]');
                                    }
                                    
                                    if (!divSenhaReservaElement) {
                                        // Tentar encontrar todos os elementos com "Senha" no ID
                                        var todosElementos = document.querySelectorAll('[id*="Senha"]');
                                        if (todosElementos.length > 0) {
                                            divSenhaReservaElement = todosElementos[0];
                                        }
                                    }
                                    
                                    if (divSenhaReservaElement) {
                                        // FORÇAR EXIBIÇÃO - método mais direto
                                        divSenhaReservaElement.removeAttribute('style');
                                        divSenhaReservaElement.style.display = 'block';
                                        divSenhaReservaElement.style.visibility = 'visible';
                                        divSenhaReservaElement.style.marginTop = '1.5rem';
                                        divSenhaReservaElement.style.marginBottom = '1.5rem';
                                        divSenhaReservaElement.style.padding = '1rem';
                                        divSenhaReservaElement.style.backgroundColor = '#f8f9fa';
                                        divSenhaReservaElement.style.borderRadius = '8px';
                                        divSenhaReservaElement.style.border = '1px solid #dee2e6';
                                        divSenhaReservaElement.classList.add('show');
                                        divSenhaReservaElement.removeAttribute('hidden');
                                        
                                        // Criar estilo dinâmico para garantir que fique visível
                                        var styleId = 'style-for-divSenhaReserva';
                                        var existingStyle = document.getElementById(styleId);
                                        if (existingStyle) {
                                            existingStyle.remove();
                                        }
                                        var style = document.createElement('style');
                                        style.id = styleId;
                                        style.textContent = '#' + divSenhaReservaElement.id + ' { display: block !important; visibility: visible !important; }';
                                        document.head.appendChild(style);
                                        
                                        // Scroll para o campo
                                        setTimeout(function() {
                                            divSenhaReservaElement.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                                            
                                            // Focar no campo de senha
                                            var txtSenhaReservaElement = document.getElementById('<%= txtSenhaReserva.ClientID %>');
                                            if (!txtSenhaReservaElement) {
                                                txtSenhaReservaElement = document.querySelector('[id*="txtSenhaReserva"]');
                                            }
                                            if (txtSenhaReservaElement) {
                                                txtSenhaReservaElement.required = true;
                                                txtSenhaReservaElement.focus();
                                            }
                                        }, 100);
                                        
                                        return true;
                                    } else {
                                        return false;
                                    }
                                }
                                
                                // Mostrar campo de senha IMEDIATAMENTE
                                mostrarCampoSenha();
                                
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
                                
                                // Tentar novamente após um pequeno delay para garantir
                                setTimeout(function() {
                                    var divSenhaReservaElement = document.getElementById('<%= divSenhaReserva.ClientID %>');
                                    if (!divSenhaReservaElement) {
                                        divSenhaReservaElement = document.querySelector('[id*="divSenhaReserva"]');
                                    }
                                    if (divSenhaReservaElement && window.getComputedStyle(divSenhaReservaElement).display === 'none') {
                                        mostrarCampoSenha();
                                    }
                                }, 100);
                                
                                // Atualizar link de recuperar senha com email/telefone
                                var linkRecuperarSenha = document.getElementById('linkRecuperarSenha');
                                if (linkRecuperarSenha) {
                                    var isEmail = login.indexOf('@') > -1;
                                    var loginParam = isEmail ? 'email' : 'telefone';
                                    linkRecuperarSenha.href = 'RecuperarSenha.aspx?' + loginParam + '=' + encodeURIComponent(login);
                                }
                                
                                // Cliente encontrado, aguardando senha
                            } else {
                                // Cliente encontrado mas não tem senha - fazer login automático e mostrar área de reserva
                                // Continuar direto para a tela de reserva sem confirmação
                                ocultarMensagem();
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
                        mostrarMensagem('<i class="fas fa-exclamation-triangle"></i> Erro ao verificar cliente. Tente novamente.', 'danger');
                    });
                }
            }
            
            // Função para preencher dados do cliente e mostrar área de reserva AUTOMATICAMENTE
            // Esta função é chamada após login bem-sucedido e continua direto para a tela de reserva
            function preencherDadosCliente(cliente) {
                if (!cliente) return;
                
                // Fazer login na sessão via PageMethod
                var login = txtLoginDinamico ? txtLoginDinamico.value.trim() : '';
                
                // Chamar método para fazer login na sessão
                if (typeof PageMethods !== 'undefined') {
                    PageMethods.FazerLoginSessao(cliente.id, function(result) {
                        if (result && result.sucesso) {
                            
                            // Preencher campos do formulário IMEDIATAMENTE (visuais e hidden)
                            if (txtNome && cliente.nome) {
                                txtNome.value = cliente.nome;
                                // Preencher campo hidden para garantir que o valor seja enviado no postback
                                var hdnNome = document.getElementById('<%= hdnNome.ClientID %>');
                                if (hdnNome) {
                                    hdnNome.value = cliente.nome;
                                }
                            }
                            if (txtEmail && cliente.email) {
                                txtEmail.value = cliente.email;
                                // Preencher campo hidden para garantir que o valor seja enviado no postback
                                var hdnEmail = document.getElementById('<%= hdnEmail.ClientID %>');
                                if (hdnEmail) {
                                    hdnEmail.value = cliente.email;
                                }
                            }
                            if (txtTelefone && cliente.telefone) {
                                // Formatar telefone para exibição
                                var telFormatado = cliente.telefone.replace(/\D/g, '');
                                if (telFormatado.length <= 10) {
                                    telFormatado = telFormatado.replace(/^(\d{2})(\d{4})(\d{0,4}).*/, '($1) $2-$3');
                                } else {
                                    telFormatado = telFormatado.replace(/^(\d{2})(\d{5})(\d{0,4}).*/, '($1) $2-$3');
                                }
                                txtTelefone.value = telFormatado;
                                // Preencher campo hidden com telefone sem formatação para garantir que o valor seja enviado no postback
                                var hdnTelefone = document.getElementById('<%= hdnTelefone.ClientID %>');
                                if (hdnTelefone) {
                                    hdnTelefone.value = cliente.telefone.replace(/\D/g, '');
                                }
                            }
                            
                            // Ocultar área de login IMEDIATAMENTE
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
                            
                            // Atualizar título do modal para "Finalizar Reserva"
                            var modalReservaLabel = document.getElementById('modalReservaLabel');
                            if (modalReservaLabel) {
                                modalReservaLabel.textContent = 'Finalizar Reserva';
                            }
                            
                            // Mostrar área de reserva IMEDIATAMENTE (sem confirmação)
                            var divDadosReserva = document.getElementById('<%= divDadosReserva.ClientID %>');
                            if (!divDadosReserva) {
                                divDadosReserva = document.querySelector('[id*="divDadosReserva"]');
                            }
                            if (divDadosReserva) {
                                // Forçar exibição com múltiplas propriedades
                                divDadosReserva.style.display = 'block';
                                divDadosReserva.style.visibility = 'visible';
                                divDadosReserva.style.opacity = '1';
                                divDadosReserva.removeAttribute('hidden');
                                divDadosReserva.classList.remove('d-none');
                                divDadosReserva.classList.add('d-block');
                                
                                // Garantir que todos os campos dentro estejam visíveis
                                var campos = divDadosReserva.querySelectorAll('input, select, textarea, label, .form-label, .mb-3, .form-control, .form-select');
                                campos.forEach(function(campo) {
                                    campo.style.display = '';
                                    campo.style.visibility = 'visible';
                                    campo.style.opacity = '1';
                                    campo.removeAttribute('hidden');
                                });
                                
                                // Garantir que os labels e divs também estejam visíveis
                                var labels = divDadosReserva.querySelectorAll('label, .form-label, .mb-3, .small, .text-muted');
                                labels.forEach(function(label) {
                                    label.style.display = '';
                                    label.style.visibility = 'visible';
                                });
                            } else {
                                // Tentar novamente após um pequeno delay
                                setTimeout(function() {
                                    divDadosReserva = document.getElementById('<%= divDadosReserva.ClientID %>');
                                    if (!divDadosReserva) {
                                        divDadosReserva = document.querySelector('[id*="divDadosReserva"]');
                                    }
                                    if (divDadosReserva) {
                                        divDadosReserva.style.display = 'block';
                                        divDadosReserva.style.visibility = 'visible';
                                    }
                                }, 200);
                            }
                            
                            // Mostrar botões de reserva
                            var divBotoesReserva = document.getElementById('divBotoesReserva');
                            if (divBotoesReserva) {
                                divBotoesReserva.style.display = 'flex';
                                divBotoesReserva.style.visibility = 'visible';
                            }
                            
                            // Mostrar botão Confirmar Reserva
                            var btnConfirmarReserva = document.getElementById('<%= btnConfirmarReserva.ClientID %>');
                            if (!btnConfirmarReserva) {
                                btnConfirmarReserva = document.querySelector('[id*="btnConfirmarReserva"]');
                            }
                            if (btnConfirmarReserva) {
                                btnConfirmarReserva.style.display = 'inline-block';
                                btnConfirmarReserva.style.visibility = 'visible';
                                btnConfirmarReserva.removeAttribute('hidden');
                            }
                            
                            // Atualizar variável global
                            window.usuarioLogado = true;
                            
                            // Scroll suave para a área de reserva
                            setTimeout(function() {
                                if (divDadosReserva) {
                                    divDadosReserva.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                                }
                            }, 100);
                            
                            // Atualizar menu do header via JavaScript
                            setTimeout(function() {
                                var linkLogin = document.querySelector('[id*="linkLogin"]');
                                var linkMinhasReservas = document.querySelector('[id*="linkMinhasReservas"]');
                                var linkMeusDados = document.querySelector('[id*="linkMeusDados"]');
                                var linkLogout = document.querySelector('[id*="linkLogout"]');
                                var clienteNome = document.querySelector('[id*="clienteNome"]');
                                
                                if (linkLogin) linkLogin.style.display = 'none';
                                if (linkMinhasReservas) linkMinhasReservas.style.display = 'inline';
                                if (linkMeusDados) linkMeusDados.style.display = 'inline';
                                if (linkLogout) linkLogout.style.display = 'inline';
                                if (clienteNome && cliente.nome) {
                                    clienteNome.textContent = 'Olá, ' + cliente.nome;
                                    clienteNome.style.display = 'inline';
                                }
                                
                                // Atualizar isAdmin do cliente se retornado
                                if (result.isAdmin !== undefined) {
                                    cliente.isAdmin = result.isAdmin;
                                }
                                
                                // Se for admin, mostrar link de admin
                                var linkAdmin = document.querySelector('[id*="linkAdmin"]');
                                if (cliente.isAdmin || (result.isAdmin === true)) {
                                    if (linkAdmin) linkAdmin.style.display = 'inline';
                                } else {
                                    if (linkAdmin) linkAdmin.style.display = 'none';
                                }
                            }, 100);
                            
                        } else {
                            mostrarMensagem('<i class="fas fa-exclamation-triangle"></i> Erro ao fazer login: ' + (result.mensagem || 'Erro desconhecido'), 'danger');
                        }
                    }, function(error) {
                        // Erro ao fazer login
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
                            // Senha válida - fazer login e mostrar área de reserva AUTOMATICAMENTE
                            // Sem pedir confirmação, continuar direto para a tela de reserva
                            ocultarMensagem();
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
                        // Erro ao validar senha
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
                    
                    // Verificar também os hidden fields (caso os campos readonly estejam vazios)
                    var hdnNome = document.getElementById('<%= hdnNome.ClientID %>');
                    var hdnEmail = document.getElementById('<%= hdnEmail.ClientID %>');
                    var hdnTelefone = document.getElementById('<%= hdnTelefone.ClientID %>');
                    
                    // Se os campos visuais estiverem vazios (readonly), verificar hidden fields
                    var nomeValor = (nome && nome.value) ? nome.value.trim() : (hdnNome ? hdnNome.value : '');
                    var emailValor = (email && email.value) ? email.value.trim() : (hdnEmail ? hdnEmail.value : '');
                    var telefoneValor = (telefone && telefone.value) ? telefone.value.trim() : (hdnTelefone ? hdnTelefone.value : '');

                    var primeiroInvalido = null;
                    var nomeValido = nomeValor.length >= 3;
                    if (!nomeValido && !primeiroInvalido) primeiroInvalido = nome || hdnNome;
                    var emailValido = emailValor.length > 0 && emailValor.indexOf('@') > -1;
                    if (!emailValido && !primeiroInvalido) primeiroInvalido = email || hdnEmail;
                    var telefoneValorLimpo = telefoneValor.replace(/\D/g, '');
                    var telefoneValido = telefoneValorLimpo.length >= 10 && telefoneValorLimpo.length <= 11;
                    if (!telefoneValido && !primeiroInvalido) primeiroInvalido = telefone || hdnTelefone;
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
            }
            initLoginDinamico();
        })();
    </script>
    
    <!-- Componente Modal de Login Dinâmico -->
    <script>
        // Função global para abrir modal de login
        function abrirModalLogin() {
            if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Modal) {
                KingdomConfeitaria.Modal.show('modalLoginDinamico');
                // Resetar campos
                var txtLogin = document.getElementById('txtLoginDinamicoStandalone');
                var divSenha = document.getElementById('divSenhaStandalone');
                var divOpcaoCadastro = document.getElementById('divOpcaoCadastroStandalone');
                var divBotoesLogin = document.getElementById('divBotoesLoginStandalone');
                var divMensagem = document.getElementById('divMensagemLoginStandalone');
                
                if (txtLogin) txtLogin.value = '';
                if (divSenha) divSenha.style.display = 'none';
                if (divOpcaoCadastro) divOpcaoCadastro.style.display = 'none';
                if (divBotoesLogin) divBotoesLogin.style.display = 'none';
                if (divMensagem) {
                    divMensagem.style.display = 'none';
                    divMensagem.innerHTML = '';
                }
                
                // Focar no campo de login
                setTimeout(function() {
                    if (txtLogin) txtLogin.focus();
                }, 300);
            }
        }
        
        // Função global para fechar modal de login
        function fecharModalLogin() {
            if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Modal) {
                KingdomConfeitaria.Modal.hide('modalLoginDinamico');
            }
        }
        
        // Inicializar componente de login dinâmico standalone
        (function() {
            function initLoginStandalone() {
                if (typeof KingdomConfeitaria === 'undefined' || !KingdomConfeitaria.Utils) {
                    setTimeout(initLoginStandalone, 50);
                    return;
                }
                
                KingdomConfeitaria.Utils.ready(function() {
            var txtLoginStandalone = document.getElementById('txtLoginDinamicoStandalone');
            var txtSenhaStandalone = document.getElementById('txtSenhaStandalone');
            var btnConfirmarLoginStandalone = document.getElementById('btnConfirmarLoginStandalone');
            var clienteEncontradoStandalone = null;
            
            if (!txtLoginStandalone) return;
            
            // Função para mostrar mensagem
            function mostrarMensagemStandalone(mensagem, tipo) {
                var divMensagem = document.getElementById('divMensagemLoginStandalone');
                if (!divMensagem) return;
                divMensagem.style.display = 'block';
                divMensagem.className = 'mt-2 alert alert-' + (tipo || 'info');
                divMensagem.innerHTML = mensagem;
            }
            
            function ocultarMensagemStandalone() {
                var divMensagem = document.getElementById('divMensagemLoginStandalone');
                if (divMensagem) {
                    divMensagem.style.display = 'none';
                    divMensagem.innerHTML = '';
                }
            }
            
            // Função para verificar cliente
            function verificarClienteStandalone(login) {
                if (!login || login.trim() === '') {
                    ocultarMensagemStandalone();
                    return;
                }
                
                if (typeof PageMethods === 'undefined') {
                    // PageMethods não disponível
                    return;
                }
                
                mostrarMensagemStandalone('<i class="fas fa-spinner fa-spin"></i> Verificando...', 'info');
                
                PageMethods.VerificarClienteCadastrado(login, function(result) {
                    if (result && result.existe) {
                        clienteEncontradoStandalone = result.cliente;
                        
                        var divOpcaoCadastro = document.getElementById('divOpcaoCadastroStandalone');
                        var divBotoesLogin = document.getElementById('divBotoesLoginStandalone');
                        var divSenha = document.getElementById('divSenhaStandalone');
                        
                        if (divOpcaoCadastro) divOpcaoCadastro.style.display = 'none';
                        
                        if (result.temSenha) {
                            // Mostrar campo de senha
                            if (divSenha) {
                                divSenha.style.display = 'block';
                                setTimeout(function() {
                                    if (txtSenhaStandalone) txtSenhaStandalone.focus();
                                }, 100);
                            }
                            if (divBotoesLogin) divBotoesLogin.style.display = 'flex';
                            mostrarMensagemStandalone('<i class="fas fa-user-check"></i> Cliente encontrado! Digite sua senha.', 'success');
                        } else {
                            // Cliente sem senha - fazer login automático
                            fazerLoginStandalone(result.cliente);
                        }
                    } else {
                        clienteEncontradoStandalone = null;
                        var divSenha = document.getElementById('divSenhaStandalone');
                        var divBotoesLogin = document.getElementById('divBotoesLoginStandalone');
                        var divOpcaoCadastro = document.getElementById('divOpcaoCadastroStandalone');
                        var linkIrCadastro = document.getElementById('linkIrCadastroStandalone');
                        
                        if (divSenha) divSenha.style.display = 'none';
                        if (divBotoesLogin) divBotoesLogin.style.display = 'none';
                        if (divOpcaoCadastro) divOpcaoCadastro.style.display = 'block';
                        if (linkIrCadastro) {
                            var isEmail = login.indexOf('@') > -1;
                            var loginParam = isEmail ? 'email' : 'telefone';
                            linkIrCadastro.href = 'Cadastro.aspx?' + loginParam + '=' + encodeURIComponent(login);
                        }
                        mostrarMensagemStandalone('<i class="fas fa-info-circle"></i> Cliente não encontrado. Clique no botão abaixo para se cadastrar.', 'info');
                    }
                }, function(error) {
                    // Erro ao verificar cliente
                    mostrarMensagemStandalone('<i class="fas fa-exclamation-triangle"></i> Erro ao verificar cliente. Tente novamente.', 'danger');
                });
            }
            
            // Função para fazer login
            function fazerLoginStandalone(cliente) {
                if (!cliente) return;
                
                if (typeof PageMethods === 'undefined') {
                    // PageMethods não disponível
                    return;
                }
                
                mostrarMensagemStandalone('<i class="fas fa-spinner fa-spin"></i> Fazendo login...', 'info');
                
                PageMethods.FazerLoginSessao(cliente.id, function(result) {
                    if (result && result.sucesso) {
                        ocultarMensagemStandalone();
                        
                        // Atualizar variável global
                        window.usuarioLogado = true;
                        
                        // Atualizar menu do header
                        setTimeout(function() {
                            var linkLogin = document.querySelector('[id*="linkLogin"]');
                            var linkMinhasReservas = document.querySelector('[id*="linkMinhasReservas"]');
                            var linkMeusDados = document.querySelector('[id*="linkMeusDados"]');
                            var linkLogout = document.querySelector('[id*="linkLogout"]');
                            var clienteNome = document.querySelector('[id*="clienteNome"]');
                            
                            if (linkLogin) linkLogin.style.display = 'none';
                            if (linkMinhasReservas) linkMinhasReservas.style.display = 'inline';
                            if (linkMeusDados) linkMeusDados.style.display = 'inline';
                            if (linkLogout) linkLogout.style.display = 'inline';
                            if (clienteNome && cliente.nome) {
                                clienteNome.textContent = 'Olá, ' + cliente.nome;
                                clienteNome.style.display = 'inline';
                            }
                            
                            // Atualizar isAdmin do cliente se retornado
                            if (result.isAdmin !== undefined) {
                                cliente.isAdmin = result.isAdmin;
                            }
                            
                            // Mostrar link de admin se for administrador
                            var linkAdmin = document.querySelector('[id*="linkAdmin"]');
                            if (cliente.isAdmin || (result.isAdmin === true)) {
                                if (linkAdmin) linkAdmin.style.display = 'inline';
                            } else {
                                if (linkAdmin) linkAdmin.style.display = 'none';
                            }
                        }, 100);
                        
                        // Fechar modal e redirecionar para Minhas Reservas imediatamente
                        fecharModalLogin();
                        window.location.href = 'MinhasReservas.aspx';
                    } else {
                        mostrarMensagemStandalone('<i class="fas fa-exclamation-triangle"></i> Erro ao fazer login: ' + (result.mensagem || 'Erro desconhecido'), 'danger');
                    }
                }, function(error) {
                    // Erro ao fazer login
                    mostrarMensagemStandalone('<i class="fas fa-exclamation-triangle"></i> Erro ao fazer login. Tente novamente.', 'danger');
                });
            }
            
            // Função para validar senha
            function validarSenhaStandalone() {
                if (!clienteEncontradoStandalone) {
                    mostrarMensagemStandalone('<i class="fas fa-exclamation-triangle"></i> Cliente não encontrado. Por favor, digite seu email ou telefone primeiro.', 'warning');
                    return;
                }
                
                var senha = txtSenhaStandalone ? txtSenhaStandalone.value : '';
                var login = txtLoginStandalone ? txtLoginStandalone.value.trim() : '';
                
                if (!senha) {
                    mostrarMensagemStandalone('<i class="fas fa-exclamation-triangle"></i> Por favor, digite sua senha.', 'warning');
                    if (txtSenhaStandalone) txtSenhaStandalone.focus();
                    return;
                }
                
                if (typeof PageMethods === 'undefined') {
                    // PageMethods não disponível
                    return;
                }
                
                mostrarMensagemStandalone('<i class="fas fa-spinner fa-spin"></i> Validando senha...', 'info');
                
                PageMethods.ValidarSenhaCliente(login, senha, function(result) {
                    if (result && result.valido) {
                        fazerLoginStandalone(result.cliente);
                    } else {
                        mostrarMensagemStandalone('<i class="fas fa-times-circle"></i> ' + (result.mensagem || 'Senha incorreta.'), 'danger');
                        if (txtSenhaStandalone) {
                            txtSenhaStandalone.value = '';
                            txtSenhaStandalone.focus();
                        }
                    }
                }, function(error) {
                    // Erro ao validar senha
                    mostrarMensagemStandalone('<i class="fas fa-exclamation-triangle"></i> Erro ao validar senha. Tente novamente.', 'danger');
                });
            }
            
            // Event listeners
            var timeoutVerificacao = null;
            
            txtLoginStandalone.addEventListener('input', function() {
                clearTimeout(timeoutVerificacao);
                var login = this.value.trim();
                
                // Filtrar entrada
                var isEmail = login.indexOf('@') > -1 || /[a-zA-Z]/.test(login);
                if (isEmail) {
                    this.value = login.toLowerCase().replace(/[^a-z0-9@._-]/g, '');
                } else {
                    this.value = login.replace(/\D/g, '');
                }
                
                if (login.length >= 3) {
                    timeoutVerificacao = setTimeout(function() {
                        verificarClienteStandalone(login);
                    }, 500);
                } else {
                    ocultarMensagemStandalone();
                    var divSenha = document.getElementById('divSenhaStandalone');
                    var divBotoesLogin = document.getElementById('divBotoesLoginStandalone');
                    var divOpcaoCadastro = document.getElementById('divOpcaoCadastroStandalone');
                    if (divSenha) divSenha.style.display = 'none';
                    if (divBotoesLogin) divBotoesLogin.style.display = 'none';
                    if (divOpcaoCadastro) divOpcaoCadastro.style.display = 'none';
                }
            });
            
            if (txtSenhaStandalone) {
                txtSenhaStandalone.addEventListener('keypress', function(e) {
                    if (e.key === 'Enter') {
                        e.preventDefault();
                        validarSenhaStandalone();
                    }
                });
            }
            
            if (btnConfirmarLoginStandalone) {
                btnConfirmarLoginStandalone.addEventListener('click', function(e) {
                    e.preventDefault();
                    validarSenhaStandalone();
                });
            }
        });
            }
            initLoginStandalone();
        })();
    </script>
</body>
</html>


