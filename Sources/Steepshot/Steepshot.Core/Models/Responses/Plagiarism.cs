using System;

namespace Steepshot.Core.Models.Responses
{
    public class Plagiarism
    {
        public bool IsPlagiarism { get; set; }

        public string PlagiarismUsername { get; set; }

        public string PlagiarismPermlink { get; set; }
    }
}
