using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application;
using SIGEVIP.Domain;

namespace SIGEVIP.Tests
{
    [TestClass]
    public class ArchitectureTests
    {
        [TestMethod]
        public void DomainYApplication_DebenEstarDisponiblesDesdeTests()
        {
            var domainMarker = new DomainAssemblyMarker();
            var applicationMarker = new ApplicationAssemblyMarker();

            Assert.IsNotNull(domainMarker);
            Assert.IsNotNull(applicationMarker);
        }
    }
}
