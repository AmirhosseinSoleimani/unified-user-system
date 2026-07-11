using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.Application.Services.Authentication;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.UnitTests.Architecture;

[Trait("Category", "Architecture")]
public sealed class CleanArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(User).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(AuthService).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(AppDbContext).Assembly;
    private static readonly Assembly ApiAssembly = typeof(Program).Assembly;

    [Fact]
    public void Domain_must_not_reference_outer_layers_or_frameworks()
    {
        var forbiddenReferences = new[]
        {
            "UnifiedUserSystem.Application",
            "UnifiedUserSystem.Infrastructure",
            "UnifiedUserSystem.Api",
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore",
            "StackExchange.Redis"
        };

        AssertAssemblyDoesNotReference(DomainAssembly, forbiddenReferences);
    }

    [Fact]
    public void Application_must_not_reference_infrastructure_api_or_host_frameworks()
    {
        var forbiddenReferences = new[]
        {
            "UnifiedUserSystem.Infrastructure",
            "UnifiedUserSystem.Api",
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore",
            "StackExchange.Redis"
        };

        AssertAssemblyDoesNotReference(ApplicationAssembly, forbiddenReferences);
    }

    [Fact]
    public void Infrastructure_must_not_reference_api()
    {
        AssertAssemblyDoesNotReference(
            InfrastructureAssembly,
            "UnifiedUserSystem.Api");
    }

    [Fact]
    public void Production_assemblies_must_not_contain_business_namespace()
    {
        var productionAssemblies = new[]
        {
            DomainAssembly,
            ApplicationAssembly,
            InfrastructureAssembly,
            ApiAssembly
        };

        var violatingTypes = productionAssemblies
            .SelectMany(GetLoadableTypes)
            .Where(type =>
                type.Namespace is not null &&
                (
                    type.Namespace.Equals(
                        "UnifiedUserSystem.src.Business",
                        StringComparison.Ordinal) ||
                    type.Namespace.Contains(
                        ".Business.",
                        StringComparison.Ordinal) ||
                    type.Namespace.EndsWith(
                        ".Business",
                        StringComparison.Ordinal)
                ))
            .Select(type => type.FullName)
            .ToArray();

        violatingTypes.Should().BeEmpty(
            "the Business layer was removed and must not be reintroduced");
    }

    [Fact]
    public void Production_assemblies_must_not_be_named_business()
    {
        var productionAssemblies = new[]
        {
            DomainAssembly,
            ApplicationAssembly,
            InfrastructureAssembly,
            ApiAssembly
        };

        var assemblyNames = productionAssemblies
        .Select(assembly => assembly.GetName().Name)
        .Where(name => name != null)
        .Cast<string>()
        .ToArray();

        assemblyNames.Should().NotContain(
        name => name.Contains(
            "Business",
            StringComparison.OrdinalIgnoreCase),
        "the removed Business assembly must not be reintroduced");
    }

    [Fact]
    public void Controllers_must_exist_only_in_api()
    {
        var nonApiAssemblies = new[]
        {
            DomainAssembly,
            ApplicationAssembly,
            InfrastructureAssembly
        };

        var controllersOutsideApi = nonApiAssemblies
            .SelectMany(GetLoadableTypes)
            .Where(type =>
                !type.IsAbstract &&
                typeof(ControllerBase).IsAssignableFrom(type))
            .Select(type => type.FullName)
            .ToArray();

        controllersOutsideApi.Should().BeEmpty(
            "ASP.NET controllers belong only to the API project");

        GetLoadableTypes(ApiAssembly)
            .Should()
            .Contain(type =>
                !type.IsAbstract &&
                typeof(ControllerBase).IsAssignableFrom(type));
    }

    [Fact]
    public void DbContext_must_exist_only_in_infrastructure()
    {
        var assembliesOutsideInfrastructure = new[]
        {
            DomainAssembly,
            ApplicationAssembly,
            ApiAssembly
        };

        var dbContextsOutsideInfrastructure = assembliesOutsideInfrastructure
            .SelectMany(GetLoadableTypes)
            .Where(type =>
                !type.IsAbstract &&
                typeof(DbContext).IsAssignableFrom(type))
            .Select(type => type.FullName)
            .ToArray();

        dbContextsOutsideInfrastructure.Should().BeEmpty(
            "EF Core persistence belongs only to Infrastructure");

        GetLoadableTypes(InfrastructureAssembly)
            .Should()
            .Contain(type =>
                !type.IsAbstract &&
                typeof(DbContext).IsAssignableFrom(type));
    }

    [Fact]
    public void Redis_client_reference_must_not_leak_into_domain_application_or_api()
    {
        var assembliesOutsideInfrastructure = new[]
        {
            DomainAssembly,
            ApplicationAssembly,
            ApiAssembly
        };

        foreach (var assembly in assembliesOutsideInfrastructure)
        {
            AssertAssemblyDoesNotReference(assembly, "StackExchange.Redis");
        }
    }

    private static void AssertAssemblyDoesNotReference(
        Assembly assembly,
        params string[] forbiddenAssemblyPrefixes)
    {
        var references = assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        foreach (var forbiddenPrefix in forbiddenAssemblyPrefixes)
        {
            references.Should().NotContain(
                reference =>
                    reference.Equals(
                        forbiddenPrefix,
                        StringComparison.OrdinalIgnoreCase) ||
                    reference.StartsWith(
                        $"{forbiddenPrefix}.",
                        StringComparison.OrdinalIgnoreCase),
                $"{assembly.GetName().Name} must not reference {forbiddenPrefix}");
        }
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type is not null)!;
        }
    }
}
