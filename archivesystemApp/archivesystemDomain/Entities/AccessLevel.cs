using System;
using System.Collections.Generic;
using System.Text;

namespace archivesystemDomain.Entities
{
    public class AccessLevel
    {
        public int Id { get; set; }
        public int Level { get; set; }
        public string LevelDescription { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
