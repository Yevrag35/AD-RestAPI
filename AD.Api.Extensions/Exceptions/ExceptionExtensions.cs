using AD.Api.Collections;
using AD.Api.Reflection;
using AD.Api.Spans;
using AD.Api.Statics;
using AD.Api.Strings;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace AD.Api.Exceptions;

public static class ExceptionExtensions
{
    public static IReadOnlyList<string> GetAllMessages(this Exception? exception)
    {
        if (exception is null)
        {
            return [];
        }

        List<string> messages = [GetExceptionWithTypeMessage(exception)];

        if (exception is AggregateException aggEx)
        {
            for (int i = 0; i < aggEx.InnerExceptions.Count; i++)
            {
                GetAllMessages(aggEx.InnerExceptions[i], messages, messagesIsWritable: true);
            }

            return messages;
        }

        GetAllMessages(exception.InnerException, messages, messagesIsWritable: true);
        return messages;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="exception"></param>
    /// <param name="messages"></param>
    /// <exception cref="ArgumentException"><paramref name="messages"/> cannot be added to.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="messages"/> is null.</exception>
    public static void GetAllMessages(this Exception? exception, ICollection<string> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);
        if (messages is not List<string> && messages.IsFixedSize())
        {
            throw new ArgumentException("The provided messages list is not writeable/expandable.", nameof(messages));
        }

        GetAllMessages(exception, messages, messagesIsWritable: true);
    }
    private static void GetAllMessages(Exception? exception, ICollection<string> messages, bool messagesIsWritable)
    {
        Debug.Assert(messagesIsWritable);
        if (exception is null)
        {
            return;
        }

        messages.Add(GetExceptionWithTypeMessage(exception));
        GetAllMessages(exception.InnerException, messages, messagesIsWritable: true);
    }

    private const string NO_MSG = "No message provided.";
    private readonly record struct MessageState([Required] string TypeName, [Required] string Message) : IStringCreatable<MessageState>
    {
        public readonly int Length => this.TypeName.Length + this.Message.Length + 2;
        private static ReadOnlySpan<char> GetSeparator() => [CharConstants.SEMI_COLON, CharConstants.SPACE];
        public readonly void WriteTo(scoped Span<char> chars)
        {
            this.TypeName.CopyTo(chars);
            int pos = this.TypeName.Length;
            GetSeparator().CopyToSlice(chars, ref pos);
            this.Message.CopyTo(chars.Slice(pos));
        }
    }

    private static string GetExceptionWithTypeMessage(Exception exception)
    {
        string exTypeName = exception.GetType().GetName();
        string message = !string.IsNullOrWhiteSpace(exception.Message)
            ? exception.Message
            : NO_MSG;

        return new MessageState(exTypeName, message).CreateString();
    }
}

