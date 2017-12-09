using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
namespace Satwe_tool
{
	public class ParserWzq:ParserBase
	{
        public double EffectiveMassFactor_X;
        public double EffectiveMassFactor_Y;
        public List<InfoT> TinfoList = new List<InfoT>();
        public List<InfoVG> VgList=new List<InfoVG>();
        public InfoVG minVg_X, minVg_Y;
        private DataSummary m_sumaryData;
        
        public List<InfoT> ParseTInfo(int floorCount,string SourceName,DataSummary summaryData)
        {
            m_sourceName=SourceName;
            m_sumaryData=summaryData;
			if (SourceName == PathFinder.PKPM) {
				PkpmTInfo(floorCount);
			} else if (SourceName == PathFinder.YJK) {
				YjkTInfo(floorCount);
            }
            else if (SourceName == PathFinder.MIDAS)
            {
                MidasTInfo(floorCount);
            }          
            return TinfoList;
        }

		private void PkpmTInfo(int floorCount)
		{
			Dictionary<int, Dictionary<int, InfoVG>> mapFloorNoToVgInfo = new Dictionary<int, Dictionary<int, InfoVG>>();
			for (int i = 0; i < m_contentArray.Length; i++) {
                if (m_contentArray[i].Contains("振型号") && m_contentArray[i].Contains("平动系数"))
                {
                    for (int j = i + 1; ; j++)
                    {						
						string lineData = m_contentArray[j];
						string[] dataArray = lineData.Trim().Split(new char[] { ' ', '(', ')', '+' }, StringSplitOptions.RemoveEmptyEntries);
						if (dataArray.Length == 0) {
							break;
						}
						InfoT tInfo = new InfoT();
						tInfo.LoadData(dataArray);
						TinfoList.Add(tInfo);
					}
				} else if (m_contentArray[i].Contains("X 方向的有效质量系数")) {
					string lineData = m_contentArray[i];
					string valueData = lineData.Substring(lineData.IndexOf(':') + 1).Trim(new char[] { '%', ' ' });
					EffectiveMassFactor_X = double.Parse(valueData);
				} else if (m_contentArray[i].Contains("Y 方向的有效质量系数")) {
					string lineData = m_contentArray[i];
					string valueData = lineData.Substring(lineData.IndexOf(':') + 1).Trim(new char[] { '%', ' ' });
					EffectiveMassFactor_Y = double.Parse(valueData);
				} else if (m_contentArray[i].Contains("Fx") && m_contentArray[i].Contains("Vx")) {
					string[] lastFoorArray = null;
					for (int j = 0; j < floorCount; j++) {
						int index = i + 5 + j;						
						string lineData = m_contentArray[index];
						string[] dataArray = lineData.Split(new char[] { ' ', '(', ')', '%' },StringSplitOptions.RemoveEmptyEntries);
						if (dataArray.Length==8) {
							lastFoorArray = dataArray;
						} else {
							List<string> dataList = new List<string>(dataArray);
							dataList.Insert(0, lastFoorArray[0]);
							dataList.Insert(5, lastFoorArray[5]);
							dataArray = dataList.ToArray();
						}
						InfoVG vgInfo = new InfoVG();						
						vgInfo.LoadData(dataArray, true);
						if (!mapFloorNoToVgInfo.ContainsKey(vgInfo.TowerNo)) {
							mapFloorNoToVgInfo.Add(vgInfo.TowerNo,new Dictionary<int, InfoVG>());
						}
						mapFloorNoToVgInfo[vgInfo.TowerNo].Add(vgInfo.FoorNo, vgInfo);
						VgList.Add(vgInfo);
					}
				} else if (m_contentArray[i].Contains("Fy") && m_contentArray[i].Contains("Vy")) {
					string[] lastFoorArray = null;
					for (int j = 0; j < floorCount; j++) {
						int index = i + 5 + j;						
						string lineData = m_contentArray[index];
						string[] dataArray = lineData.Split(new char[] { ' ', '(', ')', '%' },StringSplitOptions.RemoveEmptyEntries);
						if (dataArray.Length == 8) {
							lastFoorArray = dataArray;
						} else {
							List<string> dataList = new List<string>(dataArray);
							dataList.Insert(0, lastFoorArray[0]);
							dataList.Insert(5, lastFoorArray[5]);
							dataArray = dataList.ToArray();
						}						
						int floorNo = int.Parse(dataArray[0]);
						int towerNo = int.Parse(dataArray[1]);
						InfoVG vgInfo = mapFloorNoToVgInfo[towerNo][floorNo];
						vgInfo.LoadData(dataArray, false);
					}
				}
			}

            VgList.Sort((InfoVG left, InfoVG right) =>
            {
                return left.FoorNo.CompareTo(right.FoorNo);
            });
            int startFloor=1;
            if (m_sourceName==PathFinder.YJK||m_sourceName==PathFinder.SAP)
            {
                startFloor=m_sumaryData.QianguFloor+1;
            }else if(m_sumaryData.QianguFloor>0)
            {
                startFloor = m_sumaryData.QianguFloor;
            }            
             minVg_X =  minVg_Y = VgList[startFloor-1];
            for (int i = startFloor; i < VgList.Count; i++)
            {
				if (minVg_X.Vx_Ratio > VgList[i].Vx_Ratio) {
					minVg_X = VgList[i];
				}
				if (minVg_Y.Vy_Ratio > VgList[i].Vy_Ratio) {
					minVg_Y = VgList[i];
				}
			}
		}
		private void YjkTInfo(int floorCount)
		{
			Dictionary<int, Dictionary<int, InfoVG>> mapFloorNoToVgInfo = new Dictionary<int, Dictionary<int, InfoVG>>();
            bool hasQiangGang = false;
			for (int i = 0; i < m_contentArray.Length; i++) {
                if (m_contentArray[i].Contains("振型号") && m_contentArray[i].Contains("平动系数"))
                {
                    if (m_contentArray[i].Contains("强制刚性楼板"))
                    {
                        hasQiangGang=true;
                        for (int j = i + 2; ; j++)
                        {
                            string lineData = m_contentArray[j];
                            string[] dataArray = lineData.Trim().Split(new char[] { ' ', '(', ')', '+' }, StringSplitOptions.RemoveEmptyEntries);
                            if (dataArray.Length == 0)
                            {
                                break;
                            }
                            InfoT tInfo = new InfoT();
                            tInfo.LoadData(dataArray);
                            TinfoList.Add(tInfo);
                        }
                    }
                    if (!hasQiangGang)
                    {
                        for (int j = i + 1; ; j++)
                        {
                            string lineData = m_contentArray[j];
                            string[] dataArray = lineData.Trim().Split(new char[] { ' ', '(', ')', '+' }, StringSplitOptions.RemoveEmptyEntries);
                            if (dataArray.Length == 0)
                            {
                                break;
                            }
                            InfoT tInfo = new InfoT();
                            tInfo.LoadData(dataArray);
                            TinfoList.Add(tInfo);
                        }
                    }
					
				} else if (m_contentArray[i].Contains("X向平动振型参与质量系数")) {
					string lineData = m_contentArray[i];
					string valueData = lineData.Substring(lineData.IndexOf(':') + 1).Trim(new char[] { '%', ' ' });
					EffectiveMassFactor_X = double.Parse(valueData);
				} else if (m_contentArray[i].Contains("Y向平动振型参与质量系数")) {
					string lineData = m_contentArray[i];
					string valueData = lineData.Substring(lineData.IndexOf(':') + 1).Trim(new char[] { '%', ' ' });
					EffectiveMassFactor_Y = double.Parse(valueData);
				} else if (m_contentArray[i].Contains("Fx") && m_contentArray[i].Contains("Vx")) {
					for (int j = 0; j < floorCount; j++) {
						int index = i + 2 + j;						
						string lineData = m_contentArray[index];
						string[] dataArray = lineData.Split(new char[] { ' ', '(', ')', '%' },StringSplitOptions.RemoveEmptyEntries);
  					    InfoVG vgInfo = new InfoVG();
						vgInfo.LoadData(dataArray, true);
						if (!mapFloorNoToVgInfo.ContainsKey(vgInfo.TowerNo)) {
							mapFloorNoToVgInfo.Add(vgInfo.TowerNo, new Dictionary<int, InfoVG>());
						}
						mapFloorNoToVgInfo[vgInfo.TowerNo].Add(vgInfo.FoorNo, vgInfo);
						VgList.Add(vgInfo);
					}
				} else if (m_contentArray[i].Contains("Fy") && m_contentArray[i].Contains("Vy")) {
					for (int j = 0; j < floorCount; j++) {
						int index = i + 2 + j;						
						string lineData = m_contentArray[index];
						string[] dataArray = lineData.Split(new char[] { ' ', '(', ')', '%' },StringSplitOptions.RemoveEmptyEntries);
						int floorNo = int.Parse(dataArray[0]);
						int towerNo = int.Parse(dataArray[1]);
						InfoVG vgInfo = mapFloorNoToVgInfo[towerNo][floorNo];
						vgInfo.LoadData(dataArray, false);
					}
				}
			}

            VgList.Sort((InfoVG left, InfoVG right) =>
            {
                return left.FoorNo.CompareTo(right.FoorNo);
            });
            int startFloor = 1;
            if (m_sourceName == PathFinder.YJK || m_sourceName == PathFinder.SAP)
            {
                startFloor = m_sumaryData.QianguFloor + 1;
            }
            else if (m_sumaryData.QianguFloor > 0)
            {
                startFloor = m_sumaryData.QianguFloor;
            }
            minVg_X = minVg_Y = VgList[startFloor - 1];
            for (int i = startFloor; i < VgList.Count; i++)
            {
                if (minVg_X.Vx_Ratio > VgList[i].Vx_Ratio)
                {
                    minVg_X = VgList[i];
                }
                if (minVg_Y.Vy_Ratio > VgList[i].Vy_Ratio)
                {
                    minVg_Y = VgList[i];
                }
            }
		}
        private void MidasTInfo(int floorCount)
        {
            Dictionary<int, InfoVG> mapFloorNoToVgInfo = new Dictionary<int, InfoVG>();
            for (int i = 0; i < m_contentArray.Length; i++)
            {
                if (m_contentArray[i].Contains("振型号") && m_contentArray[i].Contains("平动因子"))
                {
                    for (int j = i + 2; ; j++)
                    {
                        string lineData = m_contentArray[j];
                        string[] dataArray = lineData.Trim().Split(new char[] { ' ', '(', ')', '+' }, StringSplitOptions.RemoveEmptyEntries);
                        if (dataArray.Length == 0)
                        {
                            break;
                        }
                        InfoT tInfo = new InfoT();
                        tInfo.ShakeNo = int.Parse(dataArray[0]);
                        tInfo.T = double.Parse(dataArray[1]);
                        tInfo.Angle = 0;
                        tInfo.MoveFactor_X = double.Parse(dataArray[2])/100;
                        tInfo.MoveFactor_Y = double.Parse(dataArray[3])/100;
                        tInfo.TwistFactor = double.Parse(dataArray[4])/100;
                        tInfo.MoveFactor= tInfo.MoveFactor_X+ tInfo.MoveFactor_Y;
                        TinfoList.Add(tInfo);
                    }
                }
                else if (m_contentArray[i].Contains("X向平动振型参与质量系数总计"))
                {
                    string lineData = m_contentArray[i];
                    string valueData = lineData.Substring(lineData.IndexOf(':') + 1).Trim(new char[] { '%', ' ' });
                    EffectiveMassFactor_X = double.Parse(valueData);
                }
                else if (m_contentArray[i].Contains("Y向平动振型参与质量系数总计"))
                {
                    string lineData = m_contentArray[i];
                    string valueData = lineData.Substring(lineData.IndexOf(':') + 1).Trim(new char[] { '%', ' ' });
                    EffectiveMassFactor_Y = double.Parse(valueData);
                }
                else if (m_contentArray[i].Contains("层地震力") && m_contentArray[i].Contains("剪重比"))
                {
                    bool isY = false;
                    if (m_contentArray[i-2].Contains("90"))
                    {
                        isY = true;
                    }
                    for (int j = 0; j < floorCount; j++)
                    {
                        int index = i + 2 + j;
                        string lineData = m_contentArray[index];
                        string[] dataArray = lineData.Split(new char[] { ' ', '(', ')', '%' }, StringSplitOptions.RemoveEmptyEntries);
                        InfoVG vgInfo = new InfoVG();
                        string floorNoStr = dataArray[1].Trim('F');
                        if (floorNoStr.Contains("B"))
                        {
                            vgInfo.FoorNo = int.Parse(floorNoStr.Trim('B')) * -1;
                        }
                        else
                        {
                            vgInfo.FoorNo = int.Parse(floorNoStr.Trim('B'));
                        }
                        if (!mapFloorNoToVgInfo.ContainsKey(vgInfo.FoorNo))
                        {
                            mapFloorNoToVgInfo.Add(vgInfo.FoorNo, vgInfo);
                            VgList.Add(vgInfo);
                        }
                        else
                        {
                            vgInfo = mapFloorNoToVgInfo[vgInfo.FoorNo];
                        }
                        double ratio= double.Parse(dataArray[4]);
                        if (isY)
                        {
                            if (vgInfo.Vy_Ratio < 0.0001 || vgInfo.Vy_Ratio > ratio)
                            {
                                vgInfo.Fy = double.Parse(dataArray[2]);
                                vgInfo.Vy = double.Parse(dataArray[3]);
                                vgInfo.Vy_Ratio = ratio;
                            }
                            
                        }
                        else
                        {
                            if (vgInfo.Vx_Ratio < 0.0001 || vgInfo.Vx_Ratio > ratio)
                            {
                                vgInfo.Fx = double.Parse(dataArray[2]);
                                vgInfo.Vx = double.Parse(dataArray[3]);
                                vgInfo.Vx_Ratio = ratio;
                            }
                        }
                       
                        
                    }
                }
            }

            VgList.Sort((InfoVG left, InfoVG right) =>
            {
                return left.FoorNo.CompareTo(right.FoorNo);
            });
            int startFloor = m_sumaryData.QianguFloor + m_sumaryData.BaseCount;           
            minVg_X = minVg_Y = VgList[startFloor];
            for (int i = startFloor; i < VgList.Count; i++)
            {
                if (minVg_X.Vx_Ratio > VgList[i].Vx_Ratio)
                {
                    minVg_X = VgList[i];
                }
                if (minVg_Y.Vy_Ratio > VgList[i].Vy_Ratio)
                {
                    minVg_Y = VgList[i];
                }
            }

            VgList.Sort((InfoVG left, InfoVG right) =>
            {
                return right.FoorNo.CompareTo(left.FoorNo);
            });
        }
	}
}
