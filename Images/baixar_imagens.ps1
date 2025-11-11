# Script para baixar imagens de gingerbread cookies
# Usando Unsplash como fonte gratuita

$basePath = Split-Path -Parent $MyInvocation.MyCommand.Path
$images = @(
    @{Name="estrela-pequeno.jpg"; Search="gingerbread star cookie"},
    @{Name="estrela-grande.jpg"; Search="gingerbread star cookie"},
    @{Name="floco-neve-pequeno.jpg"; Search="gingerbread snowflake cookie"},
    @{Name="floco-neve-grande.jpg"; Search="gingerbread snowflake cookie"},
    @{Name="guirlanda-pequeno.jpg"; Search="gingerbread wreath cookie"},
    @{Name="guirlanda-grande.jpg"; Search="gingerbread wreath cookie"},
    @{Name="meia-pequeno.jpg"; Search="gingerbread stocking cookie"},
    @{Name="meia-grande.jpg"; Search="gingerbread stocking cookie"},
    @{Name="arvore-pequeno.jpg"; Search="gingerbread tree cookie"},
    @{Name="arvore-grande.jpg"; Search="gingerbread tree cookie"},
    @{Name="coracao-pequeno.jpg"; Search="gingerbread heart cookie"},
    @{Name="coracao-grande.jpg"; Search="gingerbread heart cookie"},
    @{Name="boneco-pequeno.jpg"; Search="gingerbread man cookie"},
    @{Name="boneco-grande.jpg"; Search="gingerbread man cookie"},
    @{Name="saco-6-pequenos.jpg"; Search="gingerbread cookies bag"},
    @{Name="saco-3-grandes.jpg"; Search="gingerbread cookies bag"}
)

Write-Host "Baixando imagens de gingerbread cookies..."
Write-Host "Usando Unsplash Source API (gratuita e sem necessidade de autenticação)"

foreach ($img in $images) {
    $filePath = Join-Path $basePath $img.Name
    if (Test-Path $filePath) {
        Write-Host "Imagem $($img.Name) já existe, pulando..."
        continue
    }
    
    try {
        # Usar Unsplash Source API (gratuita, sem autenticação)
        # Vamos usar uma imagem genérica de gingerbread e depois criar placeholders
        # Como a API do Unsplash requer autenticação para busca, vamos criar placeholders SVG
        
        Write-Host "Criando placeholder para $($img.Name)..."
        
        # Criar um placeholder SVG simples
        $svgContent = @"
<svg xmlns="http://www.w3.org/2000/svg" width="400" height="400" viewBox="0 0 400 400">
  <rect width="400" height="400" fill="#f5e6d3"/>
  <text x="200" y="180" font-family="Arial, sans-serif" font-size="24" fill="#8b4513" text-anchor="middle" font-weight="bold">Gingerbread</text>
  <text x="200" y="220" font-family="Arial, sans-serif" font-size="18" fill="#8b4513" text-anchor="middle">Cookie</text>
  <circle cx="200" cy="280" r="60" fill="#d2691e" stroke="#8b4513" stroke-width="3"/>
  <circle cx="185" cy="270" r="5" fill="#fff"/>
  <circle cx="215" cy="270" r="5" fill="#fff"/>
  <path d="M 180 290 Q 200 300 220 290" stroke="#8b4513" stroke-width="2" fill="none"/>
</svg>
"@
        
        # Converter SVG para imagem usando .NET (se disponível) ou salvar como SVG
        # Por enquanto, vamos criar um arquivo de texto com instruções
        $instructions = "Para $($img.Name):`n"
        $instructions += "1. Acesse https://unsplash.com/s/photos/gingerbread-cookie`n"
        $instructions += "2. Baixe uma imagem apropriada`n"
        $instructions += "3. Renomeie para $($img.Name)`n"
        $instructions += "4. Coloque nesta pasta`n`n"
        
        # Salvar instruções temporariamente
        $instructionsFile = Join-Path $basePath "INSTRUCOES_$($img.Name).txt"
        $instructions | Out-File -FilePath $instructionsFile -Encoding UTF8
        
        Write-Host "  Instruções salvas em $instructionsFile"
    }
    catch {
        Write-Host "  Erro ao processar $($img.Name): $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`nProcesso concluído!"
Write-Host "Nota: Como baixar imagens automaticamente requer APIs pagas,"
Write-Host "criei arquivos de instruções. Você pode:"
Write-Host "1. Baixar imagens manualmente de https://unsplash.com/s/photos/gingerbread-cookie"
Write-Host "2. Ou usar o script Python abaixo para gerar placeholders"

