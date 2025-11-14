using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace KingdomConfeitaria.Services
{
    public class ImageService
    {
        // Tamanhos padrão para imagens de produtos
        private const int LARGURA_MAXIMA = 800;
        private const int ALTURA_MAXIMA = 800;
        private const int LARGURA_MINIMA = 200;
        private const int ALTURA_MINIMA = 200;
        private const int QUALIDADE_JPEG = 85;

        /// <summary>
        /// Processa upload de imagem: valida, redimensiona e salva
        /// </summary>
        /// <param name="fileUpload">Arquivo enviado</param>
        /// <param name="produtoId">ID do produto para nomear o arquivo</param>
        /// <param name="pastaDestino">Pasta de destino (ex: "Images/Produtos")</param>
        /// <returns>Caminho relativo da imagem salva ou null em caso de erro</returns>
        public static string ProcessarUploadImagem(HttpPostedFile fileUpload, int produtoId, string pastaDestino = "Images/Produtos")
        {
            try
            {
                if (fileUpload == null || fileUpload.ContentLength == 0)
                {
                    return null;
                }

                // Validar tipo de arquivo
                string extensao = Path.GetExtension(fileUpload.FileName).ToLower();
                string[] extensoesPermitidas = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                
                if (Array.IndexOf(extensoesPermitidas, extensao) == -1)
                {
                    throw new Exception("Formato de arquivo não suportado. Use JPG, PNG, GIF ou WEBP.");
                }

                // Validar tamanho do arquivo (máximo 5MB)
                const int tamanhoMaximoBytes = 5 * 1024 * 1024; // 5MB
                if (fileUpload.ContentLength > tamanhoMaximoBytes)
                {
                    throw new Exception("Arquivo muito grande. Tamanho máximo: 5MB.");
                }

                // Criar pasta se não existir
                string caminhoFisico = HttpContext.Current.Server.MapPath("~/" + pastaDestino);
                if (!Directory.Exists(caminhoFisico))
                {
                    Directory.CreateDirectory(caminhoFisico);
                }

                // Gerar nome do arquivo: {produtoId}_{timestamp}.jpg
                string nomeArquivo = $"{produtoId}_{DateTime.Now:yyyyMMddHHmmss}{extensao}";
                string caminhoCompleto = Path.Combine(caminhoFisico, nomeArquivo);

                // Ler e processar imagem
                using (var imagemOriginal = Image.FromStream(fileUpload.InputStream))
                {
                    // Validar dimensões mínimas
                    if (imagemOriginal.Width < LARGURA_MINIMA || imagemOriginal.Height < ALTURA_MINIMA)
                    {
                        throw new Exception($"Imagem muito pequena. Dimensões mínimas: {LARGURA_MINIMA}x{ALTURA_MINIMA} pixels.");
                    }

                    // Calcular novo tamanho mantendo proporção
                    int novaLargura, novaAltura;
                    CalcularNovoTamanho(imagemOriginal.Width, imagemOriginal.Height, LARGURA_MAXIMA, ALTURA_MAXIMA, out novaLargura, out novaAltura);

                    // Redimensionar imagem
                    using (var imagemRedimensionada = new Bitmap(novaLargura, novaAltura))
                    {
                        using (var graphics = Graphics.FromImage(imagemRedimensionada))
                        {
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                            graphics.DrawImage(imagemOriginal, 0, 0, novaLargura, novaAltura);
                        }

                        // Salvar imagem
                        ImageFormat formato = extensao == ".png" ? ImageFormat.Png : ImageFormat.Jpeg;
                        
                        if (formato == ImageFormat.Jpeg)
                        {
                            // Salvar JPEG com qualidade
                            var encoderParameters = new EncoderParameters(1);
                            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, (long)QUALIDADE_JPEG);
                            var jpegCodec = ImageCodecInfo.GetImageDecoders().FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
                            
                            if (jpegCodec != null)
                            {
                                imagemRedimensionada.Save(caminhoCompleto, jpegCodec, encoderParameters);
                            }
                            else
                            {
                                imagemRedimensionada.Save(caminhoCompleto, ImageFormat.Jpeg);
                            }
                        }
                        else
                        {
                            imagemRedimensionada.Save(caminhoCompleto, formato);
                        }
                    }
                }

                // Retornar caminho relativo
                return $"{pastaDestino}/{nomeArquivo}";
            }
            catch (Exception ex)
            {
                // Log do erro
                LogService.Registrar("ERROR", "Sistema", "Upload de Imagem", "ImageService.cs", $"Erro ao processar upload: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Calcula novo tamanho mantendo proporção
        /// </summary>
        private static void CalcularNovoTamanho(int larguraOriginal, int alturaOriginal, int larguraMaxima, int alturaMaxima, out int novaLargura, out int novaAltura)
        {
            // Se já está dentro dos limites, manter tamanho original
            if (larguraOriginal <= larguraMaxima && alturaOriginal <= alturaMaxima)
            {
                novaLargura = larguraOriginal;
                novaAltura = alturaOriginal;
                return;
            }

            // Calcular proporção
            double proporcaoLargura = (double)larguraMaxima / larguraOriginal;
            double proporcaoAltura = (double)alturaMaxima / alturaOriginal;
            double proporcao = Math.Min(proporcaoLargura, proporcaoAltura);

            novaLargura = (int)(larguraOriginal * proporcao);
            novaAltura = (int)(alturaOriginal * proporcao);
        }

        /// <summary>
        /// Valida se o arquivo é uma imagem válida
        /// </summary>
        public static bool ValidarImagem(HttpPostedFile fileUpload)
        {
            try
            {
                if (fileUpload == null || fileUpload.ContentLength == 0)
                {
                    return false;
                }

                // Verificar extensão
                string extensao = Path.GetExtension(fileUpload.FileName).ToLower();
                string[] extensoesPermitidas = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                
                if (Array.IndexOf(extensoesPermitidas, extensao) == -1)
                {
                    return false;
                }

                // Verificar se é realmente uma imagem tentando ler
                using (var imagem = Image.FromStream(fileUpload.InputStream))
                {
                    // Verificar dimensões mínimas
                    if (imagem.Width < LARGURA_MINIMA || imagem.Height < ALTURA_MINIMA)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

