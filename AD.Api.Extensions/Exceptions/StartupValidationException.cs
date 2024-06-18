using AD.Api.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AD.Api.Exceptions
{
    public sealed class StartupValidationException : AdApiException
    {
        public Type ForType { get; }
        public IReadOnlyList<ValidationResult> ValidationResults { get; }

        private StartupValidationException(Type forType, IReadOnlyList<ValidationResult> results)
            : base(FormatMessage(forType, results))
        {
            this.ForType = forType;
            this.ValidationResults = results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="results"></param>
        /// <exception cref="StartupValidationException"></exception>
        public static void ThrowIfNotEmpty<T>(IReadOnlyList<ValidationResult> results)
        {
            if (results.Count > 0)
            {
                throw new StartupValidationException(typeof(T), results);
            }
        }

        private static string FormatMessage(Type forType, IReadOnlyList<ValidationResult> results)
        {
            StringBuilder builder = new(500);
            builder.Append(string.Format(Errors.Exception_ValidationStartup, forType.GetName()))
                .AppendLine()
                .Append('\t');

            ValidationResult first = results[0];
            WriteResult(builder, first);
            
            for (int i = 1; i < results.Count; i++)
            {
                builder.AppendLine().Append('\t');
                WriteResult(builder, results[i]);
            }

            return builder.ToString();
        }

        private static void WriteResult(StringBuilder builder, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                return;
            }

            Span<char> extras = ['[', ']', ',', ' ', '-', '>', ' '];
            Span<char> commaSpace = extras.Slice(2, 2);
            Span<char> separator = extras.Slice(3);

            string[] names = result.MemberNames.ToArray();
            if (names.Length <= 0)
            {
                builder.Append(extras[0]);
                string firstName = names[0];
                builder.Append(firstName);
                for (int i = 1; i < names.Length; i++)
                {
                    builder.Append(commaSpace);
                    builder.Append(names[i]);
                }

                builder.Append(extras[1]);
                builder.Append(separator);
            }

            builder.Append(result.ErrorMessage);
        }
    }
}

