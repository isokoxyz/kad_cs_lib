using System;
using System.Collections;
using System.Data.HashFunction;
using System.Data.HashFunction.Blake2;
using System.Text;
using System.Diagnostics;
// using System.Text.Json;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Runtime.InteropServices.JavaScript;

class HashingUtils
{
    public static string hashCommand(string input)
    {
        //Set configuration for blake2b hashing algorithm
        Blake2BConfig blake2BConfig = new Blake2BConfig();
        blake2BConfig.HashSizeInBits = 256;

        //Hash the given input and get array of unsigne 8 bit integers
        IBlake2B hash = Blake2BFactory.Instance.Create(blake2BConfig);
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hashedByteArray = hash.ComputeHash(bytes).Hash;

        //Convert to string
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < hashedByteArray.Length; i++)
        {
            stringBuilder.Append(((char)hashedByteArray[i]).ToString());
        }

        string result = b64BaseUrlEncode(stringBuilder.ToString());

        // result = b64BaseUrlEncode("z_:æGë¾¯ÇÇ YHî8sö¤¾¾ÕQ");
        // Console.WriteLine("Result 1: " + result);

        return result;
    }

    private static string b64BaseUrlEncode(string input)
    {
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_=";
        string str = input;
        int block = 0;
        int charCode = 0;
        string map = chars;
        string output = "";

        string result = string.Empty;

        for (double idx = 0.0; (int)idx < str.Length; output += map[63 & block >> 8 - (int)(idx % 1 * 8)])
        {
            idx += (3.0 / 4.0);
            if ((int)idx == str.Length)
            {
                // Console.WriteLine(idx);
                block = block << 8 | 0;
                output += map[63 & block >> 8 - (int)(idx % 1 * 8)];
                break;
            }
            charCode = (int)str[(int)idx];
            block = block << 8 | charCode;
            // Console.WriteLine("-----------");
            // Console.WriteLine(63 & block >> 8 - (int)(idx % 1 * 8));
            // Console.WriteLine("\n");
        }

        return output;
    }
}

class CommandUtilities
{
    public static string getPublicKeyFromAccount(string accountKey)
    {
        return accountKey.Split(":")[1];
    }

    public static string formatCmdForPactServerSign(string cmd) {
        var command = JsonConvert.DeserializeObject<JObject>(cmd);

        var formattedCmd = new {
            code = command["payload"]["exec"]["code"],
            caps = new object[] {},
            sender = command["meta"]["sender"],
            gasLimit = command["meta"]["gasLimit"],
            gasPrice = command["meta"]["gasPrice"],
            chainId = command["meta"]["chainId"],
            ttl = command["meta"]["ttl"],
            envData = command["payload"]["exec"]["data"],
            signingPubKey = command["meta"]["sender"].ToString().Split(":")[1],
            networkId = command["networkId"],
        };

        return JsonConvert.SerializeObject(formattedCmd);
    }

    public static string formatCmdForPactServerQuicksign(string[] cmds) {
        ArrayList commandSigDatas = new ArrayList();
        foreach(string cmd in cmds) {
            var command = JsonConvert.DeserializeObject<JObject>(cmd);
            var sigs = getSigsFromCmd(command);
            var sigData = new {
                sigs,
                cmd
            };
            commandSigDatas.Add(sigData);
        }
        Console.WriteLine("SIGDATAS");
        Console.WriteLine(JsonConvert.SerializeObject(commandSigDatas));

        var cmdSigDatasArray = commandSigDatas.ToArray();
        var cmdToQuicksign = new {
            cmdSigDatas = cmdSigDatasArray
        };

        return JsonConvert.SerializeObject(cmdToQuicksign);
    }

    public static string formatCmdForPactServerSend(string[] cmds) {
        ArrayList cmdList = new ArrayList();
        foreach (var cmd in cmds) {
            cmdList.Add(JsonConvert.DeserializeObject<JObject>(cmd));
        }
            
        var command = new {
            cmds = cmdList.ToArray(typeof(JObject))
        };

        return JsonConvert.SerializeObject(command);
    }

    public static object getSigsFromCmd(JObject cmd) {
        var signers = cmd["signers"];
        ArrayList sigs = new ArrayList();
        foreach(var signer in signers) {
            Nullable<int> sigString = null;
            var sig = new {
                pubKey = signer["pubKey"],
                sig = sigString
            };

            sigs.Add(sig);
        }
        Console.WriteLine(JsonConvert.SerializeObject(sigs));

        return sigs;
    }
}

class CmdLineUtil
{
    public static void invokeCmdLine(string command)
    {

        Process cmd = new Process();
        cmd.StartInfo.FileName = command;
        cmd.StartInfo.RedirectStandardInput = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.UseShellExecute = false;
        cmd.Start();

        cmd.StandardInput.WriteLine("echo test");
        cmd.StandardInput.Flush();
        cmd.StandardInput.Close();
        cmd.WaitForExit();
        Console.WriteLine(cmd.StandardOutput.ReadToEnd());
    }
}