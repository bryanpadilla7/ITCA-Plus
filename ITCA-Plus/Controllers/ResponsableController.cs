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
                    HelperNotify.Notificar(this, "Responsable agregado correctamente", "success");
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
        public ActionResult Responsable(int id)
        {
            ViewBag.cmbParentesco = new List<SelectListItem>
            {
                new SelectListItem { Text = "Padre", Value="Padre"},
                new SelectListItem { Text = "Madre", Value="Madre"},
                new SelectListItem { Text = "Otro", Value="Otro"}
            };

            var alumno = contexto.Alumno.FirstOrDefault(a => a.id == id);
            ViewBag.nombreAlumno = alumno?.nombre;

            var relacion = contexto.AlumnoResponsable
                .FirstOrDefault(ar => ar.alumno_id == id);

            ResponsableViewModel model = new ResponsableViewModel
            {
                AlumnoId = id
            };

            if (relacion != null)
            {
                var responsable = contexto.Responsable
                    .FirstOrDefault(r => r.id == relacion.responsable_id);

                if (responsable != null)
                {
                    model.nombre = responsable.nombre;
                    model.parentesco = responsable.parentesco;
                    model.telefono = responsable.telefono;
                    model.direccion = responsable.direccion;
                }
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult EditarResponsable(ResponsableViewModel model)
        {
            var relacion = contexto.AlumnoResponsable
                .FirstOrDefault(ar => ar.alumno_id == model.AlumnoId);

            if (relacion != null)
            {
                var responsable = contexto.Responsable
                    .FirstOrDefault(r => r.id == relacion.responsable_id);

                if (responsable != null)
                {
                    responsable.nombre = model.nombre;
                    responsable.parentesco = model.parentesco;
                    responsable.telefono = model.telefono;
                    responsable.direccion = model.direccion;
                    contexto.SaveChanges();
                    HelperNotify.Notificar(this, "Responsable modificado correctamente", "success");
                }
            }
            return RedirectToAction("Alumnos", "Alumno");
        }
    }
}