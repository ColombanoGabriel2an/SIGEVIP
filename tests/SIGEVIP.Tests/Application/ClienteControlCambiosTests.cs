using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Clientes;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Tests.Application
{
    [TestClass]
    public class ClienteControlCambiosTests
    {
        [TestMethod]
        public void Modificar_RegistraSolamenteValoresModificados()
        {
            ClienteRepositoryFalso repository =
                new ClienteRepositoryFalso();

            repository.ClienteObtenido =
                new Cliente(
                    1,
                    "Empresa original",
                    "30-12345678-9",
                    "anterior@correo.com",
                    "3415550000",
                    "Rosario",
                    "Santa Fe");

            ClienteService servicio =
                CrearServicio(repository);

            servicio.Modificar(
                1,
                "Empresa modificada",
                "30-12345678-9",
                "nuevo@correo.com",
                "3415550000",
                "Funes",
                "Santa Fe");

            AuditoriaRegistro auditoria =
                repository.AuditoriaActualizacion;

            Assert.IsNotNull(auditoria);
            Assert.AreEqual("Modificacion", auditoria.Accion);
            Assert.AreEqual(1, auditoria.IdEntidad);
            Assert.AreEqual(3, auditoria.Cambios.Count);

            AuditoriaCambioRegistro razonSocial =
                auditoria.Cambios.Single(
                    actual =>
                        actual.Campo ==
                        "RazonSocial");

            Assert.AreEqual(
                "Empresa original",
                razonSocial.ValorAnterior);

            Assert.AreEqual(
                "Empresa modificada",
                razonSocial.ValorNuevo);

            AuditoriaCambioRegistro email =
                auditoria.Cambios.Single(
                    actual =>
                        actual.Campo ==
                        "Email");

            Assert.AreEqual(
                "anterior@correo.com",
                email.ValorAnterior);

            Assert.AreEqual(
                "nuevo@correo.com",
                email.ValorNuevo);

            AuditoriaCambioRegistro localidad =
                auditoria.Cambios.Single(
                    actual =>
                        actual.Campo ==
                        "Localidad");

            Assert.AreEqual(
                "Rosario",
                localidad.ValorAnterior);

            Assert.AreEqual(
                "Funes",
                localidad.ValorNuevo);

            Assert.IsFalse(
                auditoria.Cambios.Any(
                    actual =>
                        actual.Campo ==
                        "Cuit"));

            Assert.IsFalse(
                auditoria.Cambios.Any(
                    actual =>
                        actual.Campo ==
                        "Telefono"));

            Assert.IsFalse(
                auditoria.Cambios.Any(
                    actual =>
                        actual.Campo ==
                        "Provincia"));
        }

        private static ClienteService CrearServicio(
            ClienteRepositoryFalso repository)
        {
            Usuario usuario =
                new Usuario(
                    1,
                    1,
                    "auditor.cliente",
                    new byte[32],
                    new byte[32],
                    1000);

            Grupo grupo =
                new Grupo(
                    1,
                    "GESTORES_CLIENTE",
                    "Gestores de Cliente",
                    "Grupo para la prueba.");

            grupo.AgregarComponente(
                new Permiso(
                    1,
                    ClienteService.PermisoGestionar,
                    "Gestionar Cliente",
                    "Permite gestionar Clientes."));

            usuario.AgregarGrupo(grupo);

            SesionActual sesion =
                new SesionActual();

            sesion.Iniciar(usuario);

            return new ClienteService(
                repository,
                sesion,
                new AutorizacionService());
        }

        private sealed class ClienteRepositoryFalso
            : IClienteRepository
        {
            public Cliente ClienteObtenido
            {
                get;
                set;
            }

            public AuditoriaRegistro AuditoriaActualizacion
            {
                get;
                private set;
            }

            public Cliente ObtenerPorId(
                int idCliente)
            {
                return ClienteObtenido;
            }

            public bool ExisteCuit(
                string cuit,
                int? idClienteExcluido)
            {
                return false;
            }

            public IReadOnlyCollection<ClienteListadoDto>
                Listar(
                    ClienteFiltro filtro)
            {
                return new List
                    <ClienteListadoDto>()
                    .AsReadOnly();
            }

            public int Insertar(
                Cliente cliente,
                AuditoriaRegistro auditoria)
            {
                return 1;
            }

            public void Actualizar(
                Cliente cliente,
                AuditoriaRegistro auditoria)
            {
                ClienteObtenido = cliente;
                AuditoriaActualizacion = auditoria;
            }

            public void Activar(
                int idCliente,
                AuditoriaRegistro auditoria)
            {
            }

            public void Desactivar(
                int idCliente,
                AuditoriaRegistro auditoria)
            {
            }
        }
    }
}
