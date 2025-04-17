# AaruSystem Tools

This toolkit is designed for modifying games developed with the AaruSystem engine.

## Resource Packs

Resource pack files for this engine have the `.FL4` extension, and their file header bytes are `46 4C 34 2E 30`.

### Extracting Files from a Resource Pack

Run the following command:
```
FL4Tool -e -in input.FL4 -out Extract -cp shift_jis
```

Parameter Description:
- `-e`: Extract files from the resource pack.
- `-in`: Specify the resource pack filename.
- `-out`: Specify the directory where extracted items will be stored.
- `-cp`: Specify the encoding of filenames within the resource pack. This is usually `shift_jis`.

### Creating a Resource Pack

Run the following command:
```
FL4Tool -c -in RootDirectory -out res.FL4 -cp shift_jis
```

Parameter Description:
- `-c`: Create a resource pack.
- `-in`: Specify the folder containing files you wish to add to the resource pack.
- `-out`: Specify the resource pack filename.
- `-cp`: Specify the encoding of filenames within the resource pack. This is usually `shift_jis`.

## Scripts

Script files for this engine have the `.AB` extension and do not have an identifier in the file header.

### Disassembling Scripts

Run the following command:
```
ScriptTool -d -in input.AB -icp shift_jis -out output.txt
```

Parameter Description:
- `-d`: Disassemble analysis.
- `-in`: Specify the script filename.
- `-icp`: Specify the encoding of text within the script file. This is usually `shift_jis`.
- `-out`: Specify the output filename.

### Extracting Text from Scripts

Run the following command:
```
ScriptTool -e -in input.AB -icp shift_jis -out output.txt
```

Parameter Description:
- `-e`: Extract text.
- `-in`: Specify the script filename.
- `-icp`: Specify the encoding of text within the script file. This is usually `shift_jis`.
- `-out`: Specify the output filename.

### Importing Text into Scripts

Run the following command:
```
ScriptTool -i -in input.AB -icp shift_jis -out output.AB -ocp shift_jis -txt input.TXT
```

Parameter Description:
- `-i`: Import text.
- `-in`: Specify the script filename.
- `-icp`: Specify the encoding of text within the script file. This is usually `shift_jis`.
- `-out`: Specify the output filename.
- `-ocp`: Specify the encoding of text within the output script file. This is usually `shift_jis`.
- `-txt`: Specify the filename of the file containing text you wish to import.

## Images

This engine uses standard BMP format images as well as proprietary BM2A format images, which typically have the `.BM2` extension. They can be distinguished by the header bytes of the file.

---

**Note:** This toolkit has been tested on a limited number of games only.
