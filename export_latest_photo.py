#!/usr/bin/env python3
# /// script
# requires-python = ">=3.10"
# dependencies = [
#     "pyobjc-framework-Photos",
#     "pyobjc-framework-Cocoa",
# ]
# ///
"""
Export the latest photo from macOS Photo Library

IMPORTANT: Due to macOS security requirements, Python scripts cannot automatically
request Photos permission. You must manually grant permission first:

1. Run this script once (it will fail)
2. Open System Settings > Privacy & Security > Photos
3. Add your Terminal app (or Python executable) to allowed apps
4. Run the script again

Alternative: Use the AppleScript version (export_latest_photo_applescript.scpt)
"""

from Foundation import NSSortDescriptor
from Photos import PHPhotoLibrary, PHAsset, PHImageManager, PHFetchOptions, PHImageRequestOptions
import os
import sys

def main():
    # Check if we have access (3 = Authorized)
    status = PHPhotoLibrary.authorizationStatusForAccessLevel_(2)
    
    if status != 3:
        print("❌ No Photos access. Status:", status)
        print("\nTo fix:")
        print("1. Open System Settings > Privacy & Security > Full Disk Access")
        print("2. Enable your Terminal app")
        print("3. Restart Terminal and run this script again")
        sys.exit(1)
    
    # Get latest photo
    options = PHFetchOptions.alloc().init()
    options.setSortDescriptors_([NSSortDescriptor.sortDescriptorWithKey_ascending_("creationDate", False)])
    options.setFetchLimit_(1)
    
    result = PHAsset.fetchAssetsWithMediaType_options_(1, options)
    if result.count() == 0:
        print("❌ No photos found")
        sys.exit(1)
    
    asset = result.objectAtIndex_(0)
    print(f"📸 Found: {asset.creationDate()}")
    
    # Export to ./exported/ folder
    export_dir = os.path.join(os.path.dirname(__file__), "exported")
    os.makedirs(export_dir, exist_ok=True)
    
    request_options = PHImageRequestOptions.alloc().init()
    request_options.setSynchronous_(True)
    request_options.setDeliveryMode_(1)
    
    def save_image(image, info):
        if image is None:
            print("❌ Failed to get image")
            return
        
        from datetime import datetime
        filename = f"{datetime.now().strftime('%Y%m%d_%H%M%S')}.png"
        filepath = os.path.join(export_dir, filename)
        
        with open(filepath, 'wb') as f:
            f.write(image.TIFFRepresentation())
        
        print(f"✅ Saved: {filepath}")
    
    PHImageManager.defaultManager().requestImageForAsset_targetSize_contentMode_options_resultHandler_(
        asset, (10000, 10000), 0, request_options, save_image
    )

if __name__ == "__main__":
    main()

