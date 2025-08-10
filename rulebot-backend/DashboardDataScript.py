import sys
import json
import xml.etree.ElementTree as ET
import os

def analyze_blueprism(input_file, subsheet_name, stage_width, stage_height, dataitem_width, dataitem_height):
    try:
        # Parse the XML
        xml_file_path = os.path.normpath(input_file.replace('//', '\\'))
        tree = ET.parse(xml_file_path)
        root = tree.getroot()
        
        # 1. Find the target subsheet ID
        target_subsheet_id = None
        for subsheet in root.findall(".//subsheet"):
            name_tag = subsheet.find("name")
            if name_tag is not None and name_tag.text == subsheet_name:
                target_subsheet_id = subsheet.get("subsheetid")
                break
        
        if not target_subsheet_id: 
            return {"error": f"Subsheet '{subsheet_name}' not found!"}
        
        # 2. Find all stages with matching subsheetid (excluding Block and Anchor types)
        stages = []
        for stage in root.findall(".//stage"):
            subsheetid_tag = stage.find("subsheetid")
            stage_type = stage.get('type')
            if (subsheetid_tag is not None and 
                subsheetid_tag.text == target_subsheet_id and
                stage_type not in ['SubSheetInfo', 'Block', 'Anchor']):
                
                display = stage.find("display")
                if display is not None:
                    try:
                        width = float(display.get("w", "60"))
                        height = float(display.get("h", "30"))
                        follows_rules = (width == stage_width*15) and (height == stage_height*15)
                        stages.append({'follows_rules': follows_rules})
                    except Exception as e:
                        return {"error": f"Error processing stage dimensions: {e}"}
        
        # 3. Find all data items (Data and Collection types) with matching subsheetid
        data_items = []
        for data_item in root.findall(".//stage"):
            subsheetid_tag = data_item.find("subsheetid")
            item_type = data_item.get('type')
            if (subsheetid_tag is not None and 
                subsheetid_tag.text == target_subsheet_id and
                item_type in ['Data', 'Collection']):
                
                display = data_item.find("display")
                if display is not None:
                    try:
                        width = float(display.get("w", "60"))
                        height = float(display.get("h", "30"))
                        follows_rules = (width == dataitem_width*15) and (height == dataitem_height*15)
                        data_items.append({'follows_rules': follows_rules})
                    except Exception as e:
                        return {"error": f"Error processing data item dimensions: {e}"}
        
        # Calculate compliance
        stage_compliant = sum(1 for s in stages if s['follows_rules'])
        stage_non_compliant = len(stages) - stage_compliant
        
        dataitem_compliant = sum(1 for d in data_items if d['follows_rules'])
        dataitem_non_compliant = len(data_items) - dataitem_compliant
        return (stage_compliant, stage_non_compliant, dataitem_compliant, dataitem_non_compliant)
        
    
    except FileNotFoundError:
        return {"error": f"File '{input_file}' not found"}
    except ET.ParseError:
        return {"error": f"File '{input_file}' is not a valid XML file"}
    except Exception as e:
        return {"error": f"Unexpected error: {str(e)}"}

def main():
    if len(sys.argv) != 7:
    	print(json.dumps({"error": "Expected 5 arguments: input_file subsheet_name stage_width stage_height dataitem_width dataitem_height"}))
    	sys.exit(1)
    

    try:
        input_file = sys.argv[1]
        subsheet_name = sys.argv[2]
        stage_width = int(sys.argv[3])
        stage_height = int(sys.argv[4])
        dataitem_width = int(sys.argv[5])
        dataitem_height = int(sys.argv[6])

        result = analyze_blueprism(
	    input_file,
	    subsheet_name,
            stage_width,
            stage_height,
            dataitem_width,
            dataitem_height
        )

        print(json.dumps(result))
    except ValueError:
        print(json.dumps({"error": "Dimensions must be integers"}))
        sys.exit(1)

if __name__ == "__main__":
    main() 