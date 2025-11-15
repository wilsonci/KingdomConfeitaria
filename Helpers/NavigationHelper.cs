using System;
using System.Collections.Generic;
using System.Web;
using System.Web.SessionState;

namespace KingdomConfeitaria.Helpers
{
    /// <summary>
    /// Helper para gerenciar navegação e histórico de páginas
    /// Preserva estado dos controles em Session
    /// </summary>
    public static class NavigationHelper
    {
        private const string SESSION_HISTORY = "NavigationHistory";
        private const string SESSION_STATE = "PageState_";

        /// <summary>
        /// Adiciona página atual ao histórico antes de navegar para outra página
        /// </summary>
        public static void SalvarPaginaAtual(HttpContext context, string urlAtual, Dictionary<string, object> estadoControles = null)
        {
            if (context?.Session == null) return;

            try
            {
                // Validar URL
                if (string.IsNullOrEmpty(urlAtual))
                {
                    urlAtual = context.Request?.Path ?? "/Default.aspx";
                }

                var historico = ObterHistorico(context);
                
                // Adicionar página atual ao histórico (se não for a última)
                if (historico.Count == 0 || historico[historico.Count - 1] != urlAtual)
                {
                    historico.Add(urlAtual);
                    
                    // Limitar histórico a 10 páginas
                    if (historico.Count > 10)
                    {
                        historico.RemoveAt(0);
                    }
                }

                // Salvar estado dos controles se fornecido
                // IMPORTANTE: Dictionary<string, object> não é serializável na sessão do ASP.NET
                // Preciso converter para Hashtable (serializável) ou serializar para JSON string
                if (estadoControles != null && estadoControles.Count > 0)
                {
                    bool salvouComSucesso = false;
                    Exception ultimoErro = null;
                    
                    try
                    {
                        // Opção 1: Converter para Hashtable (serializável)
                        var hashtable = new System.Collections.Hashtable();
                        foreach (var kvp in estadoControles)
                        {
                            // Converter valores para tipos serializáveis
                            object valorSerializavel = kvp.Value;
                            
                            // Se for um tipo complexo, converter para string
                            if (valorSerializavel != null && !(valorSerializavel is string) && 
                                !(valorSerializavel is int) && !(valorSerializavel is long) && 
                                !(valorSerializavel is bool) && !(valorSerializavel is decimal) &&
                                !(valorSerializavel is double) && !(valorSerializavel is float) &&
                                !(valorSerializavel is short) && !(valorSerializavel is byte))
                            {
                                valorSerializavel = valorSerializavel.ToString();
                            }
                            
                            hashtable[kvp.Key] = valorSerializavel;
                        }
                        
                        // Tentar salvar
                        context.Session[SESSION_STATE + urlAtual] = hashtable;
                        
                        // VALIDAR: Verificar se realmente foi salvo
                        var validacao = context.Session[SESSION_STATE + urlAtual];
                        if (validacao != null && validacao is System.Collections.Hashtable)
                        {
                            salvouComSucesso = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        ultimoErro = ex;
                        // Se falhar, tentar serializar para JSON string
                        try
                        {
                            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                            string estadoJson = serializer.Serialize(estadoControles);
                            context.Session[SESSION_STATE + urlAtual] = estadoJson;
                            
                            // VALIDAR: Verificar se realmente foi salvo
                            var validacao = context.Session[SESSION_STATE + urlAtual];
                            if (validacao != null && validacao is string)
                            {
                                salvouComSucesso = true;
                            }
                        }
                        catch (Exception ex2)
                        {
                            ultimoErro = ex2;
                        }
                    }
                    
                    // Se não salvou com sucesso, lançar exceção para ser tratada
                    if (!salvouComSucesso)
                    {
                        throw new Exception("Falha ao salvar estado na sessão. Erro: " + (ultimoErro?.Message ?? "Desconhecido"), ultimoErro);
                    }
                }

                context.Session[SESSION_HISTORY] = historico;
            }
            catch (Exception ex)
            {
                // Log detalhado do erro para identificar a causa raiz
                System.Diagnostics.Debug.WriteLine("=== ERRO NO NAVIGATIONHELPER.SALVARPAGINAATUAL ===");
                System.Diagnostics.Debug.WriteLine("Mensagem: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Tipo: " + ex.GetType().Name);
                System.Diagnostics.Debug.WriteLine("Stack Trace: " + ex.StackTrace);
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine("Inner Exception: " + ex.InnerException.Message);
                    System.Diagnostics.Debug.WriteLine("Inner Stack Trace: " + ex.InnerException.StackTrace);
                }
                System.Diagnostics.Debug.WriteLine("URL: " + urlAtual);
                System.Diagnostics.Debug.WriteLine("Estado Controles Count: " + (estadoControles?.Count ?? 0));
                System.Diagnostics.Debug.WriteLine("==================================================");
                
                // Re-lançar o erro - NÃO mascarar
                throw;
            }
        }

        /// <summary>
        /// Obtém URL da página anterior
        /// </summary>
        public static string ObterPaginaAnterior(HttpContext context)
        {
            if (context?.Session == null) return "Default.aspx";

            var historico = ObterHistorico(context);
            
            if (historico.Count < 2)
            {
                return "Default.aspx";
            }

            // Retornar penúltima página (última é a atual)
            return historico[historico.Count - 2];
        }

        /// <summary>
        /// Remove página atual do histórico (quando volta)
        /// </summary>
        public static void RemoverPaginaAtual(HttpContext context)
        {
            if (context?.Session == null) return;

            var historico = ObterHistorico(context);
            
            if (historico.Count > 0)
            {
                historico.RemoveAt(historico.Count - 1);
                context.Session[SESSION_HISTORY] = historico;
            }
        }

        /// <summary>
        /// Obtém estado salvo dos controles de uma página
        /// </summary>
        public static Dictionary<string, object> ObterEstadoPagina(HttpContext context, string url)
        {
            if (context?.Session == null) return new Dictionary<string, object>();

            try
            {
                var estadoSalvo = context.Session[SESSION_STATE + url];
                if (estadoSalvo == null) return new Dictionary<string, object>();
                
                // Pode ser Hashtable (se foi salvo como Hashtable) ou string JSON
                if (estadoSalvo is System.Collections.Hashtable hashtable)
                {
                    // Converter Hashtable de volta para Dictionary
                    var estado = new Dictionary<string, object>();
                    foreach (System.Collections.DictionaryEntry entry in hashtable)
                    {
                        estado[entry.Key.ToString()] = entry.Value;
                    }
                    return estado;
                }
                else if (estadoSalvo is string estadoJson)
                {
                    // Deserializar JSON string
                    try
                    {
                        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                        return serializer.Deserialize<Dictionary<string, object>>(estadoJson);
                    }
                    catch
                    {
                        return new Dictionary<string, object>();
                    }
                }
                else if (estadoSalvo is Dictionary<string, object> dict)
                {
                    // Já é Dictionary (caso raro, mas possível)
                    return dict;
                }
                
                return new Dictionary<string, object>();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Salva estado de um controle específico
        /// </summary>
        public static void SalvarEstadoControle(HttpContext context, string url, string nomeControle, object valor)
        {
            if (context?.Session == null) return;

            var estado = ObterEstadoPagina(context, url);
            estado[nomeControle] = valor;
            
            // Salvar usando o mesmo método que SalvarPaginaAtual (converte para Hashtable ou JSON)
            try
            {
                // Opção 1: Converter para Hashtable (serializável)
                var hashtable = new System.Collections.Hashtable();
                foreach (var kvp in estado)
                {
                    // Converter valores para tipos serializáveis
                    object valorSerializavel = kvp.Value;
                    
                    // Se for um tipo complexo, converter para string
                    if (valorSerializavel != null && !(valorSerializavel is string) && 
                        !(valorSerializavel is int) && !(valorSerializavel is long) && 
                        !(valorSerializavel is bool) && !(valorSerializavel is decimal) &&
                        !(valorSerializavel is double) && !(valorSerializavel is float) &&
                        !(valorSerializavel is short) && !(valorSerializavel is byte))
                    {
                        valorSerializavel = valorSerializavel.ToString();
                    }
                    
                    hashtable[kvp.Key] = valorSerializavel;
                }
                context.Session[SESSION_STATE + url] = hashtable;
            }
            catch
            {
                // Se falhar, tentar serializar para JSON string
                try
                {
                    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    string estadoJson = serializer.Serialize(estado);
                    context.Session[SESSION_STATE + url] = estadoJson;
                }
                catch
                {
                    // Se tudo falhar, não salvar (não crítico)
                }
            }
        }

        /// <summary>
        /// Obtém valor de um controle salvo
        /// </summary>
        public static object ObterValorControle(HttpContext context, string url, string nomeControle)
        {
            var estado = ObterEstadoPagina(context, url);
            return estado.ContainsKey(nomeControle) ? estado[nomeControle] : null;
        }

        /// <summary>
        /// Limpa histórico de navegação
        /// </summary>
        public static void LimparHistorico(HttpContext context)
        {
            if (context?.Session == null) return;
            context.Session[SESSION_HISTORY] = new List<string>();
        }

        /// <summary>
        /// Obtém histórico completo
        /// </summary>
        private static List<string> ObterHistorico(HttpContext context)
        {
            if (context?.Session == null) return new List<string>();

            try
            {
                var historico = context.Session[SESSION_HISTORY] as List<string>;
                return historico ?? new List<string>();
            }
            catch
            {
                // Se houver erro ao acessar a sessão, retornar lista vazia
                return new List<string>();
            }
        }

        /// <summary>
        /// Salva URL de retorno para redirecionamento após login
        /// </summary>
        public static void SalvarUrlRetorno(HttpContext context, string url)
        {
            if (context?.Session == null) return;
            context.Session["ReturnUrl"] = url;
        }

        /// <summary>
        /// Obtém URL de retorno salva
        /// </summary>
        public static string ObterUrlRetorno(HttpContext context)
        {
            if (context?.Session == null) return null;
            return context.Session["ReturnUrl"] as string;
        }

        /// <summary>
        /// Remove URL de retorno salva
        /// </summary>
        public static void RemoverUrlRetorno(HttpContext context)
        {
            if (context?.Session == null) return;
            context.Session.Remove("ReturnUrl");
        }
    }
}

