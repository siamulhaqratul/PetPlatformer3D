# Pet Platformer 3D

A small 3D platformer prototype starring a cube pet, made for the Nagorik Junior Game Developer technical test.

- **Engine:** Unity 2022.3.62f2 (URP 14)
- **Targets:** WebGL (GitHub Pages) and Android (APK)
- **Input:** Unity Input System (keyboard/gamepad on web, on-screen controls on Android)
- **Camera:** Cinemachine 2.10

## How to play

Pet Shift Run: the pet runs forward on its own. Steer between the two lanes, jump the gaps and spikes, and pick an animal at each gate pair. Reach the finish flag with 3 lives. You earn a star for finishing, a star for collecting enough coins, and a star for losing no lives.

| Action | Keyboard / gamepad | Touch |
| --- | --- | --- |
| Steer | A / D, Left / Right, left stick | Swipe left / right |
| Jump (hold to glide as the Bee) | Space, W, Up, South button | Swipe up |
| Roll / duck | S, Down, East button | Swipe down |
| Pause | Esc, P, Start | Pause button |

| Animal | Ability |
| --- | --- |
| Dog | Start form, balanced run and jump |
| Bunny | High jump |
| Bee | Hold jump to glide |

## What was cut

The plan had five animals. The Elephant (smashing crate walls) and the Penguin (sliding on snow) were cut, so the time could go into making jumping and the three remaining animals feel good.

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
