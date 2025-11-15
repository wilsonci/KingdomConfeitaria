<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Reserva.aspx.cs" Inherits="KingdomConfeitaria.paginas.Reserva" EnableViewState="false" ViewStateMode="Disabled" %>
<%@ Register Src="~/UserControls/Header.ascx" TagName="Header" TagPrefix="uc" %>
<%@ Register Src="~/UserControls/Footer.ascx" TagName="Footer" TagPrefix="uc" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Finalizar Reserva - Kingdom Confeitaria</title>
    <link href="../Content/bootstrap/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/fontawesome/all.min.css?v=2.0" rel="stylesheet" />
    <link href="../Content/app.css" rel="stylesheet" />
    <link href="../Content/layout.css" rel="stylesheet" />
    <style>
        body {
            background: linear-gradient(135deg, #f5f7fa 0%, #e9ecef 100%);
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
        }
        .container-reserva {
            max-width: 100%;
            margin: 0;
            background: transparent;
            border-radius: 0;
            box-shadow: none;
            padding: 0;
        }
        .btn-voltar {
            position: fixed;
            left: 20px;
            top: 50%;
            transform: translateY(-50%);
            z-index: 1000;
            background: linear-gradient(135deg, #1a4d2e 0%, #2d5a3d 100%);
            color: white;
            border: none;
            width: 50px;
            height: 50px;
            border-radius: 50%;
            font-size: 20px;
            cursor: pointer;
            box-shadow: 0 4px 12px rgba(26, 77, 46, 0.3);
            transition: all 0.3s;
        }
        .btn-voltar:hover {
            transform: translateY(-50%) scale(1.1);
            box-shadow: 0 6px 16px rgba(26, 77, 46, 0.4);
        }
        .secao-data-retirada {
            margin: 20px 0;
            padding: 20px;
            background: #f8f9fa;
            border-radius: 8px;
        }
        .secao-observacoes {
            margin: 20px 0;
        }
        .radio-group-datas {
            display: flex;
            flex-direction: column;
            gap: 10px;
            margin-top: 10px;
        }
        .radio-group-datas .form-check {
            display: flex;
            align-items: center;
            padding: 12px 15px;
            border: 2px solid #dee2e6;
            border-radius: 8px;
            cursor: pointer;
            transition: all 0.2s;
            margin-bottom: 0;
            position: relative;
        }
        .radio-group-datas .form-check:hover {
            border-color: #1a4d2e;
            background: #f0f8f4;
        }
        .radio-group-datas .form-check-input {
            margin-right: 12px;
            margin-top: 0;
            margin-left: 0;
            margin-bottom: 0;
            cursor: pointer;
            position: relative;
            flex-shrink: 0;
            width: 18px;
            height: 18px;
        }
        .radio-group-datas .form-check-label {
            cursor: pointer;
            margin-bottom: 0;
            flex: 1;
            padding: 0;
            line-height: 1.5;
        }
        .radio-group-datas .form-check-input:checked ~ .form-check-label,
        .radio-group-datas .form-check-input:checked + .form-check-label {
            font-weight: 600;
        }
        .radio-group-datas .form-check-input:checked {
            background-color: #1a4d2e;
            border-color: #1a4d2e;
        }
        .radio-group-datas input[type="radio"]:checked ~ .form-check-label {
            font-weight: 600;
        }
        /* Estilo quando radio está selecionado - aplicado via JavaScript */
        .radio-group-datas .form-check.selected {
            border-color: #1a4d2e;
            background: #e8f5e9;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <!-- Header (UserControl) -->
        <uc:Header ID="ucHeader" runat="server" />
        
        <!-- Botão flutuante de voltar -->
        <button type="button" class="btn-voltar" onclick="voltarPagina()" title="Voltar">
            <i class="fas fa-arrow-left"></i>
        </button>
        
        <!-- Conteúdo principal -->
        <div class="main-content-wrapper">
            <div class="main-content">
                <div class="page-container">
                    <div class="page-content">
                        <div class="container-reserva">
            <h2 class="mb-4">
                <i class="fas fa-calendar-check"></i> 
                <span id="tituloReserva">Finalizar Reserva</span>
            </h2>

            <!-- Resumo do Carrinho (sempre visível) -->
            <div class="mb-4 p-3 border rounded" style="background: #f8f9fa;">
                <h5 class="mb-3"><i class="fas fa-shopping-cart"></i> Resumo do Pedido</h5>
                <div id="resumoCarrinho" runat="server">
                    <!-- Itens do carrinho serão preenchidos via code-behind -->
                </div>
                <div class="mt-3 pt-3 border-top">
                    <div class="d-flex justify-content-between align-items-center">
                        <strong>Total:</strong>
                        <strong id="totalReserva" runat="server" style="font-size: 1.2em; color: #1a4d2e;">R$ 0,00</strong>
                    </div>
                </div>
            </div>

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
                
                <!-- Mensagens aparecem aqui -->
                <div id="divMensagemLogin" runat="server" class="mb-3" style="display: none;"></div>
                
                <div id="divOpcaoCadastro" class="mb-3" style="display: none;">
                    <div class="alert alert-info mb-3">
                        <i class="fas fa-info-circle"></i> Cliente não encontrado. Deseja se cadastrar?
                    </div>
                    <a href="Cadastro.aspx" class="btn btn-success btn-sm w-100">
                        <i class="fas fa-user-plus"></i> Ir para Cadastro
                    </a>
                </div>
                
                <!-- Botões da área de login -->
                <div id="divBotoesLogin" class="d-flex gap-2 mb-3 justify-content-center" style="display: none;">
                    <button type="button" class="btn btn-secondary" id="btnCancelarLogin">
                        <i class="fas fa-times"></i> Cancelar
                    </button>
                    <button type="button" class="btn btn-primary" id="btnConfirmarLogin">
                        <i class="fas fa-check"></i> Confirmar
                    </button>
                </div>
                
                <!-- Opções de ajuda -->
                <div class="mt-3 pt-3 border-top">
                    <div class="d-flex flex-column gap-2">
                        <a href="RecuperarSenha.aspx" class="text-decoration-none small text-center">
                            <i class="fas fa-key"></i> Recuperar Senha
                        </a>
                        <a href="../Cadastro.aspx" class="text-decoration-none small text-center">
                            <i class="fas fa-user-plus"></i> Cadastrar Novo Usuário
                        </a>
                    </div>
                </div>
            </div>
            
            <!-- ETAPA 2: Área de Reserva (só aparece após login) -->
            <div id="divDadosReserva" runat="server">
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
                
                <!-- Seção de Data de Retirada -->
                <div class="secao-data-retirada">
                    <asp:Label ID="lblDataRetirada" runat="server" CssClass="form-label">
                        <i class="fas fa-calendar-alt"></i> Data de Retirada *
                    </asp:Label>
                    <div class="radio-group-datas" id="radioGroupDatas" runat="server">
                        <!-- Os radiobuttons serão gerados dinamicamente no code-behind -->
                    </div>
                    <asp:HiddenField ID="hdnDataRetirada" runat="server" />
                </div>
                
                <!-- Seção de Observações -->
                <div class="secao-observacoes">
                    <asp:Label ID="lblObservacoes" runat="server" AssociatedControlID="txtObservacoes" CssClass="form-label">
                        <i class="fas fa-comment-alt"></i> Observações
                    </asp:Label>
                    <asp:TextBox ID="txtObservacoes" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" 
                        placeholder="Adicione observações sobre sua reserva (opcional)" aria-label="Observações"></asp:TextBox>
                    <small class="text-muted mt-2 d-block">
                        <i class="fas fa-info-circle"></i> Informe qualquer detalhe importante sobre sua reserva
                    </small>
                </div>
            </div>

            <!-- Botões de ação -->
            <div class="d-flex gap-2 justify-content-center mt-4" id="divBotoesReserva">
                <button type="button" class="btn btn-secondary" onclick="voltarPagina()">
                    <i class="fas fa-times"></i> Cancelar
                </button>
                <asp:Button ID="btnConfirmarReserva" runat="server" 
                    CssClass="btn btn-primary" 
                    OnClick="btnConfirmarReserva_Click"
                    OnClientClick="return validarFormularioReserva();"
                    Style="display: none;"
                    Text="Confirmar Reserva" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
        
        <!-- Footer (UserControl) -->
        <uc:Footer ID="ucFooter" runat="server" />
    </form>

    <script src="../Scripts/bootstrap/bootstrap.bundle.min.js" defer></script>
    <script src="../Scripts/ajax-helper.js" defer></script>
    <script src="../Scripts/navigation.js" defer></script>
    <script>
        // Adicionar ícone ao botão de confirmar reserva após carregar a página
        (function() {
            function adicionarIconeBotaoConfirmar() {
                var btnConfirmar = document.getElementById('<%= btnConfirmarReserva.ClientID %>');
                if (btnConfirmar && !btnConfirmar.querySelector('i')) {
                    var icon = document.createElement('i');
                    icon.className = 'fas fa-check';
                    btnConfirmar.insertBefore(icon, btnConfirmar.firstChild);
                    btnConfirmar.insertBefore(document.createTextNode(' '), btnConfirmar.childNodes[1]);
                }
            }
            
            // Tentar adicionar quando o DOM estiver pronto
            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', adicionarIconeBotaoConfirmar);
            } else {
                adicionarIconeBotaoConfirmar();
            }
            
            // Tentar novamente após um pequeno delay
            setTimeout(adicionarIconeBotaoConfirmar, 100);
        })();
        
        // Definir ClientIDs para esta página
        (function() {
            var ClientIDs = {
                txtLoginDinamico: '<%= txtLoginDinamico.ClientID %>',
                txtSenhaReserva: '<%= txtSenhaReserva.ClientID %>',
                divSenhaReserva: '<%= divSenhaReserva.ClientID %>',
                divLoginDinamico: '<%= divLoginDinamico.ClientID %>',
                divDadosReserva: '<%= divDadosReserva.ClientID %>',
                divMensagemLogin: '<%= divMensagemLogin.ClientID %>',
                txtNome: '<%= txtNome.ClientID %>',
                txtEmail: '<%= txtEmail.ClientID %>',
                txtTelefone: '<%= txtTelefone.ClientID %>',
                hdnNome: '<%= hdnNome.ClientID %>',
                hdnEmail: '<%= hdnEmail.ClientID %>',
                hdnTelefone: '<%= hdnTelefone.ClientID %>',
                btnConfirmarReserva: '<%= btnConfirmarReserva.ClientID %>'
            };
            window.ClientIDs = ClientIDs;

            // Variáveis globais dentro da IIFE
            var txtLoginDinamico, txtSenhaReserva, divSenhaReserva, divLoginDinamico, divDadosReserva;
            var txtNome, txtEmail, txtTelefone, nomeReserva, emailReserva, telefoneReserva;
            var clienteEncontrado = null;
            var verificacaoEmAndamento = false;
            var timeoutVerificacaoCliente = null;
            var estaLogado = <%= (Session["ClienteId"] != null && !Session.IsNewSession).ToString().ToLower() %>;

            // Funções de mensagem
            function mostrarMensagem(mensagem, tipo) {
                var divMensagem = document.getElementById(ClientIDs.divMensagemLogin);
                if (divMensagem) {
                    divMensagem.innerHTML = '<div class="alert alert-' + tipo + ' alert-dismissible fade show" role="alert">' +
                        mensagem +
                        '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
                        '</div>';
                    divMensagem.style.display = 'block';
                }
            }

            function ocultarMensagem() {
                var divMensagem = document.getElementById(ClientIDs.divMensagemLogin);
                if (divMensagem) {
                    divMensagem.style.display = 'none';
                    divMensagem.innerHTML = '';
                }
            }

            // Função para verificar cliente dinamicamente
            function verificarClienteDinamico() {
                if (estaLogado || verificacaoEmAndamento) return;
                
                if (timeoutVerificacaoCliente) {
                    clearTimeout(timeoutVerificacaoCliente);
                }
                
                timeoutVerificacaoCliente = setTimeout(function() {
                    var login = txtLoginDinamico ? txtLoginDinamico.value.trim() : '';
                    if (!login) {
                        ocultarMensagem();
                        var divSenhaReservaElement = document.getElementById(ClientIDs.divSenhaReserva);
                        if (divSenhaReservaElement) divSenhaReservaElement.style.display = 'none';
                        clienteEncontrado = null;
                        window.clienteEncontrado = null;
                        verificacaoEmAndamento = false;
                        return;
                    }
                    
                    var isEmail = login.indexOf('@') > -1;
                    var loginLimpo = isEmail ? login.toLowerCase() : login.replace(/\D/g, '');
                    
                    if (isEmail && login.length < 5) {
                        verificacaoEmAndamento = false;
                        return;
                    }
                    if (!isEmail && loginLimpo.length < 10) {
                        verificacaoEmAndamento = false;
                        return;
                    }
                    
                    verificacaoEmAndamento = true;
                    
                    KingdomConfeitaria.Ajax.callHandler(
                        '../Handlers/CallbackHandler.ashx',
                        {
                            acao: 'verificarcliente',
                            login: loginLimpo,
                            isEmail: isEmail
                        },
                        'POST',
                        function(result) {
                            verificacaoEmAndamento = false;
                            if (result && result.encontrado) {
                                clienteEncontrado = result.cliente;
                                window.clienteEncontrado = result.cliente;
                                
                                if (result.temSenha) {
                                    var divSenhaReservaElement = document.getElementById(ClientIDs.divSenhaReserva);
                                    if (divSenhaReservaElement) {
                                        divSenhaReservaElement.style.display = 'block';
                                        var txtSenhaReservaElement = document.getElementById(ClientIDs.txtSenhaReserva);
                                        if (txtSenhaReservaElement) {
                                            txtSenhaReservaElement.focus();
                                            txtSenhaReservaElement.required = true;
                                        }
                                    }
                                    
                                    var divBotoesLogin = document.getElementById('divBotoesLogin');
                                    if (divBotoesLogin) {
                                        divBotoesLogin.style.display = 'flex';
                                    }
                                    
                                    mostrarMensagem('<i class="fas fa-lock"></i> Digite sua senha para continuar.', 'info');
                                } else {
                                    ocultarMensagem();
                                    preencherDadosCliente(result.cliente);
                                }
                            } else {
                                clienteEncontrado = null;
                                window.clienteEncontrado = null;
                                var divSenhaReservaElement = document.getElementById(ClientIDs.divSenhaReserva);
                                if (divSenhaReservaElement) {
                                    divSenhaReservaElement.style.display = 'none';
                                }
                                var divBotoesLogin = document.getElementById('divBotoesLogin');
                                if (divBotoesLogin) {
                                    divBotoesLogin.style.display = 'none';
                                }
                                var divOpcaoCadastro = document.getElementById('divOpcaoCadastro');
                                if (divOpcaoCadastro) {
                                    divOpcaoCadastro.style.display = 'block';
                                }
                                mostrarMensagem('<i class="fas fa-info-circle"></i> Cliente não encontrado. Deseja se cadastrar?', 'info');
                            }
                        },
                        function(error) {
                            verificacaoEmAndamento = false;
                            ocultarMensagem();
                            mostrarMensagem('<i class="fas fa-exclamation-triangle"></i> Erro ao verificar cliente. Tente novamente.', 'danger');
                        }
                    );
                }, 500);
            }

            // Função para preencher dados do cliente
            function preencherDadosCliente(cliente) {
                if (!cliente) return;
                
                KingdomConfeitaria.Ajax.callHandler(
                    '../Handlers/CallbackHandler.ashx',
                    {
                        acao: 'fazerlogin',
                        clienteId: cliente.id
                    },
                    'POST',
                    function(result) {
                        if (result && result.sucesso) {
                            if (txtNome && cliente.nome) {
                                txtNome.value = cliente.nome;
                                var hdnNome = document.getElementById(ClientIDs.hdnNome);
                                if (hdnNome) hdnNome.value = cliente.nome;
                            }
                            if (txtEmail && cliente.email) {
                                txtEmail.value = cliente.email;
                                var hdnEmail = document.getElementById(ClientIDs.hdnEmail);
                                if (hdnEmail) hdnEmail.value = cliente.email;
                            }
                            if (txtTelefone && cliente.telefone) {
                                var telFormatado = cliente.telefone.replace(/\D/g, '');
                                if (telFormatado.length <= 10) {
                                    telFormatado = telFormatado.replace(/^(\d{2})(\d{4})(\d{0,4}).*/, '($1) $2-$3');
                                } else {
                                    telFormatado = telFormatado.replace(/^(\d{2})(\d{5})(\d{0,4}).*/, '($1) $2-$3');
                                }
                                txtTelefone.value = telFormatado;
                                var hdnTelefone = document.getElementById(ClientIDs.hdnTelefone);
                                if (hdnTelefone) hdnTelefone.value = cliente.telefone.replace(/\D/g, '');
                            }
                            
                            var divLoginDinamicoElement = document.getElementById(ClientIDs.divLoginDinamico);
                            if (divLoginDinamicoElement) {
                                divLoginDinamicoElement.style.display = 'none';
                            }
                            
                            var divDadosReservaElement = document.getElementById(ClientIDs.divDadosReserva);
                            if (divDadosReservaElement) {
                                divDadosReservaElement.style.display = 'block';
                            }
                            
                            var btnConfirmarReserva = document.getElementById(ClientIDs.btnConfirmarReserva);
                            if (btnConfirmarReserva) {
                                btnConfirmarReserva.style.display = 'inline-block';
                            }
                            
                            document.getElementById('tituloReserva').textContent = 'Finalizar Reserva';
                            ocultarMensagem();
                            
                            // Recarregar página para atualizar estado e mostrar dados completos
                            setTimeout(function() {
                                window.location.reload();
                            }, 500);
                        }
                    },
                    function(error) {
                        mostrarMensagem('<i class="fas fa-exclamation-triangle"></i> Erro ao fazer login. Tente novamente.', 'danger');
                    }
                );
            }

            // Função para validar senha
            window.validarSenha = function() {
                if (!window.clienteEncontrado) {
                    alert('Cliente não encontrado. Por favor, digite seu email ou telefone primeiro.');
                    return;
                }
                
                var txtSenhaReservaElement = document.getElementById(ClientIDs.txtSenhaReserva);
                var txtLoginDinamicoElement = document.getElementById(ClientIDs.txtLoginDinamico);
                
                var senha = txtSenhaReservaElement ? txtSenhaReservaElement.value : '';
                var login = txtLoginDinamicoElement ? txtLoginDinamicoElement.value.trim() : '';
                
                if (!senha) {
                    mostrarMensagem('<i class="fas fa-exclamation-triangle"></i> Por favor, digite sua senha.', 'warning');
                    if (txtSenhaReservaElement) txtSenhaReservaElement.focus();
                    return;
                }
                
                mostrarMensagem('<i class="fas fa-spinner fa-spin"></i> Validando senha...', 'info');
                
                KingdomConfeitaria.Ajax.callHandler(
                    '../Handlers/CallbackHandler.ashx',
                    {
                        acao: 'validarsenha',
                        login: login,
                        senha: senha
                    },
                    'POST',
                    function(result) {
                        if (result && result.valido) {
                            ocultarMensagem();
                            preencherDadosCliente(result.cliente);
                        } else {
                            mostrarMensagem('<i class="fas fa-times-circle"></i> ' + (result.mensagem || 'Senha incorreta.'), 'danger');
                            if (txtSenhaReservaElement) {
                                txtSenhaReservaElement.value = '';
                                txtSenhaReservaElement.focus();
                            }
                        }
                    },
                    function(error) {
                        mostrarMensagem('<i class="fas fa-exclamation-triangle"></i> Erro ao validar senha. Tente novamente.', 'danger');
                    }
                );
            };

            // Função para validar formulário de reserva
            window.validarFormularioReserva = function() {
                var dataRetirada = document.getElementById('<%= hdnDataRetirada.ClientID %>');
                if (!dataRetirada || !dataRetirada.value) {
                    alert('Por favor, selecione uma data de retirada.');
                    return false;
                }
                return true;
            };

            // Inicializar quando o DOM estiver pronto
            function initLoginDinamico() {
                txtLoginDinamico = document.getElementById(ClientIDs.txtLoginDinamico);
                txtSenhaReserva = document.getElementById(ClientIDs.txtSenhaReserva);
                divSenhaReserva = document.getElementById(ClientIDs.divSenhaReserva);
                divLoginDinamico = document.getElementById(ClientIDs.divLoginDinamico);
                divDadosReserva = document.getElementById(ClientIDs.divDadosReserva);
                txtNome = document.getElementById(ClientIDs.txtNome);
                txtEmail = document.getElementById(ClientIDs.txtEmail);
                txtTelefone = document.getElementById(ClientIDs.txtTelefone);
                
                if (txtLoginDinamico && !estaLogado) {
                    txtLoginDinamico.addEventListener('input', verificarClienteDinamico);
                    txtLoginDinamico.addEventListener('keypress', function(e) {
                        if (e.key === 'Enter') {
                            e.preventDefault();
                            if (window.clienteEncontrado && window.clienteEncontrado.temSenha) {
                                window.validarSenha();
                            } else if (window.clienteEncontrado && !window.clienteEncontrado.temSenha) {
                                preencherDadosCliente(window.clienteEncontrado);
                            }
                        }
                    });
                }
                
                var btnConfirmarLogin = document.getElementById('btnConfirmarLogin');
                if (btnConfirmarLogin) {
                    btnConfirmarLogin.addEventListener('click', window.validarSenha);
                }
                
                var btnCancelarLogin = document.getElementById('btnCancelarLogin');
                if (btnCancelarLogin) {
                    btnCancelarLogin.addEventListener('click', function() {
                        if (txtLoginDinamico) txtLoginDinamico.value = '';
                        if (txtSenhaReserva) txtSenhaReserva.value = '';
                        ocultarMensagem();
                        var divSenhaReservaElement = document.getElementById(ClientIDs.divSenhaReserva);
                        if (divSenhaReservaElement) divSenhaReservaElement.style.display = 'none';
                        var divBotoesLogin = document.getElementById('divBotoesLogin');
                        if (divBotoesLogin) divBotoesLogin.style.display = 'none';
                        clienteEncontrado = null;
                        window.clienteEncontrado = null;
                    });
                }
            }

            // Função para voltar
            function voltarPagina() {
                // Salvar estado antes de voltar
                if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Navigation) {
                    KingdomConfeitaria.Navigation.salvarEstadoControles();
                }
                // Usar navegação do sistema
                if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Navigation) {
                    KingdomConfeitaria.Navigation.voltar();
                } else {
                    // Fallback
                    window.history.back();
                }
            }
            
            // Função antiga mantida para compatibilidade
            function voltarPaginaOld() {
                if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Navigation) {
                    KingdomConfeitaria.Navigation.voltar();
                } else {
                    if (window.history.length > 1) {
                        window.history.back();
                    } else {
                        window.location.href = '../Default.aspx';
                    }
                }
            }
            window.voltarPagina = voltarPagina;

            // Inicializar quando o DOM estiver pronto
            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', initLoginDinamico);
            } else {
                initLoginDinamico();
            }
            
            // Configurar eventos dos radio buttons para destacar seleção
            function configurarRadioButtons() {
                var radioGroup = document.getElementById('<%= radioGroupDatas.ClientID %>');
                if (radioGroup) {
                    var radios = radioGroup.querySelectorAll('input[type="radio"]');
                    radios.forEach(function(radio) {
                        radio.addEventListener('change', function() {
                            // Remover classe 'selected' de todos
                            var allChecks = radioGroup.querySelectorAll('.form-check');
                            allChecks.forEach(function(check) {
                                check.classList.remove('selected');
                            });
                            // Adicionar classe 'selected' ao pai do radio selecionado
                            if (this.checked && this.parentElement) {
                                this.parentElement.classList.add('selected');
                            }
                        });
                        // Verificar se já está selecionado ao carregar
                        if (radio.checked && radio.parentElement) {
                            radio.parentElement.classList.add('selected');
                        }
                    });
                }
            }
            
            // Executar após o DOM estar pronto
            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', configurarRadioButtons);
            } else {
                setTimeout(configurarRadioButtons, 100);
            }
        })();
    </script>
</body>
</html>

