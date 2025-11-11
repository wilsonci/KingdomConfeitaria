using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Script.Services;
using System.Web.UI;
using KingdomConfeitaria.Services;
using KingdomConfeitaria.Models;

namespace KingdomConfeitaria
{
    public partial class VerReserva : System.Web.UI.Page
    {
        private DatabaseService _databaseService;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Configurar encoding UTF-8
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.Charset = "UTF-8";
            
            _databaseService = new DatabaseService();

            // Verificar se é uma requisição de atualização
            if (IsPostBack && Request.Form["__EVENTTARGET"] == "btnSalvarReserva")
            {
                SalvarReserva();
                return;
            }

            string token = Request.QueryString["token"];

            if (string.IsNullOrEmpty(token))
            {
                MostrarErro("Token de acesso não fornecido.");
                return;
            }

            try
            {
                var reserva = _databaseService.ObterReservaPorToken(token);

                if (reserva == null)
                {
                    MostrarErro("Reserva não encontrada ou token inválido.");
                    return;
                }

                // Verificar se cliente está logado e fazer login automático se necessário
                VerificarEAutenticarCliente(reserva);

                // Exibir detalhes da reserva
                ExibirReserva(reserva);
            }
            catch (Exception ex)
            {
                MostrarErro("Erro ao carregar reserva: " + ex.Message);
            }
        }

        private void VerificarEAutenticarCliente(Models.Reserva reserva)
        {
            // Se a reserva tem cliente associado e não está logado, fazer login automático
            if (reserva.ClienteId.HasValue && Session["ClienteId"] == null)
            {
                // Usar ClienteId diretamente ao invés de buscar por email
                var cliente = _databaseService.ObterClientePorId(reserva.ClienteId.Value);
                if (cliente != null)
                {
                    Session["ClienteId"] = cliente.Id;
                    Session["ClienteNome"] = cliente.Nome;
                    Session["ClienteEmail"] = cliente.Email;
                    Session["ClienteTelefone"] = cliente.Telefone;
                }
            }

            // Atualizar header
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

        private void ExibirReserva(Models.Reserva reserva)
        {
            var baseUrl = System.Configuration.ConfigurationManager.AppSettings["BaseUrl"] ?? Request.Url.GetLeftPart(UriPartial.Authority);
            string linkReserva = Request.Url.AbsoluteUri;
            
            string statusClass = "status-" + reserva.Status.ToLower().Replace(" ", "-");
            string statusBadge = string.Format("<span class='status-badge {0}'>{1}</span>", statusClass, reserva.Status);

            // Verificar se pode editar (usando StatusId para verificar PermiteAlteracao)
            bool podeEditar = reserva.StatusId.HasValue && 
                             _databaseService.StatusPermiteAlteracao(reserva.StatusId.Value) &&
                             Session["ClienteId"] != null && 
                             reserva.ClienteId.HasValue && 
                             reserva.ClienteId.Value.ToString() == Session["ClienteId"].ToString();

            // Itens da reserva (editáveis ou não)
            string itensHtml = "";
            if (podeEditar)
            {
                // Interface editável
                itensHtml = "<div id='itensReservaContainer'>";
                int itemIndex = 0;
                foreach (var item in reserva.Itens)
                {
                    itensHtml += string.Format(@"
                        <div class='item-reserva mb-3 p-3 border rounded' data-item-index='{0}'>
                            <div class='row align-items-center'>
                                <div class='col-md-4'>
                                    <strong>{1}</strong> ({2})
                                </div>
                                <div class='col-md-3'>
                                    <label class='form-label small'>Quantidade:</label>
                                    <input type='number' class='form-control form-control-sm quantidade-item' 
                                           value='{3}' min='1' data-produto-id='{4}' data-tamanho='{2}' 
                                           data-preco-unitario='{5}' onchange='atualizarSubtotalItem(this)' />
                                </div>
                                <div class='col-md-3'>
                                    <label class='form-label small'>Subtotal:</label>
                                    <input type='text' class='form-control form-control-sm subtotal-item' 
                                           value='R$ {6}' readonly data-subtotal='{6}' />
                                </div>
                                <div class='col-md-2 text-end'>
                                    <button type='button' class='btn btn-danger btn-sm' onclick='removerItem(this)'>
                                        <i class='fas fa-trash'></i>
                                    </button>
                                </div>
                            </div>
                            <input type='hidden' class='produto-id' value='{4}' />
                            <input type='hidden' class='nome-produto' value='{1}' />
                            <input type='hidden' class='tamanho-item' value='{2}' />
                            <input type='hidden' class='preco-unitario' value='{5}' />
                        </div>",
                        itemIndex++,
                        System.Web.HttpUtility.HtmlEncode(item.NomeProduto),
                        System.Web.HttpUtility.HtmlEncode(item.Tamanho),
                        item.Quantidade,
                        item.ProdutoId,
                        item.PrecoUnitario.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                        item.Subtotal.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
                }
                itensHtml += "</div>";
                
                // Botão para adicionar novo item
                itensHtml += string.Format(@"
                    <div class='mt-3'>
                        <button type='button' class='btn btn-primary btn-sm' onclick='adicionarNovoItem()'>
                            <i class='fas fa-plus'></i> Adicionar Item
                        </button>
                    </div>
                    <div id='novoItemContainer' class='mt-3' style='display:none;'>
                        <div class='card'>
                            <div class='card-body'>
                                <h6 class='card-title'>Adicionar Novo Item</h6>
                                <div class='row'>
                                    <div class='col-md-6 mb-2'>
                                        <label class='form-label small'>Produto:</label>
                                        <select class='form-select form-select-sm' id='novoItemProduto' onchange='selecionarProdutoNovoItem(this)'>
                                            <option value=''>Selecione um produto...</option>
                                        </select>
                                    </div>
                                    <div class='col-md-3 mb-2'>
                                        <label class='form-label small'>Tamanho:</label>
                                        <select class='form-select form-select-sm' id='novoItemTamanho' onchange='atualizarPrecoNovoItem()'>
                                            <option value=''>Selecione...</option>
                                            <option value='Pequeno'>Pequeno</option>
                                            <option value='Grande'>Grande</option>
                                        </select>
                                    </div>
                                    <div class='col-md-2 mb-2'>
                                        <label class='form-label small'>Quantidade:</label>
                                        <input type='number' class='form-control form-control-sm' id='novoItemQuantidade' 
                                               value='1' min='1' onchange='atualizarPrecoNovoItem()' />
                                    </div>
                                    <div class='col-md-1 mb-2 d-flex align-items-end'>
                                        <button type='button' class='btn btn-success btn-sm' onclick='confirmarNovoItem()'>
                                            <i class='fas fa-check'></i>
                                        </button>
                                    </div>
                                </div>
                                <div class='row mt-2'>
                                    <div class='col-md-6'>
                                        <small class='text-muted' id='novoItemPrecoInfo'></small>
                                    </div>
                                    <div class='col-md-6 text-end'>
                                        <button type='button' class='btn btn-secondary btn-sm' onclick='cancelarNovoItem()'>
                                            Cancelar
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>");
            }
            else
            {
                // Exibição somente leitura
                itensHtml = "<ul class='list-unstyled'>";
                foreach (var item in reserva.Itens)
                {
                    itensHtml += string.Format("<li class='mb-2'>{0} ({1}) - Quantidade: {2} - R$ {3}</li>",
                        System.Web.HttpUtility.HtmlEncode(item.NomeProduto), 
                        System.Web.HttpUtility.HtmlEncode(item.Tamanho), 
                        item.Quantidade, 
                        item.Subtotal.ToString("F2"));
                }
                itensHtml += "</ul>";
            }

            string previsaoHtml = "";
            if (reserva.PrevisaoEntrega.HasValue)
            {
                previsaoHtml = string.Format("<div class='alert alert-info'><i class='fas fa-calendar'></i> <strong>Previsão de Entrega:</strong> {0}</div>",
                    reserva.PrevisaoEntrega.Value.ToString("dd/MM/yyyy"));
            }

            // Formulário de edição ou exibição de observações
            string observacoesHtml = "";
            if (podeEditar)
            {
                observacoesHtml = string.Format(@"
                    <div class='mb-3'>
                        <label class='form-label'><strong>Observações:</strong></label>
                        <textarea class='form-control' id='txtObservacoes' name='txtObservacoes' rows='3'>{0}</textarea>
                    </div>", 
                    System.Web.HttpUtility.HtmlEncode(reserva.Observacoes ?? ""));
            }
            else
            {
                if (!string.IsNullOrEmpty(reserva.Observacoes))
                {
                    observacoesHtml = string.Format("<div class='alert alert-secondary'><strong>Observações:</strong> {0}</div>", 
                        System.Web.HttpUtility.HtmlEncode(reserva.Observacoes));
                }
            }

            string textoCompartilhar = string.Format("Minha reserva na Kingdom Confeitaria - Valor: R$ {0} - Data de Retirada: {1}",
                reserva.ValorTotal.ToString("F2"), reserva.DataRetirada.ToString("dd/MM/yyyy"));

            // Botões de ação
            string acoesHtml = "";
            if (Session["ClienteId"] != null)
            {
                acoesHtml = "<div class='mt-3'><a href='MinhasReservas.aspx' class='btn btn-primary'><i class='fas fa-list'></i> Ver Todas Minhas Reservas</a></div>";
            }
            else
            {
                acoesHtml = "<div class='mt-3'><a href='Login.aspx' class='btn btn-primary'><i class='fas fa-sign-in-alt'></i> Fazer Login para Gerenciar Reservas</a></div>";
            }

            // Botões de edição (apenas se pode editar)
            string botoesEdicaoHtml = "";
            if (podeEditar)
            {
                botoesEdicaoHtml = string.Format(@"
                    <div class='border-top pt-3 mt-3'>
                        <h5><i class='fas fa-edit'></i> Editar Reserva</h5>
                        <p class='text-muted'>Você pode editar a data de retirada, observações e itens enquanto a reserva estiver aberta.</p>
                        <div class='mt-3'>
                            <button type='button' class='btn btn-success' onclick='salvarReserva({0})'>
                                <i class='fas fa-save'></i> Salvar Alterações
                            </button>
                            <button type='button' class='btn btn-secondary' onclick='cancelarEdicao()'>
                                <i class='fas fa-times'></i> Cancelar
                            </button>
                        </div>
                    </div>",
                    reserva.Id);
            }
            else if (reserva.StatusId.HasValue && !_databaseService.StatusPermiteAlteracao(reserva.StatusId.Value))
            {
                botoesEdicaoHtml = "<div class='alert alert-warning mt-3'><i class='fas fa-info-circle'></i> Esta reserva não pode mais ser editada ou excluída pois já está em " + System.Web.HttpUtility.HtmlEncode(reserva.Status) + ".</div>";
            }

            // Data de retirada (editável ou não)
            string dataRetiradaHtml = "";
            if (podeEditar)
            {
                dataRetiradaHtml = string.Format(@"
                    <div class='mb-3'>
                        <label class='form-label'><strong>Data de Retirada:</strong></label>
                        <input type='date' class='form-control' id='txtDataRetiradaEdicao' name='txtDataRetiradaEdicao' value='{0}' required />
                    </div>",
                    reserva.DataRetirada.ToString("yyyy-MM-dd"));
            }
            else
            {
                dataRetiradaHtml = string.Format("<p><strong>Data de Retirada:</strong> {0}</p>",
                    reserva.DataRetirada.ToString("dd/MM/yyyy"));
            }

            conteudoContainer.InnerHtml = string.Format(@"
                <h2 class='mb-4'><i class='fas fa-receipt'></i> Detalhes da Reserva #{0}</h2>
                
                <div class='reserva-detalhes'>
                    <div class='d-flex justify-content-between align-items-start mb-4'>
                        <div>
                            <h4>Reserva #{0}</h4>
                            <p class='text-muted mb-0'>Data da Reserva: {1}</p>
                        </div>
                        <div>
                            {2}
                        </div>
                    </div>
                    
                    <div class='row mb-4'>
                        <div class='col-md-6'>
                            <h5><i class='fas fa-calendar-alt'></i> Informações da Reserva</h5>
                            {3}
                            <p><strong>Valor Total:</strong> <span class='h4 text-success'>R$ {4}</span></p>
                            {5}
                        </div>
                        <div class='col-md-6'>
                            <h5><i class='fas fa-user'></i> Dados do Cliente</h5>
                            <p><strong>Nome:</strong> {6}</p>
                            <p><strong>Email:</strong> {7}</p>
                            <p><strong>Telefone:</strong> {8}</p>
                        </div>
                    </div>
                    
                    {10}
                    
                    <div class='mb-4'>
                        <h5><i class='fas fa-shopping-bag'></i> Itens da Reserva</h5>
                        <div id='valorTotalContainer' class='alert alert-info'>
                            <strong>Valor Total: <span id='valorTotalReserva'>R$ {16}</span></strong>
                        </div>
                        <div id='itensLista'>
                            {9}
                        </div>
                    </div>
                    
                    {14}
                    
                    <div class='border-top pt-4 mt-4'>
                        <h5><i class='fas fa-share-alt'></i> Compartilhar Reserva</h5>
                        <p>Compartilhe sua reserva nas redes sociais:</p>
                        <div>
                            <button type='button' class='btn btn-share btn-primary' onclick='compartilharFacebook(\""{11}\"", \""{12}\"")'>
                                <i class='fab fa-facebook'></i> Facebook
                            </button>
                            <button type='button' class='btn btn-share btn-success' onclick='compartilharWhatsApp(\""{11}\"", \""{12}\"")'>
                                <i class='fab fa-whatsapp'></i> WhatsApp
                            </button>
                            <button type='button' class='btn btn-share btn-info' onclick='compartilharTwitter(\""{11}\"", \""{12}\"")'>
                                <i class='fab fa-twitter'></i> Twitter
                            </button>
                            <button type='button' class='btn btn-share btn-secondary' onclick='compartilharEmail(\""{11}\"", \""{12}\"")'>
                                <i class='fas fa-envelope'></i> Email
                            </button>
                        </div>
                    </div>
                    
                    {13}
                </div>
                <input type='hidden' id='hdnReservaId' name='hdnReservaId' value='{0}' />
                <input type='hidden' id='hdnToken' name='hdnToken' value='{15}' />",
                reserva.Id,
                reserva.DataReserva.ToString("dd/MM/yyyy HH:mm"),
                statusBadge,
                dataRetiradaHtml,
                reserva.ValorTotal.ToString("F2"),
                previsaoHtml,
                !string.IsNullOrEmpty(reserva.Nome) ? System.Web.HttpUtility.HtmlEncode(reserva.Nome) : "Não informado",
                !string.IsNullOrEmpty(reserva.Email) ? System.Web.HttpUtility.HtmlEncode(reserva.Email) : "Não informado",
                !string.IsNullOrEmpty(reserva.Telefone) ? System.Web.HttpUtility.HtmlEncode(reserva.Telefone) : "Não informado",
                itensHtml,
                observacoesHtml,
                linkReserva,
                textoCompartilhar.Replace("\"", "&quot;"),
                acoesHtml,
                botoesEdicaoHtml,
                Request.QueryString["token"],
                reserva.ValorTotal.ToString("F2")
            );
        }

        private void SalvarReserva()
        {
            try
            {
                string token = Request.Form["hdnToken"] ?? Request.QueryString["token"];
                if (string.IsNullOrEmpty(token))
                {
                    MostrarErro("Token de acesso não fornecido.");
                    return;
                }

                var reserva = _databaseService.ObterReservaPorToken(token);
                if (reserva == null)
                {
                    MostrarErro("Reserva não encontrada.");
                    return;
                }

                // Verificar se pode editar (usando StatusId para verificar PermiteAlteracao)
                bool podeEditar = reserva.StatusId.HasValue && 
                                 _databaseService.StatusPermiteAlteracao(reserva.StatusId.Value) &&
                                 Session["ClienteId"] != null && 
                                 reserva.ClienteId.HasValue && 
                                 reserva.ClienteId.Value.ToString() == Session["ClienteId"].ToString();

                if (!podeEditar)
                {
                    MostrarErro("Esta reserva não pode ser editada.");
                    return;
                }

                // Atualizar dados
                string dataRetiradaStr = Request.Form["txtDataRetiradaEdicao"];
                if (!string.IsNullOrEmpty(dataRetiradaStr))
                {
                    reserva.DataRetirada = DateTime.Parse(dataRetiradaStr);
                }

                string observacoes = Request.Form["txtObservacoes"];
                reserva.Observacoes = observacoes;

                // Processar itens modificados
                var novosItens = new List<ItemPedido>();
                
                // Buscar todos os campos de itens do formulário
                foreach (string key in Request.Form.AllKeys)
                {
                    if (key != null && key.StartsWith("itens[") && key.Contains("].ProdutoId"))
                    {
                        // Extrair índice do item
                        int startIndex = key.IndexOf('[') + 1;
                        int endIndex = key.IndexOf(']');
                        if (startIndex > 0 && endIndex > startIndex)
                        {
                            string indexStr = key.Substring(startIndex, endIndex - startIndex);
                            if (int.TryParse(indexStr, out int itemIndex))
                            {
                                string produtoIdStr = Request.Form[$"itens[{itemIndex}].ProdutoId"];
                                string quantidadeStr = Request.Form[$"itens[{itemIndex}].Quantidade"];
                                string tamanho = Request.Form[$"itens[{itemIndex}].Tamanho"];
                                string precoUnitarioStr = Request.Form[$"itens[{itemIndex}].PrecoUnitario"];
                                string nomeProduto = Request.Form[$"itens[{itemIndex}].NomeProduto"];

                                if (!string.IsNullOrEmpty(produtoIdStr) &&
                                    int.TryParse(produtoIdStr, out int produtoId) &&
                                    int.TryParse(quantidadeStr, out int quantidade) &&
                                    quantidade > 0 &&
                                    decimal.TryParse(precoUnitarioStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal precoUnitario))
                                {
                                    novosItens.Add(new ItemPedido
                                    {
                                        ProdutoId = produtoId,
                                        NomeProduto = nomeProduto ?? "",
                                        Tamanho = tamanho ?? "",
                                        Quantidade = quantidade,
                                        PrecoUnitario = precoUnitario,
                                        Subtotal = precoUnitario * quantidade
                                    });
                                }
                            }
                        }
                    }
                }

                // Se não houver itens no formulário, manter os itens atuais
                if (novosItens.Count == 0)
                {
                    novosItens = reserva.Itens ?? new List<ItemPedido>();
                }

                // Recalcular valor total
                reserva.ValorTotal = novosItens.Sum(i => i.Subtotal);
                reserva.Itens = novosItens;

                // Salvar no banco
                _databaseService.AtualizarReserva(reserva);
                _databaseService.AtualizarItensReserva(reserva.Id, novosItens);

                // Recarregar página com mensagem de sucesso
                Page.ClientScript.RegisterStartupScript(this.GetType(), "Sucesso", 
                    $"alert('{EscapeJavaScript("Reserva atualizada com sucesso!")}'); window.location.href = 'VerReserva.aspx?token={System.Web.HttpUtility.UrlEncode(token)}';", true);
            }
            catch (Exception ex)
            {
                MostrarErro("Erro ao salvar reserva: " + ex.Message);
            }
        }

        [System.Web.Services.WebMethod]
        [System.Web.Script.Services.ScriptMethod]
        public static object ObterProdutosDisponiveis()
        {
            try
            {
                var databaseService = new DatabaseService();
                var produtos = databaseService.ObterProdutos();
                
                return produtos.Select(p => new
                {
                    id = p.Id,
                    nome = p.Nome,
                    precoPequeno = p.PrecoPequeno,
                    precoGrande = p.PrecoGrande
                }).ToList();
            }
            catch (Exception ex)
            {
                return new { erro = ex.Message };
            }
        }

        private string EscapeJavaScript(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n");
        }

        private void MostrarErro(string mensagem)
        {
            conteudoContainer.InnerHtml = string.Format(@"
                <div class='alert alert-danger'>
                    <h4><i class='fas fa-exclamation-triangle'></i> Erro</h4>
                    <p>{0}</p>
                    <div class='mt-3'>
                        <a href='Default.aspx' class='btn btn-primary'>Voltar para Página Inicial</a>
                    </div>
                </div>",
                mensagem
            );
        }
    }
}

