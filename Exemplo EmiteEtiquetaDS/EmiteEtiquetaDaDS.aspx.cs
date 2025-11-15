using DevExpress.Web;
using N_BPMSCI;
using PortalInterno.Configuracoes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Util;

/*
 *Objetivo: Imprimir as etiquetas das DSs.
 * Pode imprimir de uma localidade toda e suas decendencias e x niveis desejados
 *
 *
 * Proximas atividades:
 *
 *
 */

namespace PortalInterno.Etiquetas
{
    public partial class EmiteEtiquetaDaDS : PaginaConectada

    {
        #region Properties

        /// <summary>
        ///     Nó que foi selecionado para impressao
        /// </summary>
        public int DsSelecionada
        {
            get
            {
                if (HttpContext.Current.Session["DsSelecionada"] != null)
                    return Convert.ToInt32(HttpContext.Current.Session["DsSelecionada"]
                        .ToString());

                return 0;
            }
            set => HttpContext.Current.Session["DsSelecionada"] = value;
        }

        /// <summary>
        ///     Altura da etiqueta a ser impressa
        /// </summary>
        public int EtiquetaAltura
        {
            get
            {
                if (HttpContext.Current.Session["EtiquetaAltura"] != null)
                    return Convert.ToInt32(HttpContext.Current.Session["EtiquetaAltura"].ToString());

                return 0;
            }
            set => HttpContext.Current.Session["EtiquetaAltura"] = value;
        }

        /// <summary>
        ///     Codigo do produto do estoque a imprimir na etiqueta
        /// </summary>
        public string EtiquetaCodigoParaGerarCodigoDeBarras
        {
            get
            {
                if (HttpContext.Current.Session["EtiquetaCodigoParaGerarCodigoDeBarras"] != null)
                    return HttpContext.Current.Session["EtiquetaCodigoParaGerarCodigoDeBarras"].ToString();

                return null;
            }
            set => HttpContext.Current.Session["EtiquetaCodigoParaGerarCodigoDeBarras"] = value;
        }

        /// <summary>
        ///     Quantidade de etiquetas que tem em cada linha no rolo de etiquetas que esta na impressora
        /// </summary>
        public int EtiquetaEspacoEntreElas
        {
            get
            {
                if (HttpContext.Current.Session["EtiquetaEspacoEntreElas"] != null)
                    return Convert.ToInt32(HttpContext.Current.Session["EtiquetaEspacoEntreElas"].ToString());

                return 0;
            }
            set => HttpContext.Current.Session["EtiquetaEspacoEntreElas"] = value;
        }

        public string EtiquetaImagemAImprimir
        {
            get
            {
                if (HttpContext.Current.Session["EtiquetaImagemAImprimir"] != null)
                    return HttpContext.Current.Session["EtiquetaImagemAImprimir"].ToString();

                return "";
            }
            set => HttpContext.Current.Session["EtiquetaImagemAImprimir"] = value;
        }

        /// <summary>
        ///     Largura da etiqueta que será impressa
        /// </summary>
        public int EtiquetaLargura
        {
            get
            {
                if (HttpContext.Current.Session["EtiquetaLargura"] != null)
                    return Convert.ToInt32(HttpContext.Current.Session["EtiquetaLargura"].ToString());

                return 0;
            }
            set => HttpContext.Current.Session["EtiquetaLargura"] = value;
        }

        /// <summary>
        ///     Nome do produto que será impresso na etiqueta
        /// </summary>
        public string EtiquetaNomeASerImpresso
        {
            get
            {
                if (HttpContext.Current.Session["EtiquetaNomeASerImpresso"] != null)
                    return HttpContext.Current.Session["EtiquetaNomeASerImpresso"].ToString();

                return "";
            }
            set => HttpContext.Current.Session["EtiquetaNomeASerImpresso"] = value;
        }

        /// <summary>
        ///     Quantidade de etiquetas que serao impressas
        /// </summary>
        public int EtiquetaQtAImprimir
        {
            get
            {
                if (HttpContext.Current.Session["EtiquetaQtAImprimir"] != null)
                    return Convert.ToInt32(HttpContext.Current.Session["EtiquetaQtAImprimir"].ToString());

                return 0;
            }
            set => HttpContext.Current.Session["EtiquetaQtAImprimir"] = value;
        }

        /// <summary>
        ///     Quantidade de etiquetas que tem em cada linha no rolo de etiquetas que esta na impressora
        /// </summary>
        public int EtiquetaQtPorLinha
        {
            get
            {
                if (HttpContext.Current.Session["EtiquetaQtPorLinha"] != null)
                    return Convert.ToInt32(HttpContext.Current.Session["EtiquetaQtPorLinha"].ToString());

                return 0;
            }
            set => HttpContext.Current.Session["EtiquetaQtPorLinha"] = value;
        }

        /// <summary>
        /// Armazena os dados do combobox pois nao tem viewstate
        /// </summary>
        public List<DsResumida> ListaDeDsParaCbb
        {
            get
            {
                if (HttpContext.Current.Session["ListaDeDsParaCbb"] != null)
                    return HttpContext.Current.Session["ListaDeDsParaCbb"] as List<DsResumida>;

                return new List<DsResumida>();
            }
            set => HttpContext.Current.Session["ListaDeDsParaCbb"] = value;
        }

        /// <summary>
        ///     Quantidade de decendentes do nó selecionado
        /// </summary>
        public int NoTotDecendentes
        {
            get
            {
                if (HttpContext.Current.Session["NoTotDecendentes"] != null)
                    return Convert.ToInt32(HttpContext.Current.Session["NoTotDecendentes"].ToString());

                return 0;
            }
            set => HttpContext.Current.Session["NoTotDecendentes"] = value;
        }

        /// <summary>
        ///     Quantidade de nivesis de decendentes do nó selecionado
        /// </summary>
        public int NoTotNiveisDeDecendentes
        {
            get
            {
                if (HttpContext.Current.Session["NoTotNiveisDeDecendentes"] != null)
                    return Convert.ToInt32(HttpContext.Current.Session["NoTotNiveisDeDecendentes"].ToString());

                return 0;
            }
            set => HttpContext.Current.Session["NoTotNiveisDeDecendentes"] = value;
        }

        /// <summary>
        /// Numero da pagina a sermostrada na tela
        /// </summary>
        public int PaginaARetornar
        {
            get
            {
                if (HttpContext.Current.Session["PaginaARetornar"] != null)
                    return Convert.ToInt32(HttpContext.Current.Session["PaginaARetornar"]
                        .ToString());

                return 0;
            }
            set => HttpContext.Current.Session["PaginaARetornar"] = value;
        }

        /// <summary>
        /// Quantidade total de paginas que tem a tabela
        /// </summary>
        public int PaginaQtTotalDaTabela
        {
            get
            {
                if (HttpContext.Current.Session["PaginaQtTotalDaTabela"] != null)
                    return Convert.ToInt32(HttpContext.Current.Session["PaginaQtTotalDaTabela"]
                        .ToString());

                return 0;
            }
            set => HttpContext.Current.Session["PaginaQtTotalDaTabela"] = value;
        }

        /// <summary>
        /// O tamnho da pagina dos dados a ser mostrado na tela
        /// </summary>
        public int PaginaTamanho
        {
            get
            {
                if (HttpContext.Current.Session["PaginaTamanho"] != null)
                    return Convert.ToInt32(HttpContext.Current.Session["PaginaTamanho"]
                        .ToString());

                return 0;
            }
            set => HttpContext.Current.Session["PaginaTamanho"] = value;
        }

        #endregion Properties

        #region Methods

        //btnPrimeiraPg btnPagAnterior  txtPaginaAtual  btnProximaPagina btnUltimaPagina txtQtLinhasDaPagina lblQtPaginas
        protected void callbackSalvarSelecao_Callback(object source, CallbackEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Parameter))
            {
                var parametros = e.Parameter.Split('|');
                if (parametros.Length == 2)
                {
                    if (parametros[0] == "cbbSelecionaDs")
                    {
                        if (parametros[1] != null && parametros[1] != "null" && Convert.ToInt32(parametros[1]) > 0)
                        {
                            DsSelecionada = Convert.ToInt32(parametros[1]);
                            DsResumida ds = NDS.RetornaDadosResumidoDs(DsSelecionada).FirstOrDefault();
                            if (ds != null)
                            {
                                EtiquetaCodigoParaGerarCodigoDeBarras = ds.Numero;
                                EtiquetaNomeASerImpresso = ds.Equipamento;
                                EtiquetaImagemAImprimir = "data:image/png;base64," +
                                                          CodigoDeBarras.CriaCodigoDeBarras128EmBase64String(
                                                              EtiquetaCodigoParaGerarCodigoDeBarras,
                                                              EtiquetaNomeASerImpresso, EtiquetaLargura,
                                                              EtiquetaAltura);
                                e.Result = " var minhaDiv = document.getElementById('divDadosDs'); " +
                                           " minhaDiv.innerHTML = '</br><p>" + NDS.RetornaDadosResumidosDaDsParaApresentacao(DsSelecionada) + "</p>'; " +
                                           " imgEtiqueta.SetImageUrl('" + EtiquetaImagemAImprimir + "')";
                            }
                        }
                    }
                    else if (parametros[0] == "txtEtiquetaQtAImprimir")
                    {
                        EtiquetaQtAImprimir = Convert.ToInt32(parametros[1]);
                        e.Result = "";
                    }
                    else if (parametros[0] == "txtEtiquetaQtPorLinha")
                    {
                        EtiquetaQtPorLinha = Convert.ToInt32(parametros[1]);
                        e.Result = "";
                    }
                    else if (parametros[0] == "txtEtiquetaLargura")
                    {
                        EtiquetaLargura = Convert.ToInt32(parametros[1]);
                        e.Result = "";
                    }
                    else if (parametros[0] == "txtEtiquetaAltura")
                    {
                        EtiquetaAltura = Convert.ToInt32(parametros[1]);
                        e.Result = "";
                    }
                    else if (parametros[0] == "txtEtiquetaEspacoEntreEtiquetas")
                    {
                        EtiquetaEspacoEntreElas = Convert.ToInt32(parametros[1]);
                        e.Result = "";
                    }
                }
            }
        }

        protected void InicializaVariaveis()
        {
            DsSelecionada = 0;
            EtiquetaLargura = 33;
            EtiquetaAltura = 22;
            EtiquetaQtPorLinha = 3;
            EtiquetaQtAImprimir = 3;
            EtiquetaEspacoEntreElas = 3;
            EtiquetaNomeASerImpresso = "Teste";
            EtiquetaCodigoParaGerarCodigoDeBarras = "999999999";
            NoTotDecendentes = 0;
            NoTotNiveisDeDecendentes = 0;
            PaginaARetornar = 1;
            PaginaTamanho = 20;
            lblDadosDs.Text = NDS.RetornaDadosResumidosDaDsParaApresentacao(DsSelecionada);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                InicializaVariaveis();
                ListaDeDsParaCbb = NDS.RetornaListaDsResumidasFiltradas("ds", 100);
            }
            // carrega a cada leitura pois nao tem viewstate
            cbbSelecionaDs.DataSource = ListaDeDsParaCbb;
            cbbSelecionaDs.DataBind();
            if (DsSelecionada > 0)
                cbbSelecionaDs.Value = DsSelecionada;
            txtEtiquetaQtAImprimir.Text = EtiquetaQtAImprimir.ToString();
            txtEtiquetaQtPorLinha.Text = EtiquetaQtPorLinha.ToString();
            txtEtiquetaLargura.Text = EtiquetaLargura.ToString();
            txtEtiquetaAltura.Text = EtiquetaAltura.ToString();
            txtEtiquetaEspacoEntreEtiquetas.Text = EtiquetaEspacoEntreElas.ToString();
        }

        #endregion Methods

        protected void cbbSelecionaDs_OnItemRequestedByValue(object source, ListEditItemRequestedByValueEventArgs e)
        {
            int value = 0;
            if (e.Value == null || !Int32.TryParse(e.Value.ToString(), out value))
                return;

            var items = NDS.RetornaDadosResumidoDs(value);
            ListaDeDsParaCbb = items;
            cbbSelecionaDs.DataSource = items;
            cbbSelecionaDs.DataBind();
        }

        protected void cbbSelecionaDs_OnItemsRequestedByFilterCondition(object source, ListEditItemsRequestedByFilterConditionEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Filter))
            {
                var items = NDS.RetornaListaDsResumidasFiltradas(e.Filter, 10);
                ListaDeDsParaCbb = items;
                cbbSelecionaDs.DataSource = items;
                cbbSelecionaDs.DataBind();
            }
        }
    }
}