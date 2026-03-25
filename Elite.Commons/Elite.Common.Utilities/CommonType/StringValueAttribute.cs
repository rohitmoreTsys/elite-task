using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.CommonType
{
    public class StringValueAttribute : System.Attribute
    {

        private string _value;

        public StringValueAttribute(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }

    }
}
