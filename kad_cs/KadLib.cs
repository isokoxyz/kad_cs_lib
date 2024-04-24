using System.Text;
using static HashingUtils;
using Newtonsoft.Json;
using static CommandUtilities;
using Newtonsoft.Json.Linq;
using System.Collections;
public class KadLib
{
    /// <summary>
    /// This method signs a command using the Pact API endpoint
    /// </summary>
    /// <param name="cmd">json serializable object containing information about the command to be signed</param>
    /// <returns>The command in a format ready for execution on the Kadena blockchain and containing the signatures</returns>
    public static async Task<string> sign(string cmd, string method)
    {
        var cmdToSign = formatCmdForPactServerSign(cmd);
        
        Console.WriteLine("FORMATTED");
        Console.WriteLine(cmdToSign);
        string result = await executeHttpRequest("http://127.0.0.1:9467/v1/sign", JsonConvert.DeserializeObject<JObject>(cmdToSign));
        var signedCmd = JsonConvert.DeserializeObject<JObject>(result);
        result = JsonConvert.SerializeObject(signedCmd["body"]);

        return result;
    }

    /// <summary>
    /// This function signs a command or group of commands using the quicksign API
    /// </summary>
    /// <param name="cmds">Array or stringified commands in json format</param>
    /// <returns>Signed command in JSON string format</returns>
    public static async Task<string> quicksign(string[] cmds) {
        var cmdToQuicksign = formatCmdForPactServerQuicksign(cmds);

        string result = await executeHttpRequest("http://127.0.0.1:9467/v1/quicksign", JsonConvert.DeserializeObject<JObject>(cmdToQuicksign));
        Console.WriteLine(result);

        return result;
    }

    /// <summary>
    /// This method sends a signed command using the Pact API endpoint
    /// </summary>
    /// <param name="signedCmd">json serializable object containing signed command to be executed over the Kadena blockchain</param>
    /// <param name="netId">string representing network being used (mainnet01 or testnet04)</param>
    /// <param name="chainId">string representing the ID of the chain the command will be executed on</param>
    /// <returns>The command in a format ready for execution on the Kadena blockchain and containing the signatures</returns>
    public static async Task<string> sendSigned(string[] signedCmds, string netId, string chainId)
    {
        var cmdToSend = formatCmdForPactServerSend(signedCmds);
        string networkUrl = getNetworkUrlWithChainId(netId, chainId);
        Console.WriteLine("================");
        Console.WriteLine("URL");
        Console.WriteLine(networkUrl + "/api/v1/send");
        Console.WriteLine(cmdToSend);
        Console.WriteLine("================");
        string result = await executeHttpRequest(networkUrl + "/api/v1/send", JsonConvert.DeserializeObject<JObject>(cmdToSend));

        return result;
    }

    /// <summary>
    /// This function executes HTTP requests to any URL
    /// </summary>
    /// <param name="url">String representing the URL of the API</param>
    /// <param name="data">Object containing body of HTTP request</param>
    /// <returns>Stringified response of API endpoint</returns>
    /// <exception cref="Exception"></exception>
    public static async Task<string> executeHttpRequest(string url, object data) {
        HttpClient httpClient = new HttpClient();
        StringContent queryString = new StringContent(
            JsonConvert.SerializeObject(data),
            Encoding.UTF8,
            "application/json"
        );
        HttpResponseMessage response = await httpClient.PostAsync(new Uri(url), queryString);
        var responseMessage = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) {
            throw new Exception(responseMessage);
        }

        return responseMessage;
    }

    /// <summary>
    /// This function builds an Exec command as defined by Kadena standards
    /// </summary>
    /// <param name="pactCode">
    ///     String representing pact contract and function to be called on the blockchain
    /// </param>
    /// <param name="chainId">String representing ID of chain the destination contract is on</param>
    /// <param name="sender">String representing wallet address of acconut sending the transaction</param>
    /// <param name="envData">Object containing data needed by the called function on the blockchain</param>
    /// <param name="signers">Array of objects containing data for each signer on the transaction</param>
    /// <param name="gasPrice">Double representing price of gas to execute transaction</param>
    /// <param name="gasLimit">Integer representing limit of gas spend for the transaction</param>
    /// <param name="ttl">Integer detailing time before transaction times out</param>
    /// <param name="networkId">String representing network used, either mainnet01 or testnet04</param>
    /// <param name="nonce">String representing nonce</param>
    /// <returns>JSON string containing all exec command details</returns>
    public static string buildExecCmd(
        string pactCode,
        string chainId,
        string sender,
        object envData,
        object[] signers,
        double gasPrice,
        int gasLimit,
        int ttl,
        string networkId,
        string nonce
    )
    {
        var commandObj = new
        {
            meta = new
            {
                chainId,
                creationTime = DateTime.Now.ToString("1:00:00 00"), //TODO: unhardcode
                gasLimit,
                gasPrice,
                sender,
                ttl
            },
            networkId,
            nonce,
            payload = new
            {
                exec = new
                {
                    code = pactCode,
                    data = envData
                }
            },
            signers
        };

        string serializedCommand = JsonConvert.SerializeObject(commandObj);

        return serializedCommand;
    }

    /// <summary>
    /// This function builds a Cont command as defined by Kadena standards
    /// </summary>
    /// <param name="pactTxHash">String representing pact ID of transaction to continue</param>
    /// <param name="sender">String representing wallet address of acconut sending the transaction</param>
    /// <param name="signers">Array of objects containing data for each signer on the transaction</param>
    /// <param name="chainId">String representing ID of chain the destination contract is on</param>
    /// <param name="networkId">String representing network used, either mainnet01 or testnet04</param>
    /// <param name="nonce">String representing nonce</param>
    /// <param name="proof">String representing proof for cross chain transactions</param>
    /// <param name="ttl">Integer detailing time before transaction times out</param>
    /// <param name="step">Integer representing step of the defpact</param>
    /// <param name="rollback">Boolean indcating whether the transaction can be rolled back</param>
    /// <param name="envData">Object containing data needed by the called function on the blockchain</param>
    /// <param name="clist">Array of capabilities needed by the function called</param>
    /// <param name="gasPrice">Double representing price of gas to execute transaction</param>
    /// <param name="gasLimit">Integer representing limit of gas spend for the transaction</param>
    /// <returns>JSON string containing all cont command details</returns>
    public static string buildContCmd(
        string pactTxHash,
        string sender,
        string[] signers,
        string chainId,
        string networkId,
        string nonce,
        string proof,
        int ttl,
        int step,
        bool rollback,
        object envData,
        object[] clist,
        double gasPrice,
        int gasLimit
    )
    {
        object meta = buildMetadata(sender, chainId, gasPrice, gasLimit, ttl);
        object signerList = buildSigners(clist, signers);
        object contCmd = new
        {
            networkId,
            payload = new
            {
                cont = new
                {
                    proof,
                    pactId = pactTxHash,
                    rollback,
                    step,
                    data = envData,
                }
            },
            signers = signerList,
            meta,
            nonce
        };

        string cmdStr = JsonConvert.SerializeObject(contCmd);
        object[] sigs = Array.Empty<object>();
        string cmdHash = hashCommand(cmdStr);

        object finalCmd = new
        {
            hash = cmdHash,
            sigs,
            cmd = cmdStr
        };

        return JsonConvert.SerializeObject(finalCmd);
    }

    /// <summary>
    /// This function gets the url for the kadena API endpoints
    /// </summary>
    /// <param name="netId">String representing the network used either mainnet01 or testnet04</param>
    /// <param name="chainId">String representing chain to call the endpoint on</param>
    /// <returns>String containing URL of the API endpoint</returns>
    public static string getNetworkUrlWithChainId(string netId, string chainId)
    {
        string networkUrl = "https://api.chainweb.com/chainweb/0.0/" + netId + "/chain/" + chainId + "/pact";

        return networkUrl;
    }

    /// <summary>
    /// This function gets the public key of an account, given an account key
    /// </summary>
    /// <param name="accountKey">String representing wallet address</param>
    /// <returns>String containing the public key derived from the wallet address</returns>
    public static string getPublicKeyFromAccount(string accountKey)
    {
        return accountKey.Split(":")[1];
    }

    /// <summary>
    /// This function builds the command metadata for the command to be executed successfully
    /// </summary>
    /// <param name="sender">String representing wallet address of acconut sending the transaction</param>
    /// <param name="chainId">String representing chain to call the endpoint on</param>
    /// <param name="gasPrice">Double representing price of gas to execute transaction</param>
    /// <param name="gasLimit">Integer representing limit of gas spend for the transaction</param>
    /// <param name="ttl">Integer detailing time before transaction times out</param>
    /// <returns>Object containing the command metadata</returns>
    public static object buildMetadata(
        string sender,
        string chainId,
        double gasPrice,
        int gasLimit,
        int ttl
    )
    {
        string currentTime = DateTime.Now.ToString("1:00:00 00");

        var meta = new
        {
            chainId,
            creationTime = currentTime,
            gasPrice,
            gasLimit,
            sender,
            ttl
        };

        return meta;
    }

    /// <summary>
    /// This function builds signers for a command given the capabilities and wallet addresses
    /// </summary>
    /// <param name="clist">Array of capabilities needed by the function called</param>
    /// <param name="signers">Array of objects containing data for each signer on the transaction</param>
    /// <returns>Object containing list of sigs</returns>
    public static object buildSigners(object[] clist, string[] signers)
    {
        ArrayList signerList = new ArrayList();
        foreach (string signer in signers) {
            string publicKey = getPublicKeyFromAccount(signer);
            object sig = new
            {
                clist,
                pubKey = publicKey
            };
            signerList.Add(sig);
        }

        return signerList;
    }

    /// <summary>
    /// This function builds a capability object as expected by the Kadena blockchain
    /// </summary>
    /// <param name="name">String representing the name of the capability</param>
    /// <param name="args">Array of objects representing args as expected by the function called</param>
    /// <returns>Object containing data of the build capabilities/returns>
    public static object buildCap(string name, object[] args)
    {
        var cap = new
        {
            name,
            args
        };

        return cap;
    }

    /// <summary>
    /// This function performs read queries to fetch data from the blockchain
    /// </summary>
    /// <param name="cmd">JSON string of command to be sent over the blockchain</param>
    /// <param name="sigs">Array of strings representing sigs needed for transaction</param>
    /// <param name="networkUrl"></param>
    /// <returns>JSON string containing response data from the blockchain</returns>
    public static async Task<string> pactFetchLocal(string cmd, string[] sigs, string networkUrl)
    {
        string hash = hashCommand(cmd);

        var payload = new
        {
            hash,
            sigs,
            cmd
        };

        string result = await executeHttpRequest(networkUrl, payload);

        return result;
    }
}