using AD.Api.Attributes.Services;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Serialization.Json;
using AD.Api.Reflection;
using AD.Api.Serialization.Json;
using AD.Api.Statics;
using AD.Api.Strings.Spans;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Buffers;
using System.Collections;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Serialization
{
    [DependencyRegistration(Lifetime = ServiceLifetime.Scoped)]
    [JsonConverter(typeof(CollectionResponseConverter))]
    public sealed class CollectionResponse : IActionResult, IDisposable
    {
        public const string JsonContentType = "application/json; charset=utf-8";

        private Array? _array;
        private bool _disposed;
        private readonly ArrayPoolReturnMethodCache _methodCache;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool _needsDisposal;
        private IEnumerable? _data;

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public bool AddResultCode { get; set; } = true;
        
        public int Count { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Guid? ContinueKey { get; set; }

        public IEnumerable Data
        {
            [DebuggerStepThrough]
            get => _data ??= Array.Empty<object>();
            [DebuggerStepThrough]
            set => _data = value;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Error { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? ErrorCode { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        [MemberNotNullWhen(true, nameof(_array))]
        internal bool NeedsDisposal => _needsDisposal;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? NextPageUrl { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public JsonSerializerOptions Options { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Enum? Result { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public int StatusCode { get; set; } = StatusCodes.Status200OK;

        public CollectionResponse(IOptions<JsonOptions> jsonOptions, ArrayPoolReturnMethodCache methodCache)
        {
            this.Options = jsonOptions.Value.JsonSerializerOptions;
            _methodCache = methodCache;
            _data = Array.Empty<object>();
            _needsDisposal = false;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            Debug.Assert(!_needsDisposal || !_disposed);
            HttpResponse response = context.HttpContext.Response;
            response.ContentType = JsonContentType;
            response.StatusCode = this.GetStatusCode();

            await JsonSerializer.SerializeAsync(response.Body, this, this.Options, 
                context.HttpContext.RequestAborted)
                .ConfigureAwait(false);
        }

        private int GetStatusCode()
        {
            int statusCode = this.StatusCode;
            return statusCode > 99 && statusCode < 600
                ? statusCode
                : StatusCodes.Status200OK;
        }

        public void SetCookie(HttpContext httpContext, in Guid cookie)
        {
            this.ContinueKey = cookie;

            SpanStringBuilder builder = new(stackalloc char[12 + LengthConstants.GUID_FORM_D]);

            builder = builder.Append("/search?continueKey=")
                             .Append(cookie, LengthConstants.GUID_FORM_D);

            this.NextPageUrl = builder.Build();
        }
        public void SetData<T>(IReadOnlyCollection<T> collection)
        {
            _needsDisposal = false;
            this.Data = collection;
            this.Count = collection.Count;
        }
        public void SetData(DirectoryResponse response, ResultEntryCollection collection)
        {
            _needsDisposal = false;
            this.Result = response.ResultCode;
            this.AddResultCode = true;
            this.Error = response.ResultCode != ResultCode.Success ? response.ErrorMessage : null;
            this.Data = collection;
            this.Count = collection.Count;
        }
        public void SetData<T>(IEnumerable<T> enumerable, int maxCount)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (!enumerable.TryGetNonEnumeratedCount(out int enumCount))
            {
                T[] array = ArrayPool<T>.Shared.Rent(maxCount);
                int count = 0;
                foreach (T item in enumerable)
                {
                    if (count == array.Length)
                    {
                        Debug.Fail("Array is too small.");
                        T[] newArray = ArrayPool<T>.Shared.Rent(array.Length * 2);
                        Array.Copy(array, newArray, array.Length);

                        ArrayPool<T>.Shared.Return(array);
                    }

                    array[count++] = item;
                }

                _needsDisposal = true;
                _array = array;
                this.Count = count;
                this.Data = array;
            }
            else
            {
                _needsDisposal = false;
                this.Count = enumCount;
                this.Data = enumerable;
            }
        }

        #region DISPOSAL

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!_disposed && _needsDisposal)
            {
                if (disposing && _array is not null)
                {
                    _methodCache.Return(_array, null, clearArray: false);
                }

                _array = null;
                _data = null;
                _disposed = true;
                _needsDisposal = false;
            }
        }

        #endregion
}
    }

