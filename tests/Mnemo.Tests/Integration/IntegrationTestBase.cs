using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mnemo.Data;
using Mnemo.Services;
using Mnemo.Services.Handlers;
using Mnemo.Services.Queries;

namespace Mnemo.Tests.Integration
{
    public class IntegrationTestBase : IDisposable
    {
        protected IServiceProvider ServiceProvider { get; private set; }
        protected AppDbContext DbContext {  get; private set; }
        protected TestDataSeeder DataSeeder { get; private set; }


        public IntegrationTestBase()
        {
            var services = new ServiceCollection();

            // Add InMemory AppDataContext
            var dbName = Guid.NewGuid().ToString();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));

            // DI Queries
            services.AddScoped<AccountQueries>();
            services.AddScoped<SessionQueries>();
            services.AddScoped<StateQueries>();
            services.AddScoped<VocabularyQueries>();

            // DI Handlers
            services.AddScoped<RepetitionResultHandler>();

            // DI Services
            services.AddScoped<AccountManagementService>();
            services.AddScoped<RepetitionSessionService>();
            services.AddScoped<RepetitionStateService>();
            services.AddScoped<VocabularyManagementService>();

            ServiceProvider = services.BuildServiceProvider();

            DbContext = ServiceProvider.GetRequiredService<AppDbContext>();
            DataSeeder = new TestDataSeeder(DbContext);
        }


        public void Dispose()
        {
            DbContext.Database.EnsureDeleted();
            DbContext.Dispose();
        }
    }
}
