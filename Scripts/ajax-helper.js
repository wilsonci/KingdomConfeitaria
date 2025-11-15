/**
 * Kingdom Confeitaria - AJAX Helper
 * Biblioteca para fazer chamadas AJAX sem ScriptManager
 * Substitui PageMethods e permite chamadas a Page Methods, Web Services e Handlers
 */

var KingdomConfeitaria = KingdomConfeitaria || {};

KingdomConfeitaria.Ajax = {
    /**
     * Chamar Page Method sem ScriptManager
     * @param {string} pagePath - Caminho da página (ex: "Default.aspx")
     * @param {string} methodName - Nome do método
     * @param {object|array} parameters - Parâmetros do método
     * @param {function} onSuccess - Callback de sucesso
     * @param {function} onError - Callback de erro
     */
    callPageMethod: function(pagePath, methodName, parameters, onSuccess, onError) {
        // Construir URL do Page Method
        var url = pagePath + '/PageMethods/' + methodName;
        
        // Preparar dados
        var data = JSON.stringify(parameters || {});
        
        // Fazer requisição
        this._makeRequest(url, 'POST', data, {
            'Content-Type': 'application/json; charset=utf-8'
        }, onSuccess, onError);
    },
    
    /**
     * Chamar Web Service ASMX sem ScriptManager
     * @param {string} servicePath - Caminho do serviço (ex: "Services/CallbackService.asmx")
     * @param {string} methodName - Nome do método
     * @param {object|array} parameters - Parâmetros do método
     * @param {function} onSuccess - Callback de sucesso
     * @param {function} onError - Callback de erro
     */
    callWebService: function(servicePath, methodName, parameters, onSuccess, onError) {
        // Construir URL do Web Service
        var url = servicePath + '/' + methodName;
        
        // Preparar dados no formato SOAP ou JSON
        var data = JSON.stringify(parameters || {});
        
        // Fazer requisição
        this._makeRequest(url, 'POST', data, {
            'Content-Type': 'application/json; charset=utf-8'
        }, onSuccess, onError);
    },
    
    /**
     * Chamar Generic Handler (.ashx) sem ScriptManager
     * @param {string} handlerPath - Caminho do handler (ex: "Handlers/CallbackHandler.ashx")
     * @param {object} parameters - Parâmetros (serão enviados como query string ou body)
     * @param {string} method - Método HTTP ('GET' ou 'POST')
     * @param {function} onSuccess - Callback de sucesso
     * @param {function} onError - Callback de erro
     */
    callHandler: function(handlerPath, parameters, method, onSuccess, onError) {
        method = method || 'POST';
        var url = handlerPath;
        var data = null;
        var headers = {
            'Content-Type': 'application/json; charset=utf-8'
        };
        
        if (method === 'GET') {
            // Adicionar parâmetros na query string
            var queryString = this._objectToQueryString(parameters || {});
            if (queryString) {
                url += (url.indexOf('?') === -1 ? '?' : '&') + queryString;
            }
        } else {
            // Enviar parâmetros no body
            data = JSON.stringify(parameters || {});
        }
        
        this._makeRequest(url, method, data, headers, onSuccess, onError);
    },
    
    /**
     * Fazer requisição HTTP genérica
     * @param {string} url - URL da requisição
     * @param {string} method - Método HTTP
     * @param {string} data - Dados a enviar
     * @param {object} headers - Headers HTTP
     * @param {function} onSuccess - Callback de sucesso
     * @param {function} onError - Callback de erro
     */
    _makeRequest: function(url, method, data, headers, onSuccess, onError) {
        // Usar fetch API (moderno) ou XMLHttpRequest (compatibilidade)
        if (typeof fetch !== 'undefined') {
            this._makeFetchRequest(url, method, data, headers, onSuccess, onError);
        } else {
            this._makeXHRRequest(url, method, data, headers, onSuccess, onError);
        }
    },
    
    /**
     * Fazer requisição usando Fetch API
     */
    _makeFetchRequest: function(url, method, data, headers, onSuccess, onError) {
        var options = {
            method: method,
            headers: headers || {},
            credentials: 'same-origin' // IMPORTANTE: Envia cookies (Session)
        };
        
        if (data && (method === 'POST' || method === 'PUT')) {
            options.body = data;
        }
        
        fetch(url, options)
            .then(function(response) {
                if (!response.ok) {
                    throw new Error('HTTP error! status: ' + response.status);
                }
                return response.text();
            })
            .then(function(text) {
                try {
                    // Tentar parsear como JSON
                    var result = JSON.parse(text);
                    
                    // Se for resposta de Page Method ou Web Service, extrair o resultado
                    if (result.d !== undefined) {
                        result = result.d; // Page Methods retornam em .d
                    }
                    
                    if (onSuccess) {
                        onSuccess(result);
                    }
                } catch (e) {
                    // Se não for JSON, pode ser JavaScript para executar
                    if (text && text.trim().length > 0) {
                        if (onSuccess) {
                            onSuccess(text);
                        }
                    } else {
                        if (onSuccess) {
                            onSuccess(null);
                        }
                    }
                }
            })
            .catch(function(error) {
                console.error('Erro na requisição AJAX:', error);
                if (onError) {
                    onError(error);
                }
            });
    },
    
    /**
     * Fazer requisição usando XMLHttpRequest (fallback para navegadores antigos)
     */
    _makeXHRRequest: function(url, method, data, headers, onSuccess, onError) {
        var xhr = new XMLHttpRequest();
        
        xhr.open(method, url, true);
        
        // Adicionar headers
        if (headers) {
            for (var key in headers) {
                if (headers.hasOwnProperty(key)) {
                    xhr.setRequestHeader(key, headers[key]);
                }
            }
        }
        
        // IMPORTANTE: Permitir cookies (Session)
        xhr.withCredentials = true;
        
        xhr.onreadystatechange = function() {
            if (xhr.readyState === 4) {
                if (xhr.status >= 200 && xhr.status < 300) {
                    try {
                        var result = JSON.parse(xhr.responseText);
                        
                        // Se for resposta de Page Method ou Web Service, extrair o resultado
                        if (result.d !== undefined) {
                            result = result.d;
                        }
                        
                        if (onSuccess) {
                            onSuccess(result);
                        }
                    } catch (e) {
                        // Se não for JSON, pode ser JavaScript para executar
                        if (xhr.responseText && xhr.responseText.trim().length > 0) {
                            if (onSuccess) {
                                onSuccess(xhr.responseText);
                            }
                        } else {
                            if (onSuccess) {
                                onSuccess(null);
                            }
                        }
                    }
                } else {
                    var error = new Error('HTTP error! status: ' + xhr.status);
                    console.error('Erro na requisição AJAX:', error);
                    if (onError) {
                        onError(error);
                    }
                }
            }
        };
        
        xhr.send(data);
    },
    
    /**
     * Converter objeto para query string
     */
    _objectToQueryString: function(obj) {
        var parts = [];
        for (var key in obj) {
            if (obj.hasOwnProperty(key)) {
                parts.push(encodeURIComponent(key) + '=' + encodeURIComponent(obj[key]));
            }
        }
        return parts.join('&');
    },
    
    /**
     * Executar JavaScript retornado do servidor
     * @param {string} javascript - Código JavaScript para executar
     */
    executeJavaScript: function(javascript) {
        if (!javascript || typeof javascript !== 'string') {
            return;
        }
        
        try {
            // Criar função anônima e executar
            var func = new Function(javascript);
            func();
        } catch (e) {
            console.error('Erro ao executar JavaScript do servidor:', e);
            console.error('JavaScript:', javascript);
        }
    }
};

/**
 * Wrapper para compatibilidade com PageMethods (substitui ScriptManager)
 * Permite usar a mesma sintaxe do PageMethods, mas sem ScriptManager
 */
window.PageMethods = window.PageMethods || {
    // Este objeto será populado dinamicamente com os métodos da página
    // Exemplo: PageMethods.VerificarClienteCadastrado = function(login, onSuccess, onError) { ... }
};

/**
 * Função helper para criar métodos PageMethods dinamicamente
 * @param {string} pagePath - Caminho da página
 * @param {string} methodName - Nome do método
 */
function createPageMethod(pagePath, methodName) {
    return function() {
        var args = Array.prototype.slice.call(arguments);
        var onSuccess = null;
        var onError = null;
        var parameters = [];
        
        // Separar parâmetros de callbacks
        args.forEach(function(arg) {
            if (typeof arg === 'function') {
                if (onSuccess === null) {
                    onSuccess = arg;
                } else {
                    onError = arg;
                }
            } else {
                parameters.push(arg);
            }
        });
        
        // Chamar via AJAX
        KingdomConfeitaria.Ajax.callPageMethod(
            pagePath,
            methodName,
            parameters,
            function(result) {
                if (onSuccess) {
                    onSuccess(result);
                }
            },
            function(error) {
                if (onError) {
                    onError(error);
                } else {
                    console.error('Erro ao chamar PageMethod:', methodName, error);
                }
            }
        );
    };
}

/**
 * Inicializar PageMethods para uma página específica
 * @param {string} pagePath - Caminho da página (ex: "Default.aspx")
 * @param {array} methodNames - Array com nomes dos métodos disponíveis
 */
function initPageMethods(pagePath, methodNames) {
    if (!methodNames || !Array.isArray(methodNames)) {
        return;
    }
    
    methodNames.forEach(function(methodName) {
        window.PageMethods[methodName] = createPageMethod(pagePath, methodName);
    });
}

