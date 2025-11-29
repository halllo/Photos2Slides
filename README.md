# Photos2Slides

This experimental application explores dotent MAUI on maccatalyst capabilities.

- 🚧 Accessing Photo Library
- 🚧 Detect Slide & Deskew
- 🚧 Create a Slide Deck

## Problems

## ❌ Aspose.Slides.NET cannot use GDIP on macos

On macos we get this error:

```log
Hello, World!
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

To circumvent this, we can export only images and run the slides generation in an ubunto container with all relevant libgdip packages installed.
