namespace KTANERoboExpert.Uncertain
{
    /// <summary>Represents a value dependent on edgework that may or may not be known.</summary>
    public interface IUncertain
    {
        /// <summary><see langword="true"/> if and only if the value is certainly known.</summary>
        public bool IsCertain { get; }
        /// <summary>The method to query the user for the value, if unknown.</summary>
        public void Fill(Action onFill, Action? onCancel = null);
    }

    /// <inheritdoc cref="IUncertain"/>
    public interface IUncertain<T> : IUncertain
    {
        /// <summary>The value, if known.</summary>
        public T? Value { get; }
    }
}
