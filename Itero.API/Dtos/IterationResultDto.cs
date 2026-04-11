using Itero.API.Data.Entities;

namespace Itero.API.Dtos
{
    public class IterationResultDto
    {
        public int Correct { get; set; }
        public int Total { get; set; }
        public float Percent { get; set; }
        public char LetterGrade { get; set; }
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
        public VocabularyEntryDto[] FailedEntries { get; set; }


        public IterationResultDto(int correct, int total, VocabularyEntryDto[] failedEntries, DateTime started, DateTime finished)
        {
            Correct = correct;
            Total = total;
            Percent = (float) Correct / Total * 100;
            Started = started;
            Finished = finished;
            FailedEntries = failedEntries;

            if (Percent >= 90)
                LetterGrade = 'A';
            else if (Percent >= 80)
                LetterGrade = 'B';
            else if (Percent >= 70)
                LetterGrade = 'C';
            else if (Percent >= 60)
                LetterGrade = 'D';
            else
                LetterGrade = 'F';
        }
    }
}
