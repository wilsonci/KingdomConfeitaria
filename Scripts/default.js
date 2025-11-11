/**
 * Kingdom Confeitaria - JavaScript específico para Default.aspx
 */

// Funções específicas da página principal
var DefaultPage = DefaultPage || {};

// Funções de carrinho
DefaultPage.Carrinho = {
    /**
     * Obter preço do produto baseado no tamanho selecionado
     */
    obterPrecoDoProduto: function(produtoId) {
        var tamanhoElement = document.getElementById('tamanho_' + produtoId);
        if (!tamanhoElement) {
            return null;
        }

        // Se for um select, pegar a opção selecionada
        if (tamanhoElement.tagName === 'SELECT') {
            var option = tamanhoElement.options[tamanhoElement.selectedIndex];
            if (option) {
                return option.getAttribute('data-preco');
            }
        }
        // Se for um input hidden, pegar o atributo data-preco
        else if (tamanhoElement.tagName === 'INPUT') {
            return tamanhoElement.getAttribute('data-preco');
        }

        return null;
    },

    /**
     * Adicionar produto ao carrinho
     */
    adicionar: function(produtoId, nome, tamanho, quantidade) {
        quantidade = quantidade || 1;

        // Obter o preço do elemento
        var preco = this.obterPrecoDoProduto(produtoId);

        // Validar e normalizar o preço
        if (!preco || preco === '' || preco === 'undefined' || preco === 'null') {
            alert('Erro: Preço inválido. Por favor, selecione um tamanho.');
            return;
        }

        // Garantir que o preço use ponto como separador decimal
        var precoNormalizado = String(preco).replace(',', '.').trim();

        // Validar se é um número válido
        if (isNaN(parseFloat(precoNormalizado))) {
            alert('Erro: Preço inválido: ' + preco);
            return;
        }

        // Obter o tamanho se não foi fornecido
        if (!tamanho) {
            var tamanhoElement = document.getElementById('tamanho_' + produtoId);
            if (tamanhoElement) {
                tamanho = tamanhoElement.value;
            }
        }

        KingdomConfeitaria.Utils.postBack('AdicionarAoCarrinho', produtoId + '|' + nome + '|' + tamanho + '|' + precoNormalizado + '|' + quantidade);
    },

    /**
     * Atualizar quantidade de um item
     */
    atualizarQuantidade: function(produtoId, tamanho, incremento) {
        KingdomConfeitaria.Utils.postBack('AtualizarQuantidade', produtoId + '|' + tamanho + '|' + incremento);
    },

    /**
     * Remover item do carrinho
     */
    remover: function(produtoId, tamanho) {
        KingdomConfeitaria.Utils.postBack('RemoverItem', produtoId + '|' + tamanho);
    }
};

// Funções de seleção de tamanho
DefaultPage.Tamanho = {
    /**
     * Selecionar tamanho de um produto
     */
    selecionar: function(btn, produtoId, tamanho, preco) {
        // Remover classe active de todos os botões do mesmo produto
        var card = btn.closest('.produto-card');
        if (card) {
            card.querySelectorAll('.btn-tamanho').forEach(function(b) {
                b.classList.remove('active');
            });
        }

        // Adicionar classe active ao botão clicado
        btn.classList.add('active');

        // Mostrar container de quantidade
        var quantidadeContainer = document.getElementById('quantidadeContainer_' + produtoId);
        if (quantidadeContainer) {
            quantidadeContainer.style.display = 'block';
            var tamanhoSelecionado = document.getElementById('tamanhoSelecionado_' + produtoId);
            var precoSelecionado = document.getElementById('precoSelecionado_' + produtoId);
            if (tamanhoSelecionado) tamanhoSelecionado.value = tamanho;
            if (precoSelecionado) precoSelecionado.value = preco;
        }
    },

    /**
     * Aumentar quantidade
     */
    aumentarQuantidade: function(produtoId) {
        var input = document.getElementById('quantidade_' + produtoId);
        if (input) {
            var valor = parseInt(input.value) || 1;
            input.value = valor + 1;
        }
    },

    /**
     * Diminuir quantidade
     */
    diminuirQuantidade: function(produtoId) {
        var input = document.getElementById('quantidade_' + produtoId);
        if (input) {
            var valor = parseInt(input.value) || 1;
            if (valor > 1) {
                input.value = valor - 1;
            }
        }
    }
};

// Funções de modal de reserva
DefaultPage.ModalReserva = {
    /**
     * Abrir modal de reserva
     */
    abrir: function() {
        KingdomConfeitaria.Modal.show('modalReserva');
        
        // Verificar se usuário está logado e mostrar/ocultar áreas apropriadas
        var estaLogado = window.usuarioLogado === true;
        
        // Tentar encontrar elementos pelos IDs (podem ter ClientIDs do ASP.NET)
        var divAvisoLogin = document.getElementById('divAvisoLogin');
        var divUsuarioLogado = document.getElementById('divUsuarioLogado');
        var divLoginDinamico = document.querySelector('[id*="divLoginDinamico"]');
        var divDadosReserva = document.querySelector('[id*="divDadosReserva"]');
        var btnConfirmarReserva = document.querySelector('[id*="btnConfirmarReserva"]');
        
        if (divAvisoLogin) {
            divAvisoLogin.style.display = estaLogado ? 'none' : 'block';
        }
        if (divUsuarioLogado) {
            divUsuarioLogado.style.display = estaLogado ? 'block' : 'none';
        }
        
        // Ocultar botões de login inicialmente (serão mostrados quando senha for solicitada)
        var divBotoesLogin = document.getElementById('divBotoesLogin');
        if (divBotoesLogin) {
            divBotoesLogin.style.display = 'none';
        }
        
        // Se estiver logado, mostrar área de reserva e ocultar área de login
        if (estaLogado) {
            if (divLoginDinamico) {
                divLoginDinamico.style.display = 'none';
            }
            if (divDadosReserva) {
                divDadosReserva.style.display = 'block';
            }
            if (btnConfirmarReserva) {
                btnConfirmarReserva.style.display = 'inline-block';
            }
            
            // Mostrar botões de reserva
            var divBotoesReserva = document.getElementById('divBotoesReserva');
            if (divBotoesReserva) {
                divBotoesReserva.style.display = 'flex';
            }
            
            // Os dados do cliente já devem estar preenchidos pelo servidor no Page_Load
            // Mas vamos garantir que os campos estejam visíveis
            var txtNome = document.querySelector('[id*="txtNome"]');
            var txtEmail = document.querySelector('[id*="txtEmail"]');
            var txtTelefone = document.querySelector('[id*="txtTelefone"]');
            
            // Se os campos estiverem vazios, tentar preencher com dados da sessão (se disponíveis via JavaScript)
            // Normalmente os dados já vêm do servidor, mas isso é um fallback
            if (txtNome && !txtNome.value && window.clienteNome) {
                txtNome.value = window.clienteNome;
            }
            if (txtEmail && !txtEmail.value && window.clienteEmail) {
                txtEmail.value = window.clienteEmail;
            }
            if (txtTelefone && !txtTelefone.value && window.clienteTelefone) {
                txtTelefone.value = window.clienteTelefone;
            }
        } else {
            // Se não estiver logado, mostrar área de login e ocultar área de reserva
            if (divLoginDinamico) {
                divLoginDinamico.style.display = 'block';
            }
            if (divDadosReserva) {
                divDadosReserva.style.display = 'none';
            }
            if (btnConfirmarReserva) {
                btnConfirmarReserva.style.display = 'none';
            }
            
            // Ocultar botões de reserva
            var divBotoesReserva = document.getElementById('divBotoesReserva');
            if (divBotoesReserva) {
                divBotoesReserva.style.display = 'none';
            }
        }
        
        // Re-inicializar botões após abrir
        setTimeout(function() {
            KingdomConfeitaria.Modal.initCloseButtons('modalReserva');
        }, 100);
    },

    /**
     * Fechar modal de reserva
     */
    fechar: function() {
        KingdomConfeitaria.Modal.hide('modalReserva');
    },

    /**
     * Validar formulário de reserva
     * Os ClientIDs serão configurados via script inline no Default.aspx
     */
    validarFormulario: function() {
        // Esta função será sobrescrita pelo script inline no Default.aspx
        // que terá acesso aos ClientIDs do servidor
        var modal = document.getElementById('modalReserva');
        if (!modal || !modal.classList.contains('show')) {
            return true;
        }

        // Login automático será feito no servidor se necessário
        // Não é necessário validar login aqui, o servidor fará login automático

        // Tentar encontrar os campos pelos IDs padrão ou por atributos
        var nome = document.getElementById('txtNome') || document.querySelector('[id*="txtNome"]');
        var email = document.getElementById('txtEmail') || document.querySelector('[id*="txtEmail"]');
        var telefone = document.getElementById('txtTelefone') || document.querySelector('[id*="txtTelefone"]');
        var dataRetirada = document.getElementById('ddlDataRetirada') || document.querySelector('[id*="ddlDataRetirada"]');

        if (!nome || !email || !telefone || !dataRetirada) {
            console.warn('Campos do formulário não encontrados. Usando validação básica.');
            return true; // Deixar validação do servidor lidar com isso
        }

        var primeiroInvalido = null;
        var nomeValido = DefaultPage.Validacao.validarNome(nome);
        if (!nomeValido && !primeiroInvalido) primeiroInvalido = nome;
        var emailValido = DefaultPage.Validacao.validarEmail(email);
        if (!emailValido && !primeiroInvalido) primeiroInvalido = email;
        var telefoneValido = DefaultPage.Validacao.validarTelefone(telefone);
        if (!telefoneValido && !primeiroInvalido) primeiroInvalido = telefone;
        var dataValida = dataRetirada && dataRetirada.value && dataRetirada.value !== '';
        if (!dataValida && !primeiroInvalido) primeiroInvalido = dataRetirada;

        if (!nomeValido || !emailValido || !telefoneValido || !dataValida) {
            if (primeiroInvalido) {
                try {
                    primeiroInvalido.focus();
                    primeiroInvalido.scrollIntoView({ behavior: 'smooth', block: 'center' });
                } catch (e) {
                    console.error('Erro ao focar campo inválido:', e);
                }
            }
            alert('Por favor, preencha todos os campos obrigatórios corretamente.');
            return false;
        }
        return true;
    }
};

// Funções de validação de campos
DefaultPage.Validacao = {
    /**
     * Validar campo de telefone
     */
    validarTelefone: function(input) {
        var value = input.value.replace(/\D/g, '');
        var isValid = value.length >= 10 && value.length <= 11;

        if (isValid) {
            input.classList.remove('is-invalid');
            input.classList.add('is-valid');
        } else {
            input.classList.remove('is-valid');
            input.classList.add('is-invalid');
        }

        return isValid;
    },

    /**
     * Validar campo de nome
     */
    validarNome: function(input) {
        var nome = input.value.trim();
        var isValid = nome.length >= 3;

        if (isValid) {
            input.classList.remove('is-invalid');
            input.classList.add('is-valid');
        } else {
            input.classList.remove('is-valid');
            input.classList.add('is-invalid');
        }

        return isValid;
    },

    /**
     * Validar campo de email
     */
    validarEmail: function(input) {
        var email = input.value.trim();
        var isValid = email.length > 0 && email.includes('@') && email.includes('.');

        if (isValid) {
            input.classList.remove('is-invalid');
            input.classList.add('is-valid');
        } else {
            input.classList.remove('is-valid');
            input.classList.add('is-invalid');
        }

        return isValid;
    }
};

// Funções de imagens
DefaultPage.Imagens = {
    /**
     * Carregar imagens de forma silenciosa
     */
    carregarSilenciosamente: function() {
        var imagens = document.querySelectorAll('img.produto-imagem[data-original-src]');
        imagens.forEach(function(img) {
            var originalSrc = img.getAttribute('data-original-src');
            if (originalSrc && originalSrc !== '') {
                // Tentar carregar a imagem real
                var testImg = new Image();
                testImg.onload = function() {
                    // Se carregou com sucesso, usar a imagem real
                    if (img.src.indexOf('data:image/svg') === -1) {
                        img.src = originalSrc;
                    }
                };
                testImg.onerror = function() {
                    // Se falhou, manter o placeholder (já está definido)
                };
                // Só tentar carregar se não for um data URI
                if (img.src.indexOf('data:image/svg') === 0 && originalSrc.indexOf('data:') !== 0) {
                    testImg.src = originalSrc;
                }
            }
        });
    }
};

// Inicialização quando a página carregar
KingdomConfeitaria.Utils.ready(function() {
    // Carregar imagens silenciosamente
    DefaultPage.Imagens.carregarSilenciosamente();

    // Suprimir erros 404 de imagens no console
    window.addEventListener('error', function(e) {
        if (e.target && e.target.tagName === 'IMG') {
            if (e.target.classList.contains('produto-imagem')) {
                var placeholderSvg = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='200' height='200'%3E%3Crect width='200' height='200' fill='%23e9ecef'/%3E%3Ctext x='50%25' y='50%25' font-family='Arial' font-size='14' fill='%23999999' text-anchor='middle' dy='.3em'%3EImagem N%26atilde%3Bo Disponível%3C/text%3E%3C/svg%3E";
                if (e.target.src !== placeholderSvg) {
                    e.target.src = placeholderSvg;
                }
                e.preventDefault();
                return true;
            }
        }
    }, true);

    // Re-inicializar após postback
    window.addEventListener('pageshow', function(event) {
        DefaultPage.Imagens.carregarSilenciosamente();
        KingdomConfeitaria.Modal.initCloseButtons('modalReserva');
    });
});

// Funções globais para compatibilidade com código inline
function obterPrecoDoProduto(produtoId) {
    return DefaultPage.Carrinho.obterPrecoDoProduto(produtoId);
}

function adicionarAoCarrinho(produtoId, nome, tamanho, quantidade) {
    DefaultPage.Carrinho.adicionar(produtoId, nome, tamanho, quantidade);
}

function atualizarQuantidade(produtoId, tamanho, incremento) {
    DefaultPage.Carrinho.atualizarQuantidade(produtoId, tamanho, incremento);
}

function removerItem(produtoId, tamanho) {
    DefaultPage.Carrinho.remover(produtoId, tamanho);
}

function selecionarTamanho(btn, produtoId, tamanho, preco) {
    DefaultPage.Tamanho.selecionar(btn, produtoId, tamanho, preco);
}

function aumentarQuantidade(produtoId) {
    DefaultPage.Tamanho.aumentarQuantidade(produtoId);
}

function diminuirQuantidade(produtoId) {
    DefaultPage.Tamanho.diminuirQuantidade(produtoId);
}

function atualizarPreco(produtoId) {
    var select = document.getElementById('tamanho_' + produtoId);
    if (select && select.tagName === 'SELECT') {
        var option = select.options[select.selectedIndex];
        if (option) {
            var preco = parseFloat(option.getAttribute('data-preco'));
            var precoElement = document.getElementById('precoUnitario_' + produtoId);
            if (precoElement && !isNaN(preco)) {
                precoElement.textContent = 'R$ ' + preco.toFixed(2).replace('.', ',');
            }
        }
    }
}

function atualizarTotalSelecionado(sacoId) {
    var seletores = document.querySelectorAll('.seletor-produto-saco[data-saco-id="' + sacoId + '"]');
    var total = 0;
    seletores.forEach(function(select) {
        if (select.value && select.value !== '') {
            total++;
        }
    });
    var totalElement = document.getElementById('totalSelecionado_' + sacoId);
    if (totalElement) {
        totalElement.textContent = total;
    }
}

function adicionarSacoAoCarrinho(sacoId, nomeSaco, tamanhoSaco, quantidadeMaxima) {
    var seletores = document.querySelectorAll('.seletor-produto-saco[data-saco-id="' + sacoId + '"]');
    var produtosSelecionados = [];
    var todosPreenchidos = true;
    
    seletores.forEach(function(select) {
        if (select.value && select.value !== '') {
            produtosSelecionados.push(select.value);
        } else {
            todosPreenchidos = false;
        }
    });
    
    if (!todosPreenchidos || produtosSelecionados.length !== quantidadeMaxima) {
        alert('Por favor, selecione todos os ' + quantidadeMaxima + ' biscoitos para o saco.');
        return;
    }
    
    // Obter o preço do saco
    var preco = obterPrecoDoProduto(sacoId);
    if (!preco) {
        alert('Erro: Preço inválido.');
        return;
    }
    
    var precoNormalizado = String(preco).replace(',', '.').trim();
    
    // Enviar os dados: sacoId|nomeSaco|tamanhoSaco|preco|quantidade|produtosSelecionados
    var quantidade = document.getElementById('quantidade_' + sacoId) ? document.getElementById('quantidade_' + sacoId).value : 1;
    var dados = sacoId + '|' + nomeSaco + '|' + tamanhoSaco + '|' + precoNormalizado + '|' + quantidade + '|' + produtosSelecionados.join(',');
    
    KingdomConfeitaria.Utils.postBack('AdicionarSacoAoCarrinho', dados);
}

function validarCampoTelefone(input) {
    return DefaultPage.Validacao.validarTelefone(input);
}

function validarCampoNome(input) {
    return DefaultPage.Validacao.validarNome(input);
}

function validarCampoEmail(input) {
    return DefaultPage.Validacao.validarEmail(input);
}

function validarFormularioReserva() {
    return DefaultPage.ModalReserva.validarFormulario();
}

function fecharModalReserva() {
    DefaultPage.ModalReserva.fechar();
}


