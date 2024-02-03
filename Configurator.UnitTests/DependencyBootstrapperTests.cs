using System;
using Configurator.Utilities;
using Configurator.Windows;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

namespace Configurator.UnitTests
{
    public class DependencyBootstrapperTests : UnitTestBase<DependencyBootstrapper>
    {
        [Fact]
        public void When_initializing_service_provider_with_no_args()
        {
            var serviceProvider = Because(() => ClassUnderTest.InitializeServiceProvider());

            It("configures dependencies", () =>
            {
                GetMock<IServiceCollection>().Verify(x => x.Add(Moq.It.Is<ServiceDescriptor>(y => y.ServiceType == typeof(ITokenizer))), Times.Once);
                serviceProvider.ShouldNotBeNull();
            });
        }

        [Fact]
        public void When_initializing_static_dependencies()
        {
            var expectedTokenizer = GetMock<ITokenizer>().Object;

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(ITokenizer))).Returns(expectedTokenizer);

            Because(() => ClassUnderTest.InitializeStaticDependencies(serviceProviderMock.Object));

            It("configures them", () =>
            {
                RegistrySettingValueDataConverter.Tokenizer.ShouldNotBeNull().ShouldBe(expectedTokenizer);
            });
        }
    }
}
