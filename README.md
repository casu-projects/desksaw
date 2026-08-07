THIS IS THE SOURCE CODE. NOT THE ACTUAL BUILD. GO TO RELEASES. YOU ARE ON THE WRONG PAGE UNLESS YOU WANT THE SOURCE CODE

hey this literally isnt done

im making this public  with the intent on having other people help me with it

the project is a mess

https://godotengine.org/ USE THE .NET VERSION OR IT WILL PROBABLY BREAK IDK

you might need to import this addon 

https://github.com/4d49/godot-console

---

### macOS: app fails to open with a "(-47)" error

If the app won't open on macOS (e.g. "The application 'desksaw' can't be opened. (-47)"), the download is blocked by Gatekeeper because of the quarantine attribute. Remove it and try again:

```
xattr -dr com.apple.quarantine /path/to/desksaw.app
```

The path depends on where you saved the app. In Terminal, type `xattr -dr com.apple.quarantine ` (with a trailing space), drag the `desksaw.app` file into the window to insert its full path, then press Enter.
