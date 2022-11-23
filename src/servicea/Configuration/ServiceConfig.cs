namespace ServiceA.Configuration
{
    public class ServiceConfig
    {
        public const string SECTION_NAME = "ServiceA";
        public string BackendServiceName { get; set; }
        public int BackendPort { get; set; }

        public bool IsValid() => !string.IsNullOrWhiteSpace(BackendServiceName) && BackendPort > 0;
    }
}
