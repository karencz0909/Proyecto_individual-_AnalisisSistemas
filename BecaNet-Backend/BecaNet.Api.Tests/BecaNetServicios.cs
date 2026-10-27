using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BecaNet.Api.Tests
{
    
    public class EvaluadorBeca
    {
        public bool EsElegible(decimal promedio)
        {
            if (promedio < 0 || promedio > 100)
                throw new ArgumentOutOfRangeException();
            return promedio >= 85;
        }
    }

    public class CalculadorCobertura
    {
        public decimal CalcularPorcentaje(decimal ingreso, int vulnerabilidad)
        {
            if (ingreso < 0 || vulnerabilidad < 1 || vulnerabilidad > 10)
                throw new ArgumentException("Parámetros fuera de rango");
            if (ingreso > 10000) return 0;
            if (ingreso <= 3000 && vulnerabilidad >= 8) return 100;
            return 50;
        }
    }

    public class ValidadorDocumento
    {
        public bool EsDocumentoValido(string extension, decimal tamanoMB)
        {
            if (tamanoMB <= 0 || tamanoMB > 5.0m) return false;
            string ext = extension.ToLower().Replace(".", "");
            return ext == "pdf" || ext == "jpg" || ext == "png";
        }
    }

    public class Autenticador
    {
        public bool ValidarCredenciales(string correo, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
                return false;
            bool correoValido = correo.Contains("@") && correo.Contains(".");
            bool contrasenaValida = contrasena.Length >= 8;
            return correoValido && contrasenaValida;
        }
    }

    public class EvaluadorGlobal
    {
        public decimal CalcularPuntuacion(decimal academica, decimal socioeconomica, decimal entrevista)
        {
            if (academica < 0 || academica > 100 ||
                socioeconomica < 0 || socioeconomica > 100 ||
                entrevista < 0 || entrevista > 100)
                throw new ArgumentOutOfRangeException();
            return (academica * 0.40m) + (socioeconomica * 0.40m) + (entrevista * 0.20m);
        }
    }

    public class ValidadorComite
    {
        public bool EsComiteValido(List<int> idsEvaluadores)
        {
            if (idsEvaluadores == null || idsEvaluadores.Count < 3) return false;
            bool esImpar = idsEvaluadores.Count % 2 != 0;
            bool tieneDuplicados = idsEvaluadores.Count != idsEvaluadores.Distinct().Count();
            return esImpar && !tieneDuplicados;
        }
    }

    public class ControlAccesos
    {
        public string ProcesarIntento(int intentosPrevios, bool esExito)
        {
            if (intentosPrevios < 0) throw new ArgumentException("Intentos inválidos");
            if (esExito) return "Activa";
            int nuevosIntentos = intentosPrevios + 1;
            return nuevosIntentos >= 3 ? "Bloqueada" : "Activa";
        }
    }

    public class ValidadorPrograma
    {
        public bool EsEdadValida(int edad, string programa)
        {
            if (edad < 15 || edad > 99) return false;
            switch (programa.ToUpper())
            {
                case "PREGRADO": return edad >= 15 && edad <= 25;
                case "TECNICA": return edad >= 15 && edad <= 35;
                case "POSTGRADO": return edad >= 20 && edad <= 99;
                default: return false;
            }
        }
    }

    public class FlujoSolicitud
    {
        public bool EsTransicionValida(string estadoActual, string nuevoEstado)
        {
            switch (estadoActual.ToUpper())
            {
                case "BORRADOR": return nuevoEstado.ToUpper() == "ENVIADA";
                case "ENVIADA": return nuevoEstado.ToUpper() == "EN_EVALUACION" || nuevoEstado.ToUpper() == "CANCELADA";
                case "EN_EVALUACION": return nuevoEstado.ToUpper() == "APROBADA" || nuevoEstado.ToUpper() == "RECHAZADA";
                default: return false;
            }
        }
    }

    public class GeneradorExpediente
    {
        public bool ValidarCodigo(string codigo)
        {
        if (string.IsNullOrWhiteSpace(codigo)) return false;
        Regex regex = new Regex(@"^BEC-\d{4}-\d{5}$");
        return regex.IsMatch(codigo);
        }
    }
}