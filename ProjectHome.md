# KPEnhancedListview  [![](http://kontactr.com/pics/small.gif)](http://kontactr.com/user/frankglaser) #
> Extend the KeePass Listview for inline editing.
<p></li></ul>

<wiki:gadget url="https://gcodeadsense.googlecode.com/svn/release/gCodeAdSense/gCodeAdSense.xml" width="468" height="60" border="0" up_ad_client="3727478741117936" up_ad_slot="0072735561" up_ad_width="468" up_ad_height="60" up_ad_project=document.URL /><br>
<p>

<a href='Hidden comment: 
*Update in progress - Please wait*
==KeePass message after update==
If You get the following message from KeePass:
"The following plugin is incompatible with the current KeePass version"
Try the following steps:
Maybe You have used an old Version below 0.9.1.0 then use the latest KPEnhancedListview V0_9_1_0
or You have the old kpenhancedlistview.DLL in Your Keepass folder, then just delete it and make sure You have only the new kpenhancedlistview.PLGX.
'></a>

## News ##
  * Version 0.9.3.0 is online (Changes http://kpenhancedlistview.blogspot.com/2013/08/kpenhancedlistview-0930.html)

  * KeePass gets native support for Custom Columns in Version 2.11
  * Blog: http://kpenhancedlistview.blogspot.com
  * Discuss at: http://groups.google.com/group/kpEnhanceListview
  * Contact me: [Mail](http://kontactr.com/user/frankglaser)
<p></li></ul>

<table border='0'>
> <tr>
<blockquote><td>
<blockquote>
</blockquote></td>
<td>
<blockquote><a href='http://twitter.com/share?url=http://code.google.com/p/kpenhancedlistview/'><img src='http://kpenhancedlistview.googlecode.com/svn/wiki/Twitter.png' /></a>
</blockquote></td>
<td>
<blockquote><a href='http://www.facebook.com/sharer.php?u=http://code.google.com/p/kpenhancedlistview/&t=KPEnhancedListview'><img src='http://kpenhancedlistview.googlecode.com/svn/wiki/Facebook.png' /></a>
</blockquote></td>
<td align='center' valign='middle'>
<blockquote><wiki:gadget border="0" url="http://stefansundin.com/stuff/flattr/google-project-hosting.xml" width="110" height="20" up_uid="frankglaser" up_title="kpenhancedlistview" up_url="http://code.google.com/p/kpenhancedlistview/" /><br>
</blockquote></td>
<td align='center' valign='middle'>
<blockquote><a href='https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=CH7PBX4C8WZAE&item_name=KPEnhancedListview&cancel_return=http://code.google.com/p/kpenhancedlistview&return=http://code.google.com/p/kpenhancedlistview&currency_code=EUR'><img src='https://www.paypal.com/en_US/i/btn/btn_donate_LG.gif' /></a>
</blockquote></td>
<td align='center' valign='middle'>
<blockquote><wiki:gadget url="http://www.ohloh.net/p/527283/widgets/project_thin_badge.xml" height="36" width="120px" border="0"/><br>
</blockquote></td>
</blockquote><blockquote></tr>
</table>
<p></blockquote>

## Description ##
**KPEnhancedListview [v0.9.2.1 for KeePass ≥ 2.18] (Microsoft .NET Framework ≥ 3.5)**
  * KPEL  was developed for everyone who wants to edit the entries directly without having to open the EntryForm and the Tabs there.
  * PLGX plugin file format
  * Update notification via the KeePass Update check mechanism

**KPEL Menu**
  * All options can be enabled / disabled separately via the menu
  * All settings will be saved using the KeePass session saving

> ![![](http://2.bp.blogspot.com/-7741p-2SFAM/T1toCh7nS8I/AAAAAAAAALI/l_7C3FqONRI/s200/KPELMenu.jpg)](http://2.bp.blogspot.com/-7741p-2SFAM/T1toCh7nS8I/AAAAAAAAALI/l_7C3FqONRI/s1600/KPELMenu.jpg)

**1) Inline Editing:**
> Now You can edit the entries directly in the listview.

> Howto activate:
    * F3 or Ctrl+F2 will edit the selected entries title.
    * A slow double click with the left mouse button (like Windows Explorer) will directly edit a subitem (A fast double click will execute the KeePass default function).
    * A Slow double click on images will open the image editor

> Howto leave:
    * Enter and a left click on the listview saves the changes. If there are no changes detected, nothing will be saved.
    * Escape, resize or other actions, that cause the InlineEditing lost the focus discard/cancel the changes.
    * Mouse wheel scrolling the list is allowed during the Inline Editing.

> During editing:
    * Tab goes through the subitems
    * Shift-Tab goes backwards
    * Cursor Keys will step into the corresponding direction
> > > But only if the whole text is marked or the cursor reaches the end of the text. For e.g.
> > > Cursor Key Right will only step forward if the Cursor reaches the end of the text inside
> > > Cursor Down will only step down if the last line of the text is reached
    * Ctrl + Cursor Key will do a force step
    * Use Pos1/Home or End to unselect the text
    * Pos1/Home will set the cursor to the line beginning
    * End will set the cursor to the line end


> Notes to Multiline Editing for e.g. the notes field:
    * The height of the Multiline Edit field will be adapted dynamically to show all lines
    * Ctrl+Enter will add a new line

> If a field is protected it will be saved protected.

> If a subitem or user string is not defined for the selected entry, it will be created.
> Same if You have defined a new customColumn.

> Following fields display only a read only textbox, so You can copy the content, but not modify it.
> CreationTime, LastAccessTime, LastModificationTime, ExpiryTime, Attachment, Uuid
> All other fields can be edited, also the CustomColums, Tags ...!

> If a KeePass column is marked as hidden (asterisks) it will be displayed as plain text during editing.

> Custom Columns from Plugins are ReadOnly per default.

> ![![](http://3.bp.blogspot.com/-SiXShpVyN0E/T1txSnKJrDI/AAAAAAAAALg/4v6ATnkD-84/s200/Preview_0_9_1_0_Listview.jpg)](http://3.bp.blogspot.com/-SiXShpVyN0E/T1txSnKJrDI/AAAAAAAAALg/4v6ATnkD-84/s1600/Preview_0_9_1_0_Listview.jpg) ![![](http://3.bp.blogspot.com/-1qLlDVtyKiQ/T1toDvsUPcI/AAAAAAAAALM/FqmYvGtNezc/s200/KPELMultiLine.jpg)](http://3.bp.blogspot.com/-1qLlDVtyKiQ/T1toDvsUPcI/AAAAAAAAALM/FqmYvGtNezc/s1600/KPELMultiLine.jpg)

**2) Add Entry:**
> Double click on empty listview area opens will add a new entry to the corresponding group

> ![![](http://3.bp.blogspot.com/-IDGSEhI2DYs/T1toCH5THYI/AAAAAAAAALA/V6XjhQGdIzY/s200/AddEntry.jpg)](http://3.bp.blogspot.com/-IDGSEhI2DYs/T1toCH5THYI/AAAAAAAAALA/V6XjhQGdIzY/s1600/AddEntry.jpg)

**3) Open Group on Header click:**
> For e.g. Search for an entry, the list will contain the matching entries grouped according to their folder.
> Now a double click on a group headerwill open the corresponding the folder and all entries will be displayed.

> ![![](http://2.bp.blogspot.com/-xeC0HE_qE98/T1toEm0MFMI/AAAAAAAAALY/uyvy8EjR5p8/s200/OpenGroup.jpg)](http://2.bp.blogspot.com/-xeC0HE_qE98/T1toEm0MFMI/AAAAAAAAALY/uyvy8EjR5p8/s1600/OpenGroup.jpg)

**4) Direct editing mode the entry view:**
> This mode allows the editing of the notes of the selected entry in the entry view of the main window.

> ![![](http://3.bp.blogspot.com/-LFyMXuYfYsw/UgEf_iECrYI/AAAAAAAAAao/GtG1cyGo3BY/s400/KP_NotesEditing.png)](http://3.bp.blogspot.com/-LFyMXuYfYsw/UgEf_iECrYI/AAAAAAAAAao/GtG1cyGo3BY/s1600/KP_NotesEditing.png)

Actually the TAN lists are not tested yet!

Tested with KeePass 2.18

Suggestions and comments are welcome.
You can either use the KeePass Forum, the Issue Tab, Blog Comments or use the contact form.
<p>

<wiki:gadget url="https://gcodeadsense.googlecode.com/svn/release/gCodeAdSense/gCodeAdSense.xml" width="468" height="60" border="0" up_ad_client="3727478741117936" up_ad_slot="0072735561" up_ad_width="468" up_ad_height="60" up_ad_project=document.URL /><br>
<p>

<h2>Mentions</h2>
<ul><li><a href='http://keepass.info/plugins.html#kpenhancedlistview'>http://keepass.info/plugins.html#kpenhancedlistview</a>
</li><li><a href='http://www.ohloh.net/p/kpenhancedlistview'>http://www.ohloh.net/p/kpenhancedlistview</a>
</li><li><a href='http://www.ghacks.net/2012/04/08/keepass-plugins-that-improve-the-password-managers-functionality/'>http://www.ghacks.net/2012/04/08/keepass-plugins-that-improve-the-password-managers-functionality/</a></li></ul>

<wiki:gadget url="http://www.ohloh.net/p/527283/widgets/project_factoids.xml" border="0" width="400" height="200"/>