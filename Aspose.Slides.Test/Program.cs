// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");


var asposeLicenseFilename = Path.Combine(AppContext.BaseDirectory, "Aspose.Total.lic");
new Aspose.Slides.License().SetLicense(asposeLicenseFilename);

using var presentation = new Aspose.Slides.Presentation();

var slide = presentation.Slides.AddEmptySlide(presentation.LayoutSlides[0]);

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
var textBox = slide.Shapes.AddAutoShape(
    Aspose.Slides.ShapeType.Rectangle,
    100, 100,
    500, 100);
var textFrame = textBox.TextFrame;
textFrame.Text = "Hello, Aspose.Slides!";

presentation.Save($"my_{DateTime.Now:yyyyMMddHHmmss}.pptx", Aspose.Slides.Export.SaveFormat.Pptx);