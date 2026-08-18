![GitHub repo size](https://img.shields.io/github/repo-size/mg0x7BE/skunk-html)
![GitHub License](https://img.shields.io/github/license/mg0x7BE/skunk-html)
![GitHub Created At](https://img.shields.io/github/created-at/mg0x7BE/skunk-html)
![GitHub forks](https://img.shields.io/github/forks/mg0x7BE/skunk-html)
![GitHub Repo stars](https://img.shields.io/github/stars/mg0x7BE/skunk-html)

# SkunkHTML

**The simplest way to run a blog on GitHub Pages.**

Fork this repo (or use it as a template). Enable GitHub Pages. You have a blog. That's it.

![SkunkHTML](https://mg0x7BE.github.io/skunk-html/images/skunk-final.png)

No CLI to install. No config files to learn. No build tools on your machine.
Write Markdown, push to GitHub, your site updates automatically.

**See it in action:** [Live demo](https://mg0x7BE.github.io/skunk-html/)

## Get started in 60 seconds

1. **Fork** this repository
2. Go to **Settings > Pages > Source: GitHub Actions**
3. Your blog is live at `https://YOUR-USERNAME.github.io/skunk-html/`

To publish a post: add a Markdown file to the `markdown-blog/` folder. The file name **is the date** - name it like `2025-03-24.md` and start the file with a `# Title` heading. The title comes from the heading, the date comes from the file name. Push. Done.

## Features

- **Zero local setup** - everything runs on GitHub Actions
- **Markdown -> HTML** - write in Markdown, get a clean website
- **RSS feed** - your readers can subscribe (`/feed.xml`)
- **Sitemap** - search engines find your content (`/sitemap.xml`)
- **SEO meta tags** - Open Graph and Twitter Cards out of the box
- **Structured data** - JSON-LD (BlogPosting, BreadcrumbList, WebSite) for richer search results, plus `robots.txt` and a `noindex`'d 404 page
- **[llms.txt](https://llmstxt.org)** - a curated index of your posts/pages for LLMs, plus `llms-full.txt` with every post's full text in one file
- **404 page** - a "not found" page in your site's style, generated automatically
- **Dark mode** - respects your visitors' system preference automatically
- **Templates** - switch design (and optionally HTML) with one `Config.fs` setting, or tweak CSS variables directly
- **Table of contents** - posts with 2+ headings get a sticky sidebar TOC with smooth-scroll anchor links and scroll-based active-section highlighting
- **Smart header** - hides on scroll down, slides back in and stays pinned on scroll up
- **Comments** - optional [Giscus](https://giscus.app/) integration
- **Syntax highlighting** - code blocks are highlighted automatically
- **Tailwind CSS + PostCSS** - styles are compiled from `input.css`, no framework CSS is hand-vendored
- **Two deployment modes** - GitHub builds everything (GitHub Actions), or you build locally and push the result (Deploy from a branch) - see below

## Customize your site

Edit `Config.fs` - you only need to change the values in quotes:

```fsharp
let siteTitle = "My Blog"
let siteDescription = "A blog powered by SkunkHTML"
let siteBaseUrl = "https://YOUR-USERNAME.github.io/skunk-html"  // No trailing slash
let siteLanguage = "en"
let siteAuthor = "Your Name"
let siteImage = "assets/avatar.jpg"  // Preview image for social shares
```

You don't need to know F# - just edit the text between the quotation marks. The same file also contains the interface texts (like the "blog entries" heading and the 404 page messages) - translate them if your blog is not in English.

**Base URL examples** - set `siteBaseUrl` to match where your site is hosted:
- GitHub project page: `https://YOUR-USERNAME.github.io/skunk-html`
- GitHub user page (repo named `<user>.github.io`): `https://YOUR-USERNAME.github.io`
- Custom domain: `https://example.com`
- Self-hosted with subpath: `https://example.com/blog`

### Templates

SkunkHTML ships with multiple templates - each one a self-contained folder under `themes/` bundling a Sass color theme and (optionally) its own HTML. To switch, set `theme` in `Config.fs` and rebuild (`npm run build:scss` or `make build-scss` - or just `npm run build` / `make build`):

```fsharp
let theme = "default"  // "default", "ocean", "terminal", or "ink" - see themes/
```

| Template | Set `theme` to | Style |
|----------|-----------------|-------|
| Default | `"default"` | Clean, minimal with dark mode |
| Ocean | `"ocean"` | Cool blue tones (GitHub-inspired) |
| Terminal | `"terminal"` | Green-on-dark hacker aesthetic |
| Ink | `"ink"` | Warm serif typography (newspaper-inspired) |

All templates respect `prefers-color-scheme` and get the site's full design - fixed header, post cards, pagination, table of contents, theme switcher, and so on - not just colors: `_components.scss` holds that shared design as a Sass mixin every template includes, and each `themes/<name>/theme.scss` is just a color map (plus a couple of `@include` calls into the shared helpers in `_palette.scss`) on top of it. Edit a template's `$light`/`$dark` maps directly if you want to tweak individual colors instead of switching templates entirely.

A template folder can also override the HTML: drop a `header.html`, `footer.html`, and/or `head.html` into `themes/<name>/` and it's used instead of the shared one in `html/` - anything you don't provide falls back to `html/` unchanged. None of the four built-in templates do this (they're palette-only), but you can add your own `themes/<name>/theme.scss` (following the shape of the existing ones) with HTML overrides alongside it, set `theme` to `"<name>"`, and it's picked up with no code changes.

### Content structure

- **Blog posts**: Markdown files in `markdown-blog/` whose names start with a digit. The file name is the publication date (e.g. `2025-03-24.md`). The post title comes from the first `# Heading` inside the file.
- **Other pages**: Markdown files in `markdown-blog/` that don't start with a digit (e.g. `about.md`, `featured.md`)
- **Front page**: `markdown-blog/index.md` - optional welcome content displayed above the post list

### HTML fragments

Customize the header, footer, and page head by editing files in `html/`:

- `header.html` - site navigation and logo
- `footer.html` - footer content
- `head.html` - meta tags, CSS links, and favicons
- `script_giscus.html` - Giscus comments configuration

A non-default template (see Templates above) can override `header.html`/`footer.html`/`head.html` by providing its own copy in `themes/<name>/` - see that section for details.

## Folder structure

```
skunk-html/
├── .github/workflows/    # GitHub Actions build & deploy
├── assets/               # Avatar, favicon, shared resources
├── css/                  # Generated stylesheets (styles.css + tweaks.css) - build output, not edited directly
├── fonts/                # Custom fonts
├── html/                 # HTML fragments (header, footer, head)
├── markdown-blog/        # Your content goes here
│   └── images/           # Images used in articles
├── scripts/              # Syntax highlighting script
├── themes/               # Alternative templates - one folder per template, see Templates below
├── tools/
│   ├── resolve-theme.js  # Picks the Sass entry file from Config.fs's `theme` setting
│   └── SeoCheck/         # SEO regression check (C#/AngleSharp), run in CI
├── input.css              # Tailwind CSS source (compiles to css/styles.css)
├── tweaks.scss             # "default" template, Sass source (compiles to css/tweaks.css)
├── _palette.scss           # Shared color-palette Sass mixins used by tweaks.scss and themes/*/theme.scss
├── _components.scss        # Shared site-design Sass mixin used by tweaks.scss and themes/*/theme.scss
├── package.json            # Tailwind CSS / PostCSS + Sass build tooling
├── postcss.config.js      # PostCSS config
├── Makefile                # make wrapper around npm/dotnet build commands
├── Config.fs              # Your site settings (title, URL, texts)
├── SkunkUtils.fs          # Utility functions
├── SkunkHtml.fs           # HTML generation engine
├── Program.fs             # Build entry point
└── skunk-html.fsproj      # F# project file
```

## Updating

To pick up engine improvements later, use GitHub's **Sync fork** button (or merge upstream manually). Everything that makes your blog yours lives in `Config.fs` (including which template is active), `tweaks.scss`, any `themes/<name>/` folders you've added or customized, and the content folders (`markdown-blog/`, `html/`, `assets/`, `fonts/`) - engine updates normally don't touch those, so syncing merges cleanly. `css/` is build output and isn't tracked in git except as part of `skunk-html-output/` (see Deployment modes below).

## Deployment modes

SkunkHTML supports both options under **Settings > Pages > Build and deployment > Source**:

- **GitHub Actions** (recommended, zero local setup) - `.github/workflows/main.yml` builds the CSS (Tailwind/PostCSS) and the site (F#/.NET) on every push to `main` and deploys automatically. You don't need anything installed locally.
- **Deploy from a branch** - you build locally (see below), then commit the regenerated `skunk-html-output/` folder and push. Point Pages at branch `main`, folder `/skunk-html-output`. Nothing runs in CI; GitHub Pages just serves the committed files.

Both modes read from the same source files, so you can switch between them any time in the repo settings without changing anything else.

## Build locally

Needed if you use "Deploy from a branch", or just want to preview changes before pushing. Requires [.NET](https://dotnet.microsoft.com/download) and [Node.js](https://nodejs.org/).

```bash
git clone https://github.com/mg0x7BE/skunk-html.git
cd skunk-html
npm install     # installs Tailwind CSS / PostCSS and Sass (once)
npm run build   # compiles input.css and the active template's CSS into css/, then runs the site generator
```

A `Makefile` mirrors the same commands if you prefer `make`: `make install`, `make build`, `make build-css` / `make build-scss`, `make watch-css` / `make watch-scss` (recompile on change), `make serve` (builds and serves `skunk-html-output/` at `localhost:8000`).

Your site ends up in `skunk-html-output/`. If you're using "Deploy from a branch", commit and push that folder after building.

## How it works

When you push a Markdown file to `markdown-blog/`, the build compiles `input.css` with Tailwind CSS and PostCSS into `css/styles.css`, and the active template's Sass (`tools/resolve-theme.js` picks `tweaks.scss` or a `themes/<name>/theme.scss` based on `Config.fs`'s `theme` setting) into `css/tweaks.css`. Then a small F# program converts your Markdown to HTML using [FSharp.Formatting](https://github.com/fsprojects/FSharp.Formatting) - reading `header.html`/`footer.html`/`head.html` from the template's folder if it overrides them, `html/` otherwise - generates RSS and sitemap, and wraps everything in a clean layout. Depending on your chosen Pages deployment mode (see above), this either runs on GitHub Actions automatically, or locally on your machine before you push.

`input.css` (the base layer that used to be MVP.css) goes through Tailwind/PostCSS. `tweaks.scss` and the template files in `themes/*/theme.scss` go through Sass - they hold the actual color palette on top of `_components.scss`'s shared site design, via `_palette.scss`'s shared mixins.

The entire build engine is ~400 lines of F#. No frameworks. No plugins beyond the Tailwind and Sass CSS builds. No magic.

## Contributing

Suggestions, bug reports, and pull requests welcome. Use [discussions](https://github.com/mg0x7BE/skunk-html/discussions), [issues](https://github.com/mg0x7BE/skunk-html/issues), or PRs.

## License

[Unlicense](https://en.wikipedia.org/wiki/Unlicense) - do whatever you want with it.

## Dependencies

- [Tailwind CSS](https://tailwindcss.com/) + [PostCSS](https://postcss.org/) - base layer styling (build-time only, via `npm`)
- [Sass](https://sass-lang.com/) (Dart Sass) - theme/palette styling (build-time only, via `npm`)
- [AngleSharp](https://anglesharp.github.io/) - HTML parsing for the SEO regression check below (build-time only, via NuGet)
- [microlight.js](https://github.com/asvd/microlight) - syntax highlighting
- [FSharp.Formatting](https://github.com/fsprojects/FSharp.Formatting) - Markdown processing

## SEO regression checks

`tools/SeoCheck` (`dotnet run --project tools/SeoCheck/SeoCheck.csproj -- skunk-html-output`, or `make seo-check`) is a small C#/AngleSharp console tool that parses every page in the built `skunk-html-output/` and fails (non-zero exit) if any page is missing a required tag or has one out of range. It runs in CI on every push/PR, right after the site is generated. Checked per page:

- `<title>` length (5-70 characters)
- meta description presence, uniqueness, and length (10-170 characters)
- `<img>` tags have an `alt` attribute
- `description`/`viewport` meta tags, plus `twitter:card`/`twitter:title`/`twitter:description`
- Open Graph tags: `og:title`, `og:description`, `og:type`, `og:url`
- `<link rel="canonical">` is present with a non-empty `href`
- exactly one `<h1>`

`404.html` is excluded (it's intentionally `noindex`'d). Thresholds are set from the site's own real content, so the check flags future regressions rather than pre-existing copy.

This runs on .NET rather than as an npm package on purpose: an earlier version used the [seo-analyzer](https://github.com/maddevsio/seo-analyzer) npm package, but its CLI binary is broken as published (missing files in the tarball), several of its default rules didn't fit SkunkHTML's URL scheme, and it pulled in old, vulnerable transitive dependencies (`request`, `sitemap-stream-parser`) for a URL-crawling feature this project never used. Since the whole check is really "parse HTML, look for a handful of tags" - work AngleSharp already does - and .NET is already required to build the site at all, there was no good reason to also require Node just for this.