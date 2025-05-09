using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ITCA_Plus.Models
{
    public class HelperNotify
    {
        public HelperNotify() { }
        public static void Notificar(Controller controller, string mensaje, string tipo = "info")
        {
            controller.TempData["mensaje"] = mensaje;
            controller.TempData["tipo"] = tipo.ToLower(); // info, success, warning, error
        }
    }
}