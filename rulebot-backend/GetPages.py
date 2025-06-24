import sys
import json

def main():
    if len(sys.argv) < 2:
        print(json.dumps([]))  # Return empty list if no input
        return

    input_str = sys.argv[1]
    
    # Example: Split by comma, trim spaces
    result = ["Start Page", "Mid Page", "Show Page", "End Page"]

    # You can choose to return a tuple instead:
    # result = tuple(result)

    print(json.dumps(result))  # Output JSON-formatted list or tuple

if __name__ == "__main__":
    main()