namespace IsoBoiler.Logging
{
    /// <summary>
    /// An Exception class that provides a vehicle for CustomProperties to be passed around with the exception
    /// in order to make it simpler to .Log(exception) in the final catch block.
    /// </summary>
    public class ExceptionBoiler : Exception
    {
        public Dictionary<string, object> CustomProperties { get; set; }
        public ExceptionBoiler(string message, Dictionary<string, object> customProperties) : base(message)
        {
            CustomProperties = customProperties;
        }
    }
}
