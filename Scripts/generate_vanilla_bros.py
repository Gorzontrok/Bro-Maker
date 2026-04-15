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


def generate_fix_body(bro, common_null_fixes=None):
    """Generate the C# code for the FixNullVariableLocal() marker block."""
    lines = []

    # Determine the prefab source
    prefab_source = bro.get("prefabSource", None)
    if prefab_source:
        hero_type = prefab_source["heroType"]
        cast_class = prefab_source["castClass"]
    else:
        hero_type = bro["heroType"]
        cast_class = bro["baseClass"]

    fixes = bro.get("nullFixes", [])
    has_ref_fixes = any(fix["type"] != "setValue" for fix in fixes)

    if has_ref_fixes:
        prefab_var = "bro"
        lines.append(
            f"            var bro = HeroController.GetHeroPrefab(HeroType.{hero_type}).As<{cast_class}>();"
        )
        lines.append(f"            if (bro == null) return;")
        lines.append(f"            CopySerializedValues(bro);")
    else:
        prefab_var = "prefab"
        lines.append(
            f"            var prefab = HeroController.GetHeroPrefab(HeroType.{hero_type});"
        )
        lines.append(f"            if (prefab == null) return;")
        lines.append(f"            CopySerializedValues(prefab);")

    # Common null-checks for reference-type fields shared across all bros
    if common_null_fixes:
        for field in common_null_fixes:
            lines.append(
                f"            if ({field} == null) {field} = {prefab_var}.{field};"
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


def generate_awake_body(bro):
    """Generate the C# code for the AWAKE_FIXES marker block."""
    awake_fixes = bro.get("awakeFixes", [])
    if not awake_fixes:
        return None

    prefab_source = bro.get("prefabSource", None)
    if prefab_source:
        hero_type = prefab_source["heroType"]
        cast_class = prefab_source["castClass"]
    else:
        hero_type = bro["heroType"]
        cast_class = bro["baseClass"]

    lines = []
    lines.append(
        f"                var awakePrefab = HeroController.GetHeroPrefab(HeroType.{hero_type}).As<{cast_class}>();"
    )
    lines.append(f"                if (awakePrefab != null)")
    lines.append(f"                {{")

    for fix in awake_fixes:
        fix_type = fix["type"]
        if fix_type == "prefabCopy":
            for field in fix["fields"]:
                lines.append(f"                    {field} = awakePrefab.{field};")
        elif fix_type == "setValue":
            field = fix["field"]
            value = fix["value"]
            lines.append(f"                    {field} = {value};")

    lines.append(f"                }}")

    return "\n".join(lines)


def generate_bro(template, bro, common_null_fixes=None):
    """Generate a single bro wrapper class from the template."""
    result = template

    # Replace class declaration
    result = re.sub(
        r"public class RambroM : Rambro, ICustomHero, IAbilityOwner",
        f"public class {bro['className']} : {bro['baseClass']}, ICustomHero, IAbilityOwner",
        result,
    )

    # Replace HeroPreset attribute
    result = re.sub(
        r'\[HeroPreset\("Rambro", HeroType\.Rambro\)\]',
        f'[HeroPreset("{bro["presetName"]}", HeroType.{bro["heroType"]})]',
        result,
    )

    # Replace FixNullVariableLocal marker block
    fix_body = generate_fix_body(bro, common_null_fixes)
    result = re.sub(
        r"            // GENERATOR:FIXES\n.*?            // GENERATOR:END",
        fix_body,
        result,
        flags=re.DOTALL,
    )

    # Replace Awake fixes marker block
    awake_body = generate_awake_body(bro)
    if awake_body:
        result = re.sub(
            r"                // GENERATOR:AWAKE_FIXES\n.*?                // GENERATOR:AWAKE_END",
            awake_body,
            result,
            flags=re.DOTALL,
        )
    else:
        # Remove empty marker block
        result = re.sub(
            r"\n                // GENERATOR:AWAKE_FIXES\n                // GENERATOR:AWAKE_END\n",
            "\n",
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

    common_null_fixes = config.get("commonNullFixes", [])
    generated = 0
    for bro in config["bros"]:
        output_path = os.path.join(output_dir, f"{bro['className']}.cs")
        content = generate_bro(template, bro, common_null_fixes)

        with open(output_path, "w") as f:
            f.write(content)

        generated += 1

    print(f"Generated {generated} vanilla bro wrapper classes in {output_dir}")


if __name__ == "__main__":
    main()
