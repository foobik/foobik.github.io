module Config

open System.IO

// --- Site metadata (edit these for your site) ---
// Only change the values in quotes - the rest is just labels.
let siteTitle = "SkunkHTML"
let siteDescription = "The blog on GitHub Pages."
let siteBaseUrl = "https://foobik.github.io"  // No trailing slash. Include repo name if using project pages.
let siteLanguage = "ru"
let siteAuthor = "(x)"  // Optional, used in RSS feed and meta tags
let siteImage = "assets/avatar.png"  // Optional, preview image for social shares (og:image), relative to site root. "" disables

let theme = "test"  // Template: "default", or a folder name under themes/ (e.g. "ocean", "terminal", "ink"). Changes both the CSS design and any HTML it overrides. Rebuild after changing this.

// --- Interface text (translate these if your blog is not in English) ---
let blogEntriesHeading = "blog entries"  // Section heading above the post list on the front page
let publishedOnText = "Published on"  // Shown before the date at the end of each post
let untitledPageTitle = "No Title"  // Fallback title for pages without a # heading
let notFoundTitle = "Page not found"  // Browser tab title of the 404 page
let notFoundMessage = "This page does not exist or has been moved."  // 404 page text
let notFoundBackText = "Back to the front page"  // 404 page link text
let readMoreText = "Read more"  // Link text on each card on the front page
let newerPostsText = "Newer"  // Pagination link to the previous (more recent) page
let olderPostsText = "Older"  // Pagination link to the next (older) page
let noPostsYetText = "No posts yet - check back soon!"  // Shown on the front page when there are no articles
let tableOfContentsHeading = "Table of contents"  // Sidebar heading above a post's table of contents (shown when a post has 2+ headings)

// --- Pagination ---
let articlesPerPage = 6  // How many post cards appear on each front-page listing before paginating

// --- Folder layout (you normally don't need to touch these) ---
let sourceDir = __SOURCE_DIRECTORY__

let markdownDir = Path.Combine(sourceDir, "markdown-blog")
let htmlDir = Path.Combine(sourceDir, "html")
let outputDir = Path.Combine(sourceDir, "skunk-html-output")

// The active template's folder, if `theme` names one that exists and has a
// theme.scss (the same definition of "a real template" tools/resolve-theme.js
// uses for the CSS side - keeping both in sync avoids a template's CSS and
// HTML disagreeing about whether it's actually selected). See html/ fragments
// below and SkunkUtils.fs's Disk.readThemedFile. None for "default" (or an
// unrecognized/incomplete name), meaning html/ is used for everything.
let themeDir =
    let dir = Path.Combine(sourceDir, "themes", theme)
    if theme <> "default" && File.Exists(Path.Combine(dir, "theme.scss")) then Some dir else None

let cssDir = Path.Combine(sourceDir, "css")
let outputCssDir = Path.Combine(outputDir, "css")

let fontsDir = Path.Combine(sourceDir, "fonts")
let outputFontsDir = Path.Combine(outputDir, "fonts")

let imagesDir = Path.Combine(markdownDir, "images")
let outputImagesDir = Path.Combine(outputDir, "images")

let assetsDir = Path.Combine(sourceDir, "assets")
let outputAssetsDir = Path.Combine(outputDir, "assets")

let scriptsDir = Path.Combine(sourceDir, "scripts")
let outputScriptsDir = Path.Combine(outputDir, "scripts")

let frontPageMarkdownFileName = "index.md"
