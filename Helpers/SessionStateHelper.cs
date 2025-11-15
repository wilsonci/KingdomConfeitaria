using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace KingdomConfeitaria.Helpers
{
    /// <summary>
    /// Helper para salvar e restaurar estado de controles em Session
    /// Garante que nenhum dado seja perdido durante navegação
    /// </summary>
    public static class SessionStateHelper
    {
        private const string SESSION_PAGE_STATE = "PageState_";
        private const string SESSION_CURRENT_PAGE = "CurrentPage";

        /// <summary>
        /// Salva estado de todos os controles da página atual
        /// </summary>
        public static void SalvarEstadoPagina(Page page, string pageUrl = null)
        {
            if (page?.Session == null) return;

            if (string.IsNullOrEmpty(pageUrl))
            {
                pageUrl = page.Request.Path;
            }

            var estado = new Dictionary<string, object>();

            // Salvar estado de todos os controles
            SalvarEstadoControles(page, estado);

            // Salvar na sessão - IMPORTANTE: Dictionary não é serializável, precisa converter
            if (estado != null && estado.Count > 0)
            {
                try
                {
                    // Converter para Hashtable (serializável)
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
                    page.Session[SESSION_PAGE_STATE + pageUrl] = hashtable;
                }
                catch
                {
                    // Se falhar, tentar serializar para JSON string
                    try
                    {
                        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                        string estadoJson = serializer.Serialize(estado);
                        page.Session[SESSION_PAGE_STATE + pageUrl] = estadoJson;
                    }
                    catch
                    {
                        // Se tudo falhar, não salvar estado (não crítico para postbacks)
                    }
                }
            }
            
            page.Session[SESSION_CURRENT_PAGE] = pageUrl;
        }

        /// <summary>
        /// Restaura estado de todos os controles da página atual
        /// </summary>
        public static void RestaurarEstadoPagina(Page page, string pageUrl = null)
        {
            if (page?.Session == null) return;

            if (string.IsNullOrEmpty(pageUrl))
            {
                pageUrl = page.Request.Path;
            }

            // Pode ser Hashtable, JSON string ou Dictionary (legado)
            var estadoSalvo = page.Session[SESSION_PAGE_STATE + pageUrl];
            if (estadoSalvo == null) return;
            
            Dictionary<string, object> estado = null;
            
            // Converter de volta para Dictionary
            if (estadoSalvo is System.Collections.Hashtable hashtable)
            {
                estado = new Dictionary<string, object>();
                foreach (System.Collections.DictionaryEntry entry in hashtable)
                {
                    estado[entry.Key.ToString()] = entry.Value;
                }
            }
            else if (estadoSalvo is string estadoJson)
            {
                try
                {
                    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    estado = serializer.Deserialize<Dictionary<string, object>>(estadoJson);
                }
                catch
                {
                    return; // Se falhar ao deserializar, não restaurar
                }
            }
            else if (estadoSalvo is Dictionary<string, object> dict)
            {
                estado = dict; // Caso legado
            }
            
            if (estado == null || estado.Count == 0) return;

            // Restaurar estado dos controles
            RestaurarEstadoControles(page, estado);
        }

        /// <summary>
        /// Salva estado de um controle específico
        /// </summary>
        public static void SalvarEstadoControle(Page page, string controleId, object valor, string pageUrl = null)
        {
            if (page?.Session == null) return;

            if (string.IsNullOrEmpty(pageUrl))
            {
                pageUrl = page.Request.Path;
            }

            // Obter estado existente (pode ser Hashtable, JSON string ou Dictionary)
            var estadoSalvo = page.Session[SESSION_PAGE_STATE + pageUrl];
            Dictionary<string, object> estado = null;
            
            if (estadoSalvo is System.Collections.Hashtable hashtable)
            {
                estado = new Dictionary<string, object>();
                foreach (System.Collections.DictionaryEntry entry in hashtable)
                {
                    estado[entry.Key.ToString()] = entry.Value;
                }
            }
            else if (estadoSalvo is string estadoJson)
            {
                try
                {
                    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    estado = serializer.Deserialize<Dictionary<string, object>>(estadoJson);
                }
                catch
                {
                    estado = new Dictionary<string, object>();
                }
            }
            else if (estadoSalvo is Dictionary<string, object> dict)
            {
                estado = dict;
            }
            else
            {
                estado = new Dictionary<string, object>();
            }

            estado[controleId] = valor;
            
            // Salvar de volta (convertendo para Hashtable ou JSON)
            try
            {
                var hashtableNovo = new System.Collections.Hashtable();
                foreach (var kvp in estado)
                {
                    object valorSerializavel = kvp.Value;
                    if (valorSerializavel != null && !(valorSerializavel is string) && 
                        !(valorSerializavel is int) && !(valorSerializavel is long) && 
                        !(valorSerializavel is bool) && !(valorSerializavel is decimal) &&
                        !(valorSerializavel is double) && !(valorSerializavel is float) &&
                        !(valorSerializavel is short) && !(valorSerializavel is byte))
                    {
                        valorSerializavel = valorSerializavel.ToString();
                    }
                    hashtableNovo[kvp.Key] = valorSerializavel;
                }
                page.Session[SESSION_PAGE_STATE + pageUrl] = hashtableNovo;
            }
            catch
            {
                try
                {
                    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    string estadoJson = serializer.Serialize(estado);
                    page.Session[SESSION_PAGE_STATE + pageUrl] = estadoJson;
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
        public static object ObterValorControle(Page page, string controleId, string pageUrl = null)
        {
            if (page?.Session == null) return null;

            if (string.IsNullOrEmpty(pageUrl))
            {
                pageUrl = page.Request.Path;
            }

            // Pode ser Hashtable, JSON string ou Dictionary (legado)
            var estadoSalvo = page.Session[SESSION_PAGE_STATE + pageUrl];
            if (estadoSalvo == null) return null;
            
            Dictionary<string, object> estado = null;
            
            if (estadoSalvo is System.Collections.Hashtable hashtable)
            {
                estado = new Dictionary<string, object>();
                foreach (System.Collections.DictionaryEntry entry in hashtable)
                {
                    estado[entry.Key.ToString()] = entry.Value;
                }
            }
            else if (estadoSalvo is string estadoJson)
            {
                try
                {
                    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    estado = serializer.Deserialize<Dictionary<string, object>>(estadoJson);
                }
                catch
                {
                    return null;
                }
            }
            else if (estadoSalvo is Dictionary<string, object> dict)
            {
                estado = dict;
            }
            
            if (estado == null || !estado.ContainsKey(controleId))
            {
                return null;
            }

            return estado[controleId];
        }

        /// <summary>
        /// Limpa estado de uma página específica
        /// </summary>
        public static void LimparEstadoPagina(Page page, string pageUrl = null)
        {
            if (page?.Session == null) return;

            if (string.IsNullOrEmpty(pageUrl))
            {
                pageUrl = page.Request.Path;
            }

            page.Session.Remove(SESSION_PAGE_STATE + pageUrl);
        }

        /// <summary>
        /// Limpa estado de todas as páginas
        /// </summary>
        public static void LimparTodoEstado(Page page)
        {
            if (page?.Session == null) return;

            var keysToRemove = new List<string>();
            foreach (string key in page.Session.Keys)
            {
                if (key.StartsWith(SESSION_PAGE_STATE))
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                page.Session.Remove(key);
            }
        }

        /// <summary>
        /// Salva estado recursivamente de todos os controles
        /// </summary>
        private static void SalvarEstadoControles(Control parent, Dictionary<string, object> estado)
        {
            foreach (Control controle in parent.Controls)
            {
                // Ignorar controles sem ID
                if (string.IsNullOrEmpty(controle.ID)) continue;

                try
                {
                    // TextBox, HtmlInputText, HtmlInputHidden
                    if (controle is TextBox textBox)
                    {
                        estado[controle.ID] = textBox.Text;
                    }
                    else if (controle is HtmlInputText htmlInputText)
                    {
                        estado[controle.ID] = htmlInputText.Value;
                    }
                    else if (controle is HtmlInputHidden htmlInputHidden)
                    {
                        estado[controle.ID] = htmlInputHidden.Value;
                    }
                    else if (controle is HtmlTextArea htmlTextArea)
                    {
                        estado[controle.ID] = htmlTextArea.Value;
                    }
                    // CheckBox, HtmlInputCheckBox
                    else if (controle is CheckBox checkBox)
                    {
                        estado[controle.ID] = checkBox.Checked;
                    }
                    else if (controle is HtmlInputCheckBox htmlCheckBox)
                    {
                        estado[controle.ID] = htmlCheckBox.Checked;
                    }
                    // RadioButton
                    else if (controle is RadioButton radioButton)
                    {
                        estado[controle.ID] = radioButton.Checked;
                    }
                    // DropDownList, HtmlSelect
                    else if (controle is DropDownList dropDownList)
                    {
                        estado[controle.ID] = dropDownList.SelectedValue;
                    }
                    else if (controle is HtmlSelect htmlSelect)
                    {
                        estado[controle.ID] = htmlSelect.Value;
                    }
                    // ListBox
                    else if (controle is ListBox listBox)
                    {
                        var selectedValues = new List<string>();
                        foreach (ListItem item in listBox.Items)
                        {
                            if (item.Selected)
                            {
                                selectedValues.Add(item.Value);
                            }
                        }
                        estado[controle.ID] = string.Join(",", selectedValues);
                    }
                }
                catch
                {
                    // Ignorar erros ao salvar estado de controles específicos
                }

                // Recursão para controles filhos
                if (controle.HasControls())
                {
                    SalvarEstadoControles(controle, estado);
                }
            }
        }

        /// <summary>
        /// Restaura estado recursivamente de todos os controles
        /// </summary>
        private static void RestaurarEstadoControles(Control parent, Dictionary<string, object> estado)
        {
            foreach (Control controle in parent.Controls)
            {
                // Ignorar controles sem ID
                if (string.IsNullOrEmpty(controle.ID)) continue;

                if (!estado.ContainsKey(controle.ID)) continue;

                try
                {
                    var valor = estado[controle.ID];
                    if (valor == null) continue;

                    // TextBox, HtmlInputText, HtmlInputHidden
                    if (controle is TextBox textBox)
                    {
                        textBox.Text = valor.ToString();
                    }
                    else if (controle is HtmlInputText htmlInputText)
                    {
                        htmlInputText.Value = valor.ToString();
                    }
                    else if (controle is HtmlInputHidden htmlInputHidden)
                    {
                        htmlInputHidden.Value = valor.ToString();
                    }
                    else if (controle is HtmlTextArea htmlTextArea)
                    {
                        htmlTextArea.Value = valor.ToString();
                    }
                    // CheckBox, HtmlInputCheckBox
                    else if (controle is CheckBox checkBox)
                    {
                        if (valor is bool boolVal)
                        {
                            checkBox.Checked = boolVal;
                        }
                        else if (bool.TryParse(valor.ToString(), out bool parsedBool))
                        {
                            checkBox.Checked = parsedBool;
                        }
                    }
                    else if (controle is HtmlInputCheckBox htmlCheckBox)
                    {
                        if (valor is bool boolVal)
                        {
                            htmlCheckBox.Checked = boolVal;
                        }
                        else if (bool.TryParse(valor.ToString(), out bool parsedBool))
                        {
                            htmlCheckBox.Checked = parsedBool;
                        }
                    }
                    // RadioButton
                    else if (controle is RadioButton radioButton)
                    {
                        if (valor is bool boolVal)
                        {
                            radioButton.Checked = boolVal;
                        }
                        else if (bool.TryParse(valor.ToString(), out bool parsedBool))
                        {
                            radioButton.Checked = parsedBool;
                        }
                    }
                    // DropDownList, HtmlSelect
                    else if (controle is DropDownList dropDownList)
                    {
                        string valorStr = valor.ToString();
                        if (dropDownList.Items.FindByValue(valorStr) != null)
                        {
                            dropDownList.SelectedValue = valorStr;
                        }
                    }
                    else if (controle is HtmlSelect htmlSelect)
                    {
                        string valorStr = valor.ToString();
                        if (htmlSelect.Items.FindByValue(valorStr) != null)
                        {
                            htmlSelect.Value = valorStr;
                        }
                    }
                    // ListBox
                    else if (controle is ListBox listBox)
                    {
                        string valorStr = valor.ToString();
                        var valores = valorStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (ListItem item in listBox.Items)
                        {
                            item.Selected = valores.Contains(item.Value);
                        }
                    }
                }
                catch
                {
                    // Ignorar erros ao restaurar estado de controles específicos
                }

                // Recursão para controles filhos
                if (controle.HasControls())
                {
                    RestaurarEstadoControles(controle, estado);
                }
            }
        }
    }
}

