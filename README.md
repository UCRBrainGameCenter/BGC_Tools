# BGC Tools

A collection of useful Unity Tools and scripts developed at the UC Riverside Brain Game Center.

Developers:

- [Trevor Stavropoulos](https://tstavropoulos.github.io/) ([@tstavropoulos](https://github.com/tstavropoulos))
- [Colan Biemer](https://bi3mer.github.io/#resume) ([@bi3mer](https://github.com/bi3mer))
- Randy Mester ([@randymester](https://github.com/randymester))
- Lorenzo Alamillo ([@nadnbuds](https://github.com/nadnbuds))
- [Jordan Whiting](https://jaerdon.com/) ([@Jaerdon](https://github.com/Jaerdon))

[Doxygen-built documentation available here](
https://ucrbraingamecenter.github.io/documentation/namespaces.html).

Pull requests are welcome.

## How To Use

This project has been built largely to be imported as a managed-code plugin.  Within a Unity project, navigate to `${project_root}/Assets/Plugins`, and clone this repository and set it up as a submodule:

```sh
cd ${project_root}/Assets/Plugins
git submodule add https://github.com/UCRBrainGameCenter/BGC_Tools.git
```

If you're not familiar with using git submodules, [check out Trevor's blog post on submodules](https://tstavropoulos.github.io/blargs/git_submodules.html).

Take a look at our MIT Licensed [CleanBGCProject Unity Demo Project](https://github.com/UCRBrainGameCenter/CleanBGCProject) to see some of these tools in action.

## Features

### User Management

- Easily-integrated system for managing multiple distinct user profiles in `Users.PlayerData`
- User data is serialized into JSON and readily available for inspection, debugging, reproduction

### Audio

- Advanced Audio synthesis library (Check out examples in `Tests.SynthesisTests`)
- Fourier transforms, Spectrogram generation
- Managed code to Read and Write WAV files in `Audio.WaveEncoding`
- Midi Encoding, Decoding, and Rendering (Alpha) in `Audio.Midi.MidiEncoding`

### Custom Data Structures

- `IDepletable` containers, representing sets of values that can be depleted when taken, but can also be refreshed (located in `DataStructures.Generic`)
  - `DepletableList<T>` will return values from an underlying collection in a fixed order, and can be refreshed to its original state (like a stack that can be reset)
  - `DepletableBag<T>` will return values in a random order from an underlying collection (like shuffle on a music player)
- Efficient `RingBuffer<T>` implementation
- Several forms of `IPool`, for pooling objects

### Mobile, Desktop, and Editor-Friendly File I/O

- Streamline the process of accessing data directories and subdirectories on all platforms with `IO.DataManagement`
- Convenient JSON reading, parsing, and writing with `IO.FileReader` and `IO.FileWriter`
- Zip Compression/Decompression support
- Inclusion of [LightJson](https://github.com/MarcosLopezC/LightJson)

### Mathematics

- Robust implementation of Complex number support with `Mathematics.Complex32` and `Mathematics.Complex64`
- Fast Fourier Transforms (including for sample buffers that are not a power of 2)
- Bezier Curves
- Implementation of common math functions in `Mathematics.GeneralMath`
- Number factorization, least common multiple, and combinatorics
- Numeric Recipe for the Modified Bessel Function of the First Kind

### State Machine

- Fast, fully-programmatic state machine implementation in `StateMachine` namespace.
- Sample implementation included in [README.md](StateMachine/README.md).

### UI

- Multi-featured, convenient Modal Dialog in `UI.Dialogs.ModalDialog`
- ModePanels for control of UI Frame Instances in `UI.Panels.ModePanel`

### General Utilities

- Robust Semantic Versioning struct in `Utility.ApplicationVersion`
- Coroutine Management utility in `Utility.CoroutineUtility`
- Local file-browser Scene and Scripts, to allow the inspection of application files on-device (`UnityAssets/Scenes/FileBrowser.unity`)
