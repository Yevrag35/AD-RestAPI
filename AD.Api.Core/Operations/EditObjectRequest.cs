using AD.Api.Collections;
using System.ComponentModel.DataAnnotations;

namespace AD.Api.Core.Operations;

public sealed class EditObjectRequest : IEditOperation, IValidatableObject
{
    public AddDictionary? Add { get; set; }
    public ClearDictionary? Clear { get; set; }
    public RemoveDictionary? Remove { get; set; }
    public ReplaceDictionary? Replace { get; set; }
    public SetDictionary? Set { get; set; }

    public int Count
    {
        [DebuggerStepThrough]
        get => this.GetCount();
    }

    public void ApplyToRequest(ModifyRequest request)
    {
        if (!this.Set.IsNullOrEmpty())
        {
            this.Set.ApplyToRequest(request);
        }

        if (!this.Remove.IsNullOrEmpty())
        {
            this.Remove.ApplyToRequest(request);
        }

        if (!this.Add.IsNullOrEmpty())
        {
            this.Add.ApplyToRequest(request);
        }

        if (!this.Replace.IsNullOrEmpty())
        {
            this.Replace.ApplyToRequest(request);
        }

        if (!this.Clear.IsNullOrEmpty())
        {
            this.Clear.ApplyToRequest(request);
        }
    }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (this.Add.IsNullOrEmpty() && this.Clear.IsNullOrEmpty() && this.Remove.IsNullOrEmpty() && this.Replace.IsNullOrEmpty() && this.Set.IsNullOrEmpty())
        {
            yield return new ValidationResult("At least one operation must be specified.", [nameof(this.Add), nameof(this.Clear), nameof(this.Remove), nameof(this.Replace), nameof(this.Set)]);
        }
    }

    [DebuggerStepThrough]
    private int GetCount()
    {
        int count = 0;
        GetCountFromDictionary(this.Add, ref count);
        GetCountFromDictionary(this.Clear, ref count);
        GetCountFromDictionary(this.Remove, ref count);
        GetCountFromDictionary(this.Replace, ref count);
        GetCountFromDictionary(this.Set, ref count);
        return count;
    }
    [DebuggerStepThrough]
    private static void GetCountFromDictionary<TCol>(TCol? operation, ref int count) where TCol : IReadOnlyCollection<DirectoryAttributeModification>
    {
        count += !operation.IsNullOrEmpty() ? operation.Count : 0;
    }
    [DebuggerStepThrough]
    private static void GetCountFromDictionary(ReplaceDictionary? operation, ref int count)
    {
        count += !operation.IsNullOrEmpty() ? operation.Count : 0;
    }
}
