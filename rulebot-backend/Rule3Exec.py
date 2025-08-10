import sys
import ast
import json
import xml.etree.ElementTree as ET
import uuid

def generate_uuid():
    return str(uuid.uuid4())

def get_block_reference(stage, block_prefix_map):
    item_name = stage.get('name', '')
    private_tag = stage.find("private")
    for block_name, prefix in block_prefix_map:
        if item_name.startswith(prefix):
            return block_name
    return "Global" if private_tag is None else "Local"

def collect_block_items(root, subsheetid, block_name, block_prefix_map):
    items = []
    for stage in root.findall(".//stage"):
        if (stage.find("subsheetid") is not None and
            stage.find("subsheetid").text == subsheetid and
            stage.get("type") in ["Data", "Collection"] and
            get_block_reference(stage, block_prefix_map) == block_name):
            items.append(stage)
    return items

def calculate_block_dimensions(items):
    max_width = 90
    total_height = 15
    for item in items:
        display = item.find("display")
        if display is not None:
            h = int(display.get("h", "30"))
            w = int(display.get("w", "60"))
            total_height += h
            max_width = max(max_width, w)
    total_height += 15
    return max_width, total_height

def organize_items_in_block(items, block_x, block_y):
    current_y = block_y + 15
    for item in items:
        display = item.find("display")
        if display is not None:
            w = int(display.get("w", "60"))
            h = int(display.get("h", "30"))
            new_x = block_x + (max(0, (w // 2)))
            new_y = current_y + h // 2
            display.set("x", str(new_x))
            display.set("y", str(new_y))
            current_y += h

def create_or_update_block(root, subsheetid, block_name, color, x, y, width, height):
    for stage in root.findall(".//stage"):
        if (stage.get("type") == "Block" and
            stage.get("name") == block_name and
            stage.find("subsheetid") is not None and
            stage.find("subsheetid").text == subsheetid):
            display = stage.find("display")
            if display is not None:
                display.set("x", str(x))
                display.set("y", str(y))
                display.set("w", str(width))
                display.set("h", str(height))
            font = stage.find("font")
            if font is not None:
                font.set("color", color)
            else:
                ET.SubElement(stage, "font", {
                    "family": "Segoe UI", "size": "10", "style": "Regular", "color": color
                })
            return stage

    stage = ET.Element("stage", {
        "stageid": generate_uuid(),
        "name": block_name,
        "type": "Block"
    })
    ET.SubElement(stage, "subsheetid").text = subsheetid
    ET.SubElement(stage, "loginhibit", {"onsuccess": "true"})
    ET.SubElement(stage, "display", {"x": str(x), "y": str(y), "w": str(width), "h": str(height)})
    ET.SubElement(stage, "font", {
        "family": "Segoe UI", "size": "10", "style": "Regular", "color": color
    })
    root.append(stage)
    return stage

def run_rule(xml_path, pages, parameters):
    tree = ET.parse(xml_path)
    root = tree.getroot()

    block_prefixes = parameters  # list of tuples: (blockName, prefix, color)
    block_prefix_map = [(b[0], b[1]) for b in block_prefixes]
    block_colors = {b[0]: b[2] for b in block_prefixes}
    block_names = list(block_colors.keys())

    subsheetid = None
    for subsheet in root.findall(".//subsheet"):
        name_tag = subsheet.find("name")
        if name_tag is not None and name_tag.text in pages:
            subsheetid = subsheet.get("subsheetid")
            break
    if not subsheetid:
        raise ValueError(f"Subsheet(s) {pages} not found.")

    smallest_X = float('inf')
    SubSheetInfo_y = 0
    SubSheetInfo_h = 90
    for stage in root.findall(".//stage"):
        if stage.find("subsheetid") is not None and stage.find("subsheetid").text == subsheetid:
            display = stage.find("display")
            if display is not None:
                x_val = int(display.get("x", "0"))
                if x_val < smallest_X:
                    smallest_X = x_val
                if stage.get("type") == "SubSheetInfo":
                    SubSheetInfo_y = int(display.get("y", "0"))
                    SubSheetInfo_h = int(display.get("h", "90"))

    new_X_coordinate = smallest_X - 75
    current_y = SubSheetInfo_y + SubSheetInfo_h + 30

    for block_name in block_names:
        items = collect_block_items(root, subsheetid, block_name, block_prefix_map)
        if not items:
            continue
        width, height = calculate_block_dimensions(items)
        create_or_update_block(root, subsheetid, block_name, block_colors[block_name],
                               new_X_coordinate - width, current_y, width, height)
        organize_items_in_block(items, new_X_coordinate - width, current_y)
        current_y += height + 15

    updated_path = xml_path.replace(".xml", "_updated.xml")
    tree.write(updated_path, encoding="utf-8", xml_declaration=False)
    return updated_path

def main():
    if len(sys.argv) != 4:
        print("[ERROR] Invalid number of arguments. Expected 3.")
        print("Usage: Rule4Exec.py <xmlPath> <pageNameCSV> <serializedParamList>")
        sys.exit(1)

    xml_path = sys.argv[1]
    page = sys.argv[2]
    param_list_str = sys.argv[3]

    try:
        pages = [p.strip() for p in page.split(",")]
        parameters = ast.literal_eval(param_list_str)
        if not isinstance(parameters, list):
            raise ValueError("Parameter list is not a list")

        result = run_rule(xml_path, pages, parameters)
        print(json.dumps([result]))

    except Exception as e:
        print(f"[ERROR] Exception occurred: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()
