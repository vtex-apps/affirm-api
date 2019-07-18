namespace UnitTests.Fakes
{
    using Affirm.Services;

    public class FakeVtexEnvironmentVariableProvider : IVtexEnvironmentVariableProvider
    {
        public string Account { get; set; }
        public string Workspace { get; set; }
        public string ApplicationName { get; set; }
        public string ApplicationVendor { get; set; }
        public string Region { get; set; }
    }
}
