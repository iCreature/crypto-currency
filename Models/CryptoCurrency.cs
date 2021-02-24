using Neptune;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Data;
using System.Net;
using System.IO;
using System.Text;

namespace Neptune_Wallet.Models
{

    // import Newtonsoft.Json Package
    public class CryptoCurrency
    {
        private List<Transaction> _CurrentTransactions = new List<Transaction>();
        private List<Block> _chain = new List<Block>();
        private List<Node> _nodes = new List<Node>();
        
        private Block _lastBlock => _chain.Last();

        public string NodeId { get; private set; }
        static int blockCount = 0;
        static decimal reward = 50;// actual currency ---- Upcoin ----- Azania
        /*
         * total number of coins 
         * eg.  monthly subscription 500zar = coins worth 2000zar month  
         *  monthly subscription 1200zar = coins worth 3000zar a month
         * 
         * 
         */
        static string minerPrivateKey = "";
        static Wallet _minersWallet = RSA.RSA.KeyGenerate();

        public CryptoCurrency()
        {
            minerPrivateKey = "h890904jkfjkwgkngnenrer90ijo434n34jkg";//_minersWallet.PrivateKey;
            NodeId = "hp349543534jergfdkg";// _minersWallet.PublicKey;

            string sign = RSA.RSA.Sign(minerPrivateKey, "hh890904jkfjkwgkngnenrer90ijo434n34jkg");
            bool b = RSA.RSA.Verify(NodeId, "hp349543534jergfdkg", sign);

            // initial Transaction
            var transaction = new Transaction { Sender = "0", Recipent = NodeId, Amount = 50, Fees = 0, Signature = "" };
            // fees = (Amount/25)*0.1
            _CurrentTransactions.Add(transaction);

            CreateNewBlock(proof:100,previousHash:"1");
        }
        // new miner added to the network
        private void RegisterNode(string address)
        {
            _nodes.Add(new Node{Address =new Uri(address) });
        }
        private Block CreateNewBlock(int proof, string previousHash = null)
        {
            var block = new Block()
            {
                Index = _chain.Count,
                TimeStamp = DateTime.UtcNow,
                Transactions = _CurrentTransactions.ToList(),
                nonce = proof,
                PreviousHash = previousHash ?? GetHash(_chain.Last())
            };
            _CurrentTransactions.Clear();
            _chain.Add(block);
            return block;
        }
        private string  GetHash(Block block)
        {
            string blocktext = JsonConvert.SerializeObject(block);

            return GetSha256(blocktext);
        }

        // Generate the Hash
        private string GetSha256(string data)
        {
            var sha256 = new SHA256Managed();
            var hashBuilder = new StringBuilder();

            byte[] bytes = Encoding.Unicode.GetBytes(data);
            byte[] hash = sha256.ComputeHash(bytes);

            foreach (byte x in hash)
            {
                hashBuilder.Append($"{x:x2}");
            }
            return hashBuilder.ToString();
        }

        private int CreateProofOfWork(string previousHash)
        {
            int proof = 0;

            while (!IsValidProof(_CurrentTransactions,proof,previousHash))
            {
                proof++;
            }
            var transaction = new Transaction { Sender = "0", Recipent = NodeId, Amount = reward, Fees=0,Signature="" };
            _CurrentTransactions.Add(transaction);
            blockCount++;
            return proof;
        }

        private bool IsValidProof(List<Transaction>transactions, int proof, string previousHash)
        {
            var signature = transactions.Select(x => x.Signature).ToArray();
            string guess = $"{signature}{proof}{previousHash}";
            string result = GetSha256(guess);
            return result.StartsWith("00"); // difficulty
        }
        public bool VerifyTransactioSignature(Transaction transaction, string signedMessage, string publicKey)
        {
            string OriginalMessage = transaction.ToString();
            bool verified= RSA.RSA.Verify(publicKey,OriginalMessage,signedMessage);
            return verified;
        }
        private List<Transaction> TransactionByAddress( string OwnerAddress)
        {
            List<Transaction> trans = new List<Transaction>();
            foreach (var block in _chain.OrderByDescending(x=>x.Index))
            {
                var ownerTransactions = block.Transactions.Where(x => x.Sender == OwnerAddress || x.Recipent == OwnerAddress);
                trans.AddRange(ownerTransactions);
            }
            return trans;
        }
        public bool HasBalance(Transaction transaction)
        {
            var trans = TransactionByAddress(transaction.Sender);
            decimal balance = 0;
            foreach (var item in trans)
            {
                if (item.Recipent==transaction.Sender)
                {
                    balance = balance + item.Amount;
                }
                else
                {
                    balance = balance - item.Amount;
                }  
            }
            return balance>= (transaction.Amount+transaction.Fees);
        }
        private void AddTransation(Transaction transaction)
        {
            _CurrentTransactions.Add(transaction);
            if (transaction.Sender!=NodeId)
            {
                _CurrentTransactions.Add(new Transaction {

                    Sender = transaction.Sender,
                    Recipent = NodeId,
                    Amount = transaction.Fees,
                    Signature="",
                    Fees = 0

                });
            }
        }// private end

        private bool ResolvedConflicts()
        {
            List<Block> newChain = null;
            int maxLength = _chain.Count();
            foreach (Node node in _nodes)
            {
                var url = new Uri(node.Address,"/chain");
                var request = (HttpWebRequest)WebRequest.Create(url);
                var response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode==HttpStatusCode.OK)
                {
                    var model = new {

                        chain = new List<Block>(),
                        length=0
                    };

                    string json = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    var data = JsonConvert.DeserializeAnonymousType(json,model);
                    if (data.chain.Count>_chain.Count&& IsValidChain(data.chain))
                    {
                        maxLength = data.chain.Count();
                        newChain = data.chain;
                    }
                }
            }
            if (newChain!=null)
            {
                _chain = newChain;
                return true;

            }
            return false;
        }
        private bool IsValidChain(List<Block> chain)
        {
            Block block = null;
            Block lastBlock = chain.First();
            int currentIndex = 1;

            while (currentIndex<chain.Count)
            {
                block = chain.ElementAt(currentIndex);

                // check the correctness of the hash
                if (block.PreviousHash!=GetHash(lastBlock))
                {
                    return false;
                }
                // check the correctness of the Proof of Work
                if (!IsValidProof(block.Transactions,block.nonce,block.PreviousHash))
                {
                    return false;
                }
                lastBlock = block;
                currentIndex++;
            }
            return true;
        }

        // web server call 
        // calling API to excute
        internal Block mine()
        {
            int proof = CreateProofOfWork(_lastBlock.PreviousHash);
            Block block = CreateNewBlock(proof);

            return block;
        }
        internal string GetFullChain()
        {
            var response = new
            {
                chain = _chain.ToArray(),
                length = _chain.Count()
            };
            return JsonConvert.SerializeObject(response);
        }
        internal string RegisterNodes(string[] nodes)
        {
            var builder = new StringBuilder();
            foreach (var node in nodes)
            {
                string url = node;
                RegisterNode(url);
                builder.Append($"{url}, ");
            }
            builder.Insert(0,$"{nodes.Count()} new Nodes have been added: ");
            string result = builder.ToString();
            return result.Substring(0, result.Length - 2);
        }
        internal object Consensus()
        {
            bool replaced = ResolvedConflicts();
            string message = replaced ? "was replaced" : "is authorized";

            var response = new
            {
                Message = $"Chain:{message}",
                Chain= _chain
            };
            return response;// Json.SerialzedObject(response)
        }

        internal object CreateTransaction( Transaction transaction)
        {
            var rsp = new object();

            //verify
            var verified = VerifyTransactioSignature(transaction, transaction.Signature,transaction.Sender);

            if (verified==false || transaction.Sender== transaction.Recipent)
            {
                rsp = new { message = $"Invalid Transaction!"};
                return rsp;
            }
            if (HasBalance(transaction)==false)
            {
                rsp = new { message = $"InSufficent" };
                return rsp;
            }

            AddTransation(transaction);

            var blockIndex = _lastBlock!= null ?  _lastBlock.Index+1:0;

            rsp = new {message =$"Transaction will be added to Block{blockIndex}" };
            return rsp;
        }
        internal List<Transaction> GetTransactions()
        {
            return _CurrentTransactions;
        }
        internal List<Block> GetBlocks()
        {
            return _chain;
        }
        internal List<Node>GetNodes()
        {
            return _nodes;
        }
        internal Wallet GetWallet()
        {
            return _minersWallet;
        }
    }  
}