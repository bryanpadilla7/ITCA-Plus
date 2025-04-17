using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITCA_Plus.Models
{
    public class NotasReporteViewModel
    {
        public string carnet { get; set; }
        public string alumno { get; set; }
        public string materia { get; set; }
        public decimal Trimestre1 { get; set; }
        public decimal Trimestre2 { get; set; }
        public decimal Trimestre3 { get; set; }
        public decimal PromedioFinal { get; set; }
    }
}