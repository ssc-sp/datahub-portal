using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Datahub.SpecflowTests.Utils;

public static class TestServiceProviderMockExtension
{
        public static void AddStub<T>(this TestServiceProvider services) where T : class
        {
            var mock = Substitute.For<T>();
            services.AddSingleton(mock);
        }
}