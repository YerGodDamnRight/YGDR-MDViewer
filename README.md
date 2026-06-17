
<h1 style="text-align: center;">YGDR Markdown Viewer</h1>

<p align="center"><strong>Rendered markdown · Inline editing · Search · GIF images · Syntax highlighting · Programmatic API</strong></p>

<p align="center">
  <a href="https://unity.com"><img src="https://img.shields.io/badge/Unity-2022.3_LTS-black?logo=unity" alt="Unity"></a>
  <a href="LICENSE.md"><img src="https://img.shields.io/badge/License-GPLv3-green" alt="License"></a>
  <a href="https://claude.ai"><img src="https://img.shields.io/badge/Built_with-Claude-blueviolet?logo=anthropic" alt="Built with Claude"></a>
</p>

<p align="center">
  <a href="#installation">Install</a> ·
  <a href="#features">Features</a> ·
  <a href="#usage">Usage</a> ·
  <a href="#api">API</a> ·
  <a href="https://github.com/YerGodDamnRight/YGDR-MDViewer/releases/latest">Releases</a> ·
  <a href="LICENSE.md">License</a>
</p>

---

A Unity Editor extension that renders Markdown files as rich formatted documents — directly in the Editor. Supports inline editing, search, images (including animated GIFs), syntax-highlighted code blocks, and a full programmatic API for opening docs from your own tools.

---

## Features

<details>
<summary><strong>Markdown Rendering</strong></summary>

- Headings H1–H6
- **Bold**, *italic*, ***bold italic***, ~~strikethrough~~, `inline code`
- Blockquotes (nested) and GitHub Alerts (`NOTE` `TIP` `IMPORTANT` `WARNING` `CAUTION`)
- Ordered, unordered, and mixed nested lists
- Task lists — `[x]` checked · `[-]` blocked · `[ ]` open — inline or in lists
- Tables with left / center / right column alignment
- Horizontal rules
- Collapsible `<details>` / `<summary>` blocks
- `<kbd>` keyboard shortcut rendering
- Links — external URLs, same-page anchors, relative file links, auto-links, hover tooltips

</details>

<details>
<summary><strong>Images & GIFs</strong></summary>

- Local paths (relative to the `.md` file) and remote URLs
- Animated GIF playback in-Editor
- Resize via title string: `"width=300 height=200"`
- Hover tooltip via `alt=hover text here`
- Supported formats: `png` `jpg` `gif` `bmp` `tga` `tiff` `webp` `psd`
- Paths with spaces wrapped in `<angle brackets>`

</details>

<details>
<summary><strong>Syntax Highlighting</strong></summary>

Fenced code blocks highlight automatically when a language is specified:

- `csharp` / `cs`
- `json`

</details>

<details>
<summary><strong>Inline Raw Editor</strong></summary>

Toggle to raw markdown source from any rendered view. Large files are paginated in chunks — use the Prev / Next toolbar to navigate. **Ctrl+S** saves to disk without leaving the window.

</details>

<details>
<summary><strong>Search</strong></summary>

**Ctrl+F** opens the search bar. Results highlight in-document with match count and Prev / Next navigation. **Esc** closes.

</details>

<details>
<summary><strong>Navigation</strong></summary>

- Anchor links (`#section-heading`) smooth-scroll to the target heading
- Relative file links open the linked `.md` in the same window

</details>

<details>
<summary><strong>Dark & Light Skin</strong></summary>

Auto-matches Unity's editor theme. Override any time in **Edit → Preferences → YGDR MDV**.

</details>

---

## Installation

1. Import via the [VPM repo](https://yergoddamnright.github.io/YGDR-VPM-Listing/) **or** download the latest `.unitypackage` from the [releases page](https://github.com/YerGodDamnRight/YGDR-MDViewer/releases/latest) and import into your project

**Requirements:**
- Unity 2022.3 LTS

---

## Usage

Select any `.md` file in the Project window — it renders automatically in the Unity Inspector. No setup required.

To edit, click the **Edit** button in the viewer toolbar to toggle raw mode. **Ctrl+S** saves.

---

## API

Open a Markdown file from code — useful for embedding help docs in your own Editor tools:

```csharp
using YGDR.MDV;

// Minimal — path or GUID
MDViewer.Open("Assets/Docs/README.md");
MDViewer.Open("a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4"); // GUID — stable across file moves

// Full options
MDViewer.Open(
    "Assets/Docs/README.md",
    anchor:   "section-heading",  // scroll to anchor on open
    title:    "My Docs",          // window title override
    lineMin:  10,                 // first line to display
    lineMax:  50,                 // last line to display
    editable: false               // disable editing (default: true)
);
```

---

## MDV Generator Window

Open via **YGDR → MDV Generator** in the Unity menu bar.

A helper window for building `MDViewer.Open()` calls without writing code:

- Drag any `.md` asset into the field to auto-populate its path and GUID
- Copy the GUID directly for use in stable, move-safe references
- Set anchor, title, line range, and editable flag interactively
- Preview and copy the generated call with one click
- **Cheat Sheet** button opens the full MDV syntax reference

---

## License

[GNU General Public License v3.0](LICENSE.md)

---

## 3rd Party Credits

[Third Party Notices](Packages/com.ygdr.mdv/Third%20Party%20Notices.md)

---

<p align="center"><sub>by <a href="https://github.com/YerGodDamnRight">YerGodDamnRight</a> · Developed with AI assistance (<a href="https://claude.ai">Claude</a> / Anthropic)</sub></p>
