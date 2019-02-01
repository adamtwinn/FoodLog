using FoodLog.Api.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FoodLog.Api
{
    public static class DbInitialization
    {
        public static void Initialize(IConfiguration configuration)
        {
            using (var context = new FoodContext(configuration))
            {
                context.Database.EnsureCreated();
                context.Database.Migrate();
            }
        }
    }
}
