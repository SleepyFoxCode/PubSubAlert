using System;
using System.IO;


namespace RazorPagesTwitchPubSub{
    public class Log{

        static string diretory = "log\\";
        static string filename = "log.txt";


        public static void WriteToLog(string str){
            try{
                using(FileStream fs = new FileStream(diretory + filename, FileMode.Append)){
                    using(StreamWriter writer = new StreamWriter(fs)){
                        writer.Write(str);
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
