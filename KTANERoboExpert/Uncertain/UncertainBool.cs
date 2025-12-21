namespace KTANERoboExpert.Uncertain
{
    /// <summary>Represents a boolean condition dependent on edgework that may or may not be known.</summary>
    public class UncertainBool : Uncertain<bool>
    {
        /// <summary>A definitely known condition.</summary>
        private UncertainBool(bool value) : base(value) { }
        /// <summary>A definitely unknown condition.</summary>
        private UncertainBool(Action<Action, Action?> getValue) : base(getValue) { }

        /// <inheritdoc cref="UncertainBool(bool)"/>
        public static new UncertainBool Of(bool value) => new(value);
        /// <inheritdoc cref="UncertainBool(Action{Action, Action?})"/>
        public static new UncertainBool Of(Action<Action, Action?> getValue) => new(getValue);

        /// <summary>Combines two conditions with a logical AND while correctly handling uncertainty.</summary>
        public static UncertainBool operator &(UncertainBool a, UncertainBool b)
        {
            if (a.IsCertain && b.IsCertain)
                return Of(a.Value && b.Value);

            if ((a.IsCertain && !a.Value) || (b.IsCertain && !b.Value))
                return Of(false);

            return Of(a.IsCertain ? b._getValue.Item! : a._getValue.Item!);
        }

        /// <summary> Combines two conditions with a logical OR while correctly handling uncertainty.</summary>
        public static UncertainBool operator |(UncertainBool a, UncertainBool b)
        {
            if (a.IsCertain && b.IsCertain)
                return Of(a.Value || b.Value);

            if ((a.IsCertain && a.Value) || (b.IsCertain && b.Value))
                return Of(true);

            return Of(a.IsCertain ? b._getValue.Item! : a._getValue.Item!);
        }

        /// <summary>Combines two conditions with a logical XOR while correctly handling uncertainty.</summary>
        public static UncertainBool operator ^(UncertainBool a, UncertainBool b)
        {
            if (a.IsCertain && b.IsCertain)
                return Of(a.Value ^ b.Value);

            return Of(a.IsCertain ? b._getValue.Item! : a._getValue.Item!);
        }
        public static UncertainBool operator &(bool a, UncertainBool b) => a ? b : false;
        public static UncertainBool operator |(bool a, UncertainBool b) => a ? true : b;
        public static UncertainBool operator &(UncertainBool a, bool b) => b & a;
        public static UncertainBool operator |(UncertainBool a, bool b) => b | a;

        public static UncertainBool operator !(UncertainBool b) => b.IsCertain ? !b.Value : b;

        /// <inheritdoc cref="UncertainBool.UncertainBool(bool)"/>
        public static implicit operator UncertainBool(bool value) => Of(value);
    }
}
