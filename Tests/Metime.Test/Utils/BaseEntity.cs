using Metime.Attributes;
using Metime.Enums;
using Metime.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Metime.Test.Utils
{
    public class BaseEntity : ITimezoneConvertible
    {
        [Key]
        public long Id { get; set; }

        [NotMapped] public TimezoneFormat Kind { get; set; } = TimezoneFormat.UTC;
    }
}
