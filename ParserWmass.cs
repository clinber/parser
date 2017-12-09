using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Satwe_tool
{
	public class ParserWmass:ParserBase
	{
		public DataSummary m_SumInfo;
      
		protected ParserWmass()
		{
	    }
		public static ParserWmass InstanceLeft
		{
			get
			{
				if (_instanceLeft == null) {
                    _instanceLeft = new ParserWmass();
				}
                return _instanceLeft as ParserWmass;
			}
		}

        public static ParserWmass InstanceRight
        {
            get
            {
                if (_instanceRight == null)
                {
                    _instanceRight = new ParserWmass();
                }
                return _instanceRight as ParserWmass;
            }
        }


		public override bool ReadFile(params string[] filePaths)
		{
            m_SumInfo = new DataSummary();
            m_contentArray = new string[0];
            m_content = "";
            foreach (var item in filePaths)
            {
                if (!base.AppendFile(item))
                {
                    return false;
                }
            }

            this.ParseSummaryInfo();
            this.ParseSesmicInfo();
            this.ParseEiInfo();
            this.ParseMassInfo();
            this.ParseRvInfo();
           	
            if (m_sourceName==PathFinder.SAP)//Sap 周期与位移信息都在一个文件中，所以只需要在这里读取即可
            {
                SapTInfo();
                SapDispInfo();
            }
			return true;
		}

		/// <summary>
		/// 结构总信息
		/// </summary>
		/// <returns></returns>
		public DataSummary ParseSummaryInfo()
		{
            //简化处理，无需读取材料等信息
			if (m_sourceName==PathFinder.PKPM) {
				PkpmSummaryInfo();
            }
            else if (m_sourceName == PathFinder.YJK)
            {
				YjkSummaryInfo();
            }
            else if (m_sourceName == PathFinder.SAP)
            {
                SapSummaryInfo();
            }
            else if (m_sourceName == PathFinder.MIDAS)
            {
                MidasSummaryInfo();
            }				
			return m_SumInfo;
		}
		private void PkpmSummaryInfo()
		{
			PkpmFloorMatInfo();
			for (int i = 0; i < m_contentArray.Length; i++) {
                if (m_contentArray[i].Contains("MBASE"))
                {
					string lineData = m_contentArray[i];
					m_SumInfo.BaseCount = int.Parse(lineData.Substring(lineData.IndexOf('=') + 1).Trim());
                }
                else if (m_contentArray[i].Contains("结构类别:"))
                {
					string lineData = m_contentArray[i];
					string[] splitData = lineData.Split(new char[]{' ',':','='},StringSplitOptions.RemoveEmptyEntries);
					m_SumInfo.StructureType = splitData[1];
                }
                else if (m_contentArray[i].Contains("MQIANGU"))
                {
                    string lineData = m_contentArray[i];
                    m_SumInfo.QianguFloor = int.Parse(lineData.Substring(lineData.IndexOf('=') + 1).Trim());
                }
                else if (m_contentArray[i].Contains("周期折减系数"))
                {
                    string lineData = m_contentArray[i];
                    m_SumInfo.Tc = double.Parse(lineData.Substring(lineData.IndexOf('=') + 1).Trim());
                }
			}
		}
		private void YjkSummaryInfo()
		{
			YjkFloorMatInfo();
			for (int i = 0; i < m_contentArray.Length; i++) {
                if (m_contentArray[i].Contains("地下室层数"))
                {
					string lineData = m_contentArray[i];
					string[] dataArray = lineData.Split(new char[] { ':', '=' }, StringSplitOptions.RemoveEmptyEntries);
					m_SumInfo.BaseCount = int.Parse(dataArray[1]);                   
				} else if (m_contentArray[i].Contains("结构体系")) {
					string lineData = m_contentArray[i];
					string[] splitData = lineData.Split(new char[] { ':', ' ' }, StringSplitOptions.RemoveEmptyEntries);
					m_SumInfo.StructureType = splitData[1];
                }
                else if (m_contentArray[i].Contains("嵌固端所在层号"))
                {
					string lineData = m_contentArray[i];
					string[] splitData = lineData.Split(new char[] { ':', ' ' }, StringSplitOptions.RemoveEmptyEntries);
					m_SumInfo.QianguFloor = int.Parse(splitData[1]);					
				}
                else if (m_contentArray[i].Contains("周期折减系数"))
                {
                    string lineData = m_contentArray[i];
                    string[] splitData = lineData.Split(new char[] { ':', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    m_SumInfo.Tc = double.Parse(splitData[1]);
                }    
			}
		}
        private void SapSummaryInfo()
        {
            SapFloorMatInfo();
            for (int i = 0; i < m_contentArray.Length; i++)
            {

                if (m_contentArray[i].Contains("NBASEMENT0"))
                {
                    string lineData = m_contentArray[i];
                    m_SumInfo.BaseCount = int.Parse(lineData.Substring(lineData.IndexOf('=') + 1).Trim());
                }
                else if (m_contentArray[i].Contains("KIND_TB"))
                {
                    string lineData = m_contentArray[i];
                    m_SumInfo.StructureType = lineData.Substring(lineData.LastIndexOf('(') + 1);
                    m_SumInfo.StructureType=m_SumInfo.StructureType.Remove(m_SumInfo.StructureType.Length - 1);
                } if (m_contentArray[i].Contains("ISUB_FIX"))
                {
                    string lineData = m_contentArray[i];
                    m_SumInfo.QianguFloor = int.Parse(lineData.Substring(lineData.IndexOf('=') + 1).Trim());
                }                
            }
        }
        private void MidasSummaryInfo()
        {
            MidasFloorMatInfo();
            for (int i = 0; i < m_contentArray.Length; i++)
            {

                if (m_contentArray[i].Contains("地下室层数:"))
                {
                    string lineData = m_contentArray[i];
                    m_SumInfo.BaseCount = int.Parse(lineData.Substring(lineData.IndexOf(':') + 1).Trim());
                }
                else if (m_contentArray[i].Contains("结构体系:"))
                {
                    string lineData = m_contentArray[i];
                    m_SumInfo.StructureType = lineData.Substring(lineData.LastIndexOf(':') + 1).Trim();
                } if (m_contentArray[i].Contains("嵌固层所在楼层:"))
                {
                    string lineData = m_contentArray[i];
                    m_SumInfo.QianguFloor = int.Parse(lineData.Substring(lineData.IndexOf(':') + 1).Trim());
                }

            }
        }

        //楼层与材料信息
		private List<InfoMaterial> PkpmFloorMatInfo()
		{
			int dataIndex = 0;
			for (int i = 0; i < m_contentArray.Length; i++) {
                if (m_contentArray[i].Contains("各层构件数量、构件材料"))
                {
					dataIndex = i + 6;
					break;
				}
			}
			string lastFloorNo = string.Empty;
			string lastStdNo = string.Empty;
			int floorCount = 0;
			for (int i = dataIndex; i < m_contentArray.Length; i++) {
				string lineData = m_contentArray[i];
				string[] splitData = lineData.Split(new char[] { ' ', '(', ')', '/' },StringSplitOptions.RemoveEmptyEntries);
				List<string> result = new List<string>(splitData);				
				if (result.Count ==0) {
					break;
				}
				if (result.Count==14) {
					lastFloorNo = result[0];
					lastStdNo = result[1];
					floorCount++;
				} else if (result.Count==18)//pkpm 2.2
				{
                    lastFloorNo = result[0];
                    lastStdNo = result[1];
                    floorCount++;
                    result.RemoveAt(6);
                    result.RemoveAt(9);
                    result.RemoveAt(12);
                    result.RemoveAt(12);
				}
				else {
					result.Insert(0, lastStdNo);
					result.Insert(0, lastFloorNo);
				}
				InfoMaterial elemInfo = new InfoMaterial();
				elemInfo.LoadData(result);
				m_SumInfo.FloorElemMatInfo.Add(elemInfo);
			}
			m_SumInfo.FloorCount =floorCount;
			return m_SumInfo.FloorElemMatInfo;
		}
		private List<InfoMaterial> YjkFloorMatInfo()
		{
			int dataIndex = 0;
			for (int i = 0; i < m_contentArray.Length; i++) {
                if (m_contentArray[i].Contains("各层构件数量、构件材料和层高"))
                {
					dataIndex = i + 5;
					break;
				}
			}
			string lastFloorNo = string.Empty;			
			int floorCount = 0;
			for (int i = dataIndex; i < m_contentArray.Length; i++) {
				string lineData = m_contentArray[i];
				string[] splitData = lineData.Split(new char[] { ' ', '(', ')', '/' },StringSplitOptions.RemoveEmptyEntries);
				if (splitData.Length != 8) {
					break;
				}
				if (lastFloorNo!=splitData[0]) {
					lastFloorNo = splitData[0];
					floorCount++;
				}
				InfoMaterial elemInfo = new InfoMaterial();
				m_SumInfo.FloorElemMatInfo.Add(elemInfo);
			}
			m_SumInfo.FloorCount = floorCount;
			return m_SumInfo.FloorElemMatInfo;
		}
        private List<InfoMaterial> SapFloorMatInfo()
        {
            int dataIndex = 0;
            for (int i = 0; i < m_contentArray.Length; i++)
            {
                if (m_contentArray[i].Contains("层号")&&m_contentArray[i].Contains("数量")&&m_contentArray[i].Contains("层高"))
                {
                    dataIndex = i + 1;
                    break;
                }
            }
            int lastFloorNo =0;
            int floorCount = 0;
            for (int i = dataIndex; i < m_contentArray.Length; i++)
            {
                string lineData = m_contentArray[i];
                string[] splitData = lineData.Split(new char[] { ' '}, StringSplitOptions.RemoveEmptyEntries);
                if (splitData.Length == 0)
                {
                    break;
                }
                int floorNo;
                if (int.TryParse(splitData[0],out floorNo) && lastFloorNo != floorNo)
                {
                    lastFloorNo = floorNo;
                    floorCount++;
                    InfoMaterial elemInfo = new InfoMaterial();
                    m_SumInfo.FloorElemMatInfo.Add(elemInfo);
                }                
               
            }
            m_SumInfo.FloorCount = floorCount;
            return m_SumInfo.FloorElemMatInfo;
        }
        private List<InfoMaterial> MidasFloorMatInfo()
        {
            int dataIndex = 0;
            for (int i = 0; i < m_contentArray.Length; i++)
            {
                if (m_contentArray[i].Contains("层号") && m_contentArray[i].Contains("高度") && m_contentArray[i].Contains("总高"))
                {
                    dataIndex = i +3;
                    break;
                }
            }
           
             int lastFloorNo =0;
            int floorCount = 0;
            for (int i = dataIndex; i < m_contentArray.Length; i++)
            {
                string lineData = m_contentArray[i];
                string[] splitData = lineData.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (splitData.Length < 2)
                {
                    break;
                }
                int floorNo;
                string floorNoStr = splitData[1].Trim('F');
                if (floorNoStr.Contains("B"))//地下室，B1F，转换成-1
                {
                    floorNoStr=floorNoStr.Trim('B');
                    if (int.TryParse(floorNoStr, out floorNo))
                    {
                        floorNo *= -1;
                        if (lastFloorNo != floorNo)
                        {
                            lastFloorNo = floorNo;
                            floorCount++;
                            InfoMaterial elemInfo = new InfoMaterial();
                            elemInfo.StoryNo = floorNo;
                            m_SumInfo.FloorElemMatInfo.Add(elemInfo);
                        }                        
                    }
                }
                else if (int.TryParse(floorNoStr, out floorNo) && lastFloorNo != floorNo)
                {
                    lastFloorNo = floorNo;
                    floorCount++;
                    InfoMaterial elemInfo = new InfoMaterial();
                    elemInfo.StoryNo = floorNo;
                    m_SumInfo.FloorElemMatInfo.Add(elemInfo);
                }
            }           
            m_SumInfo.FloorCount = floorCount;
            return m_SumInfo.FloorElemMatInfo;
        }
		

		/// <summary>
		/// 地震总信息
		/// </summary>
		/// <returns></returns>
		public InfoSesmic ParseSesmicInfo()
		{
			if (m_sourceName == PathFinder.PKPM) {
				PkpmSesmicInfo();
            }
            else if (m_sourceName == PathFinder.YJK)
            {
				YjkSesmicInfo();
            }
            else if (m_sourceName == PathFinder.SAP)
            {
                SapSesmicInfo();
            }
            else if (m_sourceName == PathFinder.MIDAS)
            {
                MidasSesmicInfo();
            }	
			
			return m_SumInfo.SesmicInfo;
		}
		private void PkpmSesmicInfo()
		{
			for (int i = 0; i < m_contentArray.Length; i++) {
                if (m_contentArray[i].Contains("风荷载信息"))
                {
					List<string> result = new List<string>();
					string lineData = m_contentArray[i + 1];
					result.Add(lineData.Substring(lineData.LastIndexOf('=') + 1).Trim());
					lineData = m_contentArray[i + 4];
					result.Add(lineData.Substring(lineData.LastIndexOf('=') + 1).Trim());
					lineData = m_contentArray[i + 5];
					result.Add(lineData.Substring(lineData.LastIndexOf('=') + 1).Trim());
					lineData = m_contentArray[i + 3];
					result.Add(lineData.Substring(lineData.LastIndexOf(':') + 1).Trim());
					m_SumInfo.WindInfo.LoadData(result);
				}
                if (m_contentArray[i].Contains("地震信息"))
                {
					List<string> result = new List<string>(8);
					
                    int findCount=0;
                    int indexTofind = i;
                    while (findCount < 8)
                    {
                        indexTofind++;
                        string lineData = m_contentArray[indexTofind];
                        if (lineData.Contains("地震烈度"))
                        {
                            result.Add(lineData.Substring(lineData.LastIndexOf('=') + 1).Trim());
                            findCount++;
                        }
                        else if (lineData.Contains("场地类别"))
                        {
                            result.Add(lineData.Substring(lineData.LastIndexOf('=') + 1).Trim());
                            findCount++;
                        }
                        else if (lineData.Contains("设计地震分组"))
                        {
                            result.Add(lineData.Substring(lineData.LastIndexOf(':') + 1).Trim());
                            findCount++;
                        }
                        else if (lineData.Contains("特征周期"))
                        {
                            result.Add(lineData.Substring(lineData.LastIndexOf('=') + 1).Trim());
                            findCount++;
                        }
                        else if (lineData.Contains("框架的抗震等级") && !lineData.Contains("钢"))
                        {
                            result.Add(lineData.Substring(lineData.LastIndexOf('=') + 1).Trim());
                            findCount++;
                        }
                        else if (lineData.Contains("剪力墙的抗震等级"))
                        {
                            result.Add(lineData.Substring(lineData.LastIndexOf('=') + 1).Trim());
                            findCount++;
                        }
                        else if (lineData.Contains("是否考虑偶然偏心"))
                        {
                            result.Add(lineData.Substring(lineData.LastIndexOf(':') + 1).Trim());
                            findCount++;
                        }
                        else if (lineData.Contains("是否考虑双向地震扭转效应"))
                        {
                            result.Add(lineData.Substring(lineData.LastIndexOf(':') + 1).Trim());
                            findCount++;
                        }
                    }                 
					m_SumInfo.SesmicInfo.LoadData(result);
					break;
				}
			}
		}
		private void YjkSesmicInfo()
		{
			for (int i = 0; i < m_contentArray.Length; i++) {
                if (m_contentArray[i].Contains("地面粗糙程度"))
                {
					List<string> result = new List<string>();
					string lineData = m_contentArray[i + 1];
					result.Add(lineData.Substring(lineData.LastIndexOf(':') + 1).Trim());
					lineData = m_contentArray[i + 2];
					result.Add(lineData.Substring(lineData.LastIndexOf(':') + 1).Trim());
					lineData = m_contentArray[i + 3];
					result.Add(lineData.Substring(lineData.LastIndexOf(':') + 1).Trim());
					lineData = m_contentArray[i];
					result.Add(lineData.Substring(lineData.LastIndexOf(':') + 1).Trim());
					m_SumInfo.WindInfo.LoadData(result);
				}

                if (m_contentArray[i].Contains("地震信息"))
                {
					for (int j = i+1; j <i+50; j++) {
						string lineData = m_contentArray[j];
						string[] dataArray = lineData.Split(new char[] { ':', '(', ' ' }, StringSplitOptions.RemoveEmptyEntries);			
						if (lineData.Contains("地震烈度:")) {
							m_SumInfo.SesmicInfo.Strength = double.Parse(dataArray[1]);
						} else if (lineData.Contains("场地类别:")) {
							m_SumInfo.SesmicInfo.SiteType = dataArray[1];
						} else if (lineData.Contains("设计地震分组:")) {
							m_SumInfo.SesmicInfo.SiteGroup = dataArray[1];
						} else if (lineData.Contains("特征周期:")) {
							m_SumInfo.SesmicInfo.Tg = double.Parse(dataArray[1]);
                        }
                        else if (lineData.Contains("框架的抗震等级:") && !lineData.Contains("钢"))
                        {
							m_SumInfo.SesmicInfo.FrameGrade =int.Parse(dataArray[1]);
						} else if (lineData.Contains("剪力墙的抗震等级:")) {
							m_SumInfo.SesmicInfo.WallGrade = int.Parse(dataArray[1]);
						} else if (lineData.Contains("是否考虑偶然偏心:")) {
							m_SumInfo.SesmicInfo.Consider_e = dataArray[1] == "是";
						} else if (lineData.Contains("是否考虑双向地震扭转效应:")) {
							m_SumInfo.SesmicInfo.Consider_doubleT = dataArray[1] == "是";
						}
					}
					break;
				}
			}
		}
        private void SapSesmicInfo()
        {
            int foundNumber = 0;
            for (int i = 0; i < m_contentArray.Length; i++)
            {
                string lineData = m_contentArray[i];
                if (lineData.Contains("是否考虑双向地震效应(1/0)"))
                {
                    string[] dataArray = lineData.Split(new char[] { '=', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries);                        
                    m_SumInfo.SesmicInfo.Consider_doubleT = dataArray[3]=="1";
                } 
                if (m_contentArray[i].Contains("地震反应谱分析信息"))
                {
                    for (int j = i + 1; j < i + 50; j++)
                    {
                        lineData = m_contentArray[j];
                        string[] dataArray = lineData.Split(new char[] { '=', '(',')',' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (lineData.Contains("地震烈度"))
                        {
                            m_SumInfo.SesmicInfo.Strength = double.Parse(dataArray[2]);
                        }
                        else if (lineData.Contains("场地类型"))
                        {
                            m_SumInfo.SesmicInfo.SiteType = dataArray[2];
                        }
                        else if (lineData.Contains("地震分组"))
                        {
                            m_SumInfo.SesmicInfo.SiteGroup = dataArray[4];
                        }
                        else if (lineData.Contains("特征周期"))
                        {
                            m_SumInfo.SesmicInfo.Tg = double.Parse(dataArray[3]);
                        }
                        else if (lineData.Contains("框架抗震等级"))
                        {
                            m_SumInfo.SesmicInfo.FrameGrade = int.Parse(dataArray[2]);
                        }
                        else if (lineData.Contains("剪力墙抗震等级"))
                        {
                            m_SumInfo.SesmicInfo.WallGrade = int.Parse(dataArray[2]);
                        }
                        else if (lineData.Contains("是否考虑偶然偏心地震"))
                        {
                            m_SumInfo.SesmicInfo.Consider_e = dataArray[3] != "0";
                        }                        
                    }
                    foundNumber++;
                }else if (m_contentArray[i].Contains("风荷载信息"))
                {
                    List<string> result = new List<string>();
                    string smooth="";
                    for (int j = i + 1; j < i + 50; j++)
                    {
                        lineData = m_contentArray[j];
                        if (lineData.Contains("W0=")&&result.Count==0)
                        {
                            result.Add(lineData.Substring(lineData.IndexOf('=') + 1).Trim());
                        }
                        else if (lineData.Contains("第 1 风荷载作用方向结构周期"))
                        {
                            result.Add(lineData.Substring(lineData.IndexOf('=') + 1).Trim());
                        }
                        else if (lineData.Contains("第 2 风荷载作用方向结构周期"))
                        {
                            result.Add(lineData.Substring(lineData.IndexOf('=') + 1).Trim());
                        }
                        else if (lineData.Contains("ISMOOTH="))
                        {
                           smooth=lineData.Substring(lineData.IndexOf('=') + 1).Trim();                           
                        }
                    }
                    result.Add(smooth);
                    m_SumInfo.WindInfo.LoadData(result);
                    foundNumber++;
                }
                if (foundNumber==2)
                {
                    break;
                }
            }
        }
        private void MidasSesmicInfo()
        {
            for (int i = 0; i < m_contentArray.Length; i++)
            {
                if (m_contentArray[i].Contains("风荷载信息"))
                {
                    List<string> result = new List<string>();
                    string lineData = m_contentArray[i + 2];
                    result.Add(lineData.Substring(lineData.LastIndexOf(')') + 1).Trim());
                    lineData = m_contentArray[i + 13];
                    result.Add(lineData.Substring(lineData.LastIndexOf(':') + 1).Trim());
                    lineData = m_contentArray[i + 14];
                    result.Add(lineData.Substring(lineData.LastIndexOf(':') + 1).Trim());
                    lineData = m_contentArray[i + 3];
                    result.Add(lineData.Substring(lineData.LastIndexOf(':') + 1).Trim());
                    m_SumInfo.WindInfo.LoadData(result);
                }
                if (m_contentArray[i].Contains("地震信息"))
                {
                    for (int j = i + 1; j < i + 50; j++)
                    {
                        string lineData = m_contentArray[j];
                        string[] dataArray = lineData.Split(new char[] { ':', '(', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (lineData.Contains("地震烈度:"))
                        {
                            m_SumInfo.SesmicInfo.Strength = double.Parse(dataArray[1]);
                        }
                        else if (lineData.Contains("场地类别:"))
                        {
                            m_SumInfo.SesmicInfo.SiteType = dataArray[1];
                        }
                        else if (lineData.Contains("设计地震分组:"))
                        {
                            m_SumInfo.SesmicInfo.SiteGroup = dataArray[1];
                        }
                        else if (lineData.Contains("特征周期(Tg):"))
                        {
                            m_SumInfo.SesmicInfo.Tg = double.Parse(dataArray[2]);
                        }
                        else if (lineData.Contains("框架抗震等级(内力调整):"))
                        {
                            switch (dataArray[2])
                            {
                                case "一级":
                                    m_SumInfo.SesmicInfo.FrameGrade=1;
                                    break;
                                case "二级":
                                    m_SumInfo.SesmicInfo.FrameGrade=2;
                                    break;
                                case "三级":
                                    m_SumInfo.SesmicInfo.FrameGrade = 3;
                                    break;
                                case "四级":
                                    m_SumInfo.SesmicInfo.FrameGrade = 4;
                                    break;
                                default:
                                    m_SumInfo.SesmicInfo.FrameGrade = 0;
                                    break;
                            }
                            
                        }
                        else if (lineData.Contains("剪力墙抗震等级(内力调整): "))
                        {
                            switch (dataArray[2])
                            {
                                case "一级":
                                    m_SumInfo.SesmicInfo.WallGrade = 1;
                                    break;
                                case "二级":
                                    m_SumInfo.SesmicInfo.WallGrade = 2;
                                    break;
                                case "三级":
                                    m_SumInfo.SesmicInfo.WallGrade = 3;
                                    break;
                                case "四级":
                                    m_SumInfo.SesmicInfo.WallGrade = 4;
                                    break;
                                default:
                                    m_SumInfo.SesmicInfo.WallGrade = 0;
                                    break;
                            }
                        }
                        else if (lineData.Contains("是否考虑偶然偏心:") && !lineData.Contains("最不利地震"))
                        {
                            m_SumInfo.SesmicInfo.Consider_e = dataArray[1] == "是";
                        }
                        else if (lineData.Contains("是否考虑双向地震扭转效应:"))
                        {
                            m_SumInfo.SesmicInfo.Consider_doubleT = dataArray[1] == "是";
                        }
                    }
                    break;
                }
            }
        }


        //刚度比信息
        public List<InfoEi> ParseEiInfo()
        {
            if (m_sourceName == PathFinder.PKPM)
            {
				PkpmEiInfo();
            }
            else if (m_sourceName == PathFinder.YJK)
            {
				YjkEiInfo();
            }
            else if (m_sourceName == PathFinder.SAP)
            {
                SapEiInfo();
            }
            else if (m_sourceName == PathFinder.MIDAS)
            {
                MidasEiInfo();
            }  
			return m_SumInfo.FloorEiInfo;            
        }
		private void PkpmEiInfo()
		{
            int i = 0;
            try
            {               
                for (; i < m_contentArray.Length; i++)
                {
                    if (m_contentArray[i].Contains("Floor No") && m_contentArray[i + 1].Contains("Xstif"))
                    {
                        List<string> dataList = new List<string>();
                        string lineData = m_contentArray[i];
                        int indexOfTower = lineData.IndexOf("Tower No");
                        if (indexOfTower < 0)
                        {
                            continue;
                        }
                        dataList.Add(lineData.Substring(11, indexOfTower - 11).Trim());
                        dataList.Add(lineData.Substring(indexOfTower + 10).Trim());
                        i = i + 4;

                        lineData = m_contentArray[i];
                        int indexOfRaty = lineData.IndexOf("Raty");
                        dataList.Add(lineData.Substring(8, indexOfRaty - 8).Trim());
                        dataList.Add(lineData.Substring(indexOfRaty + 6).Trim());
                        i = i + 1;

                        lineData = m_contentArray[i];
                        indexOfRaty = lineData.IndexOf("Raty1");
                        dataList.Add(lineData.Substring(8, indexOfRaty - 8).Trim());
                        dataList.Add(lineData.Substring(indexOfRaty + 6).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                        if (lineData.Contains("薄弱层地震剪力放大系数="))
                        {
                            dataList.Add(lineData.Substring(lineData.LastIndexOf('=') + 1).Trim());
                        }

                        i = i + 1;
                        lineData = m_contentArray[i];
                        indexOfRaty = lineData.IndexOf("Raty2");
                        string[] subStrArray = null;
                        if (indexOfRaty >= 0)
                        {
                            subStrArray = lineData.Split(new char[] { '=', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            dataList.Add(subStrArray[1]);
                            dataList.Add(subStrArray[3]);
                            if (subStrArray.Length == 6)
                            {
                                dataList.Add(subStrArray[5]);
                            }
                            i = i + 1;
                        }
                        lineData = m_contentArray[i];
                        if (lineData.Contains("薄弱层地震剪力放大系数="))
                        {
                            dataList.Add(lineData.Substring(lineData.LastIndexOf('=') + 1).Trim());
                        }

                        InfoEi EiInfo = new InfoEi();
                        EiInfo.LoadData(dataList);
                        m_SumInfo.FloorEiInfo.Add(EiInfo);
                    }
                } 
            }
            catch (Exception exp)
            {
                Debug.Write(exp.Message);
                throw new Exception(string.Format("解析Wmass文件出错，出错位置第{0}行",i));                
            }
            if (m_SumInfo.QianguFloor==0)
            {
                m_SumInfo.minRtx = m_SumInfo.FloorEiInfo[0];
                m_SumInfo.minRty = m_SumInfo.FloorEiInfo[0];
            }

            for (i = 0; i < m_SumInfo.FloorEiInfo.Count; i++)
            {
				InfoEi item = m_SumInfo.FloorEiInfo[i];
				if (item.FloorNo==m_SumInfo.QianguFloor) {
					m_SumInfo.minRtx = item;
                    m_SumInfo.minRty = item;
				}				
				if (m_SumInfo.minRtx1 == null || m_SumInfo.minRtx1.Ratx1 > item.Ratx1) {
					m_SumInfo.minRtx1 = item;
				}
				if (m_SumInfo.minRty1 == null || m_SumInfo.minRty1.Raty1 > item.Raty1) {
					m_SumInfo.minRty1 = item;
				}
				if (m_SumInfo.minRtx2 == null || m_SumInfo.minRtx2.Ratx2 > item.Ratx2) {
					m_SumInfo.minRtx2 = item;
				}
				if (m_SumInfo.minRty2 == null || m_SumInfo.minRty2.Raty2 > item.Raty2) {
					m_SumInfo.minRty2 = item;
				}
			}
		}
		private void YjkEiInfo()
		{
            int i = 0;
            try
            {  
			for (; i < m_contentArray.Length; i++) {
				if (m_contentArray[i].Contains("Floor No") && m_contentArray[i + 1].Contains("Xstif")) {
					List<string> dataList = new List<string>();
					string lineData = m_contentArray[i];
					int indexOfTower = lineData.IndexOf("Tower No");
					if (indexOfTower < 0) {
						continue;
					}
					dataList.Add(lineData.Substring(11, indexOfTower - 11).Trim());
					dataList.Add(lineData.Substring(indexOfTower + 10).Trim());
					int index= i + 4;
					lineData = m_contentArray[index];
					int indexOfRaty = lineData.IndexOf("Raty");
					dataList.Add(lineData.Substring(8, indexOfRaty - 8).Trim());
					dataList.Add(lineData.Substring(indexOfRaty + 6).Trim());

					index = index + 2;
					lineData = m_contentArray[index];
					indexOfRaty = lineData.IndexOf("Raty1");
					if (indexOfRaty >= 0) {
						dataList.Add(lineData.Substring(8, indexOfRaty - 8).Trim());
						dataList.Add(lineData.Substring(indexOfRaty + 6).Trim());
						index = index + 1;
					}
					lineData = m_contentArray[index];
					indexOfRaty = lineData.IndexOf("Raty2");
					if (indexOfRaty >= 0) {
						dataList.Add(lineData.Substring(8, indexOfRaty - 8).Trim());
						dataList.Add(lineData.Substring(indexOfRaty + 6).Trim());
						index = index + 1;
					}

					lineData = m_contentArray[i+5];
					dataList.Add(lineData.Substring(lineData.IndexOf('=') + 1).Trim());
					

					InfoEi EiInfo = new InfoEi();
					EiInfo.LoadData(dataList);
					m_SumInfo.FloorEiInfo.Add(EiInfo);
				}
			}
            }
            catch (Exception exp)
            {
                Debug.Write(exp.Message);
                throw new Exception(string.Format("解析Wmass文件出错，出错位置第{0}行", i));
            }
			for (i = 0; i < m_SumInfo.FloorEiInfo.Count; i++) {
				InfoEi item = m_SumInfo.FloorEiInfo[i];
                if (item.FloorNo == m_SumInfo.QianguFloor+1)
                {
                    m_SumInfo.minRtx = item;
                    m_SumInfo.minRty = item;
                }
				if (m_SumInfo.minRtx1 == null || m_SumInfo.minRtx1.Ratx1 > item.Ratx1) {
					m_SumInfo.minRtx1 = item;
				}
				if (m_SumInfo.minRty1 == null || m_SumInfo.minRty1.Raty1 > item.Raty1) {
					m_SumInfo.minRty1 = item;
				}
				if (m_SumInfo.minRtx2 == null || m_SumInfo.minRtx2.Ratx2 > item.Ratx2) {
					m_SumInfo.minRtx2 = item;
				}
				if (m_SumInfo.minRty2 == null || m_SumInfo.minRty2.Raty2 > item.Raty2) {
					m_SumInfo.minRty2 = item;
				}
			}
		}
        private void SapEiInfo()
        {
            for (int i = 0; i < m_contentArray.Length; i++)
            {
                if (m_contentArray[i].Contains("X刚度比") && m_contentArray[i].Contains("Y刚度比") &&
                    m_contentArray[i].Contains("X刚度比1") && m_contentArray[i].Contains("Y刚度比1") &&
                    m_contentArray[i].Contains("X刚度比2") && m_contentArray[i].Contains("Y刚度比2"))
                {
                    for (int j = i + 1; j < i + this.m_SumInfo.FloorCount + 1;j++ )
                    {
                        List<string> dataList = new List<string>();
                        string lineData = m_contentArray[j];
                        string[] dataArray = lineData.Split(new char[] {' ' }, StringSplitOptions.RemoveEmptyEntries);
                        dataList.Add(dataArray[0]);
                        dataList.Add(dataArray[1]);
                        dataList.Add(dataArray[4]);
                        dataList.Add(dataArray[5]);
                        dataList.Add(dataArray[6]);
                        dataList.Add(dataArray[7]);
                        dataList.Add(dataArray[8]);
                        dataList.Add(dataArray[9]);
                        dataList.Add(dataArray[11]);
                        InfoEi EiInfo = new InfoEi();
                        EiInfo.LoadData(dataList);
                        m_SumInfo.FloorEiInfo.Add(EiInfo);                    
                    } 
                }
            }
            if (m_SumInfo.QianguFloor == 0)
            {
                m_SumInfo.minRtx = m_SumInfo.FloorEiInfo[0];
                m_SumInfo.minRty = m_SumInfo.FloorEiInfo[0];
            }
            for (int i = 0; i < m_SumInfo.FloorEiInfo.Count; i++)
            {
                InfoEi item = m_SumInfo.FloorEiInfo[i];
                if (item.FloorNo == m_SumInfo.QianguFloor)
                {
                    m_SumInfo.minRtx = item;
                    m_SumInfo.minRty = item;
                }				
                if (m_SumInfo.minRtx1 == null || m_SumInfo.minRtx1.Ratx1 > item.Ratx1)
                {
                    m_SumInfo.minRtx1 = item;
                }
                if (m_SumInfo.minRty1 == null || m_SumInfo.minRty1.Raty1 > item.Raty1)
                {
                    m_SumInfo.minRty1 = item;
                }
                if (m_SumInfo.minRtx2 == null || m_SumInfo.minRtx2.Ratx2 > item.Ratx2)
                {
                    m_SumInfo.minRtx2 = item;
                }
                if (m_SumInfo.minRty2 == null || m_SumInfo.minRty2.Raty2 > item.Raty2)
                {
                    m_SumInfo.minRty2 = item;
                }
            }
        }
        private void MidasEiInfo()
        {
            Dictionary<int, InfoEi> mapFloorToEi = new Dictionary<int, InfoEi>();
            for (int i = 0; i < m_contentArray.Length; i++)
            {
                if (m_contentArray[i].Contains("RS_0"))
                {
                    i = i + 2;
                    if (m_contentArray[i].Contains("Rat") && m_contentArray[i].Contains("Rat1") &&
                        m_contentArray[i].Contains("Rat2") && m_contentArray[i].Contains("薄弱层"))
                    {
                        for (int j = i + 2; j < i + this.m_SumInfo.FloorCount + 2; j++)
                        {
                            List<string> dataList = new List<string>();
                            string lineData = m_contentArray[j];
                            string[] dataArray = lineData.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                           
                            InfoEi EiInfo = new InfoEi();
                            string floorNoStr = dataArray[1].Trim('F');
                            if (floorNoStr.Contains("B"))
                            {
                                EiInfo.FloorNo =int.Parse(floorNoStr.Trim('B')) * -1;
                            }
                            else
                            {
                                EiInfo.FloorNo = int.Parse(floorNoStr.Trim('B'));
                            }
                            if (mapFloorToEi.ContainsKey(EiInfo.FloorNo))
                            {
                                EiInfo = mapFloorToEi[EiInfo.FloorNo];
                            }
                            else
                            {
                                mapFloorToEi.Add(EiInfo.FloorNo, EiInfo);
                                m_SumInfo.FloorEiInfo.Add(EiInfo);
                            }
                            EiInfo.Ratx = double.Parse(dataArray[4]);
                            if (!double.TryParse(dataArray[5],out EiInfo.Ratx1))
                            {
                                EiInfo.Ratx1=1;
                            }
                            if (!double.TryParse(dataArray[6], out EiInfo.Ratx2))
                            {
                                EiInfo.Ratx2 = 1;
                            }                           
                        }
                    }
                }
                else if (m_contentArray[i].Contains("RS_90"))
                {
                    i = i + 2;
                    if (m_contentArray[i].Contains("Rat") && m_contentArray[i].Contains("Rat1") &&
                        m_contentArray[i].Contains("Rat2") && m_contentArray[i].Contains("薄弱层"))
                    {
                        for (int j = i + 2; j < i + this.m_SumInfo.FloorCount + 2; j++)
                        {
                            List<string> dataList = new List<string>();
                            string lineData = m_contentArray[j];
                            string[] dataArray = lineData.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            
                            InfoEi EiInfo = new InfoEi();
                            string floorNoStr = dataArray[1].Trim('F');
                            if (floorNoStr.Contains("B"))
                            {
                                EiInfo.FloorNo = int.Parse(floorNoStr.Trim('B')) * -1;
                            }
                            else
                            {
                                EiInfo.FloorNo = int.Parse(floorNoStr.Trim('B'));
                            }
                            if (mapFloorToEi.ContainsKey(EiInfo.FloorNo))
                            {
                                EiInfo = mapFloorToEi[EiInfo.FloorNo];
                            }
                            else
                            {
                                mapFloorToEi.Add(EiInfo.FloorNo, EiInfo);
                                m_SumInfo.FloorEiInfo.Add(EiInfo);
                            }                           
                            EiInfo.Raty = double.Parse(dataArray[4]);
                            if (!double.TryParse(dataArray[5], out EiInfo.Raty1))
                            {
                                EiInfo.Raty1 = 1;
                            }
                            if (!double.TryParse(dataArray[6], out EiInfo.Raty2))
                            {
                                EiInfo.Raty2 = 1;
                            }          
                           
                        }
                    }
                }
            }
            
            if (m_SumInfo.QianguFloor == 0)
            {
                m_SumInfo.minRtx = mapFloorToEi[1];
                m_SumInfo.minRty = mapFloorToEi[1];
            }
            for (int i = 0; i < m_SumInfo.FloorEiInfo.Count; i++)
            {
                InfoEi item = m_SumInfo.FloorEiInfo[i];
                if (item.FloorNo == m_SumInfo.QianguFloor)
                {
                    m_SumInfo.minRtx = item;
                    m_SumInfo.minRty = item;
                }
                if (m_SumInfo.minRtx1 == null || m_SumInfo.minRtx1.Ratx1 > item.Ratx1)
                {
                    m_SumInfo.minRtx1 = item;
                }
                if (m_SumInfo.minRty1 == null || m_SumInfo.minRty1.Raty1 > item.Raty1)
                {
                    m_SumInfo.minRty1 = item;
                }
                if (m_SumInfo.minRtx2 == null || m_SumInfo.minRtx2.Ratx2 > item.Ratx2)
                {
                    m_SumInfo.minRtx2 = item;
                }
                if (m_SumInfo.minRty2 == null || m_SumInfo.minRty2.Raty2 > item.Raty2)
                {
                    m_SumInfo.minRty2 = item;
                }
            }
        }


        //楼层质量比
        public List<InfoMass> ParseMassInfo()
        {
            if (m_sourceName == PathFinder.PKPM)
            {
				PkpmMassInfo();
            }
            else if (m_sourceName == PathFinder.YJK)
            {
				YjkMassInfo();
			}
            else if (m_sourceName == PathFinder.SAP)
            {
                SapMassInfo();
            }
            else if (m_sourceName == PathFinder.MIDAS)
            {
                MidasMassInfo();
            } 
            return m_SumInfo.FloorMassInfo;
        }
		private void PkpmMassInfo()
		{
			for (int i = 0; i < m_contentArray.Length; i++) {
				if (m_contentArray[i].Contains("各层的质量、质心坐标信息")) {
					i = i + 5;
					string lastFloorNo=string.Empty;
					for (int j = 0; j < m_SumInfo.FloorElemMatInfo.Count; j++) {
						List<string> dataList = new List<string>();
						int rowIndex = i + j;
						string lineData = m_contentArray[rowIndex];		
						int indexOfBrack=lineData.IndexOf('(');
						if (indexOfBrack>=0)
						{
							lineData=lineData.Remove(indexOfBrack);
						}						
						string[] dataArray = lineData.Split(new char[]{' ','('},StringSplitOptions.RemoveEmptyEntries);
						if (dataArray.Length==9) {
							lastFloorNo = dataArray[0];
							dataList.Add(dataArray[0]);
							dataList.Add(dataArray[1]);
							dataList.Add(dataArray[8]);
						} else {
							dataList.Add(lastFloorNo);
							dataList.Add(dataArray[0]);
							dataList.Add(dataArray[7]);
						}						
						InfoMass massInfo = new InfoMass();
						massInfo.LoadData(dataList);
						m_SumInfo.FloorMassInfo.Add(massInfo);
					}

				}
			}

			if (m_SumInfo.FloorMassInfo.Count > 0) {
				m_SumInfo.maxMassInfo = m_SumInfo.FloorMassInfo[0];
				m_SumInfo.minMassInfo = m_SumInfo.FloorMassInfo[0];
				for (int i = 0; i < m_SumInfo.FloorMassInfo.Count; i++) {
					if (m_SumInfo.maxMassInfo.MassRate < m_SumInfo.FloorMassInfo[i].MassRate) {
						m_SumInfo.maxMassInfo = m_SumInfo.FloorMassInfo[i];
					}
					if (m_SumInfo.minMassInfo.MassRate > m_SumInfo.FloorMassInfo[i].MassRate) {
						m_SumInfo.minMassInfo = m_SumInfo.FloorMassInfo[i];
					}
				}
			}

		}
		private void YjkMassInfo()
		{
			for (int i = 0; i < m_contentArray.Length; i++) {
				if (m_contentArray[i].Contains("各层质量、质心坐标")) {
					i = i + 5;
					for (int j = 0; j < m_SumInfo.FloorElemMatInfo.Count; j++) {
						List<string> dataList = new List<string>();
						int rowIndex = i + j;
						string lineData = m_contentArray[rowIndex];						
						string[] dataArray = lineData.Split(new char[]{' '},StringSplitOptions.RemoveEmptyEntries);
						dataList.Add(dataArray[0]);
						dataList.Add(dataArray[1]);
						dataList.Add(dataArray[9]);
						InfoMass massInfo = new InfoMass();
						massInfo.LoadData(dataList);
						m_SumInfo.FloorMassInfo.Add(massInfo);
					}

				}
			}

			if (m_SumInfo.FloorMassInfo.Count > 0) {
				m_SumInfo.maxMassInfo = m_SumInfo.FloorMassInfo[0];
				m_SumInfo.minMassInfo = m_SumInfo.FloorMassInfo[0];
				for (int i = 0; i < m_SumInfo.FloorMassInfo.Count; i++) {
					if (m_SumInfo.maxMassInfo.MassRate < m_SumInfo.FloorMassInfo[i].MassRate) {
						m_SumInfo.maxMassInfo = m_SumInfo.FloorMassInfo[i];
					}
					if (m_SumInfo.minMassInfo.MassRate > m_SumInfo.FloorMassInfo[i].MassRate) {
						m_SumInfo.minMassInfo = m_SumInfo.FloorMassInfo[i];
					}
				}
			}

		}
        private void SapMassInfo()
        {
            for (int i = 0; i < m_contentArray.Length; i++)
            {
                if (m_contentArray[i].Contains("本层质量/下层质量"))
                {
                    for (int j = i+1; j <= i+m_SumInfo.FloorCount; j++)
                    {
                        List<string> dataList = new List<string>();                       
                        string lineData = m_contentArray[j];
                        string[] dataArray = lineData.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        dataList.Add(dataArray[0]);
                        dataList.Add("1");
                        dataList.Add(dataArray[2]);
                        InfoMass massInfo = new InfoMass();
                        massInfo.LoadData(dataList);
                        m_SumInfo.FloorMassInfo.Add(massInfo);
                    }

                }
            }

            if (m_SumInfo.FloorMassInfo.Count > 0)
            {
                m_SumInfo.maxMassInfo = m_SumInfo.FloorMassInfo[0];
                m_SumInfo.minMassInfo = m_SumInfo.FloorMassInfo[0];
                for (int i = 0; i < m_SumInfo.FloorMassInfo.Count; i++)
                {
                    if (m_SumInfo.maxMassInfo.MassRate < m_SumInfo.FloorMassInfo[i].MassRate)
                    {
                        m_SumInfo.maxMassInfo = m_SumInfo.FloorMassInfo[i];
                    }
                    if (m_SumInfo.minMassInfo.MassRate > m_SumInfo.FloorMassInfo[i].MassRate)
                    {
                        m_SumInfo.minMassInfo = m_SumInfo.FloorMassInfo[i];
                    }
                }
            }

        }
        private void MidasMassInfo()
        {
            for (int i = 0; i < m_contentArray.Length; i++)
            {
                if (m_contentArray[i].Contains("各层的质量,质心坐标信息"))
                {
                    i = i + 5;
                    for (int j = 0; j < m_SumInfo.FloorElemMatInfo.Count; j++)
                    {                       
                        int rowIndex = i + j;
                        string lineData = m_contentArray[rowIndex];
                        string[] dataArray = lineData.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        string floorNoStr = dataArray[1].Trim('F');
                        int floorNo;
                        if (floorNoStr.Contains("B"))
                        {
                            floorNo = int.Parse(floorNoStr.Trim('B')) * -1;
                        }
                        else
                        {
                            floorNo = int.Parse(floorNoStr.Trim('B'));
                        }                       
                        InfoMass massInfo = new InfoMass();
                        massInfo.FoorNo = floorNo;
                        massInfo.TowerNo = 1;
                        if (j == m_SumInfo.FloorElemMatInfo.Count-1)
                        {
                            massInfo.MassRate = 1;
                        }
                        else
                        {
                            string[] dataArray2 = m_contentArray[rowIndex + 1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            massInfo.MassRate = (double.Parse(dataArray[5]) + double.Parse(dataArray[6])) / (double.Parse(dataArray2[5]) + double.Parse(dataArray2[6]));                       
                        }
                        m_SumInfo.FloorMassInfo.Add(massInfo);
                    }

                }
            }

            if (m_SumInfo.FloorMassInfo.Count > 0)
            {
                m_SumInfo.maxMassInfo = m_SumInfo.FloorMassInfo[0];
                m_SumInfo.minMassInfo = m_SumInfo.FloorMassInfo[0];
                for (int i = 0; i < m_SumInfo.FloorMassInfo.Count; i++)
                {
                    if (m_SumInfo.maxMassInfo.MassRate < m_SumInfo.FloorMassInfo[i].MassRate)
                    {
                        m_SumInfo.maxMassInfo = m_SumInfo.FloorMassInfo[i];
                    }
                    if (m_SumInfo.minMassInfo.MassRate > m_SumInfo.FloorMassInfo[i].MassRate)
                    {
                        m_SumInfo.minMassInfo = m_SumInfo.FloorMassInfo[i];
                    }
                }
            }

        }

        //楼层抗剪承载力比 刚重比
		public List<InfoRv> ParseRvInfo()
		{
            if (m_sourceName == PathFinder.PKPM)
            {
				PkpmRvInfo();
            }
            else if (m_sourceName == PathFinder.YJK)
            {
				YjkRvInfo();
            }
            else if (m_sourceName == PathFinder.SAP)
            {
                SapRvInfo();
            }
            else if (m_sourceName == PathFinder.MIDAS)
            {
                MidasRvInfo();
            }   
			
			return m_SumInfo.FloorRvInfo;
		}
		private void PkpmRvInfo()
		{
			for (int i = 0; i < m_contentArray.Length; i++) {
				if (m_contentArray[i].Contains("X向刚重比")) {
					string dataVal = m_contentArray[i].Substring(m_contentArray[i].IndexOf('=') + 1).Trim();
					m_SumInfo.EGFactor_X = double.Parse(dataVal);
				} else if (m_contentArray[i].Contains("Y向刚重比")) {
					string dataVal = m_contentArray[i].Substring(m_contentArray[i].IndexOf('=') + 1).Trim();
					m_SumInfo.EGFactor_Y = double.Parse(dataVal);
                }
                else if (m_contentArray[i].Contains("层号") && m_contentArray[i].Contains("X刚重比"))
				{
                    i = i + 1;
                    string[] dataArray = m_contentArray[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    m_SumInfo.EGFactor_X = m_SumInfo.EGFactor_Y = 1000;
                    while (dataArray.Length>5)
                    {
                        double xFactor = double.Parse(dataArray[dataArray.Length - 2]);
                        double yFactor = double.Parse(dataArray[dataArray.Length - 1]);
                        if (xFactor<m_SumInfo.EGFactor_X)
                        {
                            m_SumInfo.EGFactor_X = xFactor;
                        }
                        if (yFactor<m_SumInfo.EGFactor_Y)
                        {
                            m_SumInfo.EGFactor_Y = yFactor;
                        }
                        i++;
                        dataArray = m_contentArray[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);                    
                    }
				}

				if (m_contentArray[i].Contains("楼层抗剪承载力")) {
					i = i + 8;
					for (int j = 0; j < m_SumInfo.FloorElemMatInfo.Count; j++) {
						List<string> dataList = new List<string>();
						int rowIndex = i + j;
						string lineData = m_contentArray[rowIndex];
						string[] dataArray = lineData.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						List<string> filtedResult = new List<string>();
						for (int k = 0; k < dataArray.Length; k++) {
							if (dataArray[k] != string.Empty) {
								filtedResult.Add(dataArray[k]);
							}
						}
						dataList.Add(filtedResult[0]);
						dataList.Add(filtedResult[1]);
						dataList.Add(filtedResult[2]);
						dataList.Add(filtedResult[3]);
						dataList.Add(filtedResult[filtedResult.Count - 2].Trim());
						dataList.Add(filtedResult[filtedResult.Count - 1].Trim());

						InfoRv rvInfo = new InfoRv();
						rvInfo.LoadData(dataList);
						m_SumInfo.FloorRvInfo.Add(rvInfo);
					}
					break;
				}
			}
			if (m_SumInfo.FloorRvInfo.Count > 0) {
				m_SumInfo.minRvInfo_X = m_SumInfo.FloorRvInfo[0];
				m_SumInfo.minRvInfo_Y = m_SumInfo.FloorRvInfo[0];
				for (int i = 0; i < m_SumInfo.FloorRvInfo.Count; i++) {
					if (m_SumInfo.minRvInfo_X.Ratio_X > m_SumInfo.FloorRvInfo[i].Ratio_X) {
						m_SumInfo.minRvInfo_X = m_SumInfo.FloorRvInfo[i];
					}
					if (m_SumInfo.minRvInfo_Y.Ratio_Y > m_SumInfo.FloorRvInfo[i].Ratio_Y) {
						m_SumInfo.minRvInfo_Y = m_SumInfo.FloorRvInfo[i];
					}
				}
			} else {
				throw new Exception("楼层抗剪承载力数据不存在，请进行稳定性计算");
			}
		}
		private void YjkRvInfo()
		{
			for (int i = 0; i < m_contentArray.Length; i++) {
				if (m_contentArray[i].Contains("X向刚重比")) {
					string dataVal = m_contentArray[i].Substring(m_contentArray[i].IndexOf('=') + 1).Trim();
					m_SumInfo.EGFactor_X = double.Parse(dataVal);
				} else if (m_contentArray[i].Contains("Y向刚重比")) {
					string dataVal = m_contentArray[i].Substring(m_contentArray[i].IndexOf('=') + 1).Trim();
					m_SumInfo.EGFactor_Y = double.Parse(dataVal);
                }
                else if (m_contentArray[i].Contains("层号") && m_contentArray[i].Contains("X刚重比"))
                {
                    i = i + 1;
                    string[] dataArray = m_contentArray[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    m_SumInfo.EGFactor_X = m_SumInfo.EGFactor_Y = 1000;
                    while (dataArray.Length > 5)
                    {
                        double xFactor = double.Parse(dataArray[dataArray.Length - 2]);
                        double yFactor = double.Parse(dataArray[dataArray.Length - 1]);
                        if (xFactor < m_SumInfo.EGFactor_X)
                        {
                            m_SumInfo.EGFactor_X = xFactor;
                        }
                        if (yFactor < m_SumInfo.EGFactor_Y)
                        {
                            m_SumInfo.EGFactor_Y = yFactor;
                        }
                        i++;
                        dataArray = m_contentArray[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                }

				if (m_contentArray[i].Contains("楼层抗剪承载力")) {
					i = i + 6;
					for (int j = 0; j < m_SumInfo.FloorElemMatInfo.Count; j++) {
						List<string> dataList = new List<string>();
						int rowIndex = i + j;
						string lineData = m_contentArray[rowIndex];
						string[] dataArray = lineData.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						dataList.Add(dataArray[0]);
						dataList.Add(dataArray[1]);
						dataList.Add(dataArray[2]);
						dataList.Add(dataArray[3]);
						dataList.Add(dataArray[4]);
						dataList.Add(dataArray[5]);

						InfoRv rvInfo = new InfoRv();
						rvInfo.LoadData(dataList);
						m_SumInfo.FloorRvInfo.Add(rvInfo);
					}
					break;
				}
			}
			if (m_SumInfo.FloorRvInfo.Count > 0) {
				m_SumInfo.minRvInfo_X = m_SumInfo.FloorRvInfo[0];
				m_SumInfo.minRvInfo_Y = m_SumInfo.FloorRvInfo[0];
				for (int i = 0; i < m_SumInfo.FloorRvInfo.Count; i++) {
					if (m_SumInfo.minRvInfo_X.Ratio_X > m_SumInfo.FloorRvInfo[i].Ratio_X) {
						m_SumInfo.minRvInfo_X = m_SumInfo.FloorRvInfo[i];
					}
					if (m_SumInfo.minRvInfo_Y.Ratio_Y > m_SumInfo.FloorRvInfo[i].Ratio_Y) {
						m_SumInfo.minRvInfo_Y = m_SumInfo.FloorRvInfo[i];
					}
				}
			} else {
                throw new Exception("楼层抗剪承载力数据不存在，请进行稳定性计算");
			}
		}
        private void SapRvInfo()
        {
            for (int i = 0; i < m_contentArray.Length; i++)
            {
                if (m_contentArray[i].Contains("X向刚重比 EJd/GH**2="))
                {
                    string dataVal = m_contentArray[i].Substring(m_contentArray[i].IndexOf('=') + 1).Trim();
                    m_SumInfo.EGFactor_X = double.Parse(dataVal);
                }
                else if (m_contentArray[i].Contains("Y向刚重比 EJd/GH**2="))
                {
                    string dataVal = m_contentArray[i].Substring(m_contentArray[i].IndexOf('=') + 1).Trim();
                    m_SumInfo.EGFactor_Y = double.Parse(dataVal);
                }

                if (m_contentArray[i].Contains("VX/VXP"))
                {
                    for (int j = i+1; j <=i+m_SumInfo.FloorCount; j++)
                    {
                        List<string> dataList = new List<string>();                       
                        string lineData = m_contentArray[j];
                        string[] dataArray = lineData.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        dataList.Add(dataArray[0]);
                        dataList.Add(dataArray[1]);
                        dataList.Add(dataArray[2]);
                        dataList.Add(dataArray[3]);
                        dataList.Add(dataArray[4]);
                        dataList.Add(dataArray[5]);
                        InfoRv rvInfo = new InfoRv();
                        rvInfo.LoadData(dataList);
                        m_SumInfo.FloorRvInfo.Add(rvInfo);
                    }                   
                }
            }
            if (m_SumInfo.FloorRvInfo.Count > 0)
            {
                m_SumInfo.minRvInfo_X = m_SumInfo.FloorRvInfo[0];
                m_SumInfo.minRvInfo_Y = m_SumInfo.FloorRvInfo[0];
                for (int i = 0; i < m_SumInfo.FloorRvInfo.Count; i++)
                {
                    if (m_SumInfo.minRvInfo_X.Ratio_X > m_SumInfo.FloorRvInfo[i].Ratio_X)
                    {
                        m_SumInfo.minRvInfo_X = m_SumInfo.FloorRvInfo[i];
                    }
                    if (m_SumInfo.minRvInfo_Y.Ratio_Y > m_SumInfo.FloorRvInfo[i].Ratio_Y)
                    {
                        m_SumInfo.minRvInfo_Y = m_SumInfo.FloorRvInfo[i];
                    }
                }
            }
            else
            {
                throw new Exception("楼层抗剪承载力数据不存在，请进行稳定性计算");
            }
        }
        private void MidasRvInfo()
        {
            Dictionary<int, InfoRv> mapFloorToRv = new Dictionary<int, InfoRv>();            
            for (int i = 0; i < m_contentArray.Length; i++)
            {
                if (m_contentArray[i].Contains("SumG") && m_contentArray[i].Contains("刚重比"))
                {
                    m_SumInfo.EGFactor_X = m_SumInfo.EGFactor_Y = 1000;
                    i = i + 1;
                    for (int j = 0; j < 6; j++)
			        {
			            i=i+1;
                        string[] dataArray = m_contentArray[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        double factor = double.Parse(dataArray[5]);
                        if (dataArray[0].Contains("90"))
                        {
                            if (factor < m_SumInfo.EGFactor_Y)
                            {
                                m_SumInfo.EGFactor_Y= factor;
                            }
                        }else
                        {
                            if (factor < m_SumInfo.EGFactor_X)
                            {
                                m_SumInfo.EGFactor_X= factor;
                            }
                        }

			        }
                }
                
                if (m_contentArray[i].Contains("工况") && m_contentArray[i + 2].Contains("V") && m_contentArray[i + 2].Contains("Rat"))
                {
                    string workNo = m_contentArray[i];
                    i = i + 4;
                    for (int j = 0; j < m_SumInfo.FloorElemMatInfo.Count; j++)
                    {
                        List<string> dataList = new List<string>();
                        int rowIndex = i + j;
                        string lineData = m_contentArray[rowIndex];
                        string[] dataArray = lineData.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        InfoRv rvInfo = new InfoRv();
                        string floorNoStr = dataArray[1].Trim('F');
                        if (floorNoStr.Contains("B"))
                        {
                            rvInfo.FoorNo = int.Parse(floorNoStr.Trim('B')) * -1;
                        }
                        else
                        {
                            rvInfo.FoorNo = int.Parse(floorNoStr.Trim('B'));
                        }
                        if (mapFloorToRv.ContainsKey(rvInfo.FoorNo))
                        {
                            rvInfo = mapFloorToRv[rvInfo.FoorNo];
                            
                        }
                        else
                        {
                            mapFloorToRv.Add(rvInfo.FoorNo, rvInfo);
                            m_SumInfo.FloorRvInfo.Add(rvInfo);
                        }

                        double Rv = double.Parse(dataArray[2]);
                        double Rat;
                        if (!double.TryParse(dataArray[3], out Rat))
                        {
                            Rat = 1000;
                        }
                        if (workNo.Contains("90"))
                        {
                            if (rvInfo.Ratio_Y<0.1||rvInfo.Ratio_Y > Rat)
                            {
                                rvInfo.Ratio_Y = Rat;
                            }
                            if (rvInfo.Rv_Y<0.1||rvInfo.Rv_Y > Rv)
                            {
                                rvInfo.Rv_Y = Rv;
                            }
                        }
                        else
                        {
                            if (rvInfo.Ratio_X < 0.1 || rvInfo.Ratio_X > Rat)
                            {
                                rvInfo.Ratio_X = Rat;
                            }
                            if (rvInfo.Rv_X < 0.1 || rvInfo.Rv_X > Rv)
                            {
                                rvInfo.Rv_X = Rv;
                            }
                        }
                       
                    }                    
                }
            }

            if (m_SumInfo.FloorRvInfo.Count > 0)
            {
                m_SumInfo.minRvInfo_X = m_SumInfo.FloorRvInfo[0];
                m_SumInfo.minRvInfo_Y = m_SumInfo.FloorRvInfo[0];
                for (int i = 0; i < m_SumInfo.FloorRvInfo.Count; i++)
                {
                    if (m_SumInfo.minRvInfo_X.Ratio_X > m_SumInfo.FloorRvInfo[i].Ratio_X)
                    {
                        m_SumInfo.minRvInfo_X = m_SumInfo.FloorRvInfo[i];
                    }
                    if (m_SumInfo.minRvInfo_Y.Ratio_Y > m_SumInfo.FloorRvInfo[i].Ratio_Y)
                    {
                        m_SumInfo.minRvInfo_Y = m_SumInfo.FloorRvInfo[i];
                    }
                }
            }
            else
            {
                throw new Exception("楼层抗剪承载力数据不存在，请进行稳定性计算");
            }
        }

        //周期比
        public void ParseTInfo( ParserWzq wzqParser )
        {
            m_SumInfo.VibrationT = wzqParser.ParseTInfo(m_SumInfo.FloorElemMatInfo.Count,m_sourceName,m_SumInfo);
            if (m_SumInfo.VibrationT.Count>0)
            {
                m_SumInfo.FirstTq = m_SumInfo.VibrationT[m_SumInfo.VibrationT.Count - 1];
                m_SumInfo.FirstTt = m_SumInfo.VibrationT[m_SumInfo.VibrationT.Count - 1];                
            }            
            foreach (var item in m_SumInfo.VibrationT)
            {
                if (item.MoveFactor > 0.5 && m_SumInfo.FirstTq.T < item.T)
                {
                    m_SumInfo.FirstTq = item;
                }
                if (item.TwistFactor > 0.5 && m_SumInfo.FirstTt.T < item.T)
                {
                    m_SumInfo.FirstTt = item;
                }
            }
            m_SumInfo.EffectiveMassFactor_X = wzqParser.EffectiveMassFactor_X;
            m_SumInfo.EffectiveMassFactor_Y = wzqParser.EffectiveMassFactor_Y;
            m_SumInfo.VgList = wzqParser.VgList;
            m_SumInfo.minVg_X = wzqParser.minVg_X;
            m_SumInfo.minVg_Y = wzqParser.minVg_Y;
        }

        //位移比
        public void ParseDispInfo(ParserWdisp dispParser)
        {
            m_SumInfo.maxDispX = dispParser.maxX;
            m_SumInfo.maxDispX_floor = dispParser.maxX_floor;
            m_SumInfo.maxDispY = dispParser.maxY;
            m_SumInfo.maxDispY_floor = dispParser.maxY_floor;
            m_SumInfo.maxX_FloorDispAngle = dispParser.maxX_FloorDispAngle;
            m_SumInfo.maxY_FloorDispAngle = dispParser.maxY_FloorDispAngle;
            m_SumInfo.DispInfoList = dispParser.dispList;
        }

        //周期与剪重比
        private void SapTInfo()
        {
            for (int i = 0; i < m_contentArray.Length; i++)
            {
                if (m_contentArray[i].Contains("扭振成份") && m_contentArray[i].Contains("总侧振成份"))
                {  
                     m_SumInfo.VibrationT=new List<InfoT>();
                    for (int j = i+1; ; j++)
                    {
                        string lineData = m_contentArray[j];
                        string[] dataArray = lineData.Trim().Split(new char[] { ' ', '(', ')', '+' }, StringSplitOptions.RemoveEmptyEntries);
                        if (dataArray.Length == 0)
                        {
                            break;
                        }
                        InfoT tInfo = new InfoT();
                        dataArray[3] = dataArray[7];
                        dataArray[7] = dataArray[4];
                        dataArray[4] = dataArray[5];
                        dataArray[5] = dataArray[6];
                        dataArray[6] = dataArray[7];
                        tInfo.LoadData(dataArray);
                         m_SumInfo.VibrationT.Add(tInfo);
                    }
                }
                else if (m_contentArray[i].Contains("有效质量系数为")&&m_contentArray[i].Contains("EX"))
                {
                    string lineData = m_contentArray[i];
                    string[] dataArray = lineData.Trim().Split(new char[] { '为', ','}, StringSplitOptions.RemoveEmptyEntries);
                    m_SumInfo.EffectiveMassFactor_X = double.Parse(dataArray[1])*100;
                }
                else if (m_contentArray[i].Contains("有效质量系数为")&&m_contentArray[i].Contains("EY"))
                {
                    string lineData = m_contentArray[i];
                    string[] dataArray = lineData.Trim().Split(new char[] { '为', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    m_SumInfo.EffectiveMassFactor_Y = double.Parse(dataArray[1])*100;
                }

                else if (m_contentArray[i].Contains("轴力") && m_contentArray[i].Contains("EX"))
                {
                    m_SumInfo.VgList=new List<InfoVG>();//轴重比
                    for (int j = i+1; j <=i+m_SumInfo.FloorCount; j++)
                    {                       
                        string lineData = m_contentArray[j];
                        string[] dataArray = lineData.Split(new char[] { ' ', '(', ')', '%' }, StringSplitOptions.RemoveEmptyEntries);
                        InfoVG vgInfo = new InfoVG();
                        vgInfo.LoadData(dataArray);                       
                        m_SumInfo.VgList.Add(vgInfo);
                    }
                }                
            }
            int startFloor=1;
            if (m_sourceName == PathFinder.YJK || m_sourceName == PathFinder.SAP)
            {
                startFloor=m_SumInfo.QianguFloor+1;
            }else if(m_SumInfo.QianguFloor>0)
            {
                startFloor = m_SumInfo.QianguFloor;
            }
            
             m_SumInfo.minVg_X =  m_SumInfo.minVg_Y = m_SumInfo.VgList[startFloor-1];
            for (int i = startFloor; i < m_SumInfo.VgList.Count; i++)
            {
                if ( m_SumInfo.minVg_X.Vx_Ratio > m_SumInfo.VgList[i].Vx_Ratio)
                {
                     m_SumInfo.minVg_X = m_SumInfo.VgList[i];
                }
                if ( m_SumInfo.minVg_Y.Vy_Ratio > m_SumInfo.VgList[i].Vy_Ratio)
                {
                     m_SumInfo.minVg_Y = m_SumInfo.VgList[i];
                }
            }

            m_SumInfo.FirstTq = m_SumInfo.VibrationT[m_SumInfo.VibrationT.Count - 1];
            m_SumInfo.FirstTt = m_SumInfo.VibrationT[m_SumInfo.VibrationT.Count - 1];
            foreach (var item in m_SumInfo.VibrationT)
            {
                if (item.MoveFactor > 0.5 && m_SumInfo.FirstTq.T < item.T)
                {
                    m_SumInfo.FirstTq = item;
                }
                if (item.TwistFactor > 0.5 && m_SumInfo.FirstTt.T < item.T)
                {
                    m_SumInfo.FirstTt = item;
                }
            }
        }
        private void SapDispInfo()
        {
            m_SumInfo.DispInfoList = new List<InfoDisp>();
            for (int i = 0; i < m_contentArray.Length; i++)
            {
                string lineData = m_contentArray[i].Trim();
                if (lineData.Contains("指定水平力引起的楼层位移") && lineData.Contains("X") && lineData[0] == '(')
                {
                    InfoDisp dispInfo = new InfoDisp();                    
                    string[] dataArray = lineData.Trim().Split(new char[] { ' ', '(', ')', '+' }, StringSplitOptions.RemoveEmptyEntries);
                    dispInfo.WorkNo=int.Parse(dataArray[0]);
                    dispInfo.Description = lineData;
                    m_SumInfo.DispInfoList.Add(dispInfo);
                    int hitCount = 0;
                    while (i < m_contentArray.Length)
                    {
                        i++;
                        if (hitCount == 3)
                        {
                            break;
                        }
                        if (m_contentArray[i].Contains("本工况下全楼最大位移比"))
                        {
                            string line = m_contentArray[i];
                            string[] subArray = line.Split(new char[] { '(', ')', '=' }, StringSplitOptions.RemoveEmptyEntries);
                            dispInfo.XFactor = double.Parse(subArray[1]);
                            dispInfo.XFactor_description = subArray[2];
                            dispInfo.XFactor_FullDescription = line;
                            hitCount++;
                        }
                        else if (m_contentArray[i].Contains("本工况下全楼最大层间位移角"))
                        {
                            string line = m_contentArray[i];
                            string[] subArray = line.Split(new char[] { '(', '/', ')' }, StringSplitOptions.RemoveEmptyEntries);
                            dispInfo.XMaxDisp_floorAngle = int.Parse(subArray[1]);
                            dispInfo.XMaxDisp_floorAngle_description = subArray[2];
                            dispInfo.XMaxDisp_floorAngle_FullDescription = line;
                            hitCount++;
                        }
                        else  if (m_contentArray[i].Contains("本工况下全楼最大层间位移比"))
                        {
                            string line = m_contentArray[i];
                            string[] subArray = line.Split(new char[] { '(', ')', '=' }, StringSplitOptions.RemoveEmptyEntries);
                            dispInfo.XFactor_floor = double.Parse(subArray[1]);
                            dispInfo.XFactor_floor_description = subArray[2];
                            dispInfo.XFactor_floor_FullDescription = line;
                            hitCount++;
                        }
                        
                    }
                }
                else if (lineData.Contains("指定水平力引起的楼层位移") && lineData.Contains("Y") && lineData[0] == '(')
                {
                    InfoDisp dispInfo = new InfoDisp();                    
                    string[] dataArray = lineData.Trim().Split(new char[] { ' ', '(', ')', '+' }, StringSplitOptions.RemoveEmptyEntries);
                    dispInfo.WorkNo=int.Parse(dataArray[0]);
                    dispInfo.Description = lineData;
                    m_SumInfo.DispInfoList.Add(dispInfo);
                    int hitCount = 0;
                    while (i < m_contentArray.Length)
                    {
                        i++;
                        if (hitCount==3)
                        {
                            break;
                        }
                        if (m_contentArray[i].Contains("本工况下全楼最大位移比"))
                        {
                            string line = m_contentArray[i];
                            string[] subArray = line.Split(new char[] { '(', ')', '=' }, StringSplitOptions.RemoveEmptyEntries);
                            dispInfo.YFactor = double.Parse(subArray[1]);
                            dispInfo.YFactor_description = subArray[2];
                            dispInfo.YFactor_FullDescription = line;
                            hitCount++;
                        }
                        else if (m_contentArray[i].Contains("本工况下全楼最大层间位移角"))
                        {
                            string line = m_contentArray[i];
                            string[] subArray = line.Split(new char[] { '(', '/', ')' }, StringSplitOptions.RemoveEmptyEntries);
                            dispInfo.YMaxDisp_floorAngle = int.Parse(subArray[1]);
                            dispInfo.YMaxDisp_floorAngle_description = subArray[2];
                            dispInfo.YMaxDisp_floorAngle_FullDescription = line;
                            hitCount++;
                        }
                        else  if (m_contentArray[i].Contains("本工况下全楼最大层间位移比"))
                        {
                            string line = m_contentArray[i];
                            string[] subArray = line.Split(new char[] { '(', ')', '=' }, StringSplitOptions.RemoveEmptyEntries);
                            dispInfo.YFactor_floor = double.Parse(subArray[1]);
                            dispInfo.YFactor_floor_description = subArray[2];
                            dispInfo.YFactor_floor_FullDescription = line;
                            hitCount++;
                        }                        
                    }
                }
                else if (lineData.Contains("楼层位移") && lineData.Contains("X") && lineData[0] == '(')
                {
                    InfoDisp dispInfo = new InfoDisp();
                    string[] dataArray = lineData.Trim().Split(new char[] { ' ', '(', ')', '+' }, StringSplitOptions.RemoveEmptyEntries);
                    dispInfo.WorkNo = int.Parse(dataArray[0]);
                    dispInfo.Description = lineData;
                    m_SumInfo.DispInfoList.Add(dispInfo);
                    
                    while (i < m_contentArray.Length)
                    {
                        i++;                        
                        if (m_contentArray[i].Contains("本工况下全楼最大层间位移角"))
                        {
                            string line = m_contentArray[i];
                            string[] subArray = line.Split(new char[] { '(', '/', ')' }, StringSplitOptions.RemoveEmptyEntries);
                            dispInfo.XMaxDisp_floorAngle = int.Parse(subArray[1]);
                            dispInfo.XMaxDisp_floorAngle_description = subArray[2];
                            dispInfo.XMaxDisp_floorAngle_FullDescription = line;
                            break;
                        }
                    }
                }
                else if (lineData.Contains("楼层位移") && lineData.Contains("Y") && lineData[0] == '(')
                {
                    InfoDisp dispInfo = new InfoDisp();
                    string[] dataArray = lineData.Trim().Split(new char[] { ' ', '(', ')', '+' }, StringSplitOptions.RemoveEmptyEntries);
                    dispInfo.WorkNo = int.Parse(dataArray[0]);
                    dispInfo.Description = lineData;
                    m_SumInfo.DispInfoList.Add(dispInfo);
                    while (i < m_contentArray.Length)
                    {
                        i++;                        
                        if (m_contentArray[i].Contains("本工况下全楼最大层间位移角"))
                        {
                            string line = m_contentArray[i];
                            string[] subArray = line.Split(new char[] { '(', '/', ')' }, StringSplitOptions.RemoveEmptyEntries);
                            dispInfo.YMaxDisp_floorAngle = int.Parse(subArray[1]);
                            dispInfo.YMaxDisp_floorAngle_description = subArray[2];
                            dispInfo.YMaxDisp_floorAngle_FullDescription =line;
                            break;
                        }
                    }
                }                                
            }

            if (m_SumInfo.DispInfoList.Count > 0)
            {
                m_SumInfo.maxDispX =  m_SumInfo.maxDispY =  m_SumInfo.maxDispX_floor =  m_SumInfo.maxDispY_floor = m_SumInfo.DispInfoList[0];
                m_SumInfo.maxX_FloorDispAngle = m_SumInfo.maxY_FloorDispAngle = m_SumInfo.DispInfoList[0];
                for (int i = 1; i < m_SumInfo.DispInfoList.Count; i++)
                {
                    if (m_SumInfo.maxDispX.XFactor < m_SumInfo.DispInfoList[i].XFactor)
                    {
                        m_SumInfo.maxDispX = m_SumInfo.DispInfoList[i];
                    }
                    if (m_SumInfo.maxDispY.YFactor < m_SumInfo.DispInfoList[i].YFactor)
                    {
                         m_SumInfo.maxDispY = m_SumInfo.DispInfoList[i];
                    }
                    if ( m_SumInfo.maxDispX_floor.XFactor_floor < m_SumInfo.DispInfoList[i].XFactor_floor)
                    {
                         m_SumInfo.maxDispX_floor = m_SumInfo.DispInfoList[i];
                    }
                    if (m_SumInfo.maxDispY_floor.YFactor_floor < m_SumInfo.DispInfoList[i].YFactor_floor)
                    {
                        m_SumInfo.maxDispY_floor = m_SumInfo.DispInfoList[i];
                    }
                    if ((m_SumInfo.maxX_FloorDispAngle.XMaxDisp_floorAngle < 0.1 ||
                        (m_SumInfo.maxX_FloorDispAngle.XMaxDisp_floorAngle > m_SumInfo.DispInfoList[i].XMaxDisp_floorAngle
                        && m_SumInfo.DispInfoList[i].XMaxDisp_floorAngle > 0.1)) && !m_SumInfo.DispInfoList[i].Description.Contains("偏心"))
                    {
                        m_SumInfo.maxX_FloorDispAngle = m_SumInfo.DispInfoList[i];
                    }
                    if ((m_SumInfo.maxY_FloorDispAngle.YMaxDisp_floorAngle < 0.1 ||
                        (m_SumInfo.maxY_FloorDispAngle.YMaxDisp_floorAngle > m_SumInfo.DispInfoList[i].YMaxDisp_floorAngle
                        && m_SumInfo.DispInfoList[i].YMaxDisp_floorAngle > 0.1)) && !m_SumInfo.DispInfoList[i].Description.Contains("偏心"))
                    {
                        m_SumInfo.maxY_FloorDispAngle = m_SumInfo.DispInfoList[i];
                    }
                }
            }              
        }       
	}
}
