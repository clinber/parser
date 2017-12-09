using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Satwe_tool
{
	public class ParserWdisp:ParserBase
	{
        public List<InfoDisp> dispList = new List<InfoDisp>();
        public InfoDisp maxX,maxY,maxX_floor,maxY_floor;
        public InfoDisp maxX_FloorDispAngle, maxY_FloorDispAngle;//层间位移角
        
        public void ParseDisp()
        {
            if (m_sourceName==PathFinder.PKPM||
                m_sourceName == PathFinder.YJK)
            {
                parsePkpmYjk();
            }
            else if (m_sourceName == PathFinder.MIDAS)
            {
                parseMidas();
            }
        }

        private void parsePkpmYjk()
        {
            for (int i = 0; i < m_contentArray.Length; i++)
            {
                if (m_contentArray[i].Contains("工况"))
                {
                    InfoDisp dispInfo = new InfoDisp();
                    dispList.Add(dispInfo);
                    string lineData = m_contentArray[i];
                    string[] dataArray = lineData.Trim().Split(new string[] { "工况", " " }, StringSplitOptions.RemoveEmptyEntries);
                    dispInfo.WorkNo = int.Parse(dataArray[1]);
                    dispInfo.Description = lineData;
                    if (m_contentArray[i].Contains("规定水平力"))
                    {
                        i = i + 1;
                        while (i < m_contentArray.Length && !m_contentArray[i].Contains("工况"))
                        {
                            if (m_contentArray[i].Contains("X方向最大位移") && m_contentArray[i].Contains("比值"))
                            {
                                string line = m_contentArray[i];
                                string[] subArray = line.Split(new char[] { '(', ':', '：', ')' }, StringSplitOptions.RemoveEmptyEntries);
                                dispInfo.XFactor = double.Parse(subArray[1]);
                                dispInfo.XFactor_description = subArray[subArray.Length - 1];
                                dispInfo.XFactor_FullDescription = line;
                            }
                            else if (m_contentArray[i].Contains("Y方向最大位移") && m_contentArray[i].Contains("比值"))
                            {
                                string line = m_contentArray[i];
                                string[] subArray = line.Split(new char[] { '(', ':', '：', ')' }, StringSplitOptions.RemoveEmptyEntries);
                                dispInfo.YFactor = double.Parse(subArray[1]);
                                dispInfo.YFactor_description = subArray[subArray.Length - 1];
                                dispInfo.YFactor_FullDescription = line;
                            }

                            if (m_contentArray[i].Contains("X方向最大层间位移") && m_contentArray[i].Contains("比值"))
                            {
                                string line = m_contentArray[i];
                                string[] subArray = line.Split(new char[] { '(', ':', '：', ')' }, StringSplitOptions.RemoveEmptyEntries);
                                dispInfo.XFactor_floor = double.Parse(subArray[1]);
                                dispInfo.XFactor_floor_description = subArray[subArray.Length - 1];
                                dispInfo.XFactor_floor_FullDescription = line;
                            }
                            else if (m_contentArray[i].Contains("Y方向最大层间位移") && m_contentArray[i].Contains("比值"))
                            {
                                string line = m_contentArray[i];
                                string[] subArray = line.Split(new char[] { '(', ':', '：', ')' }, StringSplitOptions.RemoveEmptyEntries);
                                dispInfo.YFactor_floor = double.Parse(subArray[1]);
                                dispInfo.YFactor_floor_description = subArray[subArray.Length - 1];
                                dispInfo.YFactor_floor_FullDescription = line;
                            }
                            i = i + 1;
                        }
                        i = i - 1;
                    }
                    else
                    {
                        i = i + 1;
                        while (i < m_contentArray.Length && !m_contentArray[i].Contains("工况"))
                        {
                            if (m_contentArray[i].Contains("X") && m_contentArray[i].Contains("最大层间位移角"))
                            {
                                string line = m_contentArray[i];
                                string[] datas = line.Split(new char[] { '/', ':', '：', '.', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                                dispInfo.XMaxDisp_floorAngle = int.Parse(datas[2]);
                                dispInfo.XMaxDisp_floorAngle_description = datas[datas.Length - 1];
                                dispInfo.XMaxDisp_floorAngle_FullDescription = line;
                            }
                            else if (m_contentArray[i].Contains("Y") && m_contentArray[i].Contains("最大层间位移角"))
                            {
                                string line = m_contentArray[i];
                                string[] datas = line.Split(new char[] { '/', ':', '：', '.', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                                dispInfo.YMaxDisp_floorAngle = int.Parse(datas[2]);
                                dispInfo.YMaxDisp_floorAngle_description = datas[datas.Length - 1];
                                dispInfo.YMaxDisp_floorAngle_FullDescription = line;
                            }
                            i = i + 1;
                        }
                        i = i - 1;
                    }
                }

            }
            if (dispList.Count > 0)
            {
                maxX = maxY = maxX_floor = maxY_floor = dispList[0];
                maxX_FloorDispAngle = maxY_FloorDispAngle = dispList[0];
                for (int i = 1; i < dispList.Count; i++)
                {
                    if (maxX.XFactor < dispList[i].XFactor)
                    {
                        maxX = dispList[i];
                    }
                    if (maxY.YFactor < dispList[i].YFactor)
                    {
                        maxY = dispList[i];
                    }
                    if (maxX_floor.XFactor_floor < dispList[i].XFactor_floor)
                    {
                        maxX_floor = dispList[i];
                    }
                    if (maxY_floor.YFactor_floor < dispList[i].YFactor_floor)
                    {
                        maxY_floor = dispList[i];
                    }
                    if ((maxX_FloorDispAngle.XMaxDisp_floorAngle < 0.1 ||
                        (maxX_FloorDispAngle.XMaxDisp_floorAngle > dispList[i].XMaxDisp_floorAngle
                        && dispList[i].XMaxDisp_floorAngle > 0.1)) && !dispList[i].Description.Contains("偶然偏心")
                        && !dispList[i].Description.Contains("双向"))
                    {
                        maxX_FloorDispAngle = dispList[i];
                    }
                    if ((maxY_FloorDispAngle.YMaxDisp_floorAngle < 0.1 ||
                        (maxY_FloorDispAngle.YMaxDisp_floorAngle > dispList[i].YMaxDisp_floorAngle
                        && dispList[i].YMaxDisp_floorAngle > 0.1)) && !dispList[i].Description.Contains("偶然偏心")
                        && !dispList[i].Description.Contains("双向"))
                    {
                        maxY_FloorDispAngle = dispList[i];
                    }
                }
            }
        }

        private void parseMidas()
        {
            int workNo = 1;
            for (int i = 0; i < m_contentArray.Length; i++)
            {
                if (m_contentArray[i].Contains("层号") && m_contentArray[i].Contains("Ratio"))
                {
                    InfoDisp dispInfo = new InfoDisp();
                    dispList.Add(dispInfo);
                    string lineData = m_contentArray[i - 5];
                    dispInfo.WorkNo = workNo++;
                    dispInfo.Description = lineData;
                    i++;
                    if (!lineData.Contains("90"))
                    {
                        while (i < m_contentArray.Length && !m_contentArray[i].Contains("层号"))
                        {
                            if (m_contentArray[i].Contains("最大层间位移角"))
                            {
                                string line = m_contentArray[i];
                                string[] datas = line.Split(new char[] { '/', ':', '：', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                                dispInfo.XMaxDisp_floorAngle = int.Parse(datas[4]);
                                dispInfo.XMaxDisp_floorAngle_description = datas[1];
                                dispInfo.XMaxDisp_floorAngle_FullDescription = line;
                            }
                            if (m_contentArray[i].Contains("最大位移与层平均位移的比值"))
                            {
                                string line = m_contentArray[i];
                                string[] subArray = line.Split(new char[] { '(', ':', '：', ')' }, StringSplitOptions.RemoveEmptyEntries);
                                dispInfo.XFactor = double.Parse(subArray[3]);
                                dispInfo.XFactor_description = subArray[1];
                                dispInfo.XFactor_FullDescription = line;
                            }
                            if (m_contentArray[i].Contains("最大层间位移与平均层间位移的比值"))
                            {
                                string line = m_contentArray[i];
                                string[] subArray = line.Split(new char[] { '(', ':', '：', ')' }, StringSplitOptions.RemoveEmptyEntries);
                                dispInfo.XFactor_floor = double.Parse(subArray[3]);
                                dispInfo.XFactor_floor_description = subArray[1];
                                dispInfo.XFactor_floor_FullDescription = line;
                            }
                            i = i + 1;
                        }                        
                    }
                    else
                    {
                        while (i < m_contentArray.Length && !m_contentArray[i].Contains("层号"))
                        {
                            if (m_contentArray[i].Contains("最大层间位移角"))
                            {
                                string line = m_contentArray[i];
                                string[] datas = line.Split(new char[] { '/', ':', '：', '.', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                                dispInfo.YMaxDisp_floorAngle = int.Parse(datas[4]);
                                dispInfo.YMaxDisp_floorAngle_description = datas[1];
                                dispInfo.YMaxDisp_floorAngle_FullDescription = line;
                            }
                            if (m_contentArray[i].Contains("最大位移与层平均位移的比值"))
                            {
                                string line = m_contentArray[i];
                                string[] subArray = line.Split(new char[] { '(', ':', '：', ')' }, StringSplitOptions.RemoveEmptyEntries);
                                dispInfo.YFactor = double.Parse(subArray[3]);
                                dispInfo.YFactor_description = subArray[1];
                                dispInfo.YFactor_FullDescription = line;
                            }
                            if (m_contentArray[i].Contains("最大层间位移与平均层间位移的比值"))
                            {
                                string line = m_contentArray[i];
                                string[] subArray = line.Split(new char[] { '(', ':', '：', ')' }, StringSplitOptions.RemoveEmptyEntries);
                                dispInfo.YFactor_floor = double.Parse(subArray[3]);
                                dispInfo.YFactor_floor_description = subArray[1];
                                dispInfo.YFactor_floor_FullDescription = line;
                            }
                            i = i + 1;
                        }                       
                    }
                    i = i - 1;
                }
            }

            if (dispList.Count > 0)
            {
                maxX = maxY = maxX_floor = maxY_floor = dispList[0];
                maxX_FloorDispAngle = maxY_FloorDispAngle = dispList[0];
                for (int i = 1; i < dispList.Count; i++)
                {
                    if (maxX.XFactor < dispList[i].XFactor)
                    {
                        maxX = dispList[i];
                    }
                    if (maxY.YFactor < dispList[i].YFactor)
                    {
                        maxY = dispList[i];
                    }
                    if (maxX_floor.XFactor_floor < dispList[i].XFactor_floor)
                    {
                        maxX_floor = dispList[i];
                    }
                    if (maxY_floor.YFactor_floor < dispList[i].YFactor_floor)
                    {
                        maxY_floor = dispList[i];
                    }
                    if ((maxX_FloorDispAngle.XMaxDisp_floorAngle < 0.1 ||
                        (maxX_FloorDispAngle.XMaxDisp_floorAngle > dispList[i].XMaxDisp_floorAngle
                        && dispList[i].XMaxDisp_floorAngle > 0.1)) && !dispList[i].Description.Contains("偶然偏心"))
                    {
                        maxX_FloorDispAngle = dispList[i];
                    }
                    if ((maxY_FloorDispAngle.YMaxDisp_floorAngle < 0.1 ||
                        (maxY_FloorDispAngle.YMaxDisp_floorAngle > dispList[i].YMaxDisp_floorAngle
                        && dispList[i].YMaxDisp_floorAngle > 0.1)) && !dispList[i].Description.Contains("偶然偏心"))
                    {
                        maxY_FloorDispAngle = dispList[i];
                    }
                }
            }
        }
	}
}
