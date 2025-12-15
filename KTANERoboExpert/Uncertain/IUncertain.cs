using System.Diagnostics.CodeAnalysis;

namespace KTANERoboExpert.Uncertain
{
    /// <summary>
    /// Represents a value dependent on edgework that may or may not be known.
    /// </summary>
    public interface IUncertain<T>
    {
        /// <summary>
        /// <see langword="true"/> if and only if the value is certainly known.
        /// </summary>
        [MemberNotNullWhen(true, nameof(Value))]
        public bool IsCertain { get; }
        /// <summary>
        /// The value, if known.
        /// </summary>
        public T? Value { get; }
        /// <summary>
        /// The method to query the user for the value, if unknown.
        /// </summary>
        public void Fill();
    }
}
