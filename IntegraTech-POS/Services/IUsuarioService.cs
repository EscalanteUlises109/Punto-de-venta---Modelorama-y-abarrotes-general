using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IntegraTech_POS.Models;

namespace IntegraTech_POS.Services
{
    public interface IUsuarioService
    {
        Task<Usuario?> LoginAsync(string nombreUsuario, string password);
        Task<bool> CreateUsuarioAsync(Usuario usuario, string password);
        Task<bool> UpdateUsuarioAsync(Usuario usuario);
        Task<bool> DeleteUsuarioAsync(int id);
        Task<Usuario?> GetUsuarioByIdAsync(int id);
        Task<List<Usuario>> GetUsuariosAsync();
        Task<bool> CambiarPasswordAsync(int usuarioId, string passwordActual, string passwordNueva);
        Task<bool> VerificarPermisoAsync(int usuarioId, string accion);
    }
}
