#!/usr/bin/env python3
import json
import os
import re
import sys

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
CONFIG_PATH = os.path.join(SCRIPT_DIR, "vanilla-bros.json")

HEADER = "// Auto-generated from RambroM.cs, do not edit manually\n\n"


def generate_bro(template, bro):
    result = template

    result = re.sub(
        r"public class RambroM : Rambro, ICustomHero",
        f"public class {bro['className']} : {bro['baseClass']}, ICustomHero",
        result,
    )

    result = re.sub(
        r'\[HeroPreset\("Rambro", HeroType\.Rambro\)\]',
        f'[HeroPreset("{bro["presetName"]}", HeroType.{bro["heroType"]})]',
        result,
    )

    fixes = bro.get("nullFixes", [])
    for fix in fixes:
        if fix["type"] != "raw":
            sys.exit(f"{bro['className']}: 'nullFixes' handles only 'raw' fixes, got '{fix['type']}'")
    if fixes:
        fix_body = "\n".join("            " + line for fix in fixes for line in fix["code"])
        result = re.sub(
            r"            // GENERATOR:FIXES\n.*?            // GENERATOR:END",
            lambda _: fix_body,
            result,
            flags=re.DOTALL,
        )
    else:
        result = re.sub(
            r"\n\n            // GENERATOR:FIXES\n            // GENERATOR:END\n",
            "\n",
            result,
        )

    awake_fixes = bro.get("awakeFixes", [])
    for fix in awake_fixes:
        if fix["type"] != "prefabCopy":
            sys.exit(f"{bro['className']}: 'awakeFixes' handles only 'prefabCopy' fixes, got '{fix['type']}'")
    if awake_fixes:
        lines = []
        lines.append(f"                if (Info.BetterAnimationDefaulted)")
        lines.append(f"                {{")
        lines.append(
            f"                    {bro['baseClass']} awakePrefab = HeroController.GetHeroPrefab(HeroType.{bro['heroType']}).As<{bro['baseClass']}>();"
        )
        lines.append(f"                    if (awakePrefab != null)")
        lines.append(f"                    {{")
        for fix in awake_fixes:
            for field in fix["fields"]:
                lines.append(f'                        if (!Info.beforeAwake.ContainsKey("{field}"))')
                lines.append(f"                        {{")
                lines.append(f"                            {field} = awakePrefab.{field};")
                lines.append(f"                        }}")
        lines.append(f"                    }}")
        lines.append(f"                }}")
        awake_body = "\n".join(lines)
        result = re.sub(
            r"                // GENERATOR:AWAKE_FIXES\n.*?                // GENERATOR:AWAKE_END",
            lambda _: "\n" + awake_body + "\n",
            result,
            flags=re.DOTALL,
        )
    else:
        result = re.sub(
            r"                // GENERATOR:AWAKE_FIXES\n                // GENERATOR:AWAKE_END\n",
            "",
            result,
        )

    return HEADER + result


def main():
    with open(CONFIG_PATH, "r") as f:
        config = json.load(f)
    with open(os.path.join(SCRIPT_DIR, config["templateFile"]), "r") as f:
        template = f.read()
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
