using System.Text.Json;
using IronPPT;
using IronPPT.Models;

License.LicenseKey = File.ReadAllText("IronPPT.lic");

var document = new PresentationDocument();

Slide slide1 = new Slide();
slide1.AddText("Hello, World from IronPPT!");
document.AddSlide(slide1);


var folderWithImages = "/Users/manuelnaujoks/Projects/Photos2Slides/exported";
var imageFiles = Directory
    .GetFiles(folderWithImages, "*.jpeg")
    .Select(f => new FileInfo(f))
    .OrderBy(f => f.CreationTime)
    .ToArray();
    
foreach (var imagePath in imageFiles)
{
    Console.WriteLine("Adding image: " + imagePath);
	var slide = new Slide();
	var image = new Image();
	image.LoadFromFile(imagePath.FullName);
	image.Resize(700, 525);
	slide.AddImage(image);
	document.AddSlide(slide);
}

document.Save($"slides_for_{imageFiles.Length}_images.pptx");
