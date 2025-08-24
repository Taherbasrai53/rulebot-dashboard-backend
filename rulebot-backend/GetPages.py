import sys
import json
import xml.etree.ElementTree as ET
import os

def list_all_pages(xml_path):
    try:
        tree = ET.parse(xml_path)
        root = tree.getroot()
    except Exception as e:
        raise RuntimeError(f"Error reading XML file: {e}")

    result = []

    if root.get("type") == "object":
        result.append("Initialise")
    else:
        result.append("Main Page")

    for subsheet in root.findall(".//subsheet"):
        name_tag = subsheet.find("name")
        if name_tag is not None and name_tag.text:
            result.append(name_tag.text)

    return result

def main():
    if len(sys.argv) != 2:
        print("[ERROR] Invalid number of arguments. Expected 1.")
        print("Usage: GetPages.py <xmlPath>")
        sys.exit(1)

    xml_path = sys.argv[1]

    try:
        if not os.path.exists(xml_path):
            raise FileNotFoundError(f"File not found: {xml_path}")

        pages = list_all_pages(xml_path)

        # return as JSON
        print(json.dumps(pages))

    except Exception as e:
        print(f"[ERROR] Exception occurred: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()
