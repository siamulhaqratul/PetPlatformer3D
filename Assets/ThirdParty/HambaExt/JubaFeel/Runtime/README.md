# JubaFeel Unity Package

**JubaFeel** is a Unity runtime package that provides a flexible framework for creating and managing UI and interactive effects, such as button tap animations and shine transitions, using DOTween. It is designed for easy integration and extension in your Unity projects.

---

## Features

- **JEffect**: Abstract base class for custom effects with show/hide logic and optional delayed start.
- **TapEffect**: Drop-in button tap animation (scaling) for UI Buttons.
- **ShineEffect**: Shine animation for UI elements using DOTween.
- Inspector integration for quick testing and configuration.
- Clean, extensible architecture for building your own effects.

---

## Installation

1. **Dependencies**  
   - [DOTween](http://dotween.demigiant.com/) (required)
   - Unity 2020.3 or newer recommended

2. **Import**  
   - HambaExt unityasset.
   - Ensure DOTween and its modules are installed in your project.

---

## Usage

### 1. TapEffect

Add a scaling tap animation to any UI Button:

- Select your Button in the hierarchy.
- Add the `TapEffect` component.
- (Optional) Configure advanced options in the Inspector.

**How it works:**  
When the button is pressed, it scales down; when released, it scales back up.

### 2. ShineEffect

Animate a "shine" RectTransform across a target UI element:

- Create two UI elements: one for the shine effect, one as the target.
- Add the `ShineEffect` component to a GameObject.
- Assign the `shine` and `target` RectTransforms in the Inspector.
- Use the Inspector button or call `Show()` from code to trigger the effect.


## Inspector Options

- **showOnStart**: Automatically show the effect on Start.
- **useDelay**: Wait for a delay before showing the effect.
- **delayAmount**: Delay time in seconds.

---

## Assembly Definition

The package includes a `.asmdef` file (`JFeel.asmdef`) for modular compilation and dependency management.  
Make sure your project references DOTween and any other required assemblies.

---

## License

This package is provided as-is for use in your Unity projects.  
See the repository or package source for license details.

---

## Support

For questions or issues, please contact the package author or open an issue in your project's repository.

---

**Enjoy smooth and extensible UI effects with JubaFeel!**