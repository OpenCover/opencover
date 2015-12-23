/*
 * Created by SharpDevelop.
 * User: ddur
 * Date: 23.12.2015.
 * Time: 14:17
 *
 * Copyright https://github.com/ddur  
 * This source code is released under the MIT License; see the accompanying license file.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using OpenCover.Framework.Model;

namespace OpenCover.Framework.Utility
{
	/// <summary>
	/// Collection of CodeCoverageStringTextSources
	/// </summary>
	public class SourceRepository : IDictionary<uint, CodeCoverageStringTextSource>
	{
		private readonly IDictionary<uint, CodeCoverageStringTextSource> repo = new Dictionary<uint, CodeCoverageStringTextSource>();
		/// <summary>
		/// 
		/// </summary>
		public SourceRepository()
		{
		}

        private uint fileID_cache = 0;
        private CodeCoverageStringTextSource textSource_cache = null;

        public CodeCoverageStringTextSource getCodeCoverageStringTextSource (uint fileId) {
            CodeCoverageStringTextSource source = null;
            if (fileId != 0) {
                if (fileID_cache == fileId) {
                    source = textSource_cache;
                } else {
                    this.TryGetValue (fileId, out source);
                    if (source != null) {
                        fileID_cache = fileId;
                        textSource_cache = source;
                    }
                }
            }
            return source;
        }

        /// <summary>
        /// SequencePoint source-string if available, else empty string 
        /// </summary>
        /// <param name="sp">SequencePoint</param>
        /// <returns>string</returns>
        public string getSequencePointText (SequencePoint sp) {
            if (sp != null) {
                CodeCoverageStringTextSource source = this.getCodeCoverageStringTextSource (sp.FileId);
                return source != null ? source.GetText(sp) : "";
            }
            return "";
        }
        /// <summary>
        /// True if SequencePoint source-string == "{"
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        public bool isLeftBraceSequencePoint (SequencePoint sp) {
            return sp.isSingleCharSequencePoint && this.getSequencePointText(sp) == "{";
        }
        /// <summary>
        /// True if SequencePoint source-string == "}"
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        public bool isRightBraceSequencePoint (SequencePoint sp) {
            return sp.isSingleCharSequencePoint && this.getSequencePointText(sp) == "}";
        }

        #region IDictionary implementation
		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool ContainsKey(uint key)
		{
			return repo.ContainsKey(key);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void Add(uint key, CodeCoverageStringTextSource value)
		{
			repo.Add(key, value);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool Remove(uint key)
		{
			return repo.Remove(key);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGetValue(uint key, out CodeCoverageStringTextSource value)
		{
			return repo.TryGetValue(key, out value);
		}
		/// <summary>
		/// 
		/// </summary>
		public CodeCoverageStringTextSource this[uint key] {
			get {
				return repo[key];
			}
			set {
				repo[key] = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public ICollection<uint> Keys {
			get {
				return repo.Keys;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public ICollection<CodeCoverageStringTextSource> Values {
			get {
				return repo.Values;
			}
		}
		#endregion

		#region ICollection implementation
		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		public void Add(KeyValuePair<uint, CodeCoverageStringTextSource> item)
		{
			repo.Add(item);
		}
		/// <summary>
		/// 
		/// </summary>
		public void Clear()
		{
			repo.Clear();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Contains(KeyValuePair<uint, CodeCoverageStringTextSource> item)
		{
			return repo.Contains(item);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <param name="arrayIndex"></param>
		public void CopyTo(KeyValuePair<uint, CodeCoverageStringTextSource>[] array, int arrayIndex)
		{
			repo.CopyTo(array, arrayIndex);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Remove(KeyValuePair<uint, CodeCoverageStringTextSource> item)
		{
			return repo.Remove(item);
		}
		/// <summary>
		/// 
		/// </summary>
		public int Count {
			get {
				return repo.Count;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public bool IsReadOnly {
			get {
				return repo.IsReadOnly;
			}
		}
		#endregion

		#region IEnumerable implementation
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public IEnumerator<KeyValuePair<uint, CodeCoverageStringTextSource>> GetEnumerator()
		{
			return repo.GetEnumerator();
		}
		#endregion

		#region IEnumerable implementation
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)repo).GetEnumerator();
		}
		#endregion
	}
}
