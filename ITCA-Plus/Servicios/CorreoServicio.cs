using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ITCA_Plus.Models;
using MailKit.Security;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace ITCA_Plus.Servicios
{
	public static class CorreoServicio
	{
        //rdwr gvxd zyjl tdpl
        private static string _Host = "smtp.gmail.com";
        private static int _Puerto = 587;
        private static string _Nombre = "ITCA Plus";
        private static string _Correo = "Luishi3e@gmail.com";
        private static string _Clave = "rdwr gvxd zyjl tdpl";

        public static bool Enviar(CorreoDTO correodto)
        {
            try
            {
                var mensaje = new MimeMessage();
                mensaje.From.Add(new MailboxAddress(_Nombre, _Correo));
                mensaje.To.Add(MailboxAddress.Parse(correodto.Para));
                mensaje.Subject = correodto.Asunto;
                mensaje.Body = new TextPart(TextFormat.Html)
                {
                    Text = correodto.Contenido
                };

                var smtp = new SmtpClient();
                smtp.Connect(_Host, _Puerto, SecureSocketOptions.StartTls);
                smtp.Authenticate(_Correo, _Clave);
                smtp.Send(mensaje);
                smtp.Disconnect(true);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}