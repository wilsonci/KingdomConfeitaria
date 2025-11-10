using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using KingdomConfeitaria.Models;
using KingdomConfeitaria.Services;
using KingdomConfeitaria.Helpers;

namespace KingdomConfeitaria
{
    public partial class Default : System.Web.UI.Page
    {
        private DatabaseService _databaseService;
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
            _databaseService = new DatabaseService();

            if (!IsPostBack)
            {
                CarregarProdutos();
                CarregarDatasRetirada();
                VerificarLogin();
                AtualizarCarrinho(); // Atualizar carrinho na primeira carga
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

                foreach (var produto in produtos)
                {
                    string nomeEscapado = produto.Nome.Replace("\"", "\\\"").Replace("'", "\\'");
                    string precoPequenoStr = produto.PrecoPequeno.ToString("F2").Replace(",", ".");
                    string precoGrandeStr = produto.PrecoGrande.ToString("F2").Replace(",", ".");
                    // Se o produto tem apenas um preço (pacote), mostrar diferente
                    bool temPequeno = produto.PrecoPequeno > 0;
                    bool temGrande = produto.PrecoGrande > 0;
                    
                    string tamanhosHtml = "";
                    if (temPequeno && temGrande)
                    {
                        tamanhosHtml = string.Format(@"
                            <div class='mb-2'>
                                <label class='form-label'><strong>Tamanho:</strong></label>
                                <select id='tamanho_{0}' class='form-select mb-2' onchange='atualizarPreco({0})'>
                                    <option value='Pequeno' data-preco='{1}'>Pequeno - R$ {2}</option>
                                    <option value='Grande' data-preco='{3}'>Grande - R$ {4}</option>
                                </select>
                            </div>", produto.Id, precoPequenoStr, produto.PrecoPequeno.ToString("F2"), precoGrandeStr, produto.PrecoGrande.ToString("F2"));
                    }
                    else if (temPequeno)
                    {
                        tamanhosHtml = string.Format(@"<input type='hidden' id='tamanho_{0}' value='Pequeno' data-preco='{1}' />", produto.Id, precoPequenoStr);
                    }
                    else if (temGrande)
                    {
                        tamanhosHtml = string.Format(@"<input type='hidden' id='tamanho_{0}' value='Grande' data-preco='{1}' />", produto.Id, precoGrandeStr);
                    }

                    // Se for saco promocional, mostrar seletor de produtos
                    string seletorProdutosHtml = "";
                    string onclickSaco = "";
                    if (produto.EhSacoPromocional && produto.QuantidadeSaco > 0 && !string.IsNullOrEmpty(produto.TamanhoSaco))
                    {
                        var produtosDisponiveis = _databaseService.ObterProdutosPorTamanho(produto.TamanhoSaco);
                        string opcoesProdutos = "";
                        foreach (var prod in produtosDisponiveis)
                        {
                            string nomeProdEscapado = prod.Nome.Replace("\"", "\\\"").Replace("'", "\\'");
                            opcoesProdutos += string.Format("<option value='{0}'>{1}</option>", prod.Id, prod.Nome);
                        }
                        
                        seletorProdutosHtml = string.Format(@"
                            <div class='mb-3 p-3 bg-light rounded'>
                                <label class='form-label'><strong>Escolha os {0} biscoitos ({1}):</strong></label>
                                <div id='seletorProdutos_{2}'>
                                    {3}
                                </div>
                                <small class='text-muted'>Total selecionado: <span id='totalSelecionado_{2}'>0</span> de {0}</small>
                            </div>", 
                            produto.QuantidadeSaco, 
                            produto.TamanhoSaco,
                            produto.Id,
                            GerarSeletoresProdutos(produto.Id, produtosDisponiveis, produto.QuantidadeSaco));
                        
                        onclickSaco = string.Format("adicionarSacoAoCarrinho({0}, \"{1}\", \"{2}\", {3})", 
                            produto.Id, nomeEscapado, produto.TamanhoSaco, produto.QuantidadeSaco);
                    }

                    string html = string.Format(@"
                        <div class='produto-card mb-3'>
                            <div class='row'>
                                <div class='col-md-3'>
                                    <img src='{0}' alt='{1}' class='produto-imagem' onerror='this.onerror=null; this.src=""Images/placeholder.png"";' />
                                </div>
                                <div class='col-md-9'>
                                    <h5>{1}</h5>
                                    <p class='text-muted'>{2}</p>
                                    {9}
                                    {11}
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
                                            <p class='h5 text-primary' id='precoUnitario_{3}'>{10}</p>
                                        </div>
                                        <div class='col-md-4'>
                                            <button class='btn btn-success w-100' type='button' 
                                                onclick='{12}'>
                                                <i class='fas fa-cart-plus'></i> Adicionar ao Pedido
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>",
                        !string.IsNullOrEmpty(produto.ImagemUrl) ? produto.ImagemUrl : "Images/placeholder.png",
                        produto.Nome,
                        produto.Descricao,
                        produto.Id,
                        precoPequenoStr,
                        produto.PrecoPequeno.ToString("F2"),
                        precoGrandeStr,
                        produto.PrecoGrande.ToString("F2"),
                        nomeEscapado,
                        tamanhosHtml,
                        temPequeno ? "R$ " + produto.PrecoPequeno.ToString("F2") : "R$ " + produto.PrecoGrande.ToString("F2"),
                        seletorProdutosHtml,
                        !string.IsNullOrEmpty(onclickSaco) 
                            ? onclickSaco
                            : "adicionarAoCarrinho(" + produto.Id + ", \"" + nomeEscapado + "\", document.getElementById(\"tamanho_" + produto.Id + "\").value, document.getElementById(\"quantidade_" + produto.Id + "\").value)");
                    produtosContainer.InnerHtml += html;
                }
            }
            catch (Exception ex)
            {
                produtosContainer.InnerHtml = string.Format("<div class='alert alert-danger'>Erro ao carregar produtos: {0}</div>", ex.Message);
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
            catch (Exception ex)
            {
                // Log do erro para debug
                System.Diagnostics.Debug.WriteLine("Erro ao processar adicionar ao carrinho: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Argument recebido: " + argument);
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
                if (partes.Length >= 6)
                {
                    int sacoId = int.Parse(partes[0]);
                    string nomeSaco = partes[1];
                    string tamanhoSaco = partes[2];
                    decimal precoSaco = decimal.Parse(partes[3].Replace(",", "."), CultureInfo.InvariantCulture);
                    int quantidadeSacos = int.Parse(partes[4]);
                    string produtosIdsStr = partes[5];
                    
                    var produtosIds = produtosIdsStr.Split(',').Select(id => int.Parse(id.Trim())).ToList();
                    
                    // Obter informações dos produtos selecionados para exibir no nome
                    var todosProdutos = _databaseService.ObterTodosProdutos();
                    var produtosSelecionados = todosProdutos.Where(p => produtosIds.Contains(p.Id)).ToList();
                    
                    // Criar nome do saco com os produtos selecionados
                    string nomesProdutos = string.Join(", ", produtosSelecionados.Select(p => p.Nome));
                    string nomeSacoCompleto = nomeSaco + " - " + nomesProdutos;
                    
                    // Adicionar o saco promocional ao carrinho
                    // Usar uma chave única baseada no sacoId + tamanho + produtos selecionados
                    string chaveSaco = sacoId + "|" + tamanhoSaco + "|" + produtosIdsStr;
                    var sacoItem = Carrinho.FirstOrDefault(i => 
                        i.ProdutoId == sacoId && 
                        i.Tamanho == tamanhoSaco && 
                        i.NomeProduto.Contains(nomeSaco));
                    
                    if (sacoItem != null)
                    {
                        // Se já existe um saco similar, aumentar a quantidade
                        sacoItem.Quantidade += quantidadeSacos;
                        sacoItem.Subtotal = sacoItem.Quantidade * sacoItem.PrecoUnitario;
                    }
                    else
                    {
                        // Adicionar novo saco
                        Carrinho.Add(new ItemPedido
                        {
                            ProdutoId = sacoId,
                            NomeProduto = nomeSacoCompleto,
                            Tamanho = tamanhoSaco,
                            Quantidade = quantidadeSacos,
                            PrecoUnitario = precoSaco,
                            Subtotal = precoSaco * quantidadeSacos
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao processar adicionar saco ao carrinho: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Argument recebido: " + argument);
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
                // Adicionar script para garantir que o botão esteja habilitado no cliente
                // Usar um nome único para o script
                string scriptKey = "HabilitarBotao_" + DateTime.Now.Ticks;
                ScriptManager.RegisterStartupScript(this, GetType(), scriptKey, 
                    "setTimeout(function() { " +
                    "var btn = document.getElementById('" + btnFazerReserva.ClientID + "'); " +
                    "if (btn) { " +
                    "btn.disabled = false; " +
                    "btn.removeAttribute('disabled'); " +
                    "btn.style.opacity = '1'; " +
                    "btn.style.cursor = 'pointer'; " +
                    "btn.classList.remove('disabled'); " +
                    "} " +
                    "}, 100);", true);
            }
            else
            {
                btnFazerReserva.Enabled = false;
            }
        }

        protected void btnFazerReserva_Click(object sender, EventArgs e)
        {
            if (Carrinho.Count == 0)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "CarrinhoVazio", 
                    "alert('Adicione produtos ao carrinho antes de fazer a reserva.');", true);
                return;
            }

            // Preencher campos do modal se cliente estiver logado
            if (Session["ClienteNome"] != null)
            {
                txtNome.Text = Session["ClienteNome"].ToString();
            }
            if (Session["ClienteEmail"] != null)
            {
                txtEmail.Text = Session["ClienteEmail"].ToString();
            }
            if (Session["ClienteTelefone"] != null)
            {
                txtTelefone.Text = Session["ClienteTelefone"].ToString();
            }

            // Limpar observações
            txtObservacoes.Text = "";

            // Abrir modal via JavaScript - múltiplas tentativas para garantir
            string scriptAbrirModal = @"
                setTimeout(function() { 
                    try { 
                        var modalElement = document.getElementById('modalReserva'); 
                        if (modalElement) { 
                            // Tentar usar Bootstrap 5
                            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                                var modal = bootstrap.Modal.getOrCreateInstance(modalElement);
                                modal.show();
                            } else {
                                // Fallback: mostrar modal manualmente
                                modalElement.style.display = 'block';
                                modalElement.classList.add('show');
                                document.body.classList.add('modal-open');
                                var backdrop = document.createElement('div');
                                backdrop.className = 'modal-backdrop fade show';
                                backdrop.id = 'modalBackdrop';
                                document.body.appendChild(backdrop);
                            }
                        } else { 
                            console.error('Modal não encontrado');
                            alert('Erro: Modal não encontrado. Por favor, recarregue a página.');
                        } 
                    } catch(e) { 
                        console.error('Erro ao abrir modal:', e);
                        alert('Por favor, preencha os dados do formulário abaixo e clique em Confirmar Reserva.');
                    } 
                }, 200);";
            
            ScriptManager.RegisterStartupScript(this, GetType(), "AbrirModal_" + DateTime.Now.Ticks, scriptAbrirModal, true);
        }

        protected void btnConfirmarReserva_Click(object sender, EventArgs e)
        {
            if (Carrinho.Count == 0)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "CarrinhoVazio", 
                    "alert('Adicione produtos ao carrinho antes de fazer a reserva.');", true);
                return;
            }

            // Validar campos obrigatórios
            if (string.IsNullOrWhiteSpace(txtNome.Text))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "NomeVazio", 
                    "alert('Por favor, preencha o nome.');", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text) || !txtEmail.Text.Contains("@"))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "EmailInvalido", 
                    "alert('Por favor, preencha um email válido.');", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTelefone.Text))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "TelefoneVazio", 
                    "alert('Por favor, preencha o telefone.');", true);
                return;
            }

            if (ddlDataRetirada.SelectedValue == null || string.IsNullOrEmpty(ddlDataRetirada.SelectedValue))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "DataVazia", 
                    "alert('Por favor, selecione uma data de retirada.');", true);
                return;
            }

            try
            {
                // Verificar se cliente está logado
                int? clienteId = null;
                if (Session["ClienteId"] != null)
                {
                    clienteId = (int)Session["ClienteId"];
                }

                // Formatar email e telefone antes de salvar
                string emailFormatado = txtEmail.Text.Trim().ToLowerInvariant();
                string telefoneFormatado = System.Text.RegularExpressions.Regex.Replace(txtTelefone.Text, @"[^\d]", "");

                var reserva = new Reserva
                {
                    Nome = txtNome.Text.Trim(),
                    Email = emailFormatado,
                    Telefone = telefoneFormatado,
                    DataRetirada = DateTime.Parse(ddlDataRetirada.SelectedValue),
                    DataReserva = DateTime.Now,
                    Status = "Pendente",
                    ValorTotal = Carrinho.Sum(i => i.Subtotal),
                    Itens = new List<ItemPedido>(Carrinho),
                    Observacoes = txtObservacoes.Text,
                    ConvertidoEmPedido = false,
                    PrevisaoEntrega = null,
                    Cancelado = false,
                    ClienteId = clienteId
                };

                // Salvar no banco de dados
                _databaseService.SalvarReserva(reserva);

                // Enviar emails (não bloquear se falhar)
                try
                {
                    var emailService = new EmailService();
                    emailService.EnviarConfirmacaoReserva(reserva);
                }
                catch (Exception exEmail)
                {
                    System.Diagnostics.Debug.WriteLine("Erro ao enviar email: " + exEmail.Message);
                    // Continuar mesmo se email falhar
                }

                // Enviar WhatsApp se tiver telefone (não bloquear se falhar)
                try
                {
                    var whatsAppService = new WhatsAppService();
                    if (!string.IsNullOrEmpty(reserva.Telefone))
                    {
                        whatsAppService.EnviarConfirmacaoReserva(reserva, reserva.Telefone);
                    }
                }
                catch (Exception exWhatsApp)
                {
                    System.Diagnostics.Debug.WriteLine("Erro ao enviar WhatsApp: " + exWhatsApp.Message);
                    // Continuar mesmo se WhatsApp falhar
                }

                // Limpar carrinho
                Carrinho.Clear();

                // Mostrar modal de sucesso
                ScriptManager.RegisterStartupScript(this, GetType(), "FecharModalReserva", 
                    "var modal = bootstrap.Modal.getInstance(document.getElementById('modalReserva')); modal.hide();", true);
                ScriptManager.RegisterStartupScript(this, GetType(), "AbrirModalSucesso", 
                    "var modal = new bootstrap.Modal(document.getElementById('modalSucesso')); modal.show();", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "Erro", 
                    string.Format("alert('Erro ao processar reserva: {0}');", ex.Message.Replace("'", "\\'")), true);
            }
        }

        private void VerificarLogin()
        {
            try
            {
                if (clienteNome != null && linkLogin != null && linkMinhasReservas != null && linkLogout != null)
                {
                    if (Session["ClienteId"] != null)
                    {
                        clienteNome.InnerText = "Olá, " + (Session["ClienteNome"] != null ? Session["ClienteNome"].ToString() : "");
                        linkLogin.Visible = false;
                        linkMinhasReservas.Visible = true;
                        linkLogout.Visible = true;
                    }
                    else
                    {
                        clienteNome.InnerText = "";
                        linkLogin.Visible = true;
                        linkMinhasReservas.Visible = false;
                        linkLogout.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log do erro (implementar logging adequado)
                System.Diagnostics.Debug.WriteLine("Erro ao verificar login: " + ex.Message);
            }
        }
    }
}

