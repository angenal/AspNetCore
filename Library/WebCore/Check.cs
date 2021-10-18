using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WebCore.Annotations;

namespace WebCore
{
    [DebuggerStepThrough]
    public static class Check : object
    {
        public static IEnumerable<T> NotNullOrEmpty<T>(IEnumerable<T> value, string parameterName)
        {
            if (value == null || value.Any() == false)
                throw new ArgumentException(AbstractionsStrings.CollectionArgumentIsEmpty(parameterName));
            return value;
        }

        public static string NotNullOrEmpty(string value, [InvokerParameterName, NotNull] string parameterName)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException(AbstractionsStrings.ArgumentIsEmpty(parameterName));
            return value;
        }

        public static string NotNullOrWhiteSpace(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(AbstractionsStrings.ArgumentIsEmpty(parameterName));
            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static T NotNull<T>([NoEnumeration] T value, [InvokerParameterName, NotNull] string parameterName)
        {
            if (value == null)
            {
                NotEmpty(parameterName, "parameterName");
                throw new ArgumentNullException(parameterName);
            }
            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static IReadOnlyList<T> NotEmpty<T>(IReadOnlyList<T> value, [InvokerParameterName, NotNull] string parameterName)
        {
            NotNull(value, parameterName);
            if (value.Count == 0)
            {
                NotEmpty(parameterName, "parameterName");
                throw new ArgumentException(AbstractionsStrings.CollectionArgumentIsEmpty(parameterName));
            }
            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static string NotEmpty(string value, [InvokerParameterName, NotNull] string parameterName)
        {
            Exception ex = null;
            if (value == null)
            {
                ex = new ArgumentNullException(parameterName);
            }
            else if (value.Trim().Length == 0)
            {
                ex = new ArgumentException(AbstractionsStrings.ArgumentIsEmpty(parameterName));
            }
            if (ex != null)
            {
                NotEmpty(parameterName, "parameterName");
                throw ex;
            }
            return value;
        }

        public static string NullButNotEmpty(string value, [InvokerParameterName, NotNull] string parameterName)
        {
            if (value != null && value.Length == 0)
            {
                NotEmpty(parameterName, "parameterName");
                throw new ArgumentException(AbstractionsStrings.ArgumentIsEmpty(parameterName));
            }
            return value;
        }

        public static IReadOnlyList<T> HasNoNulls<T>(IReadOnlyList<T> value, [InvokerParameterName, NotNull] string parameterName) where T : class
        {
            NotNull(value, parameterName);
            if (value.Any((T e) => e == null))
            {
                NotEmpty(parameterName, "parameterName");
                throw new ArgumentException(parameterName);
            }
            return value;
        }
    }
}
