//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
namespace OpenCover.Framework.Model
{
    /// <summary>
    /// A point may have a document reference
    /// </summary>
    public interface IDocumentReference
    {
        /// <summary>
        /// The document url
        /// </summary>
        string Document { get; set; }

        /// <summary>
        /// The document id after lookup
        /// </summary>
        uint FileId { get; set; }
    }
}