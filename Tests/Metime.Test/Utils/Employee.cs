using Metime.Attributes;
using Metime.Enums;
using Metime.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Metime.Test.Utils
{
    public class Employee : BaseEntity
    {
        public string Name { get; set; }

        [IgnoreTimezone]
        public DateTime BirthDate { get; set; }
        public TimeSpan ShiftStart { get; set; }
        public TimeSpan? ShiftEnd { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
