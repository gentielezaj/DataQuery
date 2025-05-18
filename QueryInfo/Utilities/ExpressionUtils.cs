using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QueryInfo.Utilities
{
    internal class ExpressionUtils
    {
        public static PropertyInfo GetPropertyInfo(LambdaExpression lambda)
        {
            if (lambda.Body is MemberExpression memberExpr && memberExpr.Member is PropertyInfo propInfo)
            {
                return propInfo;
            }
            if (lambda.Body is UnaryExpression unaryExpr && unaryExpr.Operand is MemberExpression memberOperand && memberOperand.Member is PropertyInfo propInfo2)
            {
                return propInfo2;
            }
            throw new ArgumentException("LambdaExpression is not a property access", nameof(lambda));
        }

        public static string GetPropertyPath(LambdaExpression expression)
        {
            var path = new Stack<string>();
            Expression? expr = expression.Body;

            while (expr is MemberExpression memberExpr)
            {
                path.Push(memberExpr.Member.Name);
                expr = memberExpr.Expression;
            }

            // Handle conversions (e.g., object casting)
            if (expr is UnaryExpression unaryExpr && unaryExpr.Operand is MemberExpression member)
            {
                path.Push(member.Member.Name);
                expr = member.Expression;
            }

            return string.Join(".", path);
        }
    }
}
