/****************************************************
 * File: BytesConversion.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/08/2021
   * Project: RL-terrain-adaptation
   * Last update: 20/02/2023
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Static class to convert to array of bytes and viceversa.
/// </summary>
public static class BytesConversion
{
    public static byte[] ToBytes<T>(this T[,] array) where T : struct
    {
        var buffer = new byte[array.GetLength(0) * array.GetLength(1) * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T))];
        Buffer.BlockCopy(array, 0, buffer, 0, buffer.Length);
        return buffer;
    }
    public static void FromBytes<T>(this T[,] array, byte[] buffer) where T : struct
    {
        var len = Math.Min(array.GetLength(0) * array.GetLength(1) * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)), buffer.Length);
        Buffer.BlockCopy(buffer, 0, array, 0, len);
    }
}
