namespace app.Models;

public class Cliente
{
    public int ClienteId { get; set; }
    public string Nombre { get; set; }
    public string NumeroIdentificacion { get; set; }
    public string Direccion { get; set; }
    public string Telefono { get; set; }
    public string CorreoElectronico { get; set; }

    // Relaciones
    public ICollection<Transaccion> Transacciones { get; set; } // Relacion 1:N con Transacciones
    public ICollection<Factura> Facturas { get; set; } // Relacion 1:N Facturas

}