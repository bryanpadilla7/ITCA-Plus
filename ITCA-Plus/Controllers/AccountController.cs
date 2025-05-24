using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ITCA_Plus.Models;
using ITCA_Plus.Servicios;


namespace ITCA_Plus.Controllers
{
    public class AccountController : Controller
    {
        ITCAPlusEntities db = new ITCAPlusEntities();
        // GET: Account
        public ActionResult Login()
        {
            Session["cuenta"] = null;
            return View();
        }

        [HttpPost]
        public ActionResult Login(Usuarios user)
        {
            
            //Usuarios findUser = db.Usuarios.FirstOrDefault(u => u.correo == user.correo && u.contrasena == UtilidadServicio.Encriptar(user.contrasena));
            Usuarios findUser = db.Usuarios.FirstOrDefault(u => u.correo == user.correo && u.contrasena == user.contrasena);

            if (findUser == null)
            {
                ViewBag.Error = "Correo o contraseña incorrecta";
                return View("Login", user);
            }

            if (!(bool)findUser.confirmar)
            {
                ViewBag.mensaje = $"Falta comfirmar su cuenta. Se le envio un correo a {findUser.correo}";
                return View("Login", user);
            }
            else if ((bool)findUser.restablecer)
            {
                ViewBag.mensaje = $"Se ha solicitado restablecer su cuenta, favor revise su bandeja del correo {findUser.correo}";
                return View("Login", user);
            }

            Session["cuenta"] = findUser;
            Session["rolUsuario"] = findUser.rol;
            ViewBag.Error = null;

            if (findUser.rol == "Admin")
                return RedirectToAction("Index", "Admin");
            else if (findUser.rol == "Director")
                return RedirectToAction("Index", "Docente");
            else
                return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            Session["cuenta"] = null;
            return RedirectToAction("Login", "Account");
        }

        //pendiente
        public ActionResult Registrar(Usuarios user)
        {
            return View();
        }

        public ActionResult Confirmar(String correo)
        {
            Usuarios user = db.Usuarios.FirstOrDefault(u => u.correo == correo);
            if (user == null)
            {
                ViewBag.Respuesta = false;
                return View();
            }

            ViewBag.Respuesta = true;
            ViewBag.Nombre = user.nombre;
            user.confirmar = true;
            db.SaveChanges();
            return View();
        }

        public ActionResult ResEnvio()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResEnvio(Usuarios us)
        {
            Usuarios usuario = db.Usuarios.FirstOrDefault(u => u.correo == us.correo);
            if (usuario == null)
            {
                ViewBag.mensaje = "El correo no existe";
                return View();
            }

            string path = HttpContext.Server.MapPath("~/plantilla/Restablecer.html");
            string content = System.IO.File.ReadAllText(path);
            string url = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Headers["host"], "/Account/Restablecer?correo="+ usuario.correo);

            string htmlBody = string.Format(content, usuario.nombre, url);

            CorreoDTO correoDTO = new CorreoDTO()
            {
                Para = usuario.correo,
                Asunto = "Restablecer cuenta",
                Contenido = htmlBody
            };

            bool enviado = CorreoServicio.Enviar(correoDTO);
            usuario.restablecer = true;
            db.SaveChanges();
            ViewBag.mensaje = "Se ha enviado un correo a " + usuario.correo;
            return View();
        }

        
        public ActionResult Restablecer(string correo)
        {
            ViewBag.Correo = correo;
            return View();
        }

        

        [HttpPost]
        public ActionResult Restablecer(string correo, string nuevaContrasena, string confirmarContrasena)
        {

            if (nuevaContrasena != confirmarContrasena)
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                ViewBag.Correo = correo;
                return View();
            }

            var usuario = db.Usuarios.FirstOrDefault(u => u.correo == correo);
            if (usuario == null)
            {
                ViewBag.Error = "Usuario no encontrado.";
                return View();
            }

            usuario.contrasena = nuevaContrasena; // Aquí deberías encriptarla idealmente
            usuario.restablecer = false;
            db.SaveChanges();

            ViewBag.mensaje = "Contraseña restablecida con éxito.";
            return View();
        }
    }
}