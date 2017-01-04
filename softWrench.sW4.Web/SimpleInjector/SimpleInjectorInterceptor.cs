using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Castle.DynamicProxy;

namespace softWrench.sW4.Web.SimpleInjector {

    public static class InterceptorExtensions {
        private static readonly ProxyGenerator Generator = new ProxyGenerator();
        private static readonly Func<Type, object[], IInterceptor, object> CreateProxy =
            (type, arguments, interceptors) => Generator.CreateClassProxy(type, arguments, interceptors);

        /// <summary>
        /// Creates a castle proxy of the types that pass the predicate given that is intercepted by the TInterceptor type given.
        /// </summary>
        /// <typeparam name="TInterceptor">The type of the interceptor</typeparam>
        /// <param name="container"></param>
        /// <param name="predicate"></param>
        public static void InterceptWith<TInterceptor>(this Container container, Predicate<Type> predicate) where TInterceptor : class, IInterceptor {
            container.ExpressionBuilding += (s, e) => {
                var originalType = e.RegisteredServiceType;
                if (!predicate(originalType)) {
                    return;
                }

                var newExpression = e.Expression as NewExpression;
                if (newExpression == null) {
                    return;
                }

                var argumentsExpressionCollection = newExpression.Arguments;
                var arguments = new List<object>();
                if (argumentsExpressionCollection.Count > 0) {
                    argumentsExpressionCollection.ToList().ForEach(argExp => {
                        arguments.Add(((ConstantExpression)argExp).Value);
                    });
                }

                var interceptorExpression = container.GetRegistration(typeof(TInterceptor), true).BuildExpression();

                var createProxyExpression = Expression.Constant(CreateProxy);
                var constructorArgumentsExpression = Expression.Constant(arguments.ToArray());
                var originalTypeExpression = Expression.Constant(originalType, typeof(Type));
                var finalExpression = Expression.Invoke(createProxyExpression, originalTypeExpression, constructorArgumentsExpression, interceptorExpression);

                e.Expression = Expression.Convert(finalExpression, originalType);
            };
        }
    }
}