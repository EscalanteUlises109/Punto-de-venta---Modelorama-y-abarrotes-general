using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using IntegraTech_POS.Models;
using QuestPdfContainer = QuestPDF.Infrastructure.IContainer;
using QuestPdfColors = QuestPDF.Helpers.Colors;

namespace IntegraTech_POS.Services
{
    public class ReportePDFService
    {
        public async Task<string> GenerarReporteVentasCSVAsync(List<Venta> ventas, DateTime fecha, DatabaseService databaseService)
        {
            try
            {
                var fileName = $"Reporte_Ventas_{fecha:yyyy-MM-dd}.csv";
                var directorio = Path.Combine(FileSystem.AppDataDirectory, "Reportes");
                if (!Directory.Exists(directorio)) Directory.CreateDirectory(directorio);
                var filePath = Path.Combine(directorio, fileName);

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("# Venta,Fecha,Hora,Cliente,MÃ©todo Pago,Producto,CategorÃ­a,Proveedor,Cantidad,Precio Unit.,Subtotal,Total Venta,Descuento,Notas");

                foreach (var venta in ventas.OrderBy(v => v.Fecha_Venta))
                {
                    var detalles = await databaseService.GetDetallesVentaConProductosAsync(venta.Id_Venta);
                    if (detalles.Any())
                    {
                        foreach (var d in detalles)
                        {
                            sb.AppendLine($"{venta.Id_Venta}," +
                                           $"{venta.Fecha_Venta:dd/MM/yyyy}," +
                                           $"{venta.Fecha_Venta:HH:mm:ss}," +
                                           $"\"{(string.IsNullOrEmpty(venta.Cliente) ? "Cliente general" : venta.Cliente)}\"," +
                                           $"\"{(string.IsNullOrEmpty(venta.Metodo_Pago) ? "Efectivo" : venta.Metodo_Pago)}\"," +
                                           $"\"{d.Producto?.Nombre_Producto ?? "N/A"}\"," +
                                           $"\"{(string.IsNullOrEmpty(d.Producto?.Categoria) ? "Sin categorÃ­a" : d.Producto.Categoria)}\"," +
                                           $"\"{(string.IsNullOrEmpty(d.Producto?.Distribuidor) ? "Sin proveedor" : d.Producto.Distribuidor)}\"," +
                                           $"{d.Cantidad}," +
                                           $"{d.Precio_Unitario:N2}," +
                                           $"{d.Subtotal:N2}," +
                                           $"{venta.Total:N2}," +
                                           $"{venta.Descuento:N2}," +
                                           $"\"{(string.IsNullOrEmpty(venta.Notas) ? "" : venta.Notas)}\"");
                        }
                    }
                }

                await File.WriteAllTextAsync(filePath, sb.ToString());
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"âŒ Error generando CSV: {ex.Message}");
                return string.Empty;
            }
        }
        public async Task<string> GenerarReporteVentasPDFAsync(List<Venta> ventas, DateTime fecha, DatabaseService databaseService)
        {
            try
            {
                
                QuestPDF.Settings.License = LicenseType.Community;

                var fileName = $"Reporte_Ventas_{fecha:yyyy-MM-dd}.pdf";
                var directorio = Path.Combine(FileSystem.AppDataDirectory, "Reportes");
                
                if (!Directory.Exists(directorio))
                {
                    Directory.CreateDirectory(directorio);
                }
                
                var filePath = Path.Combine(directorio, fileName);

                
                var totalVentas = ventas.Count;
                var totalRecaudado = ventas.Sum(v => v.Total);
                var totalDescuentos = ventas.Sum(v => v.Descuento);
                var ticketPromedio = totalVentas > 0 ? totalRecaudado / totalVentas : 0;

                
                await Task.Run(() =>
                {
                    Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.Letter);
                            page.Margin(40);
                            page.PageColor(QuestPdfColors.White);
                            page.DefaultTextStyle(x => x.FontSize(10));

                            
                            page.Header().Element(HeaderContainer);

                            
                            page.Content().Column(column =>
                            {
                                column.Spacing(15);

                                
                                column.Item().Text($"Reporte de Ventas - {fecha:dd/MM/yyyy}")
                                    .FontSize(18)
                                    .Bold()
                                    .FontColor(QuestPdfColors.Blue.Darken2);

                                
                                column.Item().Element(c => ResumenEstadisticas(c, totalVentas, totalRecaudado, totalDescuentos, ticketPromedio));

                                column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(QuestPdfColors.Grey.Lighten2);

                                
                                column.Item().Text("Detalle de Ventas")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(QuestPdfColors.Blue.Darken1);

                                column.Item().Element(c => TablaVentas(c, ventas, databaseService));
                            });

                            
                            page.Footer().Element(FooterContainer);
                        });
                    }).GeneratePdf(filePath);
                });

                Console.WriteLine($"âœ… PDF generado exitosamente: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"âŒ Error generando PDF: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                return string.Empty;
            }
        }

        private void HeaderContainer(QuestPdfContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("IntegraTech POS")
                        .FontSize(20)
                        .Bold()
                        .FontColor(QuestPdfColors.Blue.Darken3);
                    
                    column.Item().Text("Sistema de Punto de Venta")
                        .FontSize(10)
                        .FontColor(QuestPdfColors.Grey.Darken1);
                });

                row.ConstantItem(100).Column(column =>
                {
                    column.Item().AlignRight().Text($"Fecha de GeneraciÃ³n:")
                        .FontSize(8)
                        .FontColor(QuestPdfColors.Grey.Darken1);
                    
                    column.Item().AlignRight().Text($"{DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(10)
                        .Bold();
                });
            });
        }

        private void ResumenEstadisticas(QuestPdfContainer container, int totalVentas, decimal totalRecaudado, decimal totalDescuentos, decimal ticketPromedio)
        {
            container.Background(QuestPdfColors.Blue.Lighten4).Padding(15).Column(column =>
            {
                column.Spacing(8);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Total de Ventas").FontSize(9).FontColor(QuestPdfColors.Grey.Darken1);
                        col.Item().Text(totalVentas.ToString()).FontSize(16).Bold().FontColor(QuestPdfColors.Blue.Darken2);
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Total Recaudado").FontSize(9).FontColor(QuestPdfColors.Grey.Darken1);
                        col.Item().Text($"${totalRecaudado:N2}").FontSize(16).Bold().FontColor(QuestPdfColors.Green.Darken2);
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Descuentos Aplicados").FontSize(9).FontColor(QuestPdfColors.Grey.Darken1);
                        col.Item().Text($"${totalDescuentos:N2}").FontSize(16).Bold().FontColor(QuestPdfColors.Red.Darken1);
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Ticket Promedio").FontSize(9).FontColor(QuestPdfColors.Grey.Darken1);
                        col.Item().Text($"${ticketPromedio:N2}").FontSize(16).Bold().FontColor(QuestPdfColors.Blue.Darken2);
                    });
                });
            });
        }

        private void TablaVentas(QuestPdfContainer container, List<Venta> ventas, DatabaseService databaseService)
        {
            container.Column(column =>
            {
                foreach (var venta in ventas.OrderByDescending(v => v.Fecha_Venta))
                {
                    
                    column.Item().PaddingBottom(5).Background(QuestPdfColors.Blue.Darken3).Padding(8).Row(row =>
                    {
                        row.RelativeItem().Text($"Venta #{venta.Id_Venta}").FontSize(11).Bold().FontColor(QuestPdfColors.White);
                        row.RelativeItem().Text($"{venta.Fecha_Venta:dd/MM/yyyy} - {venta.Fecha_Venta:HH:mm:ss}").FontSize(9).FontColor(QuestPdfColors.White);
                        row.RelativeItem().AlignRight().Text($"Cliente: {(string.IsNullOrEmpty(venta.Cliente) ? "Cliente general" : venta.Cliente)}").FontSize(9).FontColor(QuestPdfColors.White);
                        row.ConstantItem(80).AlignRight().Text($"{(string.IsNullOrEmpty(venta.Metodo_Pago) ? "Efectivo" : venta.Metodo_Pago)}").FontSize(9).FontColor(QuestPdfColors.White);
                    });

                    
                    var detalles = databaseService.GetDetallesVentaConProductosAsync(venta.Id_Venta).GetAwaiter().GetResult();

                    if (detalles.Any())
                    {
                        
                        column.Item().Border(1).BorderColor(QuestPdfColors.Grey.Lighten2).Table(table =>
                        {
                            
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);    
                                columns.RelativeColumn(1.5f); 
                                columns.RelativeColumn(1.5f); 
                                columns.RelativeColumn(1);    
                                columns.RelativeColumn(1.2f); 
                                columns.RelativeColumn(1.2f); 
                            });

                            
                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("Producto").Bold();
                                header.Cell().Element(HeaderStyle).Text("CategorÃ­a").Bold();
                                header.Cell().Element(HeaderStyle).Text("Proveedor").Bold();
                                header.Cell().Element(HeaderStyle).AlignCenter().Text("Cant.").Bold();
                                header.Cell().Element(HeaderStyle).AlignRight().Text("P. Unit.").Bold();
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Subtotal").Bold();

                                static QuestPdfContainer HeaderStyle(QuestPdfContainer c)
                                {
                                    return c.Background(QuestPdfColors.Blue.Lighten3).Padding(4).DefaultTextStyle(x => x.FontSize(8).FontColor(QuestPdfColors.Blue.Darken3));
                                }
                            });

                            
                            foreach (var detalle in detalles)
                            {
                                table.Cell().Element(CellStyle).Column(col =>
                                {
                                    col.Item().Text(detalle.Producto?.Nombre_Producto ?? "N/A").FontSize(8).Bold();
                                    if (!string.IsNullOrEmpty(detalle.Producto?.Codigo_Barras))
                                    {
                                        col.Item().Text($"CÃ³digo: {detalle.Producto.Codigo_Barras}").FontSize(7).FontColor(QuestPdfColors.Grey.Darken1);
                                    }
                                });
                                
                                table.Cell().Element(CellStyle).Text(string.IsNullOrEmpty(detalle.Producto?.Categoria) ? "Sin categorÃ­a" : detalle.Producto.Categoria).FontSize(8);
                                table.Cell().Element(CellStyle).Text(string.IsNullOrEmpty(detalle.Producto?.Distribuidor) ? "Sin proveedor" : detalle.Producto.Distribuidor).FontSize(8);
                                table.Cell().Element(CellStyle).AlignCenter().Text(detalle.Cantidad.ToString()).FontSize(8).Bold();
                                table.Cell().Element(CellStyle).AlignRight().Text($"${detalle.Precio_Unitario:N2}").FontSize(8);
                                table.Cell().Element(CellStyle).AlignRight().Text($"${detalle.Subtotal:N2}").FontSize(8).Bold();
                            }

                            
                            table.Cell().ColumnSpan(5).Element(TotalCellStyle).AlignRight().Text("Subtotal:").Bold();
                            table.Cell().Element(TotalCellStyle).AlignRight().Text($"${detalles.Sum(d => d.Subtotal):N2}").Bold();

                            if (venta.Descuento > 0)
                            {
                                table.Cell().ColumnSpan(5).Element(TotalCellStyle).AlignRight().Text("Descuento:").Bold();
                                table.Cell().Element(TotalCellStyle).AlignRight().Text($"-${venta.Descuento:N2}").FontColor(QuestPdfColors.Red.Darken1).Bold();
                            }

                            table.Cell().ColumnSpan(5).Element(FinalTotalStyle).AlignRight().Text("TOTAL:").Bold();
                            table.Cell().Element(FinalTotalStyle).AlignRight().Text($"${venta.Total:N2}").FontSize(10).Bold();

                            static QuestPdfContainer CellStyle(QuestPdfContainer c)
                            {
                                return c.BorderBottom(1).BorderColor(QuestPdfColors.Grey.Lighten3).Padding(4).DefaultTextStyle(x => x.FontSize(8));
                            }

                            static QuestPdfContainer TotalCellStyle(QuestPdfContainer c)
                            {
                                return c.Background(QuestPdfColors.Grey.Lighten4).Padding(4).DefaultTextStyle(x => x.FontSize(9));
                            }

                            static QuestPdfContainer FinalTotalStyle(QuestPdfContainer c)
                            {
                                return c.Background(QuestPdfColors.Green.Lighten4).Padding(4).DefaultTextStyle(x => x.FontSize(9).FontColor(QuestPdfColors.Green.Darken3));
                            }
                        });
                    }
                    else
                    {
                        column.Item().Border(1).BorderColor(QuestPdfColors.Grey.Lighten2).Padding(10).AlignCenter().Text("Sin productos registrados").FontSize(8).Italic().FontColor(QuestPdfColors.Grey.Darken1);
                    }

                    
                    if (!string.IsNullOrEmpty(venta.Notas))
                    {
                        column.Item().PaddingTop(2).PaddingBottom(2).Background(QuestPdfColors.Yellow.Lighten4).Padding(5).Row(row =>
                        {
                            row.ConstantItem(50).Text("Notas:").FontSize(8).Bold().FontColor(QuestPdfColors.Orange.Darken2);
                            row.RelativeItem().Text(venta.Notas).FontSize(8).FontColor(QuestPdfColors.Grey.Darken2);
                        });
                    }

                    
                    column.Item().PaddingBottom(15);
                }
            });
        }

        private void FooterContainer(QuestPdfContainer container)
        {
            container.AlignCenter().Column(column =>
            {
                column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(QuestPdfColors.Grey.Lighten2);
                column.Item().Text(text =>
                {
                    text.Span("Generado por ").FontSize(8).FontColor(QuestPdfColors.Grey.Darken1);
                    text.Span("IntegraTech POS").FontSize(8).Bold().FontColor(QuestPdfColors.Blue.Darken2);
                    text.Span($" â€¢ PÃ¡gina ").FontSize(8).FontColor(QuestPdfColors.Grey.Darken1);
                    text.CurrentPageNumber().FontSize(8).Bold();
                    text.Span(" de ").FontSize(8).FontColor(QuestPdfColors.Grey.Darken1);
                    text.TotalPages().FontSize(8).Bold();
                });
            });
        }
    }
}

