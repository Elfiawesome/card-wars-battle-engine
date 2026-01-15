using System.Reflection;

namespace CardWars.Core.Common.Dispatching;

// Might change this for a .py script to create the auto registration
// Cuz i don't like dynamically loading .-.
public static class RequestRegister
{
	public static void RegisterAssembly<TAttribute, TRequestDispatcher>(Assembly assembly, TRequestDispatcher dispatcher)
		where TAttribute : Attribute
		where TRequestDispatcher : RequestHandlerBase
	{
		var registerMethodDefinition = dispatcher.GetType()
			.GetMethods(BindingFlags.Instance | BindingFlags.Public)
			.FirstOrDefault(m => m.Name == "RegisterHandler" && m.IsGenericMethodDefinition);

		if (registerMethodDefinition == null)
		{
			throw new InvalidOperationException($"Could not find a generic 'RegisterHandler' method on type {dispatcher.GetType().Name}.");
		}
		
		var expectedInterfaceDefinition = registerMethodDefinition.GetParameters().First().ParameterType.GetGenericTypeDefinition();

		var handlerTypes = assembly.GetTypes()
			.Where(t => t.GetCustomAttribute<TAttribute>() != null && t.IsClass && !t.IsAbstract);

		foreach (var handlerType in handlerTypes)
		{
			var interfaceType = handlerType.GetInterfaces()
				.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == expectedInterfaceDefinition);

			if (interfaceType == null) continue;

			var requestType = interfaceType.GetGenericArguments()
				.FirstOrDefault(arg => typeof(IRequest).IsAssignableFrom(arg));

			if (requestType == null)
			{
				throw new InvalidOperationException($"Handler {handlerType.Name} implements the correct interface but no generic argument implements IRequest.");
			}

			try
			{
				var handlerInstance = Activator.CreateInstance(handlerType);

				var genericRegisterMethod = registerMethodDefinition.MakeGenericMethod(requestType);

				genericRegisterMethod.Invoke(dispatcher, [handlerInstance]);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException ?? ex;
			}
		}
	}
}