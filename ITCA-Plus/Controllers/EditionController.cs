using ITCA_Plus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ITCA_Plus.Controllers
{
    public class EditionController : Controller
    {
        ITCAPlusEntities contexto = new ITCAPlusEntities();
        // GET: Edition
        public ActionResult CambiosEdicion()
        {
            var lista = contexto.notiCambioNota
    .Where(x => !x.permiso && x.fechaCierre == null)
    .ToList();

            ViewBag.ListadoPeticionesEdicion = lista;
            return View();
        }
        [HttpPost]
        public ActionResult CambiosEdicion(int id)
        {
            var lista = contexto.notiCambioNota
    .Where(x => !x.permiso && x.fechaCierre == null)
    .ToList();

            ViewBag.ListadoPeticionesEdicion = lista;
            var solicitud = contexto.notiCambioNota.FirstOrDefault(x => x.id == id);

            if (solicitud != null)
            {
                solicitud.permiso = true;
                contexto.SaveChanges();
                HelperNotify.Notificar(this, "Has aprobado la solicitud", "success");
            }

            return RedirectToAction("CambiosEdicion");
            
        }
    }
}