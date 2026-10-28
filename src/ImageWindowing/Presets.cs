namespace ImageWindowing;

/// <summary>Common CT window presets (center / width in HU).</summary>
public static class Presets
{
    public static readonly WindowLevel Brain = new(40, 80);
    public static readonly WindowLevel SoftTissue = new(40, 400);
    public static readonly WindowLevel Lung = new(-600, 1500);
    public static readonly WindowLevel Bone = new(400, 1800);

    public static IReadOnlyDictionary<string, WindowLevel> All { get; } =
        new Dictionary<string, WindowLevel>(StringComparer.OrdinalIgnoreCase)
        {
            ["brain"] = Brain,
            ["soft-tissue"] = SoftTissue,
            ["lung"] = Lung,
            ["bone"] = Bone,
        };

    public static bool TryGet(string name, out WindowLevel window) => All.TryGetValue(name, out window);
}
