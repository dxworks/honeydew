#!/usr/bin/env python3

from __future__ import annotations

import argparse
import json
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

from summary_render import render_summary


SUMMARY_DATA_FILE_NAME = 'honeydew-summary-data.json'


def build_payload(summary_data: dict[str, Any]) -> dict[str, Any]:
    status = str(summary_data.get('status') or 'success')
    generated_at = _format_generated_at(summary_data.get('generatedAt'))

    metadata = {
        'project.name': summary_data.get('projectName', 'unknown'),
        'solutions.count': _to_int(summary_data.get('solutionsCount')),
        'projects.count': _to_int(summary_data.get('projectsCount')),
        'projects.csharp.count': _to_int(summary_data.get('projectsCSharpCount')),
        'projects.visualbasic.count': _to_int(summary_data.get('projectsVisualBasicCount')),
        'files.total': _to_int(summary_data.get('filesCount')),
        'files.csharp.count': _to_int(summary_data.get('filesCSharpCount')),
        'files.visualbasic.count': _to_int(summary_data.get('filesVisualBasicCount')),
        'classes.top.level': _to_int(summary_data.get('topLevelClassesCount')),
        'interfaces.count': _to_int(summary_data.get('interfacesCount')),
        'abstract.classes.count': _to_int(summary_data.get('abstractClassesCount')),
        'unprocessed.projects.count': _to_int(summary_data.get('unprocessedProjectsCount')),
        'unprocessed.source.files.count': _to_int(summary_data.get('unprocessedSourceFilesCount')),
        'source.lines.total': _to_int(summary_data.get('sourceCodeLines')),
        'generated.at': generated_at,
    }

    markdown = '\n'.join(
        [
            '## Honeydew',
            '',
            f'- Status: {status}',
            f"- Project: {summary_data.get('projectName', 'unknown')}",
            f"- Solutions: {_to_int(summary_data.get('solutionsCount'))}",
            f"- Projects: {_to_int(summary_data.get('projectsCount'))}",
            f"- C# projects: {_to_int(summary_data.get('projectsCSharpCount'))}",
            f"- Visual Basic projects: {_to_int(summary_data.get('projectsVisualBasicCount'))}",
            f"- Source files: {_to_int(summary_data.get('filesCount'))}",
            f"- C# files: {_to_int(summary_data.get('filesCSharpCount'))}",
            f"- Visual Basic files: {_to_int(summary_data.get('filesVisualBasicCount'))}",
            f"- Top-level classes: {_to_int(summary_data.get('topLevelClassesCount'))}",
            f"- Interfaces: {_to_int(summary_data.get('interfacesCount'))}",
            f"- Abstract classes: {_to_int(summary_data.get('abstractClassesCount'))}",
            f"- Unprocessed projects: {_to_int(summary_data.get('unprocessedProjectsCount'))}",
            f"- Unprocessed source files: {_to_int(summary_data.get('unprocessedSourceFilesCount'))}",
            f"- Source lines: {_to_int(summary_data.get('sourceCodeLines'))}",
            f'- Generated at: {generated_at}',
        ]
    )

    template_model = {
        'status': status,
        'statusClass': _to_status_class(status),
        'projectName': summary_data.get('projectName', 'unknown'),
        'solutionsCount': _to_int(summary_data.get('solutionsCount')),
        'projectsCount': _to_int(summary_data.get('projectsCount')),
        'projectsCSharpCount': _to_int(summary_data.get('projectsCSharpCount')),
        'projectsVisualBasicCount': _to_int(summary_data.get('projectsVisualBasicCount')),
        'filesCount': _to_int(summary_data.get('filesCount')),
        'filesCSharpCount': _to_int(summary_data.get('filesCSharpCount')),
        'filesVisualBasicCount': _to_int(summary_data.get('filesVisualBasicCount')),
        'topLevelClassesCount': _to_int(summary_data.get('topLevelClassesCount')),
        'interfacesCount': _to_int(summary_data.get('interfacesCount')),
        'abstractClassesCount': _to_int(summary_data.get('abstractClassesCount')),
        'unprocessedProjectsCount': _to_int(summary_data.get('unprocessedProjectsCount')),
        'unprocessedSourceFilesCount': _to_int(summary_data.get('unprocessedSourceFilesCount')),
        'sourceCodeLines': _to_int(summary_data.get('sourceCodeLines')),
        'generatedAt': generated_at,
    }

    return {
        'tool': 'honeydew',
        'status': status,
        'metadata': metadata,
        'markdown': markdown,
        'templateModel': template_model,
    }


def build_missing_payload() -> dict[str, Any]:
    return {
        'tool': 'honeydew',
        'status': 'missing',
        'metadata': {},
        'markdown': '\n'.join([
            '## Honeydew',
            '',
            '- Status: missing',
            '- Summary input is missing',
        ]),
        'templateModel': {
            'status': 'missing',
            'statusClass': 'status-missing',
            'isMissing': True,
        },
    }


def _to_status_class(status: str) -> str:
    if status == 'success':
        return 'status-success'
    if status == 'partial':
        return 'status-warning'
    if status == 'failed':
        return 'status-error'
    if status == 'missing':
        return 'status-missing'
    return 'status-unknown'


def _to_int(value: Any) -> int:
    try:
        return int(value)
    except Exception:
        return 0


def _format_generated_at(value: Any) -> str:
    if value is None:
        return 'unknown'

    raw_value = str(value).strip()
    if not raw_value:
        return 'unknown'

    try:
        parsed = datetime.fromisoformat(raw_value.replace('Z', '+00:00'))
    except ValueError:
        return raw_value

    return parsed.astimezone(timezone.utc).strftime('%Y-%m-%d %H:%M:%S UTC')


def main() -> int:
    parser = argparse.ArgumentParser(
        prog='honeydew-summary.py',
        description='Generates Honeydew summary artifacts for Voyager',
    )
    parser.add_argument('results_directory', nargs='?', default='results')
    args = parser.parse_args()

    target_directory = Path(args.results_directory).resolve()
    summary_data_path = target_directory / SUMMARY_DATA_FILE_NAME

    try:
        if not summary_data_path.exists():
            print(
                f"summary input missing for honeydew: expected '{SUMMARY_DATA_FILE_NAME}' in "
                f"'{target_directory}'; generating missing summary artifacts"
            )
            payload = build_missing_payload()
        else:
            payload = build_payload(json.loads(summary_data_path.read_text(encoding='utf-8')))
        rendered = render_summary(target_directory, payload)
        print(f"Generated summary markdown at {rendered['summaryMdPath']}")
        print(f"Generated summary html at {rendered['summaryHtmlPath']}")
        return 0
    except Exception as error:
        print(f"summary generation failed for '{target_directory}': {error}")
        return 1


if __name__ == '__main__':
    raise SystemExit(main())
