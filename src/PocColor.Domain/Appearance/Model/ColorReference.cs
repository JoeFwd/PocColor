using System;

namespace PocColor.Domain.Appearance.Model;

public sealed class ColorReference : IEquatable<ColorReference>
{
    private ColorReference(ColorReferenceKind kind, uint? argb, int? paletteId)
    {
        Kind = kind;
        Argb = argb;
        PaletteId = paletteId;
    }

    public ColorReferenceKind Kind { get; }

    public uint? Argb { get; }

    public int? PaletteId { get; }

    public static ColorReference FromArgb(uint value)
    {
        return new ColorReference(ColorReferenceKind.Argb, value, null);
    }

    public static ColorReference FromPaletteId(int value)
    {
        return new ColorReference(ColorReferenceKind.PaletteId, null, value);
    }

    public bool Equals(ColorReference other)
    {
        return other != null
            && Kind == other.Kind
            && Argb == other.Argb
            && PaletteId == other.PaletteId;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as ColorReference);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = (int)Kind;
            hash = (hash * 397) ^ (Argb.HasValue ? Argb.Value.GetHashCode() : 0);
            hash = (hash * 397) ^ (PaletteId.HasValue ? PaletteId.Value : 0);
            return hash;
        }
    }
}
