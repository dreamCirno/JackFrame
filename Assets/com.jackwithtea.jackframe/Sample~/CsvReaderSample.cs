using System.Collections;
using System.Collections.Generic;
using JackFrame;
using System.IO;

namespace JackFrame.Sample {

    public class DemoModel {
        public long id;
        public string name;
        public bool allowUse;
        public int[] intArray;
        public string[] stringArray;
    }


    public class CsvReaderSample {

        public void Test() {

            //从文件读取
            //List<DemoModel> demoList = CsvReader.CsvFileToClassList<DemoModel>(@"G:\ItemTable.csv", '&');

            //从数组读取
            string[] csv = new string[] {
            "id,name,allowUse,intArray,stringArray",
            "long,string,bool,int[],string[]",
            "1,物品1,1,0&1&2,demo&one",
            "2,物品2,0,34&567,demo&2"
            };
            List<DemoModel> demoList = CsvReader.CsvToClassList<DemoModel>(csv, '&');

            //输出列表
            for (int i = 0; i < demoList.Count; i++) {
                string logStr = demoList[i].id.ToString() + " " + demoList[i].name + " " + demoList[i].allowUse;
                PLog.Log(logStr);
            }

            //根据csv生成类
            CsvReader.CsvToClassFile(csv, "JackFrame.Demo.CSV_DEMO", "ItemModel", @"G:");
            //读取csv文件，再根据csv生成类
            //CsvReader.ReadCSVFileToClassFile(@"ItemTable.csv", "JackFrame.Demo.CSV_DEMO", "ItemModel", @"G:");
        }

    }
}