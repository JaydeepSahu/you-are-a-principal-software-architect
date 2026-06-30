import re
from pathlib import Path
import sys

try:
    import yaml
except ImportError:
    import subprocess
    subprocess.check_call([sys.executable, "-m", "pip", "install", "PyYAML"])
    import yaml

root = Path(__file__).resolve().parent
backlog_file = root / "backlog.yaml"
output_dir = root / "issues"
output_dir.mkdir(parents=True, exist_ok=True)

with backlog_file.open("r", encoding="utf-8") as f:
    data = yaml.safe_load(f)

for epic in data.get("epics", []):
    epic_id = epic.get("id", "")
    epic_title = epic.get("title", "")
    for story in epic.get("stories", []):
        story_id = story.get("id", "")
        title = story.get("title", "")
        description = story.get("description", "")
        acceptance_criteria = story.get("acceptance_criteria", [])
        sanitized_title = re.sub(r"[^A-Za-z0-9_-]", "", title.replace(" ", "-").lower())
        filename = output_dir / f"{story_id}-{sanitized_title}.md"
        content = [
            "---",
            "labels:",
            f"  - {epic_id}",
            f"  - {epic_title}",
            "  - story",
            "---",
            "",
            f"# {story_id}: {title}",
            "",
            f"**Epic:** {epic_id} — {epic_title}",
            "",
            "## Description",
            "",
            description,
            "",
            "## Acceptance Criteria",
            ""
        ]
        content.extend(f"- {item}" for item in acceptance_criteria)
        filename.write_text("\n".join(content) + "\n", encoding="utf-8")
        print(f"Wrote {filename}")
