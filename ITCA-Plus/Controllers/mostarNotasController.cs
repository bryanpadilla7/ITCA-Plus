using ITCA_Plus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ITCA_Plus.Controllers
{
    public class mostarNotasController : Controller
    {
        ITCAPlusEntities contexto = new ITCAPlusEntities();
        int userActualID =2 ;
        int anioActual = DateTime.Now.Year;
        public void llenarcmb()
        {
            //cmbGrado este solo sirve si es el docentequien esta en la vis
            //debo mostrarle todos los grados al admin
            if (Session["rolUser"] == "Admin")
            {
                var listaDocentes = (from d in contexto.Docente
                                     join u in contexto.Usuarios on d.usuario_id equals u.id
                                     select new SelectListItem
                                     {
                                         Value = d.id.ToString(),
                                         Text = u.nombre
                                     }).ToList();

                ViewBag.cmbDocentes = listaDocentes;
                //cmbGrado
                ViewBag.cmbGrado = contexto.vw_MateriasAsignadasDocente
                    .Select(x => new SelectListItem
                    {
                        Text = x.grado_nombre,
                        Value = x.grado_id.ToString()
                    }).Distinct()
                    .ToList();
            }
            else
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
        }
        [HttpPost]
        public JsonResult NotasPorAlumno(int alumno, int materiaid)
        {
            var nombreMateria= contexto.Materia.FirstOrDefault(mt => mt.id== materiaid); 
                var data = contexto.vw_NotasPorAlumno.Where(x => x.alumno_id == alumno && x.materia == nombreMateria.nombre).ToList();
                ViewBag.data = data;
                if (data != null && data.Any())
                {
                    var notas = data.Select(x => new {
                        periodo = x.trimestres,
                        nota1 = x.nota1,
                        nota2 = x.nota2,
                        nota3 = x.nota3,
                        prom = x.promedio
                    }).ToList();

                    return Json(new
                    {
                        existe = true,
                        notas = notas
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { existe = false }, JsonRequestBehavior.AllowGet);
                }
           
        }

        [HttpPost]
        public JsonResult NotasPorGrupo(int materia, string grado)
        {
            var nombreMateria = contexto.Materia.FirstOrDefault(mt => mt.id == materia);
            var data = contexto.vw_NotasPorAlumno.Where(x => x.materia == nombreMateria.nombre && x.grado == grado).ToList();

            if (data != null && data.Any())
            {
                var notas = data.Select(x => new {
                    periodo = x.trimestres,
                    carnet= x.carnet,
                    nombre= x.alumno,
                    nota1 = x.nota1,
                    nota2 = x.nota2,
                    nota3 = x.nota3,
                    prom = x.promedio
                }).ToList();
                return Json(new
                {
                    existe = true,
                    notas = notas
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { existe = false }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: mostarNotas
        public ActionResult CertificadoNotas()
        {
            
            llenarcmb();
            return View();
        }
        [HttpPost]
         public ActionResult CertificadoNotas(int grado, int materia, int alumno = 0)
        {
            if (ModelState.IsValid)
            {
                ModelState.Remove("grado");
                llenarcmb();
                if (alumno != 0)
                {
                    var notas = contexto.vw_AlumnosPorMateriaGrado.Where(n => n.grado_id == grado
                          && n.materia_id == materia && n.alumno_id == alumno && n.anio_escolar == anioActual).ToList();
                    ViewBag.ListadoPorMateriaGrado = notas;
                }
                else
                {
                    var notas = contexto.vw_AlumnosPorMateriaGrado.Where(n => n.grado_id == grado
                        && n.materia_id == materia && n.anio_escolar == anioActual).ToList();
                    ViewBag.ListadoPorMateriaGrado = notas;
                }
            }
            return View();
        }
        public ActionResult reportesEspeciales()
        {
            //cmbGrado
            ViewBag.cmbGrado = contexto.vw_MateriasAsignadasDocente
                .Select(x => new SelectListItem
                {
                    Text = x.grado_nombre,
                    Value = x.grado_nombre.ToString()
                }).Distinct()
                .ToList();
            
           int startYear = 2024;
            int currentYear = DateTime.Now.Year;
            // Genera una lista desde el 2024 hasta el año inmediatamente anterior al actual
            var anios = Enumerable.Range(startYear, currentYear - startYear + 1)
                      .OrderByDescending(y => y)
                      .Select(y => new SelectListItem
                      {
                          Text = y.ToString(),
                          Value = y.ToString()
                      })
                      .ToList();

            // Asignamos la lista al ViewBag
            ViewBag.cmbAnioReportes = anios;
            ViewBag.listadoter = ViewBag.listado;
            return View();
        }
        [HttpPost]
        public ActionResult reportesEspeciales(string grado, string materia=null, int año=2025)
        {
            var consulta = contexto.vw_NotasAnualesPorAlumno
        .Where(x => x.grado == grado && x.anio_escolar == año);

            if (!string.IsNullOrEmpty(materia))
            {
                consulta = consulta.Where(x => x.materia == materia);
            }

            var listado = consulta
                .Select(x => new {
                    x.carnet,
                    x.alumno,
                    x.materia,
                    x.Trimestre1,
                    x.Trimestre2,
                    x.Trimestre3,
                    x.PromedioFinal
                })
                .Distinct()
                .ToList();

            ViewBag.listado = listado;
            //cmbGrado
            ViewBag.cmbGrado = contexto.vw_MateriasAsignadasDocente
                .Select(x => new SelectListItem
                {
                    Text = x.grado_nombre,
                    Value = x.grado_nombre.ToString()
                }).Distinct()
                .ToList();

            int startYear = 2024;
            int currentYear = DateTime.Now.Year;
            // Genera una lista desde el 2024 hasta el año inmediatamente anterior al actual
            var anios = Enumerable.Range(startYear, currentYear - startYear + 1)
                       .OrderByDescending(y => y)
                       .Select(y => new SelectListItem
                       {
                           Text = y.ToString(),
                           Value = y.ToString()
                       })
                       .ToList();

            // Asignamos la lista al ViewBag
            ViewBag.cmbAnioReportes = anios;
            return View();
        }
        [HttpPost]
        public JsonResult ObtenerMaterias(string grado)
        {
           
                var materias = contexto.vw_MateriasAsignadasDocente.Where(x => x.grado_nombre == grado && x.anio_escolar == anioActual)
               .Select(x => new SelectListItem
               {
                   Text = x.materia_nombre,
                   Value = x.materia_nombre.ToString()
               }).Distinct()
               .ToList();
                return Json(materias, JsonRequestBehavior.AllowGet);
        }
    }
}