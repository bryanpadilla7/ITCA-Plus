using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Security.Cryptography;
using System.Text;	

namespace ITCA_Plus.Servicios
{
	public static class UtilidadServicio
	{
        public static string Encriptar(string texto)
        {
            string hash = string.Empty;

            using (SHA256 sha256 = SHA256.Create())
            {
                // Convertir el texto a bytes y calcular el hash    
                byte[] hashValue = sha256.ComputeHash(Encoding.UTF8.GetBytes(texto));
                // Convertir el array a una cadena de texto
                foreach (byte b in hashValue)
                {
                    hash += $"{b:X2}";
                }

                return hash;
            }
        }

        public static string GenerarToken()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}