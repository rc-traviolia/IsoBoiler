namespace IsoBoiler.Tests.Helpers
{
    public class ExampleResponse
    {
        public string RequestName { get; set; } = string.Empty;
        public string RequestDescription { get; set; } = string.Empty;
        public int RequestID { get; set; }
        public int ResponseID { get; set; }
        public bool Successful { get; set; }
    }
}
