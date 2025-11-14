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
        var modalElement = document.getElementById('modalEditarProduto');
        if (modalElement) {
            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                var modal = bootstrap.Modal.getInstance(modalElement);
                if (modal) {
                    modal.hide();
                } else {
                    modalElement.classList.remove('show');
                    modalElement.style.display = 'none';
                    modalElement.setAttribute('aria-hidden', 'true');
                    document.body.classList.remove('modal-open');
                    var backdrop = document.querySelector('.modal-backdrop');
                    if (backdrop) backdrop.remove();
                }
            } else {
                modalElement.classList.remove('show');
                modalElement.style.display = 'none';
                modalElement.setAttribute('aria-hidden', 'true');
                document.body.classList.remove('modal-open');
                var backdrop = document.querySelector('.modal-backdrop');
                if (backdrop) backdrop.remove();
            }
        }
    },

    /**
     * Fechar modal de novo produto
     */
    fecharNovoProduto: function() {
        var modalElement = document.getElementById('modalNovoProduto');
        if (modalElement) {
            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                var modal = bootstrap.Modal.getInstance(modalElement);
                if (modal) {
                    modal.hide();
                } else {
                    modalElement.classList.remove('show');
                    modalElement.style.display = 'none';
                    modalElement.setAttribute('aria-hidden', 'true');
                    document.body.classList.remove('modal-open');
                    var backdrop = document.querySelector('.modal-backdrop');
                    if (backdrop) backdrop.remove();
                }
            } else {
                modalElement.classList.remove('show');
                modalElement.style.display = 'none';
                modalElement.setAttribute('aria-hidden', 'true');
                document.body.classList.remove('modal-open');
                var backdrop = document.querySelector('.modal-backdrop');
                if (backdrop) backdrop.remove();
            }
        }
    },

    /**
     * Fechar modal de editar reserva
     */
    fecharEditarReserva: function() {
        var modalElement = document.getElementById('modalEditarReserva');
        if (modalElement) {
            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                var modal = bootstrap.Modal.getInstance(modalElement);
                if (modal) {
                    modal.hide();
                } else {
                    modalElement.classList.remove('show');
                    modalElement.style.display = 'none';
                    modalElement.setAttribute('aria-hidden', 'true');
                    document.body.classList.remove('modal-open');
                    var backdrop = document.querySelector('.modal-backdrop');
                    if (backdrop) backdrop.remove();
                }
            } else {
                modalElement.classList.remove('show');
                modalElement.style.display = 'none';
                modalElement.setAttribute('aria-hidden', 'true');
                document.body.classList.remove('modal-open');
                var backdrop = document.querySelector('.modal-backdrop');
                if (backdrop) backdrop.remove();
            }
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
                // Erro ao parsear produtos permitidos
            }
        }
        
        // Atualizar preview da imagem
        if (txtImagemUrl) {
            if (typeof atualizarPreviewImagem === 'function') {
                setTimeout(function() { atualizarPreviewImagem(txtImagemUrl); }, 100);
            } else if (AdminPage.Produtos && AdminPage.Produtos.atualizarPreview) {
                setTimeout(function() { AdminPage.Produtos.atualizarPreview(txtImagemUrl); }, 100);
            }
        }
        
        // Abrir modal
        var modalElement = document.getElementById('modalEditarProduto');
        if (modalElement) {
            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                var modal = new bootstrap.Modal(modalElement);
                modal.show();
            } else {
                modalElement.classList.add('show');
                modalElement.style.display = 'block';
                modalElement.setAttribute('aria-hidden', 'false');
                document.body.classList.add('modal-open');
                var backdrop = document.createElement('div');
                backdrop.className = 'modal-backdrop fade show';
                document.body.appendChild(backdrop);
            }
        }
    },

    /**
     * Preview de imagem ao digitar URL
     */
    atualizarPreview: function(input) {
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
    editar: function(id, statusId, valorTotal, convertidoEmPedido, cancelado, previsaoEntrega, observacoes) {
        var hdnReservaId = document.querySelector('input[id*="hdnReservaId"]');
        var ddlStatus = document.querySelector('select[id*="ddlStatus"]');
        var txtValorTotal = document.querySelector('input[id*="txtValorTotal"]');
        var chkConvertido = document.querySelector('input[id*="chkConvertidoEmPedido"]');
        var chkCancelado = document.querySelector('input[id*="chkCancelado"]');
        var txtPrevisao = document.querySelector('input[id*="txtPrevisaoEntrega"]');
        var txtObservacoes = document.querySelector('textarea[id*="txtObservacoesReserva"]');
        
        // Preencher ID da reserva
        if (hdnReservaId) {
            hdnReservaId.value = id ? id.toString() : '';
        }
        
        // Preencher Status
        if (ddlStatus) {
            if (statusId && statusId !== '0' && statusId !== 0) {
                ddlStatus.value = statusId.toString();
            } else {
                ddlStatus.selectedIndex = 0;
            }
        }
        
        // Preencher Valor Total
        if (txtValorTotal) {
            txtValorTotal.value = valorTotal ? valorTotal.toString().replace(',', '.') : '0.00';
        }
        
        // Preencher Convertido em Pedido
        if (chkConvertido) {
            chkConvertido.checked = convertidoEmPedido === true || convertidoEmPedido === 'true' || convertidoEmPedido === 'True' || convertidoEmPedido === '1';
        }
        
        // Preencher Cancelado
        if (chkCancelado) {
            chkCancelado.checked = cancelado === true || cancelado === 'true' || cancelado === 'True' || cancelado === '1';
        }
        
        // Preencher Previsão de Entrega
        if (txtPrevisao) {
            if (previsaoEntrega && previsaoEntrega !== '' && previsaoEntrega !== 'null') {
                try {
                    var data = new Date(previsaoEntrega);
                    if (!isNaN(data.getTime())) {
                        var dataFormatada = data.toISOString().slice(0, 16);
                        txtPrevisao.value = dataFormatada;
                    } else {
                        txtPrevisao.value = '';
                    }
                } catch (e) {
                    txtPrevisao.value = '';
                }
            } else {
                txtPrevisao.value = '';
            }
        }
        
        // Preencher Observações
        if (txtObservacoes) {
            txtObservacoes.value = observacoes ? observacoes.toString() : '';
        }
        
        // Abrir modal
        var modalElement = document.getElementById('modalEditarReserva');
        if (modalElement) {
            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                var modal = new bootstrap.Modal(modalElement);
                modal.show();
            } else {
                modalElement.classList.add('show');
                modalElement.style.display = 'block';
                modalElement.setAttribute('aria-hidden', 'false');
                document.body.classList.add('modal-open');
                var backdrop = document.createElement('div');
                backdrop.className = 'modal-backdrop fade show';
                document.body.appendChild(backdrop);
            }
        }
    }
};

// Função global para compatibilidade com onclick inline
function editarReserva(id, statusId, valorTotal, convertidoEmPedido, cancelado, previsaoEntrega, observacoes) {
    AdminPage.Reservas.editar(id, statusId, valorTotal, convertidoEmPedido, cancelado, previsaoEntrega, observacoes);
}

