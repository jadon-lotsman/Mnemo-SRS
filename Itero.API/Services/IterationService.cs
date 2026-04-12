using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Itero.API.Common;
using Itero.API.Data;
using Itero.API.Data.Entities;
using Itero.API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Itero.API.Services
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

        public async Task<IterationResultDto?> FinishIterationAsync(int userId)
        {
            var iteration = await GetIterationAsync(userId);

            if (iteration == null || iteration.Iterettes == null)
                return null;


            int correctCount = 0;
            int totalCount = iteration.Iterettes.Count;
            var failedEntries = new List<VocabularyEntryDto>();

            const float SimilarityBorder = 0.75f;

            foreach (var iterette in iteration.Iterettes)
            {
                var restoredEntry = await _vocabularyService.GetEntryByIdAsync(userId, iterette.BaseVocabularyEntryId);

                if(CalcSimilarity(iterette, restoredEntry) >= SimilarityBorder)
                {
                    correctCount++;
                }
                else
                {
                    failedEntries.Add(Mapper.MapToDto(restoredEntry));
                }
            }


            if (!iteration.WasFinished)
            {
                iteration.Finished = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            var result = new IterationResultDto(correctCount, totalCount, failedEntries.ToArray(), iteration.Started, iteration.Finished.Value);

            return result;
        }

        private float CalcSimilarity(Iterette iterette, VocabularyEntry entry)
        {
            string answer = iterette.UserAnswer;

            if (iterette.IsForwardQuestion)
            {
                float maxSimilarity = 0;

                foreach (string translation in entry.Translations)
                {
                    float currentSimilarity = answer.GetSimilarity(translation);
                    maxSimilarity = Math.Max(maxSimilarity, currentSimilarity);
                }

                return maxSimilarity;
            }
            else
            {
                return answer.GetSimilarity(entry.Foreign);
            }
        }
    }
}
