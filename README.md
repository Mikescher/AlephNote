# AlephNote [![Build status](https://ci.appveyor.com/api/projects/status/6od3qq5kpisd9l5t?svg=true)](https://ci.appveyor.com/project/Mikescher/alephnote-vqio1)

An extensible, lightweight desktop note client for multiple backends

![](https://raw.githubusercontent.com/Mikescher/AlephNote/master/docs/preview.png)  

### ([Download](https://github.com/Mikescher/AlephNote/releases/latest))


### ([Wiki/Manual](https://github.com/Mikescher/AlephNote/wiki/))

AlephNote is a lightweight note taking desktop app usable with multiple backends:

### Standard Note

With the StandardNotePlugin your notes get synced with a [Standard File server](https://standardnotes.org/).
Your notes are locally encrypted and cannot be read from anyone but you

### Simplenote

With the SimpleNotePlugin you can sync your notes with the free [SimpleNote](https://simplenote.com/) online service

### Nextcloud/owncloud notes

The NextcloudPlugin syncs your notes with the [notes app](https://github.com/nextcloud/notes) running on you own private nextcloud/owncloud instance

### Local

You can also simply not use a remote backend and either use the HeadlessPlugin (don't sync the notes anywhere) or the FilesystemPlugin (sync the notes with another folder).


## Installation

Simply download the latest [release](https://github.com/Mikescher/AlephNote/releases/latest) and extract it where you want (all settings etc are portable).
By default the program automatically searches for new versions and downloads them.
If there is demand for an installer I could make one, but personally I like portable programs more.

> **Attention**  
> Sometimes Windows marks the zip-file as untrusted because you downloaded it from the web, and after extraction [all .exe and .dll files will be blocked](https://weblogs.asp.net/dixin/understanding-the-internet-file-blocking-and-unblocking). To prevent this please right-click on the zip file, go to the file properties and click there "Unblock". Then extract the archive.  
If you do the extraction first you have to do the unblocking on every single dll and exe file ([see also here for a better explanation wiht images](https://weblogs.asp.net/dixin/understanding-the-internet-file-blocking-and-unblocking))

## Contribution

Contributions are always welcome, either with additional plugins for other backends or improvements to the core app.

Also anyone with an ounce of design talent: Feel free to improve the [website](https://mikescher.github.io/AlephNote/)...

Also there are probably a ton of spelling errors, any native english-speaker can probably fix a lot of smaller errors.

## System Requirements

Windows Version:
 - dotNet 4.6 or higher
 - Windows 7 or higher

Linux version
 - TBA

## Features

 - [X] Synchronization with [SimpleNote](https://simplenote.com/)
 - [X] Synchronization with [Nextcloud Notes](https://github.com/nextcloud/notes)
 - [X] Synchronization with [Standard Notes](https://standardnotes.org/)
 - [X] Usage without remote provider (= headless plugin)
 - [X] In-editor markdown rendering (similar to [qownnotes](http://www.qownnotes.org/))
 - [X] interactive highlighting of checkbox lists (e.g. TODO lists)
 - [X] clickable + highlighted links
 - [X] (optionally) backup all notes to local git repo (for backup / history)
 - [X] Drop files/text directly into app to create notes
 - [X] Sort notes into folders
 - [X] simulate folders for notes with remote provider that do not support folders (path is encoded in filename)
 - [X] Customizable shortcuts
 - [X] Readonly mode

## License

[MIT](https://github.com/Mikescher/AlephNote/blob/master/LICENSE)

