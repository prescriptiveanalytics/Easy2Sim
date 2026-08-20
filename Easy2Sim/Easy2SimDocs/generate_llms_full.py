"""Generates docs/llms-full.txt: the complete documentation flattened into one file.

Snippet includes (--8<-- "file") are expanded with the actual content of the
example files, so the generated file contains the real, compilable code.

Run this script whenever the documentation changes:

    python generate_llms_full.py
"""
import re
from pathlib import Path

DOCS_ROOT = Path(__file__).parent
DOCS_DIR = DOCS_ROOT / "docs"

# Same base paths as configured for pymdownx.snippets in mkdocs.yml
SNIPPET_BASE_PATHS = [DOCS_DIR, DOCS_ROOT / "examples"]

# Documentation pages in navigation order
PAGES = [
    "README.md",
    "getting-started.md",
    "Components.md",
    "Solvers.md",
    "visualization.md",
    "ai-assistants.md",
]

SNIPPET_RE = re.compile(r'--8<--\s*"([^"]+)"')


def expand_snippets(text: str) -> str:
    def replace(match: re.Match) -> str:
        relative_path = match.group(1)
        for base_path in SNIPPET_BASE_PATHS:
            candidate = base_path / relative_path
            if candidate.exists():
                return candidate.read_text(encoding="utf-8").strip()
        raise FileNotFoundError(f"Snippet not found: {relative_path}")

    return SNIPPET_RE.sub(replace, text)


def main() -> None:
    parts = []
    for page in PAGES:
        text = (DOCS_DIR / page).read_text(encoding="utf-8")
        parts.append(f"<!-- Source: {page} -->\n\n{expand_snippets(text).strip()}")

    output = "\n\n---\n\n".join(parts) + "\n"
    target = DOCS_DIR / "llms-full.txt"
    # Always write LF line endings so the generated file is identical
    # on every machine and does not dirty the git working tree
    target.write_text(output, encoding="utf-8", newline="\n")
    print(f"Wrote {target} ({len(output)} characters)")


if __name__ == "__main__":
    main()
