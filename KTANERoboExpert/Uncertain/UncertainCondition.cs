using System.Diagnostics.CodeAnalysis;

namespace KTANERoboExpert.Uncertain
{
    /// <summary>Represents a chain of elseif conditions.</summary>
    public class UncertainCondition<T> : IUncertain<T>
    {
        private readonly (UncertainBool, T)[] _values;
        /// <summary><see langword="true"/> if this <see cref="UncertainCondition{T}"/> could not have "fallen through" to an undefined case.</summary>
        public bool Exhaustive { get; private init; }
        /// <inheritdoc/>
        [MemberNotNullWhen(true, nameof(Value))]
        public bool IsCertain { get => Exhaustive && Reduce().Count() is 1; }
        /// <summary>Every possible outcome of this condition chain.</summary>
        public IEnumerable<T> Possibilities { get => Reduce().Select(tup => tup.Item2); }
        /// <summary>The only possible outcome of this condition chain, if applicable.</summary>
        public T? Value { get => Reduce().First().Item2; }

        private IEnumerable<(UncertainBool, T)> Reduce()
        {
            HashSet<T> used = [];

            foreach (var v in _values)
                if (used.Add(v.Item2))
                    yield return v;
        }

        /// <inheritdoc/>
        public void Fill(Action onFill, Action? onCancel = null) => _values[0].Item1.Fill(onFill, onCancel);

        /// <summary>A condition and the result of it being true.</summary>
        public static UncertainCondition<T> Of(UncertainBool key, T value) => new(key, value);
        private UncertainCondition(UncertainBool key, T value)
        {
            if (key.IsCertain && !key.Value)
            {
                _values = [];
                Exhaustive = false;
            }
            else if (key.IsCertain && key.Value)
            {
                _values = [(key, value)];
                Exhaustive = true;
            }
            else
            {
                _values = [(key, value)];
                Exhaustive = false;
            }
        }
        private UncertainCondition((UncertainBool, T)[] values, bool exhaustive)
        {
            _values = values;
            Exhaustive = exhaustive;
        }

        /// <summary>Adds another condition to the end of this chain.</summary>
        public UncertainCondition<T> OrElse(UncertainCondition<T> other)
        {
            if (Exhaustive)
                return this;

            return new([.. _values, .. other._values], other.Exhaustive);
        }

        /// <summary>Adds an "otherwise" condition to the end of this chain.</summary>
        public UncertainCondition<T> OrElse(T other)
        {
            if (Exhaustive)
                return this;

            return new([.. _values, (true, other)], true);
        }

        /// <inheritdoc cref="UncertainCondition{T}.OrElse(UncertainCondition{T})"/>
        public static UncertainCondition<T> operator |(UncertainCondition<T> a, UncertainCondition<T> b) => a.OrElse(b);
        /// <inheritdoc cref="UncertainCondition{T}.OrElse(T)"/>
        public static UncertainCondition<T> operator |(UncertainCondition<T> a, T b) => a.OrElse(b);
        /// <inheritdoc cref="UncertainCondition{T}.UncertainCondition(UncertainBool, T)"/>
        public static implicit operator UncertainCondition<T>((UncertainBool key, T value) tup) => Of(tup.key, tup.value);
    }
}
