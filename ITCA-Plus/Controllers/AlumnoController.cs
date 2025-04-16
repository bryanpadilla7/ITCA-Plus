using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ITCA_Plus.Models;

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

            if(fotoAlumno != null && fotoAlumno.ContentLength > 0)
            {
                byte[] imagenData = null;
                using (var binaryAlumno = new BinaryReader(fotoAlumno.InputStream))
                {
                    imagenData = binaryAlumno.ReadBytes(fotoAlumno.ContentLength);
                }
                a.fotografia = imagenData;
            }
            switch (accion)
            {
                case "Guardar":
                    a.carnet = GenerarCarnet();
                    contexto.Alumno.Add(a);
                    contexto.SaveChanges();
                    break;
                case "Modificar":
                    if (modificacion.Equals("si"))
                    {
                        Alumno temp = contexto.Alumno.FirstOrDefault(s => s.carnet == a.carnet);
                        temp.nombre = a.nombre;
                        temp.fecha_nacimiento = a.fecha_nacimiento;
                        temp.genero = a.genero;
                        temp.fotografia = a.fotografia;
                        contexto.SaveChanges();
                    }
                    break;
                case "Eliminar":
                    contexto.Alumno.Remove(contexto.Alumno.FirstOrDefault(s => s.carnet == a.carnet));
                    contexto.SaveChanges();
                    break;

            }
            return RedirectToAction("Alumnos");
        }

        public ActionResult convertirImagen(int codAlumno)
        {
            var fotoAlumno = contexto.Alumno.Where(a => a.id == codAlumno).FirstOrDefault();
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
            if(int.TryParse(numeroS, out numero))
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