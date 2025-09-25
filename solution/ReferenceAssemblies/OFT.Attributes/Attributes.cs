// Reference assembly for OFT.Attributes
using System;

namespace OFT.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HelpLinkAttribute : Attribute
    {
        public string Url { get; set; }

        public HelpLinkAttribute(string url)
        {
            Url = url;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ParameterAttribute : Attribute
    {
        public ParameterAttribute()
        {
        }
    }
}