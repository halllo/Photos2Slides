#!/usr/bin/osascript
# Export the latest photo from Photos app to ./exported/ folder

tell application "Photos"
	set exportPath to (do shell script "pwd") & "/exported/"
	do shell script "mkdir -p " & quoted form of exportPath
	
	set allPhotos to every media item
	if (count of allPhotos) is 0 then error "No photos found"
	
	# Find the most recent photo
	set latestPhoto to item 1 of allPhotos
	set latestDate to date of latestPhoto
	
	repeat with aPhoto in rest of allPhotos
		if (date of aPhoto) > latestDate then
			set latestPhoto to aPhoto
			set latestDate to date of aPhoto
		end if
	end repeat
	
	export {latestPhoto} to (POSIX file exportPath)
	return "✅ Exported photo from " & latestDate
end tell
