﻿using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Maple2.Model.Common;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
public readonly record struct Color(byte Blue, byte Green, byte Red, byte Alpha) {
    public override string ToString() => $"ARGB({Alpha:X2}, {Red:X2}, {Green:X2}, {Blue:X2})";
}


[StructLayout(LayoutKind.Sequential, Size = 8)]
public readonly struct SkinColor {
    public Color Primary { get; }
    public Color Secondary { get; }

    public SkinColor(Color color) {
        Primary = color;
        Secondary = color;
    }

    [JsonConstructor]
    public SkinColor(Color primary, Color secondary) {
        Primary = primary;
        Secondary = secondary;
    }

    public override string ToString() => $"Primary:{Primary}|Secondary:{Secondary}";
}


[StructLayout(LayoutKind.Sequential, Size = 20)]
public readonly struct EquipColor {
    public Color Primary { get; }
    public Color Secondary { get; }
    public Color Tertiary { get; }
    public int Index { get; }

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly int Unknown = 0;

    public EquipColor(Color color) {
        this.Primary = color;
        this.Secondary = color;
        this.Tertiary = color;
        this.Index = -1;
    }

    [JsonConstructor]
    public EquipColor(Color primary, Color secondary, Color tertiary, int index = -1) {
        this.Primary = primary;
        this.Secondary = secondary;
        this.Tertiary = tertiary;
        this.Index = index;
    }

    public override string ToString() =>
        $"Primary:{Primary}|Secondary:{Secondary}|Tertiary:{Tertiary}|Index:{Index}";
}
