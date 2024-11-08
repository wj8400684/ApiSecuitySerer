using FluentValidation;
using MediatR;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using ApiSecuityServer.Model;

namespace ApiSecuityServer.Hub.Application.Behaviors;

internal sealed class ValidationPipelineBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : ApiResponse
{
    private readonly Type _responseType = typeof(TResponse);
    private readonly IValidator<TRequest>[] _validators = validators.ToArray();

    private static readonly ConcurrentDictionary<Type, Func<string, bool, int, TResponse>> InstanceActivators = new();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        if (_validators.Select(validator => validator.Validate(request)).Any(result => !result.IsValid))
            return CreateInstance(_responseType, "请求参数错误", false, 50);

        return await next();
    }

    private static TResponse CreateInstance(Type responseType, string errorMessage, bool successFul, int errorCode)
    {
        var activator = InstanceActivators.GetOrAdd(responseType, bodyType =>
        {
            var constructor = typeof(ApiResponse<>)
                .GetGenericTypeDefinition()
                .MakeGenericType(typeof(TResponse).GenericTypeArguments[0])
                .GetConstructors()
                .Single(x => x.GetParameters().Length == 3);

            var stringType = typeof(string);
            var boolType = typeof(bool);
            var intType = typeof(int);

            var errorMessageParameter = Expression.Parameter(stringType);
            var successFulParameter = Expression.Parameter(boolType);
            var errorCodeParameter = Expression.Parameter(intType);

            var expression = Expression.Lambda<Func<string, bool, int, TResponse>>
            (
                Expression.New(
                    constructor,
                    Expression.Convert(errorMessageParameter, stringType),
                    Expression.Convert(successFulParameter, boolType),
                    Expression.Convert(errorCodeParameter, intType)
                ),
                errorMessageParameter,
                successFulParameter,
                errorCodeParameter
            );

            return expression.Compile();
        });

        return activator(errorMessage, successFul, errorCode);
    }
}