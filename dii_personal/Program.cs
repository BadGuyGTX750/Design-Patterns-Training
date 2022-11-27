//dotnet new console -o nume_folder
// De citit despre Singleton/Transient Lifetimes + de implementat cele 2 variante

using System;

public class Program
{
    public static void Main()
    {
        Console.WriteLine("\nis running...");
        DependencyContainer container = new DependencyContainer();
        container.AddDependency<CallService>();
        container.AddDependency<MessageService>();
        container.AddDependency<AntenaService>();
        DependencyResolver builder = new DependencyResolver(container);
        builder.GetService<CallService>().ExecuteService();
        builder.GetService<MessageService>().ExecuteService();
        builder.GetService<MessageService>().ExecuteService();
    }
}
// Dependencies to resolve:
// Client - - - > CallService - - - > AntenaService
// Client - - - > MessageService - - - > AntenaService
public class DependencyResolver
{
    DependencyContainer _container;
    public DependencyResolver(DependencyContainer container)
    {
        _container = container;
    }
    public T GetService<T>()
    {
        return (T)GetService(typeof(T));
    }
    public object GetService(Type type)
    {
        var dependency = _container.GetDependency(type);
        var constructor = dependency.type.GetConstructors().Single();
        var parameters = constructor.GetParameters().ToArray();

        if (parameters.Length > 0)
        {
            var parameterImplementations = new object[parameters.Length];
            for(int i = 0; i < parameters.Length; i++)
            {
                parameterImplementations[i] = GetService(parameters[i].ParameterType);
            }
            return CreateImplementation(dependency, t => Activator.CreateInstance(t, parameterImplementations));
        }
        return CreateImplementation(dependency, t => Activator.CreateInstance(t));
    }
    public object CreateImplementation(Dependency dependency, Func<Type, object> factory)
    {
        if (dependency.IsImplemented)
        {
            return dependency.Implementation;
        }
        var implementation = factory(dependency.type);
        return implementation;
    }
}

public class DependencyContainer
{
    List<Dependency> _dependencies;
    public DependencyContainer()
    {
        _dependencies = new List<Dependency>();
    }
    public void AddDependency<T>()
    {
        _dependencies.Add(new Dependency(typeof(T)));
    }
    public Dependency GetDependency(Type type)
    {
        return _dependencies.First(dependency => dependency.type == type);
    }
}

public class Dependency
{
    public Dependency (Type t)
    {
        type = t;
        IsImplemented = false;
    }
    public Type type{get;set;}
    public object Implementation{get;set;}
    public bool IsImplemented{get;set;}
    public void AddImplementation (object i)
    {
        Implementation = i;
        IsImplemented = true;
    }
}

public class CallService
{
    AntenaService _antenaNumber;
    public CallService(AntenaService antenaNumber)
    {
        _antenaNumber = antenaNumber;
    }
    public void ExecuteService()
    {
        Console.WriteLine($"Call some number...\n   Connecting to antena {_antenaNumber.GetNearestAntena()}...");
    }
}

public class MessageService
{
    AntenaService _antenaNumber;
    public MessageService(AntenaService antenaNumber)
    {
        _antenaNumber = antenaNumber;
    }
    public void ExecuteService()
    {
        Console.WriteLine($"Message some number...\n   Connecting to antena {_antenaNumber.GetNearestAntena()}...");
    }
}

public class AntenaService
{
    public int GetNearestAntena()
    {
        return new Random().Next(0, 100);
    }
}
