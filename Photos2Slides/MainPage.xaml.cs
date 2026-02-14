using Photos;
using System.Collections.ObjectModel;
using Foundation;

namespace Photos2Slides;

public partial class MainPage : ContentPage
{
	public ObservableCollection<PhotoItem> Photos { get; set; } = [];

	public MainPage()
	{
		InitializeComponent();
		Loaded += MainPage_Loaded;
		PhotosCollectionView.ItemsSource = Photos;
	}

	private void MainPage_Loaded(object? sender, EventArgs e)
	{
		var status = PHPhotoLibrary.GetAuthorizationStatus(PHAccessLevel.ReadWrite);
		MainThread.BeginInvokeOnMainThread(async () =>
		{
			await DisplayAlertAsync(status.ToString(), "We need Photo library access.", "OK");
		});
	}

	private void OnLoadPhotosClicked(object sender, EventArgs e)
	{
		var status = PHPhotoLibrary.GetAuthorizationStatus(PHAccessLevel.ReadWrite);
		PHPhotoLibrary.RequestAuthorization(PHAccessLevel.ReadWrite, s =>
		{
			if (s == PHAuthorizationStatus.Authorized)
			{
				MainThread.BeginInvokeOnMainThread(() => Photos.Clear());
				Task.Run(() => LoadRecentPhotos());
			}
			else
			{
				MainThread.BeginInvokeOnMainThread(async () =>
				{
					await DisplayAlertAsync("Permission Denied", "Photo library access is required to load photos.", "OK");
				});
			}
		});
	}

	private void LoadRecentPhotos()
	{
		var sevenDaysAgo = DateTime.Now.AddDays(-7);

		var fetchOptions = new PHFetchOptions
		{
			SortDescriptors = [new NSSortDescriptor("creationDate", false)],
			Predicate = NSPredicate.FromFormat($"creationDate >= %@", NSDate.FromTimeIntervalSinceReferenceDate((sevenDaysAgo - new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds)),
			//FetchLimit = 10,//only for local testing
		};

		var fetchResult = PHAsset.FetchAssets(PHAssetMediaType.Image, fetchOptions);

		var count = (nuint)fetchResult.Count;

		for (nuint i = 0; i < count; i++)
		{
			var asset = fetchResult[(nint)i] as PHAsset;
			if (asset != null)
			{
				var requestOptions = new PHImageRequestOptions
				{
					DeliveryMode = PHImageRequestOptionsDeliveryMode.HighQualityFormat,
					Synchronous = false
				};

				PHImageManager.DefaultManager.RequestImageForAsset(
					asset,
					new CoreGraphics.CGSize(400, 400),
					PHImageContentMode.AspectFit,
					requestOptions,
					(image, info) =>
					{
						if (image != null)
						{
							MainThread.BeginInvokeOnMainThread(() =>
							{
								Photos.Add(new PhotoItem
								{
									ImageSource = ImageSource.FromStream(() =>
									{
										var data = image.AsPNG();
										return data?.AsStream() ?? Stream.Null;
									}),
									OriginalAsset = asset // Store reference to original asset
								});
							});
						}
					}
				);
			}
		}
	}
	private void OnShowDuplicateClicked(object sender, EventArgs e)
	{
		if (sender is Button button && button.BindingContext is PhotoItem photoItem)
		{
			photoItem.ShowDuplicate = true;
		}
	}

	private async void OnExportImageClicked(object sender, EventArgs e)
	{
		if (sender is Button button && button.BindingContext is PhotoItem photoItem)
		{
			var image = await photoItem.GetOriginal(PHImageManager.MaximumSize, PHImageContentMode.Default);
			var data = image?.AsPNG();
			if (data != null)
			{
				var filePath = GetExportFilePath(".png");
				using (var fileStream = File.Create(filePath))
				{
					var stream = data.AsStream();
					await stream.CopyToAsync(fileStream);
				}

				await MainThread.InvokeOnMainThreadAsync(async () =>
				{
					await DisplayAlertAsync("Image Saved", $"Full-resolution image saved to: {filePath}", "OK");
				});

			}
		}
	}

	private async void OnExportSlidesClicked(object sender, EventArgs e)
	{
		try
		{
			var filePath = GetExportFilePath(".pptx");
			await SlidesCreator.CreateSlidesAsync(Photos, filePath);
			await MainThread.InvokeOnMainThreadAsync(async () =>
			{
				await DisplayAlertAsync("Slides Saved", $"Slides saved to: {filePath}", "OK");
			});
		}
		catch (Exception ex)
		{
			await MainThread.InvokeOnMainThreadAsync(async () =>
			{
				await DisplayAlertAsync("Error", $"Failed to create slides: {ex.Message}", "OK");
			});
		}
	}

	private static string GetExportFilePath(string postfix)
	{
		var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
		var fileName = $"{timestamp}{postfix}";
		var documentsPath = "/Users/Manuel.Naujoks/Projects/Photos2Slides";
		var outputDirectory = Path.Combine(documentsPath, "exported");
		Directory.CreateDirectory(outputDirectory); // Creates directory if it doesn't exist
		var filePath = Path.Combine(outputDirectory, fileName);
		return filePath;
	}
}

