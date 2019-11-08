using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SEOAnalyzer.Models;
using SEOCounter;
using SEOCounter.Models;

namespace SEOAnalyzer.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private WordCounterMV _wordCounterMV = new WordCounterMV();

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		public IActionResult Index(string sortOrder, bool checkMetaTags, bool checkWords, string url, bool checkExternalLink)
		{	
			return View(_wordCounterMV);
		}

		[HttpPost]
		public IActionResult Index(WordCounterMV wordCounterMV, string sortOrder)
		{
			ViewData["WordSortParam"] = string.IsNullOrEmpty(sortOrder) ? "word_desc" : "";
			ViewData["CountSortParam"] = sortOrder == "count_asc" ? "count_desc" : "count_asc";
			ViewData["Url"] = wordCounterMV.Url == null ? "" : wordCounterMV.Url;
			_wordCounterMV = wordCounterMV;

			if (!string.IsNullOrEmpty(_wordCounterMV.Url))
			{
				WordCounter wc = new WordCounter(_wordCounterMV.Url, _wordCounterMV.IsUrl, _wordCounterMV.StopWords);

				if (wordCounterMV.CheckWords)
				{
					_wordCounterMV.Words = wc.GetWordsList();

					if (_wordCounterMV.Words != null)
					{
						switch (sortOrder)
						{
							case "word_desc":
								_wordCounterMV.Words = _wordCounterMV.Words.OrderByDescending(item => item.Value).ToList();
								break;
							case "count_asc":
								_wordCounterMV.Words = _wordCounterMV.Words.OrderBy(item => item.Count).ToList();
								break;
							case "count_desc":
								_wordCounterMV.Words = _wordCounterMV.Words.OrderByDescending(item => item.Count).ToList();
								break;
							default:
								_wordCounterMV.Words = _wordCounterMV.Words.OrderBy(item => item.Value).ToList();
								break;
						}
					}
				}

				if (wordCounterMV.CheckMetaTags)
				{
					_wordCounterMV.MetaTags = wc.GetKeywordsList();

					if (_wordCounterMV.MetaTags != null)
					{
						switch (sortOrder)
						{
							case "word_desc":
								_wordCounterMV.MetaTags = _wordCounterMV.MetaTags.OrderByDescending(item => item.Value).ToList();
								break;
							case "count_asc":
								_wordCounterMV.MetaTags = _wordCounterMV.MetaTags.OrderBy(item => item.Count).ToList();
								break;
							case "count_desc":
								_wordCounterMV.MetaTags = _wordCounterMV.MetaTags.OrderByDescending(item => item.Count).ToList();
								break;
							default:
								_wordCounterMV.MetaTags = _wordCounterMV.MetaTags.OrderBy(item => item.Value).ToList();
								break;
						}
					}
				}

				if (_wordCounterMV.CheckExternalLink)
				{
					_wordCounterMV.TotalExternalLink = wc.GetExternalLinksNumber();
				}
			}

			return View(_wordCounterMV);
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
