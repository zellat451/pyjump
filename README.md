﻿# PyJump Jump listing

This tool helps crawling Google Drive files, categorizing them, and uploading them to a Google Sheets.

It's designed to work locally on your machine and uses your personal Google account for access.

(Yes, I know it's in c#, I coded the damned thing. It just started as a python program, and then I forgot to rename it when I switched language. 
Do me a solid and pretend it means 'pretty jump listing' or something.)


[Click here to download the zip](https://github.com/zellat451/pyjump/releases/latest)

---

## Requirements

- Runs on .NET 8 or later
- A Google account
- A Google Sheets document to receive data (that you are owner of)
- Probably Windows, because this is a Windows Forms app
- (optionally) `credentials.json` file from Google Cloud Console

---

## Authentication

On first use, you'll be prompted to log into your Google Account via a browser window. The app stores a token locally to skip login next time.

If you want to log on another identity, delete the `token.json` folder/file in the executable folder. The app will then prompt you to log in again.

Do **not** share your token file publicly. It is your google account identity and possesses all your access rights.

All google drive api-using projects require an identity on Google Cloud Console. 
I made one for myself, which you can still use, but you can create your own if you want.

If you want to use your own Google Cloud Console project:
1. Go to the [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project
3. Enable the Google Drive API
4. Enable the Google Sheets API
5. Create OAuth 2.0 credentials (as a **Desktop app**)
6. Download the `credentials.json` file
7. Replace the `credentials.json` file in the `Resouces` folder of the executable folder

---

## First Time Configuration

Open the `Resources\appsettings.json` file:

1. This is the way the application will find the Google Sheets document to upload data to. **Replace it with yours**. 
You can find the ID in the URL of your Google Sheets document.
Or, you can just give it the full URL of the document, it can read that too.
```json
{
  "spreadsheet_id": "https://docs.google.com/spreadsheets/d/your-spreadsheet-id-here/edit#gid=0"
}
```

2. This is the way the application will find the Google Drive folders to crawl. Give it the URLs and unique names for each. 
Don't forget: if you can't access the folder with your google account, neither will the app.
```json
{
  "mainDrives": [
	{
	  "url": "https://drive.google.com/drive/folders/your-folder-id-here",
	  "name": "Folder Name"
	},
	{
	  "url": "https://drive.google.com/drive/folders/another-folder-id-here",
	  "name": "Another Folder Name"
	}
  ]
}
```

3. This is the way the application will decide on which list the files will be uploaded to. 
More specifically, this is a list of (case-insensitive) keywords that the application will look for in the folder and file names.
If the scanning finds these keywords in the name of either files or folders, it will set the type of the folder/file to the one specified next to that keyword.
By default, Folders are `j` (Jump) and files take the same type as their folder.
```json
{
	 "keywords": {
        "/stories": "s",
        "\\stories": "s",
        "for stories": "s",
        "writefagging": "s",
        "discussions": "s"
	}
}
```

---

## How to use
1. Run the application (`pyjump.exe`. Create a shortcut for later if you need to). The first time launch may take a moment as it initializes the spreadsheet.
2. Connect to your Google account (if not already done)
3. Click the button `scan Whitelist` to start scanning the `mainDrives` for their folders recursively. This may take some time.
4. You can use the button `Edit Whitelist` to see what's registered in the whitelist. 
   	- You can also add folders manually.
	- You can also remove folders manually, but be careful with that. They will be added the next time you scan the whitelist.
   	- Folders have many informations. What interests you is `Type`. It can take many values:
		- `j`: Jump. This is the default type. It means that the folder is a jump folder. All files found in this folder will be uploaded to the `Jumps` list.
		- `s`: Story. This means that the folder is a story folder. All files found in this folder will be uploaded to the `Stories` list.
		- `o`: Other. This type is never used in the application. It is just a placeholder for future use if you need it.
		- `-`: Blacklisted. This means that the folder is blacklisted. All files found in this folder will be ignored and not uploaded to any list. In fact, the folder will be ignored during the file crawling.
5. Click the button `scan Files` to start scanning the produced `Whitelist` for their files. This may take some time.
	- After scanning a Whitelist entry, its property `LastChecked` will be set to the current date utc, midnight.
	- Future scans will only get files that have been modified since the last check, to go faster.
6. You can similarly use the button `Edit Files` to see what's registered in the files list. 
	- You can also add files manually.
	- You can also remove files manually, but be careful with that.
		- If the files you removed were in the past, then the scanning will not find them again.
		- You can use the button `Reset Whitelist Times` to force a complete scan once again.
	- Files have many informations. What interests you is that every file is linked to a whitelist entry and shares its `Type` by default. It can take many values:
		- `j`: Jump. It means that the file is a jump file. It will be uploaded to the `Jumps` list.
		- `s`: Story. It means that the file is a story file. It will be uploaded to the `Stories` list.
		- `o`: Other. It means that the file is a file of another type. It will be uploaded to the `Others` list.
		- `-`: Blacklisted. This means that the file is blacklisted. It will be ignored and not uploaded to any list.
		- By default, a file will use the same type as the whitelist entry it linked to. If the file name contains one of the `keywords` from the appsetting, it will be given the corresponding type. You can also edit it manually. It will not be reset by future scans, even if the file infos are updated.
7. Click the button `Build Sheets` to start uploading the files to the Google Sheets document. This is actually pretty fast.
	- The application will reupload the entire content of the database every time.
	- Blacklisted files will be ignored.
	- Five sheets will be created in the spreadsheet:
		- `Jumps`: All files with type `j`. However, files with the same `Name` and `Owner` will be regrouped: only the one with the latest `LastModified` date will be shown.
		- `Stories`: All files with type `s`. However, files with the same `Name` and `Owner` will be regrouped: only the one with the latest `LastModified` date will be shown.
		- `Jumps (Unfiltered)`: All files with type `j`. All files will be shown, even if they have the same `Name` and `Owner`.
		- `Stories (Unfiltered)`: All files with type `s`. All files will be shown, even if they have the same `Name` and `Owner`.
		- `Others`: All files with type `o`
		- `Whitelist`: All whitelist entries stored in the DB.
		- All sheets are created at the application's launch, and recreated if needed when pretty much any button is pressed.
		- All files are ordered by `LastModified` date, from the most recent to the oldest.
8. You can click the button `Go to sheet` to open the Google Sheets document in your browser immediately.

...Or, you can just click the big button and let `Fren` take care of everything. Yes, they are a fren and they work good. The big `:)` button:
1. Scans to refresh the whitelist
1. Scans to get the new files
1. Uploads the data to the spreadsheet
1. And it opens the spreadsheet for you at the end

That's all that Fren does by default, though you can change also tell it to delete broken entries, or to do less things by using the checkbox next to the button.

All checkboxes have a default value. You can change them if you want, and the app will remember them for the next time you launch it.

---

# Explanation of each button
1. `Go to sheet`: Opens the Google Sheets document in your browser.
1. `Edit Whitelist`: Opens the whitelist editor. You can add, remove, and edit whitelist entries in DB.
1. `Edit Files`: Opens the files editor. You can add, remove, and edit files in DB.
1. `Edit Identities`: Opens the identities editor. You can add, remove, and edit Owner identities in DB.
	- Owner identities are used to group files by their owner in the filtered sheets. If you find recurrent owners posting the same files with different accounts, you can create a link between their different identities. The filter will take these different identities when searching for duplicate files.
1. `Batch Ignore`: Allows you to set a large number of files as `FilterIgnored` simultaneously.
    - This takes a `.txt` file. Each line must contain either a hyperlink to a file (the excel formula present in the sheet), or a Google Drive file id. This will read each line, parse the fileId, find it in the database if it exists, and then set all the corresponding files as FilterIgnored = true. These files will not be considered during the Jumps/Stories sheet data filtering. This information can be changed manually in the `Edit Files` table.
1. `Force match Type`: Updates all files in DB to match the type of their linked whitelist entry. This is useful if you edited the whitelist entries and want to update the files to match.
1. `Scan Whitelist`: Scans the `mainDrives` for their folders recursively. This may take some time.
1. `Scan Files`: Scans the whitelist for their files recursively. This may take some time. Only scans new files from the last time the whitelist entry was checked, to go quicker.
1. `Reset Whitelist Times`: Resets the `LastChecked` date of all whitelist entries to null. This will force a complete scan of all files in the whitelist.
1. `Match Whitelist to Drives`: Removes from the DB all whitelist entries with a name that doesn't match any of the `mainDrives` entries. This is useful if you removed a folder from the `mainDrives` and want to remove all its entries from the DB.
1. `Delete broken entries`: Removes all broken entries from the DB, such as trashed files or folders, or those you don't have the permissions for anymore. This may take some time. This does not delete the folders or files which return an error `404 Not Found`, because it is possible that the API simply doesn't have the permissions to check them despite them being there.
	- You can select to delete either the files, or the folders, or both using the checkboxes next to the button.
1. `Build sheets`: Uploads the files to the Google Sheets document.
1. `Clear all data`: Clears all the data in the DB. Does not affect the data uploaded to the Google Sheets document.
	- You can select to clear either the files, or the folders, or both using the checkboxes next to the button.
1. `:)`: The Fren button. It does everything for you. You can set what you want Fren to do using the checkboxes next to the button.
	- `Scan Whitelist`: Scans the `mainDrives` for their folders recursively. This may take some time. (Checked by default)
	- `Scan Files`: Scans the whitelist for their files recursively. This may take some time. Only scans new files from the last time the whitelist entry was checked, to go quicker. (Checked by default)
	- `Broken Whitelist`: Scans the whitelist for broken links, and removes them from the DB. This may take some time.
	- `Broken Files`: Scans the files for broken links, and removes them from the DB. This may take some time.
	- `Build Sheets`: Uploads the files to the Google Sheets document. (Checked by default)
	- `Go to sheet`: Opens the Google Sheets document in your browser. (Checked by default)
1. `Enable/Disable Logging`: Enables or disables copying logs in a local file. If active, all logs will be copied in a file named after today's date in the `logs` folder. The folder/file will be created if it doesn't exist, and appended to if it does. `false` by default, so we don't spam your disk with logs.
1. `Enable/Disable Threading`: Enables or disables multithreading. If active, the application will use multiple threads to scan the files and folders. This is much faster, but it may cause issues with the Google API request limits. `true` by default with `5` threads, which should be within limit. 
You can dynamically change the number of threads with the text box above the Enabling button, or before launch in the other_preferences.json file if it has been generated. The application will use the number of threads specified in the file. It will use the default value of `5` threads. If you set it to a negative number, it will use `1` thread.
1. `Import/Export data`: Imports or exports the data in the DB to a json file. This is useful if you want to backup your data, especially between version updates. 
It also exports the preferences from the multiple `xxx_preferences.json` files, which contain among other things: the max number of threads, the permissions for threading and file logging, and the checkbox states at the time of export.
You can decide on the export folder, but the name is automatically generated from the current date. Imported data will overwrite the current data in the DB. The import file must be a json file, and it must be in the same format as the export file.

---

# End notes

<s>Unfortunately, we can't use multithreading due to api constraints. Google puts a limit on how many requests you can make on a minute, and I haven't found a way to make my tasks wait it out and try again.
I tried, and it's sooooo much faster... but then it locks up after a few hundred files and never starts again. So we're taking it one by one for now. Still faster and easier to use than the google app script, at least for me.</s>

**As of 2025-04-30, this app finally supports multithreading. It is much faster, but it is still limited by the Google APIs.**