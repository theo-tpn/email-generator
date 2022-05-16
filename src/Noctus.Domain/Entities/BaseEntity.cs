using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Noctus.Domain.Entities
{
    public class BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? LastUpdateOn { get; set; }

        public BaseEntity()
        {
            CreatedOn = DateTime.Now;
        }
    }
}
