using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.SecurityFilters
{
    public class ValidationResult
    {
        public bool IsValid { get; private set; }
        public string DecodedTerm { get; private set; }
        public string ErrorMessage { get; private set; }
        public SecurityPattern ViolatedPattern { get; private set; }

        private ValidationResult() { }

        public static ValidationResult CreateSuccess(string decodedTerm)
        {
            return new ValidationResult
            {
                IsValid = true,
                DecodedTerm = decodedTerm
            };
        }

        public static ValidationResult CreateFailed(SecurityPattern pattern)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = string.Format("Search contains potentially dangerous content: {0}", pattern.Description),
                ViolatedPattern = pattern
            };
        }

        public static ValidationResult CreateFailed(string message)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = message
            };
        }
    }
}