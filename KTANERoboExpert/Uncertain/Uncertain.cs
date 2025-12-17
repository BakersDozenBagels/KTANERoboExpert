using System.Diagnostics.CodeAnalysis;

namespace KTANERoboExpert.Uncertain
{
    /// <inheritdoc cref="IUncertain{T}"/>
    public class Uncertain<T> : IUncertain<T>
    {
        private readonly Maybe<T> _value;
        protected readonly Maybe<Action<Action, Action?>> _getValue;

        /// <summary>
        /// A definitely known value.
        /// </summary>
        protected Uncertain(T value)
        {
            _value = new(value);
            _getValue = new();
        }
        /// <summary>
        /// A definitely unknown value.
        /// </summary>
        protected Uncertain(Action<Action, Action?> getValue)
        {
            _value = new();
            _getValue = new(getValue);
        }

        public static Uncertain<T> Of(T value) => new(value);
        public static Uncertain<T> Of(Action<Action, Action?> getValue) => new(getValue);

        /// <inheritdoc/>
        [MemberNotNullWhen(true, nameof(Value))]
        public bool IsCertain => _value.Exists;

        /// <inheritdoc/>
        public T? Value => _value.Item;
        /// <inheritdoc/>
        public void Fill(Action onFill, Action? onCancel = null)
        {
            if (!_getValue.Exists)
                return;
            _getValue.Item(onFill, onCancel);
        }

        /// <inheritdoc cref="Uncertain{T}.Uncertain(T)"/>
        public static implicit operator Uncertain<T>(T value) => Of(value);
        /// <inheritdoc cref="Uncertain{T}.Uncertain(Action)"/>
        public static explicit operator Uncertain<T>(Action<Action, Action?> getValue) => Of(getValue);
        /// <inheritdoc cref="Uncertain{T}.Value"/>
        public static explicit operator T(Uncertain<T> u) => u.Value!;
    }
}
