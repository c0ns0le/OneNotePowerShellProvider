#region Microsoft Community License
/*****
Microsoft Community License (Ms-CL)
Published: October 12, 2006

   This license governs  use of the  accompanying software. If you use
   the  software, you accept this  license. If you  do  not accept the
   license, do not use the software.

1. Definitions

   The terms "reproduce,"    "reproduction," "derivative works,"   and
   "distribution" have  the same meaning here  as under U.S. copyright
   law.

   A  "contribution" is the  original  software, or  any additions  or
   changes to the software.

   A "contributor"  is any  person  that distributes  its contribution
   under this license.

   "Licensed  patents" are  a contributor's  patent  claims that  read
   directly on its contribution.

2. Grant of Rights

   (A) Copyright   Grant-  Subject to  the   terms  of  this  license,
   including the license conditions and limitations in section 3, each
   contributor grants   you a  non-exclusive,  worldwide, royalty-free
   copyright license to reproduce its contribution, prepare derivative
   works of its  contribution, and distribute  its contribution or any
   derivative works that you create.

   (B) Patent Grant-  Subject to the terms  of this license, including
   the   license   conditions and   limitations   in  section  3, each
   contributor grants you   a non-exclusive, worldwide,   royalty-free
   license under its licensed  patents to make,  have made, use, sell,
   offer   for   sale,  import,  and/or   otherwise   dispose  of  its
   contribution   in  the  software   or   derivative  works  of   the
   contribution in the software.

3. Conditions and Limitations

   (A) Reciprocal  Grants- For any  file you distribute  that contains
   code from the software (in source code  or binary format), you must
   provide recipients the source code  to that file  along with a copy
   of this  license,  which license  will  govern that  file.  You may
   license other  files that are  entirely  your own  work and  do not
   contain code from the software under any terms you choose.

   (B) No Trademark License- This license does not grant you rights to
   use any contributors' name, logo, or trademarks.

   (C)  If you  bring  a patent claim    against any contributor  over
   patents that you claim  are infringed by  the software, your patent
   license from such contributor to the software ends automatically.

   (D) If you distribute any portion of the  software, you must retain
   all copyright, patent, trademark,  and attribution notices that are
   present in the software.

   (E) If  you distribute any  portion of the  software in source code
   form, you may do so only under this license by including a complete
   copy of this license with your  distribution. If you distribute any
   portion  of the software in  compiled or object  code form, you may
   only do so under a license that complies with this license.

   (F) The  software is licensed  "as-is." You bear  the risk of using
   it.  The contributors  give no  express  warranties, guarantees  or
   conditions. You   may have additional  consumer  rights  under your
   local laws   which  this license  cannot   change. To   the  extent
   permitted under  your local  laws,   the contributors  exclude  the
   implied warranties of   merchantability, fitness for  a  particular
   purpose and non-infringement.


*****/
#endregion

using System;
using System.Xml;

namespace Microsoft.Office.OneNote.PowerShell.Provider
{
    /// <summary>
    /// This class implements helper routines for manipulating OneNote XML fragments for the
    /// UpdateHierarchy API.
    /// </summary>
    class OneNoteXml
    {
        /// <summary>
        /// the namespace schema for OneNote.
        /// </summary>
        public const string OneNoteSchema = @"http://schemas.microsoft.com/office/onenote/2007/onenote";

        #region Properties

        private XmlDocument _document;
        
        private XmlNamespaceManager nsmgr;

        /// <summary>
        /// This is the underlying document that we've built.
        /// </summary>
        public XmlDocument Document
        {
            get { return _document; }
        }
        #endregion

        public OneNoteXml( )
        {
            _document = new XmlDocument( );
            nsmgr = new XmlNamespaceManager(_document.NameTable);
            nsmgr.AddNamespace("one", OneNoteSchema);
        }

        /// <summary>
        /// Creates a OneNote section.
        /// </summary>
        /// <param name="id">OPTIONAL: a OneNote identifier for the section.</param>
        /// <param name="name">OPTIONAL: The name for the section.</param>
        /// <returns>The XML element representing the section. The caller needs to insert it into the document
        /// in the appropriate location.</returns>
        public XmlElement CreateSection(string name, string id)
        {
            return coreCreateElement("Section", name, id);
        }

        public XmlElement CreatePage(string name, string id)
        {
            XmlElement page = coreCreateElement("Page", name, id);
            if (!String.IsNullOrEmpty(name))
            {
                XmlElement title = coreCreateElement("Title", null, null);
                page.AppendChild(title);
                XmlElement oe = coreCreateElement("OE", null, null);
                title.AppendChild(oe);
                XmlElement t = coreCreateElement("T", null, null);
                oe.AppendChild(t);
                t.AppendChild(_document.CreateCDataSection(name));
            }
            return page;
        }

        public XmlElement CreateOutline( )
        {
            XmlElement outline = coreCreateElement("Outline", null, null);
            return outline;
        }

        public XmlElement CreateOEChildren( )
        {
            XmlElement oeChildren = coreCreateElement("OEChildren", null, null);
            return oeChildren;
        }

        /// <summary>
        /// Creates an one:OE element and populates it with the one:T element
        /// <c>text</c>.
        /// </summary>
        /// <param name="text">The text to embed.</param>
        /// <returns>A pointer to the OE element. (NOT the one:T element.)</returns>
        public XmlElement CreateTextOE(string text)
        {
            XmlElement oe = coreCreateElement("OE", null, null);
            XmlElement t = coreCreateElement("T", null, null);
            oe.AppendChild(t);
            t.AppendChild(_document.CreateCDataSection(text));
            return oe;
        }

        public XmlElement CreateInsertedFileOE(System.IO.FileInfo fi)
        {
            XmlElement oe = coreCreateElement("OE", null, null);
            XmlElement e = coreCreateElement("InsertedFile", null, null);
            oe.AppendChild(e);
            XmlAttribute path = _document.CreateAttribute("pathSource");
            path.Value = fi.FullName;
            e.Attributes.Append(path);
            return oe;
        }

        /// <summary>
        /// Finds a OneNote element named <c>elementName</c> under <c>parent</c> and
        /// returns it, OR creates the element if it does not exist.
        /// </summary>
        /// <param name="parent">The parent to start searching from.</param>
        /// <param name="elementNames">The element names to look for, in the order of finding them from <c>parent</c>. 
        /// You must insert the appropriate namespace prefix.
        /// <c>one:</c> is predefined for you as the OneNote prefix.</param>
        /// <returns>A pointer to the XmlElement.</returns>
        public XmlElement FindOrCreate(XmlNode parent, string [] elementNames)
        {
            XmlElement e = null;
            foreach (string elementName in elementNames)
            {
                e = (XmlElement)parent.SelectSingleNode(elementName, nsmgr);
                if (e == null)
                {
                    e = _document.CreateElement(elementName, OneNoteSchema);
                    if (elementName == "one:Outline")
                    {
                        //
                        //  Special hack when creating Outline elements -- make the font
                        //  be courier-new. Make the width be 600.
                        //

                        XmlAttribute style = _document.CreateAttribute("style");
                        style.Value = "font-family:'Courier New';font-size:10.0pt";
                        e.Attributes.Append(style);
                        XmlElement size = _document.CreateElement("one:Size", OneNoteSchema);
                        XmlAttribute width = _document.CreateAttribute("width");
                        width.Value = "600";
                        size.Attributes.Append(width);
                        XmlAttribute height = _document.CreateAttribute("height");
                        height.Value = "0";
                        size.Attributes.Append(height);
                        e.AppendChild(size);
                    }
                    parent.AppendChild(e);
                }
                parent = e;
            }
            return e;
        }

        /// <summary>
        /// Adds an HTML block (suitable for importing) to a page or outline.
        /// </summary>
        /// <param name="parent">the page or outline in question.</param>
        /// <param name="html">The HTML to add.</param>
        public void AddHtmlBlock(XmlElement parent, string html)
        {
            if (parent.LocalName == "Page")
            {
                XmlElement outline = coreCreateElement("Outline", null, null);
                parent.AppendChild(outline);
                parent = outline;
                outline = coreCreateElement("OEChildren", null, null);
                parent.AppendChild(outline);
                parent = outline;
            }
            XmlElement hb = coreCreateElement("HTMLBlock", null, null);
            hb.AppendChild(_document.CreateCDataSection(html));
            parent.AppendChild(hb);
        }

        /// <summary>
        /// Core private helper routine to create OneNote XML elements.
        /// </summary>
        /// <param name="elementName">The name of the element to create (e.g., "Section" or "Page")</param>
        /// <param name="id">OPTIONAL: OneNote ID for this element.</param>
        /// <returns>A pointer to the newly-minted element.</returns>
        private XmlElement coreCreateElement(string elementName, string name, string id)
        {
            XmlElement element = _document.CreateElement("one", elementName, OneNoteSchema);
            applyOptionalAttribute("ID", id, element);
            applyOptionalAttribute("name", name, element);
            return element;
        }

        private void applyOptionalAttribute(string name, string value, XmlElement section)
        {
            if (!String.IsNullOrEmpty(value))
            {
                XmlAttribute idAttribute = _document.CreateAttribute(name);
                idAttribute.Value = value;
                section.Attributes.Append(idAttribute);
            }
        }
    }
}
