using NUnit.Framework;
using System;
using System.Collections.Generic;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace BecaNet.Api.Tests
{
    // 1. EvaluadorBecaTests
    [TestFixture]
    public class EvaluadorBecaTests
    {
        private EvaluadorBeca evaluador = new EvaluadorBeca();

        [Test]
        public void Promedio70_NoEsElegible()
        {
            Assert.AreEqual(false, evaluador.EsElegible(70));
        }

        [Test]
        public void Promedio85_EsElegible()
        {
            Assert.AreEqual(true, evaluador.EsElegible(85));
        }

        [Test]
        public void PromedioInvalido_LanzaExcepcion()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => evaluador.EsElegible(-5));
        }
    }

    // 2. CalculadorCoberturaTests
    [TestFixture]
    public class CalculadorCoberturaTests
    {
        private CalculadorCobertura calculador = new CalculadorCobertura();

        [Test]
        public void IngresoAlto_DevuelveCero()
        {
            Assert.AreEqual(0, calculador.CalcularPorcentaje(12000, 10));
        }

        [Test]
        public void IngresoBajoVulnerabilidadAlta_DevuelveCien()
        {
            Assert.AreEqual(100, calculador.CalcularPorcentaje(2500, 9));
        }

        [Test]
        public void ParametrosInvalidos_LanzaExcepcion()
        {
            Assert.Throws<ArgumentException>(() => calculador.CalcularPorcentaje(-500, 5));
        }
    }

    // 3. ValidadorDocumentoTests
    [TestFixture]
    public class ValidadorDocumentoTests
    {
        private ValidadorDocumento validador = new ValidadorDocumento();

        [Test]
        public void PdfValido_DevuelveTrue()
        {
            Assert.AreEqual(true, validador.EsDocumentoValido("pdf", 2.5m));
        }

        [Test]
        public void TamanoExcedido_DevuelveFalse()
        {
            Assert.AreEqual(false, validador.EsDocumentoValido("pdf", 5.1m));
        }

        [Test]
        public void FormatoNoPermitido_DevuelveFalse()
        {
            Assert.AreEqual(false, validador.EsDocumentoValido("docx", 2.0m));
        }
    }

    // 4. AutenticadorTests
    [TestFixture]
    public class AutenticadorTests
    {
        private Autenticador autenticador = new Autenticador();

        [Test]
        public void CredencialesCorrectas_DevuelveTrue()
        {
            Assert.AreEqual(true, autenticador.ValidarCredenciales("estudiante@becanet.gt", "Beca2026!"));
        }

        [Test]
        public void CorreoInvalido_DevuelveFalse()
        {
            Assert.AreEqual(false, autenticador.ValidarCredenciales("estudiante.becanet.gt", "Beca2026!"));
        }

        [Test]
        public void ContrasenaCorta_DevuelveFalse()
        {
            Assert.AreEqual(false, autenticador.ValidarCredenciales("estudiante@becanet.gt", "12345"));
        }
    }

    // 5. EvaluadorGlobalTests
    [TestFixture]
    public class EvaluadorGlobalTests
    {
        private EvaluadorGlobal evaluador = new EvaluadorGlobal();

        [Test]
        public void PuntuacionPerfecta_DevuelveCien()
        {
            Assert.AreEqual(100.00m, evaluador.CalcularPuntuacion(100, 100, 100));
        }

        [Test]
        public void PuntuacionMixta_DevuelveCalculoCorrecto()
        {
            Assert.AreEqual(82.00m, evaluador.CalcularPuntuacion(80, 90, 70));
        }

        [Test]
        public void NotaInvalida_LanzaExcepcion()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => evaluador.CalcularPuntuacion(-10, 80, 90));
        }
    }

    // 6. ValidadorComiteTests
    [TestFixture]
    public class ValidadorComiteTests
    {
        private ValidadorComite validador = new ValidadorComite();

        [Test]
        public void ComiteTresIntegrantes_DevuelveTrue()
        {
            Assert.AreEqual(true, validador.EsComiteValido(new List<int> { 101, 102, 103 }));
        }

        [Test]
        public void ComiteCantidadPar_DevuelveFalse()
        {
            Assert.AreEqual(false, validador.EsComiteValido(new List<int> { 101, 102, 103, 104 }));
        }

        [Test]
        public void ComiteConDuplicados_DevuelveFalse()
        {
            Assert.AreEqual(false, validador.EsComiteValido(new List<int> { 101, 102, 101 }));
        }
    }

    // 7. ControlAccesosTests
    [TestFixture]
    public class ControlAccesosTests
    {
        private ControlAccesos control = new ControlAccesos();

        [Test]
        public void Exito_MantieneCuentaActiva()
        {
            Assert.AreEqual("Activa", control.ProcesarIntento(2, true));
        }

        [Test]
        public void TercerFallo_BloqueaCuenta()
        {
            Assert.AreEqual("Bloqueada", control.ProcesarIntento(2, false));
        }

        [Test]
        public void IntentoInvalido_LanzaExcepcion()
        {
            Assert.Throws<ArgumentException>(() => control.ProcesarIntento(-1, false));
        }
    }

    // 8. ValidadorProgramaTests
    [TestFixture]
    public class ValidadorProgramaTests
    {
        private ValidadorPrograma validador = new ValidadorPrograma();

        [Test]
        public void PregradoEdadCorrecta_DevuelveTrue()
        {
            Assert.AreEqual(true, validador.EsEdadValida(18, "PREGRADO"));
        }

        [Test]
        public void PregradoEdadExcedida_DevuelveFalse()
        {
            Assert.AreEqual(false, validador.EsEdadValida(26, "PREGRADO"));
        }

        [Test]
        public void PostgradoEdadValida_DevuelveTrue()
        {
            Assert.AreEqual(true, validador.EsEdadValida(45, "POSTGRADO"));
        }
    }

    // 9. FlujoSolicitudTests
    [TestFixture]
    public class FlujoSolicitudTests
    {
        private FlujoSolicitud flujo = new FlujoSolicitud();

        [Test]
        public void BorradorAEnviada_DevuelveTrue()
        {
            Assert.AreEqual(true, flujo.EsTransicionValida("BORRADOR", "ENVIADA"));
        }

        [Test]
        public void BorradorAAprobada_DevuelveFalse()
        {
            Assert.AreEqual(false, flujo.EsTransicionValida("BORRADOR", "APROBADA"));
        }

        [Test]
        public void EstadoFinalNoCambia_DevuelveFalse()
        {
            Assert.AreEqual(false, flujo.EsTransicionValida("APROBADA", "EN_EVALUACION"));
        }
    }

    // 10. GeneradorExpedienteTests
    [TestFixture]
    public class GeneradorExpedienteTests
    {
        private GeneradorExpediente generador = new GeneradorExpediente();

        [Test]
        public void CodigoCorrecto_DevuelveTrue()
        {
            Assert.AreEqual(true, generador.ValidarCodigo("BEC-2026-00125"));
        }

        [Test]
        public void PrefijoIncorrecto_DevuelveFalse()
        {
            Assert.AreEqual(false, generador.ValidarCodigo("SOL-2026-00125"));
        }

        [Test]
        public void FormatoIncompleto_DevuelveFalse()
        {
            Assert.AreEqual(false, generador.ValidarCodigo("BEC-2026-125"));
        }
    }
}