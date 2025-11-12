/**
 * Kingdom Confeitaria - JavaScript Principal
 * Funções compartilhadas entre todas as páginas
 */

// Namespace para evitar conflitos
var KingdomConfeitaria = KingdomConfeitaria || {};

// Funções utilitárias
KingdomConfeitaria.Utils = {
    /**
     * Escapar strings para JavaScript
     */
    escapeJs: function(str) {
        if (!str) return '';
        return String(str)
            .replace(/\\/g, '\\\\')
            .replace(/'/g, "\\'")
            .replace(/"/g, '\\"')
            .replace(/\r/g, '\\r')
            .replace(/\n/g, '\\n')
            .replace(/\t/g, '\\t');
    },

    /**
     * Executar função quando DOM estiver pronto
     */
    ready: function(fn) {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', fn);
        } else {
            fn();
        }
    },

    /**
     * Verificar se a sessão ainda está ativa
     */
    verificarSessao: function() {
        // Fazer uma requisição AJAX para verificar se a sessão ainda está ativa
        // Isso será implementado no servidor se necessário
        // Por enquanto, apenas verificar se há dados de sessão no cliente
        return true;
    },

    /**
     * Fazer postback do ASP.NET
     */
    postBack: function(target, argument) {
        // Verificar se __doPostBack está disponível
        if (typeof __doPostBack !== 'undefined') {
            __doPostBack(target, argument || '');
        } else {
            // Se não estiver disponível, usar método alternativo
            var form = document.getElementById('form1');
            if (form) {
                // Remover inputs anteriores se existirem
                var existingTarget = form.querySelector('input[name="__EVENTTARGET"]');
                if (existingTarget) existingTarget.remove();
                var existingArg = form.querySelector('input[name="__EVENTARGUMENT"]');
                if (existingArg) existingArg.remove();
                
                var eventTarget = document.createElement('input');
                eventTarget.type = 'hidden';
                eventTarget.name = '__EVENTTARGET';
                eventTarget.value = target;
                form.appendChild(eventTarget);
                
                if (argument) {
                    var eventArgument = document.createElement('input');
                    eventArgument.type = 'hidden';
                    eventArgument.name = '__EVENTARGUMENT';
                    eventArgument.value = argument;
                    form.appendChild(eventArgument);
                }
                
                form.submit();
            }
        }
    }
};

// Gerenciamento de modais Bootstrap
KingdomConfeitaria.Modal = {
    /**
     * Abrir modal
     */
    show: function(modalId) {
        var modalElement = document.getElementById(modalId);
        if (!modalElement) {
            return;
        }

        if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
            var modal = bootstrap.Modal.getOrCreateInstance(modalElement);
            modal.show();
            modalElement.removeAttribute('aria-hidden');
        } else {
            // Fallback manual
            modalElement.style.display = 'block';
            modalElement.classList.add('show');
            modalElement.removeAttribute('aria-hidden');
            document.body.classList.add('modal-open');
            var backdrop = document.createElement('div');
            backdrop.className = 'modal-backdrop fade show';
            document.body.appendChild(backdrop);
        }
    },

    /**
     * Fechar modal
     */
    hide: function(modalId) {
        var modalElement = document.getElementById(modalId);
        if (!modalElement) {
            return;
        }

        if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
            var modal = bootstrap.Modal.getInstance(modalElement);
            if (modal) {
                modal.hide();
            } else {
                modal = new bootstrap.Modal(modalElement);
                modal.hide();
            }
        } else {
            // Fallback manual
            modalElement.style.display = 'none';
            modalElement.classList.remove('show');
            modalElement.setAttribute('aria-hidden', 'true');
            document.body.classList.remove('modal-open');
            var backdrop = document.querySelector('.modal-backdrop');
            if (backdrop) {
                backdrop.remove();
            }
        }
    },

    /**
     * Inicializar botões de fechar de um modal
     */
    initCloseButtons: function(modalId) {
        var modalElement = document.getElementById(modalId);
        if (!modalElement) return;

        var closeButtons = modalElement.querySelectorAll('[data-bs-dismiss="modal"], .btn-close, button[data-modal-close="' + modalId + '"]');
        closeButtons.forEach(function(btn) {
            // Remover listeners antigos
            var newBtn = btn.cloneNode(true);
            btn.parentNode.replaceChild(newBtn, btn);

            // Adicionar novo listener
            newBtn.addEventListener('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                KingdomConfeitaria.Modal.hide(modalId);
                return false;
            });
        });

        // Listener para eventos do Bootstrap
        modalElement.addEventListener('hidden.bs.modal', function() {
            modalElement.setAttribute('aria-hidden', 'true');
        });

        modalElement.addEventListener('shown.bs.modal', function() {
            modalElement.removeAttribute('aria-hidden');
        });
    }
};

// Função global para abrir modal de login (redireciona para Login.aspx)
function abrirModalLogin() {
    window.location.href = 'Login.aspx';
}

// Inicialização quando a página carregar
KingdomConfeitaria.Utils.ready(function() {
    // Inicializar todos os modais encontrados
    var modals = document.querySelectorAll('.modal');
    modals.forEach(function(modal) {
        if (modal.id) {
            KingdomConfeitaria.Modal.initCloseButtons(modal.id);
        }
    });

    // Re-inicializar após postback usando eventos modernos
    window.addEventListener('pageshow', function(event) {
        modals.forEach(function(modal) {
            if (modal.id) {
                KingdomConfeitaria.Modal.initCloseButtons(modal.id);
            }
        });
    });
});

