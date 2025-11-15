<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="KingdomConfeitaria.Default" EnableEventValidation="false" EnableViewState="false" ViewStateMode="Disabled" %>
<%@ Register Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" Namespace="System.Web.UI" TagPrefix="asp" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Kingdom Confeitaria - Reserva de Ginger Breads</title>
    <link rel="icon" type="image/svg+xml" href="Images/logo-kingdom-confeitaria.svg" />
    <link href="Content/bootstrap/bootstrap.min.css" rel="stylesheet" />
    <link href="Content/fontawesome/all.min.css?v=2.0" rel="stylesheet" />
    <link href="Content/app.css" rel="stylesheet" />
    <style>
        * {
            box-sizing: border-box;
        }
        body {
            background: linear-gradient(135deg, #f5f7fa 0%, #e9ecef 100%);
            min-height: 100vh;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            margin: 0;
            padding: 0;
            padding-bottom: 80px; /* Espaço para carrinho flutuante */
        }
        .header-logo {
            background: #ffffff;
            padding: 16px 20px;
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            width: 100%;
            z-index: 1000;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            border-bottom: 1px solid #e0e0e0;
        }
        .header-top {
            display: flex;
            justify-content: space-between;
            align-items: center;
            width: 100%;
            gap: 12px;
            flex-wrap: wrap;
        }
        .header-logo img {
            max-width: 200px;
            width: 100%;
            height: auto;
            display: block;
        }
        .header-user-name {
            color: #1a4d2e;
            font-weight: 600;
            font-size: 14px;
            text-align: right;
        }
        .header-actions {
            display: flex;
            gap: 8px;
            align-items: center;
            background: rgba(255, 255, 255, 0.95);
            padding: 8px 12px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            flex-wrap: wrap;
            justify-content: flex-end;
            max-width: 100%;
            overflow-x: auto;
            -webkit-overflow-scrolling: touch;
            scrollbar-width: none;
        }
        .header-actions::-webkit-scrollbar {
            display: none;
        }
        .header-actions a {
            color: #1a4d2e;
            text-decoration: none;
            font-size: 13px;
            font-weight: 600;
            padding: 6px 12px;
            border-radius: 6px;
            transition: all 0.2s;
            white-space: nowrap;
            flex-shrink: 0;
        }
        .header-actions a:hover {
            background: #1a4d2e;
            color: #fff;
        }
        /* Ícone do carrinho com badge */
        .carrinho-header {
            position: relative;
            display: inline-block;
            padding: 6px 10px;
            border-radius: 6px;
            transition: all 0.2s;
        }
        .carrinho-header:hover {
            background: #1a4d2e;
        }
        .carrinho-header:hover i {
            color: #fff;
        }
        .carrinho-header i {
            font-size: 20px;
            color: #1a4d2e;
            transition: color 0.3s;
        }
        .carrinho-header.com-itens i {
            color: #dc3545;
        }
        .carrinho-header.com-itens:hover i {
            color: #fff;
        }
        .carrinho-badge {
            position: absolute;
            top: 2px;
            right: 2px;
            background: #dc3545;
            color: white;
            border-radius: 50%;
            min-width: 18px;
            height: 18px;
            font-size: 10px;
            font-weight: 700;
            display: flex;
            align-items: center;
            justify-content: center;
            line-height: 1;
            padding: 0 4px;
            border: 2px solid white;
            box-shadow: 0 2px 4px rgba(0,0,0,0.2);
        }
        .carrinho-badge.oculto {
            display: none;
        }
        /* Ajustar badge para números maiores */
        .carrinho-badge:not(.oculto) {
            min-width: 18px;
            padding: 0 4px;
        }
        /* Estilos específicos do carrinho - responsivos */
        @media (max-width: 575.98px) {
            .carrinho-header i {
                font-size: 18px;
                display: inline-block !important;
            }
            .carrinho-badge {
                min-width: 16px;
                height: 16px;
                font-size: 9px;
                top: 1px;
                right: 1px;
            }
        }
        /* Estilos específicos da página Default */
        .container-main {
            background: transparent;
        }
        
        .header-spacer {
            height: auto;
            min-height: 60px;
            width: 100%;
        }
        
        @media (min-width: 992px) {
            .header-spacer {
                min-height: 80px;
            }
        }
        /* Hero Section */
        .hero-section {
            background: linear-gradient(135deg, #1a4d2e 0%, #2d5a3d 100%);
            color: white;
            padding: 40px 16px;
            text-align: center;
            margin-bottom: 30px;
        }
        .hero-title {
            font-size: 28px;
            font-weight: 700;
            margin-bottom: 12px;
            line-height: 1.2;
        }
        .hero-subtitle {
            font-size: 16px;
            opacity: 0.9;
            margin-bottom: 0;
            line-height: 1.5;
        }
        /* Seção de Produtos */
        .produtos-section {
            padding: 0 16px 20px 16px;
            flex: 1;
        }
        .produtos-wrapper {
            display: flex;
            flex-direction: column;
            gap: 20px;
        }
        .section-title {
            font-size: 22px;
            font-weight: 700;
            color: #333;
            margin-bottom: 16px;
            display: flex;
            align-items: center;
            gap: 8px;
        }
        .section-title i {
            color: #1a4d2e;
        }
        /* Carrossel de Produtos */
        .produtos-carrossel {
            position: relative;
            margin-bottom: 20px;
            background: #000000;
            border-radius: 16px;
            padding: 16px;
        }
        .carrossel-container {
            display: flex;
            overflow-x: auto;
            scroll-snap-type: x mandatory;
            -webkit-overflow-scrolling: touch;
            scrollbar-width: none;
            gap: 12px;
            padding: 0 0 12px 0;
        }
        .carrossel-container::-webkit-scrollbar {
            display: none;
        }
        /* Botões de navegação do carrossel */
        .carrossel-nav-btn {
            position: absolute;
            top: 50%;
            transform: translateY(-50%);
            width: 40px;
            height: 40px;
            background: rgba(255, 255, 255, 0.9);
            border: 2px solid #1a4d2e;
            border-radius: 50%;
            color: #1a4d2e;
            font-size: 18px;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 10;
            transition: all 0.3s ease;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
        }
        .carrossel-nav-btn:hover {
            background: #1a4d2e;
            color: white;
            transform: translateY(-50%) scale(1.1);
            box-shadow: 0 4px 12px rgba(26, 77, 46, 0.4);
        }
        .carrossel-nav-btn:active {
            transform: translateY(-50%) scale(0.95);
        }
        .carrossel-nav-btn.esquerda {
            left: 8px;
        }
        .carrossel-nav-btn.direita {
            right: 8px;
        }
        .carrossel-nav-btn.oculto {
            display: none;
        }
        @media (max-width: 768px) {
            .carrossel-nav-btn {
                width: 36px;
                height: 36px;
                font-size: 16px;
            }
            .carrossel-nav-btn.esquerda {
                left: 4px;
            }
            .carrossel-nav-btn.direita {
                right: 4px;
            }
        }
        .produto-card-carrossel {
            flex: 0 0 140px;
            background: white;
            border-radius: 12px;
            padding: 12px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.08);
            scroll-snap-align: start;
            transition: transform 0.2s, box-shadow 0.2s, border 0.2s;
            text-decoration: none;
            color: inherit;
            display: flex;
            flex-direction: column;
            border: 2px solid #ffffff;
            position: relative;
        }
        .produto-card-carrossel:hover {
            border-color: #1a4d2e;
            box-shadow: 0 4px 12px rgba(26, 77, 46, 0.3);
            transform: translateY(-2px);
        }
        .produto-imagem-carrossel-wrapper {
            position: relative;
            cursor: pointer;
            margin-bottom: 8px;
        }
        .produto-imagem-carrossel-wrapper:hover {
            opacity: 0.9;
        }
        /* Animação da mãozinha clicando */
        .maozinha-clique {
            position: absolute;
            z-index: 1000;
            pointer-events: none;
            opacity: 0;
            transition: opacity 0.3s ease;
        }
        .maozinha-clique.visivel {
            opacity: 1;
        }
        .maozinha-clique i {
            font-size: 40px;
            color: #ffffff;
            text-shadow: 0 2px 8px rgba(0, 0, 0, 0.5);
            animation: cliqueAnimacao 1.5s ease-in-out infinite;
        }
        @keyframes cliqueAnimacao {
            0%, 100% {
                transform: translateY(0) scale(1);
            }
            50% {
                transform: translateY(10px) scale(0.9);
            }
        }
        .produto-imagem-carrossel {
            width: 100%;
            height: 100px;
            object-fit: cover;
            border-radius: 8px;
            margin-bottom: 8px;
            background: #f0f0f0;
        }
        .produto-nome-carrossel {
            font-size: 13px;
            font-weight: 600;
            color: #333;
            margin-bottom: 4px;
            line-height: 1.3;
            display: -webkit-box;
            -webkit-line-clamp: 2;
            -webkit-box-orient: vertical;
            overflow: hidden;
        }
        .produto-preco-carrossel {
            font-size: 14px;
            font-weight: 700;
            color: #1a4d2e;
            margin-top: 4px;
            margin-bottom: 8px;
        }
        .btn-reservar-produto {
            width: 100%;
            padding: 8px 12px;
            background: linear-gradient(135deg, #1a4d2e 0%, #2d5a3d 100%);
            color: white;
            border: none;
            border-radius: 8px;
            font-size: 12px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.2s;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 6px;
            margin-top: auto;
        }
        .btn-reservar-produto:hover {
            background: linear-gradient(135deg, #2d5a3d 0%, #1a4d2e 100%);
            transform: translateY(-1px);
            box-shadow: 0 2px 8px rgba(26, 77, 46, 0.4);
        }
        .btn-reservar-produto:active {
            transform: translateY(0);
        }
        .btn-reservar-produto i {
            font-size: 14px;
        }
        /* Modal de Detalhe do Produto */
        .modal-produto-detalhe .modal-dialog {
            margin: 0;
            max-width: 100%;
            height: 100vh;
        }
        .modal-produto-detalhe .modal-content {
            border-radius: 0;
            border: none;
            height: 100vh;
            display: flex;
            flex-direction: column;
        }
        .modal-produto-detalhe .modal-header {
            border-bottom: 1px solid #e0e0e0;
            padding: 16px;
            background: white;
            position: sticky;
            top: 0;
            z-index: 10;
        }
        .modal-produto-detalhe .modal-body {
            flex: 1;
            overflow-y: auto;
            padding: 20px;
            padding-bottom: 100px;
            background: white;
        }
        .produto-imagem-detalhe {
            width: 100%;
            max-height: 300px;
            object-fit: contain;
            border-radius: 12px;
            margin-bottom: 20px;
            background: #f8f8f8;
        }
        .produto-nome-detalhe {
            font-size: 22px;
            font-weight: 700;
            color: #333;
            margin-bottom: 12px;
        }
        .produto-descricao-detalhe {
            font-size: 15px;
            color: #666;
            margin-bottom: 20px;
            line-height: 1.5;
        }
        .produto-preco-detalhe {
            font-size: 24px;
            font-weight: 700;
            color: #1a4d2e;
            margin-bottom: 24px;
        }
        .quantidade-selector {
            display: flex;
            align-items: center;
            gap: 16px;
            margin-bottom: 24px;
            padding: 16px;
            background: #f8f8f8;
            border-radius: 12px;
        }
        .quantidade-selector label {
            font-weight: 600;
            color: #333;
            margin: 0;
        }
        .quantidade-controls {
            display: flex;
            align-items: center;
            gap: 12px;
            margin-left: auto;
        }
        .btn-quantidade {
            width: 40px;
            height: 40px;
            border-radius: 50%;
            border: 2px solid #1a4d2e;
            background: white;
            color: #1a4d2e;
            font-size: 20px;
            font-weight: 600;
            display: flex;
            align-items: center;
            justify-content: center;
            cursor: pointer;
            transition: all 0.2s;
        }
        .btn-quantidade:active {
            background: #1a4d2e;
            color: white;
        }
        .quantidade-value {
            font-size: 18px;
            font-weight: 600;
            min-width: 30px;
            text-align: center;
        }
        .modal-produto-detalhe .modal-footer {
            border-top: 1px solid #e0e0e0;
            padding: 16px;
            background: white;
            display: flex;
            gap: 12px;
            justify-content: center;
            position: sticky;
            bottom: 0;
            z-index: 10;
            flex-shrink: 0;
        }
        .btn-adicionar-pedido {
            min-width: 120px;
            height: 45px;
            padding: 8px 16px;
            background: #28a745;
            color: white;
            border: none;
            border-radius: 8px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.2s;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 8px;
            flex-shrink: 0;
            box-shadow: 0 2px 4px rgba(0,0,0,0.2);
        }
        .btn-adicionar-pedido:hover {
            background: #218838;
            transform: scale(1.1);
            box-shadow: 0 3px 6px rgba(0,0,0,0.3);
        }
        .btn-adicionar-pedido:active {
            background: #1e7e34;
            transform: scale(0.95);
        }
        .btn-adicionar-pedido i,
        .btn-cancelar-pedido i {
            margin: 0;
            font-size: 16px;
        }
        .btn-cancelar-pedido {
            min-width: 120px;
            height: 45px;
            padding: 8px 16px;
            background: #dc3545;
            color: white;
            border: none;
            border-radius: 8px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.2s;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 8px;
            flex-shrink: 0;
            box-shadow: 0 2px 4px rgba(0,0,0,0.2);
        }
        .btn-cancelar-pedido:hover {
            background: #c82333;
            transform: scale(1.1);
            box-shadow: 0 3px 6px rgba(0,0,0,0.3);
        }
        .btn-cancelar-pedido:active {
            background: #bd2130;
            transform: scale(0.95);
        }
        .btn-cancelar-pedido i {
            margin: 0;
        }
        .preco-total-modal {
            font-size: 20px;
            font-weight: 700;
            color: #1a4d2e;
            text-align: center;
            margin-top: 16px;
            padding: 12px;
            background: #f8f8f8;
            border-radius: 8px;
        }
        /* Carrinho Flutuante Mobile */
        .carrinho-flutuante {
            position: fixed;
            bottom: 0;
            left: 0;
            right: 0;
            background: white;
            box-shadow: 0 -2px 10px rgba(0,0,0,0.1);
            z-index: 999;
            padding: 12px 16px;
            border-top: 1px solid #e0e0e0;
        }
        .carrinho-flutuante-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 12px;
        }
        .carrinho-flutuante-total {
            font-size: 18px;
            font-weight: 700;
            color: #333;
        }
        .carrinho-flutuante-itens {
            font-size: 13px;
            color: #666;
        }
        .btn-fazer-reserva {
            width: 100%;
            padding: 14px;
            background: #1a4d2e;
            color: white;
            border: none;
            border-radius: 8px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: background 0.2s;
        }
        .btn-fazer-reserva:disabled {
            background: #ccc;
            cursor: not-allowed;
        }
        .btn-fazer-reserva:active:not(:disabled) {
            background: #2d5a3d;
        }
        /* Modal de Carrinho */
        .modal-carrinho .modal-dialog {
            margin: 0;
            max-width: 100%;
            height: 80vh;
            margin-top: 20vh;
        }
        .modal-carrinho .modal-content {
            border-radius: 20px 20px 0 0;
            height: 100%;
            display: flex;
            flex-direction: column;
        }
        .modal-carrinho .modal-body {
            flex: 1;
            overflow-y: auto;
            padding: 20px;
        }
        /* Modal de Carrinho - Desktop */
        @media (min-width: 992px) {
            .modal-carrinho .modal-dialog {
                max-width: 800px;
                margin: 1.75rem auto;
                height: auto;
                max-height: 85vh;
            }
            .modal-carrinho .modal-content {
                border-radius: 12px;
                max-height: 85vh;
                height: auto;
            }
            .modal-carrinho .modal-body {
                max-height: calc(85vh - 120px);
            }
        }
        .item-carrinho {
            background: white;
            padding: 16px;
            border-radius: 12px;
            margin-bottom: 12px;
            border: 1px solid #e0e0e0;
            box-shadow: 0 2px 4px rgba(0,0,0,0.05);
        }
        .item-carrinho-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            margin-bottom: 12px;
        }
        .item-carrinho-info {
            flex: 1;
        }
        .item-carrinho-nome {
            font-weight: 600;
            color: #333;
            margin-bottom: 4px;
            font-size: 15px;
        }
        .item-carrinho-detalhes {
            font-size: 13px;
            color: #666;
            margin-bottom: 8px;
        }
        .item-carrinho-preco {
            font-weight: 700;
            color: #1a4d2e;
            font-size: 16px;
        }
        .item-carrinho-acoes {
            display: flex;
            flex-direction: column;
            gap: 8px;
            align-items: flex-end;
        }
        .item-carrinho-controles {
            display: flex;
            align-items: center;
            gap: 12px;
            margin-top: 8px;
        }
        .controle-quantidade {
            display: flex;
            align-items: center;
            gap: 8px;
            background: #f8f9fa;
            border-radius: 8px;
            padding: 4px;
        }
        .btn-quantidade-carrinho {
            width: 32px;
            height: 32px;
            border: none;
            border-radius: 6px;
            background: white;
            color: #1a4d2e;
            font-weight: 600;
            font-size: 18px;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
            transition: all 0.2s;
            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
        }
        .btn-quantidade-carrinho:hover {
            background: #1a4d2e;
            color: white;
            transform: scale(1.1);
        }
        .btn-quantidade-carrinho:active {
            transform: scale(0.95);
        }
        .quantidade-carrinho {
            min-width: 30px;
            text-align: center;
            font-weight: 600;
            color: #333;
            font-size: 16px;
        }
        .btn-acoes-item {
            display: flex;
            gap: 6px;
        }
        .btn-acoes-item button {
            padding: 6px 12px;
            border: none;
            border-radius: 6px;
            font-size: 12px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.2s;
            display: flex;
            align-items: center;
            gap: 4px;
        }
        .btn-adicionar-mais {
            background: #28a745;
            color: white;
        }
        .btn-adicionar-mais:hover {
            background: #218838;
            transform: translateY(-1px);
            box-shadow: 0 2px 4px rgba(40, 167, 69, 0.3);
        }
        .btn-remover-item {
            background: #dc3545;
            color: white;
        }
        .btn-remover-item:hover {
            background: #c82333;
            transform: translateY(-1px);
            box-shadow: 0 2px 4px rgba(220, 53, 69, 0.3);
        }
        .btn-remover-item-old {
            background: #ff4444;
            color: white;
            border: none;
            border-radius: 6px;
            width: 32px;
            height: 32px;
            display: flex;
            align-items: center;
            justify-content: center;
            cursor: pointer;
        }
        /* Desktop */
        @media (min-width: 992px) {
            .container-main {
                max-width: 1200px;
                margin: 0 auto 20px auto;
                padding: 0 20px;
            }
            .hero-section {
                border-radius: 16px;
                margin: 0 0 40px 0;
                padding: 60px 40px;
            }
            .hero-title {
                font-size: 36px;
            }
            .hero-subtitle {
                font-size: 18px;
            }
            .produtos-wrapper {
                display: flex;
                gap: 20px;
                align-items: flex-start;
            }
            .produtos-section {
                padding: 0 0 20px 0;
                flex: 1;
                min-width: 0;
            }
            .section-title {
                font-size: 26px;
                margin-bottom: 24px;
            }
            .carrinho-flutuante {
                display: none;
            }
            .carrinho-fixo {
                position: sticky;
                top: 90px;
                background: white;
                border-radius: 12px;
                padding: 20px;
                box-shadow: 0 2px 8px rgba(0,0,0,0.08);
                max-height: calc(100vh - 120px);
                overflow-y: auto;
                width: 320px;
                flex-shrink: 0;
                transition: all 0.3s ease;
            }
            .produtos-carrossel {
                padding: 20px;
            }
            .maozinha-clique i {
                font-size: 50px;
            }
            .carrossel-container {
                flex-wrap: wrap;
                gap: 16px;
            }
            .produto-card-carrossel {
                flex: 0 0 calc(25% - 12px);
                max-width: 200px;
            }
            .info-section {
                margin: 30px 0;
                padding: 30px !important;
            }
        }
        .is-valid {
            border-color: #28a745 !important;
        }
        .is-invalid {
            border-color: #dc3545 !important;
        }
        .invalid-feedback {
            display: block;
            width: 100%;
            margin-top: 0.25rem;
            font-size: 0.875em;
            color: #dc3545;
        }
        .valid-feedback {
            display: block;
            width: 100%;
            margin-top: 0.25rem;
            font-size: 0.875em;
            color: #28a745;
        }
        /* Estilos para seção de data de retirada */
        .secao-data-retirada {
            background: linear-gradient(135deg, #1a4d2e 0%, #2d5a3d 100%);
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 24px;
            box-shadow: 0 4px 12px rgba(26, 77, 46, 0.3);
        }
        .secao-data-retirada .form-label {
            color: white;
            font-size: 18px;
            font-weight: 700;
            margin-bottom: 16px;
            display: flex;
            align-items: center;
            gap: 8px;
        }
        .secao-data-retirada .form-label i {
            font-size: 20px;
        }
        /* Estilos para radiobuttons de data */
        .radio-group-datas {
            display: flex;
            flex-direction: column;
            gap: 12px;
        }
        .radio-item-data {
            background: white;
            border: 2px solid rgba(255, 255, 255, 0.3);
            border-radius: 8px;
            padding: 12px 16px;
            cursor: pointer;
            transition: all 0.3s ease;
            display: flex;
            align-items: center;
            gap: 12px;
        }
        .radio-item-data:hover {
            background: rgba(255, 255, 255, 0.95);
            border-color: rgba(255, 255, 255, 0.6);
            transform: translateX(4px);
        }
        .radio-item-data input[type="radio"] {
            width: 20px;
            height: 20px;
            cursor: pointer;
            accent-color: #1a4d2e;
        }
        .radio-item-data input[type="radio"]:checked + label {
            font-weight: 700;
            color: #1a4d2e;
        }
        .radio-item-data label {
            flex: 1;
            margin: 0;
            cursor: pointer;
            color: #333;
            font-size: 15px;
            display: flex;
            align-items: center;
            gap: 8px;
        }
        .radio-item-data input[type="radio"]:checked ~ .radio-item-data {
            border-color: #1a4d2e;
        }
        .radio-item-data:has(input[type="radio"]:checked) {
            background: #f0f7f3;
            border-color: #1a4d2e;
            box-shadow: 0 2px 8px rgba(26, 77, 46, 0.2);
        }
        /* Estilos para seção de observações */
        .secao-observacoes {
            background: #f8f9fa;
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 20px;
            border: 2px solid #e9ecef;
        }
        .secao-observacoes .form-label {
            color: #1a4d2e;
            font-size: 18px;
            font-weight: 700;
            margin-bottom: 12px;
            display: flex;
            align-items: center;
            gap: 8px;
        }
        .secao-observacoes .form-label i {
            font-size: 20px;
        }
        .secao-observacoes textarea {
            border: 2px solid #dee2e6;
            border-radius: 8px;
            padding: 12px;
            font-size: 15px;
            transition: all 0.3s ease;
        }
        .secao-observacoes textarea:focus {
            border-color: #1a4d2e;
            box-shadow: 0 0 0 3px rgba(26, 77, 46, 0.1);
            outline: none;
        }
        /* Animação de shake para validação */
        @keyframes shake {
            0%, 100% { transform: translateX(0); }
            25% { transform: translateX(-10px); }
            75% { transform: translateX(10px); }
        }
        /* Garantir que o campo de senha tenha espaço e seja visível */
        #divSenhaReserva {
            margin-top: 1.5rem !important;
            margin-bottom: 1.5rem !important;
            padding: 1rem !important;
            background-color: #f8f9fa !important;
            border-radius: 8px !important;
            border: 1px solid #dee2e6 !important;
        }
        #divSenhaReserva[style*="display: block"],
        #divSenhaReserva.show {
            display: block !important;
        }
        #divSenhaReserva .form-control {
            font-size: 1.1rem;
            padding: 0.875rem;
            min-height: 48px;
        }
        #divSenhaReserva label {
            font-weight: 600;
            margin-bottom: 0.5rem;
        }
        /* Espaçamento para os botões de login */
        #divBotoesLogin {
            margin-top: 2rem;
            padding-top: 1.5rem;
            border-top: 2px solid #dee2e6;
        }
        /* Garantir que a área de login tenha espaço suficiente */
        #divLoginDinamico {
            min-height: 250px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server" novalidate>
        <div class="container-fluid">
            <div class="header-logo">
                <div class="header-top">
                    <a href="Default.aspx" style="text-decoration: none; display: inline-block;">
                        <img id="logoImg" src="Images/logo-kingdom-confeitaria.svg" alt="Kingdom Confeitaria" loading="eager" decoding="async" style="cursor: pointer;" />
                    </a>
                    <h1 id="logoFallback" class="header-title" style="display: none; color: #1a4d2e; margin: 0; font-size: 20px;">
                        <a href="Default.aspx" style="text-decoration: none; color: inherit;"><i class="fas fa-crown"></i> Kingdom Confeitaria</a>
                    </h1>
                    <div class="header-user-name" id="clienteNome" runat="server" style="display: none;"></div>
                    <div class="header-actions">
                        <a href="Default.aspx"><i class="fas fa-home"></i> Home</a>
                        <a href="#" id="linkLogin" runat="server" style="display: inline;" onclick="abrirModalLogin(); return false;"><i class="fas fa-sign-in-alt"></i> Entrar</a>
                        <a href="paginas/MinhasReservas.aspx" id="linkMinhasReservas" runat="server" style="display: none;"><i class="fas fa-clipboard-list"></i> Minhas Reservas</a>
                        <a href="paginas/MeusDados.aspx" id="linkMeusDados" runat="server" style="display: none;"><i class="fas fa-user"></i> Meus Dados</a>
                        <a href="paginas/Admin.aspx" id="linkAdmin" runat="server" style="display: none;"><i class="fas fa-cog"></i> Painel Gestor</a>
                        <a href="paginas/Logout.aspx" id="linkLogout" runat="server" style="display: none;"><i class="fas fa-sign-out-alt"></i> Sair</a>
                        <a href="#" class="carrinho-header" id="carrinhoHeader" onclick="abrirModalCarrinho(); return false;" title="Ver carrinho">
                            <i class="fas fa-shopping-cart"></i>
                            <span class="carrinho-badge oculto" id="carrinhoBadge">0</span>
                        </a>
                    </div>
                </div>
            </div>
            <!-- Espaçador dinâmico para o header fixo -->
            <div class="header-spacer" id="headerSpacer"></div>
            
            <div class="container-main">
                <div class="produtos-wrapper">
                    <!-- Seção de Produtos -->
                    <div class="produtos-section">
                        <h2 class="section-title">
                            <i class="fas fa-birthday-cake"></i>
                            Nossos Produtos
                        </h2>
                        <div class="produtos-carrossel">
                            <!-- Botão de navegação esquerda -->
                            <button type="button" class="carrossel-nav-btn esquerda oculto" id="btnCarrosselEsquerda" aria-label="Produtos anteriores">
                                <i class="fas fa-chevron-left"></i>
                            </button>
                            <!-- Botão de navegação direita -->
                            <button type="button" class="carrossel-nav-btn direita" id="btnCarrosselDireita" aria-label="Próximos produtos">
                                <i class="fas fa-chevron-right"></i>
                            </button>
                            <div class="carrossel-container" id="produtosContainer" runat="server">
                                <!-- Produtos serão carregados aqui -->
                            </div>
                            <!-- Animação da mãozinha clicando -->
                            <div class="maozinha-clique" id="maozinhaClique">
                                <i class="fas fa-hand-pointer"></i>
                            </div>
                        </div>

                        <!-- Seção Informativa -->
                        <div class="info-section" style="padding: 20px 16px; background: #f8f9fa; margin: 20px 0; border-radius: 12px;">
                            <div style="display: flex; flex-direction: column; gap: 16px;">
                                <div style="display: flex; align-items: start; gap: 12px;">
                                    <div style="width: 40px; height: 40px; background: #1a4d2e; border-radius: 50%; display: flex; align-items: center; justify-content: center; flex-shrink: 0;">
                                        <i class="fas fa-calendar-check" style="color: white; font-size: 18px;"></i>
                                    </div>
                                    <div>
                                        <h3 style="font-size: 16px; font-weight: 600; color: #333; margin-bottom: 4px;">Reserve com Antecedência</h3>
                                        <p style="font-size: 14px; color: #666; margin: 0; line-height: 1.5;">Faça sua reserva e retire nas segundas-feiras disponíveis</p>
                                    </div>
                                </div>
                                <div style="display: flex; align-items: start; gap: 12px;">
                                    <div style="width: 40px; height: 40px; background: #1a4d2e; border-radius: 50%; display: flex; align-items: center; justify-content: center; flex-shrink: 0;">
                                        <i class="fas fa-heart" style="color: white; font-size: 18px;"></i>
                                    </div>
                                    <div>
                                        <h3 style="font-size: 16px; font-weight: 600; color: #333; margin-bottom: 4px;">Feito com Amor</h3>
                                        <p style="font-size: 14px; color: #666; margin: 0; line-height: 1.5;">Produtos artesanais preparados com ingredientes selecionados</p>
                                    </div>
                                </div>
                                <div style="display: flex; align-items: start; gap: 12px;">
                                    <div style="width: 40px; height: 40px; background: #1a4d2e; border-radius: 50%; display: flex; align-items: center; justify-content: center; flex-shrink: 0;">
                                        <i class="fas fa-truck" style="color: white; font-size: 18px;"></i>
                                    </div>
                                    <div>
                                        <h3 style="font-size: 16px; font-weight: 600; color: #333; margin-bottom: 4px;">Retirada Disponível</h3>
                                        <p style="font-size: 14px; color: #666; margin: 0; line-height: 1.5;">Retire seu pedido no dia agendado</p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="carrinho-fixo d-none d-lg-block" id="carrinhoFixo">
                        <h3 style="font-size: 18px; font-weight: 600; margin-bottom: 16px; color: #333; display: flex; align-items: center; gap: 8px;">
                            <i class="fas fa-shopping-cart"></i> Meu Pedido
                        </h3>
                        <div id="carrinhoContainer" runat="server" style="max-height: calc(100vh - 350px); overflow-y: auto; margin-bottom: 16px;">
                            <p class="text-muted" style="text-align: center; padding: 20px;">Seu carrinho está vazio</p>
                        </div>
                        <div class="total-carrinho" id="totalContainer" runat="server" style="display: none; background: #f8f9fa; padding: 16px; border-radius: 8px; margin-top: 16px; border-top: 2px solid #1a4d2e;">
                            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px;">
                                <span style="font-size: 16px; color: #666;">Subtotal:</span>
                                <span style="font-size: 18px; font-weight: 600; color: #333;">R$ <span id="totalPedido" runat="server">0,00</span></span>
                            </div>
                            <div style="display: flex; justify-content: space-between; align-items: center; padding-top: 8px; border-top: 1px solid #dee2e6;">
                                <span style="font-size: 18px; font-weight: 700; color: #1a4d2e;">Total:</span>
                                <span style="font-size: 22px; font-weight: 700; color: #1a4d2e;">R$ <span id="totalPedidoFinal" runat="server">0,00</span></span>
                            </div>
                        </div>
                        <asp:Button ID="btnFazerReserva" runat="server" 
                            Text="Fazer Reserva" 
                            CssClass="btn btn-reservar" 
                            OnClick="btnFazerReserva_Click" 
                            Enabled="false" 
                            UseSubmitBehavior="true"
                            CausesValidation="false"
                            Style="background: linear-gradient(135deg, #1a4d2e 0%, #2d5a3d 100%); color: white; border: none; padding: 14px; border-radius: 8px; width: 100%; margin-top: 16px; font-weight: 600; font-size: 16px; transition: all 0.3s; box-shadow: 0 2px 8px rgba(26, 77, 46, 0.3);" />
                    </div>
                </div>
            </div>

            <!-- Carrinho Flutuante Mobile -->
            <div class="carrinho-flutuante d-lg-none">
                <div class="carrinho-flutuante-header">
                    <div>
                        <div class="carrinho-flutuante-total" id="totalFlutuante" style="display: none;">
                            Total: R$ <span id="totalPedidoFlutuante">0,00</span>
                        </div>
                        <div class="carrinho-flutuante-itens" id="itensFlutuante">
                            <span id="qtdItensFlutuante">0</span> item(s)
                        </div>
                    </div>
                    <button type="button" class="btn btn-sm" onclick="abrirModalCarrinho()" style="background: #f0f0f0; border: none; padding: 8px 12px; border-radius: 6px;">
                        <i class="fas fa-shopping-cart"></i> Ver
                    </button>
                </div>
                <button type="button" class="btn-fazer-reserva" id="btnFazerReservaFlutuante" disabled>
                    Fazer Reserva
                </button>
                <script type="text/javascript">
                    (function() {
                        function configurarBotaoFlutuante() {
                            var btnFlutuante = document.getElementById('btnFazerReservaFlutuante');
                            if (btnFlutuante && typeof window.ClientIDs !== 'undefined' && window.ClientIDs.btnFazerReserva) {
                                var btnReserva = document.getElementById(window.ClientIDs.btnFazerReserva);
                                if (btnReserva) {
                                    btnFlutuante.addEventListener('click', function() {
                                        btnReserva.click();
                                    });
                                    return true;
                                }
                            }
                            return false;
                        }
                        
                        // Tentar configurar imediatamente
                        if (!configurarBotaoFlutuante()) {
                            // Se não funcionou, tentar após um delay
                            setTimeout(function() {
                                configurarBotaoFlutuante();
                            }, 100);
                        }
                    })();
                </script>
            </div>

        </div>
        
        <!-- Controles Hidden para dados de reserva -->
        <asp:HiddenField ID="hdnDataRetirada" runat="server" />
        <div id="radioGroupDatas" runat="server" style="display: none;"></div>
    </form>

    <script src="Scripts/bootstrap/bootstrap.bundle.min.js" defer></script>
    <script src="Scripts/navigation.js" defer></script>
    <!-- Garantir que __doPostBack esteja disponível antes de carregar os scripts -->
    <script type="text/javascript">
        // Função __doPostBack será gerada pelo ASP.NET automaticamente
        // Se não estiver disponível, criar uma versão básica
        if (typeof __doPostBack === 'undefined') {
            function __doPostBack(eventTarget, eventArgument) {
                if (!eventTarget) return false;
                var form = document.getElementById('form1');
                if (!form) {
                    return false;
                }
                
                // Remover inputs anteriores se existirem
                var existingTarget = form.querySelector('input[name="__EVENTTARGET"]');
                if (existingTarget) existingTarget.remove();
                var existingArg = form.querySelector('input[name="__EVENTARGUMENT"]');
                if (existingArg) existingArg.remove();
                
                var targetInput = document.createElement('input');
                targetInput.type = 'hidden';
                targetInput.name = '__EVENTTARGET';
                targetInput.value = eventTarget;
                form.appendChild(targetInput);
                
                if (eventArgument) {
                    var argInput = document.createElement('input');
                    argInput.type = 'hidden';
                    argInput.name = '__EVENTARGUMENT';
                    argInput.value = eventArgument;
                    form.appendChild(argInput);
                }
                
                form.submit();
                return false;
            }
        }
    </script>
    <!-- IMPORTANTE: Este script DEVE ser executado PRIMEIRO, antes de qualquer outro script -->
    <script type="text/javascript">
        // Definir ClientIDs como variáveis globais IMEDIATAMENTE
        // Este script deve ser executado antes de qualquer outro que use ClientIDs
        (function() {
            try {
                // Função helper para escapar strings JavaScript
                function escapeJs(str) {
                    if (!str) return '';
                    return String(str)
                        .replace(/\\/g, '\\\\')
                        .replace(/'/g, "\\'")
                        .replace(/"/g, '\\"')
                        .replace(/\r/g, '\\r')
                        .replace(/\n/g, '\\n')
                        .replace(/\t/g, '\\t');
                }
                
                window.ClientIDs = {
                    btnFazerReserva: '<%= btnFazerReserva != null ? EscapeJavaScript(btnFazerReserva.ClientID) : "" %>',
                    txtLoginDinamico: '<%= txtLoginDinamico != null ? EscapeJavaScript(txtLoginDinamico.ClientID) : "" %>',
                    txtSenhaReserva: '<%= txtSenhaReserva != null ? EscapeJavaScript(txtSenhaReserva.ClientID) : "" %>',
                    divSenhaReserva: '<%= divSenhaReserva != null ? EscapeJavaScript(divSenhaReserva.ClientID) : "" %>',
                    divLoginDinamico: '<%= divLoginDinamico != null ? EscapeJavaScript(divLoginDinamico.ClientID) : "" %>',
                    txtNome: '<%= txtNome != null ? EscapeJavaScript(txtNome.ClientID) : "" %>',
                    txtEmail: '<%= txtEmail != null ? EscapeJavaScript(txtEmail.ClientID) : "" %>',
                    txtTelefone: '<%= txtTelefone != null ? EscapeJavaScript(txtTelefone.ClientID) : "" %>',
                    divDadosReserva: '<%= divDadosReserva != null ? EscapeJavaScript(divDadosReserva.ClientID) : "" %>',
                    btnConfirmarReserva: '<%= btnConfirmarReserva != null ? EscapeJavaScript(btnConfirmarReserva.ClientID) : "" %>',
                    hdnNome: '<%= hdnNome != null ? EscapeJavaScript(hdnNome.ClientID) : "" %>',
                    hdnEmail: '<%= hdnEmail != null ? EscapeJavaScript(hdnEmail.ClientID) : "" %>',
                    hdnTelefone: '<%= hdnTelefone != null ? EscapeJavaScript(hdnTelefone.ClientID) : "" %>',
                    hdnDataRetirada: '<%= hdnDataRetirada != null ? EscapeJavaScript(hdnDataRetirada.ClientID) : "" %>',
                    radioGroupDatas: '<%= radioGroupDatas != null ? EscapeJavaScript(radioGroupDatas.ClientID) : "" %>',
                    produtosContainer: '<%= produtosContainer != null ? EscapeJavaScript(produtosContainer.ClientID) : "" %>',
                    carrinhoContainer: '<%= carrinhoContainer != null ? EscapeJavaScript(carrinhoContainer.ClientID) : "" %>',
                    totalPedido: '<%= totalPedido != null ? EscapeJavaScript(totalPedido.ClientID) : "" %>'
                };
            } catch (e) {
                console.error('Erro ao definir ClientIDs:', e);
                window.ClientIDs = {};
            }
        })();
    </script>
    <!-- Scripts comuns da aplicação -->
    <script src="Scripts/ajax-helper.js" defer></script>
    <script src="Scripts/app.js" defer></script>
    <!-- Scripts específicos da página principal -->
    <!-- Versão 2.0 - Forçar recarregamento para evitar cache -->
    <script src="Scripts/default.js?v=2.0" defer></script>
    <script>
        // Scripts inline apenas para dados dinâmicos do servidor (ClientIDs)
        // Todas as funções JavaScript estão em Scripts/app.js e Scripts/default.js
        
        // Aguardar carregamento do app.js antes de executar
        (function() {
            var loginDinamicoInicializado = false;
            var tentativasLoginDinamico = 0;
            var maxTentativasLoginDinamico = 10; // Reduzido de 20 para 10 (máximo de 500ms)
            
            function initLoginDinamico() {
                if (loginDinamicoInicializado) return;
                
                // Verificar se scripts já carregaram
                if (typeof KingdomConfeitaria === 'undefined' || !KingdomConfeitaria.Utils) {
                    tentativasLoginDinamico++;
                    if (tentativasLoginDinamico < maxTentativasLoginDinamico) {
                        setTimeout(initLoginDinamico, 50);
                    } else {
                        console.warn('KingdomConfeitaria não carregou após ' + maxTentativasLoginDinamico + ' tentativas');
                    }
                    return;
                }
                
                loginDinamicoInicializado = true;
                
                // Configurar Login Dinâmico - usar DOMContentLoaded se já carregou
                if (document.readyState === 'loading') {
                    document.addEventListener('DOMContentLoaded', initLoginDinamicoReady);
                } else {
                    // DOM já carregou, executar imediatamente
                    initLoginDinamicoReady();
                }
            }
            
            // Variáveis globais dentro da IIFE para acesso em toda a função
            var txtNome, txtEmail, txtTelefone, nomeReserva, emailReserva, telefoneReserva, txtLoginDinamico;
            
            function initLoginDinamicoReady() {
                // Verificar se ClientIDs está disponível
                if (typeof window.ClientIDs === 'undefined') {
                    console.error('ClientIDs não está definido. Aguardando...');
                    setTimeout(initLoginDinamicoReady, 100);
                    return;
                }
                
                var ClientIDs = window.ClientIDs;
                
                // Se os controles de login dinâmico não existem, não há problema - apenas retornar silenciosamente
                // Isso pode acontecer se a página não tiver esses controles (ex: Default.aspx não usa mais login dinâmico)
                if (!ClientIDs || !ClientIDs.txtLoginDinamico || ClientIDs.txtLoginDinamico === '') {
                    // Não logar como erro - é esperado se os controles não existem
                    return;
                }
                
                txtLoginDinamico = document.getElementById(ClientIDs.txtLoginDinamico);
                if (!txtLoginDinamico) {
                    // Se o elemento não existe no DOM, não há problema - apenas retornar
                    return;
                }
                var txtSenhaReserva = document.getElementById(ClientIDs.txtSenhaReserva);
                var divSenhaReserva = document.getElementById(ClientIDs.divSenhaReserva);
                var divMensagemLogin = document.getElementById('divMensagemLogin');
                var divDadosUsuario = document.getElementById('divDadosUsuario');
                txtNome = document.getElementById(ClientIDs.txtNome);
                txtEmail = document.getElementById(ClientIDs.txtEmail);
                txtTelefone = document.getElementById(ClientIDs.txtTelefone);
                var divDadosReserva = document.getElementById(ClientIDs.divDadosReserva);
                if (!divDadosReserva) {
                    divDadosReserva = document.querySelector('[id*="divDadosReserva"]');
                }
                var btnConfirmarReserva = document.getElementById(ClientIDs.btnConfirmarReserva);
                if (!btnConfirmarReserva) {
                    btnConfirmarReserva = document.querySelector('[id*="btnConfirmarReserva"]');
                }
                nomeReserva = txtNome; // Alias para compatibilidade
                emailReserva = txtEmail; // Alias para compatibilidade
                telefoneReserva = txtTelefone; // Alias para compatibilidade
                var estaLogado = window.usuarioLogado === true;
                
                var clienteEncontrado = null;
                // Tornar global para acesso fora da IIFE
                window.clienteEncontrado = clienteEncontrado;
                
                // Função para mostrar mensagem
                function mostrarMensagem(mensagem, tipo) {
                    if (!divMensagemLogin) return;
                    divMensagemLogin.style.display = 'block';
                    divMensagemLogin.className = 'mt-2 alert alert-' + (tipo || 'info');
                    divMensagemLogin.innerHTML = mensagem;
                }
                // Tornar global para acesso fora da IIFE
                window.mostrarMensagem = mostrarMensagem;
                
                // Função para ocultar mensagem
                function ocultarMensagem() {
                    if (divMensagemLogin) {
                        divMensagemLogin.style.display = 'none';
                    }
                }
                // Tornar global para acesso fora da IIFE
                window.ocultarMensagem = ocultarMensagem;
                
                // Função para verificar cliente enquanto digita - com debounce
                var verificacaoEmAndamento = false;
                var timeoutVerificacaoCliente = null;
                
                function verificarClienteDinamico() {
                    if (estaLogado || verificacaoEmAndamento) return;
                    
                    // Limpar timeout anterior
                    if (timeoutVerificacaoCliente) {
                        clearTimeout(timeoutVerificacaoCliente);
                    }
                    
                    // Debounce - aguardar 500ms após parar de digitar
                    timeoutVerificacaoCliente = setTimeout(function() {
                        var login = txtLoginDinamico ? txtLoginDinamico.value.trim() : '';
                        if (!login) {
                            ocultarMensagem();
                            var divSenhaReservaElement = document.getElementById(ClientIDs.divSenhaReserva);
                            if (divSenhaReservaElement) divSenhaReservaElement.style.display = 'none';
                            clienteEncontrado = null;
                            window.clienteEncontrado = null;
                            verificacaoEmAndamento = false;
                            return;
                        }
                        
                        // Detectar se é email ou telefone
                        var isEmail = login.indexOf('@') > -1;
                        var loginLimpo = isEmail ? login.toLowerCase() : login.replace(/\D/g, '');
                        
                        // Só verificar se tiver informação suficiente
                        if (isEmail && login.length < 5) {
                            verificacaoEmAndamento = false;
                            return;
                        }
                        if (!isEmail && loginLimpo.length < 10) {
                            verificacaoEmAndamento = false;
                            return;
                        }
                        
                        verificacaoEmAndamento = true;
                        
                        // Chamar Handler para verificar cliente (sem ScriptManager)
                        mostrarMensagem('<i class="fas fa-spinner fa-spin"></i> Verificando...', 'info');
                        
                        KingdomConfeitaria.Ajax.callHandler(
                            'Handlers/CallbackHandler.ashx',
                            {
                                acao: 'verificarcliente',
                                login: login
                            },
                            'POST',
                            function(result) {
                                verificacaoEmAndamento = false;
                                ocultarMensagem();
                                
                                if (result && result.existe) {
                                    clienteEncontrado = result.cliente;
                                    window.clienteEncontrado = clienteEncontrado;
                                    
                                    // Hide cadastro option
                                    var divOpcaoCadastro = document.getElementById('divOpcaoCadastro');
                                    if (divOpcaoCadastro) divOpcaoCadastro.style.display = 'none';
                                    
                                    // Ocultar botões de login inicialmente (serão mostrados se tiver senha)
                                    var divBotoesLoginInicial = document.getElementById('divBotoesLogin');
                                    if (divBotoesLoginInicial) {
                                        divBotoesLoginInicial.style.display = 'none';
                                    }
                                    
                                    if (result.temSenha) {
                                        // Cliente encontrado e tem senha - MOSTRAR CAMPO DE SENHA IMEDIATAMENTE
                                        
                                        // Função para mostrar campo de senha
                                        function mostrarCampoSenha() {
                                            // Garantir que o divLoginDinamico esteja visível
                                            var divLoginDinamicoElement = document.getElementById(ClientIDs.divLoginDinamico);
                                            if (!divLoginDinamicoElement) {
                                                divLoginDinamicoElement = document.querySelector('[id*="divLoginDinamico"]');
                                            }
                                            if (divLoginDinamicoElement) {
                                                divLoginDinamicoElement.style.display = 'block';
                                            }
                                            
                                            // Encontrar o campo de senha - tentar múltiplas formas
                                            var divSenhaReservaElement = document.getElementById(ClientIDs.divSenhaReserva);
                                            
                                            if (!divSenhaReservaElement) {
                                                divSenhaReservaElement = document.querySelector('[id*="divSenhaReserva"]');
                                            }
                                            
                                            if (!divSenhaReservaElement) {
                                                // Tentar encontrar todos os elementos com "Senha" no ID
                                                var todosElementos = document.querySelectorAll('[id*="Senha"]');
                                                if (todosElementos.length > 0) {
                                                    divSenhaReservaElement = todosElementos[0];
                                                }
                                            }
                                            
                                            if (divSenhaReservaElement) {
                                                // FORÇAR EXIBIÇÃO - método mais direto
                                                divSenhaReservaElement.removeAttribute('style');
                                                divSenhaReservaElement.style.display = 'block';
                                                divSenhaReservaElement.style.visibility = 'visible';
                                                divSenhaReservaElement.style.marginTop = '1.5rem';
                                                divSenhaReservaElement.style.marginBottom = '1.5rem';
                                                divSenhaReservaElement.style.padding = '1rem';
                                                divSenhaReservaElement.style.backgroundColor = '#f8f9fa';
                                                divSenhaReservaElement.style.borderRadius = '8px';
                                                divSenhaReservaElement.style.border = '1px solid #dee2e6';
                                                divSenhaReservaElement.classList.add('show');
                                                divSenhaReservaElement.removeAttribute('hidden');
                                                
                                                // Criar estilo dinâmico para garantir que fique visível
                                                var styleId = 'style-for-divSenhaReserva';
                                                var existingStyle = document.getElementById(styleId);
                                                if (existingStyle) {
                                                    existingStyle.remove();
                                                }
                                                var style = document.createElement('style');
                                                style.id = styleId;
                                                style.textContent = '#' + divSenhaReservaElement.id + ' { display: block !important; visibility: visible !important; }';
                                                document.head.appendChild(style);
                                                
                                                // Scroll para o campo
                                                setTimeout(function() {
                                                    divSenhaReservaElement.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                                                    
                                                    // Focar no campo de senha
                                                    var txtSenhaReservaElement = document.getElementById(ClientIDs.txtSenhaReserva);
                                                    if (!txtSenhaReservaElement) {
                                                        txtSenhaReservaElement = document.querySelector('[id*="txtSenhaReserva"]');
                                                    }
                                                    if (txtSenhaReservaElement) {
                                                        txtSenhaReservaElement.required = true;
                                                        txtSenhaReservaElement.focus();
                                                    }
                                                }, 100);
                                                
                                                return true;
                                            } else {
                                                return false;
                                            }
                                        }
                                    
                                        // Mostrar campo de senha IMEDIATAMENTE
                                        mostrarCampoSenha();
                                        
                                        // Mostrar botões de login (Confirmar, Cancelar)
                                        var divBotoesLogin = document.getElementById('divBotoesLogin');
                                        if (divBotoesLogin) {
                                            divBotoesLogin.style.display = 'flex';
                                        }
                                        
                                        // Ocultar botões de reserva
                                        var divBotoesReserva = document.getElementById('divBotoesReserva');
                                        if (divBotoesReserva) {
                                            divBotoesReserva.style.display = 'none';
                                        }
                                        
                                        // Tentar novamente após um pequeno delay para garantir
                                        setTimeout(function() {
                                            var divSenhaReservaElement = document.getElementById(ClientIDs.divSenhaReserva);
                                            if (!divSenhaReservaElement) {
                                                divSenhaReservaElement = document.querySelector('[id*="divSenhaReserva"]');
                                            }
                                            if (divSenhaReservaElement && window.getComputedStyle(divSenhaReservaElement).display === 'none') {
                                                mostrarCampoSenha();
                                            }
                                        }, 100);
                                        
                                        // Atualizar link de recuperar senha com email/telefone
                                        var linkRecuperarSenha = document.getElementById('linkRecuperarSenha');
                                        if (linkRecuperarSenha) {
                                            var isEmail = login.indexOf('@') > -1;
                                            var loginParam = isEmail ? 'email' : 'telefone';
                                            linkRecuperarSenha.href = 'paginas/RecuperarSenha.aspx?' + loginParam + '=' + encodeURIComponent(login);
                                        }
                                        
                                        // Cliente encontrado, aguardando senha
                                    } else {
                                        // Cliente encontrado mas não tem senha - fazer login automático e mostrar área de reserva
                                        // Continuar direto para a tela de reserva sem confirmação
                                        ocultarMensagem();
                                        preencherDadosCliente(result.cliente);
                                    }
                                } else {
                                    // Cliente não encontrado - mostrar opção de cadastro
                                    clienteEncontrado = null;
                                    window.clienteEncontrado = null;
                                    var divSenhaReservaElement = document.getElementById(ClientIDs.divSenhaReserva);
                                    var txtSenhaReservaElement = document.getElementById(ClientIDs.txtSenhaReserva);
                                    
                                    if (divSenhaReservaElement) {
                                        divSenhaReservaElement.style.display = 'none';
                                        if (txtSenhaReservaElement) {
                                            txtSenhaReservaElement.value = '';
                                            txtSenhaReservaElement.required = false;
                                        }
                                    }
                                    
                                    // Ocultar botões de login
                                    var divBotoesLogin = document.getElementById('divBotoesLogin');
                                    if (divBotoesLogin) {
                                        divBotoesLogin.style.display = 'none';
                                    }
                                    
                                    // Mostrar opção de cadastro
                                    var divOpcaoCadastro = document.getElementById('divOpcaoCadastro');
                                    var linkIrCadastro = document.getElementById('linkIrCadastro');
                                    if (divOpcaoCadastro) {
                                        divOpcaoCadastro.style.display = 'block';
                                    }
                                    if (linkIrCadastro) {
                                        // Detectar se é email ou telefone
                                        var isEmail = login.indexOf('@') > -1;
                                        var loginParam = isEmail ? 'email' : 'telefone';
                                        linkIrCadastro.href = 'paginas/Cadastro.aspx?' + loginParam + '=' + encodeURIComponent(login);
                                    }
                                    
                                    mostrarMensagem('<i class="fas fa-info-circle"></i> Cliente não encontrado. Clique no botão abaixo para se cadastrar.', 'info');
                                }
                            }, function(error) {
                                verificacaoEmAndamento = false;
                                ocultarMensagem();
                                mostrarMensagem('<i class="fas fa-exclamation-triangle"></i> Erro ao verificar cliente. Tente novamente.', 'danger');
                            }
                        );
                    }, 500);
                }
            }
            
            // Função para preencher dados do cliente e mostrar área de reserva AUTOMATICAMENTE
            // Esta função é chamada após login bem-sucedido e continua direto para a tela de reserva
            function preencherDadosCliente(cliente) {
                // Tornar global para acesso fora da IIFE
                window.preencherDadosCliente = preencherDadosCliente;
                
                if (!cliente) return;
                
                // Fazer login na sessão via Handler (sem ScriptManager)
                var login = txtLoginDinamico ? txtLoginDinamico.value.trim() : '';
                
                // Chamar Handler para fazer login na sessão
                KingdomConfeitaria.Ajax.callHandler(
                    'Handlers/CallbackHandler.ashx',
                    {
                        acao: 'fazerlogin',
                        clienteId: cliente.id
                    },
                    'POST',
                    function(result) {
                        if (result && result.sucesso) {
                            
                            // Preencher campos do formulário IMEDIATAMENTE (visuais e hidden)
                            if (txtNome && cliente.nome) {
                                txtNome.value = cliente.nome;
                                // Preencher campo hidden para garantir que o valor seja enviado no postback
                                var hdnNome = document.getElementById(ClientIDs.hdnNome);
                                if (hdnNome) {
                                    hdnNome.value = cliente.nome;
                                }
                            }
                            if (txtEmail && cliente.email) {
                                txtEmail.value = cliente.email;
                                // Preencher campo hidden para garantir que o valor seja enviado no postback
                                var hdnEmail = document.getElementById(ClientIDs.hdnEmail);
                                if (hdnEmail) {
                                    hdnEmail.value = cliente.email;
                                }
                            }
                            if (txtTelefone && cliente.telefone) {
                                // Formatar telefone para exibição
                                var telFormatado = cliente.telefone.replace(/\D/g, '');
                                if (telFormatado.length <= 10) {
                                    telFormatado = telFormatado.replace(/^(\d{2})(\d{4})(\d{0,4}).*/, '($1) $2-$3');
                                } else {
                                    telFormatado = telFormatado.replace(/^(\d{2})(\d{5})(\d{0,4}).*/, '($1) $2-$3');
                                }
                                txtTelefone.value = telFormatado;
                                // Preencher campo hidden com telefone sem formatação para garantir que o valor seja enviado no postback
                                var hdnTelefone = document.getElementById(ClientIDs.hdnTelefone);
                                if (hdnTelefone) {
                                    hdnTelefone.value = cliente.telefone.replace(/\D/g, '');
                                }
                            }
                            
                            // Ocultar área de login IMEDIATAMENTE
                            var divLoginDinamicoElement = document.getElementById(ClientIDs.divLoginDinamico);
                            if (divLoginDinamicoElement) {
                                divLoginDinamicoElement.style.display = 'none';
                            }
                            
                            // Ocultar campo de senha
                            if (divSenhaReserva) {
                                divSenhaReserva.style.display = 'none';
                            }
                            
                            // Ocultar botões de login
                            var divBotoesLogin = document.getElementById('divBotoesLogin');
                            if (divBotoesLogin) {
                                divBotoesLogin.style.display = 'none';
                            }
                            
                            // Ocultar mensagem de login
                            ocultarMensagem();
                            
                            
                            // Mostrar área de reserva IMEDIATAMENTE (sem confirmação)
                            var divDadosReserva = document.getElementById(ClientIDs.divDadosReserva);
                            if (!divDadosReserva) {
                                divDadosReserva = document.querySelector('[id*="divDadosReserva"]');
                            }
                            if (divDadosReserva) {
                                // Forçar exibição com múltiplas propriedades
                                divDadosReserva.style.display = 'block';
                                divDadosReserva.style.visibility = 'visible';
                                divDadosReserva.style.opacity = '1';
                                divDadosReserva.removeAttribute('hidden');
                                divDadosReserva.classList.remove('d-none');
                                divDadosReserva.classList.add('d-block');
                                
                                // Garantir que todos os campos dentro estejam visíveis
                                var campos = divDadosReserva.querySelectorAll('input, select, textarea, label, .form-label, .mb-3, .form-control, .form-select');
                                campos.forEach(function(campo) {
                                    campo.style.display = '';
                                    campo.style.visibility = 'visible';
                                    campo.style.opacity = '1';
                                    campo.removeAttribute('hidden');
                                });
                                
                                // Garantir que os labels e divs também estejam visíveis
                                var labels = divDadosReserva.querySelectorAll('label, .form-label, .mb-3, .small, .text-muted');
                                labels.forEach(function(label) {
                                    label.style.display = '';
                                    label.style.visibility = 'visible';
                                });
                            } else {
                                // Tentar novamente após um pequeno delay
                                setTimeout(function() {
                                    divDadosReserva = document.getElementById(ClientIDs.divDadosReserva);
                                    if (!divDadosReserva) {
                                        divDadosReserva = document.querySelector('[id*="divDadosReserva"]');
                                    }
                                    if (divDadosReserva) {
                                        divDadosReserva.style.display = 'block';
                                        divDadosReserva.style.visibility = 'visible';
                                    }
                                }, 200);
                            }
                            
                            // Mostrar botões de reserva
                            var divBotoesReserva = document.getElementById('divBotoesReserva');
                            if (divBotoesReserva) {
                                divBotoesReserva.style.display = 'flex';
                                divBotoesReserva.style.visibility = 'visible';
                            }
                            
                            // Mostrar botão Confirmar Reserva
                            var btnConfirmarReserva = document.getElementById(ClientIDs.btnConfirmarReserva);
                            if (!btnConfirmarReserva) {
                                btnConfirmarReserva = document.querySelector('[id*="btnConfirmarReserva"]');
                            }
                            if (btnConfirmarReserva) {
                                btnConfirmarReserva.style.display = 'inline-block';
                                btnConfirmarReserva.style.visibility = 'visible';
                                btnConfirmarReserva.removeAttribute('hidden');
                            }
                            
                            // Atualizar variável global
                            window.usuarioLogado = true;
                            
                            // Scroll suave para a área de reserva
                            setTimeout(function() {
                                if (divDadosReserva) {
                                    divDadosReserva.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                                }
                            }, 100);
                            
                            // Atualizar menu do header via JavaScript
                            setTimeout(function() {
                                var linkLogin = document.querySelector('[id*="linkLogin"]');
                                var linkMinhasReservas = document.querySelector('[id*="linkMinhasReservas"]');
                                var linkMeusDados = document.querySelector('[id*="linkMeusDados"]');
                                var linkLogout = document.querySelector('[id*="linkLogout"]');
                                var clienteNome = document.querySelector('[id*="clienteNome"]');
                                
                                if (linkLogin) linkLogin.style.display = 'none';
                                if (linkMinhasReservas) linkMinhasReservas.style.display = 'inline';
                                if (linkMeusDados) linkMeusDados.style.display = 'inline';
                                if (linkLogout) linkLogout.style.display = 'inline';
                                if (clienteNome && cliente.nome) {
                                    clienteNome.textContent = 'Olá, ' + cliente.nome;
                                    clienteNome.style.display = 'inline';
                                }
                                
                                // Atualizar isAdmin do cliente se retornado
                                if (result.isAdmin !== undefined) {
                                    cliente.isAdmin = result.isAdmin;
                                }
                                
                                // Se for admin, mostrar link de admin
                                var linkAdmin = document.querySelector('[id*="linkAdmin"]');
                                if (cliente.isAdmin || (result.isAdmin === true)) {
                                    if (linkAdmin) linkAdmin.style.display = 'inline';
                                } else {
                                    if (linkAdmin) linkAdmin.style.display = 'none';
                                }
                            }, 100);
                            
                        } else {
                            mostrarMensagem('<i class="fas fa-exclamation-triangle"></i> Erro ao fazer login: ' + (result.mensagem || 'Erro desconhecido'), 'danger');
                        }
                    }, function(error) {
                        // Erro ao fazer login
                        mostrarMensagem('<i class="fas fa-exclamation-triangle"></i> Erro ao fazer login. Tente novamente.', 'danger');
                    }
                );
            }
            
            // Função para validar senha (tornada global para ser acessível via onclick)
            window.validarSenha = function() {
                if (!window.clienteEncontrado) {
                    alert('Cliente não encontrado. Por favor, digite seu email ou telefone primeiro.');
                    return;
                }
                
                // Usar ClientID para garantir que encontre o elemento correto
                var ClientIDs = window.ClientIDs || {};
                var txtSenhaReservaElement = document.getElementById(ClientIDs.txtSenhaReserva);
                var txtLoginDinamicoElement = document.getElementById(ClientIDs.txtLoginDinamico);
                
                var senha = txtSenhaReservaElement ? txtSenhaReservaElement.value : '';
                var login = txtLoginDinamicoElement ? txtLoginDinamicoElement.value.trim() : '';
                
                if (!senha) {
                    if (window.mostrarMensagem) {
                        window.mostrarMensagem('<i class="fas fa-exclamation-triangle"></i> Por favor, digite sua senha.', 'warning');
                    } else {
                        alert('Por favor, digite sua senha.');
                    }
                    if (txtSenhaReservaElement) {
                        txtSenhaReservaElement.focus();
                    }
                    return;
                }
                
                // Chamar Handler para validar senha (sem ScriptManager)
                if (window.mostrarMensagem) {
                    window.mostrarMensagem('<i class="fas fa-spinner fa-spin"></i> Validando senha...', 'info');
                }
                
                KingdomConfeitaria.Ajax.callHandler(
                    'Handlers/CallbackHandler.ashx',
                    {
                        acao: 'validarsenha',
                        login: login,
                        senha: senha
                    },
                    'POST',
                    function(result) {
                        if (result && result.valido) {
                            // Senha válida - fazer login e mostrar área de reserva AUTOMATICAMENTE
                            // Sem pedir confirmação, continuar direto para a tela de reserva
                            if (window.ocultarMensagem) {
                                window.ocultarMensagem();
                            }
                            if (window.preencherDadosCliente) {
                                window.preencherDadosCliente(result.cliente);
                            }
                        } else {
                            // Senha inválida
                            if (window.mostrarMensagem) {
                                window.mostrarMensagem('<i class="fas fa-times-circle"></i> ' + (result.mensagem || 'Senha incorreta.'), 'danger');
                            } else {
                                alert(result.mensagem || 'Senha incorreta.');
                            }
                            if (txtSenhaReservaElement) {
                                txtSenhaReservaElement.value = '';
                                txtSenhaReservaElement.focus();
                            }
                        }
                    }, function(error) {
                        if (window.ocultarMensagem) {
                            window.ocultarMensagem();
                        }
                        // Erro ao validar senha
                        if (window.mostrarMensagem) {
                            window.mostrarMensagem('<i class="fas fa-exclamation-triangle"></i> Erro ao validar senha. Tente novamente.', 'danger');
                        } else {
                            alert('Erro ao validar senha. Tente novamente.');
                        }
                    }
                );
            };
            
            // Função para filtrar e normalizar entrada do login
            function filtrarEntradaLogin(input) {
                var valor = input.value;
                var cursorPos = input.selectionStart;
                
                // Detectar se é email ou telefone
                // Regras de detecção:
                // 1. Se contém @, é email
                // 2. Se contém letras (a-z, A-Z), é email
                // 3. Se contém apenas números, é telefone
                // 4. Se está vazio, aceitar qualquer entrada e detectar pelo primeiro caractere
                var temArroba = valor.indexOf('@') > -1;
                var temLetras = /[a-zA-Z]/.test(valor);
                var temApenasNumeros = valor.length > 0 && /^[0-9]*$/.test(valor);
                var primeiroChar = valor.length > 0 ? valor[0] : '';
                var primeiroCharIsNumero = /^[0-9]$/.test(primeiroChar);
                var primeiroCharIsLetra = /^[a-zA-Z]$/.test(primeiroChar);
                
                // Se tiver @ ou letras, é email
                // Se tiver apenas números, é telefone
                // Se estiver vazio, assumir telefone (vai aceitar apenas números até detectar letra ou @)
                var isEmail = temArroba || temLetras;
                var isTelefone = !isEmail; // Se não é email, é telefone (inclui vazio)
                
                var novoValor = '';
                var novoCursorPos = cursorPos;
                
                if (isEmail) {
                    // É email - aceitar apenas caracteres válidos de email
                    // Caracteres válidos: letras, números, @, ., _, -, +
                    for (var i = 0; i < valor.length; i++) {
                        var char = valor[i];
                        var charCode = char.charCodeAt(0);
                        
                        // Converter maiúsculas para minúsculas imediatamente
                        if (charCode >= 65 && charCode <= 90) {
                            char = String.fromCharCode(charCode + 32);
                            charCode = char.charCodeAt(0);
                        }
                        
                        // Aceitar: letras minúsculas (a-z), números (0-9), @, ., _, -, +
                        if ((charCode >= 97 && charCode <= 122) || // a-z
                            (charCode >= 48 && charCode <= 57) ||   // 0-9
                            char === '@' || char === '.' || char === '_' || char === '-' || char === '+') {
                            novoValor += char;
                        } else if (i < cursorPos) {
                            // Se o caractere foi removido antes da posição do cursor, ajustar posição
                            novoCursorPos--;
                        }
                    }
                } else {
                    // É telefone - aceitar apenas números
                    for (var i = 0; i < valor.length; i++) {
                        var char = valor[i];
                        var charCode = char.charCodeAt(0);
                        
                        // Aceitar apenas números (0-9)
                        if (charCode >= 48 && charCode <= 57) {
                            novoValor += char;
                        } else if (i < cursorPos) {
                            // Se o caractere foi removido antes da posição do cursor, ajustar posição
                            novoCursorPos--;
                        }
                    }
                }
                
                // Atualizar valor e posição do cursor
                input.value = novoValor;
                input.setSelectionRange(novoCursorPos, novoCursorPos);
                
                return novoValor;
            }
            
            // Event listener para campo de login dinâmico
            if (txtLoginDinamico) {
                // Bloquear caracteres inválidos antes de digitar
                txtLoginDinamico.addEventListener('keypress', function(e) {
                    // Permitir teclas especiais (Backspace, Delete, Tab, Arrow keys, etc.)
                    var keyCode = e.which || e.keyCode;
                    
                    // Verificar teclas de controle
                    var isControlKey = (keyCode === 8 || keyCode === 46 || keyCode === 9 || keyCode === 13 || keyCode === 27);
                    var isNavigationKey = (keyCode >= 35 && keyCode <= 40);
                    var isModifierKey = (e.ctrlKey || e.metaKey || false);
                    var isSpecialKey = isControlKey || isNavigationKey || isModifierKey;
                    
                    if (isSpecialKey) {
                        return true;
                    }
                    
                    // Validar keyCode antes de processar
                    if (!keyCode || keyCode < 0 || keyCode > 255) {
                        return true; // Permitir teclas inválidas
                    }
                    
                    var char = '';
                    var charCode = 0;
                    
                    try {
                        if (typeof String.fromCharCode !== 'function') {
                            return true; // Função não disponível
                        }
                        char = String.fromCharCode(keyCode);
                        if (!char || char.length === 0) {
                            return true; // Caractere inválido
                        }
                        charCode = char.charCodeAt(0);
                        
                        // Validar charCode antes de usar
                        if (isNaN(charCode) || charCode < 0) {
                            return true; // Permitir caracteres inválidos
                        }
                    } catch (err) {
                        return true; // Erro ao processar caractere
                    }
                    
                    var valorAtual = e.target.value;
                    var temArroba = valorAtual.indexOf('@') > -1;
                    var temLetras = /[a-zA-Z]/.test(valorAtual);
                    var isEmail = temArroba || temLetras;
                    
                    // Se o campo está vazio, permitir qualquer caractere válido (será filtrado depois)
                    // Se já tem conteúdo, validar baseado no tipo detectado
                    // Mas permitir mudança de telefone para email se digitar @ ou letras
                    if (valorAtual.length === 0) {
                        // Campo vazio: aceitar letras, números, @ (será detectado automaticamente)
                        var isLetter = (charCode >= 65 && charCode <= 90) || (charCode >= 97 && charCode <= 122);
                        var isNumber = (charCode >= 48 && charCode <= 57);
                        var isAtSymbol = (char === '@');
                        var valido = isLetter || isNumber || isAtSymbol;
                        if (!valido) {
                            e.preventDefault();
                        }
                    } else if (isEmail) {
                        // Email: aceitar letras, números, @, ., _, -, +
                        var isLetter = (charCode >= 65 && charCode <= 90) || (charCode >= 97 && charCode <= 122);
                        var isNumber = (charCode >= 48 && charCode <= 57);
                        var isAt = (char === '@');
                        var isDot = (char === '.');
                        var isUnderscore = (char === '_');
                        var isDash = (char === '-');
                        var isPlus = (char === '+');
                        var isEmailChar = isAt || isDot || isUnderscore || isDash || isPlus;
                        var valido = isLetter || isNumber || isEmailChar;
                        if (!valido) {
                            e.preventDefault();
                        }
                    } else {
                        // Telefone: aceitar números, mas também permitir @ ou letras para mudar para email
                        var isNumber = (charCode >= 48 && charCode <= 57);
                        var isLetter = (charCode >= 65 && charCode <= 90) || (charCode >= 97 && charCode <= 122);
                        var isAtSymbol = (char === '@');
                        var valido = isNumber || isLetter || isAtSymbol;
                        if (!valido) {
                            e.preventDefault();
                        }
                    }
                });
                
                // Filtrar entrada em tempo real (para casos de colar, drag&drop, etc)
                txtLoginDinamico.addEventListener('input', function(e) {
                    // Filtrar e normalizar entrada
                    var novoValor = filtrarEntradaLogin(e.target);
                    
                    // Verificar cliente com debounce (já implementado na função)
                    verificarClienteDinamico();
                });
                
                // Prevenir colar conteúdo inválido
                txtLoginDinamico.addEventListener('paste', function(e) {
                    e.preventDefault();
                    var texto = (e.clipboardData || window.clipboardData).getData('text');
                    
                    // Filtrar texto colado
                    var temArroba = texto.indexOf('@') > -1;
                    var textoFiltrado = '';
                    
                    if (temArroba) {
                        // É email - filtrar caracteres válidos e converter para minúsculas
                        for (var i = 0; i < texto.length; i++) {
                            var char = texto[i].toLowerCase();
                            var charCode = char.charCodeAt(0);
                            if ((charCode >= 97 && charCode <= 122) || // a-z
                                (charCode >= 48 && charCode <= 57) ||   // 0-9
                                char === '@' || char === '.' || char === '_' || char === '-' || char === '+') {
                                textoFiltrado += char;
                            }
                        }
                    } else {
                        // É telefone - apenas números
                        textoFiltrado = texto.replace(/\D/g, '');
                    }
                    
                    // Inserir texto filtrado na posição do cursor
                    var cursorPos = e.target.selectionStart;
                    var valorAtual = e.target.value;
                    var novoValor = valorAtual.substring(0, cursorPos) + textoFiltrado + valorAtual.substring(e.target.selectionEnd);
                    e.target.value = novoValor;
                    e.target.setSelectionRange(cursorPos + textoFiltrado.length, cursorPos + textoFiltrado.length);
                    
                    // Disparar evento input para verificar cliente
                    e.target.dispatchEvent(new Event('input'));
                });
                
                // Permitir Enter para validar senha se campo de senha estiver visível
                txtLoginDinamico.addEventListener('keydown', function(e) {
                    if (e.key === 'Enter') {
                        var divSenhaReservaCheck = document.getElementById(ClientIDs.divSenhaReserva);
                        if (divSenhaReservaCheck && divSenhaReservaCheck.style.display !== 'none') {
                            e.preventDefault();
                            var txtSenhaReservaCheck = document.getElementById(ClientIDs.txtSenhaReserva);
                            if (txtSenhaReservaCheck) {
                                txtSenhaReservaCheck.focus();
                            }
                        }
                    }
                });
            }
            
            // Event listener para campo de senha
            // Usar ClientID para garantir que encontre o elemento correto
            var txtSenhaReservaElement = document.getElementById(ClientIDs.txtSenhaReserva);
            
            if (txtSenhaReservaElement) {
                txtSenhaReservaElement.addEventListener('keypress', function(e) {
                    if (e.key === 'Enter') {
                        e.preventDefault();
                        if (window.validarSenha) {
                            window.validarSenha();
                        }
                    }
                });
                
                // Botão para validar senha (pode ser adicionado depois)
                txtSenhaReservaElement.addEventListener('blur', function() {
                    if (this.value && window.clienteEncontrado) {
                        if (window.validarSenha) {
                            window.validarSenha();
                        }
                    }
                });
            }
            
            // Event listeners para botões de login
            var btnConfirmarLogin = document.getElementById('btnConfirmarLogin');
            var btnCancelarLogin = document.getElementById('btnCancelarLogin');
            
            if (btnConfirmarLogin) {
                btnConfirmarLogin.addEventListener('click', function(e) {
                    e.preventDefault();
                    if (window.validarSenha) {
                        window.validarSenha();
                    } else {
                        alert('Função de validação não disponível. Por favor, recarregue a página.');
                    }
                });
            }
            
            if (btnCancelarLogin) {
                btnCancelarLogin.addEventListener('click', function(e) {
                    e.preventDefault();
                    if (typeof fecharModalReserva === 'function') {
                        fecharModalReserva();
                    } else if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Modal) {
                        KingdomConfeitaria.Modal.hide('modalReserva');
                    } else {
                        // Fallback: usar Bootstrap diretamente
                        var modalElement = document.getElementById('modalReserva');
                        if (modalElement && typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                            var modal = bootstrap.Modal.getInstance(modalElement);
                            if (modal) {
                                modal.hide();
                            }
                        }
                    }
                });
            }
            
            // Validação dos campos de email e telefone (se preenchidos manualmente)
            if (emailReserva) {
                emailReserva.addEventListener('input', function(e) {
                    if (typeof DefaultPage !== 'undefined' && DefaultPage.Validacao) {
                        DefaultPage.Validacao.validarEmail(e.target);
                    }
                });
            }

            if (telefoneReserva) {
                telefoneReserva.addEventListener('input', function(e) {
                    var value = e.target.value.replace(/\D/g, '');
                    if (value.length <= 11) {
                        if (value.length <= 10) {
                            value = value.replace(/^(\d{2})(\d{4})(\d{0,4}).*/, '($1) $2-$3');
                        } else {
                            value = value.replace(/^(\d{2})(\d{5})(\d{0,4}).*/, '($1) $2-$3');
                        }
                        e.target.value = value;
                    }
                    if (typeof DefaultPage !== 'undefined' && DefaultPage.Validacao) {
                        DefaultPage.Validacao.validarTelefone(e.target);
                    }
                });
            }

            if (nomeReserva) {
                nomeReserva.addEventListener('input', function(e) {
                    if (typeof DefaultPage !== 'undefined' && DefaultPage.Validacao) {
                        DefaultPage.Validacao.validarNome(e.target);
                    }
                });
            }
            
            // Atualizar função de validação do formulário com ClientIDs
            if (typeof DefaultPage !== 'undefined' && DefaultPage.ModalReserva) {
                var originalValidar = DefaultPage.ModalReserva.validarFormulario;
                DefaultPage.ModalReserva.validarFormulario = function() {
                    var modal = document.getElementById('modalReserva');
                    if (!modal || !modal.classList.contains('show')) {
                        return true;
                    }

                    var nome = document.getElementById(ClientIDs.txtNome);
                    var email = document.getElementById(ClientIDs.txtEmail);
                    var telefone = document.getElementById(ClientIDs.txtTelefone);
                    // Verificar data de retirada selecionada
                    var dataRetirada = document.querySelector('input[name="dataRetirada"]:checked');
                    var hdnDataRetirada = document.getElementById(ClientIDs.hdnDataRetirada);
                    var radioGroupDatas = document.getElementById(ClientIDs.radioGroupDatas);
                    
                    // Verificar também os hidden fields (caso os campos readonly estejam vazios)
                    var hdnNome = document.getElementById(ClientIDs.hdnNome);
                    var hdnEmail = document.getElementById(ClientIDs.hdnEmail);
                    var hdnTelefone = document.getElementById(ClientIDs.hdnTelefone);
                    
                    // Se os campos visuais estiverem vazios (readonly), verificar hidden fields
                    var nomeValor = (nome && nome.value) ? nome.value.trim() : (hdnNome ? hdnNome.value : '');
                    var emailValor = (email && email.value) ? email.value.trim() : (hdnEmail ? hdnEmail.value : '');
                    var telefoneValor = (telefone && telefone.value) ? telefone.value.trim() : (hdnTelefone ? hdnTelefone.value : '');

                    var primeiroInvalido = null;
                    var nomeValido = nomeValor.length >= 3;
                    if (!nomeValido && !primeiroInvalido) primeiroInvalido = nome || hdnNome;
                    var emailValido = emailValor.length > 0 && emailValor.indexOf('@') > -1;
                    if (!emailValido && !primeiroInvalido) primeiroInvalido = email || hdnEmail;
                    var telefoneValorLimpo = telefoneValor.replace(/\D/g, '');
                    var telefoneValido = telefoneValorLimpo.length >= 10 && telefoneValorLimpo.length <= 11;
                    if (!telefoneValido && !primeiroInvalido) primeiroInvalido = telefone || hdnTelefone;
                    
                    // Validar data de retirada
                    var dataValida = false;
                    var dataValor = '';
                    if (hdnDataRetirada && hdnDataRetirada.value) {
                        dataValida = true;
                        dataValor = hdnDataRetirada.value;
                    } else if (radioGroupDatas) {
                        var radios = radioGroupDatas.querySelectorAll('input[type="radio"]:checked');
                        if (radios && radios.length > 0) {
                            dataValida = true;
                            dataValor = radios[0].value;
                        }
                    } else if (dataRetirada && dataRetirada.value) {
                        dataValida = true;
                        dataValor = dataRetirada.value;
                    }
                    
                    if (!dataValida && !primeiroInvalido) {
                        primeiroInvalido = dataRetirada || radioGroupDatas;
                        // Destacar a seção de data de retirada
                        var secaoData = document.querySelector('.secao-data-retirada');
                        if (secaoData) {
                            secaoData.style.animation = 'shake 0.5s';
                            setTimeout(function() { secaoData.style.animation = ''; }, 500);
                        }
                        if (radioGroupDatas) {
                            radioGroupDatas.style.border = '2px solid #dc3545';
                            radioGroupDatas.style.borderRadius = '4px';
                            radioGroupDatas.style.padding = '10px';
                            setTimeout(function() {
                                radioGroupDatas.style.border = '';
                                radioGroupDatas.style.padding = '';
                            }, 3000);
                        }
                    }

                    if (!nomeValido || !emailValido || !telefoneValido || !dataValida) {
                        if (primeiroInvalido) {
                            try {
                                primeiroInvalido.focus();
                                primeiroInvalido.scrollIntoView({ behavior: 'smooth', block: 'center' });
                            } catch (e) {}
                        }
                        if (!dataValida) {
                            alert('Por favor, selecione uma data de retirada antes de confirmar a reserva.');
                        } else {
                            alert('Por favor, preencha todos os campos obrigatórios corretamente.');
                        }
                        return false;
                    }
                    return true;
                };
            }
            
            // Função global para validação do formulário de reserva (usada pelo OnClientClick)
            window.validarFormularioReserva = function() {
                // Validação será feita na página de reserva
                return true;
            };
            
            // Iniciar a função de login dinâmico
            initLoginDinamico();
        })();
    </script>
    
    <!-- Componente Modal de Login Dinâmico -->
    <script>
        // Função global para abrir modal de login (redireciona para Login.aspx)
        function abrirModalLogin() {
            window.location.href = 'paginas/Login.aspx';
        }
        
        // Função global para fechar modal de login (não mais necessária, mas mantida para compatibilidade)
        function fecharModalLogin() {
            // Não faz nada, mantida apenas para compatibilidade
        }
        
        // Inicializar componente de login dinâmico standalone
        (function() {
            var loginStandaloneInicializado = false;
            var tentativasLoginStandalone = 0;
            var maxTentativasLoginStandalone = 20; // Máximo de 1 segundo (20 * 50ms)
            
            function initLoginStandalone() {
                if (loginStandaloneInicializado) return;
                
                if (typeof KingdomConfeitaria === 'undefined' || !KingdomConfeitaria.Utils) {
                    tentativasLoginStandalone++;
                    if (tentativasLoginStandalone < maxTentativasLoginStandalone) {
                        setTimeout(initLoginStandalone, 50);
                    } else {
                        console.warn('KingdomConfeitaria não carregou para login standalone após ' + maxTentativasLoginStandalone + ' tentativas');
                    }
                    return;
                }
                
                loginStandaloneInicializado = true;
                
                KingdomConfeitaria.Utils.ready(function() {
            var txtLoginStandalone = document.getElementById('txtLoginDinamicoStandalone');
            var txtSenhaStandalone = document.getElementById('txtSenhaStandalone');
            var btnConfirmarLoginStandalone = document.getElementById('btnConfirmarLoginStandalone');
            var clienteEncontradoStandalone = null;
            
            if (!txtLoginStandalone) return;
            
            // Função para mostrar mensagem
            function mostrarMensagemStandalone(mensagem, tipo) {
                var divMensagem = document.getElementById('divMensagemLoginStandalone');
                if (!divMensagem) return;
                divMensagem.style.display = 'block';
                divMensagem.className = 'mt-2 alert alert-' + (tipo || 'info');
                divMensagem.innerHTML = mensagem;
            }
            
            function ocultarMensagemStandalone() {
                var divMensagem = document.getElementById('divMensagemLoginStandalone');
                if (divMensagem) {
                    divMensagem.style.display = 'none';
                    divMensagem.innerHTML = '';
                }
            }
            
            // Função para verificar cliente
            function verificarClienteStandalone(login) {
                if (!login || login.trim() === '') {
                    ocultarMensagemStandalone();
                    return;
                }
                
                // Chamar Handler para verificar cliente (sem ScriptManager)
                mostrarMensagemStandalone('<i class="fas fa-spinner fa-spin"></i> Verificando...', 'info');
                
                KingdomConfeitaria.Ajax.callHandler(
                    'Handlers/CallbackHandler.ashx',
                    {
                        acao: 'verificarcliente',
                        login: login
                    },
                    'POST',
                    function(result) {
                        if (result && result.existe) {
                        clienteEncontradoStandalone = result.cliente;
                        
                        var divOpcaoCadastro = document.getElementById('divOpcaoCadastroStandalone');
                        var divBotoesLogin = document.getElementById('divBotoesLoginStandalone');
                        var divSenha = document.getElementById('divSenhaStandalone');
                        
                        if (divOpcaoCadastro) divOpcaoCadastro.style.display = 'none';
                        
                        if (result.temSenha) {
                            // Mostrar campo de senha
                            if (divSenha) {
                                divSenha.style.display = 'block';
                                setTimeout(function() {
                                    if (txtSenhaStandalone) txtSenhaStandalone.focus();
                                }, 100);
                            }
                            if (divBotoesLogin) divBotoesLogin.style.display = 'flex';
                            mostrarMensagemStandalone('<i class="fas fa-user-check"></i> Cliente encontrado! Digite sua senha.', 'success');
                        } else {
                            // Cliente sem senha - fazer login automático
                            fazerLoginStandalone(result.cliente);
                        }
                    } else {
                        clienteEncontradoStandalone = null;
                        var divSenha = document.getElementById('divSenhaStandalone');
                        var divBotoesLogin = document.getElementById('divBotoesLoginStandalone');
                        var divOpcaoCadastro = document.getElementById('divOpcaoCadastroStandalone');
                        var linkIrCadastro = document.getElementById('linkIrCadastroStandalone');
                        
                        if (divSenha) divSenha.style.display = 'none';
                        if (divBotoesLogin) divBotoesLogin.style.display = 'none';
                        if (divOpcaoCadastro) divOpcaoCadastro.style.display = 'block';
                        if (linkIrCadastro) {
                            var isEmail = login.indexOf('@') > -1;
                            var loginParam = isEmail ? 'email' : 'telefone';
                            linkIrCadastro.href = 'Cadastro.aspx?' + loginParam + '=' + encodeURIComponent(login);
                        }
                        mostrarMensagemStandalone('<i class="fas fa-info-circle"></i> Cliente não encontrado. Clique no botão abaixo para se cadastrar.', 'info');
                    }
                }, function(error) {
                    // Erro ao verificar cliente
                    mostrarMensagemStandalone('<i class="fas fa-exclamation-triangle"></i> Erro ao verificar cliente. Tente novamente.', 'danger');
                });
            }
            
            // Função para fazer login
            function fazerLoginStandalone(cliente) {
                if (!cliente) return;
                
                // Chamar Handler para fazer login (sem ScriptManager)
                mostrarMensagemStandalone('<i class="fas fa-spinner fa-spin"></i> Fazendo login...', 'info');
                
                KingdomConfeitaria.Ajax.callHandler(
                    'Handlers/CallbackHandler.ashx',
                    {
                        acao: 'fazerlogin',
                        clienteId: cliente.id
                    },
                    'POST',
                    function(result) {
                        if (result && result.sucesso) {
                        ocultarMensagemStandalone();
                        
                        // Atualizar variável global
                        window.usuarioLogado = true;
                        
                        // Atualizar menu do header
                        setTimeout(function() {
                            var linkLogin = document.querySelector('[id*="linkLogin"]');
                            var linkMinhasReservas = document.querySelector('[id*="linkMinhasReservas"]');
                            var linkMeusDados = document.querySelector('[id*="linkMeusDados"]');
                            var linkLogout = document.querySelector('[id*="linkLogout"]');
                            var clienteNome = document.querySelector('[id*="clienteNome"]');
                            
                            if (linkLogin) linkLogin.style.display = 'none';
                            if (linkMinhasReservas) linkMinhasReservas.style.display = 'inline';
                            if (linkMeusDados) linkMeusDados.style.display = 'inline';
                            if (linkLogout) linkLogout.style.display = 'inline';
                            if (clienteNome && cliente.nome) {
                                clienteNome.textContent = 'Olá, ' + cliente.nome;
                                clienteNome.style.display = 'inline';
                            }
                            
                            // Atualizar isAdmin do cliente se retornado
                            if (result.isAdmin !== undefined) {
                                cliente.isAdmin = result.isAdmin;
                            }
                            
                            // Mostrar link de admin se for administrador
                            var linkAdmin = document.querySelector('[id*="linkAdmin"]');
                            if (cliente.isAdmin || (result.isAdmin === true)) {
                                if (linkAdmin) linkAdmin.style.display = 'inline';
                            } else {
                                if (linkAdmin) linkAdmin.style.display = 'none';
                            }
                        }, 100);
                        
                        // Redirecionar para Minhas Reservas imediatamente
                        window.location.href = 'paginas/MinhasReservas.aspx';
                    } else {
                        mostrarMensagemStandalone('<i class="fas fa-exclamation-triangle"></i> Erro ao fazer login: ' + (result.mensagem || 'Erro desconhecido'), 'danger');
                    }
                }, function(error) {
                    // Erro ao fazer login
                    mostrarMensagemStandalone('<i class="fas fa-exclamation-triangle"></i> Erro ao fazer login. Tente novamente.', 'danger');
                });
            }
            
            // Função para validar senha
            function validarSenhaStandalone() {
                if (!clienteEncontradoStandalone) {
                    mostrarMensagemStandalone('<i class="fas fa-exclamation-triangle"></i> Cliente não encontrado. Por favor, digite seu email ou telefone primeiro.', 'warning');
                    return;
                }
                
                var senha = txtSenhaStandalone ? txtSenhaStandalone.value : '';
                var login = txtLoginStandalone ? txtLoginStandalone.value.trim() : '';
                
                if (!senha) {
                    mostrarMensagemStandalone('<i class="fas fa-exclamation-triangle"></i> Por favor, digite sua senha.', 'warning');
                    if (txtSenhaStandalone) txtSenhaStandalone.focus();
                    return;
                }
                
                // Chamar Handler para validar senha (sem ScriptManager)
                mostrarMensagemStandalone('<i class="fas fa-spinner fa-spin"></i> Validando senha...', 'info');
                
                KingdomConfeitaria.Ajax.callHandler(
                    'Handlers/CallbackHandler.ashx',
                    {
                        acao: 'validarsenha',
                        login: login,
                        senha: senha
                    },
                    'POST',
                    function(result) {
                        if (result && result.valido) {
                        fazerLoginStandalone(result.cliente);
                    } else {
                        mostrarMensagemStandalone('<i class="fas fa-times-circle"></i> ' + (result.mensagem || 'Senha incorreta.'), 'danger');
                        if (txtSenhaStandalone) {
                            txtSenhaStandalone.value = '';
                            txtSenhaStandalone.focus();
                        }
                    }
                }, function(error) {
                    // Erro ao validar senha
                    mostrarMensagemStandalone('<i class="fas fa-exclamation-triangle"></i> Erro ao validar senha. Tente novamente.', 'danger');
                });
            }
            
            // Event listeners
            var timeoutVerificacao = null;
            
            txtLoginStandalone.addEventListener('input', function() {
                clearTimeout(timeoutVerificacao);
                var login = this.value.trim();
                
                // Filtrar entrada
                var isEmail = login.indexOf('@') > -1 || /[a-zA-Z]/.test(login);
                if (isEmail) {
                    this.value = login.toLowerCase().replace(/[^a-z0-9@._-]/g, '');
                } else {
                    this.value = login.replace(/\D/g, '');
                }
                
                if (login.length >= 3) {
                    timeoutVerificacao = setTimeout(function() {
                        verificarClienteStandalone(login);
                    }, 500);
                } else {
                    ocultarMensagemStandalone();
                    var divSenha = document.getElementById('divSenhaStandalone');
                    var divBotoesLogin = document.getElementById('divBotoesLoginStandalone');
                    var divOpcaoCadastro = document.getElementById('divOpcaoCadastroStandalone');
                    if (divSenha) divSenha.style.display = 'none';
                    if (divBotoesLogin) divBotoesLogin.style.display = 'none';
                    if (divOpcaoCadastro) divOpcaoCadastro.style.display = 'none';
                }
            });
            
            if (txtSenhaStandalone) {
                txtSenhaStandalone.addEventListener('keypress', function(e) {
                    if (e.key === 'Enter') {
                        e.preventDefault();
                        validarSenhaStandalone();
                    }
                });
            }
            
            if (btnConfirmarLoginStandalone) {
                btnConfirmarLoginStandalone.addEventListener('click', function(e) {
                    e.preventDefault();
                    validarSenhaStandalone();
                });
            }
        });
            }
            initLoginStandalone();
        })();
    </script>
    
    <!-- Funções para Modal de Produto e Carrinho -->
    <script>
        var produtoAtual = null;
        var quantidadeAtual = 1;
        
        // Função para abrir modal a partir do card (usa data attribute)
        function abrirModalProdutoFromCard(cardElement) {
            var produtoJson = cardElement.getAttribute('data-produto-json');
            if (produtoJson) {
                navegarParaProduto(produtoJson);
            }
        }
        
        function navegarParaProduto(produtoJson) {
            try {
                var produto;
                if (typeof produtoJson === 'string') {
                    // Decodificar HTML entities e fazer parse do JSON
                    var jsonStr = produtoJson
                        .replace(/&quot;/g, '"')
                        .replace(/&#39;/g, "'")
                        .replace(/&amp;/g, '&')
                        .replace(/&lt;/g, '<')
                        .replace(/&gt;/g, '>');
                    produto = JSON.parse(jsonStr);
                } else {
                    produto = produtoJson;
                }
                // Navegar para página do produto (produto já está salvo na sessão pelo servidor)
                if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Navigation) {
                    KingdomConfeitaria.Navigation.navegar('paginas/Produto.aspx?id=' + produto.id);
                } else {
                    window.location.href = 'paginas/Produto.aspx?id=' + produto.id;
                }
            } catch (e) {
                console.error('Erro ao navegar para produto:', e);
                alert('Erro ao abrir detalhes do produto. Tente novamente.');
            }
        }
        
        function abrirModalCarrinho() {
            // Navegar para página do carrinho
            if (typeof KingdomConfeitaria !== 'undefined' && KingdomConfeitaria.Navigation) {
                KingdomConfeitaria.Navigation.navegar('paginas/Carrinho.aspx');
            } else {
                window.location.href = 'paginas/Carrinho.aspx';
            }
        }
        
        // Função para ajustar o espaçamento do header dinamicamente
        function ajustarEspacamentoHeader() {
            var header = document.querySelector('.header-logo');
            var spacer = document.getElementById('headerSpacer');
            
            if (header && spacer) {
                // Obter a altura real do header
                var headerHeight = header.offsetHeight;
                
                // Ajustar a altura do espaçador
                spacer.style.height = headerHeight + 'px';
                spacer.style.minHeight = headerHeight + 'px';
            }
        }
        
        // Ajustar quando a página carregar
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', ajustarEspacamentoHeader);
        } else {
            ajustarEspacamentoHeader();
        }
        
        // Ajustar quando a janela redimensionar
        window.addEventListener('resize', ajustarEspacamentoHeader);
        
        // Observar mudanças no header usando MutationObserver
        var headerObserver = new MutationObserver(function(mutations) {
            ajustarEspacamentoHeader();
        });
        
        var headerElement = document.querySelector('.header-logo');
        if (headerElement) {
            headerObserver.observe(headerElement, {
                childList: true,
                subtree: true,
                attributes: true,
                attributeFilter: ['style', 'class']
            });
        }
        
        // Ajustar após um pequeno delay para garantir que o DOM esteja totalmente renderizado
        var headerAjustado = false;
        function ajustarEspacamentoHeaderOnce() {
            if (headerAjustado) return;
            headerAjustado = true;
            ajustarEspacamentoHeader();
        }
        
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function() {
                setTimeout(ajustarEspacamentoHeaderOnce, 100);
            });
        } else {
            setTimeout(ajustarEspacamentoHeaderOnce, 100);
        }
        
        // Adicionar ícone ao botão ASP.NET de confirmar reserva
        function adicionarIconeBotaoReserva() {
            try {
                // Verificar se ClientIDs está definido
                if (typeof window.ClientIDs === 'undefined' || !window.ClientIDs.btnConfirmarReserva) {
                    console.warn('ClientIDs não está definido ainda. Tentando novamente...');
                    return false;
                }
                
                var btnConfirmarReserva = document.getElementById(window.ClientIDs.btnConfirmarReserva);
                if (btnConfirmarReserva) {
                    var textoAtual = btnConfirmarReserva.textContent || btnConfirmarReserva.innerText || '';
                    if (!btnConfirmarReserva.querySelector('i')) {
                        // Preservar o texto "Confirma" e adicionar ícone
                        if (textoAtual.trim() === '' || textoAtual.trim() === 'Confirma') {
                            btnConfirmarReserva.innerHTML = '<i class="fas fa-check"></i> Confirma';
                        } else {
                            btnConfirmarReserva.innerHTML = '<i class="fas fa-check"></i> ' + textoAtual.trim();
                        }
                    } else {
                        // Se já tem ícone, garantir que o texto está presente
                        var textoComIcone = btnConfirmarReserva.innerHTML || '';
                        if (textoComIcone.indexOf('Confirma') === -1) {
                            btnConfirmarReserva.innerHTML = '<i class="fas fa-check"></i> Confirma';
                        }
                    }
                    return true;
                }
            } catch (e) {
                console.error('Erro ao adicionar ícone ao botão:', e);
            }
            return false;
        }
        
        // Executar quando a página carregar - apenas uma vez
        var iconeBotaoAdicionado = false;
        var tentativasIcone = 0;
        var maxTentativasIcone = 20; // Aumentar tentativas
        function adicionarIconeBotaoReservaOnce() {
            if (iconeBotaoAdicionado) return;
            
            // Verificar se ClientIDs está disponível
            if (typeof window.ClientIDs === 'undefined' || !window.ClientIDs) {
                tentativasIcone++;
                if (tentativasIcone < maxTentativasIcone) {
                    setTimeout(adicionarIconeBotaoReservaOnce, 300);
                } else {
                    console.warn('ClientIDs não foi definido após ' + maxTentativasIcone + ' tentativas');
                }
                return;
            }
            
            // Verificar se btnConfirmarReserva existe (pode ser string vazia se o controle não existir)
            if (!window.ClientIDs.btnConfirmarReserva || window.ClientIDs.btnConfirmarReserva === '') {
                // Se o controle não existe, não há problema - apenas retornar
                return;
            }
            
            if (adicionarIconeBotaoReserva()) {
                iconeBotaoAdicionado = true;
            } else {
                tentativasIcone++;
                if (tentativasIcone < maxTentativasIcone) {
                    setTimeout(adicionarIconeBotaoReservaOnce, 300);
                }
            }
        }
        
        // Aguardar um pouco mais antes de tentar pela primeira vez
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function() {
                setTimeout(adicionarIconeBotaoReservaOnce, 500);
            });
        } else {
            setTimeout(adicionarIconeBotaoReservaOnce, 500);
        }
        
        // Observar mudanças no botão - com debounce
        var btnObserverTimeout = null;
        var btnObserver = new MutationObserver(function(mutations) {
            if (btnObserverTimeout) clearTimeout(btnObserverTimeout);
            btnObserverTimeout = setTimeout(function() {
                adicionarIconeBotaoReserva();
            }, 100);
        });
        
        var btnElement = document.getElementById(ClientIDs.btnConfirmarReserva);
        if (btnElement) {
            btnObserver.observe(btnElement, {
                childList: true,
                subtree: true,
                attributes: true
            });
        }
        
        // Animação da mãozinha clicando
        function iniciarAnimacaoMao() {
            var maozinha = document.getElementById('maozinhaClique');
            var produtosContainer = document.getElementById(ClientIDs.produtosContainer);
            
            if (!maozinha || !produtosContainer) return;
            
            // Verificar se já foi mostrada antes (usando sessionStorage)
            if (sessionStorage.getItem('maozinhaMostrada') === 'true') {
                return;
            }
            
            // Aguardar produtos carregarem
            var produtos = produtosContainer.querySelectorAll('.produto-card-carrossel');
            if (produtos.length === 0) {
                // Tentar novamente após um delay
                setTimeout(iniciarAnimacaoMao, 500);
                return;
            }
            
            // Pegar o primeiro produto
            var primeiroProduto = produtos[0];
            if (!primeiroProduto) return;
            
            // Calcular posição do primeiro produto
            var produtoRect = primeiroProduto.getBoundingClientRect();
            var carrosselRect = produtosContainer.parentElement.getBoundingClientRect();
            
            // Posicionar a mãozinha no centro do primeiro produto
            var left = produtoRect.left - carrosselRect.left + (produtoRect.width / 2) - 20;
            var top = produtoRect.top - carrosselRect.top + (produtoRect.height / 2) - 20;
            
            maozinha.style.left = left + 'px';
            maozinha.style.top = top + 'px';
            
            // Mostrar a mãozinha
            setTimeout(function() {
                maozinha.classList.add('visivel');
            }, 1000); // Aparecer após 1 segundo
            
            // Esconder após 5 segundos
            setTimeout(function() {
                maozinha.classList.remove('visivel');
                sessionStorage.setItem('maozinhaMostrada', 'true');
            }, 6000);
            
            // Esconder se o usuário clicar em qualquer produto
            produtos.forEach(function(produto) {
                produto.addEventListener('click', function() {
                    maozinha.classList.remove('visivel');
                    sessionStorage.setItem('maozinhaMostrada', 'true');
                }, { once: true });
            });
        }
        
        // Iniciar animação quando a página carregar - apenas uma vez
        var animacaoMaoIniciada = false;
        var tentativasAnimacaoMao = 0;
        var maxTentativasAnimacaoMao = 10; // Máximo de 5 segundos (10 * 500ms)
        
        function iniciarAnimacaoMaoOnce() {
            if (animacaoMaoIniciada) return;
            
            var produtosContainer = document.getElementById(ClientIDs.produtosContainer);
            var produtos = produtosContainer ? produtosContainer.querySelectorAll('.produto-card-carrossel') : [];
            
            if (produtos.length === 0) {
                tentativasAnimacaoMao++;
                if (tentativasAnimacaoMao < maxTentativasAnimacaoMao) {
                    setTimeout(iniciarAnimacaoMaoOnce, 500);
                }
                return;
            }
            
            animacaoMaoIniciada = true;
            iniciarAnimacaoMao();
        }
        
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function() {
                setTimeout(iniciarAnimacaoMaoOnce, 500);
            });
        } else {
            setTimeout(iniciarAnimacaoMaoOnce, 500);
        }
        
        // Ajustar posição quando a janela redimensionar
        window.addEventListener('resize', function() {
            var maozinha = document.getElementById('maozinhaClique');
            if (maozinha && maozinha.classList.contains('visivel')) {
                iniciarAnimacaoMao();
            }
        });
        // Função JavaScript para atualizar hidden field
        function atualizarDataRetirada(valor) {
            var hdnDataRetirada = document.getElementById(ClientIDs.hdnDataRetirada);
            if (hdnDataRetirada) {
                hdnDataRetirada.value = valor;
            }
        }
        
        // Navegação do carrossel de produtos
        var navegacaoCarrosselInicializada = false;
        var tentativasNavegacaoCarrossel = 0;
        var maxTentativasNavegacaoCarrossel = 10; // Máximo de 5 segundos (10 * 500ms)
        
        function inicializarNavegacaoCarrossel() {
            if (navegacaoCarrosselInicializada) return;
            
            var produtosContainer = document.getElementById(ClientIDs.produtosContainer);
            var btnEsquerda = document.getElementById('btnCarrosselEsquerda');
            var btnDireita = document.getElementById('btnCarrosselDireita');
            
            if (!produtosContainer || !btnEsquerda || !btnDireita) {
                tentativasNavegacaoCarrossel++;
                if (tentativasNavegacaoCarrossel < maxTentativasNavegacaoCarrossel) {
                    setTimeout(inicializarNavegacaoCarrossel, 500);
                }
                return;
            }
            
            navegacaoCarrosselInicializada = true;
            
            // Função para atualizar visibilidade dos botões
            function atualizarBotoesNavegacao() {
                var scrollLeft = produtosContainer.scrollLeft;
                var scrollWidth = produtosContainer.scrollWidth;
                var clientWidth = produtosContainer.clientWidth;
                var maxScroll = scrollWidth - clientWidth;
                
                // Mostrar/ocultar botão esquerda
                if (scrollLeft > 10) {
                    btnEsquerda.classList.remove('oculto');
                } else {
                    btnEsquerda.classList.add('oculto');
                }
                
                // Mostrar/ocultar botão direita
                if (scrollLeft < maxScroll - 10) {
                    btnDireita.classList.remove('oculto');
                } else {
                    btnDireita.classList.add('oculto');
                }
            }
            
            // Função para navegar para a esquerda
            function navegarEsquerda() {
                var cardWidth = 140 + 12; // largura do card + gap
                produtosContainer.scrollBy({
                    left: -cardWidth * 2, // mover 2 cards por vez
                    behavior: 'smooth'
                });
            }
            
            // Função para navegar para a direita
            function navegarDireita() {
                var cardWidth = 140 + 12; // largura do card + gap
                produtosContainer.scrollBy({
                    left: cardWidth * 2, // mover 2 cards por vez
                    behavior: 'smooth'
                });
            }
            
            // Event listeners
            btnEsquerda.addEventListener('click', navegarEsquerda);
            btnDireita.addEventListener('click', navegarDireita);
            
            // Atualizar botões quando o scroll mudar
            produtosContainer.addEventListener('scroll', atualizarBotoesNavegacao);
            
            // Atualizar botões quando a janela redimensionar - com debounce
            var resizeTimeout = null;
            window.addEventListener('resize', function() {
                if (resizeTimeout) clearTimeout(resizeTimeout);
                resizeTimeout = setTimeout(atualizarBotoesNavegacao, 100);
            });
            
            // Atualizar botões inicialmente
            setTimeout(atualizarBotoesNavegacao, 100);
        }
        
        // Inicializar navegação quando a página carregar - apenas uma vez
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function() {
                setTimeout(inicializarNavegacaoCarrossel, 100);
            });
        } else {
            setTimeout(inicializarNavegacaoCarrossel, 100);
        }
        
        // Funções para controle de quantidade no carrinho
        function aumentarQuantidadeCarrinho(produtoId, tamanho) {
            DefaultPage.Carrinho.atualizarQuantidade(produtoId, tamanho, 1);
        }
        
        function diminuirQuantidadeCarrinho(produtoId, tamanho) {
            DefaultPage.Carrinho.atualizarQuantidade(produtoId, tamanho, -1);
        }
        
        function adicionarMaisItem(produtoId, tamanho) {
            DefaultPage.Carrinho.atualizarQuantidade(produtoId, tamanho, 1);
        }
        
        /**
         * Função para reservar produto rapidamente (adiciona ao carrinho diretamente)
         * Refatorada para usar data attributes do botão, tornando o código mais limpo e seguro
         * @param {HTMLElement} buttonElement - Elemento do botão clicado
         */
        function reservarProdutoRapido(buttonElement) {
            // Prevenir propagação do evento (event está disponível no escopo global quando chamado via onclick)
            try {
                if (typeof event !== 'undefined' && event) {
                    event.stopPropagation();
                    event.preventDefault();
                }
            } catch (e) {
                // Ignorar se event não estiver disponível
            }
            
            try {
                // Validar se o elemento do botão foi fornecido
                if (!buttonElement) {
                    console.error('Elemento do botão não fornecido');
                    return;
                }
                
                // Obter o card do produto
                var card = buttonElement.closest('.produto-card-carrossel');
                if (!card) {
                    console.error('Card do produto não encontrado');
                    alert('Erro: Não foi possível identificar o produto.');
                    return;
                }
                
                // Obter dados do produto dos data attributes
                var produtoId = card.getAttribute('data-produto-id');
                var produtoJson = card.getAttribute('data-produto-json');
                var precoStr = card.getAttribute('data-produto-preco');
                
                console.log('Dados obtidos do card:', {
                    produtoId: produtoId,
                    temJson: !!produtoJson,
                    precoStr: precoStr,
                    tipoPreco: typeof precoStr
                });
                
                // Validar ID do produto
                if (!produtoId || produtoId === '' || produtoId === 'null' || produtoId === 'undefined') {
                    console.error('ID do produto inválido:', produtoId);
                    alert('Erro: ID do produto inválido.');
                    return;
                }
                
                // Tentar obter dados do JSON se disponível
                var nome = '';
                var tamanho = 'Único';
                var produtoData = null;
                var ehSacoPromocional = false;
                
                if (produtoJson) {
                    try {
                        // Decodificar HTML entities e fazer parse do JSON
                        var jsonStr = produtoJson
                            .replace(/&quot;/g, '"')
                            .replace(/&#39;/g, "'")
                            .replace(/&amp;/g, '&')
                            .replace(/&lt;/g, '<')
                            .replace(/&gt;/g, '>');
                        produtoData = JSON.parse(jsonStr);
                        nome = produtoData.nome || '';
                        tamanho = produtoData.tamanho || 'Único';
                        ehSacoPromocional = produtoData.ehSaco === true && 
                                          produtoData.produtosPermitidos && 
                                          produtoData.produtosPermitidos.length > 0;
                    } catch (e) {
                        console.warn('Erro ao parsear JSON do produto, usando valores padrão:', e);
                    }
                }
                
                // Se for saco promocional, abrir modal ao invés de adicionar ao carrinho
                if (ehSacoPromocional) {
                    console.log('Produto é saco promocional, abrindo modal de detalhes');
                    abrirModalProdutoFromCard(card);
                    return;
                }
                
                // Se não conseguiu obter do JSON, tentar obter do DOM
                if (!nome || nome === '') {
                    var nomeElement = card.querySelector('.produto-nome-carrossel');
                    if (nomeElement) {
                        nome = nomeElement.textContent.trim();
                    }
                }
                
                // Validar nome do produto
                if (!nome || nome === '') {
                    console.error('Nome do produto não encontrado');
                    alert('Erro: Nome do produto não encontrado.');
                    return;
                }
                
                // Validar e normalizar preço
                if (!precoStr || precoStr === '' || precoStr === 'undefined' || precoStr === 'null' || precoStr === 'NaN') {
                    console.error('Preço inválido no data attribute:', precoStr);
                    alert('Erro: Preço inválido. Não foi possível adicionar o produto ao carrinho.');
                    return;
                }
                
                // Converter para string e limpar
                var precoStrLimpo = String(precoStr).trim();
                
                // Remover caracteres não numéricos exceto ponto e vírgula
                precoStrLimpo = precoStrLimpo.replace(/[^\d.,]/g, '');
                
                // Normalizar preço - garantir que use ponto como separador decimal
                var precoNormalizado = precoStrLimpo.replace(',', '.');
                
                // Se tiver múltiplos pontos, manter apenas o primeiro
                var partes = precoNormalizado.split('.');
                if (partes.length > 2) {
                    precoNormalizado = partes[0] + '.' + partes.slice(1).join('');
                }
                
                // Validar se é um número válido e maior que zero
                var precoNum = parseFloat(precoNormalizado);
                if (isNaN(precoNum) || precoNum <= 0) {
                    console.error('Preço não é um número válido ou é zero. Original:', precoStr, 'Limpo:', precoStrLimpo, 'Normalizado:', precoNormalizado, 'Número:', precoNum);
                    alert('Erro: Preço inválido: ' + precoStr);
                    return;
                }
                
                // Garantir que o preço normalizado seja uma string com formato correto
                precoNormalizado = precoNum.toFixed(2);
                
                console.log('Preço processado - Original:', precoStr, 'Normalizado:', precoNormalizado, 'Número:', precoNum);
                
                // Garantir que o tamanho tenha um valor válido
                if (!tamanho || tamanho === '' || tamanho === 'undefined' || tamanho === 'null') {
                    tamanho = 'Único';
                }
                
                // Desabilitar botão temporariamente para evitar cliques múltiplos
                var originalDisabled = buttonElement.disabled;
                var originalHtml = buttonElement.innerHTML;
                buttonElement.disabled = true;
                
                // Feedback visual - mostrar loading
                buttonElement.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Adicionando...';
                buttonElement.style.opacity = '0.7';
                
                // Adicionar produto ao carrinho
                try {
                    console.log('Chamando DefaultPage.Carrinho.adicionar com:', {
                        produtoId: parseInt(produtoId),
                        nome: nome,
                        tamanho: tamanho,
                        quantidade: 1,
                        preco: precoNormalizado,
                        tipoPreco: typeof precoNormalizado
                    });
                    
                    DefaultPage.Carrinho.adicionar(
                        parseInt(produtoId), 
                        nome, 
                        tamanho, 
                        1, 
                        precoNormalizado
                    );
                    
                    // Feedback visual - sucesso
                    buttonElement.innerHTML = '<i class="fas fa-check"></i> Adicionado!';
                    buttonElement.style.background = '#28a745';
                    buttonElement.style.opacity = '1';
                    
                    // Restaurar botão após 2 segundos
                    setTimeout(function() {
                        buttonElement.innerHTML = originalHtml;
                        buttonElement.style.background = '';
                        buttonElement.style.opacity = '1';
                        buttonElement.disabled = originalDisabled;
                    }, 2000);
                    
                } catch (carrinhoError) {
                    console.error('Erro ao adicionar ao carrinho:', carrinhoError);
                    
                    // Feedback visual - erro
                    buttonElement.innerHTML = '<i class="fas fa-exclamation-triangle"></i> Erro';
                    buttonElement.style.background = '#dc3545';
                    buttonElement.style.opacity = '1';
                    
                    // Restaurar botão após 2 segundos
                    setTimeout(function() {
                        buttonElement.innerHTML = originalHtml;
                        buttonElement.style.background = '';
                        buttonElement.style.opacity = '1';
                        buttonElement.disabled = originalDisabled;
                    }, 2000);
                    
                    alert('Erro ao adicionar produto ao carrinho. Por favor, tente novamente.');
                }
                
            } catch (e) {
                console.error('Erro ao reservar produto:', e, e.stack);
                alert('Erro ao adicionar produto ao carrinho. Por favor, tente novamente.');
                
                // Restaurar botão em caso de erro
                if (buttonElement) {
                    var originalHtml = buttonElement.innerHTML;
                    buttonElement.innerHTML = originalHtml;
                    buttonElement.style.background = '';
                    buttonElement.style.opacity = '1';
                    buttonElement.disabled = false;
                }
            }
        }
    </script>
</body>
</html>



