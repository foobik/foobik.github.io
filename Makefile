.PHONY: install build build-css build-scss watch-css watch-scss serve clean help

help:
	@echo "install     - npm install (Tailwind/PostCSS + Sass build tooling)"
	@echo "build-css   - compile input.css -> css/styles.css (Tailwind/PostCSS)"
	@echo "build-scss  - compile tweaks.scss -> css/tweaks.css (Sass)"
	@echo "watch-css   - recompile css/styles.css on change"
	@echo "watch-scss  - recompile css/tweaks.css on change"
	@echo "build       - build-css + build-scss, then generate the site into skunk-html-output/"
	@echo "serve       - build, then serve skunk-html-output/ at http://localhost:8000"
	@echo "clean       - remove build artifacts (css/styles.css, css/tweaks.css, obj/, bin/)"

install:
	npm install

build-css:
	npx postcss input.css -o css/styles.css

build-scss:
	npx sass tweaks.scss:css/tweaks.css --no-source-map

watch-css:
	npx postcss input.css -o css/styles.css --watch

watch-scss:
	npx sass --watch tweaks.scss:css/tweaks.css --no-source-map

build: build-css build-scss
	dotnet run --project skunk-html.fsproj

serve: build
	python3 -m http.server 8000 --directory skunk-html-output

clean:
	rm -rf css/styles.css css/tweaks.css obj bin
