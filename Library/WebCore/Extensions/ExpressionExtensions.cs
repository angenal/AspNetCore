using System;
using System.Linq.Expressions;

namespace WebCore
{
    /// <summary>Provides methods to handle lambda expressions. </summary>
    public static class ExpressionExtensions
    {
        /// <summary>Applies to.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <param name="itemToFilter">The item to filter.</param>
        /// <returns></returns>
        public static bool ApplyTo<T>(this Predicate<T> predicate, T itemToFilter)
        {
            if (predicate == null) return true;

            foreach (var filterDelegate in predicate.GetInvocationList())
            {
                var filter = (Predicate<T>)filterDelegate;
                if (filter(itemToFilter) == false) return false;
            }

            return true;
        }

        /// <summary>Ands the specified additional predicate.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="existingPredicate">The existing predicate.</param>
        /// <param name="additionalPredicate">The additional predicate.</param>
        /// <returns></returns>
        public static Predicate<T> And<T>(this Predicate<T> existingPredicate, Predicate<T> additionalPredicate)
        {
            Check.NotNull(existingPredicate, nameof(existingPredicate));

            existingPredicate += additionalPredicate;
            return existingPredicate;
        }

        /// <summary>Returns the property name of the property specified in the given lambda (e.g. GetPropertyName(i => i.MyProperty)). </summary>
        /// <typeparam name="TClass">The type of the class with the property. </typeparam>
        /// <typeparam name="TProperty">The property type. </typeparam>
        /// <param name="expression">The lambda with the property. </param>
        /// <returns>The name of the property in the lambda. </returns>
        public static string GetPropertyName<TClass, TProperty>(this Expression<Func<TClass, TProperty>> expression)
        {
            if (expression.Body is UnaryExpression)
                return ((MemberExpression)(((UnaryExpression)expression.Body).Operand)).Member.Name;
            return ((MemberExpression)expression.Body).Member.Name;
        }

        /// <summary>Returns the property name of the property specified in the given lambda (e.g. GetPropertyName(i => i.MyProperty)). </summary>
        /// <typeparam name="TProperty">The property type. </typeparam>
        /// <param name="expression">The lambda with the property. </param>
        /// <returns>The name of the property in the lambda. </returns>
        public static string GetPropertyName<TProperty>(this Expression<Func<TProperty>> expression)
        {
            if (expression.Body is UnaryExpression)
                return ((MemberExpression)(((UnaryExpression)expression.Body).Operand)).Member.Name;
            return ((MemberExpression)expression.Body).Member.Name;
        }

        /// <summary>Returns the property name of the property specified in the given lambda (e.g. GetPropertyName(i => i.MyProperty)). </summary>
        /// <typeparam name="TClass">The type of the class with the property. </typeparam>
        /// <param name="expression">The lambda with the property. </param>
        /// <returns>The name of the property in the lambda. </returns>
        public static string GetPropertyName<TClass>(this Expression<Func<TClass, object>> expression)
        {
            if (expression.Body is UnaryExpression)
                return ((MemberExpression)(((UnaryExpression)expression.Body).Operand)).Member.Name;
            return ((MemberExpression)expression.Body).Member.Name;
        }

        /// <summary>Returns the event name of the event specified in the given lambda (e.g. GetEventName(i => i.MyEvent += null)). </summary>
        /// <typeparam name="TClass">The type of the class with the event. </typeparam>
        /// <param name="expression">The lambda with the event. </param>
        /// <returns>The name of the event in the lambda. </returns>
        public static string GetEventName<TClass>(this Expression<Action<TClass>> expression)
        {
            if (expression.Body is UnaryExpression)
                return ((MemberExpression)(((UnaryExpression)expression.Body).Operand)).Member.Name;
            return ((MemberExpression)expression.Body).Member.Name;
        }
    }
}
