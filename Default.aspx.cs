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
            
            _databaseService = new DatabaseService();

            // Verificar status de login e definir variável JavaScript
            bool estaLogado = Session["ClienteId"] != null && !Session.IsNewSession;
            Page.ClientScript.RegisterStartupScript(this.GetType(), "DefinirStatusLogin", 
                $"window.usuarioLogado = {estaLogado.ToString().ToLower()};", true);

            // Sempre verificar login para atualizar links do header
            VerificarLogin();
            
            if (!IsPostBack)
            {
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
                if (eventTarget == "AdicionarAoCarrinho")
                {
                    ProcessarAdicionarAoCarrinho(eventArgument);
                    AtualizarCarrinho();
                    CarregarProdutos(); // Recarregar para manter estado
                }
                else if (eventTarget == "AdicionarSacoAoCarrinho")
                {
                    ProcessarAdicionarSacoAoCarrinho(eventArgument);
                    AtualizarCarrinho();
                    CarregarProdutos(); // Recarregar para manter estado
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
                var produtos = _databaseService.ObterProdutos();
                
                produtosContainer.InnerHtml = "";
                
                // Se não houver produtos, mostrar mensagem
                if (produtos.Count == 0)
                {
                    produtosContainer.InnerHtml = "<div class='alert alert-warning'><i class='fas fa-exclamation-triangle'></i> Nenhum produto cadastrado no momento. Por favor, cadastre produtos na área administrativa.</div>";
                    return;
                }

                foreach (var produto in produtos)
                {
                    string nomeEscapado = produto.Nome.Replace("\"", "\\\"").Replace("'", "\\'");
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
                    
                    // Campo hidden com tamanho (extraído do nome)
                    string tamanhosHtml = string.Format(@"<input type='hidden' id='tamanho_{0}' value='{1}' data-preco='{2}' />", produto.Id, tamanho, precoStr);

                    // Se for saco promocional, mostrar seletor de produtos
                    string seletorProdutosHtml = "";
                    string onclickSaco = "";
                    if (produto.EhSacoPromocional && produto.QuantidadeSaco > 0 && !string.IsNullOrEmpty(produto.Produtos))
                    {
                        // Parsear JSON de IDs dos produtos permitidos
                        List<int> produtosIds = new List<int>();
                        try
                        {
                            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                            produtosIds = serializer.Deserialize<List<int>>(produto.Produtos);
                        }
                        catch
                        {
                            // Se falhar ao parsear, tentar como string separada por vírgula
                            if (!string.IsNullOrEmpty(produto.Produtos))
                            {
                                produtosIds = produto.Produtos.Split(',').Select(id => int.Parse(id.Trim())).ToList();
                            }
                        }
                        
                        if (produtosIds.Count > 0)
                        {
                            var produtosDisponiveis = _databaseService.ObterProdutosPorIds(produtosIds);
                            string opcoesProdutos = "";
                            foreach (var prod in produtosDisponiveis)
                            {
                                string nomeProdEscapado = prod.Nome.Replace("\"", "\\\"").Replace("'", "\\'");
                                opcoesProdutos += string.Format("<option value='{0}'>{1}</option>", prod.Id, prod.Nome);
                            }
                            
                            seletorProdutosHtml = string.Format(@"
                                <div class='mb-3 p-3 bg-light rounded'>
                                    <label class='form-label'><strong>Escolha os {0} biscoitos:</strong></label>
                                    <div id='seletorProdutos_{1}'>
                                        {2}
                                    </div>
                                    <small class='text-muted'>Total selecionado: <span id='totalSelecionado_{1}'>0</span> de {0}</small>
                                </div>", 
                                produto.QuantidadeSaco, 
                                produto.Id,
                                GerarSeletoresProdutos(produto.Id, produtosDisponiveis, produto.QuantidadeSaco));
                            
                            onclickSaco = string.Format("adicionarSacoAoCarrinho({0}, \"{1}\", {2})", 
                                produto.Id, nomeEscapado, produto.QuantidadeSaco);
                        }
                    }

                    // Usar placeholder SVG inline se a imagem não existir
                    string placeholderSvg = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='200' height='200'%3E%3Crect width='200' height='200' fill='%23e9ecef'/%3E%3Ctext x='50%25' y='50%25' font-family='Arial' font-size='14' fill='%23999999' text-anchor='middle' dy='.3em'%3EImagem N%26atilde%3Bo Disponível%3C/text%3E%3C/svg%3E";
                    
                    // Determinar a URL da imagem - usar placeholder se vazio
                    string imagemSrc = !string.IsNullOrEmpty(produto.ImagemUrl) ? produto.ImagemUrl : placeholderSvg;
                    
                    string html = string.Format(@"
                        <div class='produto-card mb-3'>
                            <div class='row'>
                                <div class='col-md-3'>
                                    <img src='{0}' alt='{1}' class='produto-imagem' data-original-src='{10}' onerror='this.onerror=null; if(this.src !== ""{9}"") {{ this.src=""{9}""; }}' />
                                </div>
                                <div class='col-md-9'>
                                    <h5>{1}</h5>
                                    <p class='text-muted'>{2}</p>
                                    {7}
                                    {8}
                                    <div class='row align-items-end'>
                                        <div class='col-md-4'>
                                            <label class='form-label'><strong>Quantidade:</strong></label>
                                            <div class='input-group'>
                                                <button class='btn btn-outline-secondary' type='button' onclick='diminuirQuantidade({3})'>-</button>
                                                <input type='number' id='quantidade_{3}' value='1' min='1' class='form-control text-center' />
                                                <button class='btn btn-outline-secondary' type='button' onclick='aumentarQuantidade({3})'>+</button>
                                            </div>
                                        </div>
                                        <div class='col-md-4'>
                                            <p class='mb-0'><strong>Preço Unitário:</strong></p>
                                            <p class='h5 text-primary' id='precoUnitario_{3}'>R$ {6}</p>
                                        </div>
                                        <div class='col-md-4'>
                                            <button class='btn btn-success w-100' type='button' 
                                                onclick='{9}'>
                                                <i class='fas fa-cart-plus'></i> Adicionar ao Pedido
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>",
                        imagemSrc,
                        produto.Nome,
                        produto.Descricao,
                        produto.Id,
                        nomeEscapado,
                        precoStr,
                        produto.Preco.ToString("F2"),
                        tamanhosHtml,
                        seletorProdutosHtml,
                        !string.IsNullOrEmpty(onclickSaco) 
                            ? onclickSaco
                            : "adicionarAoCarrinho(" + produto.Id + ", \"" + nomeEscapado + "\", document.getElementById(\"tamanho_" + produto.Id + "\").value, document.getElementById(\"quantidade_" + produto.Id + "\").value)",
                        placeholderSvg,
                        !string.IsNullOrEmpty(produto.ImagemUrl) ? produto.ImagemUrl : "");
                    produtosContainer.InnerHtml += html;
                }
            }
            catch (Exception ex)
            {
                produtosContainer.InnerHtml = string.Format("<div class='alert alert-danger'>Erro ao carregar produtos: {0}</div>", System.Web.HttpUtility.HtmlEncode(ex.Message));
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
            
            ddlDataRetirada.Items.Clear();
            foreach (var data in segundas)
            {
                ddlDataRetirada.Items.Add(new ListItem(data.ToString("dd/MM/yyyy - dddd"), data.ToString("yyyy-MM-dd")));
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
                return;
            }

            string html = "";
            decimal total = 0;

            foreach (var item in Carrinho)
            {
                total += item.Subtotal;
                html += string.Format(@"
                    <div class='item-carrinho'>
                        <div class='d-flex justify-content-between align-items-center'>
                            <div>
                                <strong>{0}</strong><br />
                                <small class='text-muted'>{1} - Qtd: {2}</small>
                            </div>
                            <div class='text-end'>
                                <strong>R$ {3}</strong><br />
                                <button class='btn btn-sm btn-danger' onclick='removerItem({4}, ""{1}"")'>
                                    <i class='fas fa-trash'></i>
                                </button>
                            </div>
                        </div>
                    </div>",
                    item.NomeProduto,
                    item.Tamanho,
                    item.Quantidade,
                    item.Subtotal.ToString("F2"),
                    item.ProdutoId);
            }

            carrinhoContainer.InnerHtml = html;
            totalPedido.InnerText = total.ToString("F2");
            totalContainer.Style["display"] = "block";
            
            // Habilitar botão se houver itens
            if (Carrinho.Count > 0)
            {
                btnFazerReserva.Enabled = true;
                // Adicionar data attribute para indicar que está habilitado
                btnFazerReserva.Attributes["data-enabled"] = "true";
            }
            else
            {
                btnFazerReserva.Enabled = false;
            }
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
                            var modal = new bootstrap.Modal(modalElement);
                            modal.show();
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

            if (ddlDataRetirada.SelectedValue == null || string.IsNullOrEmpty(ddlDataRetirada.SelectedValue))
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
                        
                        Session["ClienteId"] = cliente.Id;
                        Session["ClienteNome"] = cliente.Nome;
                        Session["ClienteEmail"] = cliente.Email;
                        Session["IsAdmin"] = cliente.IsAdmin;
                        if (!string.IsNullOrEmpty(cliente.Telefone))
                        {
                            Session["ClienteTelefone"] = cliente.Telefone;
                        }
                        Session["SessionStartTime"] = DateTime.Now;
                        
                        // Atualizar links do header após login
                        VerificarLogin();
                    }
                    else
                    {
                        // Cliente não existe - criar novo cliente e fazer login
                        cliente = new Cliente
                        {
                            Nome = nome.Trim(),
                            Email = emailFormatado,
                            Telefone = telefoneFormatado,
                            TemWhatsApp = !string.IsNullOrEmpty(telefoneFormatado),
                            Provider = "Email",
                            EmailConfirmado = false,
                            WhatsAppConfirmado = false,
                            IsAdmin = false, // Será definido automaticamente pelo CriarOuAtualizarCliente se o email for de administrador
                            DataCadastro = DateTime.Now
                        };
                        
                        clienteId = _databaseService.CriarOuAtualizarCliente(cliente);
                        
                        // Buscar cliente atualizado para obter IsAdmin correto
                        cliente = _databaseService.ObterClientePorId(clienteId);
                        
                        // Fazer login automático
                        Session["ClienteId"] = clienteId;
                        Session["ClienteNome"] = cliente.Nome;
                        Session["ClienteEmail"] = cliente.Email;
                        Session["IsAdmin"] = cliente != null ? cliente.IsAdmin : false;
                        if (!string.IsNullOrEmpty(cliente.Telefone))
                        {
                            Session["ClienteTelefone"] = cliente.Telefone;
                        }
                        Session["SessionStartTime"] = DateTime.Now;
                        
                        // Atualizar links do header após login automático
                        VerificarLogin();
                    }
                }
                catch
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "ErroLogin", 
                        $"alert('{EscapeJavaScript("Erro ao processar login automático. Por favor, tente novamente.")}');", true);
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

                    try
                    {
                        var whatsAppService = new WhatsAppService();
                        if (!string.IsNullOrEmpty(reserva.Telefone))
                        {
                            whatsAppService.EnviarConfirmacaoReserva(reserva, reserva.Telefone);
                        }
                    }
                    catch
                    {
                        // Erro ao enviar WhatsApp - não bloquear o processo
                    }
                });

                // Limpar carrinho
                Carrinho.Clear();

                // Atualizar menu após reserva
                VerificarLogin();
                
                // Fechar modal de reserva e mostrar modal de sucesso
                // Depois redirecionar para Minhas Reservas
                string scriptFecharEAbrir = @"
                    setTimeout(function() {
                        if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Modal) {
                            KingdomConfeitaria.Modal.hide('modalReserva');
                            setTimeout(function() {
                                KingdomConfeitaria.Modal.show('modalSucesso');
                                
                                // Contador de redirecionamento
                                var contador = 3;
                                var contadorElement = document.getElementById('contadorRedirecionamento');
                                var intervalo = setInterval(function() {
                                    contador--;
                                    if (contadorElement) {
                                        contadorElement.textContent = contador;
                                    }
                                    if (contador <= 0) {
                                        clearInterval(intervalo);
                                        // Redirecionar para Minhas Reservas
                                        window.location.href = 'MinhasReservas.aspx';
                                    }
                                }, 1000);
                            }, 300);
                        } else {
                            // Fallback: redirecionar diretamente se modal não estiver disponível
                            setTimeout(function() {
                                window.location.href = 'MinhasReservas.aspx';
                            }, 500);
                        }
                    }, 100);";
                
                Page.ClientScript.RegisterStartupScript(this.GetType(), "FecharEAbrirModal", scriptFecharEAbrir, true);
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
                        clienteNome.Style["display"] = "inline";
                        
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

