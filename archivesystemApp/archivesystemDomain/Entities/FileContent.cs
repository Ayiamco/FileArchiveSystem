using System;

namespace archivesystemDomain.Entities
{
    public class FileContent
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public byte[] Content { get; set; }
    }
}