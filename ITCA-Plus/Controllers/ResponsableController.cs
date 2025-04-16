using ITCA_Plus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ITCA_Plus.Controllers
{
    public class ResponsableController : Controller
    {
        ITCAPlusEntities contexto = new ITCAPlusEntities();
        // GET: Responsable
        public ActionResult Responsables(int id)
        {
            ViewBag.cmbParentesco = new List<SelectListItem>
            {
                new SelectListItem { Text = "Padre", Value="Padre"},
                new SelectListItem { Text = "Madre", Value="Madre"},
                new SelectListItem { Text = "Otro", Value="Otro"}
            };

            var alumno = contexto.Alumno.FirstOrDefault(a => a.id == id);
            ViewBag.nombreAlumno = alumno?.nombre;

            var model = new ResponsableViewModel
            {
                AlumnoId = id,
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult AgregarResponsable(ResponsableViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new ITCAPlusEntities())
                {
                    var responsable = new Responsable
                    {
                        nombre = model.nombre,
                        parentesco = model.parentesco,
                        telefono = model.telefono,
                        direccion = model.direccion
                    };

                    context.Responsable.Add(responsable);
                    context.SaveChanges();

                    var alumnoResponsable = new AlumnoResponsable
                    {
                        alumno_id = model.AlumnoId,
                        responsable_id = responsable.id
                    };

                    context.AlumnoResponsable.Add(alumnoResponsable);
                    context.SaveChanges();
                }

                return RedirectToAction("Alumnos", "Alumno");
            }

            ViewBag.cmbParentesco = new List<SelectListItem>
            {
                new SelectListItem { Text = "Padre", Value="Padre" },
                new SelectListItem { Text = "Madre", Value="Madre" },
                new SelectListItem { Text = "Otro", Value="Otro" }
            };

            return View(model);
        }
        public ActionResult Responsable()
        {
            return View();
        }

        [HttpPost]
        public ActionResult EditarResponsable()
        {
            return RedirectToAction("Alumnos", "Alumno");
        }
    }
}