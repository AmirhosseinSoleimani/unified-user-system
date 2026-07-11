
using FluentAssertions;
using System.Xml.Linq;

namespace UnifiedUserSystem.UnitTests.Architecture;

[Trait("Category", "Architecture")]
public sealed class ProjectReferenceArchitectureTests
{
    [Fact]
    public void Production_project_references_must_follow_clean_architecture()
    {
        var root = FindRepositoryRoot();

        var domainProject = LoadProject(
            root,
            "src",
            "UnifiedUserSystem.Domain",
            "UnifiedUserSystem.Domain.csproj");

        var applicationProject = LoadProject(
            root,
            "src",
            "UnifiedUserSystem.Application",
            "UnifiedUserSystem.Application.csproj");

        var infrastructureProject = LoadProject(
            root,
            "src",
            "UnifiedUserSystem.Infrastructure",
            "UnifiedUserSystem.Infrastructure.csproj");

        var apiProject = LoadProject(
            root,
            "src",
            "UnifiedUserSystem.Api",
            "UnifiedUserSystem.Api.csproj");

        GetProjectReferences(domainProject).Should().BeEmpty(
            "Domain must not reference any outer production project");

        GetProjectReferences(applicationProject)
            .Should()
            .NotContain(reference =>
                ContainsProject(reference, "Infrastructure") ||
                ContainsProject(reference, "Api"));

        GetProjectReferences(infrastructureProject)
            .Should()
            .NotContain(reference =>
                ContainsProject(reference, "Api"));

        GetProjectReferences(apiProject)
            .Should()
            .Contain(reference =>
                ContainsProject(reference, "Application"));

        GetProjectReferences(apiProject)
            .Should()
            .Contain(reference =>
                ContainsProject(reference, "Infrastructure"));
    }

    [Fact]
    public void Domain_project_must_not_reference_persistence_web_or_redis_packages()
    {
        var root = FindRepositoryRoot();

        var domainProject = LoadProject(
            root,
            "src",
            "UnifiedUserSystem.Domain",
            "UnifiedUserSystem.Domain.csproj");

        var forbiddenPackages = new[]
        {
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore",
            "StackExchange.Redis",
            "Npgsql.EntityFrameworkCore.PostgreSQL"
        };

        GetPackageReferences(domainProject)
            .Should()
            .NotContain(package =>
                forbiddenPackages.Any(forbidden =>
                    package.StartsWith(
                        forbidden,
                        StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void Application_project_must_not_reference_ef_aspnet_or_redis_packages()
    {
        var root = FindRepositoryRoot();

        var applicationProject = LoadProject(
            root,
            "src",
            "UnifiedUserSystem.Application",
            "UnifiedUserSystem.Application.csproj");

        var forbiddenPackages = new[]
        {
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore",
            "StackExchange.Redis",
            "Npgsql.EntityFrameworkCore.PostgreSQL"
        };

        GetPackageReferences(applicationProject)
            .Should()
            .NotContain(package =>
                forbiddenPackages.Any(forbidden =>
                    package.StartsWith(
                        forbidden,
                        StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void Business_project_or_directory_must_not_exist()
    {
        var root = FindRepositoryRoot();

        Directory
            .EnumerateDirectories(root, "*", SearchOption.AllDirectories)
            .Where(path => !IsGeneratedPath(path))
            .Should()
            .NotContain(path =>
                Path.GetFileName(path)
                    .Equals("Business", StringComparison.OrdinalIgnoreCase));

        Directory
            .EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories)
            .Where(path => !IsGeneratedPath(path))
            .Should()
            .NotContain(path =>
                Path.GetFileNameWithoutExtension(path)
                    .Contains("Business", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Production_source_must_not_declare_business_namespace()
    {
        var root = FindRepositoryRoot();
        var sourceRoot = Path.Combine(root, "src");

        var violations = Directory
            .EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsGeneratedPath(path))
            .Where(path =>
            {
                var content = File.ReadAllText(path);

                return content.Contains(
                           "namespace UnifiedUserSystem.src.Business",
                           StringComparison.Ordinal) ||
                       content.Contains(
                           ".Business.",
                           StringComparison.Ordinal);
            })
            .ToArray();

        violations.Should().BeEmpty(
            "the removed Business layer must not return through namespaces or using directives");
    }

    private static XDocument LoadProject(
        string root,
        params string[] pathParts)
    {
        var path = pathParts.Aggregate(root, Path.Combine);

        File.Exists(path).Should().BeTrue(
            $"project file must exist at {path}");

        return XDocument.Load(path);
    }

    private static string[] GetProjectReferences(XDocument project)
        => project
            .Descendants("ProjectReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Cast<string>()
            .ToArray();

    private static string[] GetPackageReferences(XDocument project)
        => project
            .Descendants("PackageReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Cast<string>()
            .ToArray();

    private static bool ContainsProject(string reference, string name)
        => reference.Contains(
            $"UnifiedUserSystem.{name}",
            StringComparison.OrdinalIgnoreCase);

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "UnifiedUserSystem.sln")))
                return current.FullName;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not find UnifiedUserSystem.sln from the test output directory.");
    }

    private static bool IsGeneratedPath(string path)
        => path.Contains(
               $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
               StringComparison.OrdinalIgnoreCase) ||
           path.Contains(
               $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
               StringComparison.OrdinalIgnoreCase) ||
           path.Contains(
               $"{Path.DirectorySeparatorChar}.git{Path.DirectorySeparatorChar}",
               StringComparison.OrdinalIgnoreCase);
}