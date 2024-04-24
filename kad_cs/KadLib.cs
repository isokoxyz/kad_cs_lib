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

    public static async Task<string> quicksign(string[] cmds) {
        var cmdToQuicksign = formatCmdForPactServerQuicksign(cmds);

        string result = await executeHttpRequest("http://127.0.0.1:9467/v1/quicksign", JsonConvert.DeserializeObject<JObject>(cmdToQuicksign));
        Console.WriteLine(result);

        return "";
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

    public static string getNetworkUrlWithChainId(string netId, string chainId)
    {
        string networkUrl = "https://api.chainweb.com/chainweb/0.0/" + netId + "/chain/" + chainId + "/pact";

        return networkUrl;
    }

    public static string getPublicKeyFromAccount(string accountKey)
    {
        return accountKey.Split(":")[1];
    }

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

    public static object buildCap(string name, object[] args)
    {
        var cap = new
        {
            name,
            args
        };

        return cap;
    }

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