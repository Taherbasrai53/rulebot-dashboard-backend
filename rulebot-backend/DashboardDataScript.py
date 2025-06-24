# process_input.py
import sys
import random
import json

def main():
    if len(sys.argv) != 5:
        print(json.dumps({"error": "Expected 4 arguments"}))
        sys.exit(1)

    str1 = sys.argv[1]
    str2 = sys.argv[2]
    int1 = int(sys.argv[3])
    int2 = int(sys.argv[4])

    result = [
        random.randint(1, int1 + int2),
        random.randint(1, int1 + int2),
        random.randint(1, int1 + int2),
        random.randint(1, int1 + int2),
    ]

    print(json.dumps(result))

if __name__ == "__main__":
    main()