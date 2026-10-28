using System.Globalization;
using System.Text.Json;
using ImageWindowing;

return args.FirstOrDefault() switch
{
    "render" => Render(args.Skip(1).ToArray()),
    "golden" => Golden(args.Skip(1).ToArray()),
    _ => Usage(),
};

static int Usage()
{
    Console.WriteLine("""
        Usage:
          imgwin render --preset <brain|soft-tissue|lung|bone> [--size 256] [--out image.png]
          imgwin render --center <HU> --width <HU> [--size 256] [--out image.png]
          imgwin golden [--out testdata/lut-golden.json]

        render  Generates the synthetic head phantom, applies the window and writes a PNG.
        golden  Writes the lookup-table hashes that both test suites use as regression data.
        """);
    return 1;
}

static int Render(string[] args)
{
    var options = ParseOptions(args);
    var size = options.TryGetValue("size", out var s) ? int.Parse(s, CultureInfo.InvariantCulture) : 256;
    var output = options.GetValueOrDefault("out", "phantom.png");

    WindowLevel window;
    if (options.TryGetValue("preset", out var preset))
    {
        if (!Presets.TryGet(preset, out window))
        {
            Console.Error.WriteLine($"Unknown preset '{preset}'. Choose one of: {string.Join(", ", Presets.All.Keys)}.");
            return 2;
        }
    }
    else if (options.TryGetValue("center", out var c) && options.TryGetValue("width", out var w))
    {
        window = new WindowLevel(double.Parse(c, CultureInfo.InvariantCulture), double.Parse(w, CultureInfo.InvariantCulture));
    }
    else
    {
        return Usage();
    }

    var pixels = WindowingTransform.Apply(HeadPhantom.Generate(size), window);
    PngWriter.Write(output, pixels, size, size);
    Console.WriteLine($"Wrote {output} ({size}x{size}, {window}).");
    return 0;
}

static int Golden(string[] args)
{
    var output = ParseOptions(args).GetValueOrDefault("out", "testdata/lut-golden.json");
    var entries = Presets.All.ToDictionary(
        p => p.Key,
        p => new { center = p.Value.Center, width = p.Value.Width, sha256 = LookupTableHash.Compute(p.Value) });

    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
    File.WriteAllText(output, JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true }) + "\n");
    Console.WriteLine($"Wrote {output}.");
    return 0;
}

static Dictionary<string, string> ParseOptions(string[] args)
{
    var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (var i = 0; i + 1 < args.Length; i += 2)
    {
        if (!args[i].StartsWith("--"))
            throw new ArgumentException($"Expected an option name but got '{args[i]}'.");
        options[args[i][2..]] = args[i + 1];
    }
    return options;
}
