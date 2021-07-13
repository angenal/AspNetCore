using System;

namespace WebCore.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    internal sealed class ContractAnnotationAttribute : Attribute
    {
        public string Contract { get; }

        public bool ForceFullStates { get; }

        public ContractAnnotationAttribute([NotNull] string contract) : this(contract, false)
        {
        }

        public ContractAnnotationAttribute([NotNull] string contract, bool forceFullStates)
        {
            this.Contract = contract;
            this.ForceFullStates = forceFullStates;
        }
    }
}
