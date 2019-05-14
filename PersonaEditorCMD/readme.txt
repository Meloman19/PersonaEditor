Supported file type: 
	Container: BIN (BIN, PAK, PAC, P00, ARC); PM1; BF; BVP; TBL
	Graphic: SPR; TMX
	Font: FNT
	Text: BMD (BMD, MSG); PTP

For select OLD/NEW font: open PersonaEditor.xml and input your font name (without extention).

Command: PersonaEditor.exe "FilePath" [-command] [/arguments] [-command] [/arguments]...

[-command]:
	-expimage	- Export image as PNG.
		Support: FNT, TMX
	-impimage	- Import PNG image.
		Support: FNT

	-exptable	- Export width table.
	-imptable	- Import width table.
		Support: FNT

	-expptp		- Export PTP from opened file (if it's BMD) or from all subfiles (see argument /sub).
	-impptp		- Import PTP to opened file (if it's BMD) or to all subfile.
		Support: All files.

	-exptext	- Export text from PTP.
	-imptext	- Import text to PTP.
		Support: PTP

	-expall		- Export all subfiles from opened file.
	-impall		- Import all finding subfiles.
		Support: BIN; SPR; BF; PM1; BVP; TBL.

	-exp[FileType]	- Export all files with FileType and subfiles (see argument /sub); FileType: BIN (BIN, PAK, PAC, P00, ARC); SPR; TMX; BF; PM1; BMD (BMD, MSG); FNT; BVP; HEX - HEX is all unknown type files.
		Support: All files.

	

[/arguments]:
	/sub 		- Recursively do command. Example: "file.bin -expptp /sub"

	/map "pattern"	- When you will be import text from text file you must set pattern. A text file is a set of line. Each line contains a set of values separated by Tab key. 
	
	MAP:
	   %I - ignore
	   %FN - file name
	   %MSGIND - MSG index
	   %MSGNM - MSG name
	   %STRIND - MSG's String index
	   %OLDSTR - Old text
	   %NEWSTR - New text
	   %OLDNM - Old name
	   %NEWNM - New name

	For import new string you must have: %FN & (%MSGIND | %MSGNM) & %STRIND & %NEWSTR
	Or for import new string and new name (line by line) you must have: %NEWSTR or/and %NEWNM 
	For import new name you must have: %OLDNM & %NEWNM
	

	Example:
	   Text:	E100_000.PTP[tab]1[tab]1[tab]Igor[tab]Original text...[tab]Translate...
	   Pattert:	"%FN %MSGIND %STRIND %I %I %NEWSTR"

	/lbl		- When import text to PTP (-imptext), use this command to import line by line.

	/auto "int"	- When import text to PTP (-imptext), use this command to automatic hyphenation by width (in pixel). Optional. Otherwise inserts the string as is (considering LINE FEED "\n").

	/co2n 		- When export PTP (-expptp), use this command to copy the old (source) text to the new one.

	/rmvspl 	- When export text from PTP (-exptext), use this command to replace "\n" to " ".

	/skipempty	- When import text to PTP (-imptext), use this command to skip empty (untranslate) text.

	/enc "ENCODING" - When import text to PTP (-imptext), use this command to set encoding of your text file. Optional. Default UTF-8.
		"ENCODING":	"UTF-7"
				"UTF-16"
				"UTF-32"

	/size 13842	- When import image to FNT (-impimage), use this command to set new font's size. Optional.

Basic example. You have BIN file, that contain two BF.
For export:
"PersonaEditor.exe EXAMPLE.BIN -expptp /sub /co2n" - this create PTP from all BMD files (also sub) in BIN. "/co2n" mean copy old text to new in PTP.
"PersonaEditor.exe EXAMPLE_SUB.PTP -exptext FILE.TXT /map "%FN %MSGIND %STRIND %OLDNM %OLDSTR" /rmvspl" - this export text from PTP to destination TXT. "/rmvspl" mean delete "new line" ("\n") from string.
"PersonaEditor.exe EXAMPLE_SUB.PTP -imptext FILE.TXT /map "%FN %MSGIND %STRIND %I %I %NEWSTR" /auto 580 /skipempty -save" - this import text to PTP. "/skipempty" mean dont import empty string from text file.
"PersonaEditor.exe EXAMPLE.BIN -impptp /sub -save" - this import PTP to all BMD files (also sub) in BIN. "-save" mean save opened file.

If tou have BMD file:
"PersonaEditor.exe EXAMPLE.BMD -expptp /co2n" - this create PTP from all BMD files (also sub) in BIN. "/co2n" mean copy old text to new in PTP.
"PersonaEditor.exe EXAMPLE.PTP -exptext FILE.TXT /map "%FN %MSGIND %STRIND %OLDNM %OLDSTR" /rmvspl" - this export text from PTP to destination TXT. "/rmvspl" mean delete "new line" ("\n") from string.
"PersonaEditor.exe EXAMPLE.PTP -imptext FILE.TXT /map "%FN %MSGIND %STRIND %I %I %NEWSTR" /auto 580 /skipempty -save" - this import text to PTP. "/skipempty" mean dont import empty string from text file.
"PersonaEditor.exe EXAMPLE.BMD -impptp -save" - this import PTP to all BMD files (also sub) in BIN. "-save" mean save opened file.