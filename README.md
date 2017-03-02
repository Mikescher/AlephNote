# AlephNote

An extensible, lightweight desktop note client for multiple backends

![](https://raw.githubusercontent.com/Mikescher/AlephNote/master/docs/preview.png)

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


## Contribution

Contributions are always welcome, either with additional plugins for other backends or improvements to the core app.

Also anyone with an ounce of design talent: Feel free to improve the [website](https://mikescher.github.io/AlephNote/)...

## Planned/Possible features

 - [ ] Client side encryption (for all plugins)
 - [ ] Get history from provider that support it (SimpleNote + Standard Notes)
 - [ ] In-editor markdown rendering (similar to [qownnotes](http://www.qownnotes.org/))
 - [ ] Ctrl+F search function
 - [ ] Installer (if there is demand)


## License

[MIT](https://github.com/Mikescher/AlephNote/blob/master/LICENSE)