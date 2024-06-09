using System;

namespace AD.Api.Attributes
{
    /// <summary>
    /// An interface identifying an attribute as an AD Api attribute used within this solution. This is useful 
    /// for identifying custom attribute classes versus Microsoft or other library attributes.  
    /// Using this interface directly is not required when deriving from <see cref="AdApiAttribute"/>.
    /// </summary>
    public interface IAdApiAttribute
    {
    }

    /// <summary>
    /// The base class for all AD Api solution-related attributes. Provides an easy way to identify custom attributes
    /// versus Microsoft or other library attributes.
    /// </summary>
    public abstract class AdApiAttribute : Attribute, IAdApiAttribute
    {
    }
}

