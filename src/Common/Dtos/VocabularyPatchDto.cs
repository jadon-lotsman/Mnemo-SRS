using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itereta.Common.Dtos
{
    public class VocabularyPatchDto
    {
        public string? Foreign { get; set; }
        public string? Transcription { get; set; }
        public string[]? ExamplesAdd { get; set; }
        public string[]? ExamplesRemove { get; set; }
        public string[]? TranslationsAdd { get; set; }
        public string[]? TranslationsRemove { get; set; }
    }
}
