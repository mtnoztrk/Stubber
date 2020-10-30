using Metime.Attributes;
using System.Threading.Tasks;

namespace Metime.Test.Utils
{
    [ConvertTimezone]
    public class EmployeeService
    {
        private readonly TestDbContext _dbContext;
        public EmployeeService(TestDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Employee> InsertAsync (Employee user)
        {
            await _dbContext.Employees.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }
    }
}
