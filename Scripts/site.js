function selecionarTamanho(btn, produtoId, tamanho, preco) {
    // Remover classe active de todos os botões do mesmo produto
    var card = btn.closest('.produto-card');
    card.querySelectorAll('.btn-tamanho').forEach(function(b) {
        b.classList.remove('active');
    });
    
    // Adicionar classe active ao botão clicado
    btn.classList.add('active');
    
    // Mostrar container de quantidade
    var quantidadeContainer = document.getElementById('quantidadeContainer_' + produtoId);
    if (quantidadeContainer) {
        quantidadeContainer.style.display = 'block';
        document.getElementById('tamanhoSelecionado_' + produtoId).value = tamanho;
        document.getElementById('precoSelecionado_' + produtoId).value = preco;
    }
}

function atualizarQuantidade(produtoId, tamanho, incremento) {
    var input = document.getElementById('quantidade_' + produtoId);
    if (input) {
        var quantidade = parseInt(input.value) || 1;
        quantidade += incremento;
        if (quantidade < 1) quantidade = 1;
        input.value = quantidade;
        
        // Atualizar no servidor
        __doPostBack('AtualizarQuantidade', produtoId + '|' + tamanho + '|' + incremento);
    }
}

