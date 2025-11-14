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
    public partial class Default : System.Web.UI.Page
    {
        private DatabaseService _databaseService;
        
        // Helper para escapar strings JavaScript com caracteres especiais
        private string EscapeJavaScript(string input)
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
                
                CarregarProdutos();
                CarregarDatasRetirada();
                AtualizarCarrinho(); // Atualizar carrinho na primeira carga
                
                // Se usuário acabou de fazer login, preencher dados e preparar para abrir modal
                // Validar entrada para prevenir XSS
                string abrirReservaParam = Request.QueryString["abrirReserva"];
                bool abrirReserva = !string.IsNullOrEmpty(abrirReservaParam) && 
                                    Security.InputValidator.SanitizeString(abrirReservaParam).ToLower() == "true";
                if (abrirReserva && estaLogado)
                {
                    // Preencher dados do cliente nos campos do modal
                    if (Session["ClienteNome"] != null)
                    {
                        string nomeCliente = Session["ClienteNome"].ToString();
                        txtNome.Text = nomeCliente;
                        if (hdnNome != null) hdnNome.Value = nomeCliente;
                    }
                    if (Session["ClienteEmail"] != null)
                    {
                        string emailCliente = Session["ClienteEmail"].ToString();
                        txtEmail.Text = emailCliente;
                        if (hdnEmail != null) hdnEmail.Value = emailCliente;
                    }
                    if (Session["ClienteTelefone"] != null)
                    {
                        string telefoneCliente = Session["ClienteTelefone"].ToString();
                        txtTelefone.Text = telefoneCliente;
                        if (hdnTelefone != null) hdnTelefone.Value = telefoneCliente.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
                    }
                    
                    // Configurar visibilidade dos elementos
                    divSenhaReserva.Visible = true;
                    divSenhaReserva.Style["display"] = "none";
                    divLoginDinamico.Visible = false;
                    divDadosReserva.Visible = true;
                    btnConfirmarReserva.Style["display"] = "inline-block";
                    
                    // Script para abrir modal automaticamente após carregar a página
                    string scriptAbrirModal = $@"
                        window.usuarioLogado = true;
                        setTimeout(function() {{
                            // Atualizar título do modal
                            var modalReservaLabel = document.getElementById('modalReservaLabel');
                            if (modalReservaLabel) modalReservaLabel.textContent = 'Finalizar Reserva';
                            
                            if (typeof DefaultPage !== 'undefined' && DefaultPage.ModalReserva) {{
                                DefaultPage.ModalReserva.abrir();
                            }} else if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Modal) {{
                                KingdomConfeitaria.Modal.show('modalReserva');
                            }}
                            
                            // Garantir que os dados estejam visíveis
                            var divLoginDinamico = document.getElementById('{divLoginDinamico.ClientID}');
                            var divDadosReserva = document.getElementById('{divDadosReserva.ClientID}');
                            var btnConfirmarReserva = document.getElementById('{btnConfirmarReserva.ClientID}');
                            if (divLoginDinamico) divLoginDinamico.style.display = 'none';
                            if (divDadosReserva) divDadosReserva.style.display = 'block';
                            if (btnConfirmarReserva) btnConfirmarReserva.style.display = 'inline-block';
                        }}, 300);
                    ";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "AbrirModalAposLogin", scriptAbrirModal, true);
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
                    // Não recarregar produtos - não é necessário após adicionar ao carrinho
                }
                else if (eventTarget == "AdicionarSacoAoCarrinho")
                {
                    ProcessarAdicionarSacoAoCarrinho(eventArgument);
                    AtualizarCarrinho();
                    // Não recarregar produtos - não é necessário após adicionar ao carrinho
                }
                else if (eventTarget == "AtualizarQuantidade")
                {
                    ProcessarAtualizarQuantidade(eventArgument);
                    AtualizarCarrinho();
                }
                else if (eventTarget == "RemoverItem")
                {
                    ProcessarRemoverItem(eventArgument);
                    AtualizarCarrinho();
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
                
                produtosContainer.InnerHtml = "";
                
                // Se não houver produtos, mostrar mensagem
                if (produtos.Count == 0)
                {
                    produtosContainer.InnerHtml = "<div class='alert alert-warning' style='margin: 20px;'><i class='fas fa-exclamation-triangle'></i> Nenhum produto cadastrado no momento. Por favor, cadastre produtos na área administrativa.</div>";
                    return;
                }

                foreach (var produto in produtos)
                {
                    string nomeEscapado = produto.Nome.Replace("\"", "\\\"").Replace("'", "\\'");
                    string descricaoEscapada = (!string.IsNullOrEmpty(produto.Descricao) ? produto.Descricao : "").Replace("\"", "\\\"").Replace("'", "\\'");
                    string precoStr = produto.Preco.ToString("F2").Replace(",", ".");
                    
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
                    
                    // Escapar JSON para uso em data attribute (mais seguro que onclick)
                    string produtoJsonEscapado = System.Web.HttpUtility.HtmlAttributeEncode(produtoJson);
                    
                    // HTML do card do carrossel - imagem abre modal, botão adiciona ao carrinho
                    string nomeJsEscapado = System.Web.HttpUtility.JavaScriptStringEncode(produto.Nome);
                    // Se não houver tamanho, usar "Único" como padrão
                    string tamanhoFinal = string.IsNullOrEmpty(tamanho) ? "Único" : tamanho;
                    string tamanhoJsEscapado = System.Web.HttpUtility.JavaScriptStringEncode(tamanhoFinal);
                    string precoJsEscapado = System.Web.HttpUtility.JavaScriptStringEncode(precoStr);
                    
                    string html = string.Format(@"
                        <div class='produto-card-carrossel' data-produto-id='{0}' data-produto-json='{9}'>
                            <div class='produto-imagem-carrossel-wrapper' onclick='abrirModalProdutoFromCard(this.closest("".produto-card-carrossel""))'>
                                <img src='{5}' alt='{1}' class='produto-imagem-carrossel' loading='lazy' decoding='async' onerror='this.onerror=null; if(this.src !== ""{6}"") {{ this.src=""{6}""; }}' />
                            </div>
                            <div class='produto-nome-carrossel'>{1}</div>
                            <div class='produto-preco-carrossel'>R$ {7}</div>
                            <button type='button' class='btn-reservar-produto' onclick='event.stopPropagation(); reservarProdutoRapido({0}, ""{10}"", ""{11}"", ""{12}"", this);' title='Adicionar ao carrinho'>
                                <i class='fas fa-shopping-cart'></i> Reservar
                            </button>
                        </div>",
                        produto.Id,
                        System.Web.HttpUtility.HtmlEncode(produto.Nome),
                        System.Web.HttpUtility.HtmlEncode(produto.Descricao ?? ""),
                        precoStr,
                        tamanhoFinal,
                        imagemSrc,
                        placeholderSvg,
                        produto.Preco.ToString("F2"),
                        produtoJson,
                        produtoJsonEscapado,
                        nomeJsEscapado,
                        tamanhoJsEscapado,
                        precoJsEscapado);
                    produtosContainer.InnerHtml += html;
                }
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
                radioGroup.InnerHtml = "";
                
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
                    
                    radioGroup.InnerHtml += radioHtml;
                }
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
                    
                    // Criar JSON de produtos no formato [{"qt": quantidade, "id": idProduto}, ...]
                    var produtosJson = produtosIds.Select(id => new { qt = quantidadeSacos, id = id }).ToList();
                    var jsonSerializer = new JavaScriptSerializer();
                    string produtosFormatados = jsonSerializer.Serialize(produtosJson);
                    
                    // Criar nome do saco sem incluir os produtos (eles estarão no campo Produtos)
                    string nomeSacoCompleto = nomeSaco;
                    
                    // Adicionar o saco promocional ao carrinho
                    // Usar uma chave única baseada no sacoId + produtos selecionados
                    string chaveSaco = sacoId + "|" + produtosIdsStr;
                    var sacoItem = Carrinho.FirstOrDefault(i => 
                        i.ProdutoId == sacoId && 
                        i.NomeProduto == nomeSaco);
                    
                    if (sacoItem != null)
                    {
                        // Se já existe um saco similar, aumentar a quantidade
                        sacoItem.Quantidade += quantidadeSacos;
                        sacoItem.Subtotal = sacoItem.Quantidade * sacoItem.PrecoUnitario;
                        // Atualizar produtos (mesclar JSONs)
                        if (!string.IsNullOrEmpty(sacoItem.Produtos))
                        {
                            try
                            {
                                var jsonSerializer2 = new JavaScriptSerializer();
                                var produtosExistentes = jsonSerializer2.Deserialize<List<Dictionary<string, object>>>(sacoItem.Produtos);
                                var produtosNovos = jsonSerializer2.Deserialize<List<Dictionary<string, object>>>(produtosFormatados);
                                produtosExistentes.AddRange(produtosNovos);
                                sacoItem.Produtos = jsonSerializer2.Serialize(produtosExistentes);
                            }
                            catch
                            {
                                // Se falhar ao parsear, usar o novo
                                sacoItem.Produtos = produtosFormatados;
                            }
                        }
                        else
                        {
                            sacoItem.Produtos = produtosFormatados;
                        }
                    }
                    else
                    {
                        // Adicionar novo saco
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

            string html = "";
            string htmlMobile = "";
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
                
                html += itemHtml;
                htmlMobile += itemHtml;
            }

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
                var modalCarrinhoBody = document.getElementById('modalCarrinhoBody');
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
                if (modalCarrinhoBody) {{
                    modalCarrinhoBody.innerHTML = `{2}`;
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
            totalItens,
            htmlMobile.Replace("`", "\\`").Replace("$", "\\$"));
            Page.ClientScript.RegisterStartupScript(this.GetType(), "AtualizarCarrinhoFlutuante", script, true);
        }

        protected void btnFazerReserva_Click(object sender, EventArgs e)
        {
            // Permitir que qualquer um abra o modal para montar a reserva
            // A validação de login será feita apenas ao confirmar a reserva
            
            if (Carrinho.Count == 0)
            {
                // Usar data attribute para mostrar alerta
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CarrinhoVazio", 
                    $"alert('{EscapeJavaScript("Adicione produtos ao carrinho antes de fazer a reserva.")}');", true);
                return;
            }

            // Preencher campos do modal se cliente estiver logado
            bool estaLogado = Session["ClienteId"] != null && !Session.IsNewSession;
            
            if (estaLogado)
            {
                if (Session["ClienteNome"] != null)
                {
                    string nomeCliente = Session["ClienteNome"].ToString();
                    txtNome.Text = nomeCliente;
                    if (hdnNome != null) hdnNome.Value = nomeCliente;
                }
                if (Session["ClienteEmail"] != null)
                {
                    string emailCliente = Session["ClienteEmail"].ToString();
                    txtEmail.Text = emailCliente;
                    if (hdnEmail != null) hdnEmail.Value = emailCliente;
                }
                if (Session["ClienteTelefone"] != null)
                {
                    string telefoneCliente = Session["ClienteTelefone"].ToString();
                    txtTelefone.Text = telefoneCliente;
                    if (hdnTelefone != null) hdnTelefone.Value = telefoneCliente.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
                }
                
                // Ocultar campo de senha e área de login se estiver logado
                // IMPORTANTE: NÃO usar Visible = false, usar apenas CSS para manter elemento no DOM
                divSenhaReserva.Visible = true; // SEMPRE renderizar no HTML
                divSenhaReserva.Style["display"] = "none"; // Ocultar via CSS
                divLoginDinamico.Visible = false;
                divDadosReserva.Visible = true;
                btnConfirmarReserva.Style["display"] = "inline-block";
                
                // Adicionar script para indicar que está logado e mostrar área de reserva
                string script = $@"
                    window.usuarioLogado = true;
                    var modalReservaLabel = document.getElementById('modalReservaLabel');
                    if (modalReservaLabel) modalReservaLabel.textContent = 'Finalizar Reserva';
                    var divLoginDinamico = document.getElementById('{divLoginDinamico.ClientID}');
                    var divDadosReserva = document.getElementById('{divDadosReserva.ClientID}');
                    if (!divDadosReserva) divDadosReserva = document.querySelector('[id*=""divDadosReserva""]');
                    var btnConfirmarReserva = document.getElementById('{btnConfirmarReserva.ClientID}');
                    if (!btnConfirmarReserva) btnConfirmarReserva = document.querySelector('[id*=""btnConfirmarReserva""]');
                    var divBotoesReserva = document.getElementById('divBotoesReserva');
                    
                    if (divLoginDinamico) {{
                        divLoginDinamico.style.display = 'none';
                        divLoginDinamico.style.visibility = 'hidden';
                    }}
                    if (divDadosReserva) {{
                        divDadosReserva.style.display = 'block';
                        divDadosReserva.style.visibility = 'visible';
                        divDadosReserva.style.opacity = '1';
                        divDadosReserva.removeAttribute('hidden');
                        divDadosReserva.classList.remove('d-none');
                        divDadosReserva.classList.add('d-block');
                    }}
                    if (btnConfirmarReserva) {{
                        btnConfirmarReserva.style.display = 'inline-block';
                        btnConfirmarReserva.style.visibility = 'visible';
                    }}
                    if (divBotoesReserva) {{
                        divBotoesReserva.style.display = 'flex';
                        divBotoesReserva.style.visibility = 'visible';
                    }}
                ";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "UsuarioLogado", script, true);
            }
            else
            {
                // Limpar campos se não estiver logado
                txtNome.Text = "";
                txtEmail.Text = "";
                txtTelefone.Text = "";
                txtSenhaReserva.Text = "";
                
                // Mostrar área de login e ocultar área de reserva se não estiver logado
                // IMPORTANTE: NÃO usar Visible = false, pois isso impede a renderização no HTML
                // Usar apenas CSS para ocultar, assim o elemento sempre estará no DOM
                divLoginDinamico.Visible = true;
                divDadosReserva.Visible = true; // SEMPRE renderizar no HTML, controlar visibilidade via CSS/JS
                divDadosReserva.Style["display"] = "none"; // Ocultar via CSS, não via Visible
                divSenhaReserva.Visible = true; // SEMPRE renderizar no HTML, controlar visibilidade via CSS/JS
                divSenhaReserva.Style["display"] = "none"; // Ocultar via CSS, não via Visible
                btnConfirmarReserva.Style["display"] = "none";
                
                // Adicionar script para indicar que não está logado
                string scriptNaoLogado = $@"
                    window.usuarioLogado = false;
                    var modalReservaLabel = document.getElementById('modalReservaLabel');
                    if (modalReservaLabel) modalReservaLabel.textContent = 'Login';
                    var divLoginDinamico = document.getElementById('{divLoginDinamico.ClientID}');
                    var divDadosReserva = document.getElementById('{divDadosReserva.ClientID}');
                    var btnConfirmarReserva = document.getElementById('{btnConfirmarReserva.ClientID}');
                    if (divLoginDinamico) divLoginDinamico.style.display = 'block';
                    if (divDadosReserva) divDadosReserva.style.display = 'none';
                    if (btnConfirmarReserva) btnConfirmarReserva.style.display = 'none';
                ";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "UsuarioNaoLogado", scriptNaoLogado, true);
            }

            // Limpar observações
            txtObservacoes.Text = "";

            // Abrir modal sempre que o botão "Fazer Reserva" for clicado
            // O método btnFazerReserva_Click só é chamado quando o botão é clicado, então sempre abrir o modal aqui
            string scriptAbrirModal = @"
                setTimeout(function() {
                    if (typeof DefaultPage !== 'undefined' && DefaultPage.ModalReserva) {
                        DefaultPage.ModalReserva.abrir();
                    } else if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Modal) {
                        KingdomConfeitaria.Modal.show('modalReserva');
                    } else {
                        // Fallback: usar Bootstrap diretamente
                        var modalElement = document.getElementById('modalReserva');
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
                }, 100);";
            
            Page.ClientScript.RegisterStartupScript(this.GetType(), "AbrirModal_" + DateTime.Now.Ticks, scriptAbrirModal, true);
        }

        protected void btnConfirmarReserva_Click(object sender, EventArgs e)
        {
            if (Carrinho.Count == 0)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CarrinhoVazio", 
                    $"alert('{EscapeJavaScript("Adicione produtos ao carrinho antes de fazer a reserva.")}');", true);
                return;
            }

            // Validar campos obrigatórios
            // Como os campos são ReadOnly, usar valores dos HiddenFields se os campos estiverem vazios
            string nome = !string.IsNullOrWhiteSpace(txtNome.Text) ? txtNome.Text.Trim() : (hdnNome != null ? hdnNome.Value : "");
            string email = !string.IsNullOrWhiteSpace(txtEmail.Text) ? txtEmail.Text.Trim() : (hdnEmail != null ? hdnEmail.Value : "");
            string telefone = !string.IsNullOrWhiteSpace(txtTelefone.Text) ? txtTelefone.Text.Trim() : (hdnTelefone != null ? hdnTelefone.Value : "");
            
            // Se ainda estiver vazio, tentar obter da sessão (caso o usuário esteja logado)
            if (string.IsNullOrWhiteSpace(nome) && Session["ClienteNome"] != null)
            {
                nome = Session["ClienteNome"].ToString();
            }
            if (string.IsNullOrWhiteSpace(email) && Session["ClienteEmail"] != null)
            {
                email = Session["ClienteEmail"].ToString();
            }
            if (string.IsNullOrWhiteSpace(telefone) && Session["ClienteTelefone"] != null)
            {
                telefone = Session["ClienteTelefone"].ToString();
            }
            
            if (string.IsNullOrWhiteSpace(nome))
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "NomeVazio", 
                    $"alert('{EscapeJavaScript("Por favor, preencha o nome.")}');", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "EmailInvalido", 
                    $"alert('{EscapeJavaScript("Por favor, preencha um email válido.")}');", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(telefone))
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "TelefoneVazio", 
                    $"alert('{EscapeJavaScript("Por favor, preencha o telefone.")}');", true);
                return;
            }

            // Obter data selecionada do radiobutton ou hidden field
            var hdnDataRetiradaControl = FindControl("hdnDataRetirada") as HiddenField;
            string dataRetiradaSelecionada = hdnDataRetiradaControl != null ? hdnDataRetiradaControl.Value : "";
            if (string.IsNullOrEmpty(dataRetiradaSelecionada))
            {
                // Tentar obter do formulário
                dataRetiradaSelecionada = Request.Form["dataRetirada"];
            }
            
            if (string.IsNullOrEmpty(dataRetiradaSelecionada))
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "DataVazia", 
                    $"alert('{EscapeJavaScript("Por favor, selecione uma data de retirada.")}');", true);
                return;
            }

            // Formatar email e telefone antes de processar (usar valores já obtidos acima)
            string emailFormatado = email.Trim().ToLowerInvariant();
            string telefoneFormatado = System.Text.RegularExpressions.Regex.Replace(telefone, @"[^\d]", "");

            // Verificar se cliente está logado, se não estiver, fazer login automático
            int clienteId = 0;
            Cliente cliente = null;
            
            if (Session["ClienteId"] != null && !Session.IsNewSession)
            {
                // Cliente já está logado
                try
                {
                    clienteId = (int)Session["ClienteId"];
                    if (clienteId > 0)
                    {
                        cliente = _databaseService.ObterClientePorId(clienteId);
                    }
                }
                catch
                {
                    // Sessão inválida, continuar para fazer login automático
                    clienteId = 0;
                    cliente = null;
                }
            }
            
            // Se não estiver logado ou cliente não encontrado, fazer login automático
            if (clienteId == 0 || cliente == null)
            {
                try
                {
                    // Garantir que DatabaseService está inicializado
                    if (_databaseService == null)
                    {
                        _databaseService = new DatabaseService();
                    }
                    
                    // Buscar cliente por email ou telefone
                    cliente = _databaseService.ObterClientePorEmail(emailFormatado);
                    
                    if (cliente == null && !string.IsNullOrEmpty(telefoneFormatado))
                    {
                        cliente = _databaseService.ObterClientePorTelefone(telefoneFormatado);
                    }
                    
                    if (cliente != null)
                    {
                        // Cliente existe - verificar senha
                        string senhaInformada = txtSenhaReserva != null ? txtSenhaReserva.Text : "";
                        
                        // Se o cliente tem senha cadastrada, validar
                        if (!string.IsNullOrEmpty(cliente.Senha))
                        {
                            if (string.IsNullOrEmpty(senhaInformada))
                            {
                                Page.ClientScript.RegisterStartupScript(this.GetType(), "SenhaObrigatoria", 
                                    $"alert('{EscapeJavaScript("Este email/telefone já está cadastrado. Por favor, informe sua senha para continuar.")}');", true);
                                return;
                            }
                            
                            // Verificar senha
                            if (!VerificarSenha(senhaInformada, cliente.Senha))
                            {
                                Page.ClientScript.RegisterStartupScript(this.GetType(), "SenhaIncorreta", 
                                    $"alert('{EscapeJavaScript("Senha incorreta. Por favor, tente novamente.")}');", true);
                                return;
                            }
                        }
                        else
                        {
                            // Cliente não tem senha cadastrada (cadastro antigo) - permitir continuar sem senha
                            // Mas avisar que seria bom cadastrar uma senha
                        }
                        
                        // Senha válida ou cliente sem senha - fazer login
                        clienteId = cliente.Id;
                        
                        // Atualizar último acesso (isso também atualizará IsAdmin se necessário)
                        cliente.UltimoAcesso = DateTime.Now;
                        _databaseService.CriarOuAtualizarCliente(cliente);
                        
                        // Buscar cliente atualizado para garantir que IsAdmin está correto
                        cliente = _databaseService.ObterClientePorId(cliente.Id);
                        
                        // Validar se o cliente foi encontrado
                        if (cliente == null)
                        {
                            throw new Exception("Falha ao buscar cliente após atualização. ID: " + clienteId);
                        }
                        
                        Session["ClienteId"] = cliente.Id;
                        Session["ClienteNome"] = cliente.Nome;
                        Session["ClienteEmail"] = cliente.Email;
                        Session["IsAdmin"] = cliente.IsAdmin;
                        if (!string.IsNullOrEmpty(cliente.Telefone))
                        {
                            Session["ClienteTelefone"] = cliente.Telefone;
                        }
                        Session["SessionStartTime"] = DateTime.Now;
                        
                        // Registrar log de login
                        string usuarioLog = LogService.ObterUsuarioAtual(Session);
                        LogService.RegistrarLogin(usuarioLog, "Default.aspx", $"Login automático durante reserva - Email: {cliente.Email}");
                        
                        // Atualizar links do header após login
                        VerificarLogin();
                    }
                    else
                    {
                        // Cliente não existe - criar novo cliente e fazer login
                        // Mas antes, tentar buscar novamente (pode ter sido criado em outra thread)
                        cliente = _databaseService.ObterClientePorEmail(emailFormatado);
                        if (cliente == null && !string.IsNullOrEmpty(telefoneFormatado))
                        {
                            cliente = _databaseService.ObterClientePorTelefone(telefoneFormatado);
                        }
                        
                        if (cliente != null)
                        {
                            // Cliente foi encontrado na segunda tentativa - fazer login
                            clienteId = cliente.Id;
                            cliente.UltimoAcesso = DateTime.Now;
                            _databaseService.CriarOuAtualizarCliente(cliente);
                            
                            // Buscar cliente atualizado
                            cliente = _databaseService.ObterClientePorId(cliente.Id);
                            if (cliente == null)
                            {
                                throw new Exception("Falha ao buscar cliente após atualização. ID: " + clienteId);
                            }
                            
                            Session["ClienteId"] = cliente.Id;
                            Session["ClienteNome"] = cliente.Nome;
                            Session["ClienteEmail"] = cliente.Email;
                            Session["IsAdmin"] = cliente.IsAdmin;
                            if (!string.IsNullOrEmpty(cliente.Telefone))
                            {
                                Session["ClienteTelefone"] = cliente.Telefone;
                            }
                            Session["SessionStartTime"] = DateTime.Now;
                            
                            string usuarioLog = LogService.ObterUsuarioAtual(Session);
                            LogService.RegistrarLogin(usuarioLog, "Default.aspx", $"Login automático durante reserva (cliente encontrado na segunda tentativa) - Email: {cliente.Email}");
                            
                            VerificarLogin();
                        }
                        else
                        {
                            // Cliente realmente não existe - criar novo
                            cliente = new Cliente
                            {
                                Nome = nome.Trim(),
                                Email = emailFormatado,
                                Telefone = telefoneFormatado,
                                TemWhatsApp = !string.IsNullOrEmpty(telefoneFormatado),
                                EmailConfirmado = false,
                                WhatsAppConfirmado = false,
                                IsAdmin = false, // Será definido automaticamente pelo CriarOuAtualizarCliente se o email for de administrador
                                DataCadastro = DateTime.Now
                            };
                            
                            clienteId = _databaseService.CriarOuAtualizarCliente(cliente);
                            
                            // Validar se o clienteId foi retornado corretamente
                            if (clienteId <= 0)
                            {
                                throw new Exception("Falha ao criar cliente. ID retornado foi inválido: " + clienteId);
                            }
                            
                            // Registrar log de cadastro
                            LogService.RegistrarInsercao(
                                $"Cliente ID: {clienteId}",
                                "Cliente",
                                "Default.aspx",
                                $"Cadastro automático durante reserva - Nome: {cliente.Nome}, Email: {cliente.Email}, Telefone: {cliente.Telefone ?? "N/A"}"
                            );
                            
                            // Buscar cliente atualizado para obter IsAdmin correto
                            cliente = _databaseService.ObterClientePorId(clienteId);
                            
                            // Validar se o cliente foi encontrado
                            if (cliente == null)
                            {
                                throw new Exception("Falha ao buscar cliente após criação. ID: " + clienteId);
                            }
                            
                            // Fazer login automático
                            Session["ClienteId"] = clienteId;
                            Session["ClienteNome"] = cliente.Nome;
                            Session["ClienteEmail"] = cliente.Email;
                            Session["IsAdmin"] = cliente.IsAdmin;
                            if (!string.IsNullOrEmpty(cliente.Telefone))
                            {
                                Session["ClienteTelefone"] = cliente.Telefone;
                            }
                            Session["SessionStartTime"] = DateTime.Now;
                            
                            // Registrar log de login após cadastro
                            string usuarioLogCadastro = LogService.ObterUsuarioAtual(Session);
                            LogService.RegistrarLogin(usuarioLogCadastro, "Default.aspx", $"Login automático após cadastro durante reserva - Email: {cliente.Email}");
                            
                            // Atualizar links do header após login automático
                            VerificarLogin();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Logar o erro para debug
                    try
                    {
                        string detalhesErro = ex != null ? ex.Message : "Erro desconhecido";
                        if (ex != null && ex.InnerException != null)
                        {
                            detalhesErro += " | InnerException: " + ex.InnerException.Message;
                        }
                        LogService.Registrar("ERROR", "Sistema", "Login Automático", "Default.aspx", $"Erro: {detalhesErro} | StackTrace: {ex?.StackTrace ?? "N/A"}");
                    }
                    catch
                    {
                        // Se falhar ao logar, continuar
                    }
                    
                    // Mostrar mensagem de erro mais informativa
                    string mensagemErro = "Erro ao processar login automático. Por favor, tente novamente.";
                    
                    // Verificar se é um erro específico conhecido
                    if (ex != null && !string.IsNullOrEmpty(ex.Message))
                    {
                        // Se for erro de email/telefone já cadastrado, mostrar mensagem mais amigável
                        if (ex.Message.Contains("já está cadastrado") || ex.Message.Contains("já existe"))
                        {
                            mensagemErro = ex.Message + " Por favor, faça login com suas credenciais.";
                        }
                        else
                        {
                            // Em desenvolvimento, pode mostrar mais detalhes
                            #if DEBUG
                            mensagemErro += " Detalhes: " + ex.Message;
                            #endif
                        }
                    }
                    
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "ErroLogin", 
                        $"alert('{EscapeJavaScript(mensagemErro)}');", true);
                    return;
                }
            }
            
            // Validar que ClienteId é válido
            if (clienteId <= 0)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "ClienteIdInvalido", 
                    $"alert('{EscapeJavaScript("Erro: Não foi possível identificar o cliente. Por favor, tente novamente.")}');", true);
                return;
            }

            try
            {

                // Validar produtos antes de criar a reserva
                if (Carrinho.Count == 0)
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "CarrinhoVazio", 
                        $"alert('{EscapeJavaScript("O carrinho está vazio. Por favor, adicione produtos antes de fazer a reserva.")}');", true);
                    return;
                }
                
                var todosProdutos = _databaseService.ObterTodosProdutos();
                var todosProdutosIds = todosProdutos.Select(p => p.Id).ToHashSet();
                var produtosAtivosIds = todosProdutos.Where(p => p.Ativo).Select(p => p.Id).ToHashSet();
                
                // Validar itens do carrinho
                var itensValidos = new List<ItemPedido>();
                var itensInvalidos = new List<ItemPedido>();
                
                foreach (var item in Carrinho)
                {
                    bool produtoExiste = todosProdutosIds.Contains(item.ProdutoId);
                    
                    // Aceitar item se o produto existe no banco (mesmo que inativo)
                    // Isso permite salvar reservas de produtos que foram desativados após serem adicionados ao carrinho
                    if (produtoExiste)
                    {
                        itensValidos.Add(item);
                    }
                    else
                    {
                        itensInvalidos.Add(item);
                    }
                }
                
                if (itensValidos.Count == 0)
                {
                    // Criar mensagem mais informativa
                    string mensagemErro = "Nenhum item válido no carrinho. ";
                    if (todosProdutos.Count == 0)
                    {
                        mensagemErro += "Não há produtos cadastrados no sistema. Por favor, cadastre produtos primeiro.";
                    }
                    else
                    {
                        mensagemErro += $"Os produtos no carrinho (IDs: {string.Join(", ", itensInvalidos.Select(i => i.ProdutoId))}) não foram encontrados no banco de dados. ";
                        mensagemErro += "Por favor, adicione produtos novamente.";
                    }
                    
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "SemItensValidos", 
                        $"alert('{EscapeJavaScript(mensagemErro)}');", true);
                    // Limpar carrinho se todos os itens forem inválidos
                    Carrinho.Clear();
                    AtualizarCarrinho();
                    return;
                }
                
                // Se houver itens inválidos, remover do carrinho mas continuar com os válidos
                if (itensInvalidos.Count > 0)
                {
                    foreach (var itemInvalido in itensInvalidos)
                    {
                        Carrinho.Remove(itemInvalido);
                    }
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "ItensRemovidos", 
                        $"alert('{EscapeJavaScript("Alguns produtos não estão mais disponíveis e foram removidos do carrinho. Continuando com os produtos válidos.")}');", true);
                }

                // Validar que há itens válidos para gravar
                if (itensValidos == null || itensValidos.Count == 0)
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "SemItens", 
                        $"alert('{EscapeJavaScript("Erro: Não há itens válidos para gravar na reserva.")}');", true);
                    return;
                }

                // Obter o StatusId para "Aberta" (ID = 1)
                var statusAberta = _databaseService.ObterStatusReservaPorNome("Aberta");
                int statusIdAberta = statusAberta != null ? statusAberta.Id : 1; // Se não encontrar, usar ID 1 como padrão
                
                var reserva = new Reserva
                {
                    // Nome, Email e Telefone não são mais armazenados na tabela Reservas
                    // Eles são obtidos via JOIN com a tabela Clientes quando a reserva é lida
                    DataRetirada = DateTime.Parse(ddlDataRetirada.SelectedValue),
                    DataReserva = DateTime.Now,
                    StatusId = statusIdAberta, // Definir StatusId diretamente como 1 (Aberta)
                    Status = "Aberta", // Para exibição/compatibilidade
                    ValorTotal = itensValidos.Sum(i => i.Subtotal),
                    Itens = itensValidos, // Usar apenas itens válidos
                    Observacoes = txtObservacoes.Text,
                    ConvertidoEmPedido = false,
                    PrevisaoEntrega = null,
                    Cancelado = false,
                    ClienteId = clienteId // Garantir que ClienteId está definido
                };

                // Validar que ClienteId está definido antes de salvar
                if (!reserva.ClienteId.HasValue || reserva.ClienteId.Value <= 0)
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "ClienteIdNaoDefinido", 
                        $"alert('{EscapeJavaScript("Erro: ID do cliente não está definido. Por favor, faça login novamente.")}'); window.location.href='Login.aspx';", true);
                    return;
                }

                // Salvar no banco de dados
                try
                {
                    _databaseService.SalvarReserva(reserva);
                    
                    // Verificar se a reserva foi realmente gravada
                    if (reserva.Id <= 0)
                    {
                        throw new Exception("A reserva não foi gravada. O ID não foi retornado.");
                    }
                }
                catch (Exception exSalvar)
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "ErroSalvarReserva", 
                        $"alert('{EscapeJavaScript("Erro ao salvar reserva: " + exSalvar.Message)}');", true);
                    return;
                }

                // Buscar dados do cliente e status da reserva para preencher antes de enviar email
                if (reserva.ClienteId.HasValue)
                {
                    try
                    {
                        var clienteData = _databaseService.ObterClientePorId(reserva.ClienteId.Value);
                        if (clienteData != null)
                        {
                            reserva.Nome = clienteData.Nome;
                            reserva.Email = clienteData.Email;
                            reserva.Telefone = clienteData.Telefone;
                        }
                    }
                    catch
                    {
                        // Se não conseguir buscar o cliente, continuar sem os dados
                    }
                }
                
                // Buscar status da reserva se não estiver preenchido
                if (reserva.StatusId.HasValue && string.IsNullOrEmpty(reserva.Status))
                {
                    try
                    {
                        var statusReserva = _databaseService.ObterStatusReservaPorId(reserva.StatusId.Value);
                        if (statusReserva != null)
                        {
                            reserva.Status = statusReserva.Nome;
                        }
                    }
                    catch
                    {
                        // Se não conseguir buscar o status, usar "Aberta" como padrão
                        reserva.Status = "Aberta";
                    }
                }
                
                // Se ainda não tiver status, usar "Aberta" como padrão
                if (string.IsNullOrEmpty(reserva.Status))
                {
                    reserva.Status = "Aberta";
                }
                
                // Enviar emails e WhatsApp de forma assíncrona (não bloquear a resposta)
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        var emailService = new EmailService();
                        emailService.EnviarConfirmacaoReserva(reserva);
                    }
                    catch (Exception exEmail)
                    {
                        // Log do erro (em produção, usar um sistema de log adequado)
                        System.Diagnostics.Debug.WriteLine("Erro ao enviar email de confirmação: " + exEmail.Message);
                        // Não bloquear o processo
                    }

                });

                // Limpar carrinho
                Carrinho.Clear();

                // Atualizar menu após reserva
                VerificarLogin();
                
                // Redirecionar diretamente para Minhas Reservas
                Response.Redirect("MinhasReservas.aspx", false);
            }
            catch (Exception ex)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Erro", 
                    string.Format("alert('Erro ao processar reserva: {0}');", EscapeJavaScript(ex.Message)), true);
            }
        }

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

