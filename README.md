# LinuxPls
> [!WARNING]
> This tool modifies the VRChatSDK, which is [directly against the terms of service.](https://hello.vrchat.com/legal#:~:text=or%20attempt%20to%20make%20any%20modification%20to%20any%20portion%20of%20the%20Platform)

Yet another VRChat SDK Patch for Linux support, using [Harmony](https://github.com/pardeike/Harmony) and [umu-launcher](https://github.com/Open-Wine-Components/umu-launcher). \
[Please vote for an official solution on the VRChat Canny!](https://feedback.vrchat.com/sdk-bug-reports/p/add-proton-support-to-the-sdk-for-local-tests)

**This only patches the VRChat Worlds SDK!**\
I haven't tested the Avatars SDK, but from what I can tell it seems to work fine on Linux.

# Usage
1. Install `umu-launcher` from your distributions package manager
2. Install the patch into your project in your preferred way
    - Using ALCOM
        1. Add this package to ALCOM using [the website](https://peri-perihelion.github.io/LinuxPls)
        2. Install the package with ALCOM
    - Manually
        1. Download the unitypackage from [Releases](https://github.com/peri-perihelion/LinuxPls/releases)
        
Thats it! Your VRChat client should be set automatically. Additional settings (such as targeting a specific Proton version) are avaliable in the VRChatSDK Settings tab.

## Special Thanks
- [BeffudledLabs LinuxVRChatSDKPatch](https://github.com/BefuddledLabs/LinuxVRChatSDKPatch), for creating the patch that this is based on.
- [Bartkk0's VRCSDKonLinux](https://github.com/Bartkk0/VRCSDKonLinux), which laid the ground work for patching the SDK with Linux support.
