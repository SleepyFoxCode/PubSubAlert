using System;
using System.IO;


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
