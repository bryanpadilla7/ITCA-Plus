using ITCA_Plus.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace ITCA_Plus.Controllers
{
    public class notasController : Controller
    {
        ITCAPlusEntities contexto = new ITCAPlusEntities();
        int userActualID = 1;
        public void llenarCmb()
        {
                //cmbGrado
                ViewBag.cmbGrado = contexto.vw_MateriasAsignadasDocente.Where(x => x.docente_id == userActualID)
                    .Select(x => new SelectListItem
                    {
                        Text = x.grado_nombre,
                        Value = x.grado_id.ToString()
                    }).Distinct()
                    .ToList();
        }
        [HttpPost]
        public JsonResult ObtenerAlumnosPorGrado(int grado,int mat)
        {
            var alumnos =contexto.vw_AlumnosPorMateriaGrado.Where(x => x.docente_id == userActualID
                && x.materia_id == mat && x.grado_id == grado).Select(x => new SelectListItem
                {
                    Text = x.alumno,
                    Value = x.alumno_id.ToString(),
                }).ToList();

            return Json(alumnos, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult ObtenerMaterias(int grado)
        {
            var materias = contexto.vw_MateriasAsignadasDocente.Where(x => x.docente_id == userActualID
            && x.grado_id == grado) 
                .Select(x => new SelectListItem
                {
                    Text = x.materia_nombre,
                    Value = x.materia_id.ToString()
                }).Distinct()
                .ToList();

            return Json(materias, JsonRequestBehavior.AllowGet);
        }

        // GET: notas
        public ActionResult CuadroCalificaciones()
        {
             llenarCmb();
            return View();
        }
        [HttpPost]
        public ActionResult CuadroCalificaciones(int grado, int materia, int alumno=0)
        {
            if (ModelState.IsValid)
            {
                ModelState.Remove("grado");
                llenarCmb();
                if (alumno != 0)
                {
                  var notas = contexto.vw_AlumnosPorMateriaGrado.Where(n => n.grado_id == grado
                        && n.materia_id == materia && n.alumno_id == alumno).ToList();
                    ViewBag.NotasFiltradas = notas;
                }
                else
                {
                    var notas = contexto.vw_AlumnosPorMateriaGrado.Where(n => n.grado_id == grado
                        && n.materia_id == materia).ToList();
                    ViewBag.NotasFiltradas = notas;
                }
            }
            
            return View();
        }

        public ActionResult CertificadoNotas()
        {
            List<Alumno> data = contexto.Alumno.ToList();
            //enviar a la lista
            ViewBag.data = data;
            return View();
        }
        /*[HttpPost]  
        public ActionResult CertificadoNotas()
        {
            if (ModelState.IsValid)
            {
                // Procesar la lógica de guardado
                // Redirigir a otra acción o devolver una vista
            }
            return View();
        }*/
    }

}