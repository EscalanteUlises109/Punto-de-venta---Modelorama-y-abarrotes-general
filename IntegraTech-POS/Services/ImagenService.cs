using System.IO;

namespace IntegraTech_POS.Services
{
    public interface IImagenService
    {
        Task<string?> GuardarImagenAsync(Stream imagenStream, string nombreArchivo);
        Task<bool> EliminarImagenAsync(string rutaImagen);
        string ObtenerRutaCompleta(string nombreArchivo);
        Task<byte[]?> ObtenerImagenAsync(string nombreArchivo);
        string ObtenerDirectorioImagenes();
    }

    public class ImagenService : IImagenService
    {
        private readonly string _directorioImagenes;

        public ImagenService()
        {
            
            _directorioImagenes = Path.Combine(FileSystem.AppDataDirectory, "Imagenes");
            Directory.CreateDirectory(_directorioImagenes);
        }

        public async Task<string?> GuardarImagenAsync(Stream imagenStream, string nombreArchivo)
        {
            try
            {
                
                var extension = Path.GetExtension(nombreArchivo);
                var nombreUnico = $"{Guid.NewGuid()}{extension}";
                var rutaCompleta = Path.Combine(_directorioImagenes, nombreUnico);

                
                using var fileStream = new FileStream(rutaCompleta, FileMode.Create);
                await imagenStream.CopyToAsync(fileStream);

                return nombreUnico; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando imagen: {ex.Message}");
                return null;
            }
        }

        public Task<bool> EliminarImagenAsync(string rutaImagen)
        {
            try
            {
                if (!string.IsNullOrEmpty(rutaImagen))
                {
                    var rutaCompleta = ObtenerRutaCompleta(rutaImagen);
                    if (File.Exists(rutaCompleta))
                    {
                        File.Delete(rutaCompleta);
                        return Task.FromResult(true);
                    }
                }
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error eliminando imagen: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public string ObtenerRutaCompleta(string nombreArchivo)
        {
            if (string.IsNullOrEmpty(nombreArchivo))
                return "";

            return Path.Combine(_directorioImagenes, nombreArchivo);
        }

        public async Task<byte[]?> ObtenerImagenAsync(string nombreArchivo)
        {
            try
            {
                if (string.IsNullOrEmpty(nombreArchivo))
                    return null;

                var rutaCompleta = ObtenerRutaCompleta(nombreArchivo);
                if (File.Exists(rutaCompleta))
                {
                    return await File.ReadAllBytesAsync(rutaCompleta);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo imagen: {ex.Message}");
                return null;
            }
        }

        public string ObtenerDirectorioImagenes()
        {
            return _directorioImagenes;
        }
    }
}
