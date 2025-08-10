import sys
import ast
import json
import xml.etree.ElementTree as ET
import os

def findDisplayProperties(stage_id, root):
    stage = root.find(f".//stage[@stageid='{stage_id}']")
    if stage is None:
        return None

    display = stage.find("display")
    if display is None:
        return None

    return {
        'name': stage.get('name'),
        'type': stage.get('type'),
        'x': int(display.get('x', '0')),
        'y': int(display.get('y', '0')),
        'w': int(display.get('w', '30')),  # default width
        'h': int(display.get('h', '30')),  # default height
        'onsuccess': stage.findtext("onsuccess")
    }

def calculate_relative_position(ref_stage, target_stage, min_gap=30):
    dx = target_stage['x'] - ref_stage['x']
    dy = target_stage['y'] - ref_stage['y']

    if abs(dx) > abs(dy):  # Horizontal
        if dx > 0:
            new_x = ref_stage['x'] + ref_stage['w'] + min_gap
            new_y = ref_stage['y']
        else:
            new_x = ref_stage['x'] - target_stage['w'] - min_gap
            new_y = ref_stage['y']
    else:  # Vertical
        if dy > 0:
            new_y = ref_stage['y'] + ref_stage['h'] + min_gap
            new_x = ref_stage['x']
        else:
            new_y = ref_stage['y'] - target_stage['h'] - min_gap
            new_x = ref_stage['x']

    return int(new_x), int(new_y)

def arrange_stages_flow(root, start_stage_id, min_gap=30):
    visited = set()
    current_id = start_stage_id
    updated_count = 0

    while current_id and current_id not in visited:
        visited.add(current_id)
        current_stage = findDisplayProperties(current_id, root)
        if not current_stage:
            break

        next_id = current_stage['onsuccess']
        if not next_id:
            break

        next_stage = findDisplayProperties(next_id, root)
        if not next_stage:
            break

        new_x, new_y = calculate_relative_position(current_stage, next_stage, min_gap)

        stage_elem = root.find(f".//stage[@stageid='{next_id}']")
        if stage_elem is not None:
            display = stage_elem.find("display")
            if display is not None:
                display.set('x', str(new_x))
                display.set('y', str(new_y))
                updated_count += 1

        current_id = next_id

    return updated_count

def run_rule(xml_path, pages, parameters):
    if not os.path.exists(xml_path):
        raise FileNotFoundError(f"XML file not found: {xml_path}")

    tree = ET.parse(xml_path)
    root = tree.getroot()

    # pages is list from C# "Pages" argument
    # parameters is list of tuples from C# "Parameters" argument
    # For now, we ignore parameters unless you want to use them for min_gap, etc.

    for page_name in pages:
        subsheet = root.find(f".//subsheet[name='{page_name}']")
        if subsheet is None:
            continue

        subsheet_id = subsheet.get('subsheetid')
        start_stage = root.find(f".//stage[subsheetid='{subsheet_id}'][@type='Start']")
        if start_stage is None:
            continue

        updated = arrange_stages_flow(root, start_stage.get('stageid'))
        if updated > 0:
            updated_path = os.path.splitext(xml_path)[0] + "_updated.xml"
            tree.write(updated_path, encoding='utf-8', xml_declaration=False)
            xml_path = updated_path  # Update return path

    return xml_path

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

        result = run_rule(xml_path, pages, parameters)
        print(json.dumps([result]))

    except Exception as e:
        print(f"[ERROR] Exception occurred: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()
