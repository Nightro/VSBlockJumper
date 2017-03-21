//------------------------------------------------------------------------------
// <copyright file="BlockJumper.cs" company="OzmosisGames">
//     Copyright (c) OzmosisGames.  All rights reserved.
// </copyright>
// <author>Anthony Reddan</author>
// <date>20/03/17</date>
//------------------------------------------------------------------------------

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace VSBlockJumper
{
    /// <summary>
    /// Establishes an <see cref="IAdornmentLayer"/> to place the adornment on and exports the <see cref="IWpfTextViewCreationListener"/>
    /// that instantiates the adornment on the event of a <see cref="IWpfTextView"/>'s creation
    /// </summary>
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class BlockJumperTextViewCreationListener : IWpfTextViewCreationListener
    {
        /// <summary>
        /// Called when a text view having matching roles is created over a text data model having a matching content type.
        /// Instantiates a BlockJumper manager when the textView is created.
        /// </summary>
        /// <param name="textView">The <see cref="IWpfTextView"/> upon which the adornment should be placed</param>
        public void TextViewCreated(IWpfTextView textView)
        {
            // The adornment will listen to any event that changes the layout (text changes, scrolling, etc)
            new BlockJumper(textView);
        }
    }
}
