// SEO regression gate for CI - run against the generated site
// (skunk-html-output/ by default) after `dotnet run --project skunk-html.fsproj`.
// Exits non-zero if any page fails a check below.
//
// Thresholds are set from the site's actual current content (see README),
// so this catches future regressions rather than flagging pre-existing copy.

using System.Linq;
using AngleSharp;
using AngleSharp.Dom;

var outputDir = args.Length > 0 ? args[0] : "skunk-html-output";

// 404.html is intentionally noindex'd (see SkunkHtml.fs) - it's not meant to
// rank, so it isn't held to the same SEO bar as real content.
var ignoreFiles = new HashSet<string> { "404.html" };

if (!Directory.Exists(outputDir))
{
    Console.Error.WriteLine($"Directory not found: {outputDir}");
    return 1;
}

var htmlFiles = Directory
    .GetFiles(outputDir, "*.html", SearchOption.AllDirectories)
    .Where(path => !ignoreFiles.Contains(Path.GetFileName(path)))
    .OrderBy(path => path)
    .ToList();

if (htmlFiles.Count == 0)
{
    Console.Error.WriteLine($"No HTML files found in {outputDir}");
    return 1;
}

var context = BrowsingContext.New(Configuration.Default);
var hadIssues = false;

foreach (var path in htmlFiles)
{
    var html = await File.ReadAllTextAsync(path);
    var document = await context.OpenAsync(req => req.Content(html));
    var issues = CheckPage(document).ToList();

    if (issues.Count == 0)
        continue;

    hadIssues = true;
    Console.WriteLine($"\n{path}");
    foreach (var issue in issues)
        Console.WriteLine($"  - {issue}");
}

if (hadIssues)
{
    Console.WriteLine("\nSEO check failed.");
    return 1;
}

Console.WriteLine($"SEO check passed ({htmlFiles.Count} page(s)).");
return 0;

static IEnumerable<string> CheckPage(IDocument document) =>
    CheckTitleLength(document, min: 5, max: 70)
        .Concat(CheckMetaDescription(document, min: 10, max: 170))
        .Concat(CheckImageAltAttributes(document))
        .Concat(CheckRequiredMetaByName(document, "viewport", "twitter:card", "twitter:title", "twitter:description"))
        .Concat(CheckRequiredMetaByProperty(document, "og:title", "og:description", "og:type", "og:url"))
        .Concat(CheckCanonicalLink(document))
        .Concat(CheckSingleH1(document));

static IEnumerable<string> CheckTitleLength(IDocument document, int min, int max)
{
    var title = document.Title?.Trim() ?? "";
    if (title.Length == 0)
    {
        yield return "Missing <title>";
        yield break;
    }

    if (title.Length < min)
        yield return $"<title> too short ({title.Length} chars, minimum {min})";
    if (title.Length > max)
        yield return $"<title> too long ({title.Length} chars, maximum {max})";
}

static IEnumerable<string> CheckMetaDescription(IDocument document, int min, int max)
{
    var descriptions = document.QuerySelectorAll("head > meta[name=description]");
    if (descriptions.Length == 0)
    {
        yield return "Missing <meta name=\"description\">";
        yield break;
    }

    if (descriptions.Length > 1)
        yield return $"Multiple <meta name=\"description\"> tags found ({descriptions.Length})";

    var content = descriptions[0].GetAttribute("content")?.Trim() ?? "";
    if (content.Length < min || content.Length > max)
        yield return $"Meta description length ({content.Length}) should be between {min} and {max} characters";
}

static IEnumerable<string> CheckImageAltAttributes(IDocument document)
{
    var missing = document.QuerySelectorAll("img").Count(img => string.IsNullOrEmpty(img.GetAttribute("alt")));
    if (missing > 0)
        yield return $"{missing} <img> tag(s) missing an alt attribute";
}

static IEnumerable<string> CheckRequiredMetaByName(IDocument document, params string[] names)
{
    foreach (var name in names)
    {
        var meta = document.QuerySelector($"head > meta[name=\"{name}\"]");
        if (meta is null)
            yield return $"Missing <meta name=\"{name}\">";
        else if (string.IsNullOrWhiteSpace(meta.GetAttribute("content")))
            yield return $"<meta name=\"{name}\"> has empty content";
    }
}

static IEnumerable<string> CheckRequiredMetaByProperty(IDocument document, params string[] properties)
{
    foreach (var property in properties)
    {
        var meta = document.QuerySelector($"head > meta[property=\"{property}\"]");
        if (meta is null)
            yield return $"Missing <meta property=\"{property}\">";
        else if (string.IsNullOrWhiteSpace(meta.GetAttribute("content")))
            yield return $"<meta property=\"{property}\"> has empty content";
    }
}

static IEnumerable<string> CheckCanonicalLink(IDocument document)
{
    var canonical = document.QuerySelector("head > link[rel=canonical]");
    if (canonical is null)
    {
        yield return "Missing <link rel=\"canonical\">";
        yield break;
    }

    if (string.IsNullOrWhiteSpace(canonical.GetAttribute("href")))
        yield return "<link rel=\"canonical\"> has an empty href";
}

static IEnumerable<string> CheckSingleH1(IDocument document)
{
    var count = document.QuerySelectorAll("h1").Length;
    if (count == 0)
        yield return "Missing <h1>";
    else if (count > 1)
        yield return $"Multiple <h1> tags found ({count})";
}
