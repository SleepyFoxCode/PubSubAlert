using System;
using System.IO;


namespace RazorPagesTwitchPubSub{
    public class Log{

        static string diretory = "log\\";
        static string filename = "log.txt";

        public static void WriteToLog(string msg){
            try{
                if(!Directory.Exists(diretory)) Directory.CreateDirectory(diretory);
                if(!File.Exists(diretory + filename)) File.Create(diretory + filename).Close();
                using(FileStream fs = new FileStream(diretory + filename, FileMode.Append)){
                    using(StreamWriter writer = new StreamWriter(fs)){
                        writer.WriteLine(DateTime.Now.ToString("dd-MM-yyyy_hh:mm:ss.fff") + " " + msg);
                        fs.Flush();
                    }
                }
            }
            catch(Exception e){
                System.Console.Write(e.ToString());
            }
        }
    }
}
