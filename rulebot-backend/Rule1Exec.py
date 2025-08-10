import sys
import ast
import json
import xml.etree.ElementTree as ET
import os

def run_rule(xml_path, pages, parameters):
    try:
        tree = ET.parse(xml_path)
        root = tree.getroot()
    except Exception as e:
        raise RuntimeError(f"Error reading XML file: {e}")

    updated_count = 0

    for target_name in pages:
        targetSubsheetid = None

        for subsheet in root.findall(".//subsheet"):
            name_tag = subsheet.find("name")
            if name_tag is not None and name_tag.text == target_name:
                targetSubsheetid = subsheet.get("subsheetid")
                break

        if not targetSubsheetid:
            continue 

        for stage in root.findall(".//stage"):
            subsheetid_tag = stage.find("subsheetid")

            for typeOfStage, width, height in parameters:
                if (
                    subsheetid_tag is not None
                    and subsheetid_tag.text == targetSubsheetid
                    and stage.get("type") != "SubSheetInfo"
                    and stage.get("type") == typeOfStage
                ):
                    display = stage.find("display")
                    if display is not None:
                        new_width = width * 15
                        new_height = height * 15
                        display.set("w", str(new_width))
                        display.set("h", str(new_height))
                        updated_count += 1

    if updated_count > 0:
        try:
            output_path = os.path.splitext(xml_path)[0] + "_updated.xml"
            tree.write(output_path, encoding="utf-8", xml_declaration=False)
            return output_path
        except Exception as e:
            raise RuntimeError(f"Error writing updated XML: {e}")
    else:
        return xml_path  # no changes, return original path

def main():
    if len(sys.argv) != 4:
        print("[ERROR] Invalid number of arguments. Expected 3.")
        print("Usage: Rule1Exec.py <xmlPath> <pageNameCSV> <serializedParamList>")
        sys.exit(1)

    xml_path = sys.argv[1]
    page = sys.argv[2]
    param_list_str = sys.argv[3]

    try:
        pages = [p.strip() for p in page.split(",")]
        parameters = ast.literal_eval(param_list_str)

        if not isinstance(parameters, list):
            raise ValueError("Parameter list is not a list")

        result_path = run_rule(xml_path, pages, parameters)

        print(json.dumps([result_path]))

    except Exception as e:
        print(f"[ERROR] Exception occurred: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()
