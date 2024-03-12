using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Infrastructure.Services;

namespace ResourceProvisioner.Infrastructure;

public static class ConfigureServices
{
	public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddSingleton<IRepositoryService, RepositoryService>();
		services.AddHttpClient("InfrastructureHttpClient", client =>
		{
			// client.BaseAddress = new Uri(configuration["InfrastructureRepository:PullRequestUrl"]);

			var token =
				$"{configuration["InfrastructureRepository:Username"]}:{configuration["InfrastructureRepository:Password"]}";
			var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
			client.DefaultRequestHeaders.Add("Authorization", $"Basic {encodedToken}");
		});

		services.AddSingleton<ITerraformService, TerraformService>();

		return services;
	}
}