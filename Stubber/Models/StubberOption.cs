namespace StubberProject.Models
{
    public class StubberOption
    {
        public string CodeFilePathPrefix { get; set; }
        public string StubFilePathPrefix { get; set; }
        public bool DisableNamespaces { get; set; } = false;
    }
}
