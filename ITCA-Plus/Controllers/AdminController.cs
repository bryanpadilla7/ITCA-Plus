using ITCA_Plus.Models;
using ITCA_Plus.Servicios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ITCA_Plus.Controllers
{
    public class AdminController : Controller
    {
        ITCAPlusEntities db = new ITCAPlusEntities();
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetMaterias()
        {
            var materias = db.Materia
                .Select(m => new {
                    m.id,
                    m.nombre
                })
                .ToList();

            return Json(materias, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult GetDocente()
        {

            var docentes = db.vw_PerfilDocente.Where(x => x.estado == true).Select(x => new
            {
                x.docente_id,
                x.usuario_id,
                x.nombre_docente,
                x.telefono,
                x.carnet,
                x.correo,
                x.estado,
                x.especialidad
            }).ToList();

            return Json(docentes, JsonRequestBehavior.AllowGet);

        }

        public ActionResult AgregarDocente()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetDoc(string usuario)
        {
            var user = db.Usuarios.FirstOrDefault(u => u.usuario == usuario && u.rol == "Docente");

            if (usuario == null)
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            var docente = db.Docente.FirstOrDefault(d => d.usuario_id == user.id);

            return Json(new
            {
                success = true,
                usuario = new
                {
                    user.id,
                    user.nombre,
                    user.tel,
                    user.correo,
                    user.contrasena,
                    fotografia = user.fotografia != null ? Convert.ToBase64String(user.fotografia) : null
                },
                especialidad = docente?.especialidad
            }, JsonRequestBehavior.AllowGet);

        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarDocente(Usuarios usuario, string especialidad, HttpPostedFileBase foto)
        {

            // Valida si ya existe un usuario con ese correo
            bool existeCorreo = db.Usuarios.Any(x => x.correo == usuario.correo);

            if (existeCorreo)
            {
                return Json(new { success = false, message = "Este correo ya está registrado." });
            }

            // Validar si el archivo es una imagen válida (png, jpg, jpeg, gif)
            if (foto != null && foto.ContentLength > 0)
            {
                // Lista de tipos MIME válidos para imágenes
                var tiposValidos = new[] { "image/png", "image/jpeg", "image/jpg", "image/gif" };

                if (!tiposValidos.Contains(foto.ContentType.ToLower()))
                {
                    return Json(new { success = false, message = "El archivo debe ser una imagen (png, jpg, jpeg, gif)." });
                }

                using (var binaryReader = new BinaryReader(foto.InputStream))
                {
                    usuario.fotografia = binaryReader.ReadBytes(foto.ContentLength);
                }
            }

            // Obtener el año actual en formato corto (dos dígitos)
            string anioCorto = DateTime.Now.Year.ToString().Substring(2); // "25"

            // Obtener todos los tokens de usuarios con rol "Docente" que coincidan con el año actual
            var tokensDocentesAnio = db.Usuarios
                .Where(u => u.rol == "Docente" && u.usuario != null && u.usuario.EndsWith(anioCorto))
                .Select(u => u.usuario)
                .ToList();

            // El siguiente número de secuencia es el total + 1
            int numeroSecuencial = tokensDocentesAnio.Count + 1;
            string numeroFormateado = numeroSecuencial.ToString("D2"); // Formato 2 dígitos, ej: 01, 02, etc.

            // Obtener inicial del nombre
            char inicial = usuario.nombre.Trim()[0];

            // Generar el token (carnet)
            string carnet = $"D{inicial}{numeroFormateado}{anioCorto}";

            // Asignar token al usuario
            usuario.usuario = carnet;


            usuario.confirmar = false;
            usuario.restablecer = false;
            usuario.rol = "Docente";
            db.Usuarios.Add(usuario);
            db.SaveChanges();

            Docente docente = new Docente
            {
                usuario_id = usuario.id,
                especialidad = especialidad
            };

            db.Docente.Add(docente);
            db.SaveChanges();

            string path = HttpContext.Server.MapPath("~/plantilla/Confirmar.html");
            string content = System.IO.File.ReadAllText(path);
            string url = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Headers["host"], "/Account/Confirmar?correo=" + usuario.correo);

            string htmlBody = string.Format(content, usuario.nombre, url);

            CorreoDTO correoDTO = new CorreoDTO()
            {
                Para = usuario.correo,
                Asunto = "Confirmar cuenta",
                Contenido = htmlBody
            };

            bool enviado = CorreoServicio.Enviar(correoDTO);


            return Json(new { success = true });

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ActualizarDocente(Usuarios usuario, string especialidad, HttpPostedFileBase foto)
        {
            var usuarioBD = db.Usuarios.FirstOrDefault(x => x.id == usuario.id);

            if (usuarioBD == null)
            {
                return Json(new { success = false, message = "Docente no encontrado." });
            }

            // Actualizar datos
            usuarioBD.nombre = usuario.nombre;
            usuarioBD.tel = usuario.tel;
            usuarioBD.correo = usuario.correo;

            // Validar si el archivo es una imagen válida (png, jpg, jpeg, gif)
            if (foto != null && foto.ContentLength > 0)
            {
                // Lista de tipos MIME válidos para imágenes
                var tiposValidos = new[] { "image/png", "image/jpeg", "image/jpg", "image/gif" };

                if (!tiposValidos.Contains(foto.ContentType.ToLower()))
                {
                    return Json(new { success = false, message = "El archivo debe ser una imagen (png, jpg, jpeg, gif)." });
                }

                using (var binaryReader = new BinaryReader(foto.InputStream))
                {
                    usuario.fotografia = binaryReader.ReadBytes(foto.ContentLength);
                }
            }


            // Actualizar especialidad en tabla Docente
            var docenteBD = db.Docente.FirstOrDefault(d => d.usuario_id == usuario.id);
            if (docenteBD != null)
            {
                docenteBD.especialidad = especialidad;
            }

            db.SaveChanges();

            return Json(new { success = true, message = "Docente actualizado correctamente." });
        }

        [HttpPost]
        public ActionResult EliminarDocente(int id)
        {
            var usuarioBD = db.Usuarios.FirstOrDefault(x => x.id == id);

            if (usuarioBD == null)
            {
                return Json(new { success = false, message = "Docente no encontrado." });
            }

            usuarioBD.estado = false;        
            db.SaveChanges();

            return Json(new { success = true, message = "Docente desactivado correctamente." });
        }
        public ActionResult Materia()
        {
            return View();
        }

        [HttpGet]
        public ActionResult listaMaterias()
        {
            var materias = db.Materia.Where(x=> x.estado == true).Select(x => new
            {
                x.id,
                x.nombre
            }).ToList();

            return Json(new { success = true, materias }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult Crear(Materia materia)
        {
            if (string.IsNullOrWhiteSpace(materia.nombre))
                return Json(new { success = false, message = "El nombre es obligatorio." });

            db.Materia.Add(materia);
            db.SaveChanges();

            return Json(new { success = true, message = "Materia registrada correctamente." });
        }

        [HttpPost]
        public ActionResult Actualizar(Materia materia)
        {
            var materiaBD = db.Materia.Find(materia.id);
            if (materiaBD == null)
                return Json(new { success = false, message = "Materia no encontrada." });

            materiaBD.nombre = materia.nombre;
            db.SaveChanges();

            return Json(new { success = true, message = "Materia actualizada correctamente." });
        }

        [HttpPost]
        public JsonResult Eliminar(int id)
        {
            var materia = db.Materia.Find(id);
            if (materia == null)
                return Json(new { success = false, message = "Materia no encontrada." });

            materia.estado = false; // Cambia el estado a inactivo
            db.SaveChanges();

            return Json(new { success = true, message = "Materia eliminada correctamente." });
        }


    }
}