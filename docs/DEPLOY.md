# Deploying the WebGL build to GitHub Pages

The WebGL player settings are already prepared for GitHub Pages:

- **Compression Format: Gzip**, with **Decompression Fallback** on. GitHub Pages can't send `Content-Encoding` headers, and the fallback lets the Unity loader decompress the files in JavaScript instead.
- **Template: Default**. The page title is the product name.

Build output stays out of `main` (`/Builds/` is gitignored). The build is published from a separate `gh-pages` branch that holds only the build files.

## One-time setup (after pushing `main` to GitHub)

From the repo root, create an empty `gh-pages` branch checked out next to the project:

```bash
git worktree add --orphan -b gh-pages ../PetPlatformer3D-pages
```

On GitHub, open **Settings > Pages**. Set **Source** to *Deploy from a branch*, then **Branch** to `gh-pages` / `(root)`.

## Every deploy

1. In Unity, build WebGL into `Builds/WebGL`.
2. Copy the build into the pages worktree, then commit and push it:

```bash
cp -r Builds/WebGL/. ../PetPlatformer3D-pages/ && touch ../PetPlatformer3D-pages/.nojekyll
```

```bash
git -C ../PetPlatformer3D-pages add -A && git -C ../PetPlatformer3D-pages commit -m "Deploy WebGL build" && git -C ../PetPlatformer3D-pages push -u origin gh-pages
```

The game is served at `https://<user>.github.io/<repo>/`.
