# Pet Platformer 3D

A small 3D platformer prototype starring a cube pet, made for the Nagorik Junior Game Developer technical test.

- **Engine:** Unity 2022.3.62f2 (URP 14)
- **Targets:** WebGL (GitHub Pages) and Android (APK)
- **Input:** Unity Input System (keyboard/gamepad on web, on-screen controls on Android)
- **Camera:** Cinemachine 2.10

## Project layout

```
Assets/
  _Project/            Game content (art, UI, audio, prefabs, scenes, scripts)
    Art/Kenney/        Cube Pets + Platformer Kit (FBX)
    UI/Kenney/         UI Pack sprites, fonts and sounds
  Plugins/Demigiant/   DOTween (free)
  ThirdParty/          In-house utilities: UnityExt, JubaFeel, PoolManager
  Scripts/SampleCode.cs  Coding-style reference (see CLAUDE.md)
```

## Building

- **WebGL:** File > Build Settings > WebGL > Build into `Builds/WebGL`. See [docs/DEPLOY.md](docs/DEPLOY.md) for publishing to GitHub Pages.
- **Android:** File > Build Settings > Android > Build into `Builds/Android/PetPlatformer.apk`. The APK is debug-signed, so no keystore is needed.

## Credits

- 3D models, UI, fonts and sounds: [Kenney](https://kenney.nl) (CC0): Cube Pets, Platformer Kit, UI Pack
- Tweening: [DOTween](http://dotween.demigiant.com) by Demigiant
