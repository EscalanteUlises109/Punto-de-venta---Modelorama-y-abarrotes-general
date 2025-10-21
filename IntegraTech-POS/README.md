## Punto de venta — Modelorama y abarrotes (IntegraTech-POS)

Hola, soy el autor de este proyecto. Aquí te explico, con mis propias palabras, qué hace, cómo está organizado y, sobre todo, cómo uso la base de datos (SQLite) y el “SQL” del sistema. Este repo está pensado para operar un punto de venta de Modelorama/abarrotes de forma práctica, rápida y 100% local (sin internet).

- Repo en GitHub: https://github.com/EscalanteUlises109/Punto-de-venta---Modelorama-y-abarrotes-general
- Contacto: L21211937@tectijuana.edu.mx

## Qué es y cómo funciona

Construí el sistema en .NET MAUI con Blazor. Eso me permite:
- Interfaz moderna y responsiva (Blazor + Razor Components).
- Ejecutar nativo en Windows con .NET 8.
- Guardar todo localmente con SQLite; no dependo de servidores externos.

Funciones principales:
- Ventas con carrito y actualización automática de inventario.
- Gestión de productos (código de barras único, stock mínimo, categorías/distribuidores, imagen opcional).
- Promociones vinculadas a productos.
- Usuarios, roles y auditoría de acciones clave.
- Caja (apertura/cierre) y devoluciones.
- Reportes: ventas por fecha, totales, ganancias por producto/categoría y PDF.

## Estructura breve del proyecto

- Components/Pages: Interfaces de usuario (Blazor .razor) para ventas, productos, reportes, etc.
- Services: Lógica de negocio y acceso a datos. El “corazón” es `Services/DatabaseService.cs`.
- Models: Entidades persistidas en SQLite (Producto, Venta, DetalleVenta, Usuario, etc.).
- Validators: Reglas de validación con FluentValidation.
- wwwroot: recursos estáticos (css/js).
- Platforms: assets específicos de plataformas (MAUI).

## Base de datos y “SQL” en este proyecto (muy explícito)

Tipo de BD: SQLite, mediante la librería sqlite-net (SQLiteAsyncConnection). No uso Entity Framework. Al ser SQLite, no existen “procedimientos almacenados” como los de SQL Server. En su lugar, encapsulo la lógica en métodos del servicio y consultas parametrizadas. Te explico todo por partes:

1) Ubicación del archivo de la BD
- Ruta en Windows (MAUI): `FileSystem.AppDataDirectory` → archivo `IntegraTechPOS.db`.
- Puedes respaldar todo copiando ese archivo. Si lo eliminas, la app lo recrea al iniciar.

2) Creación de tablas y migraciones ligeras
- Al iniciar, el `DatabaseService.InitializeAsync()` crea las tablas con `CreateTableAsync<T>()` para: Producto, Venta, DetalleVenta, Promocion, PromocionProducto, Usuario, AuditoriaSistema, Caja, DevolucionVenta, AjusteInventario, ConfiguracionSistema.
- Si agrego columnas nuevas con el tiempo, aplico “migraciones ligeras” con `ALTER TABLE` dentro de métodos como `EnsureUsuariosSchemaAsync()` y `EnsureVentasSchemaAsync()` (p. ej., agregar PasswordAlgorithm, PagoRecibido, Cambio, etc.).
- Creo índices útiles con `ExecuteAsync("CREATE INDEX IF NOT EXISTS ...")` para acelerar consultas por fecha o joins lógicos (venta-detalles, promo-producto).

3) Cómo sustituyo los “procedimientos almacenados”
- En SQLite no hay stored procedures. Yo encapsulo la lógica transaccional y de negocio en métodos del servicio. Ejemplos reales:
   - Guardar una venta de forma atómica: `GuardarVentaConTransaccionAsync(Venta, List<DetalleVenta>, usuarioId)`.
      - Inserta la Venta.
      - Inserta los DetalleVenta vinculados.
      - Actualiza el stock de cada Producto validando que no quede negativo.
      - Registra una entrada de auditoría.
      - Todo dentro de `BeginTransaction/Commit` para no dejar datos a medias.
   - Cierres de Caja, Devoluciones y Ajustes de Inventario también actualizan consistencia y escriben auditoría cuando aplica.

4) Consultas y parámetros (seguro contra inyección)
- Con sqlite-net normalmente uso LINQ sobre `Table<T>()` y filtros `Where(...)`, que internamente generan SQL parametrizado.
- Cuando necesito SQL directo, uso `ExecuteAsync("...", params)` con parámetros, evitando concatenar strings peligrosas.
- Ejemplos de consultas implementadas: ventas por rango de fechas, sumatorias por día/mes, ganancias por producto/categoría, detalle de una venta con productos, etc.

5) Esquema de tablas (resumen)
- Productos: Id_Producto (PK), Nombre_Producto, Precio_Venta/Compra, Distribuidor, Categoria, Cantidad, Codigo_Barras (unique), Fecha_Registro, Fecha_Vencimiento, Unidad_Medida, Stock_Minimo.
- Ventas: Id_Venta (PK), Fecha_Venta, Total, PagoRecibido, Cambio, etc.
- DetalleVenta: Id (PK), VentaId (FK lógico), Id_Producto (FK lógico), Cantidad, Precio_Unitario, Subtotal.
- Usuarios: Id_Usuario, NombreUsuario, PasswordHash (+ algoritmo), Rol, Activo, ÚltimoAcceso.
- AuditoriaSistema: registra quién hizo qué, cuándo y sobre qué tabla/registro.
- Caja, DevolucionVenta, AjusteInventario, ConfiguracionSistema: apoyan operaciones de tienda y configuración.

6) Admin por defecto y configuración
- En el primer arranque creo un usuario `admin/admin123` si no existe.
- `ConfiguracionSistema` guarda pares clave-valor que uso para features y banderas.

7) Respaldo y migración de datos
- Respaldo: copia el archivo `IntegraTechPOS.db` que está en el directorio de datos de la app.
- Restauración: reemplaza ese archivo con la copia; la app arranca con esos datos.

8) Reportes y performance
- Para reportes por fecha, uso índices y filtro por rangos `[inicio, fin)` para que sea rápido.
- Calculo ganancias reales recorriendo detalles y restando costo de compra a precio de venta.

En resumen: donde otros usarían procedimientos almacenados, yo tengo métodos en `DatabaseService` que contienen la lógica, trabajan con transacciones y aseguran consistencia.

## Cómo correr el proyecto (Windows)

Requisitos: Windows 10/11, .NET 8 SDK, Visual Studio 2022 o VS Code.

Pasos rápidos:
1) Restaurar dependencias y compilar.
2) Ejecutar el target Windows: `net8.0-windows10.0.19041.0`.

Desde VS/VS Code puedes usar las tareas incluidas (Build/Run) o comandos `dotnet`.

Primer uso: al abrir la app se crea la BD y el usuario admin por defecto (admin/admin123). En Productos puedes cargar inventario, y en Ventas ya puedes vender; el stock se descuenta automáticamente.

## Roles y seguridad

- Roles principales: Admin, Gerente y Cajero. El menú se adapta según permisos.
- Contraseñas se guardan con hash. Durante login, si detecto un algoritmo antiguo, migro a BCRYPT.
- Auditoría de acciones importantes para trazabilidad.

## Distribución

- Para entregar a un cliente: `dotnet publish -c Release -r win-x64 --self-contained` y luego crear instalador (InnoSetup/WiX). Incluir acceso directo y configurar la carpeta de datos si hace falta.

## Soporte

Si necesitas ayuda, una mejora o integración nueva, escríbeme a: L21211937@tectijuana.edu.mx

— Ulises