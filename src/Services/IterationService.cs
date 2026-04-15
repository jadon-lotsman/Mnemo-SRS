using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Itereta;
using Itereta.Common;
using Itereta.Common.Dtos;
using Itereta.Data;
using Itereta.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Itereta.Services
{
    public class IterationService
    {
        private AppDbContext _context;
        private UserService _userService;
        private VocabularyService _vocabularyService;

        private static Random _random = new Random();


        public IterationService(AppDbContext context, UserService userService, VocabularyService vocabularyService)
        {
            _context = context;
            _userService = userService;
            _vocabularyService = vocabularyService;
        }


        public async Task<RequestResult<Iteration>> GetIterationStatusAsync(int userId)
        {
            var iteration = await GetIterationAsync(userId);
            if (iteration == null) return RequestResult<Iteration>.Failure("ITERATION_NOT_FOUND");
            if (iteration.WasFinished) return RequestResult<Iteration>.Failure("ITERATION_WAS_FINISHED");
            else return RequestResult<Iteration>.Failure("ITERATION_IN_PROCESS");
        }

        public async Task<Iteration?> GetIterationAsync(int userId)
        {
            return await _context.Iterations
                .Include(i => i.Iterettes)
                .FirstOrDefaultAsync(e => e.User.Id == userId);
        }

        public async Task<List<Iterette>> GetAllIterettesAsync(int userId)
        {
            return await _context.Iterettes
                .Where(i => i.Iteration.UserId == userId)
                .ToListAsync();
        }

        public async Task<Iterette?> GetIteretteByIdAsync(int userId, int iteretteId)
        {
            return await _context.Iterettes
                .Include(i => i.Iteration)
                .FirstOrDefaultAsync(s => s.Id == iteretteId && s.Iteration.UserId == userId);
        }


        public async Task<RequestResult<Iteration>> StartIterationAsync(int userId)
        {
            var iteration = await GetIterationAsync(userId);

            if (iteration != null && !iteration.WasFinished)
                return RequestResult<Iteration>.Failure("ITERATION_NOT_FINISHED");
            else if (iteration != null && iteration.WasFinished)
                _context.Iterations.Remove(iteration);


            var iterettes = new List<Iterette>();
            var randomEntries = _vocabularyService.GetUserRandomEntries(userId);

            foreach (var entry in randomEntries)
                iterettes.Add(new Iterette(entry, _random.Next(2) == 0));


            var currentUser = await _userService.GetByIdAsync(userId);

            if (currentUser == null)
                return RequestResult<Iteration>.Failure("USER_NOT_FOUND");


            var currentIteration = new Iteration(currentUser, iterettes);

            await _context.Iterations.AddAsync(currentIteration);
            await _context.SaveChangesAsync();

            return RequestResult<Iteration>.Success(currentIteration);
        }

        public async Task<RequestResult<Iterette>> SetIteretteAnswerAsync(int userId, int iteretteId, string answer)
        {
            var iterette = await GetIteretteByIdAsync(userId, iteretteId);

            if (iterette == null)
                return RequestResult<Iterette>.Failure("ITERETTE_NOT_FOUND");

            if (iterette.Iteration.WasFinished)
                return RequestResult<Iterette>.Failure("ITERATION_WAS_FINISHED");


            iterette.UserAnswer = answer;
            await _context.SaveChangesAsync();

            return RequestResult<Iterette>.Success(iterette);
        }

        public async Task<RequestResult<IterationResultDto>> FinishIterationAsync(int userId)
        {
            var iteration = await GetIterationAsync(userId);

            if (iteration == null)
                return RequestResult<IterationResultDto>.Failure("ITERATION_NOT_FOUND");

            if (iteration.Iterettes == null)
                return RequestResult<IterationResultDto>.Failure("ITERATION_HAS_NO_ITERETTES");


            var entriesIds = iteration.Iterettes.Select(i => i.BaseVocabularyEntryId).ToList();
            var entriesDict = await _vocabularyService.GetEntriesDictByIdsAsync(userId, entriesIds);


            int missedCount = 0;
            var failedEntries = new List<VocabularyEntry>();

            foreach (var iterette in iteration.Iterettes)
            {
                if (entriesDict.TryGetValue(iterette.BaseVocabularyEntryId, out var baseEntry) && baseEntry != null)
                {
                    if (!IsSimilarEnough(iterette, baseEntry))
                        failedEntries.Add(baseEntry);
                }
                else
                {
                    missedCount++;
                }
            }


            if (!iteration.WasFinished)
            {
                iteration.Finished = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }


            int totalCount = iteration.Iterettes.Count - missedCount;
            int correctCount = totalCount - failedEntries.Count;

            var result = new IterationResultDto(
                correctCount,
                totalCount,
                Mapper.MapToDto(failedEntries),
                iteration.Started,
                iteration.Finished!.Value);

            return RequestResult<IterationResultDto>.Success(result);
        }

        private bool IsSimilarEnough(Iterette iterette, VocabularyEntry entry, float similarityBorder = 0.75f)
        {
            string userAnswer = iterette.UserAnswer;

            if (iterette.IsForwardQuestion)
                return entry.Translations.Any(t => userAnswer.GetSimilarity(t) >= similarityBorder);
            else
                return userAnswer.GetSimilarity(entry.Foreign) >= similarityBorder;
        }
    }
}
