module SkunkHtml

open SkunkUtils
open System
open System.Collections.Generic
open System.Globalization
open System.IO
open System.Text.RegularExpressions
open FSharp.Formatting.Markdown

/// A single markdown page. Blog posts carry a publication date taken from the
/// file name (e.g. 2025-03-24.md); other pages (about, links, ...) do not.
type Page =
    { SourcePath: string
      Title: string
      Link: string
      Description: string
      Date: string option }

let private baseUrl = Url.normalizeBaseUrl Config.siteBaseUrl

let private headTemplate =
    Disk.readThemedFile Config.htmlDir Config.themeDir "head.html"

let private highlightingScript =
    Path.Combine(Config.htmlDir, "script_syntax_highlighting.html")
    |> Disk.readFile

let private giscusScript =
    Path.Combine(Config.htmlDir, "script_giscus.html")
    |> Disk.readFile

let private tocScript =
    Path.Combine(Config.htmlDir, "script_toc.html")
    |> Disk.readFile

/// A table of contents entry: heading level (2 or 3), its anchor id, and its
/// plain-text label.
type private TocEntry = { Level: int; Id: string; Text: string }

/// Adds `id` attributes to h2/h3 headings in `html` (slugified from their text,
/// de-duplicated on collision) and returns the modified HTML alongside the
/// extracted table of contents, in document order.
let private injectTableOfContents (html: string) : string * TocEntry list =
    let toc = ResizeArray<TocEntry>()
    let slugCounts = Dictionary<string, int>()

    let uniqueSlug (baseSlug: string) =
        match slugCounts.TryGetValue(baseSlug) with
        | true, count ->
            slugCounts[baseSlug] <- count + 1
            $"{baseSlug}-{count + 1}"
        | false, _ ->
            slugCounts[baseSlug] <- 1
            baseSlug

    let evaluator (m: Match) =
        let level = int m.Groups[1].Value
        let inner = m.Groups[2].Value
        let text = Regex.Replace(inner, "<[^>]+>", "").Trim()
        let id = uniqueSlug (Url.toUrlFriendly text)
        toc.Add({ Level = level; Id = id; Text = text })
        $"""<h{level} id="{id}">{inner}</h{level}>"""

    let updatedHtml =
        Regex.Replace(html, @"<h([23])>(.*?)</h\1>", MatchEvaluator(evaluator), RegexOptions.Singleline)

    updatedHtml, List.ofSeq toc

/// Sidebar table of contents markup, or "" when there aren't enough headings
/// to make one worthwhile.
let private tableOfContentsHtml (toc: TocEntry list) =
    if toc.Length < 2 then
        ""
    else
        let items =
            toc
            |> List.map (fun entry ->
                let levelClass = if entry.Level = 3 then " toc__item--h3" else ""
                $"""<li class="toc__item{levelClass}"><a href="#{entry.Id}">{Xml.escape entry.Text}</a></li>""")
            |> String.concat "\n"

        $"""
        <aside class="toc" aria-label="{Xml.escape Config.tableOfContentsHeading}">
            <p class="toc__title">{Xml.escape Config.tableOfContentsHeading}</p>
            <ul class="toc__list">{items}</ul>
        </aside>
        """

// --- Structured data (JSON-LD) ---
// Appended into page content (Google reads JSON-LD from anywhere in the
// document, not just <head>) alongside the other per-page scripts below.

let private jsonLdScript (json: string) =
    "<script type=\"application/ld+json\">\n" + json + "\n</script>"

let private websiteStructuredData () =
    "{\n"
    + "  \"@context\": \"https://schema.org\",\n"
    + "  \"@type\": \"WebSite\",\n"
    + "  \"name\": \"" + Json.escape Config.siteTitle + "\",\n"
    + "  \"description\": \"" + Json.escape Config.siteDescription + "\",\n"
    + "  \"inLanguage\": \"" + Config.siteLanguage + "\",\n"
    + "  \"url\": \"" + baseUrl + "/\"\n"
    + "}"
    |> jsonLdScript

let private articleStructuredData (page: Page) (canonicalUrl: string) (date: string) =
    let imageField =
        if String.IsNullOrWhiteSpace(Config.siteImage) then
            ""
        else
            let imageUrl = $"{baseUrl}/{Config.siteImage.TrimStart('/')}"
            "  \"image\": \"" + imageUrl + "\",\n"

    let authorField =
        if String.IsNullOrWhiteSpace(Config.siteAuthor) then
            ""
        else
            "  \"author\": { \"@type\": \"Person\", \"name\": \"" + Json.escape Config.siteAuthor + "\" },\n"

    "{\n"
    + "  \"@context\": \"https://schema.org\",\n"
    + "  \"@type\": \"BlogPosting\",\n"
    + "  \"headline\": \"" + Json.escape page.Title + "\",\n"
    + "  \"description\": \"" + Json.escape page.Description + "\",\n"
    + "  \"datePublished\": \"" + date + "\",\n"
    + "  \"dateModified\": \"" + date + "\",\n"
    + "  \"inLanguage\": \"" + Config.siteLanguage + "\",\n"
    + "  \"mainEntityOfPage\": { \"@type\": \"WebPage\", \"@id\": \"" + canonicalUrl + "\" },\n"
    + imageField
    + authorField
    + "  \"publisher\": { \"@type\": \"Organization\", \"name\": \"" + Json.escape Config.siteTitle + "\" }\n"
    + "}"
    |> jsonLdScript

let private breadcrumbStructuredData (page: Page) (canonicalUrl: string) =
    "{\n"
    + "  \"@context\": \"https://schema.org\",\n"
    + "  \"@type\": \"BreadcrumbList\",\n"
    + "  \"itemListElement\": [\n"
    + "    { \"@type\": \"ListItem\", \"position\": 1, \"name\": \""
    + Json.escape Config.blogEntriesHeading
    + "\", \"item\": \""
    + baseUrl
    + "/\" },\n"
    + "    { \"@type\": \"ListItem\", \"position\": 2, \"name\": \""
    + Json.escape page.Title
    + "\", \"item\": \""
    + canonicalUrl
    + "\" }\n"
    + "  ]\n"
    + "}"
    |> jsonLdScript

let generateFinalHtml (head: string) (header: string) (footer: string) (content: string) (script: string) =
    $"""
    <!DOCTYPE html>
    <html lang="{Config.siteLanguage}" data-color-mode="user">
    <head>
        {head}
    </head>
    <body>
        <header>
            {header}
        </header>
        <main>
            {content}
        </main>
        <hr>
        <footer>
            {footer}
        </footer>
        <script>
            {script}
        </script>
    </body>
    </html>
    """

let head (titleSuffix: string) (description: string) (canonicalUrl: string) (ogType: string) =
    let fullTitle = Config.siteTitle + titleSuffix

    let seoMeta =
        let desc = if String.IsNullOrWhiteSpace(description) then Config.siteDescription else description
        let authorMeta =
            if String.IsNullOrWhiteSpace(Config.siteAuthor) then ""
            else $"""<meta name="author" content="{Xml.escape Config.siteAuthor}">"""
        let imageMeta =
            if String.IsNullOrWhiteSpace(Config.siteImage) then ""
            else
                let imageUrl = $"{baseUrl}/{Config.siteImage.TrimStart('/')}"
                $"""<meta property="og:image" content="{imageUrl}">
        <meta name="twitter:image" content="{imageUrl}">"""
        $"""
        <meta name="description" content="{Xml.escape desc}">
        {authorMeta}
        <meta property="og:title" content="{Xml.escape fullTitle}">
        <meta property="og:description" content="{Xml.escape desc}">
        <meta property="og:type" content="{ogType}">
        <meta property="og:url" content="{canonicalUrl}">
        {imageMeta}
        <meta name="twitter:card" content="summary">
        <meta name="twitter:title" content="{Xml.escape fullTitle}">
        <meta name="twitter:description" content="{Xml.escape desc}">
        <link rel="canonical" href="{canonicalUrl}">
        <link rel="alternate" type="application/rss+xml" title="{Xml.escape Config.siteTitle}" href="{baseUrl}/feed.xml">
        """

    headTemplate.Replace("{{site-title}}", Xml.escape fullTitle) + seoMeta

let private isArticle (file: string) =
    Char.IsDigit(Path.GetFileName(file)[0])

let loadPage (markdownFilePath: string) : Page =
    let lines = File.ReadAllLines(markdownFilePath)

    let title =
        lines
        |> Array.tryFind _.StartsWith("# ")
        |> Option.map _.TrimStart('#').Trim()
        |> Option.defaultValue Config.untitledPageTitle

    let description =
        lines
        |> Array.filter (fun line -> not (String.IsNullOrWhiteSpace(line)))
        |> Array.filter (fun line -> not (line.StartsWith("#")))
        |> Array.tryHead
        |> Option.defaultValue ""
        |> MarkdownUtils.stripMarkdownSyntax
        |> fun s -> if s.Length > 160 then s[..159] + "..." else s

    let date =
        if isArticle markdownFilePath then
            Some(Path.GetFileNameWithoutExtension(markdownFilePath))
        else
            None

    { SourcePath = markdownFilePath
      Title = title
      Link = Url.toUrlFriendly title + ".html"
      Description = description
      Date = date }

let createPage (header: string) (footer: string) (page: Page) =
    let canonicalUrl = $"{baseUrl}/{page.Link}"
    let markdownContent = File.ReadAllText(page.SourcePath)

    let htmlContent, ogType =
        match page.Date with
        | None -> Markdown.ToHtml(markdownContent), "website"
        | Some date ->
            let publicationDate =
                $"""<p class="publication-date">{Config.publishedOnText} <time datetime="{date}">{date}</time></p>"""
            let bodyHtml, toc = Markdown.ToHtml(markdownContent) |> injectTableOfContents

            let articleHtml =
                if toc.Length < 2 then
                    bodyHtml + "\n\n" + publicationDate
                else
                    $"""
                    <div class="article-layout">
                        <article class="article-content">
                            {bodyHtml}
                            {publicationDate}
                        </article>
                        {tableOfContentsHtml toc}
                    </div>
                    """

            let tocScriptIfAny = if toc.Length < 2 then "" else tocScript
            let structuredData = articleStructuredData page canonicalUrl date + breadcrumbStructuredData page canonicalUrl
            articleHtml + giscusScript + tocScriptIfAny + structuredData, "article"

    let finalHtmlContent =
        generateFinalHtml (head (" - " + page.Title) page.Description canonicalUrl ogType) header footer htmlContent highlightingScript

    printfn $"Processing {Path.GetFileName page.SourcePath} ->"
    Disk.writeFile (Path.Combine(Config.outputDir, page.Link)) finalHtmlContent

/// Renders a single article as a clickable card for the front-page listing.
let private articleCardHtml (article: Page) =
    let dateHtml =
        match article.Date with
        | Some date -> $"""<time class="card-date" datetime="{date}">{date}</time>"""
        | None -> ""

    let excerptHtml =
        if String.IsNullOrWhiteSpace(article.Description) then ""
        else $"""<p class="card-excerpt">{Xml.escape article.Description}</p>"""

    $"""
    <li class="card">
        <a class="card-link" href="{article.Link}">
            {dateHtml}
            <h3 class="card-title">{Xml.escape article.Title}</h3>
            {excerptHtml}
            <span class="card-more">{Config.readMoreText} →</span>
        </a>
    </li>
    """

/// index.html for page 1, pageN.html for every subsequent page.
let private indexPageFileName (pageNumber: int) =
    if pageNumber = 1 then "index.html" else $"page{pageNumber}.html"

let private paginationHtml (currentPage: int) (totalPages: int) =
    if totalPages <= 1 then
        ""
    else
        let prevHtml =
            if currentPage > 1 then
                $"""<a class="pagination__link" href="{indexPageFileName (currentPage - 1)}" rel="prev">← {Config.newerPostsText}</a>"""
            else
                $"""<span class="pagination__link pagination__link--disabled">← {Config.newerPostsText}</span>"""

        let nextHtml =
            if currentPage < totalPages then
                $"""<a class="pagination__link" href="{indexPageFileName (currentPage + 1)}" rel="next">{Config.olderPostsText} →</a>"""
            else
                $"""<span class="pagination__link pagination__link--disabled">{Config.olderPostsText} →</span>"""

        $"""
        <nav class="pagination" aria-label="Pagination">
            {prevHtml}
            <span class="pagination__status">{currentPage} / {totalPages}</span>
            {nextHtml}
        </nav>
        """

/// Splits the articles across paginated front-page listings (index.html, page2.html, ...).
/// Returns the total number of pages generated, so the caller can list them in the sitemap.
let createIndexPages (header: string) (footer: string) (articles: Page list) =
    let frontPageMarkdownFilePath = Path.Combine(Config.markdownDir, Config.frontPageMarkdownFileName)

    let frontPageContentHtml =
        if File.Exists(frontPageMarkdownFilePath) then
            printfn $"Processing {Path.GetFileName frontPageMarkdownFilePath} ->"
            Markdown.ToHtml(File.ReadAllText(frontPageMarkdownFilePath))
        else
            printfn $"Warning! File {Config.frontPageMarkdownFileName} does not exist! The main page will only contain blog entries, without a welcome message"
            ""

    let pageChunks =
        if articles.IsEmpty then [ [] ]
        else articles |> List.chunkBySize (max 1 Config.articlesPerPage)

    let totalPages = pageChunks.Length

    pageChunks
    |> List.iteri (fun index chunk ->
        let pageNumber = index + 1
        let fileName = indexPageFileName pageNumber

        let welcomeHtml = if pageNumber = 1 then frontPageContentHtml else ""

        let listHtml =
            if articles.IsEmpty then
                $"""<p class="empty-state">{Config.noPostsYetText}</p>"""
            else
                let cardsHtml = chunk |> List.map articleCardHtml |> String.concat "\n"
                $"""<ul class="card-grid">{cardsHtml}</ul>{paginationHtml pageNumber totalPages}"""

        let structuredData = if pageNumber = 1 then websiteStructuredData () else ""

        let content =
            $"""
            {welcomeHtml}
            <section class="publications">
                <h2>{Config.blogEntriesHeading}</h2>
                {listHtml}
            </section>
            {structuredData}
            """

        let canonicalUrl = if pageNumber = 1 then baseUrl + "/" else $"{baseUrl}/{fileName}"
        let titleSuffix = if pageNumber = 1 then "" else $" - {Config.blogEntriesHeading} {pageNumber}"
        let finalHtmlContent =
            generateFinalHtml (head titleSuffix Config.siteDescription canonicalUrl "website") header footer content highlightingScript

        Disk.writeFile (Path.Combine(Config.outputDir, fileName)) finalHtmlContent)

    totalPages

/// Auto-generated 404 page, served by GitHub Pages for any missing URL.
/// The <base> tag makes relative links work at any path depth. Written before
/// the regular pages, so an explicit page with the same file name wins.
let createNotFoundPage (header: string) (footer: string) =
    let content =
        $"""
        <h1>404</h1>
        <p>{Config.notFoundMessage}</p>
        <p><a href="index.html">{Config.notFoundBackText}</a></p>
        """

    let baseTag = $"""<base href="{baseUrl}/">"""
    // GitHub Pages serves this file with HTTP 200 (it's a static file, not a
    // real 404 response), so search engines need this meta tag to know not
    // to index it as real content.
    let robotsMeta = """<meta name="robots" content="noindex, follow">"""
    let headContent = baseTag + robotsMeta + head (" - " + Config.notFoundTitle) "" $"{baseUrl}/404.html" "website"
    let finalHtmlContent = generateFinalHtml headContent header footer content highlightingScript

    Disk.writeFile (Path.Combine(Config.outputDir, "404.html")) finalHtmlContent

// --- RSS Feed Generation ---

/// RFC 1123 date for RSS; falls back to the raw string when the file name is not a date
let private toRssDate (date: string) =
    match DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None) with
    | true, parsed -> parsed.ToString("R")
    | _ -> date

let createRssFeed (articles: Page list) =
    let authorElement =
        if String.IsNullOrWhiteSpace(Config.siteAuthor) then ""
        else $"    <managingEditor>{Xml.escape Config.siteAuthor}</managingEditor>\n"

    let items =
        articles
        |> List.map (fun article ->
            let pubDateElement =
                match article.Date with
                | Some date -> $"      <pubDate>{toRssDate date}</pubDate>\n"
                | None -> ""
            let description =
                if String.IsNullOrWhiteSpace(article.Description) then article.Title else article.Description
            "    <item>\n"
            + $"      <title>{Xml.escape article.Title}</title>\n"
            + $"      <link>{baseUrl}/{article.Link}</link>\n"
            + $"      <guid>{baseUrl}/{article.Link}</guid>\n"
            + pubDateElement
            + $"      <description>{Xml.escape description}</description>\n"
            + "    </item>")
        |> String.concat "\n"

    let lastBuildDate = DateTime.UtcNow.ToString("R")

    let feed =
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
        + "<rss version=\"2.0\" xmlns:atom=\"http://www.w3.org/2005/Atom\">\n"
        + "  <channel>\n"
        + $"    <title>{Xml.escape Config.siteTitle}</title>\n"
        + $"    <link>{baseUrl}</link>\n"
        + $"    <description>{Xml.escape Config.siteDescription}</description>\n"
        + $"    <language>{Config.siteLanguage}</language>\n"
        + $"    <lastBuildDate>{lastBuildDate}</lastBuildDate>\n"
        + authorElement
        + $"    <atom:link href=\"{baseUrl}/feed.xml\" rel=\"self\" type=\"application/rss+xml\" />\n"
        + items + "\n"
        + "  </channel>\n"
        + "</rss>"

    Disk.writeFile (Path.Combine(Config.outputDir, "feed.xml")) feed

// --- Sitemap Generation ---
let createSitemap (pages: Page list) =
    let entries =
        pages
        |> List.map (fun page ->
            let lastmod =
                match page.Date with
                | Some date -> $"    <lastmod>{date}</lastmod>\n"
                | None -> ""
            "  <url>\n"
            + $"    <loc>{baseUrl}/{page.Link}</loc>\n"
            + lastmod
            + "  </url>")
        |> String.concat "\n"

    let sitemap =
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
        + "<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n"
        + "  <url>\n"
        + $"    <loc>{baseUrl}/</loc>\n"
        + "  </url>\n"
        + entries + "\n"
        + "</urlset>"

    Disk.writeFile (Path.Combine(Config.outputDir, "sitemap.xml")) sitemap

// --- robots.txt ---
let createRobotsTxt () =
    let robotsTxt =
        "User-agent: *\n"
        + "Allow: /\n"
        + "\n"
        + $"Sitemap: {baseUrl}/sitemap.xml\n"

    Disk.writeFile (Path.Combine(Config.outputDir, "robots.txt")) robotsTxt

// --- llms.txt (https://llmstxt.org) ---
// A curated, LLM-friendly index of the site: title, one-line summary, and a
// linked list of posts/pages with their descriptions. llms-full.txt below is
// the companion file it points to - every post's raw Markdown in one file.
let createLlmsTxt (articles: Page list) (otherPages: Page list) =
    let linkList (pages: Page list) =
        pages
        |> List.map (fun page -> $"- [{page.Title}]({baseUrl}/{page.Link}): {page.Description}")
        |> String.concat "\n"

    let postsSection =
        if articles.IsEmpty then "" else "\n\n## Posts\n\n" + linkList articles

    let pagesSection =
        if otherPages.IsEmpty then "" else "\n\n## Pages\n\n" + linkList otherPages

    let llmsTxt =
        $"# {Config.siteTitle}\n\n"
        + $"> {Config.siteDescription}"
        + postsSection
        + pagesSection
        + "\n\n## Full text\n\n"
        + $"For the complete content of every post in a single file, see [llms-full.txt]({baseUrl}/llms-full.txt)."

    Disk.writeFile (Path.Combine(Config.outputDir, "llms.txt")) llmsTxt

/// Raw markdown source of a page, formatted as one llms-full.txt section.
/// Markdown files already open with a `# Title` heading, so it doubles as
/// this section's header - no separate one is added.
let private llmsFullTxtSection (page: Page) =
    let publishedLine =
        match page.Date with
        | Some date -> $"\n\n_Published on {date}_"
        | None -> ""

    File.ReadAllText(page.SourcePath).Trim() + publishedLine + "\n\n---\n"

let createLlmsFullTxt (articles: Page list) (otherPages: Page list) =
    let llmsFullTxt =
        $"# {Config.siteTitle} - full text\n\n"
        + $"> {Config.siteDescription}\n\n"
        + $"Generated from {baseUrl}\n\n"
        + "---\n\n"
        + ((articles @ otherPages) |> List.map llmsFullTxtSection |> String.concat "\n")

    Disk.writeFile (Path.Combine(Config.outputDir, "llms-full.txt")) llmsFullTxt
