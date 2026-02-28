#!/usr/bin/env python3
"""
XML Structure Analyzer

Parses an XML file and outputs the structure showing:
- Element nesting hierarchy
- All encountered attribute names at each element position
- Eliminates duplicate structures (same element path)
"""

import xml.etree.ElementTree as ET
import sys
from collections import defaultdict


def analyze_xml_structure(xml_file):
    """
    Parse XML file and build structure map with all attributes.
    
    Args:
        xml_file: Path to XML file to analyze
        
    Returns:
        dict: Mapping of element paths to sets of attribute names
    """
    try:
        tree = ET.parse(xml_file)
        root = tree.getroot()
    except ET.ParseError as e:
        print(f"Error parsing XML: {e}", file=sys.stderr)
        sys.exit(1)
    except FileNotFoundError:
        print(f"File not found: {xml_file}", file=sys.stderr)
        sys.exit(1)
    
    # Dictionary to store unique paths and their attributes
    structure = defaultdict(set)
    
    def traverse(element, path=""):
        """Recursively traverse XML tree and record structure."""
        # Build current path
        current_path = f"{path}/{element.tag}" if path else element.tag
        
        # Add attributes for this element path
        for attr_name in element.attrib.keys():
            structure[current_path].add(attr_name)
        
        # If element has no attributes, still record the path
        if not element.attrib:
            structure[current_path]  # This ensures the path is in dict
        
        # Recursively process children
        for child in element:
            traverse(child, current_path)
    
    traverse(root)
    return structure


def print_structure(structure):
    """
    Print the XML structure in a hierarchical tree format.
    
    Args:
        structure: dict mapping paths to sets of attribute names
    """
    print("XML Structure Analysis (Hierarchical View)")
    print("=" * 80)
    print()
    
    # Sort paths for consistent output
    sorted_paths = sorted(structure.keys())
    
    for path in sorted_paths:
        attrs = structure[path]
        
        # Calculate indentation based on path depth
        depth = path.count('/')
        indent = "  " * depth
        element_name = path.split('/')[-1]
        
        # Print element with attributes
        if attrs:
            attr_str = ", ".join(sorted(attrs))
            print(f"{indent}<{element_name}> [attributes: {attr_str}]")
        else:
            print(f"{indent}<{element_name}>")
    
    print()
    print("=" * 80)
    print(f"Total unique element paths: {len(sorted_paths)}")


def main():
    """Main entry point."""
    if len(sys.argv) != 2:
        print("Usage: python xml_structure_analyzer.py <xml_file>")
        print()
        print("Example:")
        print("  python xml_structure_analyzer.py MUT3_SE/DiagDB/Ecu/CMU09_DB000000.xml")
        sys.exit(1)
    
    xml_file = sys.argv[1]
    
    print(f"Analyzing: {xml_file}")
    print()
    
    structure = analyze_xml_structure(xml_file)
    print_structure(structure)


if __name__ == "__main__":
    main()
