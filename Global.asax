<%@ Application Language="C#" %>

<script runat="server">
    void Application_Start(object sender, EventArgs e)
    {
        // Código que é executado na inicialização do aplicativo
    }

    void Application_End(object sender, EventArgs e)
    {
        // Código que é executado no encerramento do aplicativo
    }

    void Application_Error(object sender, EventArgs e)
    {
        // Código que é executado quando ocorre um erro não tratado
    }

    void Session_Start(object sender, EventArgs e)
    {
        // Código que é executado quando uma nova sessão é iniciada
        Session["Carrinho"] = new System.Collections.Generic.List<KingdomConfeitaria.Models.ItemPedido>();
    }

    void Session_End(object sender, EventArgs e)
    {
        // Código que é executado quando uma sessão termina
    }
</script>

