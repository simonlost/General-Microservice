using System;
using System.Collections.Generic;

namespace GeneralInsurance.Interfaces.Models
{
    public class SearchResult
    {
        public bool CustomerFound { get; set; }
        public int? AccessNumber { get; set; }
        public List<Error> Errors { get; set; }
        public Guid CorollationId { get; set; }
        public bool Created { get; set; }
    }
}