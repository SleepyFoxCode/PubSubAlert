using System;
using System.IO;


namespace PubSubAlert
{
    public class Log{

        private static string Diretory = "log\\";
        private static string Filename = "log.txt";

        public static void WriteToLog(string msg){
            try{
                if(!Directory.Exists(Diretory)) Directory.CreateDirectory(Diretory);
                if(!File.Exists(Diretory + Filename)) File.Create(Diretory + Filename).Close();
                using(FileStream fs = new FileStream(Diretory + Filename, FileMode.Append)){
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
