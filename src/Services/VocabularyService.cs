using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Itereta.Common;
using Itereta.Common.Dtos;
using Itereta.Data;
using Itereta.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Itereta.Services
{
    public class VocabularyService
    {
        private AppDbContext _context;
        private UserService _userService;


        public VocabularyService(AppDbContext context, UserService userService)
        {
            _context = context;
            _userService = userService;
        }


        private IQueryable<VocabularyEntry> GetEntriesByUserQuery(int userId)
        {
            return _context.Entries.Where(e => e.User.Id == userId);
        }

        public async Task<List<VocabularyEntry>> GetAllEntriesAsync(int userId)
        {
            return await GetEntriesByUserQuery(userId)
                .ToListAsync();
        }

        public async Task<VocabularyEntry?> GetEntryByIdAsync(int userId, int id)
        {
            return await GetEntriesByUserQuery(userId)
                .FirstOrDefaultAsync(e => e.Id == id);
        }
        public async Task<Dictionary<int, VocabularyEntry>> GetEntriesDictByIdsAsync(int userId, IEnumerable<int> ids)
        {
            var list = await GetEntriesByUserQuery(userId)
                .Where(e => ids.Contains(e.Id))
                .ToListAsync();

            return list.ToDictionary(e => e.Id);
        }

        public async Task<VocabularyEntry?> GetEntryByKeyAsync(int userId, string key)
        {
            return await GetEntriesByUserQuery(userId)
                .FirstOrDefaultAsync(e => e.Foreign == key.ToLowerInvariant());
        }

        public List<VocabularyEntry> GetUserRandomEntries(int userId, int count=5)
        {
            return _context.Entries.Where(e => e.User.Id == userId)
                .AsEnumerable()
                .OrderBy(x => Guid.NewGuid())
                .Take(count)
                .ToList();
        }


        public async Task<RequestResult<VocabularyEntry>> CreateEntryAsync(int userId, VocabularyCreateDto dto)
        {
            if (!Mapper.ValidDto(dto))
                return RequestResult<VocabularyEntry>.Failure("INVALID_DATA");


            var user = await _userService.GetByIdAsync(userId);

            if (user == null)
                return RequestResult<VocabularyEntry>.Failure("USER_NOT_FOUND");


            string foreignKey = Mapper.PrepareForeign(dto.Foreign!);
            var entryByKey = await GetEntryByKeyAsync(userId, foreignKey);

            if (entryByKey != null)
                return RequestResult<VocabularyEntry>.Failure("DUPLICATE_ENTRY");


            var entry = Mapper.MapToEntry(dto, user);

            await _context.Entries.AddAsync(entry);
            await _context.SaveChangesAsync();

            return RequestResult<VocabularyEntry>.Success(entry);
        }

        public async Task<RequestResult<VocabularyEntry>> PatchEntryAsync(int userId, int entryId, VocabularyPatchDto patchDto)
        {
            if (!Mapper.ValidDto(patchDto))
                return RequestResult<VocabularyEntry>.Failure("INVALID_DATA");


            var currentEntry = await GetEntryByIdAsync(userId, entryId);

            if (currentEntry == null)
                return RequestResult<VocabularyEntry>.Failure("ENTRY_NOT_FOUND");


            Mapper.PatchFromDto(currentEntry, patchDto);

            await _context.SaveChangesAsync();

            return RequestResult<VocabularyEntry>.Success(currentEntry);
        }

        public async Task<RequestResult<bool>> RemoveEntryByIdAsync(int userId, int entryId)
        {
            var currentEntry = await GetEntryByIdAsync(userId, entryId);

            if (currentEntry == null)
                return RequestResult<bool>.Failure("ENTRY_NOT_FOUND");
                

            _context.Entries.Remove(currentEntry);
            await _context.SaveChangesAsync();

            return RequestResult<bool>.Success(true);
        }
    }
}
