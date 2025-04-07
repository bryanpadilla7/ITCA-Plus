using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ITCA_Plus.Models
{
    public class notasEstudiantes
    {
        
        public string nombreCompletoEstudiante {  get; set; }
        [Required(ErrorMessage = "Ingrese el grado al que desea calificar")]
        public int gradoEstudiante { get; set; }
        [Required(ErrorMessage = "Especifique la sección a la que hace referencia")]
        public char seccionEstudiante { get; set; }
        [Required(ErrorMessage = "Rellene el campo Materia")]
        public string materiaGrado { get; set; }


        public notasEstudiantes() { }
    }
}