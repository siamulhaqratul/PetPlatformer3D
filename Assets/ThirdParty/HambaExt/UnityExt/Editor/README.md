# HambaExt UnityExt Editor Tools

A comprehensive collection of Unity Editor extensions and utilities designed to improve workflow efficiency, asset management, and project maintenance.

## Installation

1. Import the `HambaExt.unitypackage` file into your Unity project
2. Ensure the `UnityExtEditor.asmdef` file is correctly referenced in your project
3. For Addressables functionality, make sure you have the Addressables package installed from the Package Manager

## Features

### Asset Management

- **AssetGUIDRegenerator**  
  Regenerate GUIDs for assets to fix reference issues when duplicating content.
  - Access via: `Assets > Create > Kaiyum > UnityExt > Regenerate GUID`
  - Select assets, click the menu item, confirm, and references will be updated
  - When to use, read `https://github.com/jeffjads/unity-guid-regenerator/blob/master/README.md`

- **BatchFbxOperation**  
  Batch process FBX import settings across multiple files.
  - Access via: `Tools > UnityExt > Batch operations on FBX models`
  - Select FBX files, adjust settings, and apply to all at once

- **DependenciesHunter**  
  Find unused assets or trace asset dependencies.
  - Access via: `Tools > Dependencies Hunter`
  - Locate unreferenced assets to clean up your project

- **ExportSubSprites**  
  Extract individual sprites from sprite sheets as separate files.
  - Access via: `Assets > Export Sub-Sprites`
  - Select a sprite sheet and export all sub-sprites to a folder
  - Enable 'Read/Write Enabled' in the texture import settings before

### Project Maintenance

- **MissingReferences Tools**  
  A suite of tools to find and fix missing references in your project.
  - **MissingPrefabsFinder**: Find prefabs with missing connections
    - Access via: `Tools > Find Missing Prefabs`
  - **MissingReferencesFinder**: Locate missing script references
    - Access via: `Tools > Find Missing References`
  - **MissingScriptWindow**: Interactive window to fix missing scripts
    - Access via: `Tools > Find missing scripts`

- **SaveDataClear**  
  Clear all saved player data during development.
  - Access via: `Tools > UnityExt > Clear Player Storage Save Data`
  - Useful for testing different save states

### Build Pipeline

- **BuildAddressablesProcessor**  
  Automatically build addressable asset bundles when building your project.
  - Integrates with Unity's build pipeline
  - Requires Addressables package

- **LevelLightingBaker**  
  Batch bake lighting for multiple scenes.
  - Access via: `Hamba > Bake All Scene's Lights`
  - Select scenes folder and process all scenes at once

## Usage Examples

### Finding and Fixing Missing References

```
1. Navigate to Tools > Find Missing References
2. Select the folder or scene to scan
3. Review the results in the Missing References Console Window
4. Click on items to select and fix them in your project
```

### Batch Processing FBX Files

```
1. Navigate to Tools > UnityExt > Batch operations on FBX models
2. Use "Browse" to select multiple FBX models
3. Configure the desired import settings
4. Click "Process" to update all selected files at once
```

### Regenerating Asset GUIDs

```
1. Select one or more assets in the Project window
2. Navigate to Assets > Create > Kaiyum > UnityExt > Regenerate GUID
3. Confirm the operation
4. Unity will update all references to these assets
```

## Requirements

- Unity 2019.4 or newer
- Some features require additional packages:
  - Addressables package for BuildAddressablesProcessor
  - TextMeshPro for certain templates

## Notes

- Always back up your project before using tools that modify multiple assets
- Some operations may take time on larger projects
- The functionality of certain tools may vary based on your Unity version

## License

This package is provided as part of the HambaExt tool suite. See the project repository for detailed license information.