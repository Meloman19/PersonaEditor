# Persona 3/4 Font Decompressor/Compressor
## How it's work
1. Place "FONT0.FNT" (or renamed "FONT1.FNT") file near "PersonaFont.exe"
2. Run "PersonaFont.exe", enter "decom" and you got:
* "FONT0.BMP" - indexed bitmap (Width = 16 glyph, Color depth = 4bpp).
* "FONT0 CUT.TXT" - cut's table. One line has 16 pair of number (left cut, right cut). Both numbers mean count pixels from left edge.
3. Editing bitmap and cut's table.

**Attention! Don't change cut's table structure! Result "FONT0.BMP" must have origin size and color depth!**

4. Place origin "FONT0.FNT" and changed "FONT0.BMP","FONT0 CUT.TXT" near "PersonaFont.exe"
5. Run "PersonaFont.exe", enter "com" and you got:
* "FONT0 NEW.FNT"
