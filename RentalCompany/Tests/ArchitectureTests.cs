using System.Reflection;
using Core.Authentication;
using Data.Authentication;
using Xunit;

namespace Tests;

public sealed class ArchitectureTests
{
    [Fact]
    public void AppDoesNotReferenceDataOrDatabaseLibraries()
    {
        var names = typeof(Program).Assembly.GetReferencedAssemblies().Select(name => name.Name!);
        Assert.DoesNotContain(names, name => name == "Data" || name.StartsWith("Npgsql") || name.StartsWith("Microsoft.EntityFrameworkCore"));
        Assert.DoesNotContain(typeof(IIdentityGateway).Assembly.GetReferencedAssemblies(), name => name.Name is "Core" or "RentalCompany");
    }

    [Fact]
    public void CorePublicContractsDoNotExposeDataTypes()
    {
        var types = typeof(IAccountService).Assembly.GetExportedTypes();
        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
            var signatures = methods.SelectMany(method => method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType))
                .Concat(type.GetConstructors().SelectMany(ctor => ctor.GetParameters().Select(parameter => parameter.ParameterType)))
                .Concat(type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Select(field => field.FieldType));
            foreach (var signature in signatures.SelectMany(Flatten))
            {
                Assert.NotEqual("Data", signature.Assembly.GetName().Name);
                Assert.False(signature.Namespace?.StartsWith("Microsoft.EntityFrameworkCore") == true);
            }
        }
    }

    private static IEnumerable<Type> Flatten(Type type)
    {
        yield return type;
        if (type.HasElementType)
        {
            foreach (var nested in Flatten(type.GetElementType()!)) yield return nested;
        }
        foreach (var argument in type.GetGenericArguments())
        {
            foreach (var nested in Flatten(argument)) yield return nested;
        }
    }

}
