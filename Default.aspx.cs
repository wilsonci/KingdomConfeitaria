using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Web.Script.Services;
using System.Web.Script.Serialization;
using KingdomConfeitaria.Models;
using KingdomConfeitaria.Services;
using KingdomConfeitaria.Helpers;
using KingdomConfeitaria.Security;

namespace KingdomConfeitaria
{
    public partial class Default : BasePage
    {
        private DatabaseService _databaseService;
        
        // Helper para escapar strings JavaScript com caracteres especiais
        protected string EscapeJavaScript(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            
            return input
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
        }
        
        // Helper para adicionar scripts usando data attributes em vez de ScriptManager
        private void AddPageScript(string script)
        {
            // Adicionar script como data attribute no body para ser executado após carregar
            if (Page.Header != null)
            {
                var scriptTag = new System.Web.UI.HtmlControls.HtmlGenericControl("script");
                scriptTag.Attributes["type"] = "text/javascript";
                scriptTag.InnerHtml = script;
                Page.Header.Controls.Add(scriptTag);
            }
        }
        
        private List<ItemPedido> Carrinho
        {
            get
            {
                if (Session["Carrinho"] == null)
                    Session["Carrinho"] = new List<ItemPedido>();
                return (List<ItemPedido>)Session["Carrinho"];
            }
            set { Session["Carrinho"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Configurar encoding UTF-8
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.Charset = "UTF-8";
            
            // Criar DatabaseService apenas quando necessário (lazy loading)
            // Não criar aqui para evitar inicialização desnecessária

            // Verificar status de login e definir variável JavaScript
            bool estaLogado = Session["ClienteId"] != null && !Session.IsNewSession;
            Page.ClientScript.RegisterStartupScript(this.GetType(), "DefinirStatusLogin", 
                $"window.usuarioLogado = {estaLogado.ToString().ToLower()};", true);

            // Sempre verificar login para atualizar links do header
            VerificarLogin();
            
            if (!IsPostBack)
            {
                // Inicializar DatabaseService apenas quando necessário
                if (_databaseService == null)
                {
                    _databaseService = new DatabaseService();
                }
                
                // Garantir que o carrinho está inicializado corretamente
                if (Session["Carrinho"] == null)
                {
                    Session["Carrinho"] = new List<ItemPedido>();
                }
                
                CarregarProdutos();
                CarregarDatasRetirada();
                AtualizarCarrinho(); // Atualizar carrinho na primeira carga
                
                // Restaurar estado dos controles
                SessionStateHelper.RestaurarEstadoPagina(this);
                
                // Se usuário acabou de fazer login e tem parâmetro abrirReserva, redirecionar para página de reserva
                // MAS apenas se o carrinho não estiver vazio
                string abrirReservaParam = Request.QueryString["abrirReserva"];
                bool abrirReserva = !string.IsNullOrEmpty(abrirReservaParam) && 
                                    Security.InputValidator.SanitizeString(abrirReservaParam).ToLower() == "true";
                if (abrirReserva && estaLogado)
                {
                    // Verificar se há produtos no carrinho antes de redirecionar
                    var carrinho = Session["Carrinho"] as List<ItemPedido>;
                    if (carrinho != null && carrinho.Count > 0)
                    {
                        // Redirecionar para página de reserva apenas se houver produtos
                        Response.Redirect("paginas/Reserva.aspx");
                        return;
                    }
                    // Se carrinho vazio, não redirecionar (ficar na home)
                }
            }

            string eventTarget = Request["__EVENTTARGET"];
            string eventArgument = Request["__EVENTARGUMENT"];

            if (!string.IsNullOrEmpty(eventTarget))
            {
                // Inicializar DatabaseService apenas quando necessário
                if (_databaseService == null)
                {
                    _databaseService = new DatabaseService();
                }
                
                if (eventTarget == "AdicionarAoCarrinho")
                {
                    ProcessarAdicionarAoCarrinho(eventArgument);
                    AtualizarCarrinho();
                    // Recarregar produtos para garantir que permaneçam visíveis após postback
                    CarregarProdutos();
                }
                else if (eventTarget == "AdicionarSacoAoCarrinho")
                {
                    ProcessarAdicionarSacoAoCarrinho(eventArgument);
                    AtualizarCarrinho();
                    // Recarregar produtos para garantir que permaneçam visíveis após postback
                    CarregarProdutos();
                }
                else if (eventTarget == "AtualizarQuantidade")
                {
                    ProcessarAtualizarQuantidade(eventArgument);
                    AtualizarCarrinho();
                    // Recarregar produtos para garantir que permaneçam visíveis após postback
                    CarregarProdutos();
                }
                else if (eventTarget == "RemoverItem")
                {
                    ProcessarRemoverItem(eventArgument);
                    AtualizarCarrinho();
                    // Recarregar produtos para garantir que permaneçam visíveis após postback
                    CarregarProdutos();
                }
            }
            else if (!IsPostBack)
            {
                AtualizarCarrinho();
            }
        }

        private void CarregarProdutos()
        {
            try
            {
                // Garantir que DatabaseService está inicializado
                if (_databaseService == null)
                {
                    _databaseService = new DatabaseService();
                }
                
                var produtos = _databaseService.ObterProdutos();
                
                // Se não houver produtos, mostrar mensagem
                if (produtos.Count == 0)
                {
                    produtosContainer.InnerHtml = "<div class='alert alert-warning' style='margin: 20px;'><i class='fas fa-exclamation-triangle'></i> Nenhum produto cadastrado no momento. Por favor, cadastre produtos na área administrativa.</div>";
                    return;
                }

                // Usar StringBuilder para melhor performance (evita múltiplas alocações de string)
                var htmlBuilder = new System.Text.StringBuilder(produtos.Count * 500); // Pré-alocar espaço estimado
                
                foreach (var produto in produtos)
                {
                    string nomeEscapado = produto.Nome.Replace("\"", "\\\"").Replace("'", "\\'");
                    string descricaoEscapada = (!string.IsNullOrEmpty(produto.Descricao) ? produto.Descricao : "").Replace("\"", "\\\"").Replace("'", "\\'");
                    // Garantir que o preço sempre tenha um valor válido
                    decimal precoDecimal = produto.Preco > 0 ? produto.Preco : 0;
                    string precoStr = precoDecimal.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                    
                    // Se o preço for 0, não adicionar o produto (produtos sem preço não devem aparecer)
                    if (precoDecimal <= 0)
                    {
                        continue; // Pular produtos sem preço válido
                    }
                    
                    // Extrair tamanho do nome do produto (Pequeno ou Grande)
                    string tamanho = "";
                    if (produto.Nome.Contains("Pequeno"))
                    {
                        tamanho = "Pequeno";
                    }
                    else if (produto.Nome.Contains("Grande"))
                    {
                        tamanho = "Grande";
                    }
                    
                    // Usar placeholder SVG inline se a imagem não existir
                    string placeholderSvg = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='200' height='200'%3E%3Crect width='200' height='200' fill='%23e9ecef'/%3E%3Ctext x='50%25' y='50%25' font-family='Arial' font-size='14' fill='%23999999' text-anchor='middle' dy='.3em'%3EImagem N%26atilde%3Bo Disponível%3C/text%3E%3C/svg%3E";
                    
                    // Determinar a URL da imagem - usar placeholder se vazio
                    string imagemSrc = !string.IsNullOrEmpty(produto.ImagemUrl) ? produto.ImagemUrl : placeholderSvg;
                    
                    // Gerar JSON com dados do produto para o modal (usar JavaScriptSerializer para garantir JSON válido)
                    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    
                    // Se for saco promocional, buscar os produtos permitidos
                    List<object> produtosPermitidosData = null;
                    if (produto.EhSacoPromocional && !string.IsNullOrEmpty(produto.Produtos))
                    {
                        try
                        {
                            var produtosIds = serializer.Deserialize<List<int>>(produto.Produtos);
                            var produtosPermitidos = _databaseService.ObterProdutosPorIds(produtosIds);
                            produtosPermitidosData = produtosPermitidos.Select(p => new
                            {
                                id = p.Id,
                                nome = p.Nome,
                                preco = p.Preco.ToString("F2").Replace(",", ".")
                            }).Cast<object>().ToList();
                        }
                        catch
                        {
                            // Se falhar ao parsear, deixar null
                        }
                    }
                    
                    var produtoData = new
                    {
                        id = produto.Id,
                        nome = produto.Nome,
                        descricao = produto.Descricao ?? "",
                        preco = precoStr,
                        tamanho = tamanho,
                        imagem = imagemSrc,
                        ehSaco = produto.EhSacoPromocional,
                        quantidadeSaco = produto.QuantidadeSaco,
                        produtos = produto.Produtos ?? "",
                        produtosPermitidos = produtosPermitidosData
                    };
                    string produtoJson = serializer.Serialize(produtoData);
                    
                    // Salvar produto na sessão para a página Produto.aspx
                    Session["Produto_" + produto.Id] = produtoJson;
                    
                    // Escapar JSON para uso em data attribute (mais seguro que onclick)
                    string produtoJsonEscapado = System.Web.HttpUtility.HtmlAttributeEncode(produtoJson);
                    
                    // HTML do card do carrossel - imagem abre modal, botão adiciona ao carrinho
                    string nomeJsEscapado = System.Web.HttpUtility.JavaScriptStringEncode(produto.Nome);
                    // Se não houver tamanho, usar "Único" como padrão
                    string tamanhoFinal = string.IsNullOrEmpty(tamanho) ? "Único" : tamanho;
                    string tamanhoJsEscapado = System.Web.HttpUtility.JavaScriptStringEncode(tamanhoFinal);
                    // O preço já está formatado com ponto como separador decimal (CultureInfo.InvariantCulture)
                    // Passar o preço diretamente como número no onclick (sem aspas) para evitar problemas de escape
                    
                    string html = string.Format(@"
                        <div class='produto-card-carrossel' data-produto-id='{0}' data-produto-json='{9}' data-produto-preco='{3}'>
                            <div class='produto-imagem-carrossel-wrapper' onclick='abrirModalProdutoFromCard(this.closest("".produto-card-carrossel""))'>
                                <img src='{5}' alt='{1}' class='produto-imagem-carrossel' loading='lazy' decoding='async' onerror='this.onerror=null; if(this.src !== ""{6}"") {{ this.src=""{6}""; }}' />
                            </div>
                            <div class='produto-nome-carrossel'>{1}</div>
                            <div class='produto-preco-carrossel'>R$ {7}</div>
                            <button type='button' class='btn-reservar-produto' onclick='reservarProdutoRapido(this);' title='Adicionar ao carrinho'>
                                <i class='fas fa-shopping-cart'></i> Reservar
                            </button>
                        </div>",
                        produto.Id,
                        System.Web.HttpUtility.HtmlEncode(produto.Nome),
                        System.Web.HttpUtility.HtmlEncode(produto.Descricao ?? ""),
                        precoStr, // Passar como número (sem aspas) - já está formatado com ponto
                        tamanhoFinal,
                        imagemSrc,
                        placeholderSvg,
                        produto.Preco.ToString("F2"),
                        produtoJson,
                        produtoJsonEscapado,
                        nomeJsEscapado,
                        tamanhoJsEscapado);
                    htmlBuilder.Append(html);
                }
                
                // Atribuir todo o HTML de uma vez (muito mais rápido que +=)
                produtosContainer.InnerHtml = htmlBuilder.ToString();
            }
            catch (Exception ex)
            {
                produtosContainer.InnerHtml = string.Format("<div class='alert alert-danger' style='margin: 20px;'>Erro ao carregar produtos: {0}</div>", System.Web.HttpUtility.HtmlEncode(ex.Message));
            }
        }

        private string GerarSeletoresProdutos(int sacoId, List<Produto> produtos, int quantidadeMaxima)
        {
            string html = "";
            for (int i = 0; i < quantidadeMaxima; i++)
            {
                string opcoes = "";
                foreach (var produto in produtos)
                {
                    opcoes += string.Format("<option value='{0}'>{1}</option>", produto.Id, produto.Nome);
                }
                
                html += string.Format(@"
                    <div class='mb-2'>
                        <label class='form-label'>Biscoito {0}:</label>
                        <select class='form-select seletor-produto-saco' data-saco-id='{1}' onchange='atualizarTotalSelecionado({1})'>
                            <option value=''>Selecione...</option>
                            {2}
                        </select>
                    </div>", i + 1, sacoId, opcoes);
            }
            return html;
        }

        private void CarregarDatasRetirada()
        {
            var ano = DateTime.Now.Year;
            var segundas = DateHelper.ObterSegundasAteNatal(ano);
            
            var radioGroup = FindControl("radioGroupDatas") as System.Web.UI.HtmlControls.HtmlGenericControl;
            if (radioGroup != null)
            {
                // Usar StringBuilder para melhor performance
                var htmlBuilder = new System.Text.StringBuilder(segundas.Count * 200);
                
                foreach (var data in segundas)
                {
                    string dataFormatada = data.ToString("yyyy-MM-dd");
                    string dataExibicao = data.ToString("dd/MM/yyyy");
                    string diaSemana = data.ToString("dddd", new System.Globalization.CultureInfo("pt-BR"));
                    string diaSemanaCapitalizado = char.ToUpper(diaSemana[0]) + diaSemana.Substring(1);
                    
                    // Ícone baseado no dia da semana
                    string icone = "fa-calendar";
                    if (data.Date == DateTime.Today)
                    {
                        icone = "fa-star";
                    }
                    else if (data.Date == DateTime.Today.AddDays(1))
                    {
                        icone = "fa-clock";
                    }
                    
                    string radioHtml = string.Format(@"
                        <div class='radio-item-data'>
                            <input type='radio' id='radioData_{0}' name='dataRetirada' value='{1}' onchange='atualizarDataRetirada(this.value);' />
                            <label for='radioData_{0}'>
                                <i class='fas {2}'></i>
                                <span><strong>{3}</strong> - {4}</span>
                            </label>
                        </div>",
                        dataFormatada.Replace("-", ""),
                        dataFormatada,
                        icone,
                        dataExibicao,
                        diaSemanaCapitalizado
                    );
                    
                    htmlBuilder.Append(radioHtml);
                }
                
                // Atribuir todo o HTML de uma vez (muito mais rápido que +=)
                radioGroup.InnerHtml = htmlBuilder.ToString();
            }
        }

        private void ProcessarAdicionarAoCarrinho(string argument)
        {
            try
            {
                var partes = argument.Split('|');
                if (partes.Length >= 4)
                {
                    int produtoId = int.Parse(partes[0]);
                    string nome = partes[1];
                    string tamanho = partes[2];
                    
                    // Normalizar o preço: substituir vírgula por ponto se necessário
                    string precoStr = partes[3].Replace(",", ".").Trim();
                    decimal preco = decimal.Parse(precoStr, CultureInfo.InvariantCulture);
                    
                    int quantidade = partes.Length > 4 ? int.Parse(partes[4]) : 1;

                    var itemExistente = Carrinho.FirstOrDefault(i => i.ProdutoId == produtoId && i.Tamanho == tamanho);
                    if (itemExistente != null)
                    {
                        itemExistente.Quantidade += quantidade;
                        itemExistente.Subtotal = itemExistente.Quantidade * itemExistente.PrecoUnitario;
                    }
                    else
                    {
                        Carrinho.Add(new ItemPedido
                        {
                            ProdutoId = produtoId,
                            NomeProduto = nome,
                            Tamanho = tamanho,
                            Quantidade = quantidade,
                            PrecoUnitario = preco,
                            Subtotal = preco * quantidade
                        });
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private void ProcessarAtualizarQuantidade(string argument)
        {
            var partes = argument.Split('|');
            if (partes.Length == 3)
            {
                int produtoId = int.Parse(partes[0]);
                string tamanho = partes[1];
                int incremento = int.Parse(partes[2]);

                var item = Carrinho.FirstOrDefault(i => i.ProdutoId == produtoId && i.Tamanho == tamanho);
                if (item != null)
                {
                    item.Quantidade += incremento;
                    if (item.Quantidade < 1) item.Quantidade = 1;
                    item.Subtotal = item.Quantidade * item.PrecoUnitario;
                }
            }
        }

        private void ProcessarRemoverItem(string argument)
        {
            var partes = argument.Split('|');
            if (partes.Length == 2)
            {
                int produtoId = int.Parse(partes[0]);
                string tamanho = partes[1];

                var item = Carrinho.FirstOrDefault(i => i.ProdutoId == produtoId && i.Tamanho == tamanho);
                if (item != null)
                {
                    Carrinho.Remove(item);
                }
            }
        }

        private void ProcessarAdicionarSacoAoCarrinho(string argument)
        {
            try
            {
                var partes = argument.Split('|');
                if (partes.Length >= 5)
                {
                    int sacoId = int.Parse(partes[0]);
                    string nomeSaco = partes[1];
                    decimal precoSaco = decimal.Parse(partes[2].Replace(",", "."), CultureInfo.InvariantCulture);
                    int quantidadeSacos = int.Parse(partes[3]);
                    string produtosIdsStr = partes.Length > 4 ? partes[4] : "";
                    
                    var produtosIds = produtosIdsStr.Split(',').Select(id => int.Parse(id.Trim())).ToList();
                    
                    // Validar que todos os produtos são permitidos no saco
                    var produtoSaco = _databaseService.ObterTodosProdutos().FirstOrDefault(p => p.Id == sacoId);
                    if (produtoSaco != null && produtoSaco.EhSacoPromocional && !string.IsNullOrEmpty(produtoSaco.Produtos))
                    {
                        // Parsear JSON de IDs dos produtos permitidos
                        List<int> produtosPermitidosIds = new List<int>();
                        try
                        {
                            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                            produtosPermitidosIds = serializer.Deserialize<List<int>>(produtoSaco.Produtos);
                        }
                        catch
                        {
                            // Se falhar ao parsear, tentar como string separada por vírgula
                            if (!string.IsNullOrEmpty(produtoSaco.Produtos))
                            {
                                produtosPermitidosIds = produtoSaco.Produtos.Split(',').Select(id => int.Parse(id.Trim())).ToList();
                            }
                        }
                        
                        foreach (var produtoId in produtosIds)
                        {
                            if (!produtosPermitidosIds.Contains(produtoId))
                            {
                                throw new Exception($"O produto selecionado não é permitido neste saco. Por favor, selecione apenas produtos permitidos.");
                            }
                        }
                    }
                    
                    // Criar JSON de produtos no formato [{"qt": 1, "id": idProduto}, ...]
                    // A quantidade de cada produto dentro de UM saco é sempre 1
                    // A quantidade de sacos é controlada pelo campo Quantidade do ItemPedido
                    var produtosJson = produtosIds.Select(id => new { qt = 1, id = id }).ToList();
                    var jsonSerializer = new JavaScriptSerializer();
                    string produtosFormatados = jsonSerializer.Serialize(produtosJson);
                    
                    // Criar nome do saco sem incluir os produtos (eles estarão no campo Produtos)
                    string nomeSacoCompleto = nomeSaco;
                    
                    // Adicionar o saco promocional ao carrinho
                    // Verificar se já existe um saco com os MESMOS produtos (mesma chave)
                    // Ordenar os IDs dos produtos para comparar corretamente
                    var produtosIdsOrdenados = produtosIds.OrderBy(id => id).ToList();
                    string chaveProdutos = string.Join(",", produtosIdsOrdenados);
                    
                    var sacoItem = Carrinho.FirstOrDefault(i => 
                        i.ProdutoId == sacoId && 
                        i.NomeProduto == nomeSaco &&
                        !string.IsNullOrEmpty(i.Produtos));
                    
                    // Verificar se os produtos são os mesmos
                    bool produtosIguais = false;
                    if (sacoItem != null && !string.IsNullOrEmpty(sacoItem.Produtos))
                    {
                        try
                        {
                            var jsonSerializer2 = new JavaScriptSerializer();
                            var produtosExistentes = jsonSerializer2.Deserialize<List<Dictionary<string, object>>>(sacoItem.Produtos);
                            var idsExistentes = produtosExistentes.Select(p => Convert.ToInt32(p["id"])).OrderBy(id => id).ToList();
                            produtosIguais = idsExistentes.SequenceEqual(produtosIdsOrdenados);
                        }
                        catch
                        {
                            produtosIguais = false;
                        }
                    }
                    
                    if (sacoItem != null && produtosIguais)
                    {
                        // Se já existe um saco com os mesmos produtos, apenas aumentar a quantidade
                        // Os produtos permanecem os mesmos (não mesclar)
                        sacoItem.Quantidade += quantidadeSacos;
                        sacoItem.Subtotal = sacoItem.Quantidade * sacoItem.PrecoUnitario;
                        // Manter os produtos originais (não atualizar)
                    }
                    else
                    {
                        // Adicionar novo saco (produtos diferentes ou não existe)
                        Carrinho.Add(new ItemPedido
                        {
                            ProdutoId = sacoId,
                            NomeProduto = nomeSacoCompleto,
                            Tamanho = "", // Sacos promocionais não têm tamanho
                            Quantidade = quantidadeSacos,
                            PrecoUnitario = precoSaco,
                            Subtotal = precoSaco * quantidadeSacos,
                            Produtos = produtosFormatados
                        });
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private void AtualizarCarrinho()
        {
            if (Carrinho.Count == 0)
            {
                carrinhoContainer.InnerHtml = "<p class='text-muted'>Seu carrinho está vazio</p>";
                totalContainer.Style["display"] = "none";
                btnFazerReserva.Enabled = false;
                
                // Atualizar carrinho flutuante mobile e badge do header
                string scriptVazio = @"
                    var totalFlutuante = document.getElementById('totalFlutuante');
                    var qtdItensFlutuante = document.getElementById('qtdItensFlutuante');
                    var btnFazerReservaFlutuante = document.getElementById('btnFazerReservaFlutuante');
                    var carrinhoHeader = document.getElementById('carrinhoHeader');
                    var carrinhoBadge = document.getElementById('carrinhoBadge');
                    if (totalFlutuante) totalFlutuante.style.display = 'none';
                    if (qtdItensFlutuante) qtdItensFlutuante.textContent = '0';
                    if (btnFazerReservaFlutuante) btnFazerReservaFlutuante.disabled = true;
                    if (carrinhoHeader) carrinhoHeader.classList.remove('com-itens');
                    if (carrinhoBadge) {
                        carrinhoBadge.textContent = '0';
                        carrinhoBadge.classList.add('oculto');
                    }
                ";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "AtualizarCarrinhoFlutuanteVazio", scriptVazio, true);
                return;
            }

            // Usar StringBuilder para melhor performance
            var htmlBuilder = new System.Text.StringBuilder(Carrinho.Count * 300);
            decimal total = 0;
            int totalItens = 0;

            foreach (var item in Carrinho)
            {
                total += item.Subtotal;
                totalItens += item.Quantidade;
                
                string nomeEscapado = System.Web.HttpUtility.HtmlEncode(item.NomeProduto);
                string tamanhoEscapado = System.Web.HttpUtility.HtmlEncode(item.Tamanho);
                string tamanhoJsEscapado = System.Web.HttpUtility.JavaScriptStringEncode(item.Tamanho);
                
                // HTML para desktop e mobile (mesmo formato)
                string itemHtml = string.Format(@"
                    <div class='item-carrinho' data-item-id='{4}' data-item-tamanho='{5}'>
                        <div class='item-carrinho-header'>
                            <div class='item-carrinho-info'>
                                <div class='item-carrinho-nome'>{0}</div>
                                <div class='item-carrinho-detalhes'>{1}</div>
                            </div>
                            <div class='item-carrinho-acoes'>
                                <div class='item-carrinho-preco'>R$ {3}</div>
                            </div>
                        </div>
                        <div class='item-carrinho-controles'>
                            <div class='controle-quantidade'>
                                <button type='button' class='btn-quantidade-carrinho' onclick='diminuirQuantidadeCarrinho({4}, ""{6}"")' title='Diminuir quantidade'>
                                    <i class='fas fa-minus'></i>
                                </button>
                                <span class='quantidade-carrinho'>{2}</span>
                                <button type='button' class='btn-quantidade-carrinho' onclick='aumentarQuantidadeCarrinho({4}, ""{6}"")' title='Aumentar quantidade'>
                                    <i class='fas fa-plus'></i>
                                </button>
                            </div>
                            <div class='btn-acoes-item'>
                                <button type='button' class='btn-adicionar-mais' onclick='adicionarMaisItem({4}, ""{6}"")' title='Adicionar mais'>
                                    <i class='fas fa-plus-circle'></i> Mais
                                </button>
                                <button type='button' class='btn-remover-item' onclick='removerItem({4}, ""{6}"")' title='Remover item'>
                                    <i class='fas fa-trash'></i> Remover
                                </button>
                            </div>
                        </div>
                    </div>",
                    nomeEscapado,
                    tamanhoEscapado,
                    item.Quantidade,
                    item.Subtotal.ToString("F2"),
                    item.ProdutoId,
                    tamanhoEscapado,
                    tamanhoJsEscapado);
                
                htmlBuilder.Append(itemHtml);
            }
            
            string html = htmlBuilder.ToString();

            carrinhoContainer.InnerHtml = html;
            totalPedido.InnerText = total.ToString("F2");
            var totalPedidoFinal = FindControl("totalPedidoFinal") as System.Web.UI.HtmlControls.HtmlGenericControl;
            if (totalPedidoFinal != null)
            {
                totalPedidoFinal.InnerText = total.ToString("F2");
            }
            totalContainer.Style["display"] = "block";
            
            // Habilitar botão se houver itens
            if (Carrinho.Count > 0)
            {
                btnFazerReserva.Enabled = true;
                btnFazerReserva.Attributes["data-enabled"] = "true";
            }
            else
            {
                btnFazerReserva.Enabled = false;
            }
            
            // Atualizar carrinho flutuante mobile e badge do header
            string script = string.Format(@"
                var totalFlutuante = document.getElementById('totalFlutuante');
                var totalPedidoFlutuante = document.getElementById('totalPedidoFlutuante');
                var qtdItensFlutuante = document.getElementById('qtdItensFlutuante');
                var btnFazerReservaFlutuante = document.getElementById('btnFazerReservaFlutuante');
                var carrinhoHeader = document.getElementById('carrinhoHeader');
                var carrinhoBadge = document.getElementById('carrinhoBadge');
                
                if (totalFlutuante) {{
                    totalFlutuante.style.display = 'block';
                }}
                if (totalPedidoFlutuante) {{
                    totalPedidoFlutuante.textContent = '{0}';
                }}
                if (qtdItensFlutuante) {{
                    qtdItensFlutuante.textContent = '{1}';
                }}
                if (btnFazerReservaFlutuante) {{
                    btnFazerReservaFlutuante.disabled = false;
                }}
                if (carrinhoHeader) {{
                    carrinhoHeader.classList.add('com-itens');
                }}
                if (carrinhoBadge) {{
                    carrinhoBadge.textContent = '{1}';
                    carrinhoBadge.classList.remove('oculto');
                }}
            ",
            total.ToString("F2"),
            totalItens);
            Page.ClientScript.RegisterStartupScript(this.GetType(), "AtualizarCarrinhoFlutuante", script, true);
        }

        protected void btnFazerReserva_Click(object sender, EventArgs e)
        {
            // Validar que há itens no carrinho
            if (Carrinho.Count == 0)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CarrinhoVazio", 
                    $"alert('{EscapeJavaScript("Adicione produtos ao carrinho antes de fazer a reserva.")}');", true);
                return;
            }

            // Redirecionar para página de reserva
            Response.Redirect("paginas/Reserva.aspx");
        }

        // Método antigo removido - agora redireciona para página de Reserva.aspx
        // O código antigo foi movido para paginas/Reserva.aspx.cs

        // Método btnConfirmarReserva_Click removido - agora a reserva é feita em paginas/Reserva.aspx
        // O código foi movido para paginas/Reserva.aspx.cs

        private string HashSenha(string senha)
        {
            // Usar SHA256 para hash da senha (mesmo método usado no Login)
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(senha));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerificarSenha(string senha, string hashArmazenado)
        {
            string hashSenha = HashSenha(senha);
            return hashSenha == hashArmazenado;
        }

        [System.Web.Services.WebMethod]
        [System.Web.Script.Services.ScriptMethod]
        public static object VerificarClienteCadastrado(string login)
        {
            try
            {
                if (string.IsNullOrEmpty(login))
                {
                    return new { existe = false, temSenha = false, cliente = (object)null };
                }

                var databaseService = new DatabaseService();
                Cliente cliente = null;
                
                // Detectar se é email ou telefone
                string loginLimpo = login.Trim();
                bool isEmail = loginLimpo.Contains("@");
                
                if (isEmail)
                {
                    // É email
                    cliente = databaseService.ObterClientePorEmail(loginLimpo.ToLowerInvariant());
                }
                else
                {
                    // É telefone - remover caracteres não numéricos
                    string telefoneFormatado = System.Text.RegularExpressions.Regex.Replace(loginLimpo, @"[^\d]", "");
                    
                    if (telefoneFormatado.Length >= 10)
                    {
                        cliente = databaseService.ObterClientePorTelefone(telefoneFormatado);
                    }
                }
                
                if (cliente != null)
                {
                    // Retornar dados do cliente (sem senha)
                    return new 
                    { 
                        existe = true, 
                        temSenha = !string.IsNullOrEmpty(cliente.Senha),
                        cliente = new
                        {
                            id = cliente.Id,
                            nome = cliente.Nome,
                            email = cliente.Email ?? "",
                            telefone = cliente.Telefone ?? "",
                            isAdmin = cliente.IsAdmin
                        }
                    };
                }
                
                return new { existe = false, temSenha = false, cliente = (object)null };
            }
            catch
            {
                return new { existe = false, temSenha = false, cliente = (object)null };
            }
        }

        [System.Web.Services.WebMethod]
        [System.Web.Script.Services.ScriptMethod]
        public static object ValidarSenhaCliente(string login, string senha)
        {
            try
            {
                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(senha))
                {
                    return new { valido = false, mensagem = "Login e senha são obrigatórios" };
                }

                var databaseService = new DatabaseService();
                Cliente cliente = null;
                
                // Detectar se é email ou telefone
                string loginLimpo = login.Trim();
                bool isEmail = loginLimpo.Contains("@");
                
                if (isEmail)
                {
                    cliente = databaseService.ObterClientePorEmail(loginLimpo.ToLowerInvariant());
                }
                else
                {
                    string telefoneFormatado = System.Text.RegularExpressions.Regex.Replace(loginLimpo, @"[^\d]", "");
                    if (telefoneFormatado.Length >= 10)
                    {
                        cliente = databaseService.ObterClientePorTelefone(telefoneFormatado);
                    }
                }
                
                if (cliente == null)
                {
                    return new { valido = false, mensagem = "Cliente não encontrado" };
                }
                
                if (string.IsNullOrEmpty(cliente.Senha))
                {
                    return new { valido = false, mensagem = "Este cliente não possui senha cadastrada" };
                }
                
                // Validar senha
                string hashSenha = HashSenhaStatic(senha);
                if (hashSenha != cliente.Senha)
                {
                    return new { valido = false, mensagem = "Senha incorreta" };
                }
                
                // Senha válida - retornar dados do cliente
                return new 
                { 
                    valido = true,
                    cliente = new
                    {
                        id = cliente.Id,
                        nome = cliente.Nome,
                        email = cliente.Email ?? "",
                        telefone = cliente.Telefone ?? "",
                        isAdmin = cliente.IsAdmin
                    }
                };
            }
            catch (Exception ex)
            {
                return new { valido = false, mensagem = "Erro ao validar senha: " + ex.Message };
            }
        }

        private static string HashSenhaStatic(string senha)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(senha));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        [System.Web.Services.WebMethod]
        [System.Web.Script.Services.ScriptMethod]
        public static object FazerLoginSessao(int clienteId)
        {
            try
            {
                var databaseService = new DatabaseService();
                var cliente = databaseService.ObterClientePorId(clienteId);
                
                if (cliente == null)
                {
                    return new { sucesso = false, mensagem = "Cliente não encontrado" };
                }
                
                // Atualizar último acesso (isso também atualizará IsAdmin se necessário)
                cliente.UltimoAcesso = DateTime.Now;
                databaseService.CriarOuAtualizarCliente(cliente);
                
                // Buscar cliente atualizado para garantir que IsAdmin está correto
                cliente = databaseService.ObterClientePorId(cliente.Id);
                
                // Fazer login na sessão
                HttpContext.Current.Session["ClienteId"] = cliente.Id;
                HttpContext.Current.Session["ClienteNome"] = cliente.Nome;
                HttpContext.Current.Session["ClienteEmail"] = cliente.Email;
                HttpContext.Current.Session["IsAdmin"] = cliente.IsAdmin;
                if (!string.IsNullOrEmpty(cliente.Telefone))
                {
                    HttpContext.Current.Session["ClienteTelefone"] = cliente.Telefone;
                }
                HttpContext.Current.Session["SessionStartTime"] = DateTime.Now;
                
                // Registrar log de login
                string usuarioLog = LogService.ObterUsuarioAtual(HttpContext.Current.Session);
                LogService.RegistrarLogin(usuarioLog, "Default.aspx", $"Login via FazerLoginSessao - Email: {cliente.Email}");
                
                return new { sucesso = true, isAdmin = cliente.IsAdmin };
            }
            catch (Exception ex)
            {
                return new { sucesso = false, mensagem = "Erro ao fazer login: " + ex.Message };
            }
        }

        private void VerificarLogin()
        {
            try
            {
                // Verificar se a sessão expirou
                if (Session["ClienteId"] != null && Session.IsNewSession)
                {
                    // Sessão expirou - limpar dados de autenticação
                    Session.Remove("ClienteId");
                    Session.Remove("ClienteNome");
                    Session.Remove("ClienteEmail");
                    Session.Remove("ClienteTelefone");
                }
                
                // IMPORTANTE: Sempre renderizar os links no HTML, controlar visibilidade apenas via CSS
                // Isso garante que os elementos sempre estejam no DOM e possam ser manipulados via JavaScript
                if (clienteNome != null && linkLogin != null && linkMinhasReservas != null && linkMeusDados != null && linkLogout != null && linkAdmin != null)
                {
                    // Sempre renderizar todos os elementos
                    clienteNome.Visible = true;
                    linkLogin.Visible = true;
                    linkMinhasReservas.Visible = true;
                    linkMeusDados.Visible = true;
                    linkLogout.Visible = true;
                    linkAdmin.Visible = true;
                    
                    if (Session["ClienteId"] != null && !Session.IsNewSession)
                    {
                        // Usuário logado
                        string nomeCliente = Session["ClienteNome"] != null ? Session["ClienteNome"].ToString() : "";
                        clienteNome.InnerText = "Olá, " + System.Web.HttpUtility.HtmlEncode(nomeCliente);
                        clienteNome.Style["display"] = "block";
                        
                        // Ocultar link de login, mostrar outros
                        linkLogin.Style["display"] = "none";
                        linkMinhasReservas.Style["display"] = "inline";
                        linkMeusDados.Style["display"] = "inline";
                        linkLogout.Style["display"] = "inline";
                        
                        // Mostrar link de admin se for administrador
                        bool isAdmin = Session["IsAdmin"] != null && (bool)Session["IsAdmin"];
                        linkAdmin.Style["display"] = isAdmin ? "inline" : "none";
                    }
                    else
                    {
                        // Usuário não logado
                        clienteNome.InnerText = "";
                        clienteNome.Style["display"] = "none";
                        
                        // Mostrar apenas link de login
                        linkLogin.Style["display"] = "inline";
                        linkMinhasReservas.Style["display"] = "none";
                        linkMeusDados.Style["display"] = "none";
                        linkLogout.Style["display"] = "none";
                        linkAdmin.Style["display"] = "none";
                    }
                }
            }
            catch
            {
                // Erro ao verificar login - continuar execução
            }
        }
    }
}

