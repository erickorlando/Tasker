using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasker.Entities
{
    [Table(nameof(Pastel))]
    public class Pastel
    {
        public int Id { get; set; }
        [StringLength(100)]
        public string Nombre { get; set; } = default!;

        [StringLength(500)]
        public string? UrlImagen { get; set; }
    }
}
