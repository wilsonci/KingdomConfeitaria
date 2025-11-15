<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Footer.ascx.cs" Inherits="KingdomConfeitaria.UserControls.Footer" %>

<footer class="app-footer">
    <div class="footer-content">
        <div class="footer-section">
            <h5><i class="fas fa-crown"></i> Kingdom Confeitaria</h5>
            <p>Reserve seus biscoitos Ginger Bread com facilidade e praticidade.</p>
        </div>
        <div class="footer-section">
            <h5>Links Rápidos</h5>
            <ul>
                <li><a id="footerLinkHome" runat="server"><i class="fas fa-home"></i> Home</a></li>
                <li><a id="footerLinkReservas" runat="server"><i class="fas fa-clipboard-list"></i> Minhas Reservas</a></li>
                <li><a id="footerLinkDados" runat="server"><i class="fas fa-user"></i> Meus Dados</a></li>
            </ul>
        </div>
        <div class="footer-section">
            <h5>Contato</h5>
            <p><i class="fas fa-envelope"></i> contato@kingdomconfeitaria.com.br</p>
            <p><i class="fas fa-phone"></i> (62) 98137-3922</p>
        </div>
    </div>
    <div class="footer-bottom">
        <p id="footerCopyright" runat="server"></p>
    </div>
</footer>

