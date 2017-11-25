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

Also there are probably a ton of spelling errors, any native english-speaker can probably fix a lot of smaller errors.

## System Requirements

Windows Version:
 - dotNet 4.6 or higher
 - Windows 7 or higher

Linux version
 - TBA

## Planned/Possible/Implemented features

 - [X] Synchronization with [SimpleNote](https://simplenote.com/)
 - [X] Synchronization with [Nextcloud Notes](https://github.com/nextcloud/notes)
 - [X] Synchronization with [Standard Notes](https://standardnotes.org/)
 - [ ] Synchronization with [Evernote](https://evernote.com)
 - [X] Usage without remote provider (= headless plugin)
 - [ ] Client side encryption (for all plugins)
 - [ ] Get history from provider that support it (SimpleNote + Standard Notes)
 - [X] In-editor markdown rendering (similar to [qownnotes](http://www.qownnotes.org/))
 - [X] clickable + highlighted links (http/https/ftp/mailto)
 - [X] (optionally) backup all notes to local git repo (for backup/history)
 - [X] Highlight todo lists (markdown/github syntax)
 - [X] Ctrl+F search function (content of single note)
 - [X] Global search function (normal search | regex search | tag search)
 - [X] Tag autocompletion
 - [ ] Installer (if there is demand)
 - [ ] Linux version (UI mit Eto.Forms, .Net core)
 - [X] auto updater (get version from github API)
 - [X] multiple conflict solving strategies (use local | use server | create conflict note)
 - [ ] Remember last selected note
 - [ ] readonly mode
 - [ ] sort tags alphabetically
 - [ ] edit tags without deleting (F2 ?)
 - [ ] highlight search results when using global search
 - [ ] Better first launch wizard (description, images, directly set provider), see [#6](https://github.com/Mikescher/AlephNote/issues/6)
 - [ ] Sync with multiple provider (eg sync with SimpleNote but also with local plaintext version of notes), see [#7](https://github.com/Mikescher/AlephNote/issues/7)


## License

[MIT](https://github.com/Mikescher/AlephNote/blob/master/LICENSE)

