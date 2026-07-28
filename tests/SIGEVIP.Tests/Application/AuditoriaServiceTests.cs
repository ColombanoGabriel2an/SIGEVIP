using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Application
{
    [TestClass]
    public class AuditoriaServiceTests
    {
        [TestMethod]
        public void Listar_SinSesion_RechazaOperacion()
        {
            AuditoriaService servicio =
                CrearServicio(
                    new AuditoriaRepositoryFalso(),
                    null);

            Assert.ThrowsException
                <AccesoDenegadoException>(
                    () => servicio.Listar(
                        null));
        }

        [TestMethod]
        public void Listar_UsuarioInactivo_RechazaOperacion()
        {
            Usuario usuario =
                CrearUsuarioAutorizado();

            AuditoriaService servicio =
                CrearServicio(
                    new AuditoriaRepositoryFalso(),
                    usuario);

            usuario.Desactivar();

            Assert.ThrowsException
                <AccesoDenegadoException>(
                    () => servicio.Listar(
                        null));
        }

        [TestMethod]
        public void Listar_SinPermiso_RechazaOperacion()
        {
            AuditoriaService servicio =
                CrearServicio(
                    new AuditoriaRepositoryFalso(),
                    CrearUsuarioSinPermiso());

            Assert.ThrowsException
                <AccesoDenegadoException>(
                    () => servicio.Listar(
                        null));
        }

        [TestMethod]
        public void Listar_FechasInvertidas_RechazaOperacion()
        {
            AuditoriaService servicio =
                CrearServicio(
                    new AuditoriaRepositoryFalso(),
                    CrearUsuarioAutorizado());

            AuditoriaFiltro filtro =
                new AuditoriaFiltro(
                    new DateTime(
                        2026,
                        7,
                        20),
                    new DateTime(
                        2026,
                        7,
                        10),
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty);

            Assert.ThrowsException
                <ReglaNegocioException>(
                    () => servicio.Listar(
                        filtro));
        }

        [TestMethod]
        public void Listar_ConPermiso_DevuelveResultados()
        {
            AuditoriaRepositoryFalso repository =
                new AuditoriaRepositoryFalso();

            repository.Resultados.Add(
                CrearDto());

            AuditoriaService servicio =
                CrearServicio(
                    repository,
                    CrearUsuarioAutorizado());

            IReadOnlyCollection
                <AuditoriaListadoDto> resultado =
                    servicio.Listar(
                        null);

            Assert.AreEqual(
                1,
                resultado.Count);

            Assert.IsNotNull(
                repository.UltimoFiltro);
        }

        [TestMethod]
        public void Listar_TransfiereFiltroAlRepositorio()
        {
            AuditoriaRepositoryFalso repository =
                new AuditoriaRepositoryFalso();

            AuditoriaFiltro filtro =
                new AuditoriaFiltro(
                    new DateTime(
                        2026,
                        7,
                        1),
                    new DateTime(
                        2026,
                        7,
                        31),
                    "administrador",
                    "Clientes",
                    "Alta",
                    "empresa");

            AuditoriaService servicio =
                CrearServicio(
                    repository,
                    CrearUsuarioAutorizado());

            servicio.Listar(
                filtro);

            Assert.AreSame(
                filtro,
                repository.UltimoFiltro);
        }

        private static AuditoriaService CrearServicio(
            AuditoriaRepositoryFalso repository,
            Usuario usuario)
        {
            SesionActual sesion =
                new SesionActual();

            if (usuario != null)
            {
                sesion.Iniciar(
                    usuario);
            }

            return new AuditoriaService(
                repository,
                sesion,
                new AutorizacionService());
        }

        private static Usuario CrearUsuarioAutorizado()
        {
            Usuario usuario =
                CrearUsuarioSinPermiso();

            Grupo grupo =
                new Grupo(
                    1,
                    "AUDITORES",
                    "Auditores",
                    "Grupo de prueba.");

            grupo.AgregarComponente(
                new Permiso(
                    1,
                    AuditoriaService
                        .PermisoConsultar,
                    "Consultar auditoría",
                    "Permite consultar auditoría."));

            usuario.AgregarGrupo(
                grupo);

            return usuario;
        }

        private static Usuario CrearUsuarioSinPermiso()
        {
            return new Usuario(
                1,
                1,
                "auditor",
                new byte[32],
                new byte[32],
                1000);
        }

        private static AuditoriaListadoDto CrearDto()
        {
            return new AuditoriaListadoDto(
                1L,
                new DateTime(
                    2026,
                    7,
                    20,
                    10,
                    30,
                    0),
                1,
                "administrador",
                "Clientes",
                "Alta",
                "Cliente",
                10,
                "Se registró el cliente.");
        }

        private sealed class AuditoriaRepositoryFalso
            : IAuditoriaRepository
        {
            public AuditoriaRepositoryFalso()
            {
                Resultados =
                    new List
                        <AuditoriaListadoDto>();
            }

            public List<AuditoriaListadoDto>
                Resultados
            {
                get;
                private set;
            }

            public AuditoriaFiltro UltimoFiltro
            {
                get;
                private set;
            }

            public IReadOnlyCollection
                <AuditoriaListadoDto> Listar(
                    AuditoriaFiltro filtro)
            {
                UltimoFiltro =
                    filtro;

                return Resultados
                    .AsReadOnly();
            }
        }
    }
}