﻿#region License Header

// /***************************************************************************
//  *   Copyright (c) 2011 OpenUO Software Team.
//  *   All Right Reserved.
//  *
//  *   TileData.cs
//  *
//  *   This program is free software; you can redistribute it and/or modify
//  *   it under the terms of the GNU General Public License as published by
//  *   the Free Software Foundation; either version 3 of the License, or
//  *   (at your option) any later version.
//  ***************************************************************************/

#endregion

#region Usings

using System.IO;
using System.Text;

#endregion

namespace OpenUO.Ultima
{
    public class TileData
    {
        private readonly int[] m_HeightTable;
        private readonly ItemData[] m_ItemData;
        private readonly LandData[] m_LandData;
        private readonly byte[] m_StringBuffer = new byte[20];

        public TileData(InstallLocation install)
        {
            var filePath = install.GetPath("tiledata.mul");

            if(!string.IsNullOrEmpty(filePath))
            {
                using(var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var x64 = fs.Length >= 3188736;
                    var bin = new BinaryReader(fs);

                    m_LandData = new LandData[0x4000];

                    for(var i = 0; i < 0x4000; ++i)
                    {
                        if((i & 0x1F) == 0)
                        {
                            bin.ReadInt32(); // header
                        }

                        var flags = x64 ? (TileFlag)bin.ReadInt64() : (TileFlag)bin.ReadInt32();
                        bin.ReadInt16(); // skip 2 bytes -- textureID

                        m_LandData[i] = new LandData(ReadNameString(bin), flags);
                    }

                    m_ItemData = new ItemData[0x4000];
                    m_HeightTable = new int[0x4000];

                    for(var i = 0; i < 0x4000; ++i)
                    {
                        if((i & 0x1F) == 0)
                        {
                            bin.ReadInt32(); // header
                        }

                        var flags = x64 ? (TileFlag)bin.ReadInt64() : (TileFlag)bin.ReadInt32();
                        int weight = bin.ReadByte();
                        int quality = bin.ReadByte();
                        bin.ReadInt16();
                        bin.ReadByte();
                        int quantity = bin.ReadByte();
                        int anim = bin.ReadInt16();
                        bin.ReadInt16();
                        bin.ReadByte();
                        int value = bin.ReadByte();
                        int height = bin.ReadByte();

                        m_ItemData[i] = new ItemData(ReadNameString(bin), flags, weight, quality, quantity, value, height, anim);
                        m_HeightTable[i] = height;
                    }
                }
            }
            else
            {
                throw new FileNotFoundException();
            }
        }

        /// <summary>
        ///     Gets the list of <see cref="LandData">land tile data</see>.
        /// </summary>
        public LandData[] LandTable
        {
            get { return m_LandData; }
        }

        /// <summary>
        ///     Gets the list of <see cref="ItemData">item tile data</see>.
        /// </summary>
        public ItemData[] ItemTable
        {
            get { return m_ItemData; }
        }

        public int[] HeightTable
        {
            get { return m_HeightTable; }
        }

        private string ReadNameString(BinaryReader bin)
        {
            bin.Read(m_StringBuffer, 0, 20);

            int count;

            // This is very hackish... 
            for(count = 0; count < 20 && m_StringBuffer[count] != 0; ++count)
            {
            }

            return Encoding.ASCII.GetString(m_StringBuffer, 0, count);
        }
    }
}
