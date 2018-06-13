﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OsuLiveStatusPanel.PPShow
{
    internal class Oppai
    {
        private pp_params m_cache = new pp_params();
        
        #region oppai struct
        [StructLayout(LayoutKind.Sequential)]
        public class pp_params
        {
            /* required parameters */
            public double aim, speed;
            public float base_ar, base_od;
            public Int32 max_combo;
            public UInt16 nsliders; /* required for scorev1 only */
            public UInt16 ncircles; /* ^ */
            public UInt16 nobjects;

            /* optional parameters */
            public UInt32 mode; /* defaults to MODE_STD */
            public UInt32 mods; /* defaults to MODS_NOMOD */
            public Int32 combo; /* defaults to FC */
            public UInt16 n300, n100, n50; /* defaults to SS */
            public UInt16 nmiss; /* defaults to 0 */
            public UInt16 score_version; /* defaults to PP_DEFAULT_SCORING */
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct pp_calc
        {
            /* ppv2 will store results here */
            public double total, aim, speed, acc;
            public double accuracy; /* 0.0 - 1.0 */
            public static pp_calc Empty;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct rtpp_params
        {
            public UInt32 mods;
            public int n50, n100, nmiss;
            public int combo;
            public UInt32 mode;
        }
        #endregion

        #region oppai P/Ivoke

        [DllImport("oppai.dll")]
        public extern static int sppv2_by_acc(byte[] data, UInt32 size, double acc, UInt32 mode, UInt32 mods, [Out]byte[] str_buf, UInt32 buf_size);

        [DllImport("oppai.dll")]
        public extern static bool get_ppv2(byte[] data, UInt32 data_size, ref rtpp_params args, Boolean use_cache, pp_params cache, ref pp_calc result);
        #endregion
        
        private static double round_oppai(double x)
        {
            return Math.Floor((x) + 0.5);
        }

        public static double acc_calc(int n300, int n100, int n50, int misses)
        {
            int total_hits = n300 + n100 + n50 + misses;
            double acc = 100.0;

            if (total_hits > 0)
            {
                acc = (
                    n50 * 50.0 + n100 * 100.0 + n300 * 300.0) /
                    (total_hits * 300.0);
            }

            return acc;
        }

        public static void acc_round(double acc_percent, int nobjects,
            int misses, out int n300, out int n100, out int n50)
        {
            int max300;
            double maxacc;

            misses = Math.Min(nobjects, misses);
            max300 = nobjects - misses;
            maxacc = acc_calc(max300, 0, 0, misses) * 100.0;
            acc_percent = Math.Max(0.0, Math.Min(maxacc, acc_percent));

            n50 = 0;

            /* just some black magic maths from wolfram alpha */
            n100 = (int)round_oppai(-3.0 * ((acc_percent * 0.01 - 1.0) *
            nobjects + misses) * 0.5);
            if (n100 > nobjects - misses)
            {
                /* acc lower than all 100s, use 50s */
                n100 = 0;
                n50 = (int)round_oppai(-6.0 * ((acc_percent * 0.01 - 1.0) * nobjects + misses) * 0.2);

                n50 = Math.Min(max300, n50);
            }
            else
            {
                n100 = Math.Min(max300, n100);
            }

            n300 = nobjects - n100 - n50 - misses;
        }
    }
}
