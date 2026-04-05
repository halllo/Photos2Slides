# Photos2Slides

This experimental application explores dotent MAUI on maccatalyst capabilities.

- ✅ Accessing Photo Library
- 🚧 Detect Slide & Deskew
- 🚧 Create a Slide Deck

## Problems

### ❌ PPTX creation on macos

#### ❌ Aspose.Slides.NET

We get this error:

```log
Unhandled exception. System.TypeInitializationException: The type initializer for 'Gdip' threw an exception.
 ---> System.PlatformNotSupportedException: System.Drawing.Common is not supported on non-Windows platforms. See https://aka.ms/systemdrawingnonwindows for more information.
   at System.Drawing.LibraryResolver.EnsureRegistered()
   at System.Drawing.SafeNativeMethods.Gdip.PlatformInitialize()
   at System.Drawing.SafeNativeMethods.Gdip..cctor()
   --- End of inner exception stack trace ---
   at System.Drawing.SafeNativeMethods.Gdip.GdipCreateBitmapFromScan0(Int32 width, Int32 height, Int32 stride, Int32 format, IntPtr scan0, IntPtr& bitmap)
   at.(Size , )
   at.(Size )
   at Aspose.Slides.Slide.GetThumbnail(Size imageSize)
   at .( , IPresentation, IPptxOptions )
   at .(Presentation , , IPptxOptions , InterruptionToken )
   at .(Presentation , Stream,  , IPptxOptions , InterruptionToken )
   at Aspose.Slides.Presentation.(Stream , , IPptxOptions )
   at Aspose.Slides.Presentation.Save(Stream stream, SaveFormat format, ISaveOptions options)
   at Aspose.Slides.Presentation.Save(String fname, SaveFormat format)
```

The error remains even after `brew install mono-libgdiplus`.

Associated Aspose Tickets:

- [Aspose.Slides.Presentation Throws “The Type Initializer for ‘Gdip’ Threw Exception”](https://forum.aspose.com/t/aspose-slides-presentation-throws-the-type-initializer-for-gdip-threw-exception/258583/9)

To circumvent this, we export only images and run the slides generation in an ubunto container with all relevant libgdip packages installed.

For that we use this [devcontainer](.devcontainer/devcontainer.json) and install libgdiplus:

```bash
sudo apt update && sudo apt install -y libgdiplus
```

To verify it was successfully installed:

```bash
dpkg -l | grep libgdiplus
```

Unfortunately, when we run the Aspose.Slides app, we still get the same error inside the devcontainer:

```log
Unhandled exception. System.TypeInitializationException: The type initializer for 'Gdip' threw an exception.
 ---> System.PlatformNotSupportedException: System.Drawing.Common is not supported on non-Windows platforms. See https://aka.ms/systemdrawingnonwindows for more information.
```

It seems we really have to use Aspose.Slides on Windows.

Or use a different pptx creator.

#### ✅ IronPPT

[Details](https://www.nuget.org/packages/IronPPT).

## Alternatives

Two scripts are available to export the latest photo from the macOS Photo Library:

### AppleScript (Recommended)

```bash
osascript export_latest_photo_applescript.scpt
```

- **Pros**: Simple, macOS handles permissions automatically
- **Cons**: Slower for large photo libraries (iterates through all photos)
- Exports to `./exported/` folder

### Python with PyObjC

```bash
uv run export_latest_photo.py
```

- **Pros**: Direct API access, potentially faster
- **Cons**: Requires manual permission setup (Terminal needs Full Disk Access)
- Uses PEP 723 inline dependencies (managed by `uv`)

**Permission setup for Python:**

1. Open System Settings → Privacy & Security → Full Disk Access
2. Enable your Terminal app
3. Restart Terminal
