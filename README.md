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
- **Themes** - choose from built-in color themes or tweak CSS variables
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

### Themes

SkunkHTML ships with multiple color themes, written in Sass (`.scss`). `tweaks.scss` is the active theme; it compiles to `css/tweaks.css`, which is what `html/head.html` actually links. To switch themes, copy the contents of a theme file from `themes/` into `tweaks.scss`, then rebuild (`npm run build:scss` or `make build-scss`):

| Theme | File | Style |
|-------|------|-------|
| Default | `tweaks.scss` | Clean, minimal with dark mode |
| Ocean | `themes/theme-ocean.scss` | Cool blue tones (GitHub-inspired) |
| Terminal | `themes/theme-terminal.scss` | Green-on-dark hacker aesthetic |
| Ink | `themes/theme-ink.scss` | Warm serif typography (newspaper-inspired) |

All themes respect `prefers-color-scheme` - they look great in both light and dark mode. Each theme file is just a Sass color map plus a couple of `@include` calls into the shared helpers in `_palette.scss` - edit the `$light`/`$dark` maps directly if you want to tweak individual colors instead of swapping the whole theme.

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
├── themes/               # Alternative color themes (Sass sources, see Themes below)
├── input.css              # Tailwind CSS source (compiles to css/styles.css)
├── tweaks.scss             # Active theme, Sass source (compiles to css/tweaks.css)
├── _palette.scss           # Shared Sass mixins used by tweaks.scss and themes/*.scss
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

To pick up engine improvements later, use GitHub's **Sync fork** button (or merge upstream manually). Everything that makes your blog yours lives in `Config.fs`, `tweaks.scss`, and the content folders (`markdown-blog/`, `html/`, `assets/`, `fonts/`) - engine updates normally don't touch those, so syncing merges cleanly. `css/` is build output and isn't tracked in git except as part of `skunk-html-output/` (see Deployment modes below).

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
npm run build   # compiles input.css and tweaks.scss into css/, then runs the site generator
```

A `Makefile` mirrors the same commands if you prefer `make`: `make install`, `make build`, `make build-css` / `make build-scss`, `make watch-css` / `make watch-scss` (recompile on change), `make serve` (builds and serves `skunk-html-output/` at `localhost:8000`).

Your site ends up in `skunk-html-output/`. If you're using "Deploy from a branch", commit and push that folder after building.

## How it works

When you push a Markdown file to `markdown-blog/`, the build compiles `input.css` with Tailwind CSS and PostCSS into `css/styles.css`, and `tweaks.scss` with Sass into `css/tweaks.css`. Then a small F# program converts your Markdown to HTML using [FSharp.Formatting](https://github.com/fsprojects/FSharp.Formatting), generates RSS and sitemap, and wraps everything in a clean layout. Depending on your chosen Pages deployment mode (see above), this either runs on GitHub Actions automatically, or locally on your machine before you push.

`input.css` (the base layer that used to be MVP.css) goes through Tailwind/PostCSS. `tweaks.scss` and the theme files in `themes/` go through Sass - they hold the actual color palette and component styling on top of Tailwind's base layer, via `_palette.scss`'s shared mixins.

The entire build engine is ~400 lines of F#. No frameworks. No plugins beyond the Tailwind and Sass CSS builds. No magic.

## Contributing

Suggestions, bug reports, and pull requests welcome. Use [discussions](https://github.com/mg0x7BE/skunk-html/discussions), [issues](https://github.com/mg0x7BE/skunk-html/issues), or PRs.

## License

[Unlicense](https://en.wikipedia.org/wiki/Unlicense) - do whatever you want with it.

## Dependencies

- [Tailwind CSS](https://tailwindcss.com/) + [PostCSS](https://postcss.org/) - base layer styling (build-time only, via `npm`)
- [Sass](https://sass-lang.com/) (Dart Sass) - theme/palette styling (build-time only, via `npm`)
- [microlight.js](https://github.com/asvd/microlight) - syntax highlighting
- [FSharp.Formatting](https://github.com/fsprojects/FSharp.Formatting) - Markdown processing