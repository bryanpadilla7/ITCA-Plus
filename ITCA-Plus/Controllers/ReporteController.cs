using ITCA_Plus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ITCA_Plus.Controllers
{
    public class ReporteController : Controller
    {
        ITCAPlusEntities db = new ITCAPlusEntities();

        public ActionResult NotasPorAlumno()
        {
            return View();
        }

        public ActionResult NotasAnuales()
        {
            return View();
        }

        public ActionResult Asistencia()
        {
            return View();
        }

        public ActionResult ReporteUsuarios()
        {
            return View();
        }
        public ActionResult ReporteGrados()
        {
            return View();
        }
        public ActionResult ReporteMaterias()
        {
            return View();
        }

        public ActionResult ObtenerMaterias()
        {
            var datos = db.Materia
                .Select(m => new
                {
                    m.id,
                    m.nombre
                }).ToList();

            return Json(new { data = datos }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ObtenerGrados()
        {
            var datos = db.Grado
                .Select(g => new
                {
                    g.id,
                    g.nombre,
                    g.seccion
                }).ToList();

            return Json(new { data = datos }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ObtenerNotasPorAlumno()
        {
            var usuario = Session["cuenta"] as Usuarios;
            var datos = db.vw_NotasPorAlumno
                .Where(x => x.docente_id == usuario.id)
                .ToList();
            return Json(new { data = datos }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ObtenerNotasAnuales()
        {
            var usuario = Session["cuenta"] as Usuarios;

            var datos = db.vw_NotasAnualesPorAlumno
                .Where(x => x.docente_id == usuario.id)
                .ToList();
            return Json(new { data = datos }, JsonRequestBehavior.AllowGet);

        }

        public JsonResult ObtenerAsistencia()
        {
            var usuario = Session["cuenta"] as Usuarios;
            var datos = db.vw_ListadoAsistencia
                .Where(x => x.docente == usuario.nombre)
                .ToList();
            return Json(new { data = datos }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ObtenerPerfilesDocentes()
        {
            var datos = db.vw_PerfilDocente
                .Select(d => new
                {
                    d.docente_id,
                    d.usuario_id,
                    d.nombre_docente,
                    d.telefono,
                    d.correo,
                    d.rol,
                    d.especialidad
                }).ToList();

            return Json(new { data = datos }, JsonRequestBehavior.AllowGet);
        }
    }
}