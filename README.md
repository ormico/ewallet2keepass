**Project Description**
Convert an eWallet export to a KeePass 2.x XML file, which can be imported into KeePass 2.x

This project will convert an export file created by Ilium Software eWallet (R) for Windows version 6.1.4.26824 and convert it into a KeePass KDBX (2.x) file. (This will probably work with other versions of eWallet, but this is the version I tested with.)

The conversion isn't perfect, but it does provide a way to move from eWallet to KeePass. 

Usage: 
# Export your password database from eWallet using the Export command found under the Tools menu.
# Use this tool to conver the files: {{c:\> ZacksFiasco.ewallet2keepass.exe [infile.txt [outfile.kdbx](infile.txt-[outfile.kdbx)]}}
# Use the KeePass Import command under the File menu to import the new file into an existing KeePass database. All records are imported into the root of the KeePass tree. This tool will try to match up username and password fileds, as well as URL fields, but any properties that this tool doesn't recognize will be placed in a string field in KeePass under the Advanced tab for an entry.

KeePass: [http://keepass.info/](http___keepass.info_)

eWallet: [http://www.iliumsoft.com/site/ew/ewallet.php](http___www.iliumsoft.com_site_ew_ewallet.php)

Registered trademarks belong to their respective owners.
This project is not associated with eWallet or KeepPass.
