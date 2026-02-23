from __future__ import annotations

import codecs
import json
import os
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent

localizer_start = "Localizer[\""
localizer_end = "\"]"

LOCALIZER_PATTERN = re.compile(
	rf"@?{re.escape(localizer_start)}((?:\\.|[^\"\\])*){re.escape(localizer_end)}"
)

SKIP_DIRS = {
	".git",
	".vs",
	".idea",
	".vscode",
	"bin",
	"obj",
	"node_modules",
	"TestResults",
}


def iter_source_files(root: Path) -> list[Path]:
	source_files: list[Path] = []
	for dirpath, dirnames, filenames in os.walk(root):
		dirnames[:] = [d for d in dirnames if d not in SKIP_DIRS]
		for filename in filenames:
			if filename.endswith((".cs", ".razor")):
				source_files.append(Path(dirpath) / filename)
	return source_files


def unescape_csharp_string(value: str) -> str:
	try:
		return codecs.decode(value, "unicode_escape")
	except Exception:
		return value


def collect_localizer_keys(root: Path) -> set[str]:
	keys: set[str] = set()
	for path in iter_source_files(root):
		try:
			text = path.read_text(encoding="utf-8", errors="ignore")
		except Exception:
			continue
		for match in LOCALIZER_PATTERN.finditer(text):
			keys.add(unescape_csharp_string(match.group(1)))
	return keys


def find_translation_files(root: Path) -> tuple[list[Path], list[Path]]:
	i18n_dirs = [p for p in root.rglob("i18n") if p.is_dir()]
	english_files: list[Path] = []
	french_files: list[Path] = []

	for dir_path in i18n_dirs:
		for file_path in dir_path.glob("*.json"):
			if file_path.name.endswith(".fr.json"):
				french_files.append(file_path)
			else:
				english_files.append(file_path)

	return english_files, french_files


def load_translation_keys(files: list[Path]) -> set[str]:
	keys: set[str] = set()
	for file_path in files:
		try:
			text = file_path.read_text(encoding="utf-8")
			data = json.loads(text)
		except Exception as exc:
			print(f"Warning: failed to read {file_path}: {exc}")
			continue

		if isinstance(data, dict):
			keys.update(str(k) for k in data.keys())
		else:
			print(
				f"Warning: expected a JSON object in {file_path}, got {type(data).__name__}"
			)

	return keys


def main() -> int:
	english_files, french_files = find_translation_files(ROOT)
	if not english_files or not french_files:
		print("Error: no translation files found in i18n directories.")
		print(f"English files: {len(english_files)}; French files: {len(french_files)}")
		return 2

	localizer_keys = collect_localizer_keys(ROOT)
	english_keys = load_translation_keys(english_files)
	french_keys = load_translation_keys(french_files)

	missing_english = sorted(key for key in localizer_keys if key not in english_keys)
	missing_french = sorted(key for key in localizer_keys if key not in french_keys)

	print(f"Found {len(localizer_keys)} localizer strings.")
	print(f"English translation files: {len(english_files)}")
	print(f"French translation files: {len(french_files)}")

	if missing_english:
		print("\nMissing English translations:")
		for key in missing_english:
			print(f"- {key}")

	if missing_french:
		print("\nMissing French translations:")
		for key in missing_french:
			print(f"- {key}")

	if missing_english or missing_french:
		return 1

	print("\nAll localized strings are present in English and French files.")
	return 0


if __name__ == "__main__":
	sys.exit(main())
