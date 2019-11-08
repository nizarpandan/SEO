using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using SEOCounter.Models;
using System.Linq;

namespace SEOCounter
{
	public class WordCounter
	{
		private readonly char[] _wordsSeparatorDefault = { ' ', ',', '.', '\r', '\n', '\t', '/' };
		private readonly char[] _wordsSeparator;
		private readonly HtmlNode _node;
		private List<Word> _wordsList = new List<Word>();
		private List<Word> _stopWordsList = new List<Word>();
		private List<Word> _keywordList = new List<Word>();
		private int _extLinksNumber = -1;

		public WordCounter(string pageUrlOrText, bool isUrl, string stopsWordsString = null, char[] wordsSeparator = null)
		{
			if (wordsSeparator == null)
			{
				_wordsSeparator = _wordsSeparatorDefault;
			}

			if (!String.IsNullOrEmpty(stopsWordsString))
			{
				//insert stopWords in List
				SplitTextAndCount(stopsWordsString, _stopWordsList, null, _wordsSeparator, true);
				if (_stopWordsList.Count == 0)
				{
					_stopWordsList = null;
				}
			}

			HtmlDocument htmlDocument;

			try
			{
				if (isUrl)
				{
					htmlDocument = new HtmlWeb().Load(pageUrlOrText); //load web page by url
				}
				else
				{
					htmlDocument = new HtmlDocument();
					htmlDocument.LoadHtml(pageUrlOrText); // load web page/text from string
				}
			}
			catch (Exception ex)
			{
				throw new Exception("HtmlAgilityPack: " + ex.Message);
			}

			_node = htmlDocument.DocumentNode;
		}

		public List<Word> GetWordsList()
		{
			if (_wordsList.Count > 0)
			{
				return _wordsList; //return already counted dic
			}

			ProcessAllTextNodes(_node, _wordsList, _stopWordsList, _wordsSeparator, true);

			if (_wordsList.Count == 0)
			{
				_wordsList = null;
			}

			return _wordsList;
		}


		public List<Word> GetKeywordsList()
		{
			if (_keywordList.Count > 0)
			{
				return _keywordList;
			}

			_keywordList = GetKeywordsDictionaryFromMetaTags(_node, _wordsSeparator);
			if (_keywordList != null) 
			{
				if (_wordsList.Count > 0)
				{
					List<Word> keywordsListTemp = new List<Word>();
					foreach (var keyword in _keywordList)
					{
						if (IsWordExist(_wordsList, keyword.Value))
						{
							var obj = _wordsList.FirstOrDefault(x => x.Value == keyword.Value);
							keywordsListTemp.Add(obj);
						}
						else
						{
							keywordsListTemp.Add(new Word { Count = 0, Value = keyword.Value });
						}
					}
					_keywordList = keywordsListTemp;
				}
				else
				{
					ProcessAllTextNodes(_node, _keywordList, _stopWordsList, _wordsSeparator, false);
				}
			}
				
			return _keywordList;
		}

		public int GetExternalLinksNumber()
		{
			if (_extLinksNumber >= 0)
			{
				return _extLinksNumber;
			}

			_extLinksNumber = 0;
			var nodes = _node.SelectNodes(@"//a[@href]");
			if (nodes != null)
			{
				foreach (var link in nodes)
				{
					var att = link.Attributes["href"];
					if (att == null)
					{
						continue;
					}
					
					var href = att.Value;
					if (href.StartsWith("javascript", StringComparison.InvariantCultureIgnoreCase) ||
						href.StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
					{
						continue;
					}
					
					var uri = new Uri(href, UriKind.RelativeOrAbsolute);

					if (uri.IsAbsoluteUri)
					{
						_extLinksNumber++;
					}
				}
			}

			return _extLinksNumber;
		}


		private void ProcessAllTextNodes(HtmlNode rootNode, List<Word> listWords, List<Word> stopWordsDictionary,
			char[] wordsSeparator, bool canAddNewKeys)
		{
			foreach (var node in rootNode.Descendants("#text"))
			{
				if (String.Compare(node.ParentNode.Name, "script", true, CultureInfo.InvariantCulture) != 0) //ignore inner text in <script>abcabc</script>
				{
					string s = node.InnerText;
					if (!IsCDATA(s)) //ignore CDATA
					{
						s = ReplaceNotLetters(ReplaceSpecialCharacters(s)).Trim(); //clean text
						if (!String.IsNullOrEmpty(s))
						{
							SplitTextAndCount(s, listWords, stopWordsDictionary, wordsSeparator, canAddNewKeys);
						}
					}
				}
			}
		}

		private List<Word> GetKeywordsDictionaryFromMetaTags(HtmlNode rootNode, char[] wordsSeparator)
		{
			HtmlNode keywordsNode = rootNode.SelectSingleNode("//meta[@name='Keywords']");
			if (keywordsNode != null)
			{
				string keyWords = keywordsNode.GetAttributeValue("content", "");
				if (!String.IsNullOrEmpty(keyWords))
				{
					string[] keyWordsSplitted = keyWords.Split(wordsSeparator, StringSplitOptions.RemoveEmptyEntries);
					if (keyWordsSplitted.Length > 0)
					{
						List<Word> keywordList = new List<Word>();
						foreach (var keyWord in keyWordsSplitted)
						{
							keywordList.Add(new Word { Count = 0, Value = keyWord.ToLower() });
						}
						return keywordList;
					}
				}
			}
			return null;
		}


		#region Helper

		private void SplitTextAndCount(string s, List<Word> listWord, List<Word> stopWordsDictionary, char[] wordsSeparator, bool isCanAddNewKeys)
		{
			if (String.IsNullOrEmpty(s))
			{
				return;
			}

			string[] words = s.Split(wordsSeparator, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < words.Length; i++)
			{
				if (!IsWordExist(listWord, words[i].ToLower()))
				{
					if (isCanAddNewKeys)
					{
						if (stopWordsDictionary == null || !IsWordExist(stopWordsDictionary, words[i].ToLower()))
						{
							listWord.Add(new Word { Count = 1, Value = words[i].ToLower() });
						}
					}
				}
				else
				{
					var obj = listWord.FirstOrDefault(x => x.Value == words[i].ToLower());
					if (obj != null)
					{
						obj.Count += 1;
					}
				}
			}

		}

		private string ReplaceSpecialCharacters(string s)
		{
			return Regex.Replace(s, @"&[^\s;]+;", " ");
		}

		private string ReplaceNotLetters(string s)
		{
			return Regex.Replace(s, @"[^a-zA-Z]+", " ");
		}

		private bool IsWordExist(List<Word> list, string word)
		{
			return list.Any(item => item.Value == word);
		}

		private bool IsCDATA(string s)
		{
			if (s.Length >= 10)
			{
				if (s.Substring(0, 10).ToLower().Contains("[cdata"))
				{
					return true;
				}
			}

			return false;
		}

		#endregion

	}
}
