using System.ComponentModel.DataAnnotations;

namespace Tasker.Entities
{
    public class Numerador
    {
        public int Id { get; set; }
        public long UltimoNumero { get; set; }

        [StringLength(50)]
        public string Tabla { get; set; } = default!;
    }
}
