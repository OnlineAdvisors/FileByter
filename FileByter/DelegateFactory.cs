using System;
using System.Linq.Expressions;
using System.Reflection;

namespace FileByter
{
	internal delegate object LateBoundProperty(object target);

	internal static class DelegateFactory
	{
		public static LateBoundProperty Create(PropertyInfo property)
		{
			if (property == null)
				throw new ArgumentNullException("property");

			var method = property.GetGetMethod();

			return Create(method);
		}

		private static LateBoundProperty Create(MethodInfo method)
		{
			ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");

			MethodCallExpression call = Expression.Call(
				Expression.Convert(instanceParameter, method.DeclaringType),
				method);

			Expression<LateBoundProperty> lambda = Expression.Lambda<LateBoundProperty>(
				Expression.Convert(call, typeof(object)),
				instanceParameter);

			return lambda.Compile();
		}
	}
}