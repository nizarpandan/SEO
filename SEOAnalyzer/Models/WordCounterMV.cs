using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SEOCounter.Models;

namespace SEOAnalyzer.Models
{
	public class WordCounterMV
	{
		public List<Word> Words { get; set; }
		public List<Word> MetaTags { get; set; }
		public bool CheckMetaTags { get; set; }
		public bool CheckWords { get; set; }
		public bool CheckExternalLink { get; set; }
		public string Url { get; set; }
		public string StopWords { get; set; }
		public int TotalExternalLink { get; set; } = 0;
		public bool IsUrl { get; set; }
	}
}
