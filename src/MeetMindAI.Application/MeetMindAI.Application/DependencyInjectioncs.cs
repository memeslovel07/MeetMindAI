using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation;

using MediatR;

using MeetMindAI.Application.Common.Behaviors;

using Microsoft.Extensions.DependencyInjection;

namespace MeetMindAI.Application;

/// <summary>
/// Registers application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers application services.
    /// </summary>
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));



        });

        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly);



        return services;
    }
}
