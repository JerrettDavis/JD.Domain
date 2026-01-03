namespace JD.Domain.Diff;

/// <summary>
/// Classifies changes as breaking or non-breaking.
/// </summary>
public sealed class BreakingChangeClassifier
{
    /// <summary>
    /// Determines if removing an entity is a breaking change.
    /// </summary>
    public bool IsEntityRemovalBreaking() => true;

    /// <summary>
    /// Determines if adding an entity is a breaking change.
    /// </summary>
    public bool IsEntityAdditionBreaking() => false;

    /// <summary>
    /// Determines if removing a property is a breaking change.
    /// </summary>
    public bool IsPropertyRemovalBreaking() => true;

    /// <summary>
    /// Determines if adding a property is a breaking change.
    /// </summary>
    /// <param name="isRequired">Whether the new property is required.</param>
    public bool IsPropertyAdditionBreaking(bool isRequired) => isRequired;

    /// <summary>
    /// Determines if changing a property type is a breaking change.
    /// </summary>
    public bool IsPropertyTypeChangeBreaking() => true;

    /// <summary>
    /// Determines if changing the required status of a property is a breaking change.
    /// </summary>
    /// <param name="wasOptional">Whether the property was previously optional.</param>
    /// <param name="isNowRequired">Whether the property is now required.</param>
    public bool IsRequiredChangeBreaking(bool wasOptional, bool isNowRequired)
    {
        // Breaking if changing from optional to required
        return wasOptional && isNowRequired;
    }

    /// <summary>
    /// Determines if changing key properties is a breaking change.
    /// </summary>
    public bool IsKeyChangeBreaking() => true;

    /// <summary>
    /// Determines if removing a value object is a breaking change.
    /// </summary>
    public bool IsValueObjectRemovalBreaking() => true;

    /// <summary>
    /// Determines if removing an enum is a breaking change.
    /// </summary>
    public bool IsEnumRemovalBreaking() => true;

    /// <summary>
    /// Determines if removing an enum value is a breaking change.
    /// </summary>
    public bool IsEnumValueRemovalBreaking() => true;

    /// <summary>
    /// Determines if adding an index is a breaking change.
    /// </summary>
    public bool IsIndexAdditionBreaking() => false;

    /// <summary>
    /// Determines if removing an index is a breaking change.
    /// </summary>
    public bool IsIndexRemovalBreaking() => false;

    /// <summary>
    /// Determines if rule changes are breaking.
    /// </summary>
    public bool IsRuleChangeBreaking() => false;
}
