from __future__ import annotations

import html
from pathlib import Path
from typing import Any


DEFAULT_TEMPLATE_PATH = Path(__file__).resolve().parent / 'templates' / 'summary.html'
FALLBACK_TEMPLATE = '<section><h2>{{tool}}</h2></section>'


def render_summary(
    results_directory: str | Path,
    payload: dict[str, Any],
    template_path: str | Path | None = None,
) -> dict[str, Any]:
    target = Path(results_directory)
    target.mkdir(parents=True, exist_ok=True)

    template_file = Path(template_path) if template_path else DEFAULT_TEMPLATE_PATH
    try:
        template = template_file.read_text(encoding='utf-8')
    except Exception:
        template = FALLBACK_TEMPLATE

    tool = str(payload.get('tool') or 'unknown')
    status = str(payload.get('status') or 'unknown')
    metadata = payload.get('metadata') or {}
    markdown = str(payload.get('markdown') or '')
    template_model = payload.get('templateModel') or {}

    if not isinstance(metadata, dict):
        raise ValueError('summary payload metadata must be an object')
    if not isinstance(template_model, dict):
        raise ValueError('summary payload templateModel must be an object')

    model = dict(template_model)
    model.setdefault('tool', tool)
    model.setdefault('status', status)

    rendered_html = _render_template(template, model)
    metadata_block = _build_metadata_block(tool, status, metadata)

    summary_md_path = target / 'summary.md'
    summary_html_path = target / 'summary.html'

    summary_html_path.write_text(rendered_html, encoding='utf-8')
    summary_md_path.write_text(f"{metadata_block}\n---\n{markdown}\n", encoding='utf-8')

    return {
        'status': status,
        'summaryMdPath': str(summary_md_path),
        'summaryHtmlPath': str(summary_html_path),
    }


def _build_metadata_block(tool: str, status: str, metadata: dict[str, Any]) -> str:
    lines = [
        '---',
        f'tool: {tool}',
        'html-template: reference',
        f'status: {status}',
    ]

    for key, value in metadata.items():
        lines.append(f'{key}: {_stringify_metadata_value(value)}')

    return '\n'.join(lines)


def _stringify_metadata_value(value: Any) -> str:
    if value is None:
        return 'null'
    return str(value)


def _render_template(template: str, model: dict[str, Any]) -> str:
    tokens, _ = _parse_nodes(template, 0, set())
    return _render_nodes(tokens, model)


def _parse_nodes(template: str, start: int, stop_tags: set[str]) -> tuple[list[dict[str, Any]], int]:
    index = start
    nodes: list[dict[str, Any]] = []

    while index < len(template):
        marker = template.find('{{', index)
        if marker < 0:
            if index < len(template):
                nodes.append({'type': 'text', 'value': template[index:]})
            return nodes, len(template)

        if marker > index:
            nodes.append({'type': 'text', 'value': template[index:marker]})

        if template.startswith('{{{', marker):
            close = template.find('}}}', marker + 3)
            if close < 0:
                nodes.append({'type': 'text', 'value': template[marker:]})
                return nodes, len(template)

            expression = template[marker + 3:close].strip()
            nodes.append({'type': 'raw', 'expression': expression})
            index = close + 3
            continue

        close = template.find('}}', marker + 2)
        if close < 0:
            nodes.append({'type': 'text', 'value': template[marker:]})
            return nodes, len(template)

        expression = template[marker + 2:close].strip()
        index = close + 2

        if not expression:
            continue

        if expression in stop_tags:
            return nodes, marker

        if expression.startswith('#if '):
            condition = expression[4:].strip()
            true_nodes, branch_pos = _parse_nodes(template, index, {'else', '/if'})
            false_nodes: list[dict[str, Any]] = []

            branch_marker_end = _advance_tag_end(template, branch_pos)
            branch_expression = _read_tag_expression(template, branch_pos)

            if branch_expression == 'else':
                false_nodes, end_if_pos = _parse_nodes(template, branch_marker_end, {'/if'})
                index = _advance_tag_end(template, end_if_pos)
            else:
                index = branch_marker_end

            nodes.append(
                {
                    'type': 'if',
                    'condition': condition,
                    'true_nodes': true_nodes,
                    'false_nodes': false_nodes,
                }
            )
            continue

        if expression.startswith('#each '):
            collection = expression[6:].strip()
            each_nodes, end_each_pos = _parse_nodes(template, index, {'/each'})
            index = _advance_tag_end(template, end_each_pos)
            nodes.append({'type': 'each', 'collection': collection, 'nodes': each_nodes})
            continue

        nodes.append({'type': 'var', 'expression': expression})

    return nodes, index


def _advance_tag_end(template: str, marker: int) -> int:
    if marker >= len(template):
        return marker

    close = template.find('}}', marker + 2)
    if close < 0:
        return len(template)
    return close + 2


def _read_tag_expression(template: str, marker: int) -> str:
    if marker >= len(template) or not template.startswith('{{', marker):
        return ''
    close = template.find('}}', marker + 2)
    if close < 0:
        return ''
    return template[marker + 2:close].strip()


def _render_nodes(nodes: list[dict[str, Any]], context: dict[str, Any]) -> str:
    parts: list[str] = []

    for node in nodes:
        node_type = node.get('type')
        if node_type == 'text':
            parts.append(node.get('value', ''))
            continue

        if node_type == 'var':
            value = _resolve_expression(context, node.get('expression', ''))
            parts.append(_escape(_stringify(value)))
            continue

        if node_type == 'raw':
            value = _resolve_expression(context, node.get('expression', ''))
            parts.append(_stringify(value))
            continue

        if node_type == 'if':
            condition_value = _resolve_expression(context, node.get('condition', ''))
            branch = node.get('true_nodes', []) if _is_truthy(condition_value) else node.get('false_nodes', [])
            parts.append(_render_nodes(branch, context))
            continue

        if node_type == 'each':
            collection_value = _resolve_expression(context, node.get('collection', ''))
            if isinstance(collection_value, list):
                for item in collection_value:
                    loop_context = _child_context(context, item)
                    parts.append(_render_nodes(node.get('nodes', []), loop_context))
            continue

    return ''.join(parts)


def _child_context(parent: dict[str, Any], item: Any) -> dict[str, Any]:
    return {
        '__parent__': parent,
        'this': item,
    }


def _resolve_expression(context: dict[str, Any], expression: str) -> Any:
    path = expression.strip()
    if not path:
        return ''

    if path == 'this':
        return context.get('this', '')

    segments = path.split('.')
    value = _resolve_root_value(context, segments[0])
    for segment in segments[1:]:
        value = _resolve_segment(value, segment)
        if value is None:
            return ''
    return value


def _resolve_root_value(context: dict[str, Any], key: str) -> Any:
    if key == 'this':
        return context.get('this')

    if key in context:
        return context[key]

    current_item = context.get('this')
    if isinstance(current_item, dict) and key in current_item:
        return current_item[key]

    parent = context.get('__parent__')
    if isinstance(parent, dict):
        return _resolve_root_value(parent, key)

    return ''


def _resolve_segment(value: Any, segment: str) -> Any:
    if isinstance(value, dict):
        return value.get(segment)
    return getattr(value, segment, None)


def _is_truthy(value: Any) -> bool:
    return bool(value)


def _stringify(value: Any) -> str:
    if value is None:
        return ''
    return str(value)


def _escape(value: Any) -> str:
    return html.escape(str(value), quote=True)
