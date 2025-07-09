namespace IsoBoiler.Utilities
{
    public static class FluentBooleans
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

    }
}
