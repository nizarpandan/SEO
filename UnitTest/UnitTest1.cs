using NUnit.Framework;
using SEOCounter;

namespace UnitTest
{
	public class Tests
	{
		private WordCounter _wordCounter;

		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void SEOUrl()
		{
			// Arrange
			string url = "http://www.bbc.com/news/";
			// _wordCounter = new WordCounter(url, true, "display");

			// Act
			// _wordCounter.GetWordsDictionary();

			url = "https://www.w3schools.com/tags/tag_meta.asp";
			_wordCounter = new WordCounter(url, true);
			var obj = _wordCounter.GetKeywordsList();


			// Assert
			Assert.Pass();
		}

		[Test]
		public void SEOText()
		{
			// Arrange
			string url = "halo polo popo popo";
			_wordCounter = new WordCounter(url, false, "halo");

			// Act
			_wordCounter.GetWordsList();
			//_wordCounter.GetKeywordsDictionary();

			// Assert
			Assert.Pass();
		}

		[Test]
		public void CountExternalLink()
		{
			// Arrange
			string url = "http://www.bbc.com/news/";
			_wordCounter = new WordCounter(url, true);

			// Act
			int totalLink =_wordCounter.GetExternalLinksNumber();
			//_wordCounter.GetKeywordsDictionary();

			// Assert
			Assert.Greater(totalLink, 0);
		}
	}
}