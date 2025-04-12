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
        ITCAPlusEntities contexto = new ITCAPlusEntities();
        int userActualID = 1;
        public void llenarCmb()
        {
            //Llenado de cmb para filtrar
            //cmbAlumnos
            ViewBag.cmbAlumnos = contexto.Alumno.Select(x => new SelectListItem
            {
                Text = x.nombre,
                Value = x.id.ToString(),
            });
            //cmbMateria
            ViewBag.cmbMateria = contexto.vw_MateriasAsignadasDocente.Where(x => x.docente_id == userActualID)
                .Select(x => new SelectListItem
                {
                    Text = x.materia_nombre, 
                    Value = x.materia_id.ToString()
                }).Distinct() 
                .ToList();
            //cmbGrado
            ViewBag.cmbGrado = contexto.vw_MateriasAsignadasDocente.Where(x => x.docente_id == userActualID)
                .Select(x => new SelectListItem
                {
                    Text = x.grado_nombre,
                    Value = x.grado_id.ToString()
                }).Distinct()
                .ToList();
            //cmbseccion
            ViewBag.cmbSeccion = contexto.Grado.Select(x => new SelectListItem
            {
                Text = x.seccion,
                Value = x.seccion.ToString(),
            });
            
        }
        
        // GET: notas
        public ActionResult CuadroCalificaciones()
        {
            llenarCmb();
            return View();
        }
        [HttpPost]
        public ActionResult CuadroCalificaciones(int grado, int materia)
        {
            if (ModelState.IsValid)
            {
                var notas = contexto.vw_AlumnosPorMateriaGrado.Where(n => n.grado_id == grado
                && n.materia_id == materia).ToList();

                ViewBag.NotasFiltradas = notas;
            }
            llenarCmb();
            return View();
        }

        public ActionResult CertificadoNotas()
        {
            List<Alumno> data = contexto.Alumno.ToList();
            //enviar a la lista
            ViewBag.data = data;
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
            return View(notas);
        }
    }

}