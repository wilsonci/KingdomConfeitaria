# Configuração de Publicação de Imagens

## Mudanças Implementadas

### 1. Todas as Imagens Adicionadas ao Projeto
- ✅ **16 imagens** adicionadas como Content no arquivo `.csproj`
- ✅ Todas configuradas com `CopyToOutputDirectory: PreserveNewest`
- ✅ Imagens incluídas:
  - Logo: `logo-kingdom-confeitaria.png`, `logo-kingdom-confeitaria.svg`
  - Produtos: `arvore-grande.jpg`, `arvore-pequeno.jpg`
  - Produtos: `boneco-grande.jpg`, `boneco-pequeno.jpg`
  - Produtos: `coracao-grande.jpg`, `coracao-pequeno.jpg`
  - Produtos: `estrela-grande.jpg`, `estrela-pequeno.jpg`
  - Produtos: `floco-neve-grande.jpg`, `floco-neve-pequeno.jpg`
  - Produtos: `guirlanda-grande.jpg`, `guirlanda-pequeno.jpg`
  - Produtos: `meia-grande.jpg`, `meia-pequeno.jpg`
  - Sacos: `saco-3-grandes.jpg`, `saco-6-pequenos.jpg`

### 2. Targets MSBuild Criados

#### Target: `CopyImagesToPublish`
- ✅ Executa após `CopyAllFilesToSingleFolderForPackage`
- ✅ Copia imagens durante a publicação
- ✅ Suporta múltiplos métodos de publicação:
  - `PackageLocation` (Web Deploy Package)
  - `PublishUrl` (FTP, File System)
  - `WPPAllFilesInSingleFolder` (Web Publishing Pipeline)

#### Target: `CopyImagesToOutput`
- ✅ Executa após `Build`
- ✅ Copia imagens durante o build normal (desenvolvimento)
- ✅ Garante que imagens estejam disponíveis em `bin\Images\`

### 3. Arquivos Excluídos
- ✅ Scripts PowerShell (`.ps1`) - não copiados
- ✅ Scripts Python (`.py`) - não copiados
- ✅ Arquivos de texto (`.txt`) - não copiados
- ✅ Documentação Markdown (`.md`) - não copiados

## Como Funciona

### Durante o Build (Desenvolvimento)
1. Projeto compila normalmente
2. Target `CopyImagesToOutput` executa
3. Imagens são copiadas para `bin\Images\`
4. Aplicação pode usar as imagens localmente

### Durante a Publicação
1. Projeto é publicado
2. Target `CopyImagesToPublish` executa
3. Imagens são copiadas para a pasta de publicação
4. Estrutura de pastas é preservada (`Images\`)

## Verificação

Após publicar, verifique se a pasta `Images\` existe no diretório de publicação e contém todas as imagens.

## Métodos de Publicação Suportados

- ✅ **Web Deploy (Package)**: Usa `PackageLocation`
- ✅ **FTP**: Usa `PublishUrl`
- ✅ **File System**: Usa `PublishUrl` ou `WPPAllFilesInSingleFolder`
- ✅ **Visual Studio Publish**: Funciona automaticamente

## Notas Importantes

1. **PreserveNewest**: Imagens só são copiadas se forem mais novas que as existentes
2. **Estrutura de Pastas**: A estrutura `Images\` é preservada
3. **Arquivos Excluídos**: Scripts e documentação não são copiados (economiza espaço)
4. **Build e Publish**: Funciona tanto no build normal quanto na publicação

## Troubleshooting

### Imagens não aparecem após publicação
1. Verifique se a pasta `Images\` existe no diretório de publicação
2. Verifique se as imagens estão na pasta
3. Verifique permissões da pasta `Images\` no servidor

### Imagens não são copiadas
1. Verifique se as imagens estão listadas no arquivo `.csproj`
2. Verifique se o build foi executado com sucesso
3. Verifique os logs de publicação para mensagens de erro

