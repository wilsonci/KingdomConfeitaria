/**
 * Kingdom Confeitaria - JavaScript para página MinhasReservas
 * Funcionalidades específicas da área de reservas do cliente
 */

// Namespace para evitar conflitos
var MinhasReservasPage = MinhasReservasPage || {};

// Funções de compartilhamento
MinhasReservasPage.Compartilhar = {
    /**
     * Compartilhar no Facebook
     */
    facebook: function(url, texto) {
        window.open('https://www.facebook.com/sharer/sharer.php?u=' + encodeURIComponent(url) + '&quote=' + encodeURIComponent(texto), '_blank', 'width=600,height=400');
    },

    /**
     * Compartilhar no WhatsApp
     */
    whatsapp: function(url, texto) {
        window.open('https://wa.me/?text=' + encodeURIComponent(texto + ' ' + url), '_blank');
    },

    /**
     * Compartilhar no Twitter
     */
    twitter: function(url, texto) {
        window.open('https://twitter.com/intent/tweet?url=' + encodeURIComponent(url) + '&text=' + encodeURIComponent(texto), '_blank', 'width=600,height=400');
    }
};

// Funções globais para compatibilidade com onclick inline
function compartilharFacebook(url, texto) {
    MinhasReservasPage.Compartilhar.facebook(url, texto);
}

function compartilharWhatsApp(url, texto) {
    MinhasReservasPage.Compartilhar.whatsapp(url, texto);
}

function compartilharTwitter(url, texto) {
    MinhasReservasPage.Compartilhar.twitter(url, texto);
}

