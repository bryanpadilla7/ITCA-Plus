using ITCA_Plus.Models;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ITCA_Plus.Controllers
{
    public class GradoMateriaController : Controller
    {
        ITCAPlusEntities db = new ITCAPlusEntities();

        public ActionResult Index()
        {
            return View();
        }
        // Obtener lista de grados
        public JsonResult GetGrados()
        {
            var grados = db.Grado.Select(g => new
            {
                g.id,
                g.nombre,
                g.seccion,
                g.nivel
            }).ToList();

            return Json(grados, JsonRequestBehavior.AllowGet);
        }

        // Obtener un grado específico
        public JsonResult GetGrado(int id)
        {
            var grado = db.Grado.FirstOrDefault(g => g.id == id);

            if (grado != null)
            {
                return Json(new
                {
                    success = true,
                    grado = new
                    {
                        grado.id,
                        grado.nombre,
                        grado.seccion,
                        grado.nivel
                    }
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, message = "Grado no encontrado" }, JsonRequestBehavior.AllowGet);
        }

        // Agregar Grado
        [HttpPost]
        public JsonResult AgregarGrado(Grado grado)
        {
                var cadena = grado.nombre.Split('-').ToList();
                grado.nombre = cadena[0].Trim();
                grado.nivel = (byte?)int.Parse(cadena[1].Trim());

                db.Grado.Add(grado);
                db.SaveChanges();

                return Json(new { success = true });
        }

        // Actualizar Grado
        [HttpPost]
        public JsonResult ActualizarGrado(Grado model)
        {
                var grado = db.Grado.FirstOrDefault(g => g.id == model.id);

                if (grado != null)
                {
                    var cadena = model.nombre.Split('-').ToList();
                    grado.nombre = cadena[0].Trim();
                    grado.nivel = (byte?)int.Parse(cadena[1].Trim());
                    grado.seccion = model.seccion;

                    db.SaveChanges();
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Grado no encontrado" });
        }


        //logica de las materias docente alumno
        public ActionResult Asignar()
        {
            return View();
        }

        [HttpGet]
        public JsonResult ObtenerDatosGrado(int id)
        {
            var grado = db.Grado.FirstOrDefault(g => g.id == id);
            if (grado == null)
            {
                return Json(new { success = false, message = "Grado no encontrado" }, JsonRequestBehavior.AllowGet);
            }

            var cantidadMaterias = db.DocenteGradoMateria.Count(d => d.grado_id == id);
            var cantidadAlumnos = db.GradoAlumno.Count(d => d.grado_id == id);

            var materias = db.Materia
                .Where(m => m.estado == true)
                .Select(m => new { m.id, m.nombre })
                .ToList();

            return Json(new
            {
                success = true,
                grado = new { grado.nombre, grado.seccion },
                cantidadMaterias,
                cantidadAlumnos,
                materias
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetDocentesPorEspecialidad(int materiaId)
        {
            var materia = db.Materia.FirstOrDefault(m => m.id == materiaId);
            if (materia == null)
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }

            var docentes = db.vw_PerfilDocente
                .Where(d => d.especialidad == materia.nombre && d.estado == true)
                .Select(d => new { 
                    d.nombre_docente, 
                    d.especialidad, 
                    d.carnet,
                    d.docente_id 
                }).ToList();


            return Json(docentes, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMateriasAsignadas(int gradoId)
        {
            var materias = (from m in db.vw_MateriasAsignadasDocente
                            join d in db.Docente on m.docente_id equals d.id
                            join u in db.Usuarios on d.usuario_id equals u.id
                            where m.grado_id == gradoId
                            select new
                            {
                                m.materia_id,
                                m.materia_nombre,
                                m.docente_nombre,
                                carnet = u.usuario, // o el campo que represente el carnet
                                m.docente_id
                            }).ToList();

            return Json(materias, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult AsignarMateria(int gradoId, int materiaId, int docenteId)
        {
            

            // Validación: comprobar si ya existe esa materia en ese grado
            bool existe = db.DocenteGradoMateria
                .Any(x => x.grado_id == gradoId && x.materia_id == materiaId);

            if (existe)
            {
                return Json(new { success = false, message = "La materia ya está asignada a este grado." });
            }

            // Obtener el año actual en formato corto
            string anio = DateTime.Now.Year.ToString(); // "2025"

            // Insertar nueva asignación
            var asignacion = new DocenteGradoMateria
            {
                grado_id = gradoId,
                materia_id = materiaId,
                docente_id = docenteId,
                anio_escolar = int.Parse(anio) // Convertir el año a entero
            };

            db.DocenteGradoMateria.Add(asignacion);
            db.SaveChanges();

            return Json(new { success = true, message = "Materia asignada correctamente." });
        }


        [HttpGet]
        public JsonResult GetDocentesPorMateria(string nombreMateria)
        {
            var docentes = db.vw_PerfilDocente
                .Where(d => d.especialidad == nombreMateria && d.estado == true)
                .Select(d => new
                {
                    d.docente_id,
                    nombre = d.nombre_docente,
                })
                .ToList();

            return Json(docentes, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ActualizarAsignacion(int gradoId, int materiaId, int docenteId)
        {
            var asignacion = db.DocenteGradoMateria.FirstOrDefault(x => x.grado_id == gradoId && x.materia_id == materiaId);

            if (asignacion == null)
            {
                return Json(new { success = false, message = "Asignación no encontrada." });
            }

            asignacion.docente_id = docenteId;
            db.SaveChanges();

            return Json(new { success = true, message = "Asignación actualizada correctamente." });
        }


        //alumnos logica

        [HttpGet]
        public JsonResult GetAlumnosAsignados(int gradoId)
        {
            var alumnos = db.GradoAlumno
                .Where(ga => ga.grado_id == gradoId)
                .Select(ga => new
                {
                    ga.grado_id,
                    ga.Alumno.id,
                    nombre = ga.Alumno.nombre,
                    carnet = ga.Alumno.carnet,
                    grado = ga.Grado.nombre + " " + ga.Grado.seccion
                })
                .ToList();

            return Json(alumnos, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetAlumnosDisponibles(int gradoId)
        {
            int anioActual = DateTime.Now.Year;

            // Obtener los IDs de los alumnos ya asignados a cualquier grado en el año actual
            var idsAsignados = db.GradoAlumno
                .Where(ga => ga.anio_escolar == anioActual)
                .Select(ga => ga.alumno_id)
                .Distinct()
                .ToList();

            // Traer solo los alumnos que no estén asignados en este año
            var disponibles = db.Alumno
                .Where(a => !idsAsignados.Contains(a.id))
                .Select(a => new
                {
                    a.id,
                    a.nombre
                })
                .ToList();

            return Json(disponibles, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult AsignarAlumno(int gradoId, int alumnoId)
        {
            var existe = db.GradoAlumno.Any(ga => ga.grado_id == gradoId && ga.alumno_id == alumnoId);
            if (existe)
            {
                return Json(new { success = false, message = "El alumno ya está asignado a este grado." });
            }
            string anio = DateTime.Now.Year.ToString(); // "2025"

            var asignacion = new GradoAlumno
            {
                grado_id = gradoId,
                alumno_id = alumnoId,
                anio_escolar = int.Parse(anio), // Convertir el año a entero
            };

            db.GradoAlumno.Add(asignacion);
            db.SaveChanges();

            return Json(new { success = true, message = "Alumno asignado correctamente." });
        }
        [HttpPost]
        public JsonResult EliminarAlumnoDelGrado(int alumnoId, int gradoId)
        {
            var anioActual = DateTime.Now.Year;

            var asignacion = db.GradoAlumno.FirstOrDefault(ga =>
                ga.alumno_id == alumnoId &&
                ga.grado_id == gradoId &&
                ga.anio_escolar == anioActual
            );

            if (asignacion == null)
            {
                return Json(new { success = false, message = "Asignación no encontrada." });
            }

            db.GradoAlumno.Remove(asignacion);
            db.SaveChanges();

            return Json(new { success = true, message = "Alumno eliminado del grado." });
        }


    }
}