#!/usr/bin/env python3
import sys
import xml.etree.ElementTree as ET


def main() -> int:
    if len(sys.argv) != 2:
        print("Usage: verify-coverage.py <coverage.cobertura.xml>", file=sys.stderr)
        return 2

    coverage_path = sys.argv[1]
    root = ET.parse(coverage_path).getroot()
    lines_covered = int(root.attrib["lines-covered"])
    lines_valid = int(root.attrib["lines-valid"])
    if lines_covered != lines_valid:
        print(f"Line coverage is not 100%: {lines_covered}/{lines_valid}", file=sys.stderr)
        return 1

    branch_rate = float(root.attrib["branch-rate"]) * 100
    print(f"Line coverage is 100% ({lines_covered}/{lines_valid}); branch coverage is {branch_rate:.2f}%.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
