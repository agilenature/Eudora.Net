using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eudora.Net.Data
{
    /// <summary>
    /// A small utility class that temporarily holds a piece of 
    /// embedded content while importing email from Qualcomm Eudora.
    /// </summary>
    internal class ImportedEmbeddedContent
    {
        public string Source { get; set; } = string.Empty;
        public string ID { get; set; } = string.Empty;
    }
}
