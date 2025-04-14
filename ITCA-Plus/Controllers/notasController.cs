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
        public void llenarCmb(int cmb)
        {
            switch (cmb)
            {
                case 1:
                    //cmbGrado
                    ViewBag.cmbGrado = contexto.vw_MateriasAsignadasDocente.Where(x => x.docente_id == userActualID)
                        .Select(x => new SelectListItem
                        {
                            Text = x.grado_nombre,
                            Value = x.grado_id.ToString()
                        }).Distinct()
                        .ToList();
                    break;
                case 2:
                    ViewBag.trimestres = new List<SelectListItem>
                {
                   new SelectListItem { Text = "Primer Trimestre", Value="Primer Trimestre"},
                   new SelectListItem { Text = "Segundo Trimestre", Value="Segundo Trimestre"},
                   new SelectListItem { Text = "Tercer Trimestre", Value="Tercer Trimestre"},
                };
                    break;
            }
            
        }
        [HttpPost]
        public JsonResult ObtenerAlumnosPorGrado(int grado, int mat)
        {
            var alumnos = contexto.vw_AlumnosPorMateriaGrado.Where(x => x.docente_id == userActualID
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
            llenarCmb(1);
            return View();
        }
        [HttpPost]
        public ActionResult CuadroCalificaciones(int grado, int materia, int alumno = 0)
        {
            if (ModelState.IsValid)
            {
                /*Aqui debo de agregar la sentencia de buscar en la tabla NotasEditar si ya me dieron permiso de edicion o no
                ViewBag.permisoEdicion = contexto.NotasEditar.Where(x => x.permiso == 1)
                */
                ViewBag.NotasYaGuadadas = contexto.Notas.Where(x => x.trimestres == "Primer Trimestre" 
                || x.trimestres == "Segundo Trimestre" || x.trimestres == "Tercer Trimestre").ToList();
                    ModelState.Remove("grado");
                    llenarCmb(1);
                    if (alumno != 0)
                    {
                        var notas = contexto.vw_AlumnosPorMateriaGrado.Where(n => n.grado_id == grado
                              && n.materia_id == materia && n.alumno_id == alumno).ToList();
                        ViewBag.ListadoPorMateriaGrado = notas;
                    }
                    else
                    {
                        var notas = contexto.vw_AlumnosPorMateriaGrado.Where(n => n.grado_id == grado
                            && n.materia_id == materia).ToList();
                        ViewBag.ListadoPorMateriaGrado = notas;
                    }
            }

            return View();
        }
        [HttpPost]
        public JsonResult  IngresoNotas(Notas n)
        {
            string accion = Request.Form["boton"].ToString();
            string resultado = "";
            switch (accion)
            {
                case "Guardar":
                    contexto.Notas.Add(n);
                    contexto.SaveChanges();
                    resultado = "Guardadas";
                    break;
                case "Editar":
                    Notas temp = contexto.Notas.FirstOrDefault(x => x.alumno_id == n.alumno_id && x.materia_id == n.materia_id && x.anio_escolar_id == n.anio_escolar_id);
                    temp.alumno_id = n.alumno_id;
                    temp.materia_id = n.materia_id;
                    temp.anio_escolar_id = n.anio_escolar_id;
                    temp.nota1 = n.nota1;
                    temp.nota2 = n.nota2;
                    temp.nota3 = n.nota3;
                    contexto.SaveChanges();
                    resultado = "Modificadas";
                    break;
            }
            
            return Json(resultado);
        }
        [HttpPost]
        public JsonResult NotasGuardadas(int materia, int alumno, string trimestre)
        {
            List<Notas> data = contexto.Notas.Where(x => x.alumno_id == alumno && x.materia_id == materia && x.trimestres == trimestre).ToList();
            ViewBag.data = data;
            string resultado = "";
            if (data.Count() > 0)
            {
                 resultado = "Existe";
            }
           
            return Json(resultado);
        }

        [HttpPost]
        public JsonResult EditarNotas()
        {

            string resultado = "";
            return Json(resultado);
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