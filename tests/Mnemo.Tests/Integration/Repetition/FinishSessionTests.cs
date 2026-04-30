using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mnemo.Data.Entities;
using Mnemo.Tests.Integration;
using Mnemo.Data.Entities;
using Mnemo.Services.Queries;
using Mnemo.Services;
using Mnemo.Common;

namespace Mnemo.Tests.Integration.Repetition
{
    public class FinishSessionTests : IntegrationTestBase
    {
        [Fact]
        public async Task FinishSession_ShouldStopAndRemoveSessionWithTasks()
        {
            // Arrange
            var user   = DataSeeder.CreateUser(id: 3, username: "Bob");
            var entry1 = DataSeeder.CreateEntry(id: 6, userId: user.Id, foreign: "apple", translations: "яблоко");
            var entry2 = DataSeeder.CreateEntry(id: 7, userId: user.Id, foreign: "banana", translations: "банан");
            var state1 = DataSeeder.CreateState(id: 1, userId: user.Id, entryId: entry1.Id, repetitionCounter: 2, repetitionInterval: 4, ef: SM2Helper.InitEF);
            var state2 = DataSeeder.CreateState(id: 2, userId: user.Id, entryId: entry2.Id, repetitionCounter: 2, repetitionInterval: 4, ef: SM2Helper.InitEF);

            var existingSession = new RepetitionSession(user.Id, new List<RepetitionTask>() { new RepetitionTask(entry1, true), new RepetitionTask(entry2, false) });

            DbContext.RepetitionSessions.Add(existingSession);
            await DbContext.SaveChangesAsync();

            var sessionQueries = ServiceProvider.GetRequiredService<SessionQueries>();
            var sessionService = ServiceProvider.GetRequiredService<RepetitionSessionService>();


            // Act
            var result = await sessionService.FinishRepetitionSessionAsync(user.Id);


            // Assert
            Assert.True(result.IsSuccess);
            var repetitionResult = result.Value!;
            Assert.Null(await sessionQueries.GetByUserIdAsync(user.Id));
            Assert.Empty(await sessionQueries.GetTasksByUserIdAsync(user.Id));
            Assert.True(repetitionResult.StartedAt < repetitionResult.FinishedAt);
            Assert.Equal(2, repetitionResult.Total);
        }
    }
}
