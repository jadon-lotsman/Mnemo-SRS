using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itero.API.Data.Entities
{
    public class Iteration
    {
        public int Id { get; set; }

        public DateTime Started { get; set; }
        public DateTime? Finished { get; set; }
        public bool WasFinished => Finished.HasValue;


        public int UserId { get; set; }
        public User User { get; set; }
        public List<Iterette> Iterettes { get; set; }


        public Iteration() { }

        public Iteration(User user, List<Iterette> iterettes)
        {
            Started = DateTime.UtcNow;
            Finished = null;
            UserId = user.Id;
            User = user;
            Iterettes = iterettes;
        }
    }
}
