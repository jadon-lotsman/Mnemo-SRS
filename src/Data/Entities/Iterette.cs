using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itereta.Data.Entities
{
    public class Iterette
    {
        public int Id { get; set; }
        public int BaseVocabularyEntryId { get; set; }

        public string Prompt { get; set; }
        public string UserAnswer { get; set; }
        public bool IsForwardQuestion { get; set; }


        public int IterationId { get; set; }
        public Iteration Iteration { get; set; }


        public Iterette() { }

        public Iterette(VocabularyEntry entry, bool isForwardQuestion)
        {
            BaseVocabularyEntryId = entry.Id;
            Prompt = isForwardQuestion ? entry.Foreign : entry.Translations[0];
            UserAnswer = string.Empty;
            IsForwardQuestion = isForwardQuestion;
        }
    }
}
