namespace app.Models;

public class Factura
{
    public int FacturaId { get; set; }
    public string NumeroFactura { get; set; }
    public string PeriodoFacturacion { get; set; }
    public decimal MontoFacturado { get; set; }
    public decimal MontoPagado { get; set; }

    // Laves foraneas y relaciones
    public int ClienteId { get; set; } // FK con cliente
    public Cliente Cliente { get; set; } 

    public ICollection<Transaccion> Transacciones { get; set; }
}