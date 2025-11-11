# Script simples para baixar imagens de gingerbread de fontes gratuitas
# Usa Unsplash Source API (gratuita, sem autenticação)

$ErrorActionPreference = "Continue"
$basePath = $PSScriptRoot

# Lista de imagens necessárias
$imagens = @(
    "estrela-pequeno.jpg",
    "estrela-grande.jpg",
    "floco-neve-pequeno.jpg",
    "floco-neve-grande.jpg",
    "guirlanda-pequeno.jpg",
    "guirlanda-grande.jpg",
    "meia-pequeno.jpg",
    "meia-grande.jpg",
    "arvore-pequeno.jpg",
    "arvore-grande.jpg",
    "coracao-pequeno.jpg",
    "coracao-grande.jpg",
    "boneco-pequeno.jpg",
    "boneco-grande.jpg",
    "saco-6-pequenos.jpg",
    "saco-3-grandes.jpg"
)

Write-Host "=== Baixador de Imagens de Gingerbread ===" -ForegroundColor Green
Write-Host ""

# URLs de imagens de exemplo do Unsplash (gratuitas, sem autenticação)
# Usando Unsplash Source API: https://source.unsplash.com/
# Formato: https://source.unsplash.com/400x400/?gingerbread,cookie

$baseUrl = "https://source.unsplash.com/400x400/?gingerbread,cookie"

foreach ($img in $imagens) {
    $filePath = Join-Path $basePath $img
    
    if (Test-Path $filePath) {
        Write-Host "[OK] $img já existe" -ForegroundColor Yellow
        continue
    }
    
    Write-Host "Baixando $img..." -NoNewline
    
    try {
        # Tentar baixar de Unsplash Source
        $url = $baseUrl + "&sig=$([System.Guid]::NewGuid().ToString())"
        Invoke-WebRequest -Uri $url -OutFile $filePath -TimeoutSec 10 -ErrorAction Stop
        Write-Host " [OK]" -ForegroundColor Green
    }
    catch {
        Write-Host " [ERRO]" -ForegroundColor Red
        Write-Host "  Erro: $($_.Exception.Message)" -ForegroundColor Red
        
        # Criar placeholder SVG como fallback
        Write-Host "  Criando placeholder SVG..." -NoNewline
        $svgContent = @"
<svg xmlns="http://www.w3.org/2000/svg" width="400" height="400" viewBox="0 0 400 400">
  <rect width="400" height="400" fill="#f5e6d3"/>
  <text x="200" y="180" font-family="Arial" font-size="24" fill="#8b4513" text-anchor="middle" font-weight="bold">Gingerbread</text>
  <text x="200" y="220" font-family="Arial" font-size="18" fill="#8b4513" text-anchor="middle">Cookie</text>
  <circle cx="200" cy="280" r="60" fill="#d2691e" stroke="#8b4513" stroke-width="3"/>
  <circle cx="185" cy="270" r="5" fill="#fff"/>
  <circle cx="215" cy="270" r="5" fill="#fff"/>
  <path d="M 180 290 Q 200 300 220 290" stroke="#8b4513" stroke-width="2" fill="none"/>
</svg>
"@
        $svgPath = $filePath -replace '\.jpg$', '.svg'
        $svgContent | Out-File -FilePath $svgPath -Encoding UTF8 -NoNewline
        Write-Host " [SVG criado]" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=== Processo concluído ===" -ForegroundColor Green
Write-Host ""
Write-Host "Nota: Se algumas imagens não foram baixadas, você pode:"
Write-Host "1. Baixar manualmente de https://unsplash.com/s/photos/gingerbread-cookie"
Write-Host "2. Ou usar os arquivos SVG gerados como placeholders"
Write-Host "3. Ou executar o script Python gerar_placeholders.py (requer Pillow)"

