using Photos;
using System.Collections.ObjectModel;
using Foundation;

namespace Photos2Slides;

public class PhotoItem : BindableObject
{
	private ImageSource? _imageSource;
	
	public ImageSource? ImageSource
	{
		get => _imageSource;
		set
		{
			_imageSource = value;
			OnPropertyChanged();
		}
	}
	
	private bool _showDuplicate;
	public bool ShowDuplicate
	{
		get => _showDuplicate;
		set
		{
			_showDuplicate = value;
			OnPropertyChanged();
		}
	}
	
	// Store reference to original PHAsset for full-resolution access
	public PHAsset? OriginalAsset { get; set; }
}

public partial class MainPage : ContentPage
{
	public ObservableCollection<PhotoItem> Photos { get; set; } = new ObservableCollection<PhotoItem>();

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
			Predicate = NSPredicate.FromFormat($"creationDate >= %@", NSDate.FromTimeIntervalSinceReferenceDate((sevenDaysAgo - new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds))
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
					new CoreGraphics.CGSize(600, 600),
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
			// Save the full-resolution image to filesystem
			await SaveFullResolutionImageAsync(photoItem.OriginalAsset);
		}
	}

	private async Task SaveFullResolutionImageAsync(PHAsset? asset)
	{
		if (asset == null)
		{
			await DisplayAlertAsync("Error", "No asset reference available", "OK");
			return;
		}

		var tcs = new TaskCompletionSource<bool>();

		try
		{
			// Request full-resolution image data
			var requestOptions = new PHImageRequestOptions
			{
				DeliveryMode = PHImageRequestOptionsDeliveryMode.HighQualityFormat,
				Synchronous = false,
				NetworkAccessAllowed = true // Allow downloading from iCloud if needed
			};

			PHImageManager.DefaultManager.RequestImageForAsset(
				asset,
				PHImageManager.MaximumSize, // Request maximum resolution
				PHImageContentMode.Default,
				requestOptions,
				async (image, info) =>
				{
					if (image != null)
					{
						try
						{
							// Convert to PNG data
							var data = image.AsPNG();
							
							if (data != null)
							{
								// Generate a unique filename with timestamp
								var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
								var fileName = $"transformed_{timestamp}.png";
								// Create subdirectory in Documents folder
								var documentsPath = "/Users/manuelnaujoks/Projects/Photos2Slides";
								var outputDirectory = Path.Combine(documentsPath, "exported");
								Directory.CreateDirectory(outputDirectory); // Creates directory if it doesn't exist
								var filePath = Path.Combine(outputDirectory, fileName);
								
								// Save to file
								using (var fileStream = File.Create(filePath))
								{
									var stream = data.AsStream();
									await stream.CopyToAsync(fileStream);
								}
								
								await MainThread.InvokeOnMainThreadAsync(async () =>
								{
									await DisplayAlertAsync("Image Saved", $"Full-resolution image saved to: {filePath}", "OK");
								});
								
								tcs.SetResult(true);
							}
							else
							{
								tcs.SetResult(false);
							}
						}
						catch (Exception ex)
						{
							await MainThread.InvokeOnMainThreadAsync(async () =>
							{
								await DisplayAlertAsync("Error", $"Failed to save image: {ex.Message}", "OK");
							});
							tcs.SetException(ex);
						}
					}
					else
					{
						await MainThread.InvokeOnMainThreadAsync(async () =>
						{
							await DisplayAlertAsync("Error", "Failed to retrieve full-resolution image", "OK");
						});
						tcs.SetResult(false);
					}
				}
			);

			await tcs.Task;
		}
		catch (Exception ex)
		{
			await DisplayAlertAsync("Error", $"Failed to save image: {ex.Message}", "OK");
		}
	}
}

