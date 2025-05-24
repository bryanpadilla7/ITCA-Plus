using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ITCA_Plus.Models;

namespace ITCA_Plus.Controllers
{
    public class MateriaController : Controller
    {
        // GET: Materia
        ITCAPlusEntities contexto = new ITCAPlusEntities();
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetGradosPorDocente(int docenteId)
        {
            var grados = contexto.DocenteGradoMateria
                .Where(dgm => dgm.docente_id == docenteId)
                .Select(dgm => new
                {
                    dgm.Grado.id,
                    dgm.Grado.nombre,
                    dgm.Grado.seccion,
                    dgm.Grado.nivel
                })
                .Distinct()
                .ToList();

            return Json(grados, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetMateriasDelDocenteEnGrado(int docenteId, int gradoId)
        {
            var materias = contexto.DocenteGradoMateria
                .Where(dgm => dgm.docente_id == docenteId && dgm.grado_id == gradoId)
                .Select(dgm => new
                {
                    materia_id = dgm.Materia.id,
                    materia_nombre = dgm.Materia.nombre
                })
                .ToList();

            return Json(materias, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListaFechasAsistencia(int materiaId)
        {
            ViewBag.materiaId = materiaId;
            return View();
        }

        public ActionResult Asistencia(int gradoId, int materiaId, DateTime fecha, string nombreMateria, int docenteId, string grado, string seccion, string horario)
        {
            ViewBag.Materia = nombreMateria;
            ViewBag.Fecha = fecha.ToString("dddd, dd MMMM yyyy");
            ViewBag.FechaRaw = fecha;
            ViewBag.DocenteId = docenteId; 
            ViewBag.Grado = grado;
            ViewBag.Seccion = seccion;
            ViewBag.Horario = horario;
            ViewBag.GradoId = gradoId;
            ViewBag.MateriaId = materiaId;

            ViewBag.ExisteAsistencia = contexto.Asistencia
            .Any(a => a.materia_id == materiaId
                   && a.grado_id == gradoId
                   && DbFunctions.TruncateTime(a.fecha) == fecha.Date);

            var alumnos = contexto.GradoAlumno
            .Where(ga =>
                ga.grado_id == gradoId
                && contexto.DocenteGradoMateria.Any(dgm =>
                    dgm.grado_id == gradoId &&
                    dgm.materia_id == materiaId &&
                    dgm.docente_id == docenteId
                )
            )
            .Select(ga => ga.Alumno)
            .Distinct()
            .ToList();

            ViewBag.alumnos = alumnos;
            return View();
        }

        [HttpPost]
        public ActionResult GuardarAsistencia(int materiaId, DateTime fecha, int gradoId, FormCollection form)
        {
            bool yaExiste = contexto.Asistencia.Any(a =>
                a.materia_id == materiaId &&
                a.grado_id == gradoId &&
                DbFunctions.TruncateTime(a.fecha) == fecha.Date);

            if (yaExiste)
                return RedirectToAction(nameof(ActualizarAsistencia),
                    new { materiaId, fecha, gradoId });

            var nuevos = ObtenerRegistrosDesdeForm(form, materiaId, gradoId, fecha);

            contexto.Asistencia.AddRange(nuevos);
            contexto.SaveChanges();
            HelperNotify.Notificar(this, "Se guardó la lista correctamente", "success");
            return RedirectToAction("Index");
        }

        private List<Asistencia> ObtenerRegistrosDesdeForm(FormCollection form, int materiaId, int gradoId, DateTime fecha)
        {
            var registros = new List<Asistencia>();

            foreach (var key in form.AllKeys.Where(k => k.StartsWith("asistencia[")))
            {
                var idStr = key.Substring("asistencia[".Length).TrimEnd(']');
                if (!int.TryParse(idStr, out int alumnoId)) continue;

                bool presente = form[key] == "1";
                registros.Add(new Asistencia
                {
                    alumno_id = alumnoId,
                    materia_id = materiaId,
                    grado_id = gradoId,
                    fecha = fecha,
                    presente = presente
                });
            }
            return registros;
        }

        [HttpPost]
        public ActionResult ActualizarAsistencia(int materiaId, DateTime fecha, int gradoId, FormCollection form)
        {
            var nuevosValores = ObtenerRegistrosDesdeForm(form, materiaId, gradoId, fecha);

            foreach (var nv in nuevosValores)
            {
                var existente = contexto.Asistencia.FirstOrDefault(a =>
                    a.materia_id == materiaId &&
                    a.grado_id == gradoId &&
                    a.alumno_id == nv.alumno_id &&
                    DbFunctions.TruncateTime(a.fecha) == fecha.Date);

                if (existente != null)
                {
                    existente.presente = nv.presente;   
                }
            }

            contexto.SaveChanges();
            HelperNotify.Notificar(this, "Se actualizó la asistencia correctamente", "success");
            return RedirectToAction("Index");
        }
    }
}