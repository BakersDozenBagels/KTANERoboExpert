namespace KTANERoboExpert.Uncertain
{
    /// <summary>
    /// Represents a boolean condition dependent on edgework that may or may not be known.
    /// </summary>
    public class UncertainBool : Uncertain<bool>
    {
        /// <summary>
        /// A definitely known condition.
        /// </summary>
        public UncertainBool(bool value) : base(value) { }
        /// <summary>
        /// A definitely unknown condition.
        /// </summary>
        public UncertainBool(Action getValue) : base(getValue) { }

        /// <summary>
        /// Combines two conditions with a logical AND while correctly handling uncertainty.
        /// </summary>
        public static UncertainBool operator &(UncertainBool a, UncertainBool b)
        {
            if (a.IsCertain && b.IsCertain)
                return new(a.Value && b.Value);

            if ((a.IsCertain && !a.Value) || (b.IsCertain && !b.Value))
                return new(false);

            return new(a.IsCertain ? b._getValue.Item! : a._getValue.Item!);
        }

        /// <summary>
        /// Combines two conditions with a logical OR while correctly handling uncertainty.
        /// </summary>
        public static UncertainBool operator |(UncertainBool a, UncertainBool b)
        {
            if (a.IsCertain && b.IsCertain)
                return new(a.Value || b.Value);

            if ((a.IsCertain && a.Value) || (b.IsCertain && b.Value))
                return new(true);

            return new(a.IsCertain ? b._getValue.Item! : a._getValue.Item!);
        }

        /// <summary>
        /// Combines two conditions with a logical XOR while correctly handling uncertainty.
        /// </summary>
        public static UncertainBool operator ^(UncertainBool a, UncertainBool b)
        {
            if (a.IsCertain && b.IsCertain)
                return new(a.Value ^ b.Value);

            return new(a.IsCertain ? b._getValue.Item! : a._getValue.Item!);
        }

        public static UncertainBool operator !(UncertainBool b) => b.IsCertain ? !b.Value : b;

        /// <inheritdoc cref="UncertainBool.UncertainBool(bool)"/>
        public static implicit operator UncertainBool(bool value) => new(value);
    }
}
