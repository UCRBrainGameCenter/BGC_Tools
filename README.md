# BGC Tools

A collection of useful Unity Tools and scripts developed at the UC Riverside Brain Game Center.  Pull requests welcome.

## How To Use

This project has been built largely to be imported as a managed-code plugin.  Within a Unity project, navigate to `${project_root}/Assets/Plugins`, and clone this repository and set it up as a submodule:

```sh
cd ${project_root}/Assets/Plugins
git clone https://github.com/UCRBrainGameCenter/BGC_Tools.git 
git submodule add https://github.com/UCRBrainGameCenter/BGC_Tools.git BGC_Tools
cd ${project_root}
git submodule init
```

## Features

### User Management

* Easily-integrated system for managing multiple distinct user accounts
* User data is serialized into JSON and readily available for inspection, debugging, reproduction

### Audio

* Advanced Audio synthesis library
* Fourier transforms, Spectrogram generation
* Managed code to Read and Write WAV files
* Midi Encoding, Decoding, and Rendering (Alpha)

### Custom Data Structures

* `IDepletable` containers, representing sets of values that can be depleted when taken, but can also be refreshed
  * `DepletableList<T>` will return values from an underlying collection in a fixed order, and can be refreshed to its original state (like a stack that can be reset)
  * `DepletableBag<T>` will return values in a random order from an underlying collection (like shuffle on a music player)
* Efficient `RingBuffer<T>` implementation
* Several forms of `IPool`, for pooling objects

### Mobile, Desktop, and Editor-Friendly File I/O

* Streamline the process of accessing data directories and subdirectories on all platforms with `IO.DataManagement`
* Convenient JSON reading, parsing, and writing with `IO.FileReader` and `IO.FileWriter`
* Zip Compression/Decompression support
* Inclusion of [LightJson](https://github.com/MarcosLopezC/LightJson)

### Mathematics

* Robust implementation of Complex number support with `Mathematics.Complex32` and `Mathematics.Complex64`
* Fast Fourier Transforms (including for sample buffers that are not a power of 2)
* Bezier Curves
* Implementation of common math functions in `Mathematics.GeneralMath`
* Number factorization, least common multiple, and combinatorics
* Numeric Recipe for the Modified Bessel Function of the First Kind

### State Machine

* Fast, fully-programmatic state machine implementation in `StateMachine` namespace.

### UI

* Multi-featured, convenient Modal Dialog in `UI.Dialogs.ModalDialog`
* ModePanels for control of UI Frame Instances in `UI.Panels.ModePanel`

### General Utilities

* Robust Semantic Versioning struct in `Utility.ApplicationVersion`
* Coroutine Management utility in `Utility.CoroutineUtility`
* Local file-browser Scene and Scripts, to allow the inspection of application files on-device

## Updating Documentation

### Ubuntu 

#### Installation

```sh
sudo apt-get install doxygen
npm install moxygen -g
```

#### Running

```sh
./build_wiki.sh
```
