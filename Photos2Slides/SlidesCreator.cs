using Photos;

namespace Photos2Slides;

public static class SlidesCreator
{
    public static async Task CreateSlidesAsync(IEnumerable<PhotoItem> photos, string outputPath)
    {
        //await CreateSlidesWithAsposeSlides(photos, outputPath);
        //await CreateSlidesWithIronPPT(photos, outputPath);
        await CreateSlideImagesAsync(photos.Reverse(), outputPath);
    }

    private static async Task<MemoryStream> GetImageStream(PhotoItem photo)
    {
        using UIKit.UIImage? uiImage = await photo.GetOriginal(
            new CoreGraphics.CGSize(800, 800),
            PHImageContentMode.AspectFit);
        var imageStream = new MemoryStream();
        using var fileStream = uiImage?.AsJPEG()?.AsStream();
        fileStream?.CopyTo(imageStream);
        imageStream.Position = 0;
        return imageStream;
    }

    private static async Task CreateSlideImagesAsync(IEnumerable<PhotoItem> photos, string outputPath)
    {
        int index = 0;
        int total = photos.Count();
        int padLength = total.ToString().Length;
        foreach (var photo in photos)
        {
            using var imageStream = await GetImageStream(photo);
            string paddedIndex = index.ToString($"D{padLength}");
            using (var fileStream = File.Create($"{outputPath}_{paddedIndex}.jpeg"))
            {
                await imageStream!.CopyToAsync(fileStream);
            }
            index++;
        }
    }

    private static async Task CreateSlidesWithAsposeSlides(IEnumerable<PhotoItem> photos, string outputPath)
    {
        using var licenseStream = await FileSystem.OpenAppPackageFileAsync("Aspose.Total.lic");
        new Aspose.Slides.License().SetLicense(licenseStream);

        using var presentation = new Aspose.Slides.Presentation();

        foreach (var photo in photos)
        {
            var slide = presentation.Slides.AddEmptySlide(presentation.LayoutSlides[0]);

            using var imageStream = await GetImageStream(photo);
            var aspImage = presentation.Images.AddImage(imageStream);
            var picFrame = slide.Shapes.AddPictureFrame(
                Aspose.Slides.ShapeType.Rectangle,
                0, 0,
                presentation.SlideSize.Size.Width,
                presentation.SlideSize.Size.Height,
                aspImage);

            picFrame.PictureFormat.PictureFillMode = Aspose.Slides.PictureFillMode.Stretch;
        }

        /* This fails on macos:
        *
        * Unhandled exception. System.TypeInitializationException: The type initializer for 'Gdip' threw an exception.
        * ---> System.PlatformNotSupportedException: System.Drawing.Common is not supported on non-Windows platforms. See https://aka.ms/systemdrawingnonwindows for more information.
        */
        presentation.Save(outputPath, Aspose.Slides.Export.SaveFormat.Pptx);
    }

    private static async Task CreateSlidesWithIronPPT(IEnumerable<PhotoItem> photos, string outputPath)
    {
        using var licenseStream = await FileSystem.OpenAppPackageFileAsync("IronPPT.lic");
        using var reader = new StreamReader(licenseStream);
        IronPPT.License.LicenseKey = await reader.ReadToEndAsync();

        var document = new IronPPT.PresentationDocument();

        var slide1 = new IronPPT.Models.Slide();
        slide1.AddText("Hello!");
        document.AddSlide(slide1);

        List<Stream> imageStreams = [];
        foreach (var photo in photos)
        {
            var imageStream = await GetImageStream(photo);
            imageStreams.Add(imageStream);

            var slide = new IronPPT.Models.Slide();
            var image = new IronPPT.Models.Image();
            image.LoadFromStream(imageStream);
            image.Resize(700, 525);
            slide.AddImage(image);
            document.AddSlide(slide);
        }

        document.Save(outputPath);
        foreach (var stream in imageStreams)
        {
            stream.Dispose();
        }
    }
}

