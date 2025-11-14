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
            padding: 12px 16px;
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            width: 100%;
            z-index: 1000;
            box-shadow: 0 2px 10px rgba(0,0,0,0.2);
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
            color: white;
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
            }
            .header-actions a {
                font-size: 11px;
                padding: 4px 8px;
            }
            .header-actions a i {
                display: none;
            }
        }
        .container-main {
            background: white;
            border-radius: 20px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
            margin: 100px auto 20px auto;
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
            width: 200px;
            height: 200px;
            object-fit: cover;
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
        
        /* Estilos para visualização de logs em árvore */
        .log-tree {
            font-family: 'Courier New', monospace;
            font-size: 0.9em;
        }
        .log-user-group {
            margin-bottom: 15px;
        }
        .log-user-header {
            background-color: #1a4d2e;
            color: white;
            padding: 10px 15px;
            border-radius: 5px;
            cursor: pointer;
            font-weight: 600;
            margin-bottom: 5px;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }
        .log-user-header:hover {
            background-color: #2d5a3d;
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
            .btn-voltar {
                left: 10px;
                width: 50px;
                height: 50px;
                font-size: 1.2rem;
            }
        }
        .log-user-header i {
            transition: transform 0.3s;
        }
        .log-user-header.collapsed i {
            transform: rotate(-90deg);
        }
        .log-user-content {
            margin-left: 20px;
            border-left: 2px solid #1a4d2e;
            padding-left: 15px;
        }
        .log-entity-group {
            margin-bottom: 10px;
        }
        .log-entity-header {
            background-color: #6c757d;
            color: white;
            padding: 8px 12px;
            border-radius: 5px;
            cursor: pointer;
            font-weight: 500;
            margin-bottom: 5px;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }
        .log-entity-header:hover {
            background-color: #5a6268;
        }
        .log-entity-header i {
            transition: transform 0.3s;
        }
        .log-entity-header.collapsed i {
            transform: rotate(-90deg);
        }
        .log-entity-content {
            margin-left: 20px;
            border-left: 2px solid #6c757d;
            padding-left: 12px;
        }
        .log-entry {
            background-color: #f8f9fa;
            border: 1px solid #dee2e6;
            border-radius: 4px;
            padding: 8px 10px;
            margin-bottom: 5px;
            font-size: 0.85em;
        }
        .log-entry:hover {
            background-color: #e9ecef;
        }
        .log-timestamp {
            color: #6c757d;
            font-weight: 600;
            margin-right: 10px;
        }
        .log-tipo {
            display: inline-block;
            padding: 2px 6px;
            border-radius: 3px;
            font-size: 0.75em;
            font-weight: 600;
            margin-right: 8px;
        }
        .log-tipo-INSERT { background-color: #28a745; color: white; }
        .log-tipo-UPDATE { background-color: #ffc107; color: #000; }
        .log-tipo-DELETE { background-color: #dc3545; color: white; }
        .log-tipo-CANCEL { background-color: #fd7e14; color: white; }
        .log-tipo-LOGIN { background-color: #17a2b8; color: white; }
        .log-tipo-LOGOUT { background-color: #6c757d; color: white; }
        .log-onde {
            color: #007bff;
            font-style: italic;
            margin-left: 8px;
        }
        .log-detalhes {
            color: #495057;
            margin-top: 5px;
            padding-left: 15px;
            font-size: 0.9em;
        }
        .log-count {
            background-color: rgba(255, 255, 255, 0.3);
            padding: 2px 8px;
            border-radius: 12px;
            font-size: 0.85em;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server" enctype="multipart/form-data">
        <!-- Botão de voltar na lateral esquerda -->
        <a href="javascript:void(0);" class="btn-voltar" title="Voltar" onclick="voltarPagina(); return false;">
            <i class="fas fa-arrow-left"></i>
        </a>
        <div class="container-fluid">
            <div class="header-logo">
                <div class="header-top">
                    <a href="Default.aspx" style="text-decoration: none; display: inline-block;">
                        <img src="Images/logo-kingdom-confeitaria.svg" alt="Kingdom Confeitaria" style="cursor: pointer;" />
                    </a>
                    <div class="header-user-name" id="clienteNome" runat="server" style="display: none;"></div>
                </div>
                <div class="header-actions">
                    <a href="Default.aspx"><i class="fas fa-home"></i> Home</a>
                    <a href="#" id="linkLogin" runat="server" style="display: none;" onclick="abrirModalLogin(); return false;"><i class="fas fa-sign-in-alt"></i> Entrar</a>
                    <a href="MinhasReservas.aspx" id="linkMinhasReservas" runat="server" style="display: none;"><i class="fas fa-clipboard-list"></i> Minhas Reservas</a>
                    <a href="MeusDados.aspx" id="linkMeusDados" runat="server" style="display: none;"><i class="fas fa-user"></i> Meus Dados</a>
                    <a href="Admin.aspx" id="linkAdmin" runat="server" style="display: none;"><i class="fas fa-cog"></i> Painel Gestor</a>
                    <a href="Logout.aspx" id="linkLogout" runat="server" style="display: none;"><i class="fas fa-sign-out-alt"></i> Sair</a>
                </div>
            </div>
            <div class="container-main">
                <div class="d-flex justify-content-between align-items-center mb-4">
                    <h1><i class="fas fa-cog"></i> Administração</h1>
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
                        <button class="nav-link" id="clientes-tab" data-bs-toggle="tab" data-bs-target="#clientes" type="button" role="tab">
                            <i class="fas fa-users"></i> Clientes
                        </button>
                    </li>
                    <li class="nav-item" role="presentation">
                        <button class="nav-link" id="reservas-tab" data-bs-toggle="tab" data-bs-target="#reservas" type="button" role="tab">
                            <i class="fas fa-clipboard-list"></i> Reservas
                        </button>
                    </li>
                    <li class="nav-item" role="presentation">
                        <button class="nav-link" id="tabelas-tab" data-bs-toggle="tab" data-bs-target="#tabelas" type="button" role="tab">
                            <i class="fas fa-tag"></i> Status
                        </button>
                    </li>
                    <li class="nav-item" role="presentation">
                        <button class="nav-link" id="logs-tab" data-bs-toggle="tab" data-bs-target="#logs" type="button" role="tab">
                            <i class="fas fa-file-alt"></i> Logs
                        </button>
                    </li>
                    <li class="nav-item" role="presentation">
                        <button class="nav-link" id="configuracoes-tab" data-bs-toggle="tab" data-bs-target="#configuracoes" type="button" role="tab">
                            <i class="fas fa-cog"></i> Configurações
                        </button>
                    </li>
                </ul>

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

                    <!-- Aba Clientes -->
                    <div class="tab-pane fade" id="clientes" role="tabpanel">
                        <div id="clientesContainer" runat="server">
                            <!-- Clientes serão carregados aqui -->
                        </div>
                    </div>

                    <!-- Aba Reservas -->
                    <div class="tab-pane fade" id="reservas" role="tabpanel">
                        <div id="reservasContainer" runat="server">
                            <!-- Reservas serão carregadas aqui -->
                        </div>
                    </div>

                    <!-- Aba Status -->
                    <div class="tab-pane fade" id="tabelas" role="tabpanel">
                        <ul class="nav nav-pills mb-3" id="tabelasTabs" role="tablist">
                            <li class="nav-item" role="presentation">
                                <button class="nav-link active" id="statusreserva-tab" data-bs-toggle="pill" data-bs-target="#statusreserva" type="button" role="tab">
                                    <i class="fas fa-tag"></i> Status de Reserva
                                </button>
                            </li>
                        </ul>
                        <div class="tab-content" id="tabelasTabContent">
                            <div class="tab-pane fade show active" id="statusreserva" role="tabpanel">
                                <div class="mb-3">
                                    <button type="button" class="btn btn-success" data-bs-toggle="modal" data-bs-target="#modalNovoStatusReserva">
                                        <i class="fas fa-plus"></i> Novo Status
                                    </button>
                                </div>
                                <div class="table-responsive">
                                    <table class="table table-striped table-hover">
                                        <thead class="table-dark">
                                            <tr>
                                                <th>ID</th>
                                                <th>Nome</th>
                                                <th>Descrição</th>
                                                <th>Permite Alteração</th>
                                                <th>Permite Exclusão</th>
                                                <th>Ordem</th>
                                                <th>Ações</th>
                                            </tr>
                                        </thead>
                                        <tbody id="statusReservaTableBody" runat="server">
                                            <!-- Status serão carregados aqui -->
                                        </tbody>
                                    </table>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Aba Logs -->
                    <div class="tab-pane fade" id="logs" role="tabpanel">
                        <div class="mb-3">
                            <div class="row">
                                <div class="col-md-4">
                                    <label for="diasLogs" class="form-label">Período (dias):</label>
                                    <select class="form-select" id="diasLogs" onchange="carregarLogs()">
                                        <option value="1">Último dia</option>
                                        <option value="3">Últimos 3 dias</option>
                                        <option value="7" selected>Últimos 7 dias</option>
                                        <option value="15">Últimos 15 dias</option>
                                        <option value="30">Últimos 30 dias</option>
                                    </select>
                                </div>
                                <div class="col-md-4 d-flex align-items-end">
                                    <button type="button" class="btn btn-primary" onclick="carregarLogs()">
                                        <i class="fas fa-sync-alt"></i> Atualizar
                                    </button>
                                </div>
                            </div>
                        </div>
                        <div id="logsContainer" runat="server">
                            <!-- Logs serão carregados aqui -->
                        </div>
                    </div>
                    
                    <!-- Aba Configurações -->
                    <div class="tab-pane fade" id="configuracoes" role="tabpanel">
                        <div class="card">
                            <div class="card-header bg-primary text-white">
                                <h5 class="mb-0"><i class="fas fa-cog"></i> Configurações do Sistema</h5>
                            </div>
                            <div class="card-body">
                                <div class="row">
                                    <div class="col-md-12 mb-3">
                                        <h6 class="border-bottom pb-2"><i class="fas fa-globe"></i> Configurações Gerais</h6>
                                    </div>
                                </div>
                                
                                <div class="row">
                                    <div class="col-md-6 mb-3">
                                        <label class="form-label">URL Base do Sistema *</label>
                                        <asp:TextBox ID="txtBaseUrl" runat="server" CssClass="form-control" placeholder="https://kingdomconfeitaria.com.br"></asp:TextBox>
                                        <small class="text-muted">URL completa do site (usado em emails e links)</small>
                                    </div>
                                    <div class="col-md-6 mb-3">
                                        <label class="form-label">Ambiente</label>
                                        <asp:DropDownList ID="ddlEnvironment" runat="server" CssClass="form-select">
                                            <asp:ListItem Value="Development">Desenvolvimento</asp:ListItem>
                                            <asp:ListItem Value="Production">Produção</asp:ListItem>
                                        </asp:DropDownList>
                                        <small class="text-muted">Ambiente atual do sistema</small>
                                    </div>
                                </div>
                                
                                <div class="row">
                                    <div class="col-md-12 mb-3">
                                        <h6 class="border-bottom pb-2 mt-4"><i class="fas fa-envelope"></i> Configurações de Email</h6>
                                    </div>
                                </div>
                                
                                <div class="row">
                                    <div class="col-md-6 mb-3">
                                        <label class="form-label">Servidor SMTP *</label>
                                        <asp:TextBox ID="txtSmtpServer" runat="server" CssClass="form-control" placeholder="smtp.gmail.com"></asp:TextBox>
                                    </div>
                                    <div class="col-md-6 mb-3">
                                        <label class="form-label">Porta SMTP *</label>
                                        <asp:TextBox ID="txtSmtpPort" runat="server" CssClass="form-control" TextMode="Number" placeholder="587"></asp:TextBox>
                                    </div>
                                </div>
                                
                                <div class="row">
                                    <div class="col-md-6 mb-3">
                                        <label class="form-label">Usuário SMTP (Email) *</label>
                                        <asp:TextBox ID="txtSmtpUsername" runat="server" CssClass="form-control" TextMode="Email" placeholder="seuemail@gmail.com"></asp:TextBox>
                                    </div>
                                    <div class="col-md-6 mb-3">
                                        <label class="form-label">Senha SMTP *</label>
                                        <asp:TextBox ID="txtSmtpPassword" runat="server" CssClass="form-control" TextMode="Password" placeholder="Senha de App do Gmail"></asp:TextBox>
                                        <small class="text-muted">Para Gmail, use uma Senha de App (não a senha normal)</small>
                                    </div>
                                </div>
                                
                                <div class="row">
                                    <div class="col-md-4 mb-3">
                                        <label class="form-label">Email Remetente *</label>
                                        <asp:TextBox ID="txtEmailFrom" runat="server" CssClass="form-control" TextMode="Email" placeholder="noreply@kingdomconfeitaria.com.br"></asp:TextBox>
                                    </div>
                                    <div class="col-md-4 mb-3">
                                        <label class="form-label">Email Isabela</label>
                                        <asp:TextBox ID="txtEmailIsabela" runat="server" CssClass="form-control" TextMode="Email" placeholder="isabela@kingdomconfeitaria.com.br"></asp:TextBox>
                                    </div>
                                    <div class="col-md-4 mb-3">
                                        <label class="form-label">Email Camila</label>
                                        <asp:TextBox ID="txtEmailCamila" runat="server" CssClass="form-control" TextMode="Email" placeholder="camila@kingdomconfeitaria.com.br"></asp:TextBox>
                                    </div>
                                </div>
                                
                                <div class="row mt-4">
                                    <div class="col-md-12">
                                        <asp:Button ID="btnSalvarConfiguracoes" runat="server" Text="Salvar Configurações" CssClass="btn btn-primary btn-lg" OnClick="btnSalvarConfiguracoes_Click" />
                                        <button type="button" class="btn btn-secondary btn-lg" onclick="alert('Funcionalidade de teste de email será implementada em breve.');">Testar Email</button>
                                    </div>
                                </div>
                                
                                <div class="alert alert-info mt-3">
                                    <i class="fas fa-info-circle"></i> <strong>Nota:</strong> As configurações são salvas no arquivo web.config. 
                                    Para alterações em produção, edite o arquivo Web.Release.config ou o web.config diretamente no servidor.
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal Editar Produto -->
        <div class="modal fade" id="modalEditarProduto" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-xl modal-dialog-scrollable">
                <div class="modal-content">
                    <div class="modal-header bg-primary text-white">
                        <h5 class="modal-title">
                            <i class="fas fa-edit"></i> Editar Produto
                        </h5>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <asp:HiddenField ID="hdnProdutoId" runat="server" />
                        
                        <div class="row">
                            <!-- Coluna Esquerda: Informações Básicas -->
                            <div class="col-lg-8">
                                <!-- Seção: Informações Básicas -->
                                <div class="card mb-4 border-0 shadow-sm">
                                    <div class="card-header bg-light">
                                        <h6 class="mb-0">
                                            <i class="fas fa-info-circle text-primary"></i> Informações Básicas
                                        </h6>
                                    </div>
                                    <div class="card-body">
                                        <div class="mb-3">
                                            <label class="form-label fw-bold">
                                                <i class="fas fa-tag text-muted"></i> Nome do Produto *
                                            </label>
                                            <asp:TextBox ID="txtNomeProduto" runat="server" CssClass="form-control form-control-lg" placeholder="Ex: Bolo de Chocolate Grande"></asp:TextBox>
                                        </div>
                                        <div class="mb-3">
                                            <label class="form-label fw-bold">
                                                <i class="fas fa-align-left text-muted"></i> Descrição
                                            </label>
                                            <asp:TextBox ID="txtDescricao" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" placeholder="Descreva o produto..."></asp:TextBox>
                                        </div>
                                        <div class="row">
                                            <div class="col-md-6">
                                                <div class="mb-3">
                                                    <label class="form-label fw-bold">
                                                        <i class="fas fa-dollar-sign text-success"></i> Preço (R$) *
                                                    </label>
                                                    <div class="input-group">
                                                        <span class="input-group-text">R$</span>
                                                        <asp:TextBox ID="txtPreco" runat="server" CssClass="form-control" TextMode="Number" step="0.01" placeholder="0.00"></asp:TextBox>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-md-6">
                                                <div class="mb-3">
                                                    <label class="form-label fw-bold">
                                                        <i class="fas fa-sort-numeric-up text-muted"></i> Ordem de Exibição
                                                    </label>
                                                    <asp:TextBox ID="txtOrdem" runat="server" CssClass="form-control" TextMode="Number" value="0"></asp:TextBox>
                                                    <small class="text-muted">Produtos com menor número aparecem primeiro</small>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <!-- Seção: Imagem do Produto -->
                                <div class="card mb-4 border-0 shadow-sm">
                                    <div class="card-header bg-light">
                                        <h6 class="mb-0">
                                            <i class="fas fa-image text-primary"></i> Imagem do Produto
                                        </h6>
                                    </div>
                                    <div class="card-body">
                                        <div class="row">
                                            <div class="col-md-6">
                                                <div class="mb-3">
                                                    <label class="form-label fw-bold">
                                                        <i class="fas fa-upload text-info"></i> Upload de Imagem
                                                    </label>
                                                    <asp:FileUpload ID="fileUploadImagem" runat="server" CssClass="form-control" accept="image/*" />
                                                    <small class="text-muted d-block mt-1">
                                                        <i class="fas fa-info-circle"></i> Formatos: JPG, PNG, GIF, WEBP | Mín: 200x200px | Máx: 5MB
                                                    </small>
                                                </div>
                                            </div>
                                            <div class="col-md-6">
                                                <div class="mb-3">
                                                    <label class="form-label fw-bold">
                                                        <i class="fas fa-link text-info"></i> OU URL da Imagem
                                                    </label>
                                                    <asp:TextBox ID="txtImagemUrl" runat="server" CssClass="form-control" placeholder="https://exemplo.com/imagem.jpg" onchange="atualizarPreviewImagem(this)"></asp:TextBox>
                                                    <small class="text-muted d-block mt-1">
                                                        <i class="fas fa-info-circle"></i> Cole a URL da imagem (Google Drive, Imgur, etc)
                                                    </small>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="text-center mt-3">
                                            <div class="preview-imagem-container">
                                                <img id="previewImagem" src="" alt="Preview" class="img-thumbnail" style="display: none; max-width: 300px; max-height: 300px; object-fit: contain;" />
                                                <div id="previewPlaceholder" class="text-muted p-4 border rounded" style="display: none;">
                                                    <i class="fas fa-image fa-3x mb-2"></i>
                                                    <p class="mb-0">Preview da imagem aparecerá aqui</p>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <!-- Seção: Datas de Validade -->
                                <div class="card mb-4 border-0 shadow-sm">
                                    <div class="card-header bg-light">
                                        <h6 class="mb-0">
                                            <i class="fas fa-calendar-alt text-primary"></i> Datas de Validade
                                        </h6>
                                    </div>
                                    <div class="card-body">
                                        <div class="row">
                                            <div class="col-md-6">
                                                <div class="mb-3">
                                                    <label class="form-label fw-bold">
                                                        <i class="fas fa-calendar-check text-success"></i> Reservável até
                                                    </label>
                                                    <asp:TextBox ID="txtReservavelAte" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                                                    <small class="text-muted">Data limite para reservas deste produto</small>
                                                </div>
                                            </div>
                                            <div class="col-md-6">
                                                <div class="mb-3">
                                                    <label class="form-label fw-bold">
                                                        <i class="fas fa-calendar-times text-danger"></i> Vendível até
                                                    </label>
                                                    <asp:TextBox ID="txtVendivelAte" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                                                    <small class="text-muted">Data limite para vendas deste produto</small>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <!-- Seção: Saco Promocional -->
                                <div class="card mb-4 border-0 shadow-sm">
                                    <div class="card-header bg-light">
                                        <h6 class="mb-0">
                                            <i class="fas fa-gift text-primary"></i> Configurações Promocionais
                                        </h6>
                                    </div>
                                    <div class="card-body">
                                        <div class="mb-3">
                                            <div class="form-check form-switch">
                                                <asp:CheckBox ID="chkEhSacoPromocional" runat="server" CssClass="form-check-input" onchange="toggleSacoPromocional(this)" />
                                                <label class="form-check-label fw-bold" for="<%= chkEhSacoPromocional.ClientID %>">
                                                    <i class="fas fa-shopping-bag"></i> Este produto é um Saco Promocional
                                                </label>
                                            </div>
                                        </div>
                                        <div id="divSacoPromocional" style="display: none;" class="border-top pt-3 mt-3">
                                            <div class="mb-3">
                                                <label class="form-label fw-bold">
                                                    <i class="fas fa-hashtag text-muted"></i> Quantidade de Produtos no Saco
                                                </label>
                                                <asp:TextBox ID="txtQuantidadeSaco" runat="server" CssClass="form-control" TextMode="Number" value="0"></asp:TextBox>
                                                <small class="text-muted">Quantidade de produtos que o cliente deve selecionar</small>
                                            </div>
                                            <div class="mb-3">
                                                <label class="form-label fw-bold">
                                                    <i class="fas fa-list-check text-muted"></i> Produtos Permitidos no Saco
                                                </label>
                                                <asp:ListBox ID="lstProdutosPermitidos" runat="server" CssClass="form-control" SelectionMode="Multiple" Rows="6"></asp:ListBox>
                                                <small class="text-muted">
                                                    <i class="fas fa-info-circle"></i> Selecione os produtos permitidos (Ctrl+Clique para múltiplos)
                                                </small>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <!-- Coluna Direita: Status e Preview -->
                            <div class="col-lg-4">
                                <!-- Seção: Status -->
                                <div class="card mb-4 border-0 shadow-sm sticky-top" style="top: 20px;">
                                    <div class="card-header bg-light">
                                        <h6 class="mb-0">
                                            <i class="fas fa-toggle-on text-primary"></i> Status do Produto
                                        </h6>
                                    </div>
                                    <div class="card-body">
                                        <div class="mb-3">
                                            <div class="form-check form-switch">
                                                <asp:CheckBox ID="chkAtivo" runat="server" CssClass="form-check-input" Checked="true" />
                                                <label class="form-check-label fw-bold" for="<%= chkAtivo.ClientID %>">
                                                    <i class="fas fa-check-circle text-success"></i> Produto Ativo
                                                </label>
                                            </div>
                                            <small class="text-muted">Produtos inativos não aparecem no site</small>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer bg-light">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                            <i class="fas fa-times"></i> Cancelar
                        </button>
                        <asp:Button ID="btnSalvarProduto" runat="server" Text="Salvar Alterações" CssClass="btn btn-primary" OnClick="btnSalvarProduto_Click" OnClientClick="return validarESalvarProduto();" />
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
                            <asp:TextBox ID="txtNovoNome" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Descrição</label>
                            <asp:TextBox ID="txtNovaDescricao" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Preço (R$) *</label>
                            <asp:TextBox ID="txtNovoPreco" runat="server" CssClass="form-control" TextMode="Number" step="0.01"></asp:TextBox>
                            <small class="text-muted">O tamanho (Pequeno/Grande) deve ser incluído no nome do produto</small>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Upload de Imagem</label>
                            <asp:FileUpload ID="fileUploadNovaImagem" runat="server" CssClass="form-control" accept="image/*" />
                            <small class="text-muted">Formatos aceitos: JPG, PNG, GIF, WEBP. Tamanho mínimo: 200x200px. Tamanho máximo: 5MB. A imagem será redimensionada automaticamente.</small>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">OU URL da Imagem</label>
                            <asp:TextBox ID="txtNovaImagemUrl" runat="server" CssClass="form-control"></asp:TextBox>
                            <small class="text-muted">Cole aqui a URL da imagem (pode ser do Google Drive, Imgur, etc) se preferir não fazer upload</small>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Ordem de Exibição</label>
                            <asp:TextBox ID="txtNovaOrdem" runat="server" CssClass="form-control" TextMode="Number" value="0"></asp:TextBox>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <label class="form-label">Reservável até</label>
                                    <asp:TextBox ID="txtNovoReservavelAte" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                                    <small class="text-muted">Data até quando o produto pode ser reservado</small>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <label class="form-label">Vendível até</label>
                                    <asp:TextBox ID="txtNovoVendivelAte" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                                    <small class="text-muted">Data até quando o produto pode ser vendido</small>
                                </div>
                            </div>
                        </div>
                        <div class="mb-3">
                            <div class="form-check">
                                <asp:CheckBox ID="chkNovoEhSacoPromocional" runat="server" CssClass="form-check-input" onchange="toggleSacoPromocionalNovo(this)" />
                                <label class="form-check-label" for="<%= chkNovoEhSacoPromocional.ClientID %>">
                                    É Saco Promocional
                                </label>
                            </div>
                        </div>
                        <div id="divNovoSacoPromocional" style="display: none;">
                            <div class="mb-3">
                                <label class="form-label">Quantidade de Produtos no Saco</label>
                                <asp:TextBox ID="txtNovaQuantidadeSaco" runat="server" CssClass="form-control" TextMode="Number" value="0"></asp:TextBox>
                                <small class="text-muted">Quantidade de produtos que o cliente deve selecionar para o saco</small>
                            </div>
                            <div class="mb-3">
                                <label class="form-label">Produtos Permitidos no Saco</label>
                                <asp:ListBox ID="lstNovosProdutosPermitidos" runat="server" CssClass="form-control" SelectionMode="Multiple" Rows="5"></asp:ListBox>
                                <small class="text-muted">Selecione os produtos que podem ser escolhidos para este saco (segure Ctrl para selecionar múltiplos)</small>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                        <asp:Button ID="btnSalvarNovoProduto" runat="server" Text="Salvar" CssClass="btn btn-primary" OnClick="btnSalvarNovoProduto_Click" OnClientClick="return validarESalvarNovoProduto();" />
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
                        <asp:Button ID="btnSalvarReserva" runat="server" Text="Salvar" CssClass="btn btn-primary" OnClick="btnSalvarReserva_Click" OnClientClick="return validarESalvarReserva();" />
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal Novo/Editar StatusReserva -->
        <div class="modal fade" id="modalNovoStatusReserva" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="modalStatusReservaTitle">Novo Status de Reserva</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <asp:HiddenField ID="hdnStatusReservaId" runat="server" Value="0" />
                        <div class="mb-3">
                            <label class="form-label">Nome *</label>
                            <asp:TextBox ID="txtStatusReservaNome" runat="server" CssClass="form-control" MaxLength="100"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Descrição *</label>
                            <asp:TextBox ID="txtStatusReservaDescricao" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" MaxLength="500"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Ordem</label>
                            <asp:TextBox ID="txtStatusReservaOrdem" runat="server" CssClass="form-control" TextMode="Number" value="0"></asp:TextBox>
                            <small class="text-muted">Ordem de exibição (menor número aparece primeiro)</small>
                        </div>
                        <div class="mb-3">
                            <div class="form-check">
                                <asp:CheckBox ID="chkStatusReservaPermiteAlteracao" runat="server" CssClass="form-check-input" Checked="true" />
                                <label class="form-check-label" for="<%= chkStatusReservaPermiteAlteracao.ClientID %>">
                                    Permite Alteração
                                </label>
                            </div>
                        </div>
                        <div class="mb-3">
                            <div class="form-check">
                                <asp:CheckBox ID="chkStatusReservaPermiteExclusao" runat="server" CssClass="form-check-input" Checked="true" />
                                <label class="form-check-label" for="<%= chkStatusReservaPermiteExclusao.ClientID %>">
                                    Permite Exclusão
                                </label>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                        <asp:Button ID="btnSalvarStatusReserva" runat="server" Text="Salvar" CssClass="btn btn-primary" OnClick="btnSalvarStatusReserva_Click" />
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
    <script type="text/javascript">
        // Função para expandir/colapsar grupos de log
        function toggleLogGroup(element) {
            var content = element.nextElementSibling;
            if (content) {
                if (content.style.display === 'none') {
                    content.style.display = 'block';
                    element.classList.remove('collapsed');
                } else {
                    content.style.display = 'none';
                    element.classList.add('collapsed');
                }
            }
        }

        // Função para carregar logs com período selecionado
        function carregarLogs() {
            var dias = document.getElementById('diasLogs').value;
            // Fazer postback para recarregar os logs
            __doPostBack('carregarLogs', dias);
        }

        function toggleSacoPromocional(checkbox) {
            var div = document.getElementById('divSacoPromocional');
            if (div) {
                div.style.display = checkbox.checked ? 'block' : 'none';
                // Desabilitar validação quando div estiver oculta
                var txtQuantidadeSaco = document.getElementById('<%= txtQuantidadeSaco.ClientID %>');
                if (txtQuantidadeSaco) {
                    if (!checkbox.checked) {
                        txtQuantidadeSaco.removeAttribute('min');
                        txtQuantidadeSaco.removeAttribute('max');
                        txtQuantidadeSaco.setAttribute('formnovalidate', 'true');
                        txtQuantidadeSaco.setAttribute('disabled', 'disabled');
                    } else {
                        txtQuantidadeSaco.removeAttribute('disabled');
                        txtQuantidadeSaco.removeAttribute('formnovalidate');
                        // Não adicionar min aqui, pois pode causar problemas se a div for ocultada novamente
                    }
                }
            }
        }
        
        function toggleSacoPromocionalNovo(checkbox) {
            var div = document.getElementById('divNovoSacoPromocional');
            if (div) {
                div.style.display = checkbox.checked ? 'block' : 'none';
                // Desabilitar validação quando div estiver oculta
                var txtNovaQuantidadeSaco = document.getElementById('<%= txtNovaQuantidadeSaco.ClientID %>');
                if (txtNovaQuantidadeSaco) {
                    if (!checkbox.checked) {
                        txtNovaQuantidadeSaco.removeAttribute('min');
                        txtNovaQuantidadeSaco.removeAttribute('max');
                        txtNovaQuantidadeSaco.setAttribute('formnovalidate', 'true');
                        txtNovaQuantidadeSaco.setAttribute('disabled', 'disabled');
                    } else {
                        txtNovaQuantidadeSaco.removeAttribute('disabled');
                        txtNovaQuantidadeSaco.removeAttribute('formnovalidate');
                    }
                }
            }
        }

        function editarStatusReserva(statusId) {
            __doPostBack('editarStatusReserva', statusId);
        }

        function excluirStatusReserva(statusId, nome) {
            if (confirm('Tem certeza que deseja excluir o status "' + nome + '"?\n\nEsta ação não pode ser desfeita.')) {
                __doPostBack('excluirStatusReserva', statusId);
            }
        }

        function excluirReserva(reservaId, nome) {
            if (confirm('Tem certeza que deseja excluir a reserva de "' + nome + '"?\n\nEsta ação não pode ser desfeita e enviará um email ao cliente.')) {
                __doPostBack('excluirReserva', reservaId);
            }
        }
        
        // Função para atualizar preview da imagem ao digitar URL
        function atualizarPreviewImagem(input) {
            var preview = document.getElementById('previewImagem');
            var placeholder = document.getElementById('previewPlaceholder');
            
            if (!preview || !placeholder) return;
            
            var url = input.value.trim();
            
            if (url) {
                preview.src = url;
                preview.style.display = 'block';
                placeholder.style.display = 'none';
                
                // Tratar erro de carregamento
                preview.onerror = function() {
                    preview.style.display = 'none';
                    placeholder.style.display = 'block';
                    placeholder.innerHTML = '<i class="fas fa-exclamation-triangle fa-3x mb-2 text-warning"></i><p class="mb-0">Erro ao carregar imagem</p>';
                };
                
                preview.onload = function() {
                    placeholder.style.display = 'none';
                };
            } else {
                preview.style.display = 'none';
                placeholder.style.display = 'block';
                placeholder.innerHTML = '<i class="fas fa-image fa-3x mb-2"></i><p class="mb-0">Preview da imagem aparecerá aqui</p>';
            }
        }
        
        // Atualizar preview quando o modal de edição abrir
        var modalEditarProduto = document.getElementById('modalEditarProduto');
        if (modalEditarProduto) {
            modalEditarProduto.addEventListener('shown.bs.modal', function() {
                var txtImagemUrl = document.getElementById('<%= txtImagemUrl.ClientID %>');
                if (txtImagemUrl && txtImagemUrl.value) {
                    atualizarPreviewImagem(txtImagemUrl);
                } else {
                    var placeholder = document.getElementById('previewPlaceholder');
                    if (placeholder) {
                        placeholder.style.display = 'block';
                    }
                }
            });
        }

        function carregarDadosReserva(reservaId) {
            __doPostBack('carregarDadosReserva', reservaId);
        }

        // Função global para desabilitar validação de campos ocultos
        function desabilitarValidacaoCamposOcultos() {
            // Desabilitar validação de campos em divs ocultas
            var divSaco = document.getElementById('divSacoPromocional');
            if (divSaco) {
                var isDivVisible = divSaco.style.display !== 'none' && divSaco.offsetParent !== null;
                var txtQuantidadeSaco = document.getElementById('<%= txtQuantidadeSaco.ClientID %>');
                if (txtQuantidadeSaco) {
                    if (!isDivVisible) {
                        txtQuantidadeSaco.removeAttribute('min');
                        txtQuantidadeSaco.removeAttribute('max');
                        txtQuantidadeSaco.setAttribute('formnovalidate', 'true');
                        txtQuantidadeSaco.setAttribute('disabled', 'disabled');
                    }
                }
            }
            
            var divNovoSaco = document.getElementById('divNovoSacoPromocional');
            if (divNovoSaco) {
                var isDivVisible = divNovoSaco.style.display !== 'none' && divNovoSaco.offsetParent !== null;
                var txtNovaQuantidadeSaco = document.getElementById('<%= txtNovaQuantidadeSaco.ClientID %>');
                if (txtNovaQuantidadeSaco) {
                    if (!isDivVisible) {
                        txtNovaQuantidadeSaco.removeAttribute('min');
                        txtNovaQuantidadeSaco.removeAttribute('max');
                        txtNovaQuantidadeSaco.setAttribute('formnovalidate', 'true');
                        txtNovaQuantidadeSaco.setAttribute('disabled', 'disabled');
                    }
                }
            }
            
            // Desabilitar validação de campos em modais ocultos
            var modais = ['modalEditarProduto', 'modalNovoProduto', 'modalEditarReserva'];
            modais.forEach(function(modalId) {
                var modal = document.getElementById(modalId);
                if (modal) {
                    var isModalVisible = modal.classList.contains('show') && modal.style.display !== 'none';
                    if (!isModalVisible) {
                        // Remover required de todos os campos
                        var campos = modal.querySelectorAll('[required]');
                        campos.forEach(function(campo) {
                            campo.removeAttribute('required');
                        });
                        
                        // Desabilitar validação de campos com min/max
                        var camposComValidacao = modal.querySelectorAll('input[type="number"][min], input[type="number"][max]');
                        camposComValidacao.forEach(function(campo) {
                            campo.removeAttribute('min');
                            campo.removeAttribute('max');
                            campo.setAttribute('formnovalidate', 'true');
                        });
                    }
                }
            });
        }

        function validarESalvarReserva() {
            desabilitarValidacaoCamposOcultos();
            return true;
        }

        function validarESalvarProduto() {
            desabilitarValidacaoCamposOcultos();
            return true;
        }

        function validarESalvarNovoProduto() {
            desabilitarValidacaoCamposOcultos();
            return true;
        }

        // Resetar modal quando fechar e desabilitar validação de campos ocultos
        document.addEventListener('DOMContentLoaded', function() {
            // Desabilitar validação de campos ocultos ao carregar a página
            desabilitarValidacaoCamposOcultos();
            
            // Interceptar todos os submits do formulário para garantir validação
            var form = document.getElementById('form1');
            if (form) {
                form.addEventListener('submit', function(e) {
                    desabilitarValidacaoCamposOcultos();
                });
            }
            
            var modalStatusReserva = document.getElementById('modalNovoStatusReserva');
            if (modalStatusReserva) {
                modalStatusReserva.addEventListener('hidden.bs.modal', function() {
                    var titleElement = document.getElementById('modalStatusReservaTitle');
                    if (titleElement) {
                        titleElement.textContent = 'Novo Status de Reserva';
                    }
                    // Limpar campos quando fechar
                    var hdnStatusId = document.querySelector('input[id*="hdnStatusReservaId"]');
                    var txtNome = document.querySelector('input[id*="txtStatusReservaNome"]');
                    var txtDescricao = document.querySelector('textarea[id*="txtStatusReservaDescricao"]');
                    var txtOrdem = document.querySelector('input[id*="txtStatusReservaOrdem"]');
                    var chkPermiteAlteracao = document.querySelector('input[id*="chkStatusReservaPermiteAlteracao"]');
                    var chkPermiteExclusao = document.querySelector('input[id*="chkStatusReservaPermiteExclusao"]');
                    
                    if (hdnStatusId) hdnStatusId.value = '0';
                    if (txtNome) txtNome.value = '';
                    if (txtDescricao) txtDescricao.value = '';
                    if (txtOrdem) txtOrdem.value = '0';
                    if (chkPermiteAlteracao) chkPermiteAlteracao.checked = true;
                    if (chkPermiteExclusao) chkPermiteExclusao.checked = true;
                });
            }
            
            // Desabilitar validação de campos ocultos quando modais de produto forem fechados
            var modalEditarProduto = document.getElementById('modalEditarProduto');
            if (modalEditarProduto) {
                modalEditarProduto.addEventListener('hidden.bs.modal', function() {
                    var txtQuantidadeSaco = document.getElementById('<%= txtQuantidadeSaco.ClientID %>');
                    if (txtQuantidadeSaco) {
                        txtQuantidadeSaco.removeAttribute('min');
                        txtQuantidadeSaco.removeAttribute('max');
                        txtQuantidadeSaco.setAttribute('formnovalidate', 'true');
                        txtQuantidadeSaco.setAttribute('disabled', 'disabled');
                    }
                });
            }
            
            var modalNovoProduto = document.getElementById('modalNovoProduto');
            if (modalNovoProduto) {
                modalNovoProduto.addEventListener('hidden.bs.modal', function() {
                    var txtNovaQuantidadeSaco = document.getElementById('<%= txtNovaQuantidadeSaco.ClientID %>');
                    if (txtNovaQuantidadeSaco) {
                        txtNovaQuantidadeSaco.removeAttribute('min');
                        txtNovaQuantidadeSaco.removeAttribute('max');
                        txtNovaQuantidadeSaco.setAttribute('formnovalidate', 'true');
                        txtNovaQuantidadeSaco.setAttribute('disabled', 'disabled');
                    }
                });
            }
        });
    </script>
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
        
        function voltarPagina() {
            if (window.history.length > 1) {
                window.history.back();
            } else {
                window.location.href = 'Default.aspx';
            }
        }
        
        function navegarParaAba(tabId) {
            var tabElement = document.getElementById(tabId);
            if (tabElement) {
                var tab = new bootstrap.Tab(tabElement);
                tab.show();
            }
        }
    </script>
    
    <style>
        /* Estilos para os modais de detalhes */
        .detalhe-item {
            padding: 12px;
            margin-bottom: 12px;
            background: #f8f9fa;
            border-radius: 8px;
            border-left: 4px solid #1a4d2e;
        }
        .detalhe-item strong {
            color: #1a4d2e;
            display: block;
            margin-bottom: 4px;
        }
        .detalhe-item-valor {
            color: #333;
            font-size: 15px;
        }
    </style>
    
    <script>
        // Função para mostrar detalhes da reserva (Admin)
        function mostrarDetalhesReservaAdmin(reservaId) {
            // Fazer requisição para obter detalhes da reserva
            __doPostBack('obterDetalhesReserva', reservaId.toString());
        }
        
        // Função para mostrar detalhes do cliente
        function mostrarDetalhesCliente(clienteId) {
            // Fazer requisição para obter detalhes do cliente
            __doPostBack('obterDetalhesCliente', clienteId.toString());
        }
        
        // Função para mostrar detalhes do produto
        function mostrarDetalhesProduto(produtoId) {
            // Fazer requisição para obter detalhes do produto
            __doPostBack('obterDetalhesProduto', produtoId.toString());
        }
    </script>
    
    <!-- Modal de Detalhes da Reserva (Admin) -->
    <div class="modal fade" id="modalDetalhesReservaAdmin" tabindex="-1" aria-labelledby="modalDetalhesReservaAdminLabel" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content">
                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title" id="modalDetalhesReservaAdminLabel">
                        <i class="fas fa-info-circle"></i> Detalhes da Reserva
                    </h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Fechar"></button>
                </div>
                <div class="modal-body" id="modalDetalhesReservaAdminBody">
                    <!-- Conteúdo será preenchido via JavaScript -->
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                        <i class="fas fa-times"></i> Fechar
                    </button>
                </div>
            </div>
        </div>
    </div>
    
    <!-- Modal de Detalhes do Cliente -->
    <div class="modal fade" id="modalDetalhesCliente" tabindex="-1" aria-labelledby="modalDetalhesClienteLabel" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content">
                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title" id="modalDetalhesClienteLabel">
                        <i class="fas fa-user"></i> Detalhes do Cliente
                    </h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Fechar"></button>
                </div>
                <div class="modal-body" id="modalDetalhesClienteBody">
                    <!-- Conteúdo será preenchido via JavaScript -->
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                        <i class="fas fa-times"></i> Fechar
                    </button>
                </div>
            </div>
        </div>
    </div>
    
    <!-- Modal de Detalhes do Produto -->
    <div class="modal fade" id="modalDetalhesProduto" tabindex="-1" aria-labelledby="modalDetalhesProdutoLabel" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content">
                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title" id="modalDetalhesProdutoLabel">
                        <i class="fas fa-cookie-bite"></i> Detalhes do Produto
                    </h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Fechar"></button>
                </div>
                <div class="modal-body" id="modalDetalhesProdutoBody">
                    <!-- Conteúdo será preenchido via JavaScript -->
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                        <i class="fas fa-times"></i> Fechar
                    </button>
                </div>
            </div>
        </div>
    </div>
</body>
</html>
