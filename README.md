# Pet Platformer 3D

A small 3D platformer prototype starring a cube pet, made for the Nagorik Junior Game Developer technical test.

- **Engine:** Unity 2022.3.62f2 (URP 14)
- **Targets:** WebGL (GitHub Pages) and Android (APK)
- **Input:** Unity Input System (keyboard/gamepad on web, on-screen controls on Android)
- **Camera:** Cinemachine 2.10

## How to play

Pet Shift Run: the pet runs forward on its own. Steer between the two lanes, jump the gaps and spikes, and pick an animal at each of the two gate pairs. Reach the finish flag with 3 lives.

| Action | Keyboard / gamepad | Touch |
| --- | --- | --- |
| Steer | A / D, Left / Right, left stick | Swipe left / right |
| Jump (hold for a higher jump) | Space, W, Up, South button | Swipe up |
| Roll / duck | S, Down, East button | Swipe down |
| Pause | Esc, P, Start | Pause button |

| Animal | Where | Ability |
| --- | --- | --- |
| Dog | Start form | Balanced run and jump |
| Bunny | Gate 1 | High jump |
| Cat | Gate 1 | Fast lane changes |
| Monkey | Gate 2 | Climbs tall ledges without jumping |
| Penguin | Gate 2 | Long belly slide |

**Stars:** one for reaching the flag, one for collecting 50 of the 63 coins, one for losing no lives. The best star count is saved.

**Score:** up to 1000 for distance, 25 per coin and 250 for each life left at the flag, so a perfect run scores 3325. The best score is saved and shown on the title screen.

## What was cut

The plan had a Bee (glide) and an Elephant (smashing crate walls). Both needed new movement code, so they were cut. Every gate animal is the Dog with one strength turned up, which left time for the jump feel, the course and the UI.

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

- 3D models, UI, fonts and sounds: [Kenney](https://kenney.nl) (CC0): Cube Pets, Platformer Kit, UI Pack, Digital Audio, Impact Sounds, Music Jingles
- Music: "Retro Game" by The_Mountain, from [Pixabay](https://pixabay.com) (Pixabay Content License)
- Tweening: [DOTween](http://dotween.demigiant.com) by Demigiant
