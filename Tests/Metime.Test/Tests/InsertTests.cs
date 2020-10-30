using Metime.Test.Utils;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Metime.Test.Tests
{
    public class InsertTests
    {
        private readonly EmployeeService _service;
        public InsertTests(EmployeeService employeeService)
        {
            _service = employeeService;
        }

        [Fact]
        public async Task InsertTest()
        {
            var emp = new Employee { 
                Name = "Metin",
                BirthDate = new DateTime(1991, 12, 8),
                CreatedAt = DateTime.Now,
                ShiftEnd = new TimeSpan(18,0,0),
                ShiftStart = new TimeSpan(9,0,0),
                Kind = Enums.TimezoneFormat.Local
            };
            await _service.InsertAsync(emp);
            Assert.False(emp.Id == 0);
        }
    }
}
