using OfficeOpenXml;
using Microsoft.AspNetCore.Mvc;
using Assessment.ViewModels;
using app.Data;
using app.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

public class ExcelController : Controller
{
    private readonly BaseDbContext _context;
    private const int BatchSize = 100; // Tamaño del lote para procesar los datos

    public ExcelController(BaseDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Upload(ExcelUploadViewModel model)
    {
        if (model.ExcelFile == null || model.ExcelFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Por favor, seleccione un archivo Excel.";
                return RedirectToAction("Index"); // Redirige a la vista "Index" con el mensaje de error
            }

        if (model.ExcelFile != null && model.ExcelFile.Length > 0)
        {
            try
            {
                using (var package = new ExcelPackage(model.ExcelFile.OpenReadStream()))
                {
                    var worksheet = package.Workbook.Worksheets.First();
                    int rowCount = worksheet.Dimension.Rows;

                    // Cargar clientes y facturas existentes en memoria para evitar consultas repetidas
                    var existingClientes = await _context.Clientes.AsNoTracking().ToDictionaryAsync(c => c.NumeroIdentificacion);
                    var existingFacturas = await _context.Facturas.AsNoTracking().ToDictionaryAsync(f => f.NumeroFactura);

                    var clientes = new List<Cliente>();
                    var facturas = new List<Factura>();
                    var transacciones = new List<Transaccion>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        // Clientes
                        string numeroIdentificacion = worksheet.Cells[row, 7].Text.Trim();
                        if (!existingClientes.TryGetValue(numeroIdentificacion, out var cliente))
                        {
                            cliente = new Cliente
                            {
                                Nombre = worksheet.Cells[row, 6].Text.Trim(),
                                NumeroIdentificacion = numeroIdentificacion,
                                Direccion = worksheet.Cells[row, 8].Text.Trim(),
                                Telefono = worksheet.Cells[row, 9].Text.Trim().Substring(0, Math.Min(20, worksheet.Cells[row, 9].Text.Trim().Length)),
                                CorreoElectronico = worksheet.Cells[row, 10].Text.Trim()
                            };
                            clientes.Add(cliente);
                            existingClientes[numeroIdentificacion] = cliente;
                        }

                        // Facturas
                        string numeroFactura = worksheet.Cells[row, 12].Text.Trim();
                        if (!existingFacturas.TryGetValue(numeroFactura, out var factura))
                        {
                            factura = new Factura
                            {
                                NumeroFactura = numeroFactura,
                                PeriodoFacturacion = worksheet.Cells[row, 13].Text.Trim(),
                                MontoFacturado = Convert.ToDecimal(worksheet.Cells[row, 14].Text.Trim()),
                                MontoPagado = Convert.ToDecimal(worksheet.Cells[row, 15].Text.Trim()),
                                Cliente = cliente
                            };
                            facturas.Add(factura);
                            existingFacturas[numeroFactura] = factura;
                        }

                        // Transacciones
                        var transaccion = new Transaccion
                        {
                            FechaHora = Convert.ToDateTime(worksheet.Cells[row, 2].Text.Trim()),
                            Monto = Convert.ToDecimal(worksheet.Cells[row, 3].Text.Trim()),
                            Estado = worksheet.Cells[row, 4].Text.Trim(),
                            Tipo = worksheet.Cells[row, 5].Text.Trim(),
                            PlataformaUtilizada = worksheet.Cells[row, 11].Text.Trim(),
                            Cliente = cliente,
                            Factura = factura
                        };
                        transacciones.Add(transaccion);

                        // Guardar en lotes
                        if (row % BatchSize == 0 || row == rowCount)
                        {
                            await using var transaction = await _context.Database.BeginTransactionAsync();

                            if (clientes.Count > 0)
                            {
                                _context.Clientes.AddRange(clientes);
                                await _context.SaveChangesAsync();
                                clientes.Clear();
                            }

                            if (facturas.Count > 0)
                            {
                                _context.Facturas.AddRange(facturas);
                                await _context.SaveChangesAsync();
                                facturas.Clear();
                            }

                            if (transacciones.Count > 0)
                            {
                                _context.Transacciones.AddRange(transacciones);
                                await _context.SaveChangesAsync();
                                transacciones.Clear();
                            }

                            await transaction.CommitAsync();
                        }
                    }
                }

                TempData["SuccessMessage"] = "El archivo Excel se ha procesado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ocurrió un error al procesar el archivo: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        TempData["ErrorMessage"] = "Por favor, seleccione un archivo Excel válido.";
        return View(model);
    }

}


