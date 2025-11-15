<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" EnableViewState="false" ViewStateMode="Disabled" AutoEventWireup="true" CodeBehind="EmiteEtiquetaDaDS.aspx.cs" Inherits="PortalInterno.Etiquetas.EmiteEtiquetaDaDS" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .divCabecalho {
            border: 1px solid aqua; /* Define uma borda sólida de 1px de espessura e cor preta */
            display: inline-block; /* ou display: inline; */
            padding: 10px; /* Espaçamento interno */
            margin: 2px;
            background: darkblue;
        }

        .divParametros {
            margin-top: 20px;
            display: inline-block; /* ou display: inline; */
            border: 1px solid aqua; /* Define uma borda sólida de 1px de espessura e cor preta */
            padding: 10px; /* Espaçamento interno */
            background: darkblue;
        }
    </style>
    <script type="text/javascript">

        // no da tree selecionado
        var DsSelecionada = 0;

        /* Alteração de dados e memoria da pagina sem o viewState
         * É necessario um AspxcallBack callbackSalvarSelecao na pagina Aspx e um metodo oncallback callbackSalvarSelecao_Callback no servidor tratando as chamdas ao servidor da pagina
         */
        // recebe o retorno do callback para executar
        var retornoCallback = "";

        function NoRetornoDoCallback(s,e) {
            retornoCallback = e.result;
            if (retornoCallback) {
                /*debugger;*/
                console.log('Retorno do Callback -> ' + retornoCallback);
                eval(retornoCallback.replace(/(\n|\r\n)/g, '</br>'));
                /* debugger;*/
            }
            PainelAguardandoLeitura.Hide();
        }

        // pega o nome do componente e faz a chamada do callback para alteracao. E melhor automatico para evitar erros quando alterar o nome do componente
        // checa se o dado do textbox e valido antes de fazer o callbackd
        // ex: ClientSideEvents-TextChanged="function(s,e){if (s.isValid){ altera( s.name.substring(s.name.lastIndexOf('_') + 1),s.GetValue());}}"
        function altera(nomeDoComponente, valor) {
            callbackSalvarSelecao.PerformCallback(nomeDoComponente + '|' + valor);
            PainelAguardandoLeitura.Show();
            //debugger;
        }
    </script>
    <dx:ASPxCallback ID="callbackSalvarSelecao" ClientInstanceName="callbackSalvarSelecao" runat="server"
        OnCallback="callbackSalvarSelecao_Callback"
        EnableViewState="False"
        ClientSideEvents-CallbackComplete="NoRetornoDoCallback" ViewStateMode="Disabled">
    </dx:ASPxCallback>
    <h2>Emissao de Etiquetas para as DS</h2>
    <br />
    <div class="divCabecalho">
        <p>
            <h6>Selecione as etiquetas para impressão </h6>
            Configure os parametros da etiqueta.
        OBS: MELHOR IMPRIMIR UMA FILEIRA SÓ DA IMPRESSORA PRA TESTE ANTES
        </p>
    </div>

    <br />
    <br />
    <table>
        <tr>
            <td>

                <dx:ASPxComboBox ID="cbbSelecionaDs" ClientInstanceName="cbbSelecionaDs" ViewStateMode="Disabled" EnableViewState="False" runat="server"
                    ValueField="IdDs" TextField="Numero" TextFormatString="{0} {1} {2} {3}" Width="800px" DropDownStyle="DropDown" Font-Size="11" Font-Bold="True"
                    FilterMinLength="1" LoadDropDownOnDemand="True" IncrementalFilteringDelay="100" MaxLength="200" ClientEnabled="True" EnableClientSideAPI="True"
                    EnableCallbackMode="True" CallbackPageSize="10" IncrementalFilteringMode="Contains" ValueType="System.String" AutoPostBack="False"
                    Enabled="True" DropDownWidth="1000"
                    OnItemsRequestedByFilterCondition="cbbSelecionaDs_OnItemsRequestedByFilterCondition"
                    OnItemRequestedByValue="cbbSelecionaDs_OnItemRequestedByValue"
                    ClientSideEvents-SelectedIndexChanged="function(s,e){if (s.isValid){ altera( s.name.substring(s.name.lastIndexOf('_') + 1),(s.GetSelectedItem() ? s.GetSelectedItem().value : 0 ));}}">
                    <Columns>
                        <dx:ListBoxColumn FieldName="Numero" Width="100px" />
                        <dx:ListBoxColumn FieldName="Equipamento" Width="300px" />
                        <dx:ListBoxColumn FieldName="Cliente" Width="300px" />
                        <dx:ListBoxColumn FieldName="SituacaoAtual" Width="150px" />
                    </Columns>
                </dx:ASPxComboBox>
            </td>
        </tr>
        <tr>
            <td>
                <div id="divDadosDs" style="margin: 10px;">
                    <dx:ASPxLabel ID="lblDadosDs" ClientInstanceName="lblDadosDs" runat="server" Font-Bold="True" Font-Size="14"></dx:ASPxLabel>
                </div>
            </td>
            <td>
                <dx:ASPxImage ID="imgEtiqueta" ClientInstanceName="imgEtiqueta" runat="server" ShowLoadingImage="true" Width="150px" Height="75px">
                </dx:ASPxImage>
            </td>
        </tr>
    </table>

    <div class="divParametros">
        <table>
            <tr>
                <td>
                    <dx:ASPxTextBox ID="txtEtiquetaQtAImprimir" ClientInstanceName="txtEtiquetaQtAImprimir" HelpText="Qt a Imprimir" Font-Bold="True" HelpTextStyle-Font-Bold="True"
                        runat="server" Width="30px" AutoPostBack="false" ClientSideEvents-TextChanged="function(s,e){if (s.isValid){ altera( s.name.substring(s.name.lastIndexOf('_') + 1),s.GetValue());}}">
                        <MaskSettings Mask="099" />
                        <ValidationSettings>
                            <RequiredField IsRequired="true" ErrorText="Qt > 0" />
                            <RegularExpression ValidationExpression="^[1-9]\d*$" ErrorText="O número deve ser maior que zero." />
                        </ValidationSettings>
                    </dx:ASPxTextBox>
                </td>
                <td>
                    <dx:ASPxTextBox ID="txtEtiquetaQtPorLinha" ClientInstanceName="txtEtiquetaQtPorLinha" HelpText="Etiq por Linha" Font-Bold="True" HelpTextStyle-Font-Bold="True"
                        runat="server" Width="30px" AutoPostBack="false" ClientSideEvents-TextChanged="function(s,e){if (s.isValid){ altera( s.name.substring(s.name.lastIndexOf('_') + 1),s.GetValue());}}">
                        <MaskSettings Mask="0" />
                        <ValidationSettings>
                            <RequiredField IsRequired="true" ErrorText="Qt > 0" />
                            <RegularExpression ValidationExpression="^[1-3]\d*$" ErrorText="O número deve ser maior que zero e menor ou igual a 3." />
                        </ValidationSettings>
                    </dx:ASPxTextBox>
                </td>
                <td>
                    <dx:ASPxTextBox ID="txtEtiquetaLargura" ClientInstanceName="txtEtiquetaLargura" HelpText="Largura Etiq" Font-Bold="True" HelpTextStyle-Font-Bold="True"
                        runat="server" Width="30px" AutoPostBack="false" ClientSideEvents-TextChanged="function(s,e){if (s.isValid){ altera( s.name.substring(s.name.lastIndexOf('_') + 1),s.GetValue());}}">
                        <MaskSettings Mask="0999" />
                        <ValidationSettings>
                            <RequiredField IsRequired="true" ErrorText="Qt > 0" />
                            <RegularExpression ValidationExpression="^[1-9]\d*$" ErrorText="O número deve ser maior que zero." />
                        </ValidationSettings>
                    </dx:ASPxTextBox>
                </td>

                <td>
                    <dx:ASPxTextBox ID="txtEtiquetaAltura" ClientInstanceName="txtEtiquetaAltura" HelpText="Altura Etiq" Font-Bold="True" HelpTextStyle-Font-Bold="True"
                        runat="server" Width="30px" AutoPostBack="false" ClientSideEvents-TextChanged="function(s,e){if (s.isValid){ altera( s.name.substring(s.name.lastIndexOf('_') + 1),s.GetValue());}}">
                        <MaskSettings Mask="0999" />
                        <ValidationSettings>
                            <RequiredField IsRequired="true" ErrorText="Qt > 0" />
                            <RegularExpression ValidationExpression="^[1-9]\d*$" ErrorText="O número deve ser maior que zero." />
                        </ValidationSettings>
                    </dx:ASPxTextBox>
                </td>
                <td>
                    <dx:ASPxTextBox ID="txtEtiquetaEspacoEntreEtiquetas" ClientInstanceName="txtEtiquetaEspacoEntreEtiquetas" HelpText="Epaco Entre Etiq" Font-Bold="True" HelpTextStyle-Font-Bold="True"
                        runat="server" Width="30px" AutoPostBack="false" ClientSideEvents-TextChanged="function(s,e){if (s.isValid){ altera( s.name.substring(s.name.lastIndexOf('_') + 1),s.GetValue());}}">
                        <MaskSettings Mask="0999" />
                        <ValidationSettings>
                            <RequiredField IsRequired="true" ErrorText="Qt >= 0" />
                            <RegularExpression ValidationExpression="^[0-9]\d*$" ErrorText="O número deve ser maior ou igual a zero." />
                        </ValidationSettings>
                    </dx:ASPxTextBox>
                </td>
            </tr>
            <tr>
                <td>
                    <dx:ASPxButton ID="btnImprime" ClientInstanceName="btnImprime" runat="server" Text="Imprime as Etiquetas" Font-Bold="true" BackColor="Yellow" Width="200PX" AutoPostBack="False" ClientEnabled="True"
                        ClientSideEvents-Click="function(s,e){imprimirEtiquetas(
                        imgEtiqueta.GetImageUrl(), txtEtiquetaLargura.GetValue(),txtEtiquetaAltura.GetValue(),txtEtiquetaQtPorLinha.GetValue(),
                        txtEtiquetaEspacoEntreEtiquetas.GetValue(),txtEtiquetaQtAImprimir.GetValue());}">
                    </dx:ASPxButton>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>