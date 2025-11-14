<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MinhasReservas.aspx.cs" Inherits="KingdomConfeitaria.MinhasReservas" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Minhas Reservas - Kingdom Confeitaria</title>
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
        .container-main {
            background: white;
            border-radius: 20px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
            margin: 90px auto 20px auto;
            padding: 30px;
        }
        .reserva-card {
            border: 2px solid #e9ecef;
            border-radius: 15px;
            padding: 20px;
            margin-bottom: 20px;
            transition: transform 0.3s, box-shadow 0.3s;
        }
        .reserva-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 5px 20px rgba(0,0,0,0.1);
        }
        .status-badge {
            padding: 5px 15px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: bold;
        }
        .status-pendente { background-color: #ffc107; color: #000; }
        .status-confirmado { background-color: #17a2b8; color: #fff; }
        .status-pronto { background-color: #28a745; color: #fff; }
        .status-entregue { background-color: #6c757d; color: #fff; }
        .status-cancelado { background-color: #dc3545; color: #fff; }
        .btn-share {
            margin: 5px;
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
                <h2 class="mb-4"><i class="fas fa-list"></i> Minhas Reservas</h2>
                
                <div id="alertContainer" runat="server"></div>
                
                <div id="reservasContainer" runat="server">
                    <!-- Reservas serão carregadas aqui -->
                </div>
                
                <div class="text-center mt-4">
                    <a href="Default.aspx" class="btn btn-success btn-lg">
                        <i class="fas fa-plus"></i> Fazer Nova Reserva
                    </a>
                </div>
            </div>
        </div>
        
        <!-- Modal de Detalhes da Reserva -->
        <div class="modal fade" id="modalDetalhesReserva" tabindex="-1" aria-labelledby="modalDetalhesReservaLabel" aria-hidden="true">
            <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
                <div class="modal-content">
                    <div class="modal-header bg-primary text-white">
                        <h5 class="modal-title" id="modalDetalhesReservaLabel">
                            <i class="fas fa-info-circle"></i> Detalhes da Reserva
                        </h5>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Fechar"></button>
                    </div>
                    <div class="modal-body" id="modalDetalhesReservaBody">
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
    <!-- Scripts específicos da página MinhasReservas -->
    <script src="Scripts/minhasreservas.js"></script>
    <style>
        /* Estilos para o modal de detalhes */
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
        .detalhe-itens {
            background: white;
            border: 1px solid #dee2e6;
            border-radius: 8px;
            padding: 16px;
            margin-top: 12px;
        }
        .detalhe-item-produto {
            padding: 10px;
            margin-bottom: 8px;
            background: #f8f9fa;
            border-radius: 6px;
            border-left: 3px solid #28a745;
        }
    </style>
    <script>
        // Função para mostrar detalhes da reserva
        function mostrarDetalhesReserva(reservaId) {
            var botao = event.target.closest('button');
            var reservaJson = botao.getAttribute('data-reserva-json');
            
            if (!reservaJson) {
                alert('Erro ao carregar detalhes da reserva.');
                return;
            }
            
            try {
                var reserva = JSON.parse(reservaJson);
                var modalBody = document.getElementById('modalDetalhesReservaBody');
                var modalLabel = document.getElementById('modalDetalhesReservaLabel');
                
                if (!modalBody || !modalLabel) return;
                
                // Construir HTML dos detalhes
                var html = '<div class="container-fluid">';
                
                // Informações principais
                html += '<div class="row mb-3">';
                html += '<div class="col-md-6">';
                html += '<div class="detalhe-item">';
                html += '<strong><i class="fas fa-hashtag"></i> Número da Reserva</strong>';
                html += '<div class="detalhe-item-valor">#' + reserva.id + '</div>';
                html += '</div>';
                html += '</div>';
                html += '<div class="col-md-6">';
                html += '<div class="detalhe-item">';
                html += '<strong><i class="fas fa-tag"></i> Status</strong>';
                html += '<div class="detalhe-item-valor"><span class="status-badge status-' + reserva.statusClass + '">' + escapeHtml(reserva.status) + '</span></div>';
                html += '</div>';
                html += '</div>';
                html += '</div>';
                
                // Datas
                html += '<div class="row mb-3">';
                html += '<div class="col-md-6">';
                html += '<div class="detalhe-item">';
                html += '<strong><i class="fas fa-calendar-plus"></i> Data da Reserva</strong>';
                html += '<div class="detalhe-item-valor">' + escapeHtml(reserva.dataReserva) + '</div>';
                html += '</div>';
                html += '</div>';
                html += '<div class="col-md-6">';
                html += '<div class="detalhe-item">';
                html += '<strong><i class="fas fa-calendar-check"></i> Data de Retirada</strong>';
                html += '<div class="detalhe-item-valor">' + escapeHtml(reserva.dataRetirada) + '</div>';
                html += '</div>';
                html += '</div>';
                html += '</div>';
                
                // Contato
                html += '<div class="row mb-3">';
                html += '<div class="col-md-6">';
                html += '<div class="detalhe-item">';
                html += '<strong><i class="fas fa-envelope"></i> Email</strong>';
                html += '<div class="detalhe-item-valor">' + escapeHtml(reserva.email) + '</div>';
                html += '</div>';
                html += '</div>';
                html += '<div class="col-md-6">';
                html += '<div class="detalhe-item">';
                html += '<strong><i class="fas fa-phone"></i> Telefone/WhatsApp</strong>';
                html += '<div class="detalhe-item-valor">' + escapeHtml(reserva.telefone) + '</div>';
                html += '</div>';
                html += '</div>';
                html += '</div>';
                
                // Valor e Previsão
                html += '<div class="row mb-3">';
                html += '<div class="col-md-6">';
                html += '<div class="detalhe-item">';
                html += '<strong><i class="fas fa-dollar-sign"></i> Valor Total</strong>';
                html += '<div class="detalhe-item-valor" style="font-size: 20px; font-weight: 700; color: #28a745;">R$ ' + escapeHtml(reserva.valorTotal) + '</div>';
                html += '</div>';
                html += '</div>';
                html += '<div class="col-md-6">';
                html += '<div class="detalhe-item">';
                html += '<strong><i class="fas fa-truck"></i> Previsão de Entrega</strong>';
                html += '<div class="detalhe-item-valor">' + escapeHtml(reserva.previsaoEntrega) + '</div>';
                html += '</div>';
                html += '</div>';
                html += '</div>';
                
                // Itens
                html += '<div class="detalhe-itens">';
                html += '<h6 class="mb-3"><i class="fas fa-shopping-bag"></i> Itens da Reserva</h6>';
                if (reserva.itens && reserva.itens.length > 0) {
                    reserva.itens.forEach(function(item) {
                        html += '<div class="detalhe-item-produto">';
                        html += '<div class="d-flex justify-content-between align-items-start">';
                        html += '<div>';
                        html += '<strong>' + escapeHtml(item.nome) + '</strong>';
                        html += '<div class="text-muted small">Tamanho: ' + escapeHtml(item.tamanho) + ' | Quantidade: ' + item.quantidade + '</div>';
                        if (item.produtos) {
                            try {
                                var produtosJson = JSON.parse(item.produtos);
                                if (produtosJson && produtosJson.length > 0) {
                                    html += '<div class="text-muted small mt-1">Produtos do saco: ';
                                    var produtosTexto = produtosJson.map(function(p) {
                                        return p.qt + 'x Produto ID ' + p.id;
                                    }).join(', ');
                                    html += produtosTexto + '</div>';
                                }
                            } catch(e) {}
                        }
                        html += '</div>';
                        html += '<div class="text-end">';
                        html += '<strong class="text-success">R$ ' + escapeHtml(item.subtotal) + '</strong>';
                        html += '</div>';
                        html += '</div>';
                        html += '</div>';
                    });
                } else {
                    html += '<p class="text-muted">Nenhum item encontrado.</p>';
                }
                html += '</div>';
                
                // Observações
                html += '<div class="detalhe-item mt-3">';
                html += '<strong><i class="fas fa-comment-alt"></i> Observações</strong>';
                html += '<div class="detalhe-item-valor">' + escapeHtml(reserva.observacoes) + '</div>';
                html += '</div>';
                
                // Link da reserva
                if (reserva.linkReserva) {
                    html += '<div class="mt-3 text-center">';
                    html += '<a href="' + escapeAttr(reserva.linkReserva) + '" class="btn btn-outline-primary" target="_blank">';
                    html += '<i class="fas fa-external-link-alt"></i> Ver Página da Reserva';
                    html += '</a>';
                    html += '</div>';
                }
                
                html += '</div>';
                
                modalBody.innerHTML = html;
                
                // Abrir modal
                var modalElement = document.getElementById('modalDetalhesReserva');
                if (modalElement) {
                    if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                        var modal = new bootstrap.Modal(modalElement);
                        modal.show();
                    } else {
                        modalElement.classList.add('show');
                        modalElement.style.display = 'block';
                    }
                }
            } catch (e) {
                console.error('Erro ao processar detalhes:', e);
                alert('Erro ao carregar detalhes da reserva.');
            }
        }
        
        // Funções auxiliares
        function escapeHtml(text) {
            if (!text) return '';
            var map = {
                '&': '&amp;',
                '<': '&lt;',
                '>': '&gt;',
                '"': '&quot;',
                "'": '&#039;'
            };
            return text.toString().replace(/[&<>"']/g, function(m) { return map[m]; });
        }
        
        function escapeAttr(text) {
            if (!text) return '';
            return text.toString().replace(/"/g, '&quot;').replace(/'/g, '&#039;');
        }
        
        // Funções adicionais específicas da página
        function compartilharEmail(url, texto) {
            window.location.href = 'mailto:?subject=Minha Reserva - Kingdom Confeitaria&body=' + encodeURIComponent(texto + '\n\n' + url);
        }

        function cancelarReserva(reservaId) {
            if (confirm('Tem certeza que deseja cancelar esta reserva? Esta ação não pode ser desfeita.')) {
                KingdomConfeitaria.Utils.postBack('CancelarReserva', reservaId.toString());
            }
        }

        function excluirReserva(reservaId) {
            if (confirm('Tem certeza que deseja excluir esta reserva? Esta ação não pode ser desfeita.')) {
                KingdomConfeitaria.Utils.postBack('ExcluirReserva', reservaId.toString());
            }
        }
        
        function voltarPagina() {
            if (window.history.length > 1) {
                window.history.back();
            } else {
                window.location.href = 'Default.aspx';
            }
        }
    </script>
</body>
</html>

