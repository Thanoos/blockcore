using System;
using System.Linq;
using System.Net;
using x42.Networks.Consensus;
using x42.Networks.Policies;
using x42.Networks.Setup;
using NBitcoin;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;

namespace x42.Networks
{
    public class x42Test : x42Main
    {
        public x42Test()
        {
            // START MODIFICATIONS OF GENERATED CODE
            var consensusOptions = new x42PosConsensusOptions(
                maxBlockBaseSize: 1_000_000,
                maxStandardVersion: 2,
                maxStandardTxWeight: 100_000,
                maxBlockSigopsCost: 20_000,
                maxStandardTxSigopsCost: 20_000 / 5,
                witnessScaleFactor: 4
            );
            // END MODIFICATIONS

            CoinSetup setup = x42Setup.Instance.Setup;
            NetworkSetup network = x42Setup.Instance.Test;

            this.NetworkType = NetworkType.Testnet;

            var messageStart = new byte[4];
            messageStart[0] = 0x42;
            messageStart[1] = 0x66;
            messageStart[2] = 0x52;
            messageStart[3] = 0x04;
            uint testNetMagic = BitConverter.ToUInt32(messageStart, 0); //0x4526642

            this.Name = network.Name;
            this.CoinTicker = network.CoinTicker;
            this.Magic = testNetMagic;
            this.RootFolderName = network.RootFolderName;
            this.DefaultPort = network.DefaultPort;
            this.DefaultRPCPort = network.DefaultRPCPort;
            this.DefaultAPIPort = network.DefaultAPIPort;
            this.DefaultSignalRPort = network.DefaultSignalRPort;
            this.DefaultBanTimeSeconds = 288; // 9 (MaxReorg) * 64 (TargetSpacing) / 2 = 4 hours, 26 minutes and 40 seconds

            var consensusFactory = new PosConsensusFactory();

            // Create the genesis block.
            this.GenesisTime = network.GenesisTime;
            this.GenesisNonce = network.GenesisNonce;
            this.GenesisBits = network.GenesisBits;
            this.GenesisVersion = network.GenesisVersion;
            this.GenesisReward = network.GenesisReward;

            Block genesisBlock = CreateGenesisBlock(consensusFactory,
               this.GenesisTime,
               this.GenesisNonce,
               this.GenesisBits,
               this.GenesisVersion,
               this.GenesisReward,
               setup.GenesisText);

            this.Genesis = genesisBlock;

            var buriedDeployments = new BuriedDeploymentsArray
            {
                [BuriedDeployments.BIP34] = 0,
                [BuriedDeployments.BIP65] = 0,
                [BuriedDeployments.BIP66] = 0
            };

            this.Consensus = new x42Consensus(
                consensusFactory: consensusFactory,
                consensusOptions: consensusOptions,
                coinType: setup.CoinType,
                hashGenesisBlock: genesisBlock.GetHash(),
                subsidyHalvingInterval: 210000,
                majorityEnforceBlockUpgrade: 750,
                majorityRejectBlockOutdated: 950,
                majorityWindow: 1000,
                buriedDeployments: buriedDeployments,
                bip9Deployments: new NoBIP9Deployments(),
                bip34Hash: null,
                minerConfirmationWindow: 2016, // nPowTargetTimespan / nPowTargetSpacing
                maxReorgLength: 9,
                defaultAssumeValid: null,
                maxMoney: Money.Coins(42 * 1000000),
                coinbaseMaturity: 10,
                premineHeight: 2,
                premineReward: Money.Coins(setup.PremineReward),
                proofOfWorkReward: Money.Coins(setup.PoWBlockReward),
                targetTimespan: TimeSpan.FromSeconds(14 * 24 * 60 * 60), // two weeks
                targetSpacing: setup.TargetSpacing,
                powAllowMinDifficultyBlocks: false,
                posNoRetargeting: false,
                powNoRetargeting: false,
                powLimit: new Target(new uint256("0000ffff00000000000000000000000000000000000000000000000000000000")),
                minimumChainWork: null,
                isProofOfStake: true,
                lastPowBlock: 523,
                proofOfStakeLimitV2: new BigInteger(uint256.Parse("000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeReward: Money.Coins(setup.PoSBlockReward),
                subsidyLimit: setup.SubsidyLimit,
                proofOfStakeRewardAfterSubsidyLimit: setup.ProofOfStakeRewardAfterSubsidyLimit,
                lastProofOfStakeRewardHeight: setup.LastProofOfStakeRewardHeight,
                proofOfStakeTimestampMask: setup.ProofOfStakeTimestampMask,
                posEmptyCoinbase: x42Setup.Instance.IsPoSv3()
            )
            {
                PosUseTimeFieldInKernalHash = x42Setup.Instance.IsPoSv3()
            };

            this.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (byte)network.PubKeyAddress };
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (byte)network.ScriptAddress };
            this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (239) };
            this.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x35), (0x87), (0xCF) };
            this.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x35), (0x83), (0x94) };
            this.Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2b };
            this.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 115 };

            this.Bech32Encoders = new Bech32Encoder[2];
            var encoder = new Bech32Encoder(network.CoinTicker.ToLowerInvariant());
            this.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            this.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

            this.Checkpoints = network.Checkpoints;
            this.DNSSeeds = network.DNS.Select(dns => new DNSSeedData(dns, dns)).ToList();
            this.SeedNodes = network.Nodes.Select(node => new NBitcoin.Protocol.NetworkAddress(IPAddress.Parse(node), network.DefaultPort)).ToList();

            this.StandardScriptsRegistry = new x42StandardScriptsRegistry();

            // 64 below should be changed to TargetSpacingSeconds when we move that field.
            Assert(this.DefaultBanTimeSeconds <= this.Consensus.MaxReorgLength * 64 / 2);

            Assert(this.Consensus.HashGenesisBlock == uint256.Parse(network.HashGenesisBlock));
            Assert(this.Genesis.Header.HashMerkleRoot == uint256.Parse(network.HashMerkleRoot));

            RegisterRules(this.Consensus);
            RegisterMempoolRules(this.Consensus);
        }
    }
}