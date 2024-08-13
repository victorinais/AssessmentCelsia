namespace app.Models;

public class Transaccion
{
    public int TransaccionId { get; set; }
    public DateTime FechaHora { get; set; }
    public decimal Monto { get; set; }
    public string Estado { get; set; }
    public string Tipo { get; set; }
    public string PlataformaUtilizada { get; set; }

    // Laves foraneas y relaciones
    public int ClienteId { get; set; } // FK con cliente
    public Cliente Cliente { get; set; }

    public int FacturaId { get; set; }
    public Factura Factura { get; set; }
}