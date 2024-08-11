namespace DockerDashboard.Ui.Components
{
    public class CommonInfo(string name, object value)
    {
        public string Name { get; set; } = name;

        public object? Value { get; set; } = value;
    }

    public class DetailedInfo(string category, string name, object value)
    {
        public string Category { get; set; } = category;

        public string Name { get; set; } = name;

        public object? Value { get; set; } = value;
    }
}
