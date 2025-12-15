namespace KTANERoboExpert.Uncertain
{
    /// <summary>
    /// Represents an integer dependent on edgework that may or may not be known.
    /// </summary>
    public class UncertainInt : Uncertain<int>
    {
        private readonly Maybe<int> _min = new();
        private readonly Maybe<int> _max = new();

        /// <summary>
        /// A definitely known integer.
        /// </summary>
        public UncertainInt(int value) : base(value)
        {
            _min = new(value);
            _max = new(value);
        }
        /// <summary>
        /// A definitely unknown integer.
        /// </summary>
        public UncertainInt(Action<Action, Action?> getValue) : base(getValue) { }
        private UncertainInt(Action<Action, Action?> getValue, Maybe<int> min, Maybe<int> max) : base(getValue)
        {
            _min = min;
            _max = max;
        }

        /// <summary>
        /// The minimum value, if known.
        /// </summary>
        public int Min => _min.Item;
        /// <summary>
        /// The maximum value, if known.
        /// </summary>
        public int Max => _max.Item;

        /// <inheritdoc cref="UncertainInt.UncertainInt(int)"/>
        public static UncertainInt Exactly(int value) => new(value);
        /// <summary>
        /// An unknown integer constrained to be at least some minimum value.
        /// </summary>
        public static UncertainInt AtLeast(int min, Action<Action, Action?> getValue) => new(getValue, new(min), new());
        /// <summary>
        /// An unknown integer constrained to be at most some maximum value.
        /// </summary>
        public static UncertainInt AtMost(int max, Action<Action, Action?> getValue) => new(getValue, new(), new(max));
        /// <summary>
        /// An unknown integer constrained to be within some range.
        /// </summary>
        public static UncertainInt InRange(int min, int max, Action<Action, Action?> getValue)
        {
            if (min == max)
                return new(min);
            if (min > max)
                return new(getValue);
            return new(getValue, new(min), new(max));
        }

        public static UncertainBool operator >(UncertainInt a, UncertainInt b)
        {
            int x = a._min.Exists ? a._min.Item : int.MinValue;
            int y = b._max.Exists ? b._max.Item : int.MaxValue;
            if (x > y)
                return true;
            x = a._max.Exists ? a._max.Item : int.MaxValue;
            y = b._min.Exists ? b._min.Item : int.MinValue;
            if (x <= y)
                return false;
            return new(a.IsCertain ? b._getValue.Item! : a._getValue.Item!);
        }
        public static UncertainBool operator <(UncertainInt a, UncertainInt b)
        {
            int x = a._max.Exists ? a._max.Item : int.MaxValue;
            int y = b._min.Exists ? b._min.Item : int.MinValue;
            if (x < y)
                return true;
            x = a._min.Exists ? a._min.Item : int.MinValue;
            y = b._max.Exists ? b._max.Item : int.MaxValue;
            if (x >= y)
                return false;
            return new(a.IsCertain ? b._getValue.Item! : a._getValue.Item!);
        }
        public static UncertainBool operator >=(UncertainInt a, UncertainInt b) => !(a < b);
        public static UncertainBool operator <=(UncertainInt a, UncertainInt b) => !(a > b);
        public static UncertainBool operator ==(UncertainInt a, UncertainInt b)
        {
            if (a.IsCertain && b.IsCertain)
                return a.Value == b.Value;

            if (((a < b).IsCertain && (a < b).Value) || ((a > b).IsCertain && a.Value > b.Value))
                return false;

            return new(a.IsCertain ? b._getValue.Item! : a._getValue.Item!);
        }
        public static UncertainBool operator !=(UncertainInt a, UncertainInt b) => !(a == b);

        /// <inheritdoc cref="UncertainInt.UncertainInt(int)"/>
        public static implicit operator UncertainInt(int value) => new(value);

        public static UncertainInt operator -(UncertainInt u) =>
            u.IsCertain ? new(-u.Value) : new(u._getValue.Item!, u._min.Map(x => -x), u._max.Map(x => -x ));
        public static UncertainInt operator ++(UncertainInt u) =>
            u.IsCertain ? new(u.Value + 1) : new(u._getValue.Item!, u._min.Map(x => x + 1), u._max.Map(x => x + 1));
        public static UncertainInt operator --(UncertainInt u) =>
            u.IsCertain ? new(u.Value - 1) : new(u._getValue.Item!, u._max.Map(x => x - 1), u._min.Map(x => x - 1));
        public static UncertainInt operator +(UncertainInt a, UncertainInt b)
        {
            if (a.IsCertain && b.IsCertain)
                return new(a.Value + b.Value);
            if (a.IsCertain)
                return new(b._getValue.Item!, b._min.Map(x => x + a.Value), b._max.Map(x => x + a.Value));
            if (b.IsCertain)
                return new(a._getValue.Item!, a._min.Map(x => x + b.Value), a._max.Map(x => x + b.Value));
            return new(a._getValue.Item!, a._min.FlatMap(x => b._min.Map(y => x + y)), a._max.FlatMap(x => b._max.Map(y => x + y)));
        }
        public static UncertainInt operator -(UncertainInt a, UncertainInt b) => a + (-b);

        public override bool Equals(object? other) => other is UncertainInt i && Equals(i);
        public bool Equals(UncertainInt? other) => other is not null &&
                other._getValue == _getValue &&
                other.Value == Value &&
                other._min == _min &&
                other._max == _max;
        public override int GetHashCode() => HashCode.Combine(_getValue, Value, _min, _max);
    }
}
