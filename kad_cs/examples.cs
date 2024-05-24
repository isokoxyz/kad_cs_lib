// Online C# Editor for free
// Write, Edit and Run your C# code using C# Online Compiler

using System.Collections;
using System.Data.HashFunction.Blake2;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using static KadLib;
using System.Runtime.InteropServices.JavaScript;

public class Examples
{
    public static string adminAccount = "k:f157854c15e9bb8fb55aafdecc1ce27a3d60973cbe6870045f4415dc06be06f5";

    public static void Main(string[] args)
    {
        string key = "";
        string value = "";
        string exec = execExample("8", "mainnet01", key, value);

        // string value2 = "";
        // string cont = contExample("contcs", value2, "mainnet01");
    }
    public static string execExample(string chain, string network, string key, string value) {
        string assembledCommand = buildExecCmd(
            pactCode: "(free.test-libs.write-data \"" + key + "\" \"" + value + "\")",
            chainId: chain,
            sender: adminAccount,
            envData: new {},
            signers: new string[] { },
            gasPrice: 1e-8,
            gasLimit: 150000,
            ttl: 600,
            networkId: network,
            nonce: "123"
        );
        Console.WriteLine("COMMAND");
        Console.WriteLine(assembledCommand);

        var signRes = sign(assembledCommand, "sign");
        signRes.Wait();
        string signedCmd = signRes.Result;
        Console.WriteLine("SIGNED");
        Console.WriteLine(signedCmd);

        string[] signedCmds = new string[1];
        signedCmds[0] = signedCmd;
        var sendRes = sendSigned(signedCmds, network, chain);
        sendRes.Wait();
        string sentCmd = sendRes.Result;
        Console.WriteLine("SENT");
        Console.WriteLine(sentCmd);

        return sentCmd;
    }

    public static string contExample(string key, string value, string network) {
        string sender = "k:f157854c15e9bb8fb55aafdecc1ce27a3d60973cbe6870045f4415dc06be06f5";
        string assembledCommand = buildExecCmd(
            pactCode: "(free.test-libs.multi-step-test \"" + key + "\" \"" + value + "\" 2)",
            chainId: "8",
            sender,
            envData: new {},
            signers: new object[] {},
            gasPrice: 1e-8,
            gasLimit: 150000,
            ttl: 600,
            networkId: network,
            nonce: "123"
        );

        var signRes = sign(assembledCommand, "sign");
        signRes.Wait();
        string signedCmd = signRes.Result;
        Console.WriteLine("EXEC SIGNED");
        Console.WriteLine(signedCmd);

        string[] signedCmds = new string[1];
        signedCmds[0] = signedCmd;
        var sendRes = sendSigned(signedCmds, network, "8");
        sendRes.Wait();
        string sentCmd = sendRes.Result;
        var sentCmdResult = JsonConvert.DeserializeObject<JObject>(sentCmd);
        Console.WriteLine("EXEC SENT");
        Console.WriteLine(sentCmdResult);

        if (sendRes.IsCompletedSuccessfully && !Equals(sentCmdResult, null)) {
            Console.WriteLine("SEND SUCCESSFUL");
            Console.WriteLine(sentCmdResult);
            if (!Equals(sentCmdResult["requestKeys"], null)) {
                string pactId = sentCmdResult["requestKeys"][0].ToString();
                string contCmd = buildContCmd(
                    pactTxHash: pactId,
                    sender,
                    chainId: "8",
                    networkId: network,
                    nonce: "123",
                    proof: string.Empty,
                    ttl: 600,
                    step: 1,
                    rollback: false,
                    envData: new {},
                    clist: new string[] {},
                    signers: new string[] {sender},
                    gasPrice: 1e-8,
                    gasLimit: 150000
                );
                var cmdObject = JsonConvert.DeserializeObject<JObject>(contCmd);

                if (!Equals(cmdObject, null)) {
                    // string[] cmds = new string[1];
                    // cmds[0] = cmdObject["cmd"];
                    string[] cmds = new string[1]
                    {
                        (string)cmdObject["cmd"]
                    };
                    var quicksignRes = quicksign(cmds);
                    quicksignRes.Wait();
                    var result = quicksignRes.Result;
                    Console.WriteLine("QUICKSIGNED");
                    Console.WriteLine(result);

                    string[] cmdsToSend = new string[1];
                    cmdsToSend[0] = result;
                    var res = sendSigned(signedCmds, network, "8");
                    res.Wait();
                    string sent = res.Result;
                    Console.WriteLine("SENT");
                    Console.WriteLine(sent);
                }
            }
        }
        
        return "";
    }
}