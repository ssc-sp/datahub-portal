using System.Linq.Expressions;

namespace Datahub.ProjectTools;

public static class ResourcesTools
{
	public static string GetPropertyName<T>(Expression<Func<T, object>> expression)
	{
		return GetMemberName(expression.Body);
	}

	public static string GetMemberName(Expression expression)
	{
		switch (expression.NodeType)
		{
			case ExpressionType.MemberAccess:
				return ((MemberExpression)expression).Member.Name;
			case ExpressionType.Convert:
				return GetMemberName(((UnaryExpression)expression).Operand);
			default:
				throw new NotSupportedException(expression.NodeType.ToString());
		}
	}

}