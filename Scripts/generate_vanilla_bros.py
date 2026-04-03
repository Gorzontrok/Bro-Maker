#!/usr/bin/env python3
"""
Generates vanilla bro wrapper classes from the RambroM template.

Reads RambroM.cs as text and vanilla-bros.json config, produces one .cs file
per bro entry by substituting the class declaration, HeroPreset attribute,
and FixNullVariableLocal() body.

Usage:
    python3 Scripts/generate_vanilla_bros.py

Run from the Bro-Maker repo root.
"""

import json
import os
import re
import sys

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
CONFIG_PATH = os.path.join(SCRIPT_DIR, "vanilla-bros.json")

HEADER = "// Auto-generated from RambroM.cs \u2014 do not edit manually\n"


def load_config():
    with open(CONFIG_PATH, "r") as f:
        return json.load(f)


def load_template(config):
    template_path = os.path.join(SCRIPT_DIR, config["templateFile"])
    with open(template_path, "r") as f:
        return f.read()


def generate_null_fix_body(bro):
    """Generate the C# code for FixNullVariableLocal() body."""
    fixes = bro.get("nullFixes", [])
    if not fixes:
        return ""

    lines = []

    # Determine the prefab source
    prefab_source = bro.get("prefabSource", None)
    if prefab_source:
        hero_type = prefab_source["heroType"]
        cast_class = prefab_source["castClass"]
    else:
        hero_type = bro["heroType"]
        cast_class = bro["baseClass"]

    # Check if we need the bro variable (any prefabCopy fixes?)
    needs_bro_var = any(
        fix["type"] == "prefabCopy" for fix in fixes
    )

    if needs_bro_var:
        lines.append(
            f"            var bro = HeroController.GetHeroPrefab(HeroType.{hero_type}).As<{cast_class}>();"
        )

    for fix in fixes:
        fix_type = fix["type"]

        if fix_type == "prefabCopy":
            for field in fix["fields"]:
                lines.append(f"            {field} = bro.{field};")

        elif fix_type == "findChild":
            field = fix["field"]
            child_name = fix["childName"]
            component = fix["component"]
            lines.append(
                f'            {field} = this.FindChildOfName("{child_name}").GetComponent<{component}>();'
            )

        elif fix_type == "getComponent":
            field = fix["field"]
            component = fix["component"]
            lines.append(f"            {field} = GetComponent<{component}>();")

        elif fix_type == "getComponentInChildren":
            field = fix["field"]
            component = fix["component"]
            lines.append(
                f"            {field} = GetComponentInChildren<{component}>();"
            )

        elif fix_type == "setValue":
            field = fix["field"]
            value = fix["value"]
            lines.append(f"            {field} = {value};")

        elif fix_type == "raw":
            for code_line in fix["code"]:
                lines.append(f"            {code_line}")

    return "\n".join(lines)


def generate_bro(template, bro):
    """Generate a single bro wrapper class from the template."""
    result = template

    # Replace class declaration
    result = re.sub(
        r"public class RambroM : Rambro, ICustomHero",
        f"public class {bro['className']} : {bro['baseClass']}, ICustomHero",
        result,
    )

    # Replace HeroPreset attribute
    result = re.sub(
        r'\[HeroPreset\("Rambro", HeroType\.Rambro\)\]',
        f'[HeroPreset("{bro["presetName"]}", HeroType.{bro["heroType"]})]',
        result,
    )

    # Replace FixNullVariableLocal body
    fix_body = generate_null_fix_body(bro)
    if fix_body:
        result = re.sub(
            r"(protected virtual void FixNullVariableLocal\(\)\s*\{)\s*(\})",
            rf"\1\n{fix_body}\n        \2",
            result,
        )

    # Add header
    result = HEADER + result

    return result


def main():
    config = load_config()
    template = load_template(config)
    output_dir = os.path.join(SCRIPT_DIR, config["outputDirectory"])

    os.makedirs(output_dir, exist_ok=True)

    generated = 0
    for bro in config["bros"]:
        output_path = os.path.join(output_dir, f"{bro['className']}.cs")
        content = generate_bro(template, bro)

        with open(output_path, "w") as f:
            f.write(content)

        generated += 1

    print(f"Generated {generated} vanilla bro wrapper classes in {output_dir}")


if __name__ == "__main__":
    main()
