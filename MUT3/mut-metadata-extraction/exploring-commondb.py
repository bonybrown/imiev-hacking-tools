import os
import xml.etree.ElementTree as ET
from pprint import pprint

def find_vin_search_file(vin: str, commonDB_path: str) -> str:
    """
    Identifies the correct VIN_Search_XXX.xml file for the given VIN using VIN_Selection.xml
    Args:
        vin: The VIN to search for
        commonDB_path: The path to the CommonDB directory
    Returns:
        The path to the VIN_Search_XXX.xml file
    """

    selection_file = os.path.join(commonDB_path, "VIN_Selection.xml")

    for file_elem in ET.parse(selection_file).find("ALL").findall("FILE"):
        vin_from = file_elem.find("VIN_FROM").text
        vin_to = file_elem.find("VIN_TO").text
        if vin_from <= vin <= vin_to:
            # swap extension from .exdf to .xml
            return file_elem.find("NAME").text.replace(".exdf", ".xml")

def lookup_vin(vin: str, search_file_name: str, commonDB_path: str) -> dict:
    """
    Looks up the VIN in the appropriate VIN_Search_XXX.xml file
    Args:
        vin: The VIN to search for
        search_file_name: The name of the VIN_Search_XXX.xml file
        commonDB_path: The path to the CommonDB directory
    Returns:
        A dictionary containing the VIN information
    """
    search_file = os.path.join(commonDB_path, search_file_name)

    for vi_elem in ET.parse(search_file).findall("VI"):
        if vi_elem.get("VN") == vin:
            return {
                "VehicleYear": vi_elem.find("MY").text,
                "VehicleType": vi_elem.find("TY").text,
                "VehicleKind": vi_elem.find("KD").text,
            }

def vehicle_config(destination: str, vin_info: dict, commonDB_path: str) -> dict:
    """
    Looks up the vehicle information from the Vehicle_DB.xml file
    Args:
        destination: The destination ID
        vin_info: Dictionary containing VehicleType, VehicleKind, VehicleYear
        commonDB_path: The path to the CommonDB directory
    Returns:
        A dictionary containing the vehicle information.
    """
    vehicle_db_file = os.path.join(commonDB_path, "Vehicle_DB.xml")
    tree = ET.parse(vehicle_db_file)
    for vehicles_for_destination in tree.find("CONFIG").findall("ID"):
        if vehicles_for_destination.get("DSTN_ID") == destination:
            for vehicle in vehicles_for_destination.findall("VHCL"):
                if (
                    vehicle.get("TYPE") == vin_info["VehicleType"]
                    and vehicle.get("KIND") == vin_info["VehicleKind"]
                    and vehicle.get("MODEL_YR") == vin_info["VehicleYear"]
                ):
                    return {
                        "VHCL_ID": vehicle.find("VHCL_ID").text,
                        "ENGINE_ID": vehicle.find("ENGINE_ID").text,
                        "TRANSMISSION_ID": vehicle.find("TRANSMISSION_ID").text,
                        "VHCL_VIEW": vehicle.find("VHCL_VIEW").text,
                        "MAKER_ID": vehicle.find("MAKER_ID").text,
                        "DCSMV": vehicle.find("DCSMV").text,
                        "VEHICLE_FAMILY": vehicle.find("VEHICLE_FAMILY").text,
                    }


def canbus_spec(destination: str, vin_info: dict, commonDB_path: str) -> dict:
    """
    Looks up CAN bus specification using destination-aware search
    Args:
        destination: The destination ID (e.g., "002" for NAFTA).
        vin_info: Dictionary with VehicleType, VehicleKind, VehicleYear.
        commonDB_path: Path to the CommonDB directory.
    Algorithm:
        1. Find all CANBUS_SPEC entries matching Type/Kind/ModelYear
        2. If only 1 match exists, return it (regardless of DSTN_ID)
        3. If multiple matches exist, return the one matching the destination argument
    Returns:
        Dictionary with CANBUS_ID and PASSTHRU, or None if not found.
    """
    vehicle_db_file = os.path.join(commonDB_path, "Vehicle_DB.xml")
    tree = ET.parse(vehicle_db_file)
    
    # Collect all matching entries from all DSTN_ID sections
    matches = []
    for id_elem in tree.find("CANBUS_SPEC").findall("ID"):
        dstn_id = id_elem.get("DSTN_ID")
        for vhcl in id_elem.findall("VHCL"):
            if (
                vhcl.get("TYPE") == vin_info["VehicleType"]
                and vhcl.get("KIND") == vin_info["VehicleKind"]
                and vhcl.get("MODEL_YR") == vin_info["VehicleYear"]
            ):
                matches.append({
                    "DSTN_ID": dstn_id,
                    "CANBUS_ID": vhcl.find("CANBUS_ID").text,
                    "PASSTHRU": vhcl.find("PASSTHRU").text,
                })
    
    # If only 1 match, return it
    if len(matches) == 1:
        return {"CANBUS_ID": matches[0]["CANBUS_ID"], "PASSTHRU": matches[0]["PASSTHRU"]}   
    # If multiple matches, find the one for the given destination
    elif len(matches) > 1:
        for match in matches:
            if match["DSTN_ID"] == destination:
                return {"CANBUS_ID": match["CANBUS_ID"], "PASSTHRU": match["PASSTHRU"]}

def engine_name(engine_id: str, commonDB_path: str) -> str:
    """
    Looks up the engine name from the ENGINE_DB.xml file.
    Args:
        engine_id: The ID of the engine.
        commonDB_path: The path to the CommonDB directory.
    Returns:
        The name of the engine.
    """
    engine_db_file = os.path.join(commonDB_path, "Vehicle_DB.xml")
    tree = ET.parse(engine_db_file)
    for engine in tree.find("CONFIG_ENGINE").findall("ID"):
        if engine.get("ENGINE_ID") == engine_id:
            return engine.find("ENGINE_NM").text
    print(f"Error: Engine not found in {engine_db_file}")


def transmission_name(transmission_id: str, commonDB_path: str) -> str:
    """
    Looks up the transmission name from the TRANSMISSION_DB.xml file.
    Args:
        transmission_id: The ID of the transmission.
        commonDB_path: The path to the CommonDB directory.
    Returns:
        The name of the transmission.
    """
    transmission_db_file = os.path.join(commonDB_path, "Vehicle_DB.xml")
    tree = ET.parse(transmission_db_file)
    for transmission in tree.find("CONFIG_TRANSMISSION").findall("ID"):
        if transmission.get("TRANSMISSION_ID") == transmission_id:
            return transmission.find("TRANSMISSION_NM").text
    print(f"Error: Transmission not found in {transmission_db_file}")


def model_name(model_id: str, destination: str, commonDB_path: str) -> str:
    """
    Looks up the model name from the MODEL_DB.xml file.
    Args:
        model_id: The ID of the model.
        destination: The destination of the vehicle.
        commonDB_path: The path to the CommonDB directory.
    Returns:
        The name of the model.
    """
    model_db_file = os.path.join(commonDB_path, "Vehicle_DB.xml")
    tree = ET.parse(model_db_file)
    for model in tree.find("MST_VHCL").findall("ID"):
        if model.get("DSTN_ID") == destination:
            for model in model.findall("VHCL"):
                if model.get("VHCL_ID") == model_id:
                    return model.find("VHCL_NM").text
    print(f"Error: Model name not found in {model_db_file}")


def canbus_info(canbus_id: str, diagDB_path: str) -> dict:
    """
    Looks up CAN bus information from DiagDB/DiagCanbus.xml file.
    Args:
        canbus_id: The CAN bus ID to look up (matches @cid attribute).
        diagDB_path: The path to the DiagDB directory.
    Returns:
        A dictionary containing parts_id values, their can_id_mon elements, and names.
    """
    diagcanbus_file = os.path.join(diagDB_path, "DiagCanbus.xml")
    tree = ET.parse(diagcanbus_file)
    
    # Find the <canbus> element with matching @cid attribute
    canbus_elem = tree.find(f".//canbus[@cid='{canbus_id}']")
    if canbus_elem is not None:
        parts_list = []
        # Iterate through all <parts_info> elements to find those with <parts_id>
        for parts_info_elem in canbus_elem.iter('parts_info'):
            parts_id_elem = parts_info_elem.find('parts_id')
            if parts_id_elem is not None:
                parts_entry = {
                    "parts_id": parts_id_elem.text
                }
                # Check if <can_id_mon> element exists as a sibling
                can_id_mon_elem = parts_info_elem.find('can_id_mon')
                if can_id_mon_elem is not None:
                    parts_entry["can_id_mon"] = can_id_mon_elem.text
                
                # Look up the parts name by matching pid attribute to parts_id
                parts_name_elem = tree.find(f".//parts_name[@pid='{parts_id_elem.text}']")
                if parts_name_elem is not None:
                    name_elem = parts_name_elem.find('name')
                    if name_elem is not None:
                        parts_entry["name"] = name_elem.text
                
                parts_list.append(parts_entry)
        
        return {
            "canbus_id": canbus_id,
            "parts": parts_list
        }
    
    print(f"Error: CAN bus ID {canbus_id} not found in {diagcanbus_file}")
    return None


def destination_name(destination: str, commonDB_path: str) -> str:
    """
    Looks up the destination name from the Vehicle_DB.xml file.
    Args:
        destination: The destination of the vehicle.
        commonDB_path: The path to the CommonDB directory.
    Returns:
        The name of the destination.
    """
    dest_db_file = os.path.join(commonDB_path, "Vehicle_DB.xml")
    tree = ET.parse(dest_db_file)
    for dest in tree.find("MST_DSTN").findall("ID"):
        if dest.get("DSTN_ID") == destination:
            return dest.find("DSTN_NM").text
    print(f"Error: Destination name not found in {dest_db_file}")


def maker_name(maker_id: str, commonDB_path: str) -> str:
    """
    Looks up the maker name from the MAKER_DB.xml file.
    Args:
        maker_id: The ID of the maker.
        commonDB_path: The path to the CommonDB directory.
    Returns:
        The name of the maker.
    """
    maker_db_file = os.path.join(commonDB_path, "Vehicle_DB.xml")
    tree = ET.parse(maker_db_file)
    for maker in tree.find("MST_MAKER").findall("ID"):
        if maker.get("MAKER_ID") == maker_id:
            return maker.find("MAKER_NM").text
    print(f"Error: Maker name not found in {maker_db_file}")


def vehicle_kind_name(vehicle_kind: str, vehicle_type: str, diagDB_path: str) -> dict:
    """
    Looks up the vehicle kind and type names from the DiagDB/VehicleMas.xml file.
    Args:
        vehicle_kind: The vehicle kind ID to look up.
        vehicle_type: The vehicle type ID to look up.
        diagDB_path: The path to the DiagDB directory.
    Returns:
        A dictionary with 'kind' and 'type' keys.
    """
    vehicle_mas_file = os.path.join(diagDB_path, "VehicleMas.xml")
    tree = ET.parse(vehicle_mas_file)
    
    result = {}
    
    # Look up kind
    kind_mas_elem = tree.find(f".//kind_mas[@id='{vehicle_kind}']")
    if kind_mas_elem is not None:
        kind_elem = kind_mas_elem.find('kind')
        if kind_elem is not None:
            result['kind'] = kind_elem.text
    else:
        print(f"Error: Vehicle kind {vehicle_kind} not found in {vehicle_mas_file}")
    
    # Look up type
    type_mas_elem = tree.find(f".//type_mas[@id='{vehicle_type}']")
    if type_mas_elem is not None:
        type_elem = type_mas_elem.find('type')
        if type_elem is not None:
            result['type'] = type_elem.text
    else:
        print(f"Error: Vehicle type {vehicle_type} not found in {vehicle_mas_file}")
    
    return result if result else None


def vehicle_vkey(vehicle_type: str, vehicle_kind: str, vehicle_year: str, diagDB_path: str) -> str:
    """
    Looks up the vehicle key (vkey) from the DiagDB/DiagVehicle.xml file.
    Args:
        vehicle_type: The vehicle type name (e.g., 'HA3W').
        vehicle_kind: The vehicle kind name (e.g., 'LDDR8').
        vehicle_year: The vehicle year (e.g., '12' for 2012).
        diagDB_path: The path to the DiagDB directory.
    Returns:
        The vkey string.
    """
    diag_vehicle_file = os.path.join(diagDB_path, "DiagVehicle.xml")
    tree = ET.parse(diag_vehicle_file)
    
    # Convert 2-digit year to 4-digit year (e.g., '12' -> '2012')
    model_year = f"20{vehicle_year}"
    
    # Find the model_year element with matching mid attribute
    for my_elem in tree.findall(f".//model_year[@mid='{model_year}']"): 
        # Find all type elements with matching tid attribute within this model_year
        for type_elem in my_elem.findall(f".//type[@tid='{vehicle_type}']"): 
            # Iterate through all kind elements under this type to find matching kid
            for kind_elem in type_elem.iter('kind'):
                if kind_elem.get('kid') == vehicle_kind:
                    vkey_elem = kind_elem.find('vkey')
                    if vkey_elem is not None:
                        return vkey_elem.text
    
    print(f"Error: Vehicle combination year={model_year}, type={vehicle_type}, kind={vehicle_kind} not found in {diag_vehicle_file}")
    return None


def diag_system_info(vkey: str, diagDB_path: str) -> list:
    """
    Looks up diagnostic system information from DiagDB/DiagSystem.xml file.
    Args:
        vkey: The vehicle key to look up.
        diagDB_path: The path to the DiagDB directory.
    Returns:
        A list of dictionaries containing ECU information with ecu_id, ecu_name, tx_can_id, and rx_can_id.
    """
    diag_system_file = os.path.join(diagDB_path, "DiagSystem.xml")
    tree = ET.parse(diag_system_file)
    
    ecu_list = []
    
    # Find the vkey_list element with matching vid attribute
    vkey_list_elem = tree.find(f".//vkey_list[@vid='{vkey}']")
    if vkey_list_elem is not None:
        # Get all mut_class ids from syskey_list elements
        for syskey_elem in vkey_list_elem.findall('syskey_list'):
            mut_class_elem = syskey_elem.find('mut_class')
            if mut_class_elem is not None:
                mid = mut_class_elem.text
                
                # Find the mut_class_list element with matching mid attribute
                mut_class_list = tree.find(f".//mut_class_list[@mid='{mid}']")
                if mut_class_list is not None:
                    ecu_info = {}
                    
                    # Get tx and rx can ids
                    tx_elem = mut_class_list.find('tx_can_id')
                    rx_elem = mut_class_list.find('rx_can_id')
                    if tx_elem is not None:
                        ecu_info["tx_can_id"] = tx_elem.text
                    if rx_elem is not None:
                        ecu_info["rx_can_id"] = rx_elem.text
                    
                    # Get ecu_id from skey_list
                    skey_list_elem = mut_class_list.find('.//skey_list')
                    if skey_list_elem is not None:
                        ecu_id_elem = skey_list_elem.find('ecu_id')
                        if ecu_id_elem is not None:
                            ecu_id = ecu_id_elem.text
                            ecu_info["ecu_id"] = ecu_id
                            
                            # Look up ecu_name from ecu_list
                            ecu_list_elem = tree.find(f".//ecu_list[@eid='{ecu_id}']")
                            if ecu_list_elem is not None:
                                ecu_name_elem = ecu_list_elem.find('ecu_name')
                                if ecu_name_elem is not None:
                                    ecu_info["ecu_name"] = ecu_name_elem.text
                    
                    ecu_list.append(ecu_info)
    
    if not ecu_list:
        print(f"Warning: No ECU information found for vkey {vkey} in {diag_system_file}")
    
    return ecu_list

import sys
if __name__ == "__main__":
    vin = sys.argv[1]  # read value from argv[1]
    commonDB_path = sys.argv[2]  # read value from argv[2]
    diagDB_path = os.path.join(commonDB_path,"..", "DiagDB")  # assuming DiagDB is a sibling directory to CommonDB
    # Retrieved from Vehicle_DB.xml MST_DSTN element
    # - 001 is Japan
    # - 002 is North America
    # - 003 is Europe
    # - 004 is Export
    # - 008 is Australia
    destination = "008"
    search_file_name = find_vin_search_file(vin, commonDB_path)
    vin_info = lookup_vin(vin, search_file_name, commonDB_path)
    vehicle_config = vehicle_config(destination, vin_info, commonDB_path)
    canbus_spec = canbus_spec(destination, vin_info, commonDB_path)
    canbus_info = canbus_info(canbus_spec["CANBUS_ID"], diagDB_path)
    engine_name = engine_name(vehicle_config["ENGINE_ID"], commonDB_path)
    transmission_name = transmission_name(vehicle_config["TRANSMISSION_ID"], commonDB_path)
    model_name = model_name(vehicle_config["VHCL_ID"], destination, commonDB_path)
    destination_name = destination_name(destination, commonDB_path)
    maker_name = maker_name(vehicle_config["MAKER_ID"], commonDB_path)
    vehicle_mas_info = vehicle_kind_name(vin_info["VehicleKind"], vin_info["VehicleType"], diagDB_path)
    vkey = vehicle_vkey(vehicle_mas_info.get('type'), vehicle_mas_info.get('kind'), vin_info["VehicleYear"], diagDB_path)
    diag_system = diag_system_info(vkey, diagDB_path)
    print("VIN information:")
    pprint(vin_info)
    print("vehicle configuration:")
    pprint(vehicle_config)
    print("canbus specification:")
    pprint(canbus_spec)
    print(f"engine name: {engine_name}")
    print(f"transmission name: {transmission_name}")
    print(f"model name: {model_name}")
    print(f"destination name: {destination_name}")
    print(f"maker name: {maker_name}")
    print(f"vehicle kind: {vehicle_mas_info.get('kind')}")
    print(f"vehicle type: {vehicle_mas_info.get('type')}")
    print(f"vehicle key (vkey): {vkey}")
    print("canbus info:")
    pprint(canbus_info)
    print("diagnostic system info:")
    pprint(diag_system) 