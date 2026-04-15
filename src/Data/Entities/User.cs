using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itereta.Data.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; }
        public DateTime Registered { get; set; }


        public Iteration? Iteration { get; set; }
        public List<VocabularyEntry> Entries { get; set; }


        public User() {}

        public User(string username)
        {
            Username = username;
            Registered = DateTime.UtcNow;
            Entries = new List<VocabularyEntry>();
        }
    }
}
