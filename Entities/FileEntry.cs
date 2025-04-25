namespace pyjump.Entities
{
    public class FileEntry
    {
        public string Id { get; set; }
        public string ResourceKey { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public DateTime? LastModified { get; set; }
        public string Owner { get; set; }
        public string FolderId { get; set; }
        public string FolderName { get; set; }
        public string FolderUrl { get; set; }
        public string Type { get; set; }
    }

}
