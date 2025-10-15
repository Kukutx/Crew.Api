using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crew.Application.Abstractions;

public interface IModuleInstaller
{
    void Install(IServiceCollection services, IConfiguration configuration);
}
