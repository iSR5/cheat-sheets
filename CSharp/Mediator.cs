public interface IRequest<TResponse> {}

public interface IRequestHandler<TRequest, TResponse>
where TRequest : IRequest<TResponse>
{	
	Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public interface IMediator
{
	Task<TResponse> HandleAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken);
}

public sealed class Mediator : IMediator
{
    
	private readonly IServiceProvider _provider;
	
	public Mediator(IServiceProvider provider)
	{
		_provider = provider;
	}
	
	public Task<TResponse> HandleAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
	{
			var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
			var handler = _provider.GetRequiredService(handlerType);
			return handler.HandleAsync(request, cancellationToken);
	}
}

public static class MediatorExtensions
{
    private static readonly ConcurrentDictionary<Type, Type> _handlers = new ConcurrentDictionary<Type, Type>();

	public static IServiceCollection AddMediator(this IServiceCollection services, Assembly? assembly = null)
	=> AddMediator(this IServiceCollection services, Assembly.GetCallingAssembly());
	
	public static IServiceCollection AddMediator(this IServiceCollection services, Assembly assembly)
	{
		if(assembly is null)
			throw new ArgumentNullException(nameof(assembly));
		
		services.AddScoped<IMediator, Mediator>();
		
		var handlers = assembly.GetTypes()
		.Where(s=> !s.IsAbstract && !s.IsInterface)
		.SelectMany(type=> 
			type.GetInterfaces()
			.Where(i=> i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
			.Select(i=> new {Interface = i, Implementation = type})				
		);
		
		foreach(var handler in handlers)
		{
			services.AddScoped(handler.Interface, handler.Implementation);
		}
		
		return services;
	}
}
