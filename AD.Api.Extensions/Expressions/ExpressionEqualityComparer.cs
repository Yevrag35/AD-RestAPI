using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Expressions
{
    internal class ExpressionEqualityComparer : IEqualityComparer<Expression>
    {
        public bool Equals(Expression? x, Expression? y)
        {
            if (x is null && y is null)
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return new ExpressionComparisonVisitor().AreEqual(x, y);
        }
    }
}
