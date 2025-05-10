using ITCA_Plus.Models;
using System;
using System.Collections.Generic;
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

        public ActionResult AgregarDocente()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetDocente()
        {
            var docentes = db.Usuarios
                 .Where(x => x.rol == "Docente")
                 .Select(x => new {
                     x.nombre,
                     x.tel,
                     x.usuario,
                     x.correo,
                     x.Token
                 })
                 .ToList();

            return Json(docentes, JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarDocente(Usuarios usuario, string especialidad, HttpPostedFileBase foto)
        {
            if (ModelState.IsValid)
            {
                if (foto != null && foto.ContentLength > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        foto.InputStream.CopyTo(ms);
                        usuario.fotografia = ms.ToArray();
                    }
                }

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

                return RedirectToAction("Index"); 
            }

            return View(usuario);
        }

    }
}