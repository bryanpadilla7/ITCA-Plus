using ITCA_Plus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace ITCA_Plus.Controllers
{
    public class notasController : Controller
    {
        public void listaGrados()
        {
            //tendre que hacer la consulta de que usuario docente esta logeado, para solo ponerle en estas listas
            //los grados con los que trabaja.
            ViewBag.grado_estudiante = new List<SelectListItem>
                {
                   new SelectListItem { Text = "Primer grado", Value="1"},
                   new SelectListItem { Text = "Segundo grado", Value="2"},
                   new SelectListItem { Text = "Tercer grado", Value="3"},
                   new SelectListItem { Text = "Cuarto grado", Value="4"},
                   new SelectListItem { Text = "Quinto grado", Value="5"},
                   new SelectListItem { Text = "Sexto grado", Value="6"},
                   new SelectListItem { Text = "Séptimo grado", Value="7"},
                   new SelectListItem { Text = "Octavo grado", Value="8"},
                   new SelectListItem { Text = "Noveno grado", Value="9"}
                };
        }
        public void seccionGrados()
        {
            ViewBag.seccion = new List<SelectListItem>
                {
                   new SelectListItem { Text = "A", Value="A"},
                   new SelectListItem { Text = "B", Value="B"}
                };
        }
        // GET: notas
        public ActionResult CuadroCalificaciones()
        {
            listaGrados();
            seccionGrados();
            return View();
        }
        [HttpPost]
        public ActionResult CuadroCalificaciones(notasEstudiantes notas)
        {

            if (ModelState.IsValid)
            {
                // Procesar la lógica de guardado
                // Redirigir a otra acción o devolver una vista
            }
            listaGrados();
            seccionGrados();
            return View(notas);
        }

        public ActionResult CertificadoNotas()
        {

            listaGrados();
            seccionGrados();
            return View();
        }
        [HttpPost]  
        public ActionResult CertificadoNotas(notasEstudiantes notas)
        {
            if (ModelState.IsValid)
            {
                // Procesar la lógica de guardado
                // Redirigir a otra acción o devolver una vista
            }
            listaGrados();
            seccionGrados();
            return View(notas);
        }
    }

}