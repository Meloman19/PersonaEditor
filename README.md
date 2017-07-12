# Persona 3/4 Font Decompressor/Compressor

[PersonaFont.exe](https://github.com/Meloman19/PersonaFont/releases)

"PersonaFont.exe [param1] [param2:opt]"

param1:
"decom" - Decompress;
"com" - Compress.

param2:
"-l" - Logging to file "PersonaFont.log".

## How it's work
1. Place "FONT0.FNT" (or renamed "FONT1.FNT") file near "PersonaFont.exe"
2. Run "PersonaFont.exe", enter "decom" and you got:
* "FONT0.BMP" - indexed bitmap (Width = 16 glyph, Color depth = 4bpp).
* "FONT0 WIDTH TABLE.XML" - width table.

![Cut's table](https://raw.githubusercontent.com/Meloman19/PersonaFont/master/cut_table.jpg)

3. Edit bitmap and cut's table.

**Attention! Don't change cut's table structure! Result "FONT0.BMP" must have origin size and color depth!**

4. Place source "FONT0.FNT" and changed "FONT0.BMP","FONT0 WIDTH TABLE.XML" near "PersonaFont.exe"
5. Run "PersonaFont.exe", enter "com" and you got:
* "FONT0 NEW.FNT"

# How encoded text
![How encoded text](https://raw.githubusercontent.com/Meloman19/PersonaFont/master/how_encoded_text.jpg)
