<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Admin.aspx.cs" Inherits="KingdomConfeitaria.Admin" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Administração - Kingdom Confeitaria</title>
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
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            width: 100%;
            z-index: 1000;
            box-shadow: 0 2px 10px rgba(0,0,0,0.2);
        }
        .header-logo img {
            max-width: 20%;
            width: auto;
            height: auto;
            max-height: 80px;
        }
        .container-main {
            background: white;
            border-radius: 20px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
            margin: 90px auto 20px auto;
            padding: 30px;
        }
        .nav-tabs .nav-link {
            color: #1a4d2e;
            font-weight: 600;
        }
        .nav-tabs .nav-link.active {
            background-color: #1a4d2e;
            color: white;
            border-color: #1a4d2e;
        }
        .produto-admin-card, .reserva-card {
            border: 2px solid #e9ecef;
            border-radius: 15px;
            padding: 20px;
            margin-bottom: 20px;
        }
        .produto-imagem-admin {
            max-width: 200px;
            max-height: 200px;
            border-radius: 10px;
        }
        .status-badge {
            padding: 5px 10px;
            border-radius: 5px;
            font-size: 0.85em;
            font-weight: 600;
        }
        .status-pendente { background-color: #ffc107; color: #000; }
        .status-confirmado { background-color: #17a2b8; color: #fff; }
        .status-pronto { background-color: #28a745; color: #fff; }
        .status-entregue { background-color: #6c757d; color: #fff; }
        .status-cancelado { background-color: #dc3545; color: #fff; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container-fluid">
            <div class="header-logo">
                <img src="Images/logo-kingdom-confeitaria.svg" alt="Kingdom Confeitaria" style="max-width: 100%; height: auto;" />
            </div>
            <div class="container-main">
                <div class="d-flex justify-content-between align-items-center mb-4">
                    <h1><i class="fas fa-cog"></i> Administração</h1>
                    <a href="Default.aspx" class="btn btn-outline-primary">
                        <i class="fas fa-home"></i> Voltar para o Site
                    </a>
                </div>

                <ul class="nav nav-tabs mb-4" id="adminTabs" role="tablist">
                    <li class="nav-item" role="presentation">
                        <button class="nav-link active" id="resumo-tab" data-bs-toggle="tab" data-bs-target="#resumo" type="button" role="tab">
                            <i class="fas fa-chart-line"></i> Resumo
                        </button>
                    </li>
                    <li class="nav-item" role="presentation">
                        <button class="nav-link" id="produtos-tab" data-bs-toggle="tab" data-bs-target="#produtos" type="button" role="tab">
                            <i class="fas fa-cookie-bite"></i> Produtos
                        </button>
                    </li>
                    <li class="nav-item" role="presentation">
                        <button class="nav-link" id="reservas-tab" data-bs-toggle="tab" data-bs-target="#reservas" type="button" role="tab">
                            <i class="fas fa-clipboard-list"></i> Reservas
                        </button>
                    </li>
                </ul>

                <div class="alert alert-warning mb-3">
                    <strong><i class="fas fa-exclamation-triangle"></i> Ações Administrativas:</strong>
                    <asp:Button ID="btnLimparDados" runat="server" 
                        Text="Limpar Todos os Clientes e Reservas" 
                        CssClass="btn btn-danger btn-sm ms-2" 
                        OnClick="btnLimparDados_Click"
                        OnClientClick="return confirm('ATENÇÃO: Esta ação irá apagar TODOS os clientes e reservas do banco de dados. Esta ação não pode ser desfeita. Deseja continuar?');" />
                </div>

                <div id="alertContainer" runat="server"></div>

                <div class="tab-content" id="adminTabContent">
                    <!-- Aba Resumo -->
                    <div class="tab-pane fade show active" id="resumo" role="tabpanel">
                        <div id="resumoContainer" runat="server">
                            <!-- Resumo será carregado aqui -->
                        </div>
                    </div>
                    
                    <!-- Aba Produtos -->
                    <div class="tab-pane fade" id="produtos" role="tabpanel">
                        <div class="mb-3">
                            <button type="button" class="btn btn-success" data-bs-toggle="modal" data-bs-target="#modalNovoProduto">
                                <i class="fas fa-plus"></i> Novo Produto
                            </button>
                        </div>
                        <div id="produtosAdminContainer" runat="server">
                            <!-- Produtos serão carregados aqui -->
                        </div>
                    </div>

                    <!-- Aba Reservas -->
                    <div class="tab-pane fade" id="reservas" role="tabpanel">
                        <div id="reservasContainer" runat="server">
                            <!-- Reservas serão carregadas aqui -->
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal Editar Produto -->
        <div class="modal fade" id="modalEditarProduto" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Editar Produto</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <asp:HiddenField ID="hdnProdutoId" runat="server" />
                        <div class="mb-3">
                            <label class="form-label">Nome *</label>
                            <asp:TextBox ID="txtNomeProduto" runat="server" CssClass="form-control" required></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Descrição</label>
                            <asp:TextBox ID="txtDescricao" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"></asp:TextBox>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <label class="form-label">Preço Pequeno (R$) *</label>
                                    <asp:TextBox ID="txtPrecoPequeno" runat="server" CssClass="form-control" TextMode="Number" step="0.01" required></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <label class="form-label">Preço Grande (R$) *</label>
                                    <asp:TextBox ID="txtPrecoGrande" runat="server" CssClass="form-control" TextMode="Number" step="0.01" required></asp:TextBox>
                                </div>
                            </div>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">URL da Imagem *</label>
                            <asp:TextBox ID="txtImagemUrl" runat="server" CssClass="form-control" required></asp:TextBox>
                            <small class="text-muted">Cole aqui a URL da imagem (pode ser do Google Drive, Imgur, etc)</small>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Ordem de Exibição</label>
                            <asp:TextBox ID="txtOrdem" runat="server" CssClass="form-control" TextMode="Number" value="0"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <div class="form-check">
                                <asp:CheckBox ID="chkAtivo" runat="server" CssClass="form-check-input" Checked="true" />
                                <label class="form-check-label" for="<%= chkAtivo.ClientID %>">
                                    Produto Ativo
                                </label>
                            </div>
                        </div>
                        <div class="mb-3">
                            <label>Preview da Imagem:</label><br />
                            <img id="previewImagem" src="" alt="Preview" class="produto-imagem-admin" style="display: none;" />
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                        <asp:Button ID="btnSalvarProduto" runat="server" Text="Salvar" CssClass="btn btn-primary" OnClick="btnSalvarProduto_Click" />
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal Novo Produto -->
        <div class="modal fade" id="modalNovoProduto" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Novo Produto</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <div class="mb-3">
                            <label class="form-label">Nome *</label>
                            <asp:TextBox ID="txtNovoNome" runat="server" CssClass="form-control" required></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Descrição</label>
                            <asp:TextBox ID="txtNovaDescricao" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"></asp:TextBox>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <label class="form-label">Preço Pequeno (R$) *</label>
                                    <asp:TextBox ID="txtNovoPrecoPequeno" runat="server" CssClass="form-control" TextMode="Number" step="0.01" required></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <label class="form-label">Preço Grande (R$) *</label>
                                    <asp:TextBox ID="txtNovoPrecoGrande" runat="server" CssClass="form-control" TextMode="Number" step="0.01" required></asp:TextBox>
                                </div>
                            </div>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">URL da Imagem *</label>
                            <asp:TextBox ID="txtNovaImagemUrl" runat="server" CssClass="form-control" required></asp:TextBox>
                            <small class="text-muted">Cole aqui a URL da imagem</small>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Ordem de Exibição</label>
                            <asp:TextBox ID="txtNovaOrdem" runat="server" CssClass="form-control" TextMode="Number" value="0"></asp:TextBox>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                        <asp:Button ID="btnSalvarNovoProduto" runat="server" Text="Salvar" CssClass="btn btn-primary" OnClick="btnSalvarNovoProduto_Click" />
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal Editar Reserva -->
        <div class="modal fade" id="modalEditarReserva" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Editar Reserva</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <asp:HiddenField ID="hdnReservaId" runat="server" />
                        <div class="row">
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <label class="form-label">Status *</label>
                                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                                        <asp:ListItem Value="Pendente" Text="Pendente"></asp:ListItem>
                                        <asp:ListItem Value="Confirmado" Text="Confirmado"></asp:ListItem>
                                        <asp:ListItem Value="Pronto" Text="Pronto"></asp:ListItem>
                                        <asp:ListItem Value="Entregue" Text="Entregue"></asp:ListItem>
                                        <asp:ListItem Value="Cancelado" Text="Cancelado"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <label class="form-label">Valor Total (R$)</label>
                                    <asp:TextBox ID="txtValorTotal" runat="server" CssClass="form-control" TextMode="Number" step="0.01"></asp:TextBox>
                                </div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <div class="form-check">
                                        <asp:CheckBox ID="chkConvertidoEmPedido" runat="server" CssClass="form-check-input" />
                                        <label class="form-check-label" for="<%= chkConvertidoEmPedido.ClientID %>">
                                            Convertido em Pedido
                                        </label>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <div class="form-check">
                                        <asp:CheckBox ID="chkCancelado" runat="server" CssClass="form-check-input" />
                                        <label class="form-check-label" for="<%= chkCancelado.ClientID %>">
                                            Cancelado
                                        </label>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Previsão de Entrega</label>
                            <asp:TextBox ID="txtPrevisaoEntrega" runat="server" CssClass="form-control" TextMode="DateTimeLocal"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Observações</label>
                            <asp:TextBox ID="txtObservacoesReserva" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"></asp:TextBox>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                        <asp:Button ID="btnSalvarReserva" runat="server" Text="Salvar" CssClass="btn btn-primary" OnClick="btnSalvarReserva_Click" />
                    </div>
                </div>
            </div>
        </div>
    </form>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <!-- Garantir que __doPostBack esteja disponível -->
    <script type="text/javascript">
        if (typeof __doPostBack === 'undefined') {
            function __doPostBack(eventTarget, eventArgument) {
                if (!eventTarget) return false;
                var form = document.getElementById('form1');
                if (!form) {
                    console.error('Formulário form1 não encontrado');
                    return false;
                }
                
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
    
    <!-- Scripts comuns da aplicação -->
    <script src="Scripts/app.js"></script>
    <!-- Scripts específicos da página Admin -->
    <script src="Scripts/admin.js"></script>
    <script>
        // Scripts inline apenas para dados dinâmicos do servidor (ClientIDs)
        (function() {
            function init() {
                // Preview de imagem ao digitar URL
                var txtImagemUrl = document.getElementById('<%= txtImagemUrl.ClientID %>');
                if (txtImagemUrl) {
                    txtImagemUrl.addEventListener('input', function() {
                        if (typeof AdminPage !== 'undefined' && AdminPage.Produtos) {
                            AdminPage.Produtos.atualizarPreview(this);
                        }
                    });
                }
                
                var txtNovaImagemUrl = document.getElementById('<%= txtNovaImagemUrl.ClientID %>');
                if (txtNovaImagemUrl) {
                    txtNovaImagemUrl.addEventListener('input', function() {
                        if (typeof AdminPage !== 'undefined' && AdminPage.Produtos) {
                            AdminPage.Produtos.atualizarPreview(this);
                        }
                    });
                }
            }
            
            // Executar quando DOM estiver pronto
            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', init);
            } else {
                init();
            }
        })();
    </script>
</body>
</html>
