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
    solutions_count = _to_int(summary_data.get('solutionsCount'))
    projects_count = _to_int(summary_data.get('projectsCount'))
    projects_csharp_count = _to_int(summary_data.get('projectsCSharpCount'))
    projects_visual_basic_count = _to_int(summary_data.get('projectsVisualBasicCount'))
    files_count = _to_int(summary_data.get('filesCount'))
    files_csharp_count = _to_int(summary_data.get('filesCSharpCount'))
    files_visual_basic_count = _to_int(summary_data.get('filesVisualBasicCount'))
    top_level_classes_count = _to_int(summary_data.get('topLevelClassesCount'))
    interfaces_count = _to_int(summary_data.get('interfacesCount'))
    abstract_classes_count = _to_int(summary_data.get('abstractClassesCount'))
    unprocessed_projects_count = _to_int(summary_data.get('unprocessedProjectsCount'))
    unprocessed_source_files_count = _to_int(summary_data.get('unprocessedSourceFilesCount'))
    source_code_lines = _to_int(summary_data.get('sourceCodeLines'))

    metadata = {
        'project.name': summary_data.get('projectName', 'unknown'),
        'solutions.count': solutions_count,
        'projects.count': projects_count,
        'projects.csharp.count': projects_csharp_count,
        'projects.visualbasic.count': projects_visual_basic_count,
        'files.total': files_count,
        'files.csharp.count': files_csharp_count,
        'files.visualbasic.count': files_visual_basic_count,
        'classes.top.level': top_level_classes_count,
        'interfaces.count': interfaces_count,
        'abstract.classes.count': abstract_classes_count,
        'unprocessed.projects.count': unprocessed_projects_count,
        'unprocessed.source.files.count': unprocessed_source_files_count,
        'source.lines.total': source_code_lines,
        'generated.at': generated_at,
    }

    markdown = '\n'.join(
        [
            '## Honeydew',
            '',
            f"- Project: {summary_data.get('projectName', 'unknown')}",
            f"- Solutions: {_format_int(solutions_count)}",
            f"- Projects: {_format_int(projects_count)}",
            f"- C# projects: {_format_int(projects_csharp_count)}",
            f"- Visual Basic projects: {_format_int(projects_visual_basic_count)}",
            f"- Source files: {_format_int(files_count)}",
            f"- C# files: {_format_int(files_csharp_count)}",
            f"- Visual Basic files: {_format_int(files_visual_basic_count)}",
            f"- Top-level classes: {_format_int(top_level_classes_count)}",
            f"- Interfaces: {_format_int(interfaces_count)}",
            f"- Abstract classes: {_format_int(abstract_classes_count)}",
            f"- Unprocessed projects: {_format_int(unprocessed_projects_count)}",
            f"- Unprocessed source files: {_format_int(unprocessed_source_files_count)}",
            f"- Source lines: {_format_int(source_code_lines)}",
            f'- Generated at: {generated_at}',
        ]
    )

    template_model = {
        'projectName': summary_data.get('projectName', 'unknown'),
        'solutionsCountFormatted': _format_int(solutions_count),
        'projectsCountFormatted': _format_int(projects_count),
        'projectsCSharpCountFormatted': _format_int(projects_csharp_count),
        'projectsVisualBasicCountFormatted': _format_int(projects_visual_basic_count),
        'filesCountFormatted': _format_int(files_count),
        'filesCSharpCountFormatted': _format_int(files_csharp_count),
        'filesVisualBasicCountFormatted': _format_int(files_visual_basic_count),
        'topLevelClassesCountFormatted': _format_int(top_level_classes_count),
        'interfacesCountFormatted': _format_int(interfaces_count),
        'abstractClassesCountFormatted': _format_int(abstract_classes_count),
        'unprocessedProjectsCountFormatted': _format_int(unprocessed_projects_count),
        'unprocessedSourceFilesCountFormatted': _format_int(unprocessed_source_files_count),
        'sourceCodeLinesFormatted': _format_int(source_code_lines),
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
            '- Summary input is missing',
        ]),
        'templateModel': {
            'isMissing': True,
        },
    }


def _to_int(value: Any) -> int:
    try:
        return int(value)
    except Exception:
        return 0


def _format_int(value: int) -> str:
    return f'{value:,}'


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
