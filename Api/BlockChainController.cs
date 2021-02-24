using System;
using System.Collections.Generic;
using System.Linq;
using Neptune_Wallet.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;

namespace Neptune_Wallet.Api
{
    [EnableCors("MyPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class BlockChainController : ControllerBase
    {
        public  static CryptoCurrency blockchain = new CryptoCurrency();// tag --> mine call 
        // api to insert new transaction
        [HttpPost("transaction/new")]      
        public IActionResult new_transaction([FromBody]Transaction transaction)
        {
            var rsp = blockchain.CreateTransaction(transaction);
            return Ok(rsp);
        }
        // api to get All transactions 
        [HttpGet("transaction/get")]
        public IActionResult get_transaction()
        {
            var rsp = new { transaction = blockchain.GetTransactions()};
            return Ok(rsp);
        }
        [HttpGet("chain")]
        public IActionResult full_chain()
        {
            var block = blockchain.GetBlocks();
            var rsp = new { chain = block , length = block.Count()};
            return Ok(rsp);
        }
        [HttpGet("mine")]
        public IActionResult mine() // mine Api
        {
            // throws execption
            var block = blockchain.mine();
            var rsp = new
            {
                message ="New Block Created",
                block_number = block.Index,
                transaction = block.Transactions.ToArray(),
                _nonce_ = block.nonce,
                previousHash = block.PreviousHash
            };
            return Ok(rsp);
        }
        [HttpPost("register/nodes")]
        public IActionResult register_nodes(string[] nodes)
        {
            blockchain.RegisterNodes(nodes);
            var rsp = new {
                message = "New nodes have been added",
                total_nodes = nodes.Count()
            };

            return Created("",rsp);
        }
        [HttpGet("nodes/resolved")]
        public IActionResult consenues()
        {
            return Ok(blockchain.Consensus());
        }
        [HttpGet("nodes/get")]
        public IActionResult get_nodes()
        {
            return Ok(new { nodes=blockchain.GetNodes()});
        }
        // miner's Wallet 
        [HttpGet("wallet/miner")]
        public IActionResult GetMinersWallet()
        {         
            return Ok(blockchain.GetWallet());
        }
    }
}