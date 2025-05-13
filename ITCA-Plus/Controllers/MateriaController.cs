using System;
using System.Collections.Generic;
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

        public ActionResult Asistencia()
        {
            List<Alumno> alumnos = contexto.Alumno.ToList();

            ViewBag.alumnos = alumnos;
            return View();
        }
    }
}