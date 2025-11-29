using Photos;
using System.Reflection;

namespace Photos2Slides;

public static class SlidesCreator
{
    public static async Task CreateSlidesAsync(IEnumerable<PhotoItem> photos, string outputPath)
    {
        await CreateSlideImagesAsync(photos, outputPath);

        // using var licenseStream = await FileSystem.OpenAppPackageFileAsync("Aspose.Total.lic");
        // new Aspose.Slides.License().SetLicense(licenseStream);

        // using var presentation = new Aspose.Slides.Presentation();

        // foreach (var photo in photos)
        // {
        //     var slide = presentation.Slides.AddEmptySlide(presentation.LayoutSlides[0]);

            // using UIKit.UIImage? uiImage = await photo.GetOriginal();
            // using var imageStream = new MemoryStream();
            // using var fileStream = uiImage?.AsJPEG()?.AsStream();
            // fileStream?.CopyTo(imageStream);

            // imageStream.Position = 0;
            // var aspImage = presentation.Images.AddImage(imageStream);
            // var picFrame = slide.Shapes.AddPictureFrame(
            //     Aspose.Slides.ShapeType.Rectangle,
            //     0, 0,
            //     presentation.SlideSize.Size.Width,
            //     presentation.SlideSize.Size.Height,
            //     aspImage);

            // picFrame.PictureFormat.PictureFillMode = Aspose.Slides.PictureFillMode.Stretch;

            //write "hello" into the slide
        //     var textBox = slide.Shapes.AddAutoShape(
        //         Aspose.Slides.ShapeType.Rectangle,
        //         100, 100,
        //         500, 100);
        //     var textFrame = textBox.TextFrame;
        //     textFrame.Text = "Hello, Aspose.Slides!";

        // }

        /* This fails on macos:
        *
        * Unhandled exception. System.TypeInitializationException: The type initializer for 'Gdip' threw an exception.
        * ---> System.PlatformNotSupportedException: System.Drawing.Common is not supported on non-Windows platforms. See https://aka.ms/systemdrawingnonwindows for more information.
        */
        //presentation.Save(outputPath, Aspose.Slides.Export.SaveFormat.Pptx);
    }

    private static async Task CreateSlideImagesAsync(IEnumerable<PhotoItem> photos, string outputPath)
    {
        int index = 0;
        foreach (var photo in photos)
        {
            using UIKit.UIImage? uiImage = await photo.GetOriginal(
                new CoreGraphics.CGSize(1280, 1280),
				PHImageContentMode.AspectFit);
            using var imageStream = uiImage?.AsJPEG()?.AsStream();
            using (var fileStream = File.Create($"{outputPath}_{index++}.jpeg"))
            {
                await imageStream!.CopyToAsync(fileStream);
            }
        }
    }
}

