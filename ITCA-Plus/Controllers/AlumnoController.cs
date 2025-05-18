using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ITCA_Plus.Models;
using System.Timers;

namespace ITCA_Plus.Controllers
{
    public class AlumnoController : Controller
    {
        // GET: Alumno
        ITCAPlusEntities contexto = new ITCAPlusEntities();
        public ActionResult Alumnos()
        {
            List<Alumno> alumnos = contexto.Alumno.ToList();

            var tieneResponsable = contexto.AlumnoResponsable
                .Select(ar => ar.alumno_id)
                .Distinct()
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();

            ViewBag.alumnos = alumnos;
            ViewBag.tieneResponsable = tieneResponsable;
            ViewBag.nuevoCarnet = GenerarCarnet();

            ViewBag.cmbGenero = new List<SelectListItem>
            {
                new SelectListItem { Text = "Masculino", Value="Masculino"},
                new SelectListItem { Text = "Femenino", Value="Femenino"}
            };

            return View();
        }
        [HttpPost]
        public ActionResult AgregarAlumnos(Alumno a, HttpPostedFileBase fotoAlumno)
        {
            string accion = Request.Form["boton"].ToString();
            string eliminacion = Request.Form["eliminacion"].ToString();
            string modificacion = Request.Form["modificacion"].ToString();
            string guardar = Request.Form["guardar"].ToString();

            switch (accion)
            {
                case "Guardar":
                    if (guardar.Equals("si"))
                    {
                        if (fotoAlumno != null && fotoAlumno.ContentLength > 0)
                        {
                            using (var binaryAlumno = new BinaryReader(fotoAlumno.InputStream))
                            {
                                a.fotografia = binaryAlumno.ReadBytes(fotoAlumno.ContentLength);
                            }
                        }

                        a.carnet = GenerarCarnet();
                        contexto.Alumno.Add(a);
                        contexto.SaveChanges();
                        HelperNotify.Notificar(this, "Registro agregado correctamente", "success");
                    }
                    break;
                case "Modificar":
                    if (modificacion.Equals("si"))
                    {
                        Alumno temp = contexto.Alumno.FirstOrDefault(s => s.carnet == a.carnet);

                        if (temp != null)
                        {
                            temp.nombre = a.nombre;
                            temp.fecha_nacimiento = a.fecha_nacimiento;
                            temp.genero = a.genero;

                            if (fotoAlumno != null && fotoAlumno.ContentLength > 0)
                            {
                                using (var binaryAlumno = new BinaryReader(fotoAlumno.InputStream))
                                {
                                    temp.fotografia = binaryAlumno.ReadBytes(fotoAlumno.ContentLength);
                                }
                            }
                            contexto.SaveChanges();
                            HelperNotify.Notificar(this, "Registro modificado correctamente", "success");
                        }
                    }
                    break;
                case "Eliminar":
                    contexto.Alumno.Remove(contexto.Alumno.FirstOrDefault(s => s.carnet == a.carnet));
                    contexto.SaveChanges();
                    HelperNotify.Notificar(this, "Registro eliminado correctamente", "success");

                    break;

            }
            return RedirectToAction("Alumnos");
        }

        public ActionResult convertirImagen(int codAlumno)
        {
            var fotoAlumno = contexto.Alumno.Where(a => a.id == codAlumno).FirstOrDefault();
            if (fotoAlumno == null || fotoAlumno.fotografia == null)
            {
                return HttpNotFound("Imagen no encontrada.");
            }
            return File(fotoAlumno.fotografia, "image/jpg");
        }

        private string GenerarCarnet()
        {
            var ultimoCarnet = contexto.Alumno
                .OrderByDescending(a => a.carnet)
                .Select(a => a.carnet)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(ultimoCarnet))
            {
                return "A001";
            }

            string numeroS = ultimoCarnet.Substring(1);
            int numero;
            if (int.TryParse(numeroS, out numero))
            {
                numero++;
                return "A" + numero.ToString("D3");
            }
            else
            {
                return "A001";
            }
        }
    }
}