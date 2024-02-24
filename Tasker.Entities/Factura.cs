using System.ComponentModel.DataAnnotations;

namespace Tasker.Entities
{
    public class Factura
    {
        public int Id { get; set; }
        [StringLength(20)]
        public string NumeroFactura { get; set; } = default!;

        [StringLength(100)]
        public string NombreCliente { get; set; } = default!;
        public DateTime Fecha { get; set; } = DateTime.Now;

        public float TotalVenta { get; set; }
    }
}
