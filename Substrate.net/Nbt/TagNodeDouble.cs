﻿namespace Substrate.Nbt;

/// <summary>
/// An NBT node representing a double-precision floating point tag type.
/// </summary>
public sealed class TagNodeDouble : TagNode
{

    /// <summary>
    /// Converts the node to itself.
    /// </summary>
    /// <returns>A reference to itself.</returns>
    public override TagNodeDouble ToTagDouble()
    {
        return this;
    }

    /// <summary>
    /// Gets the tag type of the node.
    /// </summary>
    /// <returns>The TAG_DOUBLE tag type.</returns>
    public override TagType GetTagType()
    {
        return TagType.TAG_DOUBLE;
    }

    /// <summary>
    /// Gets or sets a double of tag data.
    /// </summary>
    public double Data { get; set; } = 0;

    /// <summary>
    /// Constructs a new double node with a data value of 0.0.
    /// </summary>
    public TagNodeDouble() { }

    /// <summary>
    /// Constructs a new double node.
    /// </summary>
    /// <param name="d">The value to set the node's tag data value.</param>
    public TagNodeDouble(double d)
    {
        Data = d;
    }

    /// <summary>
    /// Makes a deep copy of the node.
    /// </summary>
    /// <returns>A new double node representing the same data.</returns>
    public override TagNode Copy()
    {
        return new TagNodeDouble(Data);
    }

    /// <summary>
    /// Gets a string representation of the node's data.
    /// </summary>
    /// <returns>String representation of the node's data.</returns>
    public override string ToString()
    {
        return Data.ToString();
    }

    /// <summary>
    /// Converts a system float to a double node representing the same value.
    /// </summary>
    /// <param name="f">A float value.</param>
    /// <returns>A new double node containing the given value.</returns>
    public static implicit operator TagNodeDouble(float f) => new(f);

    /// <summary>
    /// Converts a system double to a double node representing the same value.
    /// </summary>
    /// <param name="d">A double value.</param>
    /// <returns>A new double node containing the given value.</returns>
    public static implicit operator TagNodeDouble(double d) => new(d);

    /// <summary>
    /// Converts a double node to a system double representing the same value.
    /// </summary>
    /// <param name="d">A double node.</param>
    /// <returns>A system double set to the node's data value.</returns>
    public static implicit operator double(TagNodeDouble d) => d.Data;
}