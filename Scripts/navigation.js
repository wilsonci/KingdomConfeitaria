/**
 * Kingdom Confeitaria - Sistema de Navegação
 * Gerencia histórico de navegação e botão flutuante de voltar
 */

var KingdomConfeitaria = KingdomConfeitaria || {};

KingdomConfeitaria.Navigation = {
    /**
     * Salvar página atual antes de navegar
     */
    salvarPaginaAtual: function(url, estadoControles) {
        // Validar URL
        if (!url || url === '') {
            url = window.location.pathname;
        }
        
        // Determinar caminho relativo do handler baseado na localização atual
        var handlerPath = window.location.pathname.indexOf('/paginas/') > -1 
            ? '../Handlers/CallbackHandler.ashx' 
            : 'Handlers/CallbackHandler.ashx';
        
        // Salvar via Handler - apenas se KingdomConfeitaria.Ajax estiver disponível
        if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Ajax && typeof KingdomConfeitaria.Ajax.callHandler === 'function') {
            try {
                var estadoJson = '';
                if (estadoControles && typeof estadoControles === 'object') {
                    try {
                        estadoJson = JSON.stringify(estadoControles);
                    } catch (e) {
                        // Se falhar ao serializar, usar objeto vazio
                        estadoJson = '{}';
                    }
                }
                
                // Fazer chamada assíncrona - não bloquear
                KingdomConfeitaria.Ajax.callHandler(
                    handlerPath,
                    {
                        acao: 'SalvarNavegacao',
                        url: url,
                        estado: estadoJson
                    },
                    'POST',
                    function(result) {
                        // Logar resultado para debug
                        if (result && result.sucesso) {
                            console.log('Navegação salva com sucesso:', result);
                        } else {
                            console.error('Erro ao salvar navegação:', result);
                        }
                    },
                    function(error) {
                        // Logar erro completo para identificar problema
                        console.error('Erro na requisição AJAX ao salvar navegação:', error);
                    }
                );
            } catch (e) {
                // Erro ao tentar salvar - ignorar (não crítico)
            }
        }
    },

    /**
     * Navegar para uma página salvando a atual
     */
    navegar: function(url, estadoControles) {
        // Salvar estado atual antes de navegar
        var urlAtual = window.location.pathname;
        
        // Se não foi fornecido estado, coletar automaticamente
        if (!estadoControles) {
            estadoControles = this.coletarEstadoControles();
        }
        
        this.salvarPaginaAtual(urlAtual, estadoControles);
        
        // Navegar
        window.location.href = url;
    },

    /**
     * Voltar para página anterior (sem salvar alterações)
     */
    voltar: function() {
        // Salvar estado antes de voltar (para caso o usuário volte novamente)
        this.salvarEstadoControles();
        
        // Determinar caminho relativo do handler e página padrão baseado na localização atual
        var handlerPath = window.location.pathname.indexOf('/paginas/') > -1 
            ? '../Handlers/CallbackHandler.ashx' 
            : 'Handlers/CallbackHandler.ashx';
        var defaultPage = window.location.pathname.indexOf('/paginas/') > -1 
            ? '../Default.aspx' 
            : 'Default.aspx';
        
        // Obter página anterior via Handler
        if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Ajax) {
            KingdomConfeitaria.Ajax.callHandler(
                handlerPath,
                {
                    acao: 'ObterPaginaAnterior'
                },
                'POST',
                function(result) {
                    if (result && result.url) {
                        window.location.href = result.url;
                    } else {
                        // Fallback
                        if (window.history.length > 1) {
                            window.history.back();
                        } else {
                            window.location.href = defaultPage;
                        }
                    }
                },
                function(error) {
                    console.error('Erro ao obter página anterior:', error);
                    // Fallback
                    if (window.history.length > 1) {
                        window.history.back();
                    } else {
                        window.location.href = defaultPage;
                    }
                }
            );
        } else {
            // Fallback
            if (window.history.length > 1) {
                window.history.back();
            } else {
                window.location.href = defaultPage;
            }
        }
    },

    /**
     * Obter estado salvo de uma página
     */
    obterEstado: function(url, callback) {
        // Determinar caminho relativo do handler baseado na localização atual
        var handlerPath = window.location.pathname.indexOf('/paginas/') > -1 
            ? '../Handlers/CallbackHandler.ashx' 
            : 'Handlers/CallbackHandler.ashx';
        
        if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Ajax) {
            KingdomConfeitaria.Ajax.callHandler(
                handlerPath,
                {
                    acao: 'ObterEstadoPagina',
                    url: url
                },
                'POST',
                function(result) {
                    if (callback) callback(result);
                },
                function(error) {
                    console.error('Erro ao obter estado:', error);
                    if (callback) callback({});
                }
            );
        } else {
            if (callback) callback({});
        }
    },

    /**
     * Coletar estado de controles da página atual
     */
    coletarEstadoControles: function() {
        var estado = {};
        
        // Coletar valores de todos os inputs, selects e textareas
        var inputs = document.querySelectorAll('input, select, textarea');
        inputs.forEach(function(input) {
            if (input.id && input.id !== '') {
                // Ignorar inputs de tipo submit, button, reset, image
                if (input.type === 'submit' || input.type === 'button' || 
                    input.type === 'reset' || input.type === 'image') {
                    return;
                }
                
                if (input.type === 'checkbox' || input.type === 'radio') {
                    estado[input.id] = input.checked;
                } else {
                    estado[input.id] = input.value;
                }
            }
        });
        
        return estado;
    },

    /**
     * Salvar estado de controles da página atual
     */
    salvarEstadoControles: function() {
        var estado = this.coletarEstadoControles();
        
        // Salvar estado
        var urlAtual = window.location.pathname;
        this.salvarPaginaAtual(urlAtual, estado);
    }
};

// Salvar estado antes de sair da página
window.addEventListener('beforeunload', function() {
    if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Navigation) {
        // Tentar salvar estado antes de sair (pode não funcionar sempre em beforeunload)
        try {
            // Usar sendBeacon se disponível (mais confiável para beforeunload)
            if (navigator.sendBeacon) {
                var estado = KingdomConfeitaria.Navigation.coletarEstadoControles();
                var urlAtual = window.location.pathname;
                var estadoJson = JSON.stringify(estado);
                
                var handlerPath = window.location.pathname.indexOf('/paginas/') > -1 
                    ? '../Handlers/CallbackHandler.ashx' 
                    : 'Handlers/CallbackHandler.ashx';
                
                // sendBeacon precisa de Blob ou FormData, mas FormData não funciona bem
                // Vamos usar uma requisição síncrona simples (XMLHttpRequest com async=false)
                var xhr = new XMLHttpRequest();
                xhr.open('POST', handlerPath, false); // false = síncrono (bloqueia até completar)
                xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
                
                var params = 'acao=SalvarNavegacao&url=' + encodeURIComponent(urlAtual) + '&estado=' + encodeURIComponent(estadoJson);
                xhr.send(params);
            } else {
                // Fallback: tentar salvar normalmente (pode não funcionar em beforeunload)
                KingdomConfeitaria.Navigation.salvarEstadoControles();
            }
        } catch (e) {
            // Ignorar erros em beforeunload (não crítico)
        }
    }
});

// Salvar estado periodicamente (a cada 60 segundos)
// REABILITADO para identificar problemas reais
setInterval(function() {
    if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Navigation) {
        KingdomConfeitaria.Navigation.salvarEstadoControles();
    }
}, 60000);

