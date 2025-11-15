using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Web.Script.Serialization;
using System.Security.Cryptography;
using System.Text;
using KingdomConfeitaria.Services;
using KingdomConfeitaria.Models;
using KingdomConfeitaria.Helpers;

namespace KingdomConfeitaria.Handlers
{
    /// <summary>
    /// Generic Handler para callbacks AJAX sem ScriptManager
    /// Muito leve e eficiente - alternativa ao Page Methods
    /// </summary>
    public class CallbackHandler : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable
        {
            get { return false; }
        }
        
        public void ProcessRequest(HttpContext context)
        {
            // Configurar resposta PRIMEIRO - antes de qualquer coisa
            context.Response.ContentType = "application/json";
            context.Response.Charset = "utf-8";
            
            try
            {
                // IMPORTANTE: Verificar se a sessão está disponível
                bool sessionAvailable = false;
                try
                {
                    if (context.Session != null)
                    {
                        // Testar acesso à sessão
                        var test = context.Session["__Test__"];
                        sessionAvailable = true;
                        System.Diagnostics.Debug.WriteLine("Sessão está disponível e acessível");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("AVISO: Sessão é null!");
                    }
                }
                catch (Exception exSession)
                {
                    System.Diagnostics.Debug.WriteLine("ERRO ao acessar sessão: " + exSession.Message);
                    System.Diagnostics.Debug.WriteLine("Stack Trace: " + exSession.StackTrace);
                }
                // Log inicial para debug
                System.Diagnostics.Debug.WriteLine("=== CALLBACKHANDLER PROCESSREQUEST ===");
                System.Diagnostics.Debug.WriteLine("Path: " + context.Request.Path);
                System.Diagnostics.Debug.WriteLine("Method: " + context.Request.HttpMethod);
                System.Diagnostics.Debug.WriteLine("ContentType: " + (context.Request.ContentType ?? "null"));
                System.Diagnostics.Debug.WriteLine("ContentLength: " + context.Request.ContentLength);
                System.Diagnostics.Debug.WriteLine("Session Available: " + (context.Session != null));
                
                // Ler parâmetros - pode vir via QueryString, Form ou Body (JSON)
                Dictionary<string, object> bodyData = null;
                
                // Ler parâmetros do body JSON
                string contentType = context.Request.ContentType ?? "";
                System.Diagnostics.Debug.WriteLine("ContentType: " + contentType);
                
                if (contentType.Contains("application/json"))
                {
                    try
                    {
                        // IMPORTANTE: Para application/json, o ASP.NET não parseia automaticamente
                        // Precisamos ler do InputStream, mas ele pode já ter sido consumido
                        // Vamos usar uma abordagem mais robusta
                        
                        string body = "";
                        
                        // Método 1: Tentar ler do InputStream diretamente
                        try
                        {
                            // Verificar se há conteúdo
                            if (context.Request.ContentLength > 0)
                            {
                                // Tentar obter o stream buffered (se disponível)
                                var inputStream = context.Request.InputStream;
                                
                                // Se o stream não pode ser seeked, precisamos ler uma vez só
                                if (inputStream.CanSeek && inputStream.Position > 0)
                                {
                                    inputStream.Position = 0;
                                }
                                
                                // Ler o body
                                using (var reader = new System.IO.StreamReader(inputStream, System.Text.Encoding.UTF8, true, 1024, true))
                                {
                                    body = reader.ReadToEnd();
                                }
                                
                                System.Diagnostics.Debug.WriteLine("Body lido: " + (string.IsNullOrEmpty(body) ? "VAZIO" : body.Length + " caracteres"));
                            }
                        }
                        catch (Exception exRead)
                        {
                            System.Diagnostics.Debug.WriteLine("Erro ao ler InputStream: " + exRead.Message);
                            System.Diagnostics.Debug.WriteLine("Tipo: " + exRead.GetType().Name);
                            // Continuar - vamos tentar outras formas
                        }
                        
                        // Se conseguiu ler o body, deserializar
                        if (!string.IsNullOrEmpty(body) && body.Trim().StartsWith("{"))
                        {
                            try
                            {
                                var serializer = new JavaScriptSerializer();
                                bodyData = serializer.Deserialize<Dictionary<string, object>>(body);
                                System.Diagnostics.Debug.WriteLine("BodyData deserializado: " + bodyData.Count + " chaves");
                            }
                            catch (Exception exDeserialize)
                            {
                                System.Diagnostics.Debug.WriteLine("Erro ao deserializar JSON: " + exDeserialize.Message);
                                System.Diagnostics.Debug.WriteLine("Body: " + body.Substring(0, Math.Min(200, body.Length)));
                                // Não re-lançar - vamos tentar QueryString/Form
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Erro geral ao processar JSON: " + ex.Message);
                        System.Diagnostics.Debug.WriteLine("Stack Trace: " + ex.StackTrace);
                    }
                }
                
                // Obter parâmetros - priorizar body JSON, depois QueryString/Form
                string acao = "";
                string componente = "";
                string valor = "";
                
                if (bodyData != null)
                {
                    acao = bodyData.ContainsKey("acao") ? (bodyData["acao"]?.ToString() ?? "") : "";
                    componente = bodyData.ContainsKey("componente") ? (bodyData["componente"]?.ToString() ?? "") : "";
                    valor = bodyData.ContainsKey("valor") ? (bodyData["valor"]?.ToString() ?? "") : "";
                }
                
                // Se não leu do body, tentar QueryString ou Form
                if (string.IsNullOrEmpty(acao))
                {
                    acao = context.Request["acao"] ?? context.Request.Form["acao"] ?? "";
                    componente = context.Request["componente"] ?? context.Request.Form["componente"] ?? "";
                    valor = context.Request["valor"] ?? context.Request.Form["valor"] ?? "";
                    
                    System.Diagnostics.Debug.WriteLine("Ação obtida de QueryString/Form: " + acao);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Ação obtida do Body JSON: " + acao);
                }
                
                if (string.IsNullOrEmpty(acao))
                {
                    System.Diagnostics.Debug.WriteLine("ERRO: Nenhuma ação foi encontrada!");
                    System.Diagnostics.Debug.WriteLine("QueryString Keys: " + string.Join(", ", context.Request.QueryString.AllKeys));
                    System.Diagnostics.Debug.WriteLine("Form Keys: " + string.Join(", ", context.Request.Form.AllKeys));
                }
                
                // Processar baseado na ação
                object resultado = null;
                
                switch (acao.ToLower())
                {
                    case "salvardados":
                        resultado = SalvarDados(componente, valor, context);
                        break;
                    
                    case "verificarcliente":
                        string login = ObterParametroDoBodyOuRequest(context, bodyData, "login");
                        resultado = VerificarCliente(login, context);
                        break;
                    
                    case "validarsenha":
                        login = ObterParametroDoBodyOuRequest(context, bodyData, "login");
                        string senha = ObterParametroDoBodyOuRequest(context, bodyData, "senha");
                        resultado = ValidarSenha(login, senha, context);
                        break;
                    
                    case "fazerlogin":
                        int clienteId = 0;
                        int.TryParse(ObterParametroDoBodyOuRequest(context, bodyData, "clienteId") ?? "0", out clienteId);
                        resultado = FazerLogin(clienteId, context);
                        break;
                    
                    case "obterprodutosdisponiveis":
                        string produtosPermitidosJson = ObterParametroDoBodyOuRequest(context, bodyData, "produtosPermitidosJson");
                        resultado = ObterProdutosDisponiveis(produtosPermitidosJson, context);
                        break;
                    
                    case "salvarnavegacao":
                        try
                        {
                            string url = ObterParametroDoBodyOuRequest(context, bodyData, "url");
                            string estadoJson = ObterParametroDoBodyOuRequest(context, bodyData, "estado");
                            
                            System.Diagnostics.Debug.WriteLine("SalvarNavegacao chamado - URL: " + url);
                            System.Diagnostics.Debug.WriteLine("Estado JSON length: " + (estadoJson?.Length ?? 0));
                            
                            resultado = SalvarNavegacao(url, estadoJson, context);
                        }
                        catch (Exception exSalvar)
                        {
                            System.Diagnostics.Debug.WriteLine("ERRO em SalvarNavegacao: " + exSalvar.Message);
                            System.Diagnostics.Debug.WriteLine("Stack Trace: " + exSalvar.StackTrace);
                            throw; // Re-lançar para ser capturado pelo catch externo
                        }
                        break;
                    
                    case "obterpaginaanterior":
                        resultado = ObterPaginaAnterior(context);
                        break;
                    
                    case "obterestadopagina":
                        string urlEstado = ObterParametroDoBodyOuRequest(context, bodyData, "url");
                        resultado = ObterEstadoPagina(urlEstado, context);
                        break;
                    
                    default:
                        resultado = new { sucesso = false, mensagem = "Ação não reconhecida: " + acao };
                        break;
                }
                
                // Retornar JSON
                try
                {
                    if (resultado == null)
                    {
                        System.Diagnostics.Debug.WriteLine("AVISO: resultado é null!");
                        resultado = new { sucesso = false, mensagem = "Resultado é null" };
                    }
                    
                    var serializer = new JavaScriptSerializer();
                    string json = serializer.Serialize(resultado);
                    context.Response.Write(json);
                }
                catch (Exception exSerialize)
                {
                    System.Diagnostics.Debug.WriteLine("ERRO ao serializar resultado: " + exSerialize.Message);
                    System.Diagnostics.Debug.WriteLine("Stack Trace: " + exSerialize.StackTrace);
                    // Tentar retornar erro simples
                    try
                    {
                        context.Response.Write("{\"sucesso\":false,\"mensagem\":\"Erro ao serializar resposta: " + exSerialize.Message.Replace("\"", "\\\"") + "\"}");
                    }
                    catch
                    {
                        throw; // Re-lançar se não conseguir escrever resposta
                    }
                }
            }
            catch (Exception ex)
            {
                // Log detalhado do erro para identificar a causa raiz
                System.Diagnostics.Debug.WriteLine("=== ERRO NO CALLBACKHANDLER ===");
                System.Diagnostics.Debug.WriteLine("Mensagem: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Tipo: " + ex.GetType().Name);
                System.Diagnostics.Debug.WriteLine("Stack Trace: " + ex.StackTrace);
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine("Inner Exception: " + ex.InnerException.Message);
                    System.Diagnostics.Debug.WriteLine("Inner Stack Trace: " + ex.InnerException.StackTrace);
                }
                System.Diagnostics.Debug.WriteLine("==============================");
                
                // Re-lançar o erro para que seja reportado corretamente
                // NÃO mascarar erros - deixar o ASP.NET retornar o status HTTP correto
                throw;
            }
        }
        
        /// <summary>
        /// Obtém parâmetro da requisição (body JSON, Form ou QueryString)
        /// </summary>
        private string ObterParametroDoBodyOuRequest(HttpContext context, Dictionary<string, object> bodyData, string nome)
        {
            try
            {
                // Se bodyData foi fornecido, tentar obter dele primeiro
                if (bodyData != null && bodyData.ContainsKey(nome))
                {
                    var valor = bodyData[nome];
                    return valor?.ToString() ?? "";
                }
                
                // Tentar QueryString
                if (context?.Request?.QueryString != null)
                {
                    string valor = context.Request.QueryString[nome];
                    if (!string.IsNullOrEmpty(valor)) return valor;
                }
                
                // Tentar Form
                if (context?.Request?.Form != null)
                {
                    string valor = context.Request.Form[nome];
                    if (!string.IsNullOrEmpty(valor)) return valor;
                }
                
                // Tentar Request direto (pode pegar de qualquer lugar)
                if (context?.Request != null)
                {
                    string valor = context.Request[nome];
                    if (!string.IsNullOrEmpty(valor)) return valor;
                }
                
                return "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro em ObterParametroDoBodyOuRequest para '" + nome + "': " + ex.Message);
                return "";
            }
        }
        
        private object SalvarDados(string componente, string valor, HttpContext context)
        {
            // Salvar na Session
            context.Session[componente] = valor;
            
            // Retornar JavaScript para executar no cliente
            return new
            {
                sucesso = true,
                javascript = $"mostrarMensagem('{componente} salvo!'); atualizarUI();"
            };
        }
        
        private object VerificarCliente(string login, HttpContext context)
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
            
            System.Diagnostics.Debug.WriteLine("=== VERIFICARCLIENTE ===");
            System.Diagnostics.Debug.WriteLine("Login recebido: " + loginLimpo);
            System.Diagnostics.Debug.WriteLine("É email: " + isEmail);
            
            if (isEmail)
            {
                cliente = databaseService.ObterClientePorEmail(loginLimpo.ToLowerInvariant());
                System.Diagnostics.Debug.WriteLine("Cliente encontrado por email: " + (cliente != null));
            }
            else
            {
                // Remover todos os caracteres não numéricos
                string telefoneFormatado = System.Text.RegularExpressions.Regex.Replace(loginLimpo, @"[^\d]", "");
                System.Diagnostics.Debug.WriteLine("Telefone formatado: " + telefoneFormatado);
                System.Diagnostics.Debug.WriteLine("Tamanho do telefone: " + telefoneFormatado.Length);
                
                if (telefoneFormatado.Length >= 10)
                {
                    cliente = databaseService.ObterClientePorTelefone(telefoneFormatado);
                    System.Diagnostics.Debug.WriteLine("Cliente encontrado por telefone: " + (cliente != null));
                    if (cliente != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Cliente ID: " + cliente.Id);
                        System.Diagnostics.Debug.WriteLine("Cliente Nome: " + cliente.Nome);
                        System.Diagnostics.Debug.WriteLine("Cliente Telefone: " + cliente.Telefone);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Telefone muito curto (mínimo 10 dígitos)");
                }
            }
            
            if (cliente != null)
            {
                System.Diagnostics.Debug.WriteLine("Cliente encontrado - ID: " + cliente.Id + ", Nome: " + cliente.Nome);
                return new
                {
                    existe = true,
                    encontrado = true, // Compatibilidade com JavaScript
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
            
            System.Diagnostics.Debug.WriteLine("Cliente NÃO encontrado");
            return new { existe = false, encontrado = false, temSenha = false, cliente = (object)null };
        }
        
        private object ValidarSenha(string login, string senha, HttpContext context)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(senha))
            {
                return new { valido = false, mensagem = "Login e senha são obrigatórios" };
            }
            
            var databaseService = new DatabaseService();
            Cliente cliente = null;
            
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
            
            // Validar senha usando SHA256 (mesmo método usado no Default.aspx.cs)
            string hashSenha = HashSenha(senha);
            bool senhaValida = hashSenha == cliente.Senha;
            
            if (senhaValida)
            {
                return new
                {
                    valido = true,
                    mensagem = "Senha válida",
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
            
            return new { valido = false, mensagem = "Senha inválida" };
        }
        
        private object FazerLogin(int clienteId, HttpContext context)
        {
            if (clienteId <= 0)
            {
                return new { sucesso = false, mensagem = "ID do cliente inválido" };
            }
            
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
            context.Session["ClienteId"] = cliente.Id;
            context.Session["ClienteNome"] = cliente.Nome;
            context.Session["ClienteEmail"] = cliente.Email;
            context.Session["IsAdmin"] = cliente.IsAdmin;
            if (!string.IsNullOrEmpty(cliente.Telefone))
            {
                context.Session["ClienteTelefone"] = cliente.Telefone;
            }
            context.Session["SessionStartTime"] = DateTime.Now;
            
            // Registrar log de login
            try
            {
                string usuarioLog = LogService.ObterUsuarioAtual(context.Session);
                LogService.RegistrarLogin(usuarioLog, "CallbackHandler", $"Login via FazerLogin - Email: {cliente.Email}");
            }
            catch
            {
                // Se falhar ao logar, continuar
            }
            
            return new
            {
                sucesso = true,
                isAdmin = cliente.IsAdmin,
                mensagem = "Login realizado com sucesso",
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
        
        /// <summary>
        /// Hash de senha usando SHA256 (mesmo método usado no Default.aspx.cs)
        /// </summary>
        private string HashSenha(string senha)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
                return Convert.ToBase64String(hashedBytes);
            }
        }
        
        private object ObterProdutosDisponiveis(string produtosPermitidosJson, HttpContext context)
        {
            try
            {
                var databaseService = new DatabaseService();
                List<Produto> produtos;
                
                // Se produtosPermitidosJson foi especificado, filtrar por IDs
                if (!string.IsNullOrEmpty(produtosPermitidosJson))
                {
                    try
                    {
                        var serializer = new JavaScriptSerializer();
                        var produtosIds = serializer.Deserialize<List<int>>(produtosPermitidosJson);
                        produtos = databaseService.ObterProdutosPorIds(produtosIds);
                    }
                    catch
                    {
                        // Se falhar ao parsear, retornar todos os produtos não-sacos
                        produtos = databaseService.ObterProdutos().Where(p => !p.EhSacoPromocional).ToList();
                    }
                }
                else
                {
                    // Obter todos os produtos não-sacos
                    produtos = databaseService.ObterProdutos().Where(p => !p.EhSacoPromocional).ToList();
                }
                
                // Converter para formato JSON simples (mesmo formato do VerReserva.aspx.cs)
                var produtosJson = produtos.Select(p => new
                {
                    id = p.Id,
                    nome = p.Nome,
                    preco = p.Preco
                }).ToList();
                
                return produtosJson;
            }
            catch (Exception ex)
            {
                return new { erro = ex.Message };
            }
        }
        
        private object SalvarNavegacao(string url, string estadoJson, HttpContext context)
        {
            try
            {
                // Verificar se a sessão está disponível
                if (context == null || context.Session == null)
                {
                    return new { sucesso = false, mensagem = "Sessão não disponível", erro = "Context ou Session é null" };
                }
                
                // Validar URL primeiro
                if (string.IsNullOrEmpty(url))
                {
                    url = context.Request?.Path ?? "/Default.aspx";
                }
                
                // Validar URL
                if (string.IsNullOrEmpty(url))
                {
                    url = "/Default.aspx";
                }
                
                Dictionary<string, object> estado = null;
                if (!string.IsNullOrEmpty(estadoJson) && estadoJson.Trim() != "{}")
                {
                    try
                    {
                        var serializer = new JavaScriptSerializer();
                        // Tentar deserializar como Dictionary primeiro
                        try
                        {
                            estado = serializer.Deserialize<Dictionary<string, object>>(estadoJson);
                        }
                        catch (Exception ex)
                        {
                            // Se falhar, tentar como objeto genérico
                            try
                            {
                                var obj = serializer.Deserialize<object>(estadoJson);
                                if (obj is Dictionary<string, object>)
                                {
                                    estado = obj as Dictionary<string, object>;
                                }
                            }
                            catch (Exception ex2)
                            {
                                // Retornar erro detalhado
                                return new { sucesso = false, mensagem = "Erro ao deserializar estado JSON", erro = ex2.Message, stackTrace = ex2.StackTrace };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Retornar erro detalhado
                        return new { sucesso = false, mensagem = "Erro ao processar estado JSON", erro = ex.Message, stackTrace = ex.StackTrace };
                    }
                }
                
                // Tentar salvar navegação
                try
                {
                    NavigationHelper.SalvarPaginaAtual(context, url, estado);
                    
                    // VALIDAR: Verificar se o estado foi realmente salvo
                    var estadoSalvo = NavigationHelper.ObterEstadoPagina(context, url);
                    bool estadoFoiSalvo = (estado == null && estadoSalvo.Count == 0) || 
                                         (estado != null && estadoSalvo.Count > 0);
                    
                    if (!estadoFoiSalvo && estado != null && estado.Count > 0)
                    {
                        // Estado não foi salvo corretamente
                        return new { 
                            sucesso = false, 
                            mensagem = "Estado não foi salvo corretamente na sessão", 
                            aviso = "O estado pode não estar disponível na próxima requisição"
                        };
                    }
                    
                    return new { sucesso = true, estadoSalvo = estadoFoiSalvo };
                }
                catch (Exception ex)
                {
                    // Retornar erro detalhado para identificar o problema
                    return new { 
                        sucesso = false, 
                        mensagem = "Erro ao salvar navegação no NavigationHelper", 
                        erro = ex.Message, 
                        stackTrace = ex.StackTrace, 
                        tipoErro = ex.GetType().Name,
                        aviso = "O estado NÃO foi salvo e pode não estar disponível na próxima requisição"
                    };
                }
            }
            catch (Exception ex)
            {
                // Retornar erro detalhado para identificar o problema
                return new { sucesso = false, mensagem = "Erro geral em SalvarNavegacao", erro = ex.Message, stackTrace = ex.StackTrace, tipoErro = ex.GetType().Name };
            }
        }
        
        private object ObterPaginaAnterior(HttpContext context)
        {
            try
            {
                string urlAnterior = NavigationHelper.ObterPaginaAnterior(context);
                return new { sucesso = true, url = urlAnterior };
            }
            catch (Exception ex)
            {
                return new { sucesso = false, mensagem = ex.Message, url = "Default.aspx" };
            }
        }
        
        private object ObterEstadoPagina(string url, HttpContext context)
        {
            try
            {
                var estado = NavigationHelper.ObterEstadoPagina(context, url);
                return new { sucesso = true, estado = estado };
            }
            catch (Exception ex)
            {
                return new { sucesso = false, mensagem = ex.Message, estado = new Dictionary<string, object>() };
            }
        }
    }
}


