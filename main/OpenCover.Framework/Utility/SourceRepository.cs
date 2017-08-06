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
		/// Default Constructor 
		/// </summary>
		public SourceRepository()
		{
		}

        /// <summary>
        /// Get string Text Source by FileID
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public CodeCoverageStringTextSource GetCodeCoverageStringTextSource (uint fileId) {
            CodeCoverageStringTextSource source = null;
            if (fileId != 0) {
                this.TryGetValue (fileId, out source);
            }
            return source;
        }

        /// <summary>
        /// SequencePoint source-string if available, else empty string 
        /// </summary>
        /// <param name="sp">SequencePoint</param>
        /// <returns>string</returns>
        public string GetSequencePointText (SequencePoint sp) {
            if (sp != null) {
                CodeCoverageStringTextSource source = GetCodeCoverageStringTextSource (sp.FileId);
                return source != null ? source.GetText(sp) : "";
            }
            return "";
        }

        #region IDictionary implementation

        /// <summary>
		/// Implements IDictionary 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool ContainsKey(uint key)
		{
			return repo.ContainsKey(key);
		}

		/// <summary>
		/// Implements IDictionary 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void Add(uint key, CodeCoverageStringTextSource value)
		{
			repo.Add(key, value);
		}

		/// <summary>
		/// Implements IDictionary 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool Remove(uint key)
		{
			return repo.Remove(key);
		}

		/// <summary>
		/// Implements IDictionary 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGetValue(uint key, out CodeCoverageStringTextSource value)
		{
			return repo.TryGetValue(key, out value);
		}

		/// <summary>
		/// Implements IDictionary 
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
		/// Implements IDictionary 
		/// </summary>
		public ICollection<uint> Keys {
			get {
				return repo.Keys;
			}
		}

		/// <summary>
		/// Implements IDictionary 
		/// </summary>
		public ICollection<CodeCoverageStringTextSource> Values {
			get {
				return repo.Values;
			}
		}

		#endregion

		#region ICollection implementation

		/// <summary>
		/// Implements ICollection
		/// </summary>
		/// <param name="item"></param>
		public void Add(KeyValuePair<uint, CodeCoverageStringTextSource> item)
		{
			repo.Add(item);
		}

		/// <summary>
		/// Implements ICollection
		/// </summary>
		public void Clear()
		{
			repo.Clear();
		}

		/// <summary>
		/// Implements ICollection
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Contains(KeyValuePair<uint, CodeCoverageStringTextSource> item)
		{
			return repo.Contains(item);
		}

		/// <summary>
		/// Implements ICollection
		/// </summary>
		/// <param name="array"></param>
		/// <param name="arrayIndex"></param>
		public void CopyTo(KeyValuePair<uint, CodeCoverageStringTextSource>[] array, int arrayIndex)
		{
			repo.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Implements ICollection
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Remove(KeyValuePair<uint, CodeCoverageStringTextSource> item)
		{
			return repo.Remove(item);
		}

		/// <summary>
		/// Implements ICollection
		/// </summary>
		public int Count {
			get {
				return repo.Count;
			}
		}

		/// <summary>
		/// Implements ICollection
		/// </summary>
		public bool IsReadOnly {
			get {
				return repo.IsReadOnly;
			}
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Implements IEnumerable
		/// </summary>
		/// <returns></returns>
		public IEnumerator<KeyValuePair<uint, CodeCoverageStringTextSource>> GetEnumerator()
		{
			return repo.GetEnumerator();
		}

		/// <summary>
		/// Implements IEnumerable
		/// </summary>
		/// <returns></returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)repo).GetEnumerator();
		}

		#endregion
	}
}
