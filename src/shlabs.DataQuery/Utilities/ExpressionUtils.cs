using System.Linq.Expressions;
using System.Reflection;

namespace shlabs.DataQuery.Abstractions.Utilities;

internal class ExpressionUtils
{
    public static PropertyInfo GetPropertyInfo(LambdaExpression lambda)
        {
            if (lambda.Body is MemberExpression memberExpr && memberExpr.Member is PropertyInfo propInfo)
            {
                return propInfo;
            }

            if (lambda.Body is UnaryExpression unaryExpr && unaryExpr.Operand is MemberExpression memberOperand &&
                memberOperand.Member is PropertyInfo propInfo2)
            {
                return propInfo2;
            }

            throw new ArgumentException("LambdaExpression is not a property access", nameof(lambda));
        }

        public static Expression BuildSelector(Expression parameter, Expression body)
        {
            if (body is MemberExpression memberExpr)
            {
                var inner = BuildSelector(parameter, memberExpr.Expression ?? parameter);
                return Expression.PropertyOrField(inner, memberExpr.Member.Name);
            }

            if (body is MethodCallExpression methodCall)
            {
                var args = new List<Expression>();
                var parentType = parameter.Type.GetGenericArguments().Any() ? parameter.Type.GetGenericArguments()[0] : parameter.Type;
                foreach (var arg in methodCall.Arguments)
                {
                    if (arg is LambdaExpression lambda)
                    {
                        // Create a new parameter for the lambda
                        var lambdaParam = lambda.Parameters[0];

                        parentType = lambdaParam.Type.GetGenericArguments().Any() ? lambdaParam.Type.GetGenericArguments()[0] : lambdaParam.Type;
                        
                        // Build the body using the lambda's parameter
                        var innerBody = BuildSelector(lambdaParam, lambda.Body);

                        // Create a new lambda with the original parameter and rebuilt body
                        var lambdaExpression = Expression.Lambda(innerBody, lambda.Parameters);
                        // var quoteExpression = Expression.Quote(lambdaExpression);
                        args.Add(lambdaExpression);
                    }
                    else
                    {
                        var innerBody = BuildSelector(parameter, arg);
                        args.Add(innerBody);
                    }
                }

                var methods = typeof(Enumerable).GetMethods()
                    .Where(m => m.Name == methodCall.Method.Name &&
                                m.GetParameters().Length == methodCall.Method.GetParameters().Length
                                && m.GetGenericArguments().Length == methodCall.Method.GetGenericArguments().Length);
        
                var method = methods
                    .First(x =>
                    {
                        if (x.IsGenericMethod)
                        {
                            x = x.MakeGenericMethod(methodCall.Method.GetGenericArguments());
                        }
                
                        var parameters = x.GetParameters();
                        var callerParameters = methodCall.Method.GetParameters();

                        for (int i = 0; i < parameters.Length; i++)
                        {
                            if (parameters[i].ParameterType != callerParameters[i].ParameterType 
                                && !callerParameters[i].ParameterType.IsAssignableFrom(parameters[i].ParameterType))
                            {
                                return false;
                            }
                        }

                        return true;
                    });

                method = method.MakeGenericMethod(methodCall.Method.GetGenericArguments());

                var instance = methodCall.Object != null ? BuildSelector(parameter, methodCall.Object) : null;
                return Expression.Call(instance, method, args);
            }

            if (body is ParameterExpression paramExpr)
            {
                // If this is the lambda parameter that matches our selector's parameter, return our parameter
                // Otherwise keep the original parameter (for nested lambdas)
                return paramExpr.Type == parameter.Type ? parameter : paramExpr;
            }

            if (body is UnaryExpression unary)
            {
                return Expression.MakeUnary(unary.NodeType, BuildSelector(parameter, unary.Operand), unary.Type);
            }

            if (body is ConstantExpression)
            {
                return body;
            }

            if (body is BinaryExpression binary)
            {
                var left = BuildSelector(parameter, binary.Left);
                var right = BuildSelector(parameter, binary.Right);
                return Expression.MakeBinary(binary.NodeType, left, right, binary.IsLiftedToNull, binary.Method);
            }

            throw new NotSupportedException($"Unsupported expression type: {body.GetType().Name}");
        }
}