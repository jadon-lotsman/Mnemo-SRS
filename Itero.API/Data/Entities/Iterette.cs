using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itero.API.Data.Entities
{
    public class Iterette
    {
        public int Id { get; set; }

        public string Prompt { get; set; }
        public string? UserAnswer { get; set; }
        public bool IsForwardQuestion { get; set; }


        public int IterationId { get; set; }
        public Iteration Iteration { get; set; }
        public int VocabularyEntryId { get; set; }
        public VocabularyEntry VocabularyEntry { get; set; } // to delete


        public Iterette() { }

        public Iterette(VocabularyEntry entry, bool isForwardQuestion)
        {
            IsForwardQuestion = isForwardQuestion;
            Prompt = IsForwardQuestion ? entry.Foreign : entry.Translations[0];
            VocabularyEntryId = entry.Id;
            VocabularyEntry = entry;
        }
    }
}
