# PyJump Jump listing

This tool helps crawling Google Drive files, categorizing them, and uploading them to a Google Sheets.
It's designed to work locally on your machine and uses your personal Google account for access.
(Yes, I know it's in c#, I coded the damned thing. It just started as a python program, and then I forgot to rename it when I switched language. Do me a solid and assume it means 'pretty jump listing' or something.)

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

If you want to log on another identity, delete the `token.json` folder/file in the `Resouces` folder of the executable folder. The app will then prompt you to log in again.

Do **not** share your token file publicly. It is your google account identity and possesses all your access rights.

All google drive api-using projects require an identity on Google Cloud Console. I made one for myself, which you can still use, but you can create your own if you want.

If you want to use your own Google Cloud Console project:
1. Go to the [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project
3. Enable the Google Drive API
4. Enable the Google Sheets API
5. Create OAuth 2.0 credentials (as a **Desktop app**)
6. Download the `credentials.json` file
7. Replace the `credentials.json` file in the `Resouces` folder of the executable folder

---

## Configuration

Open the `Resource\appsettings.json` file:

1. This is the way the application will find the Google Sheets document to upload data to. You can find the ID in the URL of your Google Sheets document.
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
More specifically, this is a list of keywords that the application will look for in the folder names. 
All files in the folder will be uploaded to the list `Stories` instead of `Jumps`.
Do **not** use these keywords in the names of your `mainDrives`.
```json
{
	"stories_keywords": [
		"/stories",
		"for stories",
		"writefagging",
		"discussions"
	]
}
```

## How to use
1. Run the application
2. Connect to your Google account (if not already done)
3. Click the button `scan Whitelist` to start scanning the `mainDrives` for their folders recursively. This may take some time.
	- After scanning a Whitelist entry, its property `LastChecked` will be set to the current date utc, midnight.
	- Future scans will only scan folders that have been modified since the last check.
4. You can use the button `Edit Whitelist` to see what's registered in the whitelist. 
   	- You can also add folders manually, but be careful with that. They will be deleted the next time you scan the whitelist.
	- You can also remove folders manually, but be careful with that. They will be added the next time you scan the whitelist.
		- Also, deleting a Whitelist entry will delete all associated files from the database.
   	- Folders have many information. What interests you is `Type`. It can take many values:
		- `j`: Jump. This is the default type. It means that the folder is a jump folder. All files found in this folder will be uploaded to the `Jumps` list.
		- `s`: Story. Thsi is the default type if the folder's `Name` contains the keywords defined in appsettings.json. This means that the folder is a story folder. All files found in this folder will be uploaded to the `Stories` list.
		- `o`: Other. This type is never used in the application. It is just a placeholder for future use if you need it.
		- `-`: Blacklisted. This means that the folder is blacklisted. All files found in this folder will be ignored and not uploaded to any list. In fact, the folder will be ignored during the file crawling.
5. Click the button `scan Files` to start scanning the produced `Whitelist` for their files. This may take some time.
6. You can similarly use the button `Edit Files` to see what's registered in the files list. 
	- You can also add files manually, but be careful with that. They will be deleted the next time you scan the files. And they must manually be linked to a folderId, so it may not be worth it.
	- You can also remove files manually, but be careful with that.
		- If the files you removed were in the past, then the scanning will not find them again.
		- You can use the button `Reset Whitelist Times` to force a complete scan once again.
	- Files have many information. What interests you is `Type`. It can take many values:
		- `j`: Jump. It means that the file is a jump file. It will be uploaded to the `Jumps` list.
		- `s`: Story. It means that the file is a story file. It will be uploaded to the `Stories` list.
		- `o`: Other. It means that the file is a file of another type. It will be uploaded to the `Others` list.
		- `-`: Blacklisted. This means that the file is blacklisted. It will be ignored and not uploaded to any list.
		- By default, a file will use the same type as the whitelist entry it linked to.
7. Click the button `Build Sheets` to start uploading the files to the Google Sheets document. This is actually pretty fast.
	- The application will reupload the entire content of the database every time.
	- Blacklisted files will be ignored.
	- Five sheets will be created in the spreadsheet:
		- `Jumps`: All files with type `j`. However, files with the same `Name` and `Owner` will be regrouped: only the one with the latest `LastModified` date will be shown.
		- `Stories`: All files with type `s`. However, files with the same `Name` and `Owner` will be regrouped: only the one with the latest `LastModified` date will be shown.
		- `Jumps (Unfiltered)`: All files with type `j`. All files will be shown, even if they have the same `Name` and `Owner`.
		- `Stories (Unfiltered)`: All files with type `s`. All files will be shown, even if they have the same `Name` and `Owner`.
		- `Others`: All files with type `o`
		- All sheets are created at the application's launch, and recreated if needed when pretty much any button is pressed.
		- All files are ordered by `LastModified` date, from the most recent to the oldest.
8. You can click the button `Go to sheet` to open the Google Sheets document in your browser immediately.