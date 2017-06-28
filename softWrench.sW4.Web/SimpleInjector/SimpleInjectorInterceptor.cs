using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Castle.DynamicProxy;
using NHibernate.Util;

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
                if (newExpression != null) {
                    e.Expression = ProxyNewExpression<TInterceptor>(newExpression, originalType, container);
                    return;
                }

                var invocExpression = e.Expression as InvocationExpression;
                if (invocExpression == null) {
                    // a type of expression of another kind was found, no interception made, possibly causing bugs
                    // this ex is added to know before hand on development
                    throw new Exception("Component creation expression type not handled.");
                }

                var proxied = false;

                var invocMainExpression = invocExpression.Expression;
                newExpression = invocMainExpression as NewExpression;
                if (newExpression != null) {
                    proxied = true;
                    invocMainExpression = ProxyNewExpression<TInterceptor>(newExpression, originalType, container);
                }

                var proxiedList = new List<Expression>();
                invocExpression.Arguments.ForEach(expression => {
                    newExpression = expression as NewExpression;
                    if (newExpression == null ||
                        originalType != expression.Type) {
                        proxiedList.Add(expression);
                    } else {
                        proxied = true;
                        proxiedList.Add(ProxyNewExpression<TInterceptor>(newExpression, originalType, container));
                    }
                });

                if (proxied) {
                    e.Expression = Expression.Invoke(invocMainExpression, proxiedList);
                } else {
                    // the new expression was not found on invoke expression, possibly is nested
                    // TODO change code to search for new expression on deeper levels
                    // this ex is added to know before hand on development
                    throw new Exception("Component creation expression not found on invoke expression");
                }
            };
        }

        private static Expression ProxyNewExpression<TInterceptor>(NewExpression newExpression, Type originalType, Container container) where TInterceptor : class, IInterceptor {
            var interceptorExpression = container.GetRegistration(typeof(TInterceptor), true).BuildExpression();
            var createProxyExpression = Expression.Constant(CreateProxy);
            var constructorArgumentsExpression = Expression.NewArrayInit(typeof(object), newExpression.Arguments.ToArray());
            var originalTypeExpression = Expression.Constant(originalType, typeof(Type));
            var finalExpression = Expression.Invoke(createProxyExpression, originalTypeExpression, constructorArgumentsExpression, interceptorExpression);
            return Expression.Convert(finalExpression, originalType);
        }
    }
}