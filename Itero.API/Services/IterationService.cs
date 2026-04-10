using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Itero.API.Data;
using Itero.API.Data.Entities;
using Itero.API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Itero.API.Services
{
    public class IterationService
    {
        private AppDbContext _context;
        private VocabularyService _vocabularyService;
        private UserService _userService;

        public IterationService(AppDbContext context, UserService userService, VocabularyService vocabularyService)
        {
            _context = context;
            _userService = userService;
            _vocabularyService = vocabularyService;
        }


        private async Task<bool> GetAnyIterationAsync(int userId)
        {
            return await _context.Iterations
                .AnyAsync(e => e.User.Id == userId);
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
                .FirstOrDefaultAsync(s => s.Id == iteretteId && s.Iteration.UserId == userId);
        }

        public async Task<Iteration?> CreateIterationAsync(int userId)
        {
            bool HasAny = await GetAnyIterationAsync(userId);

            if (HasAny)
                return null;
             

            var iterettes = new List<Iterette>();
            var rendomEntries = _vocabularyService.GetUserRandomEntriesAsync(userId).Result;

            foreach (var entry in rendomEntries)
                iterettes.Add(new Iterette(entry, true));

            var currentUser = await _userService.GetByIdAsync(userId);
            var currentIteration = new Iteration(currentUser, iterettes);

            await _context.Iterations.AddAsync(currentIteration);
            await _context.SaveChangesAsync();

            return currentIteration;
        }


        public async Task<bool> SetIteretteAnswerAsync(int userId, int iteretteId, string answer)
        {
            var iterationPart = await GetIteretteByIdAsync(userId, iteretteId);

            if (iterationPart == null)
                return false;
                
            iterationPart.UserAnswer = answer;

            await _context.SaveChangesAsync();

            return true;
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
                var entry = await _vocabularyService.GetEntryByIdAsync(userId, iterette.VocabularyEntryId);

                if(CalcSimilarity(iterette, entry) >= SimilarityBorder)
                {
                    correctCount++;
                }
                else
                {
                    var mapper = new VocabularyEntryMapper();
                    failedEntries.Add(mapper.GetDto(iterette.VocabularyEntry));
                }
            }

            var result = new IterationResultDto(correctCount, totalCount, failedEntries.ToArray(), iteration.Created);

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
                return answer.GetSimilarity(iterette.VocabularyEntry.Foreign);
            }
        }
    }
}
