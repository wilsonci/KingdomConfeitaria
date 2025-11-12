/**
 * Kingdom Confeitaria - JavaScript para página Admin
 * Funcionalidades específicas da área administrativa
 */

// Namespace para evitar conflitos
var AdminPage = AdminPage || {};

// Funções de gerenciamento de modais
AdminPage.Modal = {
    /**
     * Fechar modal de editar produto
     */
    fecharEditarProduto: function() {
        var modal = bootstrap.Modal.getInstance(document.getElementById('modalEditarProduto'));
        if (modal) {
            modal.hide();
        }
    },

    /**
     * Fechar modal de novo produto
     */
    fecharNovoProduto: function() {
        var modal = bootstrap.Modal.getInstance(document.getElementById('modalNovoProduto'));
        if (modal) {
            modal.hide();
        }
    },

    /**
     * Fechar modal de editar reserva
     */
    fecharEditarReserva: function() {
        var modal = bootstrap.Modal.getInstance(document.getElementById('modalEditarReserva'));
        if (modal) {
            modal.hide();
        }
    }
};

// Funções de edição de produtos
AdminPage.Produtos = {
    /**
     * Editar produto
     */
    editar: function(id, nome, descricao, preco, imagemUrl, ordem, ativo, reservavelAte, vendivelAte, ehSacoPromocional, quantidadeSaco, produtosPermitidos) {
        // Obter elementos usando IDs gerados pelo ASP.NET
        var hdnProdutoId = document.querySelector('input[id*="hdnProdutoId"]');
        var txtNomeProduto = document.querySelector('input[id*="txtNomeProduto"]');
        var txtDescricao = document.querySelector('textarea[id*="txtDescricao"]');
        var txtPreco = document.querySelector('input[id*="txtPreco"]');
        var txtImagemUrl = document.querySelector('input[id*="txtImagemUrl"]');
        var txtOrdem = document.querySelector('input[id*="txtOrdem"]');
        var chkAtivo = document.querySelector('input[id*="chkAtivo"]');
        var txtReservavelAte = document.querySelector('input[id*="txtReservavelAte"]');
        var txtVendivelAte = document.querySelector('input[id*="txtVendivelAte"]');
        var chkEhSacoPromocional = document.querySelector('input[id*="chkEhSacoPromocional"]');
        var txtQuantidadeSaco = document.querySelector('input[id*="txtQuantidadeSaco"]');
        var lstProdutosPermitidos = document.querySelector('select[id*="lstProdutosPermitidos"]');
        
        if (hdnProdutoId) hdnProdutoId.value = id;
        if (txtNomeProduto) txtNomeProduto.value = nome || '';
        if (txtDescricao) txtDescricao.value = descricao || '';
        if (txtPreco) txtPreco.value = preco || '0.00';
        if (txtImagemUrl) txtImagemUrl.value = imagemUrl || '';
        if (txtOrdem) txtOrdem.value = ordem || '0';
        if (chkAtivo) chkAtivo.checked = ativo === true || ativo === 'true' || ativo === 'True';
        if (txtReservavelAte) txtReservavelAte.value = reservavelAte || '';
        if (txtVendivelAte) txtVendivelAte.value = vendivelAte || '';
        if (chkEhSacoPromocional) {
            chkEhSacoPromocional.checked = ehSacoPromocional === true || ehSacoPromocional === 'true' || ehSacoPromocional === 'True';
            toggleSacoPromocional(chkEhSacoPromocional);
        }
        if (txtQuantidadeSaco) txtQuantidadeSaco.value = quantidadeSaco || '0';
        if (lstProdutosPermitidos && produtosPermitidos) {
            try {
                var produtosIds = JSON.parse(produtosPermitidos);
                for (var i = 0; i < lstProdutosPermitidos.options.length; i++) {
                    var option = lstProdutosPermitidos.options[i];
                    option.selected = produtosIds.indexOf(parseInt(option.value)) !== -1;
                }
            } catch (e) {
                console.error('Erro ao parsear produtos permitidos:', e);
            }
        }
        
        // Atualizar preview da imagem
        var preview = document.getElementById('previewImagem');
        if (preview) {
            preview.src = imagemUrl || '';
            preview.style.display = imagemUrl ? 'block' : 'none';
        }
        
        // Abrir modal
        var modalElement = document.getElementById('modalEditarProduto');
        if (modalElement) {
            var modal = new bootstrap.Modal(modalElement);
            modal.show();
        }
    },

    /**
     * Preview de imagem ao digitar URL
     */
    atualizarPreview: function(input) {
        var preview = document.getElementById('previewImagem');
        if (preview && input) {
            preview.src = input.value;
            preview.style.display = input.value ? 'block' : 'none';
        }
    }
};

// Função global para compatibilidade com onclick inline
function editarProduto(id, nome, descricao, preco, imagemUrl, ordem, ativo, reservavelAte, vendivelAte, ehSacoPromocional, quantidadeSaco, produtosPermitidos) {
    AdminPage.Produtos.editar(id, nome, descricao, preco, imagemUrl, ordem, ativo, reservavelAte || '', vendivelAte || '', ehSacoPromocional || false, quantidadeSaco || 0, produtosPermitidos || '');
}

// Funções de edição de reservas
AdminPage.Reservas = {
    /**
     * Editar reserva
     */
    editar: function(id, status, valorTotal, convertidoEmPedido, cancelado, previsaoEntrega, observacoes) {
        var hdnReservaId = document.querySelector('input[id*="hdnReservaId"]');
        var ddlStatus = document.querySelector('select[id*="ddlStatus"]');
        var txtValorTotal = document.querySelector('input[id*="txtValorTotal"]');
        var chkConvertido = document.querySelector('input[id*="chkConvertidoEmPedido"]');
        var chkCancelado = document.querySelector('input[id*="chkCancelado"]');
        var txtPrevisao = document.querySelector('input[id*="txtPrevisaoEntrega"]');
        var txtObservacoes = document.querySelector('textarea[id*="txtObservacoesReserva"]');
        
        if (hdnReservaId) hdnReservaId.value = id;
        if (ddlStatus) ddlStatus.value = status || 'Aberta';
        if (txtValorTotal) txtValorTotal.value = valorTotal || '0.00';
        if (chkConvertido) chkConvertido.checked = convertidoEmPedido === true || convertidoEmPedido === 'true' || convertidoEmPedido === 'True';
        if (chkCancelado) chkCancelado.checked = cancelado === true || cancelado === 'true' || cancelado === 'True';
        
        if (txtPrevisao) {
            if (previsaoEntrega && previsaoEntrega !== '') {
                var data = new Date(previsaoEntrega);
                var dataFormatada = data.toISOString().slice(0, 16);
                txtPrevisao.value = dataFormatada;
            } else {
                txtPrevisao.value = '';
            }
        }
        
        if (txtObservacoes) txtObservacoes.value = observacoes || '';
        
        // Abrir modal
        var modalElement = document.getElementById('modalEditarReserva');
        if (modalElement) {
            var modal = new bootstrap.Modal(modalElement);
            modal.show();
        }
    }
};

// Função global para compatibilidade com onclick inline
function editarReserva(id, status, valorTotal, convertidoEmPedido, cancelado, previsaoEntrega, observacoes) {
    AdminPage.Reservas.editar(id, status, valorTotal, convertidoEmPedido, cancelado, previsaoEntrega, observacoes);
}

