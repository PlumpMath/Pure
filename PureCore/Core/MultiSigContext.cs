﻿using Pure.Core.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pure.Core
{
    internal class MultiSigContext
    {
        internal byte[] redeemScript;
        internal byte[][] signatures;

        private byte m;
        private byte[][] pubkeys;

        public bool Completed
        {
            get
            {
                return signatures.Count(p => p != null) >= m;
            }
        }

        public bool Full
        {
            get
            {
                return signatures.All(p => p != null);
            }
        }

        public MultiSigContext(byte[] redeemScript)
        {
            this.redeemScript = redeemScript;
            int i = 0;
            this.m = (byte)(redeemScript[i++] - 0x50);
            if (m < 1)
                throw new FormatException();
            List<byte[]> pubkeys = new List<byte[]>();
            while (redeemScript[i] == 33)
            {
                byte[] pubkey = new byte[redeemScript[i]];
                Buffer.BlockCopy(redeemScript, i + 1, pubkey, 0, redeemScript[i]);
                pubkeys.Add(pubkey);
                i += redeemScript[i] + 1;
            }
            if (pubkeys.Count != redeemScript[i] || pubkeys.Count < m)
                throw new FormatException();
            this.pubkeys = pubkeys.ToArray();
            this.signatures = new byte[pubkeys.Count][];
        }

        public bool Add(UInt160 pubKeyHash, byte[] signature)
        {
            if (signature.Length != 64)
                throw new ArgumentException();
            for (int i = 0; i < pubkeys.Length; i++)
            {
                if (pubkeys[i].ToPublicKeyHash() == pubKeyHash)
                {
                    if (signatures[i] != null)
                        return false;
                    signatures[i] = signature;
                    return true;
                }
            }
            return false;
        }

        public byte[] GetScript()
        {
            if (!Completed)
                throw new InvalidOperationException();
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                for (int i = 0; i < signatures.Length; i++)
                {
                    if (signatures[i] != null)
                    {
                        sb.Push(signatures[i]);
                    }
                }
                return sb.Push(redeemScript).ToArray();
            }
        }
    }
}
