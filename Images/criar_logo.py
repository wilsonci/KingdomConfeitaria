#!/usr/bin/env python3
"""
Script para criar logo da Kingdom Confeitaria
"""

from PIL import Image, ImageDraw, ImageFont
import os

# Cores do tema (verde esmeralda)
COR_FUNDO = (26, 77, 46)  # #1a4d2e - verde escuro
COR_TEXTO = (255, 255, 255)  # Branco
COR_DECORACAO = (45, 90, 61)  # Verde médio

def criar_logo():
    """Cria o logo da Kingdom Confeitaria"""
    # Criar imagem (largura maior para texto)
    largura = 600
    altura = 200
    img = Image.new('RGB', (largura, altura), color=COR_FUNDO)
    draw = ImageDraw.Draw(img)
    
    # Tentar usar uma fonte maior
    try:
        # Tentar fontes comuns do Windows
        font_titulo = ImageFont.truetype("arial.ttf", 48)
        font_subtitulo = ImageFont.truetype("arial.ttf", 24)
    except:
        try:
            font_titulo = ImageFont.truetype("C:/Windows/Fonts/arial.ttf", 48)
            font_subtitulo = ImageFont.truetype("C:/Windows/Fonts/arial.ttf", 24)
        except:
            # Usar fonte padrão se não encontrar
            font_titulo = ImageFont.load_default()
            font_subtitulo = ImageFont.load_default()
    
    # Desenhar coroa decorativa no lado esquerdo
    centro_y = altura // 2
    margem_esquerda = 30
    
    # Coroa simples
    pontos_coroa = [
        (margem_esquerda + 20, centro_y - 30),
        (margem_esquerda + 40, centro_y - 50),
        (margem_esquerda + 50, centro_y - 40),
        (margem_esquerda + 60, centro_y - 50),
        (margem_esquerda + 70, centro_y - 30),
        (margem_esquerda + 70, centro_y + 30),
        (margem_esquerda + 20, centro_y + 30)
    ]
    draw.polygon(pontos_coroa, fill=COR_DECORACAO, outline=COR_TEXTO, width=2)
    
    # Texto "Kingdom"
    texto_kingdom = "Kingdom"
    bbox_kingdom = draw.textbbox((0, 0), texto_kingdom, font=font_titulo)
    largura_kingdom = bbox_kingdom[2] - bbox_kingdom[0]
    altura_kingdom = bbox_kingdom[3] - bbox_kingdom[1]
    x_kingdom = margem_esquerda + 100
    y_kingdom = centro_y - altura_kingdom // 2 - 15
    draw.text((x_kingdom, y_kingdom), texto_kingdom, fill=COR_TEXTO, font=font_titulo)
    
    # Texto "Confeitaria"
    texto_confeitaria = "Confeitaria"
    bbox_conf = draw.textbbox((0, 0), texto_confeitaria, font=font_subtitulo)
    largura_conf = bbox_conf[2] - bbox_conf[0]
    x_conf = x_kingdom
    y_conf = y_kingdom + altura_kingdom + 5
    draw.text((x_conf, y_conf), texto_confeitaria, fill=COR_TEXTO, font=font_subtitulo)
    
    # Salvar como PNG
    caminho_logo = os.path.join(os.path.dirname(os.path.abspath(__file__)), "logo-kingdom-confeitaria.png")
    img.save(caminho_logo, 'PNG')
    print(f"Logo criado: {caminho_logo}")
    print(f"Tamanho: {largura}x{altura} pixels")

if __name__ == "__main__":
    criar_logo()

