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
    status = str(_get_value(summary_data, 'Status', 'status', default='success') or 'success')
    generated_at = _format_generated_at(_get_value(summary_data, 'GeneratedAt', 'generatedAt'))
    solutions_count = _to_int(_get_value(summary_data, 'SolutionsCount', 'solutionsCount'))
    projects_count = _to_int(_get_value(summary_data, 'ProjectsCount', 'projectsCount'))
    projects_csharp_count = _to_int(_get_value(summary_data, 'ProjectsCSharpCount', 'projectsCSharpCount'))
    projects_visual_basic_count = _to_int(
        _get_value(summary_data, 'ProjectsVisualBasicCount', 'projectsVisualBasicCount')
    )
    files_count = _to_int(_get_value(summary_data, 'FilesCount', 'filesCount'))
    files_csharp_count = _to_int(_get_value(summary_data, 'FilesCSharpCount', 'filesCSharpCount'))
    files_visual_basic_count = _to_int(_get_value(summary_data, 'FilesVisualBasicCount', 'filesVisualBasicCount'))
    top_level_classes_count = _to_int(_get_value(summary_data, 'TopLevelClassesCount', 'topLevelClassesCount'))
    interfaces_count = _to_int(_get_value(summary_data, 'InterfacesCount', 'interfacesCount'))
    abstract_classes_count = _to_int(_get_value(summary_data, 'AbstractClassesCount', 'abstractClassesCount'))
    unprocessed_projects_count = _to_int(_get_value(summary_data, 'UnprocessedProjectsCount', 'unprocessedProjectsCount'))
    unprocessed_source_files_count = _to_int(
        _get_value(summary_data, 'UnprocessedSourceFilesCount', 'unprocessedSourceFilesCount')
    )
    source_code_lines = _to_int(_get_value(summary_data, 'SourceCodeLines', 'sourceCodeLines'))

    metadata = {
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
    }

    markdown = '\n'.join(
        [
            '## Honeydew',
            '',
            (
                f"- Solutions: {_format_int(solutions_count)} / "
                f"Projects: {_format_int(projects_count)} / "
                f"C# projects: {_format_int(projects_csharp_count)} / "
                f"VB projects: {_format_int(projects_visual_basic_count)} / "
                f"Unprocessed projects: {_format_int(unprocessed_projects_count)}"
            ),
            (
                f"- Source lines: {_format_int(source_code_lines)} / "
                f"Source files: {_format_int(files_count)} / "
                f"C# files: {_format_int(files_csharp_count)} / "
                f"VB files: {_format_int(files_visual_basic_count)} / "
                f"Unprocessed source files: {_format_int(unprocessed_source_files_count)}"
            ),
            (
                f"- Top-level classes: {_format_int(top_level_classes_count)} / "
                f"Interfaces: {_format_int(interfaces_count)} / "
                f"Abstract classes: {_format_int(abstract_classes_count)}"
            ),
        ]
    )

    template_model = {
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


def _get_value(summary_data: dict[str, Any], *keys: str, default: Any = None) -> Any:
    for key in keys:
        if key in summary_data:
            return summary_data[key]

    return default


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
        try:
            parsed = datetime.strptime(raw_value, '%Y-%m-%d %H:%M:%S UTC').replace(tzinfo=timezone.utc)
        except ValueError:
            return raw_value

    return _format_local_datetime(parsed)


def _format_local_datetime(value: datetime) -> str:
    local_value = value.astimezone()
    return f"{local_value.strftime('%Y-%m-%d %H:%M:%S')} {_format_gmt_offset(local_value.strftime('%z'))}"


def _format_gmt_offset(offset: str) -> str:
    if len(offset) != 5:
        return 'GMT+0'

    sign = offset[0]
    hours = int(offset[1:3])
    minutes = int(offset[3:5])

    if minutes == 0:
        return f'GMT{sign}{hours}'

    return f'GMT{sign}{hours}:{minutes:02d}'


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
