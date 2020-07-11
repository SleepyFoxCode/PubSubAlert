using System;
using System;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text.Json.Serialization;

namespace RazorPagesTwitchPubSub{
    public class Log{

        static string diretory = "log\\";
        static string filename = "log.txt";
        static FileStream fs = null;

        public static void WriteToLog(string str){
            try{
                fs = new FileStream(diretory + filename, FileMode.Append);
                using(StreamWriter writer = new StreamWriter(fs)){
                    writer.Write(str);
                    fs.Flush();
                }
                fs.Close();
            }
            catch(Exception e){
                if(fs != null){
                    fs.Close();
                }
                return;
            }
        }

    }
}
