namespace IsoBoiler.Tests.Helpers
{
    public class ExampleRequest_WithRequiredProperties
    {
        public required string RequestName { get; set; }
        public string RequestDescription { get; set; } = string.Empty;
        public required int RequestID { get; set; }
    }
}
