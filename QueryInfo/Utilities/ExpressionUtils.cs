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
    }
}
