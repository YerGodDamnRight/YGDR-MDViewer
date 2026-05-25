# H1 Heading

## H2 Heading

### H3 Heading

#### H4 Heading

##### H5 Heading

###### H6 Heading

---

## Paragraphs & Inline Styles

Plain paragraph text. Lorem ipsum dolor sit amet, consectetur adipiscing elit.

**Bold** — *italic* — ***bold italic*** — ~~strikethrough~~ — `inline code` — Text with a [hyperlink](https://unity.com) — [anchor link](#code-blocks).

---

## Images

Remote image:
![Unity badge](https://img.shields.io/badge/Unity-2022.3_LTS-black.png?logo=unity)

Gifs Supported:
![gif](<gif_ref.gif>)
Local image:
![Controller reference](<controller_ref.jpg>)

### Image Sizing

Width only — 64px wide, height auto:
![Controller reference](<controller_ref.jpg> "=64")

Width + height — 128×64px (stretches):
![Controller reference](<controller_ref.jpg> "=128x64")

Title + size — tooltip "My Title", 96px wide:
![Controller reference](<controller_ref.jpg> "My Title =96")

---

## Horizontal Rule

---

## Lists

### Unordered

- Item one
- Item two
  - Nested item A
  - Nested item B
    - Double-nested item
- Item three

### Ordered

1. First item
2. Second item
   1. Nested ordered A
   2. Nested ordered B
3. Third item

### Mixed Nesting

- Unordered top
  1. Ordered inside unordered
  2. Second ordered
     - Back to unordered deep

---

## Blockquote

> Outer blockquote.
> > Inner nested blockquote.
> > > Triple nested.

---

## GitHub Alerts

> [!NOTE]
> Informational content.

> [!TIP]
> Helpful suggestion.

> [!IMPORTANT]
> Critical information.

> [!WARNING]
> Potential issue.

> [!CAUTION]
> Dangerous action warning.

---

## Table

| Feature | Status | Notes |
|:---|:---:|---:|
| **Bold** | `code` | *italic* |
| ~~strikethrough~~ | [link](https://unity.com) | Right-aligned |
| Left cell | Centered | R3 |

---

## Code Blocks

### Plain

```
plain text code block
  indented line
```

### C#

```csharp
public class Example : MonoBehaviour
{
    [SerializeField] private string _label = "hello";

    private void Start()
    {
        Debug.Log( _label.ToUpper() );
    }
}
```

### JSON

```json
{
  "name": "test",
  "values": [1, 2, 3],
  "nested": { "key": true }
}
```

### ASCII Diagram

```
BEFORE                 AFTER
┌──────────┐           ┌──────────────────┐
│  ┌──┐    │           │  ┌────────────┐  │
│  │ A│→ B │    →      │  │ SubSM      │  │
│  └──┘    │           │  │ ┌──┐  ┌──┐ │  │
└──────────┘           │  │ │A │→ │B │ │  │
                       │  └────────────┘  │
                       └──────────────────┘
```

---

## Keyboard Shortcuts

Press <kbd>Ctrl</kbd>+<kbd>C</kbd> to copy, <kbd>F2</kbd> to rename, <kbd>Esc</kbd> to cancel.

| Shortcut | Action |
|---|---|
| <kbd>F2</kbd> | Rename state |
| <kbd>F3</kbd> | Rename clip |
| <kbd>Ctrl</kbd>+<kbd>C</kbd> | Copy |
| <kbd>Ctrl</kbd>+<kbd>V</kbd> | Paste |
| <kbd>Esc</kbd> | Cancel |
| <kbd>Enter</kbd> | Confirm |

---

## Links

[External link](https://unity.com)

[Link with tooltip](https://unity.com "Unity Homepage")

[Anchor link to tables section](#table)

Auto-link: https://unity.com

---

## Inline Term Definition

- **WD** — Write Defaults, per-state toggle controlling whether unanimated properties reset
- **SubSM** — Sub-State Machine
- **AAP** — Animator-Animated Parameter

---

## Task Lists (in list)

- [x] Checked — green ☑
- [-] Blocked — red ☒
- [ ] Open — gray ☐
  - [x] Nested checked
  - [-] Nested blocked
  - [ ] Nested open

---

## Checkboxes (standalone)

Use `[x]`, `[ ]`, or `[-]` anywhere outside a list:

Status: [x] Done — priority [!] — review [ ] pending — blocked [-] waiting

Inline in a sentence: task is [x] complete, approval [ ] needed, deployment [-] on hold.

---

## Collapsible

<details>
<summary>Click to expand</summary>

Supports **bold**, *italic*, `inline code`, lists, and code blocks inside.

- List item inside details
- Second item

```csharp
var message = "Hello from inside details";
Debug.Log( message );
```

| Column | Value |
|---|---|
| Row 1 | Data |

</details>

---

## Edge Cases

Trailing spaces (hard line break):  
This line follows a hard break.

Special characters: `<` `>` `&` `"` `'` `\` `/` `|` `{` `}` `[` `]` `(` `)` `#` `*` `_` `~` `` ` ``

Unicode: → ← ↑ ↓ ⇄ ✓ ✗ • … © ® ™ 🎮

HTML entities: &amp; &lt; &gt; &nbsp;

---

## Long Paragraph

This is a long paragraph testing text wrapping. The quick brown fox jumps over the lazy dog. Pack my box with five dozen liquor jugs. How vexingly quick daft zebras jump. The five boxing wizards jump quickly. Sphinx of black quartz, judge my vow.

---

## End of Test
