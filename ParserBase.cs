using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Satwe_tool
{	
	public class ParserBase
	{
		public string m_content;
		protected string[] m_contentArray;		
		protected static ParserBase _instanceLeft;
        protected static ParserBase _instanceRight;
        public string m_filePath;
        public string m_sourceName;
		protected ParserBase()
		{
            m_contentArray = new string[0];
		}
		public virtual bool ReadFile(params string[] filePath)
		{
			if (!File.Exists(filePath[0]))
			{
				 return false;
			}
            m_filePath = filePath[0];
            Encoding coder = Encoding.Default;
			m_content = File.ReadAllText(filePath[0], coder);
            m_content = m_content.Replace('\0', ' ');
			m_contentArray = File.ReadAllLines(filePath[0], coder);
            for (int i = 0; i < m_contentArray.Length; i++)
            {
                m_contentArray[i] = m_contentArray[i].Replace('\0', ' ');
            }
			return true;
		}

        public bool AppendFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }
            m_filePath = filePath;
            Encoding coder = Encoding.Default;
            m_content=m_content+ File.ReadAllText(filePath, coder);
            m_content = m_content.Replace('\0', ' ');
            string[] appendArray=File.ReadAllLines(filePath, coder);
            List<string> strList = new List<string>();
            for (int i = 0; i < m_contentArray.Length; i++)
            {
                strList.Add(m_contentArray[i]);                
            }
            for (int i = 0; i < appendArray.Length; i++)
            {
                strList.Add(appendArray[i]);
            }
            m_contentArray = strList.ToArray();
            for (int i = 0; i < m_contentArray.Length; i++)
            {
                m_contentArray[i] = m_contentArray[i].Replace('\0', ' ');
            }
            return true;
        }

		public string FullContent(bool withNumber=true)
		{
			if (withNumber) {
                StringBuilder sb=new StringBuilder();
				for (int i = 0; i < m_contentArray.Length; i++) {
					int number = i + 1;
					sb.AppendFormat("{0,-6}{1}\n", number, m_contentArray[i]);
				}
				return sb.ToString();
			} else
				return m_content;			
		}
	}
}
