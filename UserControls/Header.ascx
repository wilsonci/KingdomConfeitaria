<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Header.ascx.cs" Inherits="KingdomConfeitaria.UserControls.Header" %>

<div class="header-logo">
    <div class="header-top">
        <a id="linkLogo" runat="server" style="text-decoration: none; display: inline-block;">
            <img id="imgLogo" runat="server" alt="Kingdom Confeitaria" style="max-width: 200px; width: 100%; height: auto; display: block; cursor: pointer;" />
        </a>
        <div class="header-user-name" id="clienteNome" runat="server"></div>
    </div>
    <div class="header-actions">
        <a id="linkHome" runat="server"><i class="fas fa-home"></i> Home</a>
        <a id="linkLogin" runat="server"><i class="fas fa-sign-in-alt"></i> Entrar</a>
        <a id="linkMinhasReservas" runat="server"><i class="fas fa-clipboard-list"></i> Minhas Reservas</a>
        <a id="linkMeusDados" runat="server"><i class="fas fa-user"></i> Meus Dados</a>
        <a id="linkAdmin" runat="server"><i class="fas fa-cog"></i> Painel Gestor</a>
        <a id="linkLogout" runat="server"><i class="fas fa-sign-out-alt"></i> Sair</a>
    </div>
</div>

