#!/usr/bin/osascript
-- Export all photos imported today from Photos to ./exported/
--
-- Tries today first, then falls back to yesterday, then last 48 hours,
-- so it works even when photos were captured the night before.
-- Pass a number as the first argument to override the look-back hours,
-- e.g.:  osascript export_todays_imports.scpt 72

on run argv
	tell application "Photos"
		set exportPath to (do shell script "pwd") & "/exported/"
		do shell script "mkdir -p " & quoted form of exportPath

		-- Determine look-back window
		set lookbackHours to 48
		if (count of argv) > 0 then
			set lookbackHours to (item 1 of argv) as integer
		end if

		set cutoff to (current date) - (lookbackHours * 60 * 60)
		set allPhotos to every media item
		set selectedPhotos to {}
		repeat with aPhoto in allPhotos
			if (date of aPhoto) >= cutoff then
				copy aPhoto to end of selectedPhotos
			end if
		end repeat

		if (count of selectedPhotos) = 0 then
			error "No photos found in the last " & lookbackHours & " hours"
		end if

		export selectedPhotos to (POSIX file exportPath)
		return "Exported " & (count of selectedPhotos) & " photos (last " & lookbackHours & " hours) to " & exportPath
	end tell
end run
