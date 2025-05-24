using ITCA_Plus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ITCA_Plus.Controllers
{
    public class HomeController : Controller
    {
        ITCAPlusEntities db = new ITCAPlusEntities();

        public ActionResult Index()
        {
            var usuario = Session["cuenta"] as Usuarios;
            if (usuario == null)
                return RedirectToAction("Login", "Account");

            ViewBag.Nombre = usuario.nombre;
            ViewBag.Correo = usuario.correo;
            ViewBag.Rol = usuario.rol;

            if (usuario.fotografia != null)
                ViewBag.Fotografia = Convert.ToBase64String(usuario.fotografia);

            
                int usuarioId = usuario.id;

                var docente = db.Docente.FirstOrDefault(d => d.usuario_id == usuarioId);
                if (docente == null)
                    return View();

                int docenteId = docente.id;

                var asignaciones = db.DocenteGradoMateria
                    .Where(a => a.docente_id == docenteId)
                    .ToList();

                int totalGrados = asignaciones.Select(a => a.grado_id).Distinct().Count();

                var gradosYAnio = asignaciones
                    .Select(a => new { a.grado_id, a.anio_escolar, GradoNombre = a.Grado.nombre })
                    .Distinct()
                    .ToList();

                var totalAlumnos = db.GradoAlumno
                    .ToList()
                    .Where(ga => gradosYAnio.Any(gy => gy.grado_id == ga.grado_id && gy.anio_escolar == ga.anio_escolar))
                    .Select(ga => ga.alumno_id)
                    .Distinct()
                    .Count();

                var resumen = new ResumenDocenteViewModel
                {
                    TotalGrados = totalGrados,
                    TotalAlumnos = totalAlumnos
                };
                ViewBag.Especialidad = docente.especialidad;
                ViewBag.TotalGrados = resumen.TotalGrados;
                ViewBag.TotalAlumnos = resumen.TotalAlumnos;

                var materias = db.vw_MateriasAsignadasDocente
                    .Where(m => m.docente_id == docenteId)
                    .ToList();

                ViewBag.MateriasAsignadas = materias;

                // GRÁFICO 1: Promedio de notas por grado
                var anios = gradosYAnio.Select(g => g.anio_escolar).Distinct().ToList();

                var promedioPorGrado = db.vw_NotasAnualesPorAlumno
                    .Where(n => n.docente_id == docenteId)
                    .GroupBy(n => n.grado)
                    .Select(g => new
                    {
                        Grado = g.Key,
                        Promedio = g.Average(x => x.PromedioFinal)
                    })
                    .ToList();

                ViewBag.LabelsPromedios = promedioPorGrado.Select(p => p.Grado).ToList();
                ViewBag.DataPromedios = promedioPorGrado.Select(p => p.Promedio).ToList();

                // GRÁFICO 2: Cantidad de alumnos por grado
                var alumnosPorGrado = db.GradoAlumno
                    .ToList() // Solución aquí
                    .Where(ga => gradosYAnio.Any(g => g.grado_id == ga.grado_id && g.anio_escolar == ga.anio_escolar))
                    .GroupBy(ga => ga.Grado.nombre)
                    .Select(g => new
                    {
                        Grado = g.Key,
                        Total = g.Select(x => x.alumno_id).Distinct().Count()
                    })
                    .ToList();

                ViewBag.LabelsAlumnos = alumnosPorGrado.Select(g => g.Grado).ToList();
                ViewBag.DataAlumnos = alumnosPorGrado.Select(g => g.Total).ToList();


                // GRÁFICO 6: Distribución de notas (barras por rangos)
                var distribucionNotas = db.vw_NotasAnualesPorAlumno
                    .ToList()
                    .Where(n => n.docente_id == docenteId) // Menos restrictivo
                    .GroupBy(n =>
                    {
                        if (n.PromedioFinal >= 9) return "Excelente (9-10)";
                        if (n.PromedioFinal >= 7) return "Bueno (7-8.9)";
                        if (n.PromedioFinal >= 5) return "Regular (5-6.9)";
                        return "Deficiente (<5)";
                    })
                    .Select(g => new { Rango = g.Key, Cantidad = g.Count() })
                    .ToList();

                ViewBag.LabelsDistribucion = distribucionNotas.Select(g => g.Rango).ToList();
                ViewBag.DataDistribucion = distribucionNotas.Select(g => g.Cantidad).ToList();

            return View();
        }

    }
}