using SQLite;
using IntegraTech_POS.Models;
using IntegraTech_POS.Helpers;

namespace IntegraTech_POS.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;

        public string GetDatabasePath()
        {
            return Path.Combine(FileSystem.AppDataDirectory, "IntegraTechPOS.db");
        }

        public async Task CloseConnectionAsync()
        {
            if (_database != null)
            {
                await _database.CloseAsync();
                _database = null;
                Console.WriteLine("ðŸ”Œ ConexiÃ³n a la base de datos cerrada");
            }
        }

        public async Task InitializeAsync()
        {
            if (_database != null)
                return;

            try
            {
                var databasePath = Path.Combine(FileSystem.AppDataDirectory, "IntegraTechPOS.db");
                Console.WriteLine($"ðŸ“ Inicializando base de datos: {databasePath}");
                
                _database = new SQLiteAsyncConnection(databasePath, 
                    SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex);

                
                await Task.WhenAll(
                    _database.CreateTableAsync<Producto>(),
                    _database.CreateTableAsync<Venta>(),
                    _database.CreateTableAsync<DetalleVenta>(),
                    _database.CreateTableAsync<Promocion>(),
                    _database.CreateTableAsync<PromocionProducto>(),
                    _database.CreateTableAsync<Usuario>(),
                    _database.CreateTableAsync<AuditoriaSistema>(),
                    _database.CreateTableAsync<Caja>(),
                    _database.CreateTableAsync<DevolucionVenta>(),
                    _database.CreateTableAsync<AjusteInventario>(),
                    _database.CreateTableAsync<ConfiguracionSistema>()
                );

                
                await EnsureUsuariosSchemaAsync();
                await EnsureVentasSchemaAsync();

                
                try
                {
                    await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_ventas_fecha ON Ventas(Fecha_Venta);");
                    await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_detalleventa_ventaid ON DetalleVenta(VentaId);");
                    await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_detalleventa_producto ON DetalleVenta(Id_Producto);");
                    await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_promoprods_promo ON PromocionProductos(PromocionId);");
                    await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_promoprods_prod ON PromocionProductos(ProductoId);");
                }
                catch (Exception idxEx)
                {
                    Console.WriteLine($"âš ï¸ No se pudieron crear algunos Ã­ndices: {idxEx.Message}");
                }

                Console.WriteLine("âœ… Base de datos inicializada correctamente");
                
                
                await CrearUsuarioAdminPorDefectoAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"âŒ Error inicializando base de datos: {ex.Message}");
                throw;
            }
        }

        private async Task EnsureUsuariosSchemaAsync()
        {
            try
            {
                if (_database == null) return;
                var tableInfo = await _database.ExecuteScalarAsync<string>("PRAGMA table_info(Usuarios);");
                
                
                await _database.ExecuteAsync("ALTER TABLE Usuarios ADD COLUMN PasswordAlgorithm TEXT NOT NULL DEFAULT 'SHA256';");
            }
            catch (Exception)
            {
                
            }
            try
            {
                await _database!.ExecuteAsync("ALTER TABLE Usuarios ADD COLUMN PasswordSalt TEXT NULL;");
            }
            catch (Exception)
            {
                
            }
        }

        private async Task EnsureVentasSchemaAsync()
        {
            try
            {
                if (_database == null) return;
                await _database.ExecuteAsync("ALTER TABLE Ventas ADD COLUMN PagoRecibido REAL NOT NULL DEFAULT 0;");
            }
            catch (Exception)
            {
                
            }
            try
            {
                await _database!.ExecuteAsync("ALTER TABLE Ventas ADD COLUMN Cambio REAL NOT NULL DEFAULT 0;");
            }
            catch (Exception)
            {
                
            }
        }

        private async Task CrearUsuarioAdminPorDefectoAsync()
        {
            try
            {
                if (_database == null) return;

                var adminExiste = await _database.Table<Usuario>()
                    .Where(u => u.NombreUsuario == "admin")
                    .FirstOrDefaultAsync();

                if (adminExiste == null)
                {
                    var password = "admin123";
                    var passwordHash = SecurityHelper.HashPassword(password);

                    var admin = new Usuario
                    {
                        NombreUsuario = "admin",
                        PasswordHash = passwordHash,
                        NombreCompleto = "Administrador del Sistema",
                        Rol = "Admin",
                        Email = "admin@integratech.com",
                        Activo = true,
                        FechaCreacion = DateTime.Now
                    };

                    await _database.InsertAsync(admin);
                    
                    Console.WriteLine("========================================");
                    Console.WriteLine("âœ… Usuario administrador creado:");
                    Console.WriteLine($"   Usuario: admin");
                    Console.WriteLine($"   ContraseÃ±a: admin123");
#if DEBUG
                    Console.WriteLine($"   Hash: {passwordHash.Substring(0, Math.Min(6, passwordHash.Length))}***");
#endif
                    Console.WriteLine($"   ID: {admin.Id_Usuario}");
                    Console.WriteLine("========================================");
                }
                else
                {
                    Console.WriteLine($"â„¹ï¸ Usuario admin ya existe - ID: {adminExiste.Id_Usuario}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"âš ï¸ Error creando usuario admin: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
            }
        }

        
        
        
        public async Task ResetearPasswordAdminAsync()
        {
            try
            {
                if (_database == null) return;

                var admin = await _database.Table<Usuario>()
                    .Where(u => u.NombreUsuario == "admin")
                    .FirstOrDefaultAsync();

                if (admin != null)
                {
                    var password = "admin123";
                    admin.PasswordHash = SecurityHelper.HashPassword(password);
                    await _database.UpdateAsync(admin);
                    
                    Console.WriteLine("========================================");
                    Console.WriteLine("âœ… ContraseÃ±a del admin RESETEADA:");
                    Console.WriteLine($"   Usuario: admin");
                    Console.WriteLine($"   Nueva contraseÃ±a: {password}");
#if DEBUG
                    Console.WriteLine($"   Nuevo hash: {admin.PasswordHash.Substring(0, Math.Min(6, admin.PasswordHash.Length))}***");
#endif
                    Console.WriteLine("========================================");
                }
                else
                {
                    Console.WriteLine("âŒ Usuario admin no encontrado para resetear");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"âŒ Error al resetear contraseÃ±a: {ex.Message}");
            }
        }

        
        
        
        public async Task EliminarYRecrearBaseDeDatosAsync()
        {
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("ðŸ—‘ï¸ ELIMINANDO BASE DE DATOS COMPLETA");
                Console.WriteLine("========================================");

                
                await CloseConnectionAsync();

                var databasePath = Path.Combine(FileSystem.AppDataDirectory, "IntegraTechPOS.db");
                Console.WriteLine($"ðŸ“ Ruta: {databasePath}");

                
                if (File.Exists(databasePath))
                {
                    File.Delete(databasePath);
                    Console.WriteLine("âœ… Base de datos eliminada");
                }
                else
                {
                    Console.WriteLine("â„¹ï¸ No existe base de datos previa");
                }

                
                await Task.Delay(500);

                
                Console.WriteLine("ðŸ“¦ Creando nueva base de datos...");
                await InitializeAsync();

                Console.WriteLine("========================================");
                Console.WriteLine("âœ… BASE DE DATOS RECREADA EXITOSAMENTE");
                Console.WriteLine("   Usuario: admin");
                Console.WriteLine("   ContraseÃ±a: admin123");
                Console.WriteLine("========================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"âŒ Error eliminando BD: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                throw;
            }
        }

        
        
        
        public async Task DiagnosticarUsuarioAdminAsync()
        {
            try
            {
                if (_database == null)
                {
                    Console.WriteLine("âŒ Base de datos no inicializada");
                    return;
                }

                var admin = await _database.Table<Usuario>()
                    .Where(u => u.NombreUsuario == "admin")
                    .FirstOrDefaultAsync();

                Console.WriteLine("========================================");
                Console.WriteLine("ðŸ” DIAGNÃ“STICO USUARIO ADMIN");
                Console.WriteLine("========================================");

                if (admin == null)
                {
                    Console.WriteLine("âŒ Usuario admin NO EXISTE en la base de datos");
                    Console.WriteLine("   Ejecuta la app para crear el usuario automÃ¡ticamente");
                }
                else
                {
                    Console.WriteLine($"âœ… Usuario encontrado");
                    Console.WriteLine($"   ID: {admin.Id_Usuario}");
                    Console.WriteLine($"   Usuario: '{admin.NombreUsuario}'");
                    Console.WriteLine($"   Nombre: {admin.NombreCompleto}");
                    Console.WriteLine($"   Rol: {admin.Rol}");
                    Console.WriteLine($"   Email: {admin.Email}");
                    Console.WriteLine($"   Activo: {admin.Activo}");
                    Console.WriteLine($"   Creado: {admin.FechaCreacion}");
                    Console.WriteLine($"   Ãšltimo acceso: {admin.UltimoAcceso}");
                    
#if DEBUG
                    Console.WriteLine($"   Hash actual: {admin.PasswordHash.Substring(0, Math.Min(6, admin.PasswordHash.Length))}***");
#endif
                    Console.WriteLine();
                    
                    
                    var testPassword = "admin123";
                    var testHash = SecurityHelper.HashPassword(testPassword);
                    var coinciden = admin.PasswordHash == testHash;
                    var verificacion = SecurityHelper.VerifyPassword(testPassword, admin.PasswordHash);
                    
                    Console.WriteLine($"ðŸ§ª Prueba con contraseÃ±a por defecto");
#if DEBUG
                    Console.WriteLine($"   Hash de prueba: {testHash.Substring(0, Math.Min(6, testHash.Length))}***");
#endif
                    Console.WriteLine($"   Â¿Hashes coinciden? {(coinciden ? "âœ… SÃ" : "âŒ NO")}");
                    Console.WriteLine($"   VerifyPassword(): {(verificacion ? "âœ… SÃ" : "âŒ NO")}");
                    
                    if (!coinciden || !verificacion)
                    {
                        Console.WriteLine();
                        Console.WriteLine("âš ï¸ Â¡LOS HASHES NO COINCIDEN!");
                        Console.WriteLine("   Ejecuta: await DatabaseService.ResetearPasswordAdminAsync()");
                    }
                }
                Console.WriteLine("========================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"âŒ Error en diagnÃ³stico: {ex.Message}");
            }
        }
        #region Datos de Ejemplo
        public async Task InsertSampleDataAsync()
        {
            if (_database == null)
            {
                Console.WriteLine("âŒ Base de datos no inicializada");
                return;
            }

            var productos = new List<Producto>
            {
                new Producto
                {
                    Nombre_Producto = "Coca Cola 355ml",
                    Precio_Venta = 15.50m,
                    Precio_Compra = 12.00m,
                    Distribuidor = "Coca Cola FEMSA",
                    Categoria = "Bebidas",
                    Cantidad = 50,
                    Codigo_Barras = "7501055363827",
                    Unidad_Medida = "Pieza",
                    Stock_Minimo = 10
                },
                new Producto
                {
                    Nombre_Producto = "Pan Bimbo Blanco",
                    Precio_Venta = 32.00m,
                    Precio_Compra = 28.00m,
                    Distribuidor = "Grupo Bimbo",
                    Categoria = "PanaderÃ­a",
                    Cantidad = 25,
                    Codigo_Barras = "7501000100503",
                    Unidad_Medida = "Pieza",
                    Stock_Minimo = 5
                },
                new Producto
                {
                    Nombre_Producto = "Leche Lala 1L",
                    Precio_Venta = 24.50m,
                    Precio_Compra = 20.00m,
                    Distribuidor = "Grupo Lala",
                    Categoria = "LÃ¡cteos",
                    Cantidad = 30,
                    Codigo_Barras = "7501020200152",
                    Unidad_Medida = "Litro",
                    Stock_Minimo = 8,
                    Fecha_Vencimiento = DateTime.Now.AddDays(7)
                }
            };

            foreach (var producto in productos)
            {
                await _database.InsertAsync(producto);
            }
            
            Console.WriteLine($"âœ… {productos.Count} productos de ejemplo insertados correctamente");
        }
        #endregion

        #region MÃ©todos de Promociones
        public async Task<int> SavePromocionAsync(Promocion promo, List<int>? productoIds = null)
        {
            try
            {
                if (_database == null) return 0;

                if (promo.Id == 0)
                    await _database.InsertAsync(promo);
                else
                    await _database.UpdateAsync(promo);

                if (productoIds != null)
                {
                    
                    var existentes = await _database.Table<PromocionProducto>().Where(pp => pp.PromocionId == promo.Id).ToListAsync();
                    foreach (var e in existentes)
                        await _database.DeleteAsync(e);
                    
                    foreach (var pid in productoIds.Distinct())
                        await _database.InsertAsync(new PromocionProducto { PromocionId = promo.Id, ProductoId = pid });
                }

                return promo.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando promociÃ³n: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<Promocion>> GetPromocionesAsync(bool soloActivas = true)
        {
            if (_database == null) return new List<Promocion>();
            var q = _database.Table<Promocion>();
            if (soloActivas)
                q = q.Where(p => p.Activa);
            return await q.OrderByDescending(p => p.Id).ToListAsync();
        }

        public async Task<List<int>> GetProductoIdsDePromocionAsync(int promocionId)
        {
            if (_database == null) return new List<int>();
            var lista = await _database.Table<PromocionProducto>().Where(pp => pp.PromocionId == promocionId).ToListAsync();
            return lista.Select(l => l.ProductoId).ToList();
        }

        public async Task<int> DeletePromocionAsync(int promocionId)
        {
            if (_database == null) return 0;
            var promo = await _database.Table<Promocion>().Where(p => p.Id == promocionId).FirstOrDefaultAsync();
            if (promo != null)
            {
                var links = await _database.Table<PromocionProducto>().Where(pp => pp.PromocionId == promocionId).ToListAsync();
                foreach (var l in links) await _database.DeleteAsync(l);
                return await _database.DeleteAsync(promo);
            }
            return 0;
        }
        #endregion
        public async Task<decimal> GetGananciasDelDiaAsync()
        {
            try
            {
                if (_database == null) return 0;
                
                
                var hoy = DateTime.Today;
                var manana = hoy.AddDays(1);
                var ventas = await _database.Table<Venta>()
                    .Where(v => v.Fecha_Venta >= hoy && v.Fecha_Venta < manana)
                    .ToListAsync();
                decimal gananciatotal = 0;

                foreach (var venta in ventas)
                {
                    var detalles = await _database.Table<DetalleVenta>()
                        .Where(d => d.VentaId == venta.Id_Venta)
                        .ToListAsync();
                    
                    foreach (var detalle in detalles)
                    {
                        var producto = await _database.Table<Producto>()
                            .Where(p => p.Id_Producto == detalle.Id_Producto)
                            .FirstOrDefaultAsync();
                        if (producto != null)
                        {
                            var gananciaPorUnidad = producto.Precio_Venta - producto.Precio_Compra;
                            gananciatotal += gananciaPorUnidad * detalle.Cantidad;
                        }
                    }
                }
                return gananciatotal;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculando ganancias del dÃ­a: {ex.Message}");
                return 0;
            }
        }
        public async Task<List<GananciaProducto>> GetGananciaPorProductoAsync()
        {
            try
            {
                if (_database == null) return new List<GananciaProducto>();
                
                var productos = await _database.Table<Producto>().ToListAsync();
                var gananciasProductos = new List<GananciaProducto>();

                foreach (var producto in productos)
                {
                    var detallesVendidos = await _database.Table<DetalleVenta>()
                        .Where(d => d.Id_Producto == producto.Id_Producto)
                        .ToListAsync();
                    var cantidadTotal = detallesVendidos.Sum(d => d.Cantidad);
                    var gananciaPorUnidad = producto.Precio_Venta - producto.Precio_Compra;
                    var gananciaTotal = gananciaPorUnidad * cantidadTotal;

                    Console.WriteLine($"Debug GetGananciaPorProductoAsync: {producto.Nombre_Producto} - Detalles encontrados: {detallesVendidos.Count}, Cantidad total vendida: {cantidadTotal}, Ganancia: ${gananciaTotal}");

                    gananciasProductos.Add(new GananciaProducto
                    {
                        NombreProducto = producto.Nombre_Producto,
                        Categoria = producto.Categoria,
                        Distribuidor = producto.Distribuidor,
                        PrecioVenta = producto.Precio_Venta,
                        PrecioCompra = producto.Precio_Compra,
                        GananciaPorUnidad = gananciaPorUnidad,
                        CantidadVendida = cantidadTotal,
                        GananciaTotal = gananciaTotal
                    });
                }
                return gananciasProductos.OrderByDescending(g => g.GananciaTotal).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo ganancias por producto: {ex.Message}");
                return new List<GananciaProducto>();
            }
        }
        public async Task<List<GananciaCategoria>> GetGananciaPorCategoriaAsync()
        {
            try
            {
                var gananciasProducto = await GetGananciaPorProductoAsync();
                var gananciasCategoria = gananciasProducto
                    .GroupBy(g => g.Categoria)
                    .Select(group => new GananciaCategoria
                    {
                        Categoria = group.Key,
                        CantidadProductos = group.Count(),
                        TotalVendido = group.Sum(g => g.CantidadVendida),
                        GananciaTotal = group.Sum(g => g.GananciaTotal),
                        VentaTotal = group.Sum(g => g.PrecioVenta * g.CantidadVendida)
                    })
                    .OrderByDescending(g => g.GananciaTotal)
                    .ToList();
                return gananciasCategoria;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo ganancias por categorÃ­a: {ex.Message}");
                return new List<GananciaCategoria>();
            }

        }

        
    
        public async Task<List<Venta>> GetVentasPorFechaAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                if (_database == null) return new List<Venta>();
                
                return await _database.Table<Venta>()
                    .Where(v => v.Fecha_Venta >= fechaInicio && v.Fecha_Venta <= fechaFin)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo ventas por fecha: {ex.Message}");
                return new List<Venta>();
            }
        }

        public async Task<int> SaveVentaAsync(Venta venta)
        {
            try
            {
                if (_database == null) return 0;
                
                if (venta.Id_Venta == 0)
                {
                    await _database.InsertAsync(venta);
                    
                    return venta.Id_Venta;
                }
                else
                {
                    await _database.UpdateAsync(venta);
                    return venta.Id_Venta;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando venta: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> SaveDetalleVentaAsync(DetalleVenta detalle)
        {
            try
            {
                if (_database == null) return 0;
                
                if (detalle.Id == 0)
                {
                    return await _database.InsertAsync(detalle);
                }
                else
                {
                    return await _database.UpdateAsync(detalle);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando detalle venta: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<DetalleVenta>> GetDetalleVentaAsync(int ventaId)
        {
            try
            {
                if (_database == null) return new List<DetalleVenta>();
                
                return await _database.Table<DetalleVenta>()
                    .Where(d => d.VentaId == ventaId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo detalle venta: {ex.Message}");
                return new List<DetalleVenta>();
            }
        }

        public async Task<List<DetalleVenta>> GetDetallesVentaConProductosAsync(int ventaId)
        {
            try
            {
                if (_database == null) return new List<DetalleVenta>();
                
                
                var detalles = await _database.Table<DetalleVenta>()
                    .Where(d => d.VentaId == ventaId)
                    .ToListAsync();
                
                
                foreach (var detalle in detalles)
                {
                    if (detalle.Id_Producto > 0)
                    {
                        detalle.Producto = await _database.Table<Producto>()
                            .Where(p => p.Id_Producto == detalle.Id_Producto)
                            .FirstOrDefaultAsync();
                    }
                }
                
                return detalles;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"âŒ Error obteniendo detalles de venta con productos #{ventaId}: {ex.Message}");
                return new List<DetalleVenta>();
            }
        }

        public async Task<decimal> GetVentasTotalMesAsync(DateTime fecha)
        {
            try
            {
                if (_database == null) return 0;
                
                var inicioMes = new DateTime(fecha.Year, fecha.Month, 1);
                var finMes = inicioMes.AddMonths(1).AddSeconds(-1);
                
                var ventas = await _database.Table<Venta>()
                    .Where(v => v.Fecha_Venta >= inicioMes && v.Fecha_Venta <= finMes)
                    .ToListAsync();
                    
                return ventas.Sum(v => v.Total);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo total ventas mes: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> SaveProductoAsync(Producto producto)
        {
            try
            {
                if (_database == null) return 0;
                
                if (producto.Id_Producto == 0)
                {
                    return await _database.InsertAsync(producto);
                }
                else
                {
                    return await _database.UpdateAsync(producto);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando producto: {ex.Message}");
                return 0;
            }
        }

        
        public async Task<List<Venta>> GetVentasAsync()
        {
            try
            {
                if (_database == null) return new List<Venta>();
                return await _database.Table<Venta>().ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo ventas: {ex.Message}");
                return new List<Venta>();
            }
        }

        public async Task<Producto> GetProductoByIdAsync(int productoId)
        {
            try
            {
                if (_database == null) return null!;
                return await _database.Table<Producto>()
                    .Where(p => p.Id_Producto == productoId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo producto por ID: {ex.Message}");
                return null!;
            }
        }

        public async Task<decimal> GetVentasDelDiaAsync(DateTime fecha)
        {
            try
            {
                if (_database == null) return 0;
                
                var inicioDelDia = fecha.Date;
                var finDelDia = fecha.Date.AddDays(1).AddSeconds(-1);
                
                var ventas = await _database.Table<Venta>()
                    .Where(v => v.Fecha_Venta >= inicioDelDia && v.Fecha_Venta <= finDelDia)
                    .ToListAsync();
                    
                return ventas.Sum(v => v.Total);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo ventas del dÃ­a: {ex.Message}");
                return 0;
            }
        }

        public async Task<decimal> GetGananciaTotalAsync()
        {
            try
            {
                if (_database == null) return 0;
                
                
                var todasLasVentas = await _database.Table<Venta>().ToListAsync();
                decimal gananciaTotal = 0;

                Console.WriteLine($"Debug GetGananciaTotalAsync: Encontradas {todasLasVentas.Count} ventas");

                foreach (var venta in todasLasVentas)
                {
                    var detalles = await _database.Table<DetalleVenta>()
                        .Where(d => d.VentaId == venta.Id_Venta)
                        .ToListAsync();
                    
                    Console.WriteLine($"Debug: Venta {venta.Id_Venta} tiene {detalles.Count} detalles");
                    
                    foreach (var detalle in detalles)
                    {
                        var producto = await _database.Table<Producto>()
                            .Where(p => p.Id_Producto == detalle.Id_Producto)
                            .FirstOrDefaultAsync();
                        if (producto != null)
                        {
                            var gananciaPorUnidad = producto.Precio_Venta - producto.Precio_Compra;
                            var gananciaDetalle = gananciaPorUnidad * detalle.Cantidad;
                            gananciaTotal += gananciaDetalle;
                            
                            Console.WriteLine($"Debug: Producto {producto.Nombre_Producto} - Precio Venta: {producto.Precio_Venta}, Precio Compra: {producto.Precio_Compra}, Cantidad: {detalle.Cantidad}, Ganancia: ${gananciaDetalle}");
                        }
                        else
                        {
                            Console.WriteLine($"Debug: No se encontrÃ³ producto con ID {detalle.Id_Producto}");
                        }
                    }
                }
                
                Console.WriteLine($"Debug: Ganancia total calculada: ${gananciaTotal}");
                return gananciaTotal;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo ganancia total: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<Producto>> GetProductosAsync()
        {
            try
            {
                if (_database == null) return new List<Producto>();
                return await _database.Table<Producto>().ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo productos: {ex.Message}");
                return new List<Producto>();
            }
        }

        public async Task<int> DeleteProductoAsync(Producto producto)
        {
            try
            {
                if (_database == null) return 0;
                return await _database.DeleteAsync(producto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error eliminando producto: {ex.Message}");
                return 0;
            }
        }

        
        public async Task<int> GetTotalVentasCountAsync()
        {
            try
            {
                if (_database == null) return 0;
                var ventas = await _database.Table<Venta>().ToListAsync();
                return ventas.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error contando ventas: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> GetTotalDetallesVentaCountAsync()
        {
            try
            {
                if (_database == null) return 0;
                var detalles = await _database.Table<DetalleVenta>().ToListAsync();
                return detalles.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error contando detalles venta: {ex.Message}");
                return 0;
            }
        }

        #region MÃ©todos de Usuario
        public async Task<int> SaveUsuarioAsync(Usuario usuario)
        {
            try
            {
                if (_database == null) return 0;
                
                if (usuario.Id_Usuario == 0)
                {
                    return await _database.InsertAsync(usuario);
                }
                else
                {
                    return await _database.UpdateAsync(usuario);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando usuario: {ex.Message}");
                return 0;
            }
        }

        public async Task<Usuario?> GetUsuarioByIdAsync(int id)
        {
            try
            {
                if (_database == null) return null;
                return await _database.Table<Usuario>()
                    .Where(u => u.Id_Usuario == id)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo usuario por ID: {ex.Message}");
                return null;
            }
        }

        public async Task<Usuario?> GetUsuarioByNombreAsync(string nombreUsuario)
        {
            try
            {
                if (_database == null) return null;
                return await _database.Table<Usuario>()
                    .Where(u => u.NombreUsuario == nombreUsuario)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo usuario por nombre: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            try
            {
                if (_database == null) return new List<Usuario>();
                return await _database.Table<Usuario>().ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo usuarios: {ex.Message}");
                return new List<Usuario>();
            }
        }
        #endregion

        #region MÃ©todos de AuditorÃ­a
        public async Task<int> RegistrarAuditoriaAsync(int usuarioId, string nombreUsuario, string accion, string detalles, string? tablaAfectada = null, int? idRegistroAfectado = null)
        {
            try
            {
                if (_database == null) return 0;

                var auditoria = new AuditoriaSistema
                {
                    Id_Usuario = usuarioId,
                    NombreUsuario = nombreUsuario,
                    Accion = accion,
                    Detalles = detalles,
                    TablaAfectada = tablaAfectada ?? string.Empty,
                    IdRegistroAfectado = idRegistroAfectado,
                    Fecha = DateTime.Now
                };

                return await _database.InsertAsync(auditoria);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registrando auditorÃ­a: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<AuditoriaSistema>> GetAuditoriasAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                if (_database == null) return new List<AuditoriaSistema>();

                var query = _database.Table<AuditoriaSistema>();

                if (fechaInicio.HasValue)
                {
                    query = query.Where(a => a.Fecha >= fechaInicio.Value);
                }

                if (fechaFin.HasValue)
                {
                    query = query.Where(a => a.Fecha <= fechaFin.Value);
                }

                return await query.OrderByDescending(a => a.Fecha).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo auditorÃ­as: {ex.Message}");
                return new List<AuditoriaSistema>();
            }
        }
        #endregion

        #region MÃ©todos de Caja
        public async Task<int> AbrirCajaAsync(Caja caja)
        {
            try
            {
                if (_database == null) return 0;
                return await _database.InsertAsync(caja);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error abriendo caja: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> CerrarCajaAsync(int cajaId, decimal montoFinal, string? notas = null)
        {
            try
            {
                if (_database == null) return 0;

                var caja = await _database.Table<Caja>()
                    .Where(c => c.Id_Caja == cajaId)
                    .FirstOrDefaultAsync();

                if (caja != null)
                {
                    caja.MontoFinal = montoFinal;
                    caja.FechaCierre = DateTime.Now;
                    caja.Abierta = false;
                    if (!string.IsNullOrEmpty(notas))
                        caja.Notas = notas;

                    return await _database.UpdateAsync(caja);
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cerrando caja: {ex.Message}");
                return 0;
            }
        }

        public async Task<Caja?> GetCajaAbiertaAsync(int usuarioId)
        {
            try
            {
                if (_database == null) return null;
                return await _database.Table<Caja>()
                    .Where(c => c.Id_Usuario == usuarioId && c.Abierta)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo caja abierta: {ex.Message}");
                return null;
            }
        }
        #endregion

        #region MÃ©todos de Devoluciones
        public async Task<int> RegistrarDevolucionAsync(DevolucionVenta devolucion)
        {
            try
            {
                if (_database == null) return 0;

                
                var result = await _database.InsertAsync(devolucion);

                if (result > 0)
                {
                    
                    var producto = await GetProductoByIdAsync(devolucion.Id_Producto);
                    if (producto != null)
                    {
                        producto.Cantidad += devolucion.Cantidad;
                        await SaveProductoAsync(producto);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registrando devoluciÃ³n: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<DevolucionVenta>> GetDevolucionesPorVentaAsync(int ventaId)
        {
            try
            {
                if (_database == null) return new List<DevolucionVenta>();
                return await _database.Table<DevolucionVenta>()
                    .Where(d => d.Id_Venta == ventaId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo devoluciones: {ex.Message}");
                return new List<DevolucionVenta>();
            }
        }
        #endregion

        #region MÃ©todos de Ajustes de Inventario
        public async Task<int> RegistrarAjusteInventarioAsync(AjusteInventario ajuste)
        {
            try
            {
                if (_database == null) return 0;

                var result = await _database.InsertAsync(ajuste);

                if (result > 0)
                {
                    
                    var producto = await GetProductoByIdAsync(ajuste.Id_Producto);
                    if (producto != null)
                    {
                        producto.Cantidad = ajuste.CantidadNueva;
                        await SaveProductoAsync(producto);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registrando ajuste de inventario: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<AjusteInventario>> GetAjustesInventarioAsync(int? productoId = null)
        {
            try
            {
                if (_database == null) return new List<AjusteInventario>();

                var query = _database.Table<AjusteInventario>();

                if (productoId.HasValue)
                {
                    query = query.Where(a => a.Id_Producto == productoId.Value);
                }

                return await query.OrderByDescending(a => a.Fecha).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo ajustes de inventario: {ex.Message}");
                return new List<AjusteInventario>();
            }
        }
        #endregion

        #region MÃ©todos con Transacciones
        
        
        
        public Task<bool> GuardarVentaConTransaccionAsync(Venta venta, List<DetalleVenta> detalles, int usuarioId)
        {
            if (_database == null) return Task.FromResult(false);

            try
            {
                
                var connection = _database.GetConnection();
                
                connection.BeginTransaction();
                
                try
                {
                    
                    connection.Insert(venta);
                    var ventaId = venta.Id_Venta;

                    
                    foreach (var detalle in detalles)
                    {
                        detalle.VentaId = ventaId;
                        connection.Insert(detalle);

                        
                        var producto = connection.Table<Producto>()
                            .Where(p => p.Id_Producto == detalle.Id_Producto)
                            .FirstOrDefault();

                        if (producto != null)
                        {
                            if (producto.Cantidad < detalle.Cantidad)
                            {
                                throw new InvalidOperationException($"Stock insuficiente para {producto.Nombre_Producto}");
                            }

                            producto.Cantidad -= detalle.Cantidad;
                            connection.Update(producto);
                        }
                    }

                    
                    var usuario = connection.Table<Usuario>()
                        .Where(u => u.Id_Usuario == usuarioId)
                        .FirstOrDefault();

                    if (usuario != null)
                    {
                        var auditoria = new AuditoriaSistema
                        {
                            Id_Usuario = usuarioId,
                            NombreUsuario = usuario.NombreUsuario,
                            Accion = "Venta",
                            Detalles = $"Venta #{ventaId} - Total: ${venta.Total:N2}",
                            TablaAfectada = "Ventas",
                            IdRegistroAfectado = ventaId,
                            Fecha = DateTime.Now
                        };
                        connection.Insert(auditoria);
                    }

                    connection.Commit();
                    Console.WriteLine($"âœ… Venta #{ventaId} guardada exitosamente con transacciÃ³n");
                    return Task.FromResult(true);
                }
                catch (Exception)
                {
                    connection.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"âŒ Error en transacciÃ³n de venta: {ex.Message}");
                return Task.FromResult(false);
            }
        }
        #endregion

        #region ConfiguraciÃ³n del Sistema
        public async Task<string?> ObtenerConfiguracionAsync(string clave)
        {
            await InitializeAsync();
            var config = await _database!.Table<ConfiguracionSistema>()
                .Where(c => c.Clave == clave)
                .FirstOrDefaultAsync();
            return config?.Valor;
        }

        public async Task<bool> GuardarConfiguracionAsync(string clave, string valor, string descripcion = "")
        {
            try
            {
                await InitializeAsync();
                
                var config = await _database!.Table<ConfiguracionSistema>()
                    .Where(c => c.Clave == clave)
                    .FirstOrDefaultAsync();

                if (config != null)
                {
                    config.Valor = valor;
                    config.FechaModificacion = DateTime.Now;
                    if (!string.IsNullOrEmpty(descripcion))
                        config.Descripcion = descripcion;
                    await _database.UpdateAsync(config);
                }
                else
                {
                    config = new ConfiguracionSistema
                    {
                        Clave = clave,
                        Valor = valor,
                        Descripcion = descripcion,
                        FechaModificacion = DateTime.Now
                    };
                    await _database.InsertAsync(config);
                }

                Console.WriteLine($"âœ… ConfiguraciÃ³n guardada: {clave} = {valor}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"âŒ Error guardando configuraciÃ³n: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ObtenerConfiguracionBoolAsync(string clave, bool valorPorDefecto = false)
        {
            var valor = await ObtenerConfiguracionAsync(clave);
            if (bool.TryParse(valor, out bool resultado))
                return resultado;
            return valorPorDefecto;
        }
        #endregion
    }

}
