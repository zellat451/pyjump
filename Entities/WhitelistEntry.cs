namespace pyjump.Entities
{
    public class WhitelistEntry
    {
        public string Id { get; set; }
        public string ResourceKey { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public DateTime? LastChecked { get; set; }
        public string Type { get; set; }
    }
}
