#!/usr/bin/env python3
"""
ECU Function Translator

Parses an ECU diagnostic file and translates function IDs to human-readable names
using the DiagLangH.xml language database.

For each skey element, outputs:
- The sid attribute
- All nested function elements with their translated names
"""

import xml.etree.ElementTree as ET
import sys
import os
import csv
import argparse


# KWP2000/UDS Service ID mapping
SERVICE_ID_MAP = {
    0x10: "StartDiagnosticSession",
    0x11: "ECUReset",
    0x14: "ClearDiagnosticInformation",
    0x17: "ReadStatusOfDiagnosticTroubleCodes",
    0x18: "ReadDiagnosticTroubleCodesByStatus",
    0x1A: "ReadECUIdentification",
    0x21: "ReadDataByLocalIdentifier",
    0x22: "ReadDataByIdentifier",
    0x23: "ReadMemoryByAddress",
    0x27: "SecurityAccess",
    0x28: "DisableNormalMessageTransmission",
    0x29: "EnableNormalMessageTransmission",
    0x2C: "DynamicallyDefineLocalIdentifier",
    0x2E: "WriteDataByIdentifier",
    0x30: "InputOutputControlByLocalIdentifier",
    0x31: "StartRoutineByLocalIdentifier",
    0x32: "StopRoutineByLocalIdentifier",
    0x33: "RequestRoutineResultsByLocalIdentifier",
    0x34: "RequestDownload",
    0x35: "RequestUpload",
    0x36: "TransferData",
    0x37: "RequestTransferExit",
    0x3B: "WriteDataByLocalIdentifier",
    0x3D: "WriteMemoryByAddress",
    0x3E: "TesterPresent",
    0x85: "ControlDTCSetting",
    0x86: "ResponseOnEvent",
}


def get_service_name(sid):
    """
    Get the service name for a given service ID.
    
    Args:
        sid: Service ID (can be string or int)
    
    Returns:
        str: Service name or empty string if unknown
    """
    try:
        sid_int = int(sid) if sid else -1
        return SERVICE_ID_MAP.get(sid_int, "")
    except (ValueError, TypeError):
        return ""


def load_function_names(diaglang_file, language='E'):
    """
    Load function name translations from DiagLangH.xml.
    
    Args:
        diaglang_file: Path to DiagLangH.xml file
        language: Language code to use for translation (default: 'E' for English)
                 Options: J, N, E, G, F, S, I, H, C, W, Y, R, T, V, K, L
    
    Returns:
        dict: Mapping of fid to translated function name
    """
    try:
        tree = ET.parse(diaglang_file)
        root = tree.getroot()
    except ET.ParseError as e:
        print(f"Error parsing DiagLangH.xml: {e}", file=sys.stderr)
        sys.exit(1)
    except FileNotFoundError:
        print(f"DiagLangH.xml not found: {diaglang_file}", file=sys.stderr)
        sys.exit(1)
    
    function_map = {}
    
    # Find function_list element
    function_list = root.find('function_list')
    if function_list is None:
        print("Warning: function_list not found in DiagLangH.xml", file=sys.stderr)
        return function_map
    
    # Parse all function_name elements
    for func_name in function_list.findall('function_name'):
        fid = func_name.get('fid')
        if fid is not None:
            # Get the translated name in the specified language
            lang_elem = func_name.find(language)
            if lang_elem is not None and lang_elem.text:
                function_map[fid] = lang_elem.text.strip()
            else:
                # Fallback to English if specified language not found
                fallback = func_name.find('E')
                if fallback is not None and fallback.text:
                    function_map[fid] = fallback.text.strip()
                else:
                    function_map[fid] = f"[Unknown Function {fid}]"
    
    return function_map


def load_item_names(diaglang_file, language='E'):
    """
    Load item name translations from DiagLangH.xml.
    
    Args:
        diaglang_file: Path to DiagLangH.xml file
        language: Language code to use for translation (default: 'E' for English)
    
    Returns:
        dict: Mapping of iid to translated item name
    """
    try:
        tree = ET.parse(diaglang_file)
        root = tree.getroot()
    except ET.ParseError as e:
        print(f"Error parsing DiagLangH.xml: {e}", file=sys.stderr)
        sys.exit(1)
    except FileNotFoundError:
        print(f"DiagLangH.xml not found: {diaglang_file}", file=sys.stderr)
        sys.exit(1)
    
    item_map = {}
    
    # Find item_list element
    item_list = root.find('item_list')
    if item_list is None:
        print("Warning: item_list not found in DiagLangH.xml", file=sys.stderr)
        return item_map
    
    # Parse all item_name elements
    for item_name in item_list.findall('item_name'):
        iid = item_name.get('iid')
        if iid is not None:
            # Get the translated name in the specified language
            lang_elem = item_name.find(language)
            if lang_elem is not None and lang_elem.text:
                item_map[iid] = lang_elem.text.strip()
            else:
                # Fallback to English if specified language not found
                fallback = item_name.find('E')
                if fallback is not None and fallback.text:
                    item_map[iid] = fallback.text.strip()
                else:
                    item_map[iid] = f"[Unknown Item {iid}]"
    
    return item_map


def load_unit_names(diaglang_file, language='E'):
    """
    Load unit name translations from DiagLangH.xml.
    
    Args:
        diaglang_file: Path to DiagLangH.xml file
        language: Language code to use for translation (default: 'E' for English)
    
    Returns:
        dict: Mapping of uid to translated unit name
    """
    try:
        tree = ET.parse(diaglang_file)
        root = tree.getroot()
    except ET.ParseError as e:
        print(f"Error parsing DiagLangH.xml: {e}", file=sys.stderr)
        sys.exit(1)
    except FileNotFoundError:
        print(f"DiagLangH.xml not found: {diaglang_file}", file=sys.stderr)
        sys.exit(1)
    
    unit_map = {}
    
    # Find unit_list element
    unit_list = root.find('unit_list')
    if unit_list is None:
        print("Warning: unit_list not found in DiagLangH.xml", file=sys.stderr)
        return unit_map
    
    # Parse all unit_name elements
    for unit_name in unit_list.findall('unit_name'):
        uid = unit_name.get('uid')
        if uid is not None:
            # Get the translated name in the specified language
            lang_elem = unit_name.find(language)
            if lang_elem is not None and lang_elem.text:
                unit_map[uid] = lang_elem.text.strip()
            else:
                # Fallback to English if specified language not found
                fallback = unit_name.find('E')
                if fallback is not None and fallback.text:
                    unit_map[uid] = fallback.text.strip()
                else:
                    unit_map[uid] = ""
    
    return unit_map


def load_range_names(diaglang_file, language='E'):
    """
    Load range name translations from DiagLangH.xml.
    
    Args:
        diaglang_file: Path to DiagLangH.xml file
        language: Language code to use for translation (default: 'E' for English)
    
    Returns:
        dict: Mapping of rid to translated range name
    """
    try:
        tree = ET.parse(diaglang_file)
        root = tree.getroot()
    except ET.ParseError as e:
        print(f"Error parsing DiagLangH.xml: {e}", file=sys.stderr)
        sys.exit(1)
    except FileNotFoundError:
        print(f"DiagLangH.xml not found: {diaglang_file}", file=sys.stderr)
        sys.exit(1)
    
    range_map = {}
    
    # Find range_list element
    range_list = root.find('range_list')
    if range_list is None:
        print("Warning: range_list not found in DiagLangH.xml", file=sys.stderr)
        return range_map
    
    # Parse all range_name elements
    for range_name in range_list.findall('range_name'):
        rid = range_name.get('rid')
        if rid is not None:
            # Get the translated name in the specified language
            lang_elem = range_name.find(language)
            if lang_elem is not None and lang_elem.text:
                range_map[rid] = lang_elem.text.strip()
            else:
                # Fallback to English if specified language not found
                fallback = range_name.find('E')
                if fallback is not None and fallback.text:
                    range_map[rid] = fallback.text.strip()
                else:
                    range_map[rid] = ""
    
    return range_map


def parse_ecu_file(ecu_file, function_map, item_map, unit_map, range_map):
    """
    Parse ECU diagnostic file and extract skey/function/item information.
    
    Args:
        ecu_file: Path to ECU XML file
        function_map: Dictionary mapping fid to function names
        item_map: Dictionary mapping iid to item names
        unit_map: Dictionary mapping uid to unit names
        range_map: Dictionary mapping rid to range names
    
    Returns:
        list: List of tuples (sid, [(fid, function_name, [items]), ...])
              where items is a list of tuples (item_no, item_lang_id, item_name, qual_sid, qual_lid, param_lists)
              and param_lists is a list of tuples (byte_pos, byte_len, unit_name, bit_pos, bit_len, factor, offset, enum_types, scale_ranges)
              and enum_types is a list of tuples (enum_string, enum_value)
              and scale_ranges is a list of tuples (range_name, up_bound, low_bound)
    """
    try:
        tree = ET.parse(ecu_file)
        root = tree.getroot()
    except ET.ParseError as e:
        print(f"Error parsing ECU file: {e}", file=sys.stderr)
        sys.exit(1)
    except FileNotFoundError:
        print(f"ECU file not found: {ecu_file}", file=sys.stderr)
        sys.exit(1)
    
    results = []
    
    # Find all skey elements
    for skey in root.findall('.//skey'):
        sid = skey.get('sid')
        if sid is None:
            continue
        
        # Build a map of qual_id elements within this skey
        qual_id_map = {}
        for qual_id_elem in skey.findall('qual_id'):
            qid = qual_id_elem.get('qid')
            if qid is not None:
                sid_elem = qual_id_elem.find('sid')
                lid_elem = qual_id_elem.find('lid')
                qual_sid = sid_elem.text if sid_elem is not None and sid_elem.text else ""
                qual_lid = lid_elem.text if lid_elem is not None and lid_elem.text else ""
                
                # Extract param_list information
                param_lists = []
                for param_list in qual_id_elem.findall('param_list'):
                    byte_pos_elem = param_list.find('byte_pos')
                    byte_len_elem = param_list.find('byte_len')
                    unit_id_elem = param_list.find('unit_id')
                    bit_pos_elem = param_list.find('bit_pos')
                    bit_len_elem = param_list.find('bit_len')
                    
                    byte_pos = byte_pos_elem.text if byte_pos_elem is not None and byte_pos_elem.text else ""
                    byte_len = byte_len_elem.text if byte_len_elem is not None and byte_len_elem.text else ""
                    unit_id = unit_id_elem.text if unit_id_elem is not None and unit_id_elem.text else "0"
                    bit_pos = bit_pos_elem.text if bit_pos_elem is not None and bit_pos_elem.text else "-1"
                    bit_len = bit_len_elem.text if bit_len_elem is not None and bit_len_elem.text else "0"
                    
                    # Look up unit name
                    unit_name = unit_map.get(unit_id, "")
                    
                    # Extract scale_type information from conv_list if present
                    factor = ""
                    offset = ""
                    enum_types = []
                    scale_ranges = []
                    conv_list = param_list.find('conv_list')
                    if conv_list is not None:
                        # Get all scale_type elements
                        scale_types = conv_list.findall('scale_type')
                        if scale_types:
                            # Check if we have multiple scale_type elements with range_id
                            has_ranges = False
                            for scale_type in scale_types:
                                range_id_elem = scale_type.find('range_id')
                                if range_id_elem is not None and range_id_elem.text and range_id_elem.text != '0':
                                    has_ranges = True
                                    break
                            
                            if has_ranges:
                                # Extract all scale_type elements with their ranges
                                for scale_type in scale_types:
                                    range_id_elem = scale_type.find('range_id')
                                    up_bound_elem = scale_type.find('up_bound')
                                    low_bound_elem = scale_type.find('low_bound')
                                    
                                    range_id = range_id_elem.text if range_id_elem is not None and range_id_elem.text else "0"
                                    up_bound = up_bound_elem.text if up_bound_elem is not None and up_bound_elem.text else ""
                                    low_bound = low_bound_elem.text if low_bound_elem is not None and low_bound_elem.text else ""
                                    
                                    if range_id != '0':
                                        range_name = range_map.get(range_id, f"[Unknown Range {range_id}]")
                                        scale_ranges.append((range_name, up_bound, low_bound))
                            else:
                                # Single scale_type without range_id - use existing logic
                                scale_type = scale_types[0]
                                factor_elem = scale_type.find('factor')
                                offset_elem = scale_type.find('offset')
                                factor = factor_elem.text if factor_elem is not None and factor_elem.text else ""
                                offset = offset_elem.text if offset_elem is not None and offset_elem.text else ""
                        
                        # Extract enum_type elements
                        for enum_type in conv_list.findall('enum_type'):
                            enum_value_elem = enum_type.find('enum_value')
                            enum_string_elem = enum_type.find('enum_string')
                            if enum_value_elem is not None and enum_string_elem is not None:
                                enum_value = enum_value_elem.text if enum_value_elem.text else ""
                                enum_string = enum_string_elem.text if enum_string_elem.text else ""
                                if enum_value and enum_string:
                                    enum_types.append((enum_string, enum_value))
                    
                    if byte_pos or byte_len:
                        param_lists.append((byte_pos, byte_len, unit_name, bit_pos, bit_len, factor, offset, enum_types, scale_ranges))
                
                qual_id_map[qid] = (qual_sid, qual_lid, param_lists)
        
        functions = []
        
        # Find func_data element
        func_data = skey.find('func_data')
        if func_data is not None:
            # Find all function elements
            for function in func_data.findall('function'):
                fid = function.get('fid')
                if fid is not None:
                    # Look up the function name
                    func_name = function_map.get(fid, f"[Unknown Function ID: {fid}]")
                    
                    # Parse items from groups
                    items = []
                    for group in function.findall('group'):
                        for item_list in group.findall('item_list'):
                            # Extract item information
                            no_elem = item_list.find('no')
                            item_no = no_elem.text if no_elem is not None and no_elem.text else "?"
                            
                            item_lang_id_elem = item_list.find('item_lang_id')
                            if item_lang_id_elem is not None and item_lang_id_elem.text:
                                item_lang_id = item_lang_id_elem.text
                                # Look up the item name
                                item_name = item_map.get(item_lang_id, f"[Unknown Item ID: {item_lang_id}]")
                                
                                # Look up qual_id information
                                qual_id_elem = item_list.find('qual_id')
                                qual_sid = ""
                                qual_lid = ""
                                param_lists = []
                                if qual_id_elem is not None and qual_id_elem.text:
                                    qual_id_value = qual_id_elem.text
                                    if qual_id_value in qual_id_map:
                                        qual_sid, qual_lid, param_lists = qual_id_map[qual_id_value]
                                
                                items.append((item_no, item_lang_id, item_name, qual_sid, qual_lid, param_lists))
                    
                    functions.append((fid, func_name, items))
        
        results.append((sid, functions))
    
    return results


def print_results(results, ecu_file, include_negative=False):
    """
    Print the extracted skey, function, and item information.
    
    Args:
        results: List of tuples (sid, [(fid, function_name, [items]), ...])
        ecu_file: Path to ECU file (for display)
        include_negative: If False, exclude items with negative qual_sid or qual_lid
    """
    print("=" * 80)
    print(f"ECU Function Translation Report")
    print(f"File: {ecu_file}")
    print("=" * 80)
    print()
    
    total_items = 0
    
    for sid, functions in results:
        print(f"SKEY sid=\"{sid}\"")
        
        if functions:
            for fid, func_name, items in functions:
                print(f"  └─ Function fid=\"{fid}\": {func_name}")
                
                if items:
                    # Group items by qual_sid/qual_lid combination
                    qual_groups = {}
                    for item_data in items:
                        item_no, item_lang_id, item_name, qual_sid, qual_lid, param_lists = item_data
                        
                        # Filter out items with negative qual_sid or qual_lid if requested
                        if not include_negative:
                            try:
                                if (qual_sid and int(qual_sid) < 0) or (qual_lid and int(qual_lid) < 0):
                                    continue
                            except (ValueError, TypeError):
                                pass
                        
                        qual_key = (qual_sid, qual_lid)
                        if qual_key not in qual_groups:
                            qual_groups[qual_key] = []
                        qual_groups[qual_key].append(item_data)
                    
                    # Display items grouped by qual_id
                    qual_keys = list(qual_groups.keys())
                    for qual_idx, qual_key in enumerate(qual_keys):
                        qual_sid, qual_lid = qual_key
                        is_last_qual = (qual_idx == len(qual_keys) - 1)
                        qual_prefix = "     └─" if is_last_qual else "     ├─"
                        
                        # Get service name for the qual_sid
                        service_name = get_service_name(qual_sid)
                        if qual_sid or qual_lid:
                            qual_label = f"qual_id (sid={qual_sid}"
                            if service_name:
                                qual_label += f" {service_name}"
                            qual_label += f", lid={qual_lid})"
                        else:
                            qual_label = "qual_id (none)"
                        print(f"{qual_prefix} {qual_label}")
                        
                        qual_items = qual_groups[qual_key]
                        for item_idx, (item_no, item_lang_id, item_name, qual_sid, qual_lid, param_lists) in enumerate(qual_items):
                            is_last_item = (item_idx == len(qual_items) - 1)
                            item_prefix = "          └─" if is_last_item else "          ├─"
                            
                            # Add param_list information if available
                            param_info = ""
                            all_ranges = []  # Collect all ranges from all param_lists
                            
                            if param_lists:
                                param_info_parts = []
                                for bp, bl, unit, bit_pos, bit_len, factor, offset, enum_types, scale_ranges in param_lists:
                                    # Format byte position with bit position if applicable
                                    try:
                                        bit_pos_int = int(bit_pos)
                                        bit_len_int = int(bit_len)
                                        
                                        if bit_pos_int > -1 and bit_len_int > 0:
                                            if bit_len_int == 1:
                                                byte_pos_str = f"{bp}[{bit_pos_int}]"
                                            else:
                                                end_bit = bit_pos_int + bit_len_int - 1
                                                byte_pos_str = f"{bp}[{bit_pos_int}..{end_bit}]"
                                        else:
                                            byte_pos_str = bp
                                    except (ValueError, TypeError):
                                        byte_pos_str = bp
                                    
                                    param_str = f"byte_pos={byte_pos_str}, byte_len={bl}"
                                    if unit:
                                        param_str += f", unit={unit}"
                                    
                                    # Collect scale_ranges for tree display (don't add inline)
                                    if scale_ranges:
                                        all_ranges.extend(scale_ranges)
                                    else:
                                        # Add scale_type information if present (single scale without ranges)
                                        if factor and offset:
                                            try:
                                                # Only show if factor is non-zero or offset is non-zero
                                                factor_float = float(factor)
                                                offset_float = float(offset)
                                                if factor_float != 0 or offset_float != 0:
                                                    param_str += f", scale(factor={factor}, offset={offset})"
                                            except (ValueError, TypeError):
                                                pass
                                    
                                    # Add enum_type information if present
                                    if enum_types:
                                        enum_strs = [f"{enum_string}={enum_value}" for enum_string, enum_value in enum_types]
                                        param_str += f", enum({', '.join(enum_strs)})"
                                    
                                    param_info_parts.append(param_str)
                                param_info = f" [{', '.join(param_info_parts)}]"
                            
                            print(f"{item_prefix} Item {item_no} [iid={item_lang_id}]: {item_name}{param_info}")
                            
                            # Print ranges as nested tree elements if they exist
                            if all_ranges:
                                range_base_indent = "               " if is_last_item else "          │    "
                                for range_idx, (range_name, up_bound, low_bound) in enumerate(all_ranges):
                                    is_last_range = (range_idx == len(all_ranges) - 1)
                                    range_prefix = f"{range_base_indent}└─" if is_last_range else f"{range_base_indent}├─"
                                    # Format range based on whether bounds are equal
                                    if low_bound == up_bound:
                                        range_display = f"[{low_bound}]"
                                    else:
                                        range_display = f"[{low_bound}..{up_bound}]"
                                    print(f"{range_prefix} {range_display} {range_name}")
                            
                            total_items += 1
        else:
            print("  └─ (no functions)")
        
        print()
    
    print("=" * 80)
    print(f"Total skey elements: {len(results)}")
    total_functions = sum(len(funcs) for _, funcs in results)
    print(f"Total functions: {total_functions}")
    print(f"Total items: {total_items}")
    print("=" * 80)


def print_results_csv(results, ecu_file, include_negative=False):
    """
    Print the extracted data in CSV format.
    
    Args:
        results: List of tuples (sid, [(fid, function_name, [items]), ...])
        ecu_file: Path to ECU file (for reference)
        include_negative: If False, exclude items with negative qual_sid or qual_lid
    """
    writer = csv.writer(sys.stdout)
    
    # Write header
    header = ['sid', 'fid', 'function_name', 'iid', 'item_name', 'qual_sid', 'qual_lid', 
              'byte_pos', 'byte_len', 'unit', 'bit_pos', 'bit_len', 'factor', 'offset',
              'range_1_name', 'range_1_low', 'range_1_up',
              'range_2_name', 'range_2_low', 'range_2_up',
              'range_3_name', 'range_3_low', 'range_3_up',
              'range_4_name', 'range_4_low', 'range_4_up',
              'range_5_name', 'range_5_low', 'range_5_up']
    writer.writerow(header)
    
    # Write data rows
    for sid, functions in results:
        if functions:
            for fid, func_name, items in functions:
                if items:
                    for item_no, item_lang_id, item_name, qual_sid, qual_lid, param_lists in items:
                        # Filter out items with negative qual_sid or qual_lid if requested
                        if not include_negative:
                            try:
                                if (qual_sid and int(qual_sid) < 0) or (qual_lid and int(qual_lid) < 0):
                                    continue
                            except (ValueError, TypeError):
                                pass
                        
                        if param_lists:
                            # One row per param_list
                            for bp, bl, unit, bit_pos, bit_len, factor, offset, enum_types, scale_ranges in param_lists:
                                row = [
                                    sid,
                                    fid,
                                    func_name,
                                    item_lang_id,
                                    item_name,
                                    qual_sid if qual_sid else '',
                                    qual_lid if qual_lid else '',
                                    bp if bp else '',
                                    bl if bl else '',
                                    unit if unit else '',
                                    bit_pos if (bit_pos and bit_pos != '-1') else '',
                                    bit_len if bit_len else '',
                                    factor if factor else '',
                                    offset if offset else ''
                                ]
                                
                                # Add range data (up to 5 ranges)
                                for i in range(5):
                                    if i < len(scale_ranges):
                                        rname, up_bound, low_bound = scale_ranges[i]
                                        row.extend([rname, low_bound, up_bound])
                                    else:
                                        row.extend(['', '', ''])
                                
                                writer.writerow(row)
                        else:
                            # Item with no param_lists
                            row = [
                                sid,
                                fid,
                                func_name,
                                item_lang_id,
                                item_name,
                                qual_sid if qual_sid else '',
                                qual_lid if qual_lid else '',
                                '', '', '', '', '', '', ''
                            ]
                            # Empty range columns (5 ranges * 3 columns each = 15)
                            row.extend([''] * 15)
                            writer.writerow(row)


def main():
    """Main entry point."""
    parser = argparse.ArgumentParser(
        description='Parse ECU diagnostic XML files and translate function/item names.',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Language codes:
  J = Japanese, N = Japanese (alt), E = English, G = German, F = French
  S = Spanish, I = Italian, H = Greek, C = Chinese (Simplified)
  W = Chinese (Traditional), Y = Turkish, R = Russian, T = Thai
  V = Vietnamese, K = Korean, L = Polish

Examples:
  %(prog)s MUT3_SE/DiagDB/Ecu/CMU09_DB000000.xml
  %(prog)s --lang G MUT3_SE/DiagDB/Ecu/CMU09_DB000000.xml
  %(prog)s --format csv MUT3_SE/DiagDB/Ecu/CMU09_DB000000.xml
  %(prog)s --lang E --format csv MUT3_SE/DiagDB/Ecu/CMU09_DB000000.xml
        """)
    
    parser.add_argument('ecu_file', 
                        help='Path to ECU diagnostic XML file')
    parser.add_argument('--lang', 
                        default='E',
                        choices=['J', 'N', 'E', 'G', 'F', 'S', 'I', 'H', 'C', 'W', 'Y', 'R', 'T', 'V', 'K', 'L'],
                        help='Language code for translations (default: E)')
    parser.add_argument('--format', 
                        default='tree',
                        choices=['tree', 'csv'],
                        help='Output format: tree or csv (default: tree)')
    parser.add_argument('--include-negative',
                        action='store_true',
                        help='Include items with negative qual_sid or qual_lid values (default: exclude them)')
    
    args = parser.parse_args()
    
    ecu_file = args.ecu_file
    language = args.lang
    output_format = args.format
    
    # Assume DiagLangH.xml is in the same DiagDB directory
    diagdb_dir = os.path.dirname(os.path.dirname(ecu_file))
    diaglang_file = os.path.join(diagdb_dir, 'DiagLangH.xml')
    
    # Check if DiagLangH.xml exists
    if not os.path.exists(diaglang_file):
        print(f"Warning: DiagLangH.xml not found at {diaglang_file}", file=sys.stderr)
        print("Trying alternate location...", file=sys.stderr)
        # Try current directory
        diaglang_file = 'DiagLangH.xml'
        if not os.path.exists(diaglang_file):
            print("Error: Could not locate DiagLangH.xml", file=sys.stderr)
            sys.exit(1)
    
    print(f"Loading function translations from: {diaglang_file}")
    print(f"Language: {language}")
    print()
    
    # Load function name translations
    function_map = load_function_names(diaglang_file, language)
    print(f"Loaded {len(function_map)} function translations")
    
    # Load item name translations
    item_map = load_item_names(diaglang_file, language)
    print(f"Loaded {len(item_map)} item translations")
    
    # Load unit name translations
    unit_map = load_unit_names(diaglang_file, language)
    print(f"Loaded {len(unit_map)} unit translations")
    
    # Load range name translations
    range_map = load_range_names(diaglang_file, language)
    print(f"Loaded {len(range_map)} range translations")
    print()
    
    # Parse ECU file
    results = parse_ecu_file(ecu_file, function_map, item_map, unit_map, range_map)
    
    # Print results in requested format
    if output_format == 'csv':
        print_results_csv(results, ecu_file, args.include_negative)
    else:
        print_results(results, ecu_file, args.include_negative)


if __name__ == "__main__":
    main()
