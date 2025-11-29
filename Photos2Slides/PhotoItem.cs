using Photos;

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

	public Task<UIKit.UIImage?> GetOriginal(CoreGraphics.CGSize targetSize, PHImageContentMode contentMode)
	{
		var tcs = new TaskCompletionSource<UIKit.UIImage?>();

		// Request full-resolution image data
		var requestOptions = new PHImageRequestOptions
		{
			DeliveryMode = PHImageRequestOptionsDeliveryMode.HighQualityFormat,
			Synchronous = false,
			NetworkAccessAllowed = true // Allow downloading from iCloud if needed
		};

		PHImageManager.DefaultManager.RequestImageForAsset(
			OriginalAsset!,
			targetSize,
			contentMode,
			requestOptions,
			async (image, info) =>
			{
				if (image != null)
				{
					try
					{
						tcs.SetResult(image);
					}
					catch (Exception ex)
					{
						tcs.SetException(ex);
					}
				}
				else
				{
					tcs.SetResult(null);
				}
			}
		);

		return tcs.Task;
	}
}

