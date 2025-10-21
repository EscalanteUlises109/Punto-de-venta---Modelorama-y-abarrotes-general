using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntegraTech_POS.Models;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace IntegraTech_POS.Services
{
    public class ProductoService : IProductoService
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<ProductoService> _logger;
        private readonly IValidator<Producto> _validator;

        public ProductoService(
            DatabaseService databaseService, 
            ILogger<ProductoService> logger,
            IValidator<Producto> validator)
        {
            _databaseService = databaseService;
            _logger = logger;
            _validator = validator;
        }

        public async Task<List<Producto>> GetProductosAsync()
        {
            try
            {
                return await _databaseService.GetProductosAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
                return new List<Producto>();
            }
        }

        public async Task<Producto?> GetProductoByIdAsync(int id)
        {
            try
            {
                return await _databaseService.GetProductoByIdAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateProductoAsync(Producto producto)
        {
            try
            {
                
                var validationResult = await _validator.ValidateAsync(producto);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Validación fallida al crear producto: {Errors}", 
                        string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                    return false;
                }

                var result = await _databaseService.SaveProductoAsync(producto);
                
                if (result > 0)
                {
                    _logger.LogInformation("Producto creado exitosamente: {Nombre} (ID: {Id})", 
                        producto.Nombre_Producto, producto.Id_Producto);
                }
                
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto: {Nombre}", producto.Nombre_Producto);
                return false;
            }
        }

        public async Task<bool> UpdateProductoAsync(Producto producto)
        {
            try
            {
                
                var validationResult = await _validator.ValidateAsync(producto);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Validación fallida al actualizar producto ID {Id}: {Errors}", 
                        producto.Id_Producto,
                        string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                    return false;
                }

                var result = await _databaseService.SaveProductoAsync(producto);
                
                if (result > 0)
                {
                    _logger.LogInformation("Producto actualizado: {Nombre} (ID: {Id})", 
                        producto.Nombre_Producto, producto.Id_Producto);
                }
                
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar producto ID: {Id}", producto.Id_Producto);
                return false;
            }
        }

        public async Task<bool> DeleteProductoAsync(int id)
        {
            try
            {
                var producto = await _databaseService.GetProductoByIdAsync(id);
                if (producto != null)
                {
                    var result = await _databaseService.DeleteProductoAsync(producto);
                    
                    if (result > 0)
                    {
                        _logger.LogInformation("Producto eliminado: {Nombre} (ID: {Id})", 
                            producto.Nombre_Producto, id);
                    }
                    
                    return result > 0;
                }
                
                _logger.LogWarning("Intento de eliminar producto inexistente ID: {Id}", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar producto ID: {Id}", id);
                return false;
            }
        }
    }
}

