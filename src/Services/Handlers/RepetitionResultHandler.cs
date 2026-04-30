using Mnemo.Contracts.Dtos.Repetition;
using Mnemo.Data.Entities;
using Mnemo.Services.Queries;
using Mnemo.Common;

namespace Mnemo.Services.Handlers
{
    public class RepetitionResultHandler
    {
        private VocabularyQueries _vocabularyQueries;
        private RepetitionStateService _stateService;


        public RepetitionResultHandler(VocabularyQueries vocabularyQueries, RepetitionStateService stateService)
        {
            _vocabularyQueries = vocabularyQueries;
            _stateService = stateService;
        }



        public async Task<RepetitionResult> ExecuteAsync(int userId, List<RepetitionTask> tasks)
        {
            var entriesIds = tasks.Select(t => t.BaseVocabularyEntryId).ToList();
            var entriesDict = await _vocabularyQueries.GetDictByIdsAsync(userId, entriesIds);

            int missedTasksCounter = 0, correctTaskCounter = 0;
            var resultedEntries = new List<VocabularyEntry>();

            foreach (var task in tasks)
            {
                if (entriesDict.TryGetValue(task.BaseVocabularyEntryId, out var entry) && entry != null)
                {
                    double similarity = GetMaxAnswerSimilarity(task, entry);
                    double quality = SM2Helper.ComputeQuality(task.RepetitionSession.AverageActionTime, task.ActionTimeSpan, task.ActionCounter, similarity);

                    await _stateService.UpdateRepetitionStateAsync(userId, entry.Id, quality, shouldIncrementCounter: true);
                    resultedEntries.Add(entry);

                    if (SM2Helper.IsPassingQuality(quality))
                        correctTaskCounter++;
                }
                else
                {
                    missedTasksCounter++;
                }
            }

            return new RepetitionResult(correctTaskCounter, resultedEntries);
        }


        private double GetMaxAnswerSimilarity(RepetitionTask task, VocabularyEntry entry)
        {
            string userAnswer = task.UserAnswer;

            if (task.IsForwardQuestion)
                return entry.Translations.Max(userAnswer.ComputeLevenshteinSimilarity);
            else
                return userAnswer.ComputeLevenshteinSimilarity(entry.Foreign);
        }
    }
}
