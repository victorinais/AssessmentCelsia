using app.Data;
using app.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace app.Controllers
{
    public class FacturaController : Controller
    {
        private readonly BaseDbContext _context;
        public FacturaController(BaseDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            List<Factura> lista = await _context.Facturas.Include(f => f.Cliente).ToListAsync(); // Incluir Cliente para mostrar detalles en la lista
            return View(lista);
        }

        [HttpGet]
        public async Task<IActionResult> Nuevo()
        {
            ViewBag.Clientes = await _context.Clientes.ToListAsync(); // Cargar lista de clientes
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Nuevo(Factura factura)
        {
            // Validar que el ClienteId exista en la tabla Clientes
            var clienteExiste = await _context.Clientes.AnyAsync(c => c.ClienteId == factura.ClienteId);
            if (!clienteExiste)
            {
                // Añadir un mensaje de error si el ClienteId no es válido
                ModelState.AddModelError("ClienteId", "Cliente inválido. Por favor, selecciona un cliente válido.");
                ViewBag.Clientes = await _context.Clientes.ToListAsync(); // Volver a cargar la lista de clientes para la vista
                return View(factura); // Retornar a la misma vista con el modelo para mostrar los errores de validación
            }

            // Si todas las validaciones son exitosas, agregar la factura
            await _context.Facturas.AddAsync(factura);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Lista));
        }


        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            Factura factura = await _context.Facturas.FirstOrDefaultAsync(e => e.FacturaId == id);
            if (factura == null)
            {
                return NotFound();
            }

            ViewBag.Clientes = await _context.Clientes.ToListAsync(); // Cargar lista de clientes para editar
            return View(factura);
        }


        [HttpPost]
        public async Task<IActionResult> Editar(Factura factura)
        {
            // Validar que el ClienteId exista en la tabla Clientes
            var clienteExiste = await _context.Clientes.AnyAsync(c => c.ClienteId == factura.ClienteId);
            if (!clienteExiste)
            {
                // Añadir un mensaje de error si el ClienteId no es válido
                ModelState.AddModelError("ClienteId", "Cliente inválido. Por favor, selecciona un cliente válido.");
                ViewBag.Clientes = await _context.Clientes.ToListAsync(); // Volver a cargar la lista de clientes para la vista
                return View(factura); // Retornar a la misma vista con el modelo para mostrar los errores de validación
            }

            _context.Facturas.Update(factura);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        [HttpGet]
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Factura factura = await _context.Facturas.FirstOrDefaultAsync(e => e.FacturaId == id);
            if (factura == null)
            {
                return NotFound();
            }

            _context.Facturas.Remove(factura);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }
    }
}