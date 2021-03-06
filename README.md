# Spinner Demo

A simple spinner UI application made in Unity3D.

Checkout the WebGL(wasm) demo [here](https://angelkyriako.github.io/Spinner).

## Screenshots

![Idle State](Screenshots/IdleState.JPG)
![Spin Started State](Screenshots/SpinStartedState.JPG)
![Spin Stopping State](Screenshots/SpinStoppingState.JPG)
![Spin Stopped State](Screenshots/SpinStoppedState.JPG)

## Features

- A simple HTTP client.
- Virtual server settings.
  - Allows to simulate of the server's results and delay in seconds
- Custom Spinning animation
  - Discrete: Scrolls block by block.
  - Continuous: Scrolls in real time.
- A powerful animation system.
  - Each Animation is a scriptable object stored on disk in a .asset file. That way they can be tweaked during play mode without losing their values.
  - Animation Groups contain multiple animations that may be run in parallel or sequentialy.
  - Animation Groups are also animations and therefore can be children of other Animation Groups.

## Libraries used
- [JSON.NET For Unity (Newtonsoft)](https://assetstore.unity.com/packages/tools/input-management/json-net-for-unity-11347)
- [DOTween](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676)
- [Explosive Toon VFX Texture](https://assetstore.unity.com/packages/vfx/particles/fire-explosions/explosive-toon-vfx-texture-free-11117)  
