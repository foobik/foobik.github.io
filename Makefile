.PHONY: install build build-css build-scss watch-css watch-scss serve seo-check clean help

help:
	@echo "install     - npm install (Tailwind/PostCSS + Sass build tooling)"
	@echo "build-css   - compile input.css -> css/styles.css (Tailwind/PostCSS)"
	@echo "build-scss  - compile the template selected by Config.fs's \`theme\` setting -> css/tweaks.css (Sass)"
	@echo "watch-css   - recompile css/styles.css on change"
	@echo "watch-scss  - recompile css/tweaks.css on change"
	@echo "build       - build-css + build-scss, then generate the site into skunk-html-output/"
	@echo "seo-check   - run the SEO regression gate (tools/SeoCheck) against skunk-html-output/"
	@echo "serve       - build, then serve skunk-html-output/ at http://localhost:8000"
	@echo "clean       - remove build artifacts (css/styles.css, css/tweaks.css, obj/, bin/)"

install:
	npm install

build-css:
	npx postcss input.css -o css/styles.css

build-scss:
	node tools/resolve-theme.js

watch-css:
	npx postcss input.css -o css/styles.css --watch

# Note: watches tweaks.scss only - it does not know about Config.fs's
# `theme` setting. If you switch templates while watch-scss is running, stop
# it and run `make build-scss` once (or restart watch-scss) to pick up the
# new template's entry file.
watch-scss:
	npx sass --watch tweaks.scss:css/tweaks.css --no-source-map

build: build-css build-scss
	dotnet run --project skunk-html.fsproj

seo-check:
	dotnet run --project tools/SeoCheck/SeoCheck.csproj -- skunk-html-output

serve: build
	python3 -m http.server 8000 --directory skunk-html-output

clean:
	rm -rf css/styles.css css/tweaks.css obj bin
