using System.Collections.Generic;

namespace Mint.Gdk.Utilities.Runtime
{
    public static class ValidatorExtensions
    {
        public static Validator<T> Chain<T>(this Validator<T> first, params Validator<T>[] others)
        {
            var current = first;
            foreach (var next in others)
            {
                current.SetNext(next);
                current = next;
            }
            return first;
        }
        public static Validator<T> BuildChain<T>(IEnumerable<Validator<T>> validators)
        {
            Validator<T> first = null;
            Validator<T> current = null;
            foreach (var validator in validators)
            {
                if (first == null)
                    first = validator;
                else
                    current.SetNext(validator);
                current = validator;
            }
            return first;
        }
    }

    public abstract class Validator<T>
    {
        protected Validator<T> _nextValidator;

        public Validator<T> SetNext(Validator<T> nextValidator)
        {
            _nextValidator = nextValidator;
            return nextValidator;
        }

        public virtual ValidationResult Validate(T input)
        {
            var result = DoValidate(input);

            if (result.IsValid && _nextValidator != null)
            {
                return _nextValidator.Validate(input);
            }

            return result;
        }

        protected abstract ValidationResult DoValidate(T input);
    }
    public struct ValidationResult
    {
        public bool IsValid { get; }
        public string ErrorMessage { get; }
        public object Context { get; } // Optional context for validation, can be used to provide additional information

        public ValidationResult(bool isValid, string errorMessage = null, object context = null)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
            Context = context;
        }

        public static ValidationResult Success() => new ValidationResult(true);
        public static ValidationResult Fail(string error, object context = null) => new ValidationResult(false, error, context);
    }
}
