//dotnet new console -o nume_folder
// De citit despre Singleton/Transient Lifetimes + de implementat cele 2 variante

using System;

public class Program
{
    public static void Main()
    {
        Console.WriteLine("\nis running...");
        DependencyContainer container = new DependencyContainer();
        //Add your dependencies 
        container.AddSingleton<CallService>();
        container.AddSingleton<MessageService>();
        container.AddTransient<AntenaService>();

        container.GetService<CallService>().ExecuteService();
        container.GetService<CallService>().ExecuteService();
        container.GetService<CallService>().ExecuteService();
        container.GetService<MessageService>().ExecuteService();
        container.GetService<MessageService>().ExecuteService();
        container.GetService<MessageService>().ExecuteService();
    }
}
// Dependencies to resolve:
// Client - - - > CallService - - - > AntenaService
// Client - - - > MessageService - - - > AntenaService
public class DependencyResolver
{
    
}

public enum DependencyLifetime {
    Transient = 0,
    Singleton = 1
}

public class DependencyContainer
{
    List<Dependency> _dependencies;
    public DependencyContainer()
    {
        _dependencies = new List<Dependency>();
    }
    public void AddSingleton<T>()
    {
        _dependencies.Add(new Dependency(typeof(T), DependencyLifetime.Singleton));
    }
    public void AddTransient<T>()
    {
        _dependencies.Add(new Dependency(typeof(T), DependencyLifetime.Transient));
    }
    public Dependency GetDependency(Type type)
    {
        return _dependencies.First(dependency => dependency.type == type);
    }
    public T GetService<T>()
    {
        return (T)GetService(typeof(T));
    }
    public object GetService(Type type)
    {
        var dependency = this.GetDependency(type);
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
        if (dependency.lifetime == DependencyLifetime.Singleton)
        {
            dependency.AddImplementation(implementation);
        }
        return implementation;
    }
}

public class Dependency
{
    public Dependency (Type t, DependencyLifetime l)
    {
        type = t;
        lifetime = l;
        IsImplemented = false;
    }
    public Type type{get;set;}
    public DependencyLifetime lifetime{get;set;}
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
    int _random;
    AntenaService _antenaNumber;
    public CallService(AntenaService antenaNumber)
    {
        _random = new Random().Next();
        _antenaNumber = antenaNumber;
    }
    public void ExecuteService()
    {
        Console.WriteLine($"Call some number ({_random})...\n   Connecting to antena {_antenaNumber.GetNearestAntena()} ({_antenaNumber._random})...");
    }
}

public class MessageService
{
    int _random;
    AntenaService _antenaNumber;
    public MessageService(AntenaService antenaNumber)
    {
        _random = new Random().Next();
        _antenaNumber = antenaNumber;
    }
    public void ExecuteService()
    {
        Console.WriteLine($"Message some number ({_random})...\n   Connecting to antena {_antenaNumber.GetNearestAntena()} ({_antenaNumber._random})...");
    }
}

public class AntenaService
{
    public int _random;
    public AntenaService()
    {
        _random = new Random().Next();
    }
    public int GetNearestAntena()
    {
        return new Random().Next(0, 100);
    }
}
