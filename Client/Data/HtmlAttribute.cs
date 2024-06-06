using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eudora.Net.Data
{
    public class HtmlAttribute
    {
        ///////////////////////////////////////////////////////////
        #region Fields
        /////////////////////////////

        private string _Name = string.Empty;
        private string _Value = string.Empty;

        /////////////////////////////
        #endregion Fields
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        public string Name
        {
            get => _Name;
            set => _Name = value;
        }

        public string Value
        {
            get => _Value;
            set => _Value = value;
        }

        public static HtmlAttribute Default = new();

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////
        
        public HtmlAttribute()
        {

        }

        public HtmlAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
