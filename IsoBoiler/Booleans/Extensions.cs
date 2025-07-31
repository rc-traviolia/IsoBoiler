namespace IsoBoiler.Booleans
{
    public static class Extensions
    {
        public static bool Or(this bool source, bool newValue)
        {
            return source || newValue;
        }

        public static bool And(this bool source, bool newValue)
        {
            return source && newValue;
        }

        public static bool Xor(this bool source, bool newValue)
        {
            return (source || newValue) && !(source && newValue);
        }

        public static bool Nor(this bool source, bool newValue)
        {
            return !(source || newValue);
        }

        public static bool Nand(this bool source, bool newValue)
        {
            return !(source && newValue);
        }

        public static bool NotIn<TType>(this TType value, params TType[] valueArray) where TType : IComparable
        {
            return !value.In(valueArray);
        }

        public static bool In<TType>(this TType value, params TType[] valueArray) where TType : IComparable
        {
            if (valueArray.Length == 0)
            {
                throw new ArgumentException("At least one value must be provided.", nameof(valueArray));
            }

            foreach (var comparedValue in valueArray)
            {
                if (value!.CompareTo(comparedValue) == 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
