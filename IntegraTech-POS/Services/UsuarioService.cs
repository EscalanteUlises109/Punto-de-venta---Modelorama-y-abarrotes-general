using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntegraTech_POS.Models;
using IntegraTech_POS.Helpers;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace IntegraTech_POS.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<UsuarioService> _logger;
        private readonly IValidator<Usuario> _validator;

        public UsuarioService(
            DatabaseService databaseService, 
            ILogger<UsuarioService> logger,
            IValidator<Usuario> validator)
        {
            _databaseService = databaseService;
            _logger = logger;
            _validator = validator;
        }

        public async Task<Usuario?> LoginAsync(string nombreUsuario, string password)
        {
            try
            {
                
                nombreUsuario = nombreUsuario?.Trim() ?? string.Empty;
                password = password?.Trim() ?? string.Empty;

                _logger.LogInformation("Intento de login para usuario: '{NombreUsuario}' (length: {Length})", nombreUsuario, password.Length);
                
                var usuario = await _databaseService.GetUsuarioByNombreAsync(nombreUsuario);
                
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado: '{NombreUsuario}'", nombreUsuario);
                    return null;
                }

                if (!usuario.Activo)
                {
                    _logger.LogWarning("Usuario inactivo: '{NombreUsuario}'", nombreUsuario);
                    return null;
                }

                
#if DEBUG
                var hashGenerado = SecurityHelper.HashPassword(password);
                string Mask(string h) => string.IsNullOrEmpty(h) ? "" : (h.Length > 6 ? h.Substring(0, 6) + "***" : "***");
                _logger.LogDebug("Hash generado (parcial): {Hash}", Mask(hashGenerado));
                _logger.LogDebug("Hash en BD (parcial): {Hash}", Mask(usuario.PasswordHash));
                _logger.LogDebug("Â¿Coinciden? {Coinciden}", hashGenerado == usuario.PasswordHash);
#endif

                if (!SecurityHelper.VerifyByAlgorithm(password, usuario.PasswordHash, usuario.PasswordAlgorithm))
                {
                    _logger.LogWarning("ContraseÃ±a incorrecta para usuario: '{NombreUsuario}'", nombreUsuario);
                    return null;
                }

                
                usuario.UltimoAcceso = DateTime.Now;
                await _databaseService.SaveUsuarioAsync(usuario);

                
                if (!string.Equals(usuario.PasswordAlgorithm, "BCRYPT", StringComparison.OrdinalIgnoreCase))
                {
                    usuario.PasswordHash = SecurityHelper.HashPasswordBCrypt(password);
                    usuario.PasswordAlgorithm = "BCRYPT";
                    await _databaseService.SaveUsuarioAsync(usuario);
                    _logger.LogInformation("Usuario {NombreUsuario} migrado a BCRYPT", usuario.NombreUsuario);
                }
                
                
                await _databaseService.RegistrarAuditoriaAsync(usuario.Id_Usuario, usuario.NombreUsuario, "Login", "Usuario iniciÃ³ sesiÃ³n");

                _logger.LogInformation("âœ… Login exitoso para usuario: '{NombreUsuario}' - Rol: {Rol}", nombreUsuario, usuario.Rol);
                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "âŒ Error durante login para usuario: '{NombreUsuario}'", nombreUsuario);
                return null;
            }
        }

        public async Task<bool> CreateUsuarioAsync(Usuario usuario, string password)
        {
            try
            {
                
                var passwordLimpia = password?.Trim() ?? string.Empty;
                
                _logger.LogInformation("========================================");
                _logger.LogInformation("Creando usuario: {Usuario}", usuario.NombreUsuario);
                _logger.LogInformation("Password length: {Len} caracteres", passwordLimpia.Length);
                
                
                usuario.PasswordHash = SecurityHelper.HashPassword(passwordLimpia);
                _logger.LogInformation("Hash generado correctamente");
                
                
                var verificacionInmediata = SecurityHelper.VerifyPassword(passwordLimpia, usuario.PasswordHash);
                _logger.LogInformation("VerificaciÃ³n inmediata: {Resultado}", verificacionInmediata ? "âœ… OK" : "âŒ FALLO");
                _logger.LogInformation("========================================");
                
                
                var validationResult = await _validator.ValidateAsync(usuario);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("ValidaciÃ³n fallida al crear usuario: {Errors}", 
                        string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                    return false;
                }

                var result = await _databaseService.SaveUsuarioAsync(usuario);
                
                if (result > 0)
                {
                    _logger.LogInformation("âœ… Usuario creado exitosamente: {NombreUsuario} (ID: {Id})", usuario.NombreUsuario, result);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario: {NombreUsuario}", usuario.NombreUsuario);
                return false;
            }
        }

        public async Task<bool> UpdateUsuarioAsync(Usuario usuario)
        {
            try
            {
                var result = await _databaseService.SaveUsuarioAsync(usuario);
                
                if (result > 0)
                {
                    _logger.LogInformation("Usuario actualizado: {NombreUsuario}", usuario.NombreUsuario);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario: {Id}", usuario.Id_Usuario);
                return false;
            }
        }

        public async Task<bool> DeleteUsuarioAsync(int id)
        {
            try
            {
                var usuario = await _databaseService.GetUsuarioByIdAsync(id);
                if (usuario != null)
                {
                    
                    usuario.Activo = false;
                    var result = await _databaseService.SaveUsuarioAsync(usuario);
                    
                    if (result > 0)
                    {
                        _logger.LogInformation("Usuario desactivado: {Id}", id);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario: {Id}", id);
                return false;
            }
        }

        public async Task<Usuario?> GetUsuarioByIdAsync(int id)
        {
            try
            {
                return await _databaseService.GetUsuarioByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por ID: {Id}", id);
                return null;
            }
        }

        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            try
            {
                return await _databaseService.GetUsuariosAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lista de usuarios");
                return new List<Usuario>();
            }
        }

        public async Task<bool> CambiarPasswordAsync(int usuarioId, string passwordActual, string passwordNueva)
        {
            try
            {
                var usuario = await _databaseService.GetUsuarioByIdAsync(usuarioId);
                
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado para cambio de contraseÃ±a: {Id}", usuarioId);
                    return false;
                }

                if (!SecurityHelper.VerifyByAlgorithm(passwordActual, usuario.PasswordHash, usuario.PasswordAlgorithm))
                {
                    _logger.LogWarning("ContraseÃ±a actual incorrecta para usuario: {Id}", usuarioId);
                    return false;
                }

                usuario.PasswordHash = SecurityHelper.HashPasswordBCrypt(passwordNueva);
                usuario.PasswordAlgorithm = "BCRYPT";
                var result = await _databaseService.SaveUsuarioAsync(usuario);
                
                if (result > 0)
                {
                    await _databaseService.RegistrarAuditoriaAsync(usuarioId, usuario.NombreUsuario, "CambioPassword", "Usuario cambiÃ³ su contraseÃ±a");
                    _logger.LogInformation("ContraseÃ±a cambiada para usuario: {Id}", usuarioId);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseÃ±a para usuario: {Id}", usuarioId);
                return false;
            }
        }

        public async Task<bool> VerificarPermisoAsync(int usuarioId, string accion)
        {
            try
            {
                var usuario = await _databaseService.GetUsuarioByIdAsync(usuarioId);
                
                if (usuario == null || !usuario.Activo)
                    return false;

                
                var permisos = new Dictionary<string, List<string>>
                {
                    ["Admin"] = new List<string> { "Todo" },
                    ["Gerente"] = new List<string> { "Ventas", "Productos", "Reportes", "EditarProductos", "EliminarProductos" },
                    ["Cajero"] = new List<string> { "Ventas", "ConsultarProductos" }
                };

                if (usuario.Rol == "Admin")
                    return true;

                if (permisos.ContainsKey(usuario.Rol))
                {
                    return permisos[usuario.Rol].Contains(accion);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar permisos para usuario: {Id}", usuarioId);
                return false;
            }
        }
    }
}

