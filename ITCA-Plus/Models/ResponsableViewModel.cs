using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ITCA_Plus.Models
{
    public class ResponsableViewModel
    {
        public int AlumnoId { get; set; }

        [Required]
        public string nombre { get; set; }

        [Required]
        public string parentesco { get; set; }

        [Required]
        public string telefono { get; set; }

        [Required]
        public string direccion { get; set; }
    }
}