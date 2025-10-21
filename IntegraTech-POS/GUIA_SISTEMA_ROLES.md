# 🔐 Sistema de Roles y Permisos - IntegraTech POS

## 📋 Descripción General

El sistema implementa un control de acceso basado en roles (RBAC - Role-Based Access Control) con 3 niveles de permisos:

1. **Administrador** (Admin) - Control total
2. **Gerente** - Acceso a mayoría de funciones
3. **Cajero** - Acceso limitado a ventas y reportes

---

## 👥 Roles y Permisos

### 🔴 **ADMINISTRADOR (Admin)**

#### Acceso a Páginas:
- ✅ **Todas las páginas del sistema**
- ✅ Home
- ✅ Ventas
- ✅ Productos (ver, crear, editar, eliminar)
- ✅ Reportes
- ✅ Sistema
- ✅ Usuarios (gestión completa)

#### Acciones Permitidas:
- ✅ Crear, editar y eliminar productos
- ✅ Realizar ventas
- ✅ Ver y generar reportes
- ✅ Gestionar usuarios (crear, editar, desactivar)
- ✅ Acceder a configuración del sistema
- ✅ Ajustar inventario
- ✅ Procesar devoluciones
- ✅ Ver auditoría completa

---

### 🟡 **GERENTE**

#### Acceso a Páginas:
- ✅ Home
- ✅ Ventas
- ✅ Productos (ver, crear, editar, eliminar)
- ✅ Reportes
- ❌ Sistema (bloqueado)
- ❌ Usuarios (bloqueado)

#### Acciones Permitidas:
- ✅ Crear, editar y eliminar productos
- ✅ Realizar ventas
- ✅ Ver y generar reportes completos
- ✅ Ajustar inventario
- ✅ Procesar devoluciones
- ❌ No puede gestionar usuarios
- ❌ No puede acceder a configuración del sistema

---

### 🔵 **CAJERO**

#### Acceso a Páginas:
- ✅ Home
- ✅ Ventas
- ✅ Reportes (solo visualización)
- ❌ Productos (bloqueado)
- ❌ Sistema (bloqueado)
- ❌ Usuarios (bloqueado)

#### Acciones Permitidas:
- ✅ Realizar ventas
- ✅ Ver reportes (solo lectura)
- ✅ Consultar información de productos
- ❌ No puede crear, editar o eliminar productos
- ❌ No puede ajustar inventario
- ❌ No puede procesar devoluciones sin autorización
- ❌ No puede gestionar usuarios
- ❌ No puede acceder a configuración

---

## 🚀 Cómo Usar el Sistema

### 1️⃣ Iniciar Sesión

```
1. Ejecutar la aplicación
2. Navegas a la pantalla de login
3. Ingresar credenciales
4. El sistema redirige según el rol
```

#### Credenciales por Defecto:

**Administrador:**
- Usuario: `admin`
- Contraseña: `admin123`
- Redirige a: Home (/)

**Cajero (Crear desde Admin):**
- Redirige a: Ventas (/ventas)

**Gerente (Crear desde Admin):**
- Redirige a: Home (/)

---

### 2️⃣ Crear Usuarios

Solo el **Administrador** puede crear usuarios:

1. Iniciar sesión como Admin
2. Ir a la sección "Usuarios" en el menú
3. Clic en "Nuevo Usuario"
4. Llenar formulario:
   - Nombre de usuario
   - Nombre completo
   - Rol (Admin/Gerente/Cajero)
   - Email
   - Contraseña
5. Guardar

---

### 3️⃣ Navegación Según Rol

El **menú lateral** se adapta automáticamente:

#### Administrador ve:
```
- Home
- Ventas
- Productos
- Reportes
- Sistema
- Usuarios
- Cerrar Sesión
```

#### Gerente ve:
```
- Home
- Ventas
- Productos
- Reportes
- Cerrar Sesión
```

#### Cajero ve:
```
- Home
- Ventas
- Reportes
- Cerrar Sesión
```

---

## 🔒 Implementación Técnica

### Archivos Clave:

```
Services/
└── AuthService.cs          ← Servicio de autenticación

Components/
├── AuthorizeView.razor     ← Componente de autorización
└── Layout/
    └── NavMenu.razor       ← Menú adaptativo por rol

Pages/
├── Login.razor             ← Pantalla de login
├── AccesoDenegado.razor    ← Página de acceso denegado
├── Productos.razor         ← Protegida (Admin/Gerente)
├── Sistema.razor           ← Protegida (Solo Admin)
└── Usuarios.razor          ← Protegida (Solo Admin)
```

---

## 💻 Uso en Código

### Verificar Rol del Usuario:

```csharp
@inject AuthService AuthService

// Verificar si es administrador
@if (AuthService.EsAdministrador())
{
    <button>Acción de Admin</button>
}

// Verificar si es gerente
@if (AuthService.EsGerente())
{
    <button>Acción de Gerente</button>
}

// Verificar si es cajero
@if (AuthService.EsCajero())
{
    <button>Acción de Cajero</button>
}
```

### Proteger una Página Completa:

```csharp
@inject AuthService AuthService
@inject NavigationManager Navigation

@if (!AuthService.EsAdministrador())
{
    <div class="alert alert-danger">
        Acceso Denegado - Solo Administradores
    </div>
}
else
{
    <!-- Contenido de la página -->
}

@code {
    protected override void OnInitialized()
    {
        if (!AuthService.EsAdministrador())
        {
            Navigation.NavigateTo("/acceso-denegado");
        }
    }
}
```

### Verificar Permiso para Acción Específica:

```csharp
// Verificar si puede editar productos
if (AuthService.PuedeRealizarAccion("EditarProducto"))
{
    // Permitir edición
}

// Verificar acceso a página
if (AuthService.TieneAccesoAPagina("/productos"))
{
    // Permitir navegación
}
```

---

## 🎨 Componente AuthorizeView

Componente reutilizable para proteger secciones:

```razor
<AuthorizeView RolRequerido="Admin">
    <ChildContent>
        <!-- Solo Admin puede ver esto -->
        <button>Eliminar Usuario</button>
    </ChildContent>
    <NotAuthorized>
        <p>No tienes permisos</p>
    </NotAuthorized>
</AuthorizeView>
```

O verificar acciones:

```razor
<AuthorizeView AccionRequerida="EditarProducto">
    <ChildContent>
        <button>Editar Producto</button>
    </ChildContent>
</AuthorizeView>
```

---

## 🔄 Flujo de Autenticación

```
1. Usuario ingresa credenciales
   ↓
2. UsuarioService.LoginAsync() verifica credenciales
   ↓
3. Si es válido → AuthService.SetUsuario(usuario)
   ↓
4. Sistema redirige según rol:
   - Admin → /
   - Gerente → /
   - Cajero → /ventas
   ↓
5. NavMenu se actualiza según rol
   ↓
6. Páginas verifican permisos en OnInitialized()
```

---

## 🛡️ Seguridad Implementada

### ✅ Verificación en Frontend:
- Menú oculta opciones no permitidas
- Páginas verifican permisos al cargar
- Componentes verifican antes de mostrar

### ✅ Verificación en Servicios:
- UsuarioService verifica permisos antes de ejecutar acciones
- DatabaseService registra auditoría de acciones

### ✅ Redirección Automática:
- Si no está autenticado → `/login`
- Si no tiene permisos → `/acceso-denegado`

---

## 📝 Ejemplos Prácticos

### Ejemplo 1: Botón Solo para Admin

```razor
@inject AuthService AuthService

@if (AuthService.EsAdministrador())
{
    <button class="btn btn-danger" @onclick="EliminarTodo">
        <i class="bi bi-trash"></i> Eliminar Todo
    </button>
}
```

### Ejemplo 2: Sección Condicional

```razor
<div class="card">
    <div class="card-header">
        <h5>Opciones Avanzadas</h5>
    </div>
    <div class="card-body">
        @if (AuthService.EsAdministrador() || AuthService.EsGerente())
        {
            <button class="btn btn-warning">Ajustar Inventario</button>
            <button class="btn btn-info">Procesar Devolución</button>
        }
        else
        {
            <p class="text-muted">
                Solo Gerentes y Administradores pueden realizar ajustes.
            </p>
        }
    </div>
</div>
```

### Ejemplo 3: Información de Usuario Actual

```razor
@inject AuthService AuthService

@if (AuthService.IsAuthenticated)
{
    <div class="alert alert-info">
        <strong>Usuario:</strong> @AuthService.UsuarioActual?.NombreCompleto<br>
        <strong>Rol:</strong> <span class="badge bg-primary">@AuthService.RolActual</span>
    </div>
}
```

---

## ⚙️ Configuración de Permisos

Para modificar permisos, editar `Services/AuthService.cs`:

```csharp
public bool TieneAccesoAPagina(string pagina)
{
    // Admin tiene acceso a todo
    if (EsAdministrador()) return true;

    // Definir permisos por rol
    var permisosGerente = new[]
    {
        "/", "/home", "/ventas", "/productos", "/reportes"
    };

    var permisosCajero = new[]
    {
        "/", "/home", "/ventas", "/reportes"
    };

    // ... resto del código
}
```

---

## 🎯 Casos de Uso

### Caso 1: Cajero Intenta Acceder a Productos
```
1. Cajero hace clic en URL /productos
2. Página verifica: !EsAdministrador() && !EsGerente()
3. Muestra mensaje: "Acceso Denegado"
4. Opción de volver a /ventas
```

### Caso 2: Gerente Intenta Gestionar Usuarios
```
1. Gerente no ve opción "Usuarios" en menú
2. Si intenta acceder directo a /usuarios
3. OnInitialized() verifica: !EsAdministrador()
4. Redirige a /acceso-denegado
```

### Caso 3: Admin Crea Nuevo Cajero
```
1. Admin va a /usuarios
2. Clic en "Nuevo Usuario"
3. Llena formulario y asigna rol "Cajero"
4. Nuevo usuario creado
5. Cajero puede login con credenciales
6. Automáticamente limitado a Ventas y Reportes
```

---

## 📊 Tabla Resumen de Permisos

| Página/Acción | Admin | Gerente | Cajero |
|---------------|-------|---------|--------|
| Home | ✅ | ✅ | ✅ |
| Ventas | ✅ | ✅ | ✅ |
| Productos (Ver) | ✅ | ✅ | ❌ |
| Productos (Crear/Editar) | ✅ | ✅ | ❌ |
| Reportes (Ver) | ✅ | ✅ | ✅ |
| Sistema | ✅ | ❌ | ❌ |
| Usuarios | ✅ | ❌ | ❌ |
| Ajustar Inventario | ✅ | ✅ | ❌ |
| Devoluciones | ✅ | ✅ | ❌ |
| Auditoría | ✅ | ❌ | ❌ |

---

## 🚨 Troubleshooting

### Problema: Usuario no puede acceder aunque tiene permisos
**Solución:** Verificar que AuthService esté registrado como Singleton en MauiProgram.cs

### Problema: Menú no se actualiza después del login
**Solución:** AuthService.OnAuthStateChanged debe estar suscrito en NavMenu

### Problema: Redirige en loop a /login
**Solución:** Verificar que la página /login no requiera autenticación

---

## ✅ Checklist de Implementación

- [x] AuthService creado y registrado
- [x] Login.razor actualizado con AuthService
- [x] NavMenu adaptativo por rol
- [x] Página AccesoDenegado.razor
- [x] Productos.razor protegida (Admin/Gerente)
- [x] Sistema.razor protegida (Solo Admin)
- [x] Usuarios.razor protegida (Solo Admin)
- [x] Verificación en OnInitialized de páginas
- [x] Componente AuthorizeView (opcional)
- [x] Documentación completa

---

## 🎉 Conclusión

El sistema de roles está completamente implementado y funcional. Cada usuario solo ve y puede acceder a las funciones permitidas según su rol, garantizando la seguridad y organización del sistema.

**Usuario por defecto:**
- Usuario: `admin`
- Contraseña: `admin123`
- Rol: Administrador

**¡Listo para usar!** 🚀

---

**Fecha:** 3 de Octubre, 2025
**Versión:** 2.1 (con Sistema de Roles)
