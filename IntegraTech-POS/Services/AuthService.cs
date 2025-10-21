using IntegraTech_POS.Models;

namespace IntegraTech_POS.Services
{
    
    
    
    public class AuthService
    {
        private Usuario? _usuarioActual;

        public event Action? OnAuthStateChanged;

        public Usuario? UsuarioActual
        {
            get => _usuarioActual;
            private set
            {
                _usuarioActual = value;
                OnAuthStateChanged?.Invoke();
            }
        }

        public bool IsAuthenticated => UsuarioActual != null;

        public string? RolActual => UsuarioActual?.Rol;

        
        
        
        public void SetUsuario(Usuario usuario)
        {
            UsuarioActual = usuario;
        }

        
        
        
        public void Logout()
        {
            UsuarioActual = null;
        }

        
        
        
        public bool TieneRol(string rol)
        {
            if (UsuarioActual == null) return false;
            return UsuarioActual.Rol.Equals(rol, StringComparison.OrdinalIgnoreCase);
        }

        
        
        
        public bool EsAdministrador()
        {
            return TieneRol("Admin");
        }

        
        
        
        public bool EsGerente()
        {
            return TieneRol("Gerente");
        }

        
        
        
        public bool EsCajero()
        {
            return TieneRol("Cajero");
        }

        
        
        
        public bool TieneAccesoAPagina(string pagina)
        {
            if (UsuarioActual == null) return false;

            
            if (EsAdministrador()) return true;

            
            var permisosGerente = new[]
            {
                "/", "/home", "/ventas", "/productos", "/reportes",
                "/producto/nuevo", "/producto/editar", "/producto"
            };

            var permisosCajero = new[]
            {
                "/", "/home", "/ventas", "/reportes"
            };

            if (EsGerente())
            {
                return permisosGerente.Any(p => pagina.StartsWith(p, StringComparison.OrdinalIgnoreCase));
            }

            if (EsCajero())
            {
                return permisosCajero.Any(p => pagina.StartsWith(p, StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }

        
        
        
        public bool PuedeRealizarAccion(string accion)
        {
            if (UsuarioActual == null) return false;

            
            if (EsAdministrador()) return true;

            
            var accionesGerente = new[]
            {
                "Ventas", "CrearProducto", "EditarProducto", "EliminarProducto",
                "VerReportes", "AjustarInventario", "Devoluciones"
            };

            var accionesCajero = new[]
            {
                "Ventas", "VerReportes", "ConsultarProductos"
            };

            if (EsGerente())
            {
                return accionesGerente.Contains(accion, StringComparer.OrdinalIgnoreCase);
            }

            if (EsCajero())
            {
                return accionesCajero.Contains(accion, StringComparer.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}

