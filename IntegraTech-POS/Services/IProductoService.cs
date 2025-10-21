using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntegraTech_POS.Models;

namespace IntegraTech_POS.Services
{
    public interface IProductoService
    {
        Task<List<Producto>> GetProductosAsync();
        Task<Producto?> GetProductoByIdAsync(int id);
        Task<bool> CreateProductoAsync(Producto producto);
        Task<bool> UpdateProductoAsync(Producto producto);
        Task<bool> DeleteProductoAsync(int id);
    }
}
