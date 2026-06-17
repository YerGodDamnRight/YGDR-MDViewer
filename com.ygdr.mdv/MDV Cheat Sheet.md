# MDV Cheat Sheet

---

## Opening MDV from code

```csharp
using YGDR.MDV;

MDViewer.Open("Assets/path/to/file.md");
MDViewer.Open("a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4"); // GUID — stable across file moves

MDViewer.Open(
    "Assets/path/to/file.md",
    anchor:   "section-heading",  // Jumps to anchor section on open
    title:    "My Doc",	          // Sets window title
    lineMin:  10,	                // Sets document beginning
    lineMax:  50,	                // Sets document end
    editable: false	              // set to diable editing, enabled by default
);
```

---

## Headings

```
# H1  ## H2  ### H3  #### H4  ##### H5  ###### H6
```

# H1
## H2
### H3
#### H4
##### H5
###### H6

---

## Text Formatting

```
**bold**   *italic*   ***bold italic***   ~~strikethrough~~   `inline code`
```

**bold**   *italic*   ***bold italic***   ~~strikethrough~~   `inline code`

---

## Links

```
[External](https://unity.com)
[With tooltip](https://unity.com "Hover text")
[Same-page anchor](#table)
[Auto-link](https://unity.com)
https://unity.com
```

[External](https://unity.com)   [With tooltip](https://unity.com "Hover text")   [Jump to Table](#table)   https://unity.com

---

## Images

```
![alt](path/to/image.png)
![alt](<file with spaces.png>)
![alt](https://example.com/image.png)
![alt](animation.gif)
![alt](path/to/image.png "width=300 height=200 alt=hover text here")
![alt](path/to/image.png "width=64")
```

- Supported types: `png` `jpg` `gif` `bmp` `tga` `tiff` `webp` `psd`
- Paths are relative to the .md file — `images/photo.png` or `./images/photo.png`
- Wrap paths with spaces in `<angle brackets>`
- Remote URLs and GIFs supported
- `width=` / `height=` optional and independent
- `alt=` sets hover tooltip, consumes rest of string
- Legacy size-only still works: `"=300x200"`

---

## Lists

```
- Item
  - Nested
    - Double nested

1. First
2. Second
   1. Nested ordered

- Unordered
  1. Mixed ordered inside
```

- Item
  - Nested
    - Double nested

1. First
2. Second
   1. Nested ordered

---

## Task Lists

```
- [x] Checked
- [-] Blocked
- [ ] Open
  - [x] Nested checked
```

- [x] Checked
- [-] Blocked
- [ ] Open
  - [x] Nested checked

---

## Standalone Checkboxes

Use `[x]` `[-]` `[ ]` anywhere outside a list:

```
Status: [x] Done — blocked [-] waiting — review [ ] pending
```

Status: [x] Done — blocked [-] waiting — review [ ] pending

---

## Blockquote

```
> Outer quote.
> > Nested quote.
> > > Triple nested.
```

> Outer quote.
> > Nested quote.
> > > Triple nested.

---

## GitHub Alerts

```
> [!NOTE]
> Informational.

> [!TIP]
> Suggestion.

> [!IMPORTANT]
> Critical.

> [!WARNING]
> Potential issue.

> [!CAUTION]
> Dangerous action.
```

> [!NOTE]
> Informational.

> [!TIP]
> Suggestion.

> [!IMPORTANT]
> Critical.

> [!WARNING]
> Potential issue.

> [!CAUTION]
> Dangerous action.

---

## Keyboard Shortcuts

```
<kbd>Ctrl</kbd>+<kbd>C</kbd>
```

<kbd>Ctrl</kbd>+<kbd>C</kbd>   <kbd>F2</kbd>   <kbd>Esc</kbd>

---

## Code Blocks

````
```csharp
Debug.Log("hello");
```
````

```csharp
Debug.Log("hello");
```

---

## Collapsible

```
<details>
<summary>Click to expand</summary>

Content here, supports full markdown.

</details>
```

<details>
<summary>Click to expand</summary>

Content here, supports **bold**, `code`, lists, and code blocks.

</details>

---

## Table

```
| Left     | Center   | Right     |
|:---------|:--------:|----------:|
| cell     | cell     | cell      |
```

| Left     | Center   | Right     |
|:---------|:--------:|----------:|
| cell     | cell     | cell      |

---

## Horizontal Rule

```
---
```

---
