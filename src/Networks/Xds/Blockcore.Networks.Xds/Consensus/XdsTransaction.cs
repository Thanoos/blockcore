using System.IO;
using NBitcoin;
using NBitcoin.Crypto;

namespace Blockcore.Networks.Xds.Consensus
{
    public class XdsTransaction : Transaction
    {
        public override bool IsProtocolTransaction()
        {
            return this.IsCoinBase || this.IsCoinStake;
        }
    }
}