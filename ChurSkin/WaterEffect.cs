﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Windows.Forms
{
    public class WaterEffect2
    {
        private System.Timers.Timer timerDraw;
        private System.Timers.Timer timerSetWave;
        //private System.ComponentModel.IContainer components;

        Bitmap m_bmp;
        byte[] m_byArrClrInfo;      //图片原始颜色信息
        byte[] m_byArrClrBuff;      //图片新的颜色信息
        int[,] m_nArrWaveCurrent;   //当前波形
        int[,] m_nArrWaveNext;      //下一帧的波形
        int m_nBmpWidth;
        int m_nBmpHeight;
        int m_nBmpWidthBySize;      //图片每行占用字节数
        private Control control;

        public WaterEffect2(Control control)
        {
            this.control = control;
            this.control.MouseMove += _MouseMove;
            timerDraw = new Timers.Timer();
            timerSetWave = new Timers.Timer();
            timerDraw.Elapsed += delegate { timerDraw_Tick(); };
            timerSetWave.Elapsed += delegate { timerSetWave_Tick(); };

            Bitmap bmp = new Bitmap(control.BackgroundImage);//打开一张图将起转换为24位
            m_bmp = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format24bppRgb);
            //加载图像信息 初始化变量
            BitmapData bmpData = m_bmp.LockBits(new Rectangle(0, 0, m_bmp.Width, m_bmp.Height), ImageLockMode.ReadOnly, m_bmp.PixelFormat);
            m_byArrClrInfo = new byte[bmpData.Stride * bmpData.Height];
            m_byArrClrBuff = new byte[m_byArrClrInfo.Length];
            m_nArrWaveCurrent = new int[m_bmp.Width, m_bmp.Height];
            m_nArrWaveNext = new int[m_bmp.Width, m_bmp.Height];
            m_nBmpWidth = m_bmp.Width;
            m_nBmpHeight = m_bmp.Height;
            m_nBmpWidthBySize = bmpData.Stride;
            Marshal.Copy(bmpData.Scan0, m_byArrClrInfo, 0, m_byArrClrInfo.Length);
            m_bmp.UnlockBits(bmpData);

            //启动水波的模拟
            timerDraw.Interval = 1025;        //绘制水波
            timerDraw.Enabled = true;
            timerSetWave.Interval = 500;    //随机产生波源
            timerSetWave.Enabled = true;
        }



        //绘制水波
        private void timerDraw_Tick()
        {
            int nNewX = 0;
            int nNewY = 0;
            BitmapData bmpData = m_bmp.LockBits(new Rectangle(0, 0, m_bmp.Width, m_bmp.Height), ImageLockMode.ReadWrite, m_bmp.PixelFormat);
            Marshal.Copy(bmpData.Scan0, m_byArrClrBuff, 0, m_byArrClrBuff.Length);
            for (int y = 1; y < m_nBmpHeight - 1; y++)
            {
                for (int x = 1; x < m_nBmpWidth - 1; x++)
                {
                    m_nArrWaveNext[x, y] = ((               //能量传递
                        m_nArrWaveCurrent[x - 1, y] +       //注意 能量传递是通过当前波形计算新的波形
                        m_nArrWaveCurrent[x + 1, y] +       //m_nArrWaveCurrent m_nArrWaveNext 不要弄翻
                        m_nArrWaveCurrent[x, y - 1] +
                        m_nArrWaveCurrent[x, y + 1])
                        >> 1) - m_nArrWaveNext[x, y];
                    m_nArrWaveNext[x, y] -= m_nArrWaveNext[x, y] >> 5;  //产生阻尼
                    //像素偏移 (模拟折射)
                    nNewX = ((m_nArrWaveNext[x + 1, y] - m_nArrWaveNext[x - 1, y]) >> 0) + x;   //右移越大 折射变大
                    nNewY = ((m_nArrWaveNext[x, y + 1] - m_nArrWaveNext[x, y - 1]) >> 0) + y;   //左移也可 折射变小

                    if (nNewX == x && nNewY == y) continue;     //没有产生像素偏移 直接跳过
                    if (nNewX < 0) nNewX = -nNewX;              //也可将其赋值为 0
                    if (nNewX >= m_nBmpWidth) nNewX = m_nBmpWidth - 1;
                    if (nNewY < 0) nNewY = -nNewY;
                    if (nNewY >= m_nBmpHeight) nNewY = m_nBmpHeight - 1;
                    //模拟光的反射 也可以跳过 不过波纹明暗度不明显
                    //m_byArrClrBuff[y * m_nBmpWidthBySize + x * 3] = m_byArrClrInfo[nNewY * m_nBmpWidthBySize + nNewX * 3];
                    //m_byArrClrBuff[y * m_nBmpWidthBySize + x * 3 + 1] = m_byArrClrInfo[nNewY * m_nBmpWidthBySize + nNewX * 3 + 1];
                    //m_byArrClrBuff[y * m_nBmpWidthBySize + x * 3 + 2] = m_byArrClrInfo[nNewY * m_nBmpWidthBySize + nNewX * 3 + 2];
                    //continue;
                    int nIncrement = m_nArrWaveNext[x, y];      //用当前像素点的能量作为光线明暗度变化标志
                    nIncrement >>= nIncrement < 0 ? 5 : 3;      //如果负数变暗 正数变量 (适当的位移一下不然差距太大)
                    //重置RGB值
                    int r = m_byArrClrInfo[nNewY * m_nBmpWidthBySize + nNewX * 3] + nIncrement;
                    int g = m_byArrClrInfo[nNewY * m_nBmpWidthBySize + nNewX * 3 + 1] + nIncrement;
                    int b = m_byArrClrInfo[nNewY * m_nBmpWidthBySize + nNewX * 3 + 2] + nIncrement;
                    if (nIncrement < 0)
                    {       //如果是负数便是变暗 则不能让其越界 0 - 255 
                        r = r < 0 ? 0 : r;
                        g = g < 0 ? 0 : g;
                        b = b < 0 ? 0 : b;
                    }
                    else {
                        r = r > 255 ? 255 : r;
                        g = g > 255 ? 255 : g;
                        b = b > 255 ? 255 : b;
                    }
                    m_byArrClrBuff[y * m_nBmpWidthBySize + x * 3] = (byte)r;
                    m_byArrClrBuff[y * m_nBmpWidthBySize + x * 3 + 1] = (byte)g;
                    m_byArrClrBuff[y * m_nBmpWidthBySize + x * 3 + 2] = (byte)b;
                }
            }
            Marshal.Copy(m_byArrClrBuff, 0, bmpData.Scan0, m_byArrClrBuff.Length);
            m_bmp.UnlockBits(bmpData);
            control.Refresh();
            //交换能量缓存 将新产生的波形 赋值给当前波形的缓存 计算下一帧的波形
            int[,] temp = m_nArrWaveCurrent;
            m_nArrWaveCurrent = m_nArrWaveNext;
            m_nArrWaveNext = temp;
        }
        //设置波源 x,y波源坐标  r波源半径 h波源的能量大小
        public void SetWavePoint(int x, int y, int r, int h)
        {
            //判断波源所在矩形位置是否越出图像 以便将越出部分坐标重置
            int nXStart = x - r < 0 ? 0 : x - r;        //波源矩形位置x轴起点
            int nYStart = y - r < 0 ? 0 : y - r;        //波源矩形位置y轴起点
            int nXLen = x + r >= m_nBmpWidth ? m_nBmpWidth - 1 : x + r;     //波源x轴矩形长度
            int nYlen = y + r >= m_nBmpHeight ? m_nBmpHeight - 1 : y + r;   //波源y轴矩形长度
            for (int posX = nXStart; posX < nXLen; posX++)
            {
                for (int posY = nYStart; posY < nYlen; posY++)
                {    //以点(x,y)半径为r内的点赋值一个能量
                    if ((posX - x) * (posX - x) + (posY - y) * (posY - y) < r * r)
                        m_nArrWaveCurrent[posX, posY] = -h;
                }
            }
        }

        private void timerSetWave_Tick()
        {
            Random rd = new Random();
            for (int i = 0; i < 5; i++)         //随机产生五个波源
                SetWavePoint(rd.Next(m_nBmpWidth - 1), rd.Next(m_nBmpHeight - 1), rd.Next(5, 10), rd.Next(32, 128));
        }

        private void _MouseMove(object sender, MouseEventArgs e)
        {
            Random rd = new Random();           //将鼠标滑动过的位置产生波源
            SetWavePoint(e.X, e.Y, rd.Next(5, 10), rd.Next(32, 128));
        }
    }
}
