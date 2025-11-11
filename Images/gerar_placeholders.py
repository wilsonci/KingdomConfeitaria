#!/usr/bin/env python3
"""
Script para gerar imagens placeholder de gingerbread cookies
Usa Pillow (PIL) para criar imagens simples
"""

from PIL import Image, ImageDraw, ImageFont
import os

# Cores do gingerbread
COR_FUNDO = (245, 230, 211)  # Bege claro
COR_BISCOITO = (210, 105, 30)  # Chocolate
COR_DECORACAO = (139, 69, 19)  # Marrom escuro
COR_BRANCO = (255, 255, 255)

def criar_imagem_gingerbread(nome_arquivo, formato="estrela"):
    """Cria uma imagem placeholder de gingerbread cookie"""
    # Criar imagem
    img = Image.new('RGB', (400, 400), color=COR_FUNDO)
    draw = ImageDraw.Draw(img)
    
    # Desenhar biscoito base (círculo)
    centro_x, centro_y = 200, 200
    raio = 120
    
    # Desenhar formato específico
    if formato == "estrela":
        # Desenhar estrela
        pontos_estrela = []
        for i in range(10):
            angulo = i * 3.14159 / 5 - 3.14159 / 2
            if i % 2 == 0:
                r = raio
            else:
                r = raio * 0.5
            x = centro_x + r * 0.8 * __import__('math').cos(angulo)
            y = centro_y + r * 0.8 * __import__('math').sin(angulo)
            pontos_estrela.append((x, y))
        draw.polygon(pontos_estrela, fill=COR_BISCOITO, outline=COR_DECORACAO, width=3)
    elif formato == "floco-neve":
        # Desenhar floco de neve (hexágono com linhas)
        import math
        pontos = []
        for i in range(6):
            angulo = i * math.pi / 3
            x = centro_x + raio * 0.8 * math.cos(angulo)
            y = centro_y + raio * 0.8 * math.sin(angulo)
            pontos.append((x, y))
        draw.polygon(pontos, fill=COR_BISCOITO, outline=COR_DECORACAO, width=3)
        # Linhas do floco
        for i in range(6):
            angulo = i * math.pi / 3
            x2 = centro_x + raio * 1.2 * math.cos(angulo)
            y2 = centro_y + raio * 1.2 * math.sin(angulo)
            draw.line([(centro_x, centro_y), (x2, y2)], fill=COR_DECORACAO, width=2)
    elif formato == "coracao":
        # Desenhar coração
        import math
        pontos = []
        for t in range(0, 100):
            t = t / 100.0 * 2 * math.pi
            x = centro_x + 16 * (math.sin(t) ** 3) * 4
            y = centro_y - (13 * math.cos(t) - 5 * math.cos(2*t) - 2 * math.cos(3*t) - math.cos(4*t)) * 2
            pontos.append((x, y))
        if len(pontos) > 2:
            draw.polygon(pontos, fill=COR_BISCOITO, outline=COR_DECORACAO, width=3)
    elif formato == "arvore":
        # Desenhar árvore de Natal
        # Tronco
        draw.rectangle([centro_x-15, centro_y+80, centro_x+15, centro_y+120], fill=COR_DECORACAO)
        # Triângulos da árvore
        draw.polygon([(centro_x, centro_y-60), (centro_x-80, centro_y+40), (centro_x+80, centro_y+40)], 
                    fill=COR_BISCOITO, outline=COR_DECORACAO, width=3)
        draw.polygon([(centro_x, centro_y-20), (centro_x-60, centro_y+60), (centro_x+60, centro_y+60)], 
                    fill=COR_BISCOITO, outline=COR_DECORACAO, width=3)
        draw.polygon([(centro_x, centro_y+20), (centro_x-40, centro_y+80), (centro_x+40, centro_y+80)], 
                    fill=COR_BISCOITO, outline=COR_DECORACAO, width=3)
    elif formato == "boneco":
        # Desenhar boneco de gingerbread
        # Cabeça
        draw.ellipse([centro_x-50, centro_y-120, centro_x+50, centro_y-20], 
                    fill=COR_BISCOITO, outline=COR_DECORACAO, width=3)
        # Corpo
        draw.rectangle([centro_x-40, centro_y-20, centro_x+40, centro_y+60], 
                      fill=COR_BISCOITO, outline=COR_DECORACAO, width=3)
        # Braços
        draw.ellipse([centro_x-80, centro_y-10, centro_x-50, centro_y+20], 
                    fill=COR_BISCOITO, outline=COR_DECORACAO, width=3)
        draw.ellipse([centro_x+50, centro_y-10, centro_x+80, centro_y+20], 
                    fill=COR_BISCOITO, outline=COR_DECORACAO, width=3)
        # Pernas
        draw.ellipse([centro_x-40, centro_y+60, centro_x-10, centro_y+110], 
                    fill=COR_BISCOITO, outline=COR_DECORACAO, width=3)
        draw.ellipse([centro_x+10, centro_y+60, centro_x+40, centro_y+110], 
                    fill=COR_BISCOITO, outline=COR_DECORACAO, width=3)
        # Olhos
        draw.ellipse([centro_x-20, centro_y-100, centro_x-10, centro_y-90], fill=COR_BRANCO)
        draw.ellipse([centro_x+10, centro_y-100, centro_x+20, centro_y-90], fill=COR_BRANCO)
        # Sorriso
        draw.arc([centro_x-20, centro_y-80, centro_x+20, centro_y-40], start=0, end=180, 
                fill=COR_DECORACAO, width=3)
    elif formato == "meia":
        # Desenhar meia
        pontos_meia = [
            (centro_x-60, centro_y-100),
            (centro_x+60, centro_y-100),
            (centro_x+60, centro_y+100),
            (centro_x+40, centro_y+100),
            (centro_x+40, centro_y+20),
            (centro_x-40, centro_y+20),
            (centro_x-40, centro_y+100),
            (centro_x-60, centro_y+100)
        ]
        draw.polygon(pontos_meia, fill=COR_BISCOITO, outline=COR_DECORACAO, width=3)
    elif formato == "guirlanda":
        # Desenhar guirlanda (círculo decorado)
        draw.ellipse([centro_x-100, centro_y-100, centro_x+100, centro_y+100], 
                    fill=None, outline=COR_BISCOITO, width=15)
        # Decorações
        for i in range(8):
            import math
            angulo = i * math.pi / 4
            x = centro_x + 80 * math.cos(angulo)
            y = centro_y + 80 * math.sin(angulo)
            draw.ellipse([x-10, y-10, x+10, y+10], fill=COR_DECORACAO)
    elif formato == "saco":
        # Desenhar saco de biscoitos
        # Corpo do saco
        pontos_saco = [
            (centro_x-80, centro_y-80),
            (centro_x+80, centro_y-80),
            (centro_x+90, centro_y+80),
            (centro_x-90, centro_y+80)
        ]
        draw.polygon(pontos_saco, fill=(139, 69, 19), outline=COR_DECORACAO, width=3)
        # Boca do saco
        draw.arc([centro_x-80, centro_y-100, centro_x+80, centro_y-60], start=180, end=0, 
                fill=COR_DECORACAO, width=5)
        # Texto
        try:
            font = ImageFont.truetype("arial.ttf", 24)
        except:
            font = ImageFont.load_default()
        draw.text((centro_x, centro_y), "6x", fill=COR_BRANCO, anchor="mm", font=font)
    else:
        # Formato padrão (círculo)
        draw.ellipse([centro_x-raio, centro_y-raio, centro_x+raio, centro_y+raio], 
                    fill=COR_BISCOITO, outline=COR_DECORACAO, width=3)
    
    # Salvar imagem
    img.save(nome_arquivo, 'JPEG', quality=85)
    print(f"Imagem criada: {nome_arquivo}")

# Mapeamento de arquivos para formatos
arquivos_formatos = {
    "estrela-pequeno.jpg": "estrela",
    "estrela-grande.jpg": "estrela",
    "floco-neve-pequeno.jpg": "floco-neve",
    "floco-neve-grande.jpg": "floco-neve",
    "guirlanda-pequeno.jpg": "guirlanda",
    "guirlanda-grande.jpg": "guirlanda",
    "meia-pequeno.jpg": "meia",
    "meia-grande.jpg": "meia",
    "arvore-pequeno.jpg": "arvore",
    "arvore-grande.jpg": "arvore",
    "coracao-pequeno.jpg": "coracao",
    "coracao-grande.jpg": "coracao",
    "boneco-pequeno.jpg": "boneco",
    "boneco-grande.jpg": "boneco",
    "saco-6-pequenos.jpg": "saco",
    "saco-3-grandes.jpg": "saco"
}

# Diretório atual
diretorio = os.path.dirname(os.path.abspath(__file__))

# Criar todas as imagens
for arquivo, formato in arquivos_formatos.items():
    caminho_completo = os.path.join(diretorio, arquivo)
    if not os.path.exists(caminho_completo):
        criar_imagem_gingerbread(caminho_completo, formato)
    else:
        print(f"Arquivo já existe: {arquivo}")

print("\nTodas as imagens foram geradas!")

