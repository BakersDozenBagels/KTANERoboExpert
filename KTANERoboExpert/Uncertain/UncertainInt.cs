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
        private UncertainInt(int value) : base(value)
        {
            _min = new(value);
            _max = new(value);
        }
        /// <summary>
        /// A definitely unknown integer.
        /// </summary>
        private UncertainInt(Action<Action, Action?> getValue) : base(getValue) { }
        private UncertainInt(Action<Action, Action?> getValue, Maybe<int> min, Maybe<int> max) : base(getValue)
        {
            _min = min;
            _max = max;
        }

        /// <summary>
        /// The minimum value, if known.
        /// </summary>
        public int Min => _min.OrElse(int.MinValue);
        /// <summary>
        /// The maximum value, if known.
        /// </summary>
        public int Max => _max.OrElse(int.MaxValue);

        /// <inheritdoc cref="UncertainInt.UncertainInt(int)"/>
        public static UncertainInt Exactly(int value) => new(value);

        /// <inheritdoc cref="UncertainInt.UncertainInt(Action{Action, Action?})"/>
        public static UncertainInt Unknown(Action<Action, Action?> getValue) => new(getValue);
        /// <summary>
        /// An unknown integer constrained to be at least some minimum value.
        /// </summary>
        public static UncertainInt AtLeast(int min, Action<Action, Action?> getValue) => InRange(getValue, new(min), new());
        /// <summary>
        /// An unknown integer constrained to be at most some maximum value.
        /// </summary>
        public static UncertainInt AtMost(int max, Action<Action, Action?> getValue) => InRange(getValue, new(), new(max));
        /// <summary>
        /// An unknown integer constrained to be within some range.
        /// </summary>
        public static UncertainInt InRange(Maybe<int> min, Maybe<int> max, Action<Action, Action?> getValue) => InRange(getValue, min, max);
        private static UncertainInt InRange(Action<Action, Action?> getValue, Maybe<int> min, Maybe<int> max)
        {
            if (min.Exists && max.Exists && min.Item == max.Item)
                return new(min.Item);
            if (min.Exists && max.Exists && min.Item > max.Item)
                return new(getValue);
            return new(getValue, min, max);
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
            return UncertainBool.Of(a.IsCertain ? b._getValue.Item! : a._getValue.Item!);
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
            return UncertainBool.Of(a.IsCertain ? b._getValue.Item! : a._getValue.Item!);
        }
        public static UncertainBool operator >=(UncertainInt a, UncertainInt b) => !(a < b);
        public static UncertainBool operator <=(UncertainInt a, UncertainInt b) => !(a > b);
        public static UncertainBool operator ==(UncertainInt a, UncertainInt b)
        {
            if (a.IsCertain && b.IsCertain)
                return a.Value == b.Value;

            if (((a < b).IsCertain && (a < b).Value) || ((a > b).IsCertain && a.Value > b.Value))
                return false;

            return UncertainBool.Of(a.IsCertain ? b._getValue.Item! : a._getValue.Item!);
        }
        public static UncertainBool operator !=(UncertainInt a, UncertainInt b) => !(a == b);

        /// <inheritdoc cref="Exactly(int)"/>
        public static implicit operator UncertainInt(int value) => Exactly(value);

        public static UncertainInt operator -(UncertainInt u) =>
            u.IsCertain ? Exactly(-u.Value) : InRange(u._getValue.Item!, u._max.Map(x => -x), u._min.Map(x => -x));
        public static UncertainInt operator ++(UncertainInt u) =>
            u.IsCertain ? Exactly(u.Value + 1) : InRange(u._getValue.Item!, u._min.Map(x => x + 1), u._max.Map(x => x + 1));
        public static UncertainInt operator --(UncertainInt u) =>
            u.IsCertain ? Exactly(u.Value - 1) : InRange(u._getValue.Item!, u._min.Map(x => x - 1), u._max.Map(x => x - 1));
        public static UncertainInt operator +(UncertainInt a, UncertainInt b)
        {
            if (a.IsCertain && b.IsCertain)
                return Exactly(a.Value + b.Value);
            if (a.IsCertain)
                return InRange(b._getValue.Item!, b._min.Map(x => x + a.Value), b._max.Map(x => x + a.Value));
            if (b.IsCertain)
                return InRange(a._getValue.Item!, a._min.Map(x => x + b.Value), a._max.Map(x => x + b.Value));
            return InRange(a._getValue.Item!, a._min.FlatMap(x => b._min.Map(y => x + y)), a._max.FlatMap(x => b._max.Map(y => x + y)));
        }
        public static UncertainInt operator -(UncertainInt a, UncertainInt b) => a + (-b);
        public static UncertainInt operator *(UncertainInt a, UncertainInt b)
        {
            if (a.IsCertain && b.IsCertain)
                return a.Value * b.Value;

            var w = a._min.Exists ? a._min.Item : int.MinValue;
            var x = b._min.Exists ? b._min.Item : int.MinValue;
            var y = a._max.Exists ? a._max.Item : int.MaxValue;
            var z = b._max.Exists ? b._max.Item : int.MaxValue;

            if (w < 0 && y < 0)
                return -((-a) * b);

            if (y < 0 && w >= 0)
                throw new ArgumentException("Illegal UncertainInt provided", nameof(a));

            if (w < 0 && y >= 0)
                return InRange(a.IsCertain ? b._getValue.Item! : a._getValue.Item!, Math.Min(w * z, x * y), Math.Max(w * x, y * z));


            return InRange(a.IsCertain ? b._getValue.Item! : a._getValue.Item!, w * x, y * z);
        }

        public override bool Equals(object? other) => other is UncertainInt i && Equals(i);
        public bool Equals(UncertainInt? other) => other is not null &&
                other._getValue == _getValue &&
                other.Value == Value &&
                other._min == _min &&
                other._max == _max;
        public override int GetHashCode() => HashCode.Combine(_getValue, Value, _min, _max);

        /// <summary>Provides a lower bound for the number to make deductions when the value is uncertain.</summary>
        public UncertainInt ButAtMost(UncertainInt max) => IsCertain ? this : InRange(_getValue.Item!, _min, Math.Min(Max, max.Max));
        /// <summary>Provides an upper bound for the number to make deductions when the value is uncertain.</summary>
        public UncertainInt ButAtLeast(UncertainInt min) => IsCertain ? this : InRange(_getValue.Item!, Math.Max(Min, min.Min), _max);
        /// <summary>Provides a lower and upper bound for the number to make deductions when the value is uncertain.</summary>
        public UncertainInt ButWithinRange(UncertainInt min, UncertainInt max) => IsCertain ? this : InRange(_getValue.Item!, Math.Max(Min, min.Min), Math.Min(Max, max.Max));
    }
}
