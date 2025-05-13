using ITCA_Plus.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using static System.Net.Mime.MediaTypeNames;
using ITCA_Plus.Models;

namespace ITCA_Plus.Controllers
{
    public class notasController : Controller
    {
        
        ITCAPlusEntities contexto = new ITCAPlusEntities();
        //int userActualID = 2;
        public Usuarios UsuarioActual => Session["cuenta"] as Usuarios;

        public int UsuarioID => UsuarioActual?.id ?? 0;
        int anioActual = DateTime.Now.Year;
        DateTime fecha = DateTime.Now;
        public void llenarCmb()
        {
            

            if (UsuarioActual.rol.ToString() == "Admin"){
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
                ViewBag.cmbGrado = contexto.vw_MateriasAsignadasDocente.Where(x => x.docente_id == UsuarioID)
                    .Select(x => new SelectListItem
                    {
                        Text = x.grado_nombre,
                        Value = x.grado_id.ToString()
                    }).Distinct()
                    .ToList();
            }
                    

                    
            
        }
        [HttpPost]
        public JsonResult ObtenerAlumnosPorGrado(int grado, int mat, int docente =0)
        {
            if (UsuarioActual.rol.ToString() == "Admin")
            {
                var alumnos = contexto.vw_AlumnosPorMateriaGrado.Where(x => x.docente_id == docente
                && x.materia_id == mat && x.grado_id == grado && x.anio_escolar == anioActual).Select(x => new SelectListItem
                {
                    Text = x.alumno,
                    Value = x.alumno_id.ToString(),
                }).ToList();

                return Json(alumnos, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var alumnos = contexto.vw_AlumnosPorMateriaGrado.Where(x => x.docente_id == UsuarioID
                && x.materia_id == mat && x.grado_id == grado && x.anio_escolar == anioActual).Select(x => new SelectListItem
                {
                    Text = x.alumno,
                    Value = x.alumno_id.ToString(),
                }).ToList();

                return Json(alumnos, JsonRequestBehavior.AllowGet);
            }
                
        }
        [HttpPost]
        public JsonResult ObtenerMaterias(int grado, int docente=0)
        {
            if (UsuarioActual.rol.ToString() == "Admin")
            {
                var materias = contexto.vw_MateriasAsignadasDocente.Where(x => x.docente_id == docente
           && x.grado_id == grado && x.anio_escolar == anioActual)
               .Select(x => new SelectListItem
               {
                   Text = x.materia_nombre,
                   Value = x.materia_id.ToString()
               }).Distinct()
               .ToList();
                return Json(materias, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var materias = contexto.vw_MateriasAsignadasDocente.Where(x => x.docente_id == UsuarioID
           && x.grado_id == grado && x.anio_escolar == anioActual)
               .Select(x => new SelectListItem
               {
                   Text = x.materia_nombre,
                   Value = x.materia_id.ToString()
               }).Distinct()
               .ToList();
                return Json(materias, JsonRequestBehavior.AllowGet);
            }
               

            
        }

        // GET: notas
        public ActionResult CuadroCalificaciones()
        {
            if (UsuarioActual != null)
            {
                llenarCmb();
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }
        [HttpPost]
        public ActionResult CuadroCalificaciones(int grado, int materia, int alumno = 0)
        {
            if (ModelState.IsValid)
            {

                ViewBag.permisoEdicion = contexto.notiCambioNota.Where(x => x.permiso == true).ToList();
                
                ViewBag.NotasYaGuadadas = contexto.Notas.Where(x => x.trimestres == "Primer Trimestre" 
                || x.trimestres == "Segundo Trimestre" || x.trimestres == "Tercer Trimestre").ToList();
                    ModelState.Remove("grado");
                    llenarCmb();
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
                    HelperNotify.Notificar(this, "Notas guardadas con éxito", "success");
                    break;
                case "Editar":
                    //Aqui trae el dato para modificarlo por el nuevo.
                    Notas temp = contexto.Notas.FirstOrDefault(x => x.alumno_id == n.alumno_id && x.materia_id == n.materia_id && x.anio_escolar == n.anio_escolar);
                    temp.alumno_id = n.alumno_id;
                    temp.materia_id = n.materia_id;
                    temp.anio_escolar = n.anio_escolar;
                    temp.nota1 = n.nota1;
                    temp.nota2 = n.nota2;
                    temp.nota3 = n.nota3;
                    contexto.SaveChanges();
                    //Aqui vamos a modificar al notiCambios para que me desahibilite el btn otra ves
                    notiCambioNota edi = contexto.notiCambioNota.FirstOrDefault(x => x.alumno_id == n.alumno_id && x.materia_id == n.materia_id && x.trimestres == n.trimestres && x.docente_id == UsuarioID);
                    edi.permiso = false;
                    edi.fechaCierre = fecha;
                    contexto.SaveChanges();
                    resultado = "Modificadas";
                    HelperNotify.Notificar(this, "Notas modificadas con éxito", "info");
                    break;
            }
            
            return Json(resultado);
        }
        [HttpPost]
        public JsonResult NotasGuardadas(int materia, int alumno, string trimestre)
        {
            var data = contexto.Notas.FirstOrDefault(x => x.alumno_id == alumno && x.materia_id == materia && x.trimestres == trimestre);
            ViewBag.data = data;
            if (data != null)
            {
                return Json(new
                {
                    existe = true,
                    nota1 = data.nota1,
                    nota2 = data.nota2,
                    nota3 = data.nota3
                },  JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { existe = false }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult EditarNotas(int idMateria, string nombreMateria, int idDoc, int idAlum, string nombreAlum)
        {
            if (UsuarioActual != null)
            {
                var trimestresEnNotiCambio = contexto.notiCambioNota.Where(x => x.alumno_id == idAlum && x.materia_id == idMateria && x.docente_id == UsuarioID)
                                     .Select(x => x.trimestres)
                                     .ToList();

                var lista = contexto.Notas
                    .Where(d => d.alumno_id == idAlum
                 && d.materia_id == idMateria
                 && !trimestresEnNotiCambio.Contains(d.trimestres))
                    .Select(d => new SelectListItem
                    {
                        Value = d.trimestres.ToString(),
                        Text = d.trimestres
                    })
                    .Distinct()
                    .ToList();
                ViewBag.idMateria = idMateria;
                ViewBag.nombreMateria = nombreMateria;
                ViewBag.idDoc = idDoc;
                ViewBag.idAlum = idAlum;
                ViewBag.nombreAlum = nombreAlum;
                var nombreDoc = contexto.Usuarios.FirstOrDefault(d => d.id == idDoc);
                ViewBag.nombreDoc = nombreDoc;
                ViewBag.trimestres = lista;

                return View();
            }
            else
            {
                return RedirectToAction("login", "AccountController");
            }
            
        }
        [HttpPost]
        public ActionResult GuardarSolicitudNotas(notiCambioNota edi)
        {
                contexto.notiCambioNota.Add(edi);
                contexto.SaveChanges();
            TempData["msj"] = "Se guardo";
            HelperNotify.Notificar(this, "Se ha guardado la solicitud de edición", "info");
            return RedirectToAction("CuadroCalificaciones");
             
        }
        
        
    }


}